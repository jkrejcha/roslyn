﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Telemetry;

/// <summary>
/// Provides access to posting telemetry events or adding information
/// to aggregated telemetry events. Posts pending telemetry at 30
/// minute intervals.
/// </summary>
internal static class TelemetryLogging
{
    private static ITelemetryLogProvider? s_logProvider;
    private static AsyncBatchingWorkQueue? s_postTelemetryQueue;

    public const string KeyName = "Name";
    public const string KeyValue = "Value";
    public const string KeyLanguageName = "LanguageName";
    public const string KeyMetricName = "MetricName";

    public static event EventHandler<EventArgs>? Flushed;

    public static void SetLogProvider(ITelemetryLogProvider logProvider, IAsynchronousOperationListener asyncListener)
    {
        s_logProvider = logProvider;

        InterlockedOperations.Initialize(ref s_postTelemetryQueue, () =>
            new AsyncBatchingWorkQueue(
                TimeSpan.FromMinutes(30),
                PostCollectedTelemetryAsync,
                asyncListener,
                CancellationToken.None));

        // Add the initial item to the queue to ensure later processing.
        s_postTelemetryQueue?.AddWork();
    }

    /// <summary>
    /// Posts a telemetry event representing the <paramref name="functionId"/> operation with context message <paramref name="logMessage"/>
    /// </summary>
    public static void Log(FunctionId functionId, KeyValueLogMessage logMessage)
    {
        GetLog(functionId)?.Log(logMessage);

        logMessage.Free();
    }

    /// <summary>
    /// Posts a telemetry event representing the <paramref name="functionId"/> operation 
    /// only if the block duration meets or exceeds <paramref name="minThresholdMs"/> milliseconds.
    /// This event will contain properties from <paramref name="logMessage"/> and the actual execution time.
    /// </summary>
    /// <param name="logMessage">Properties to be set on the telemetry event</param>
    /// <param name="minThresholdMs">Optional parameter used to determine whether to send the telemetry event</param>
    public static IDisposable? LogBlockTime(FunctionId functionId, KeyValueLogMessage logMessage, int minThresholdMs = -1)
    {
        return GetLog(functionId)?.LogBlockTime(logMessage, minThresholdMs);
    }

    /// <summary>
    /// Adds information to an aggregated telemetry event representing the <paramref name="functionId"/> operation 
    /// with the specified name and value.
    /// </summary>
    public static void LogAggregated(FunctionId functionId, TelemetryLoggingInterpolatedStringHandler name, int value)
    {
        if (GetAggregatingLog(functionId) is not { } aggregatingLog)
            return;

        var logMessage = KeyValueLogMessage.Create(m =>
        {
            m[KeyName] = name.GetFormattedText();
            m[KeyValue] = value;
        });

        aggregatingLog.Log(logMessage);
        logMessage.Free();
    }

    public static void LogAggregated(FunctionId functionId, KeyValueLogMessage logMessage)
    {
        if (GetAggregatingLog(functionId) is not { } aggregatingLog)
            return;

        aggregatingLog.Log(logMessage);
        logMessage.Free();
    }

    /// <summary>
    /// Adds block execution time to an aggregated telemetry event representing the <paramref name="functionId"/> operation 
    /// with metric <paramref name="metricName"/> only if the block duration meets or exceeds <paramref name="minThresholdMs"/> milliseconds.
    /// </summary>
    /// <param name="minThresholdMs">Optional parameter used to determine whether to send the telemetry event</param>
    public static IDisposable? LogBlockTimeAggregated(FunctionId functionId, TelemetryLoggingInterpolatedStringHandler metricName, int minThresholdMs = -1)
    {
        if (GetAggregatingLog(functionId) is not { } aggregatingLog)
            return null;

        var logMessage = KeyValueLogMessage.Create(m =>
        {
            m[KeyName] = metricName.GetFormattedText();
        });

        return aggregatingLog.LogBlockTime(logMessage, minThresholdMs);
    }

    /// <summary>
    /// Returns non-aggregating telemetry log.
    /// </summary>
    public static ITelemetryLog? GetLog(FunctionId functionId)
    {
        return s_logProvider?.GetLog(functionId);
    }

    /// <summary>
    /// Returns aggregating telemetry log.
    /// </summary>
    public static ITelemetryLog? GetAggregatingLog(FunctionId functionId, double[]? bucketBoundaries = null)
    {
        return s_logProvider?.GetAggregatingLog(functionId, bucketBoundaries);
    }

    public static void Flush()
    {
        s_logProvider?.Flush();

        Flushed?.Invoke(null, EventArgs.Empty);
    }

    private static ValueTask PostCollectedTelemetryAsync(CancellationToken cancellationToken)
    {
        Flush();

        // Ensure PostCollectedTelemetryAsync will get fired again after the collection period.
        s_postTelemetryQueue?.AddWork();

        return ValueTaskFactory.CompletedTask;
    }
}
