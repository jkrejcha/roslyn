﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UIAutomationClient;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities
{
    public static class Helper
    {
        private static IUIAutomation2 _automation;

        public static IUIAutomation2 Automation
        {
            get
            {
                if (_automation == null)
                {
                    Interlocked.CompareExchange(ref _automation, new CUIAutomation8(), null);
                }

                return _automation;
            }
        }

        /// <summary>
        /// This method will retry the action represented by the 'action' argument,
        /// milliseconds, waiting 'delay' milliseconds after each retry. If a given retry returns a value 
        /// other than default(T), this value is returned.
        /// </summary>
        /// <param name="action">the action to retry</param>
        /// <param name="delay">the amount of time to wait between retries in milliseconds</param>
        /// <typeparam name="T">type of return value</typeparam>
        /// <returns>the return value of 'action'</returns>
        public static T Retry<T>(Func<T> action, int delay)
            => Retry(action, TimeSpan.FromMilliseconds(delay));

        /// <summary>
        /// This method will retry the action represented by the 'action' argument,
        /// milliseconds, waiting 'delay' milliseconds after each retry and will swallow all exceptions. 
        /// If a given retry returns a value other than default(T), this value is returned.
        /// </summary>
        /// <param name="action">the action to retry</param>
        /// <param name="delay">the amount of time to wait between retries in milliseconds</param>
        /// <typeparam name="T">type of return value</typeparam>
        /// <returns>the return value of 'action'</returns>
        public static T RetryIgnoringExceptions<T>(Func<T> action, int delay)
            => RetryIgnoringExceptions(action, TimeSpan.FromMilliseconds(delay));

        /// <summary>
        /// This method will retry the action represented by the 'action' argument,
        /// waiting for 'delay' time after each retry. If a given retry returns a value 
        /// other than default(T), this value is returned.
        /// </summary>
        /// <param name="action">the action to retry</param>
        /// <param name="delay">the amount of time to wait between retries</param>
        /// <typeparam name="T">type of return value</typeparam>
        /// <returns>the return value of 'action'</returns>
        public static T Retry<T>(Func<T> action, TimeSpan delay)
        {
            return RetryHelper(() =>
                {
                    try
                    {
                        return action();
                    }
                    catch (COMException)
                    {
                        // Devenv can throw COMExceptions if it's busy when we make DTE calls.
                        return default(T);
                    }
                },
                delay);
        }

        /// <summary>
        /// This method will retry the asynchronous action represented by <paramref name="action"/>,
        /// waiting for <paramref name="delay"/> time after each retry. If a given retry returns a value 
        /// other than the default value of <typeparamref name="T"/>, this value is returned.
        /// </summary>
        /// <param name="action">the asynchronous action to retry</param>
        /// <param name="delay">the amount of time to wait between retries</param>
        /// <typeparam name="T">type of return value</typeparam>
        /// <returns>the return value of <paramref name="action"/></returns>
        public static Task<T> RetryAsync<T>(Func<Task<T>> action, TimeSpan delay)
        {
            return RetryAsyncHelper(async () =>
            {
                try
                {
                    return await action();
                }
                catch (COMException)
                {
                    // Devenv can throw COMExceptions if it's busy when we make DTE calls.
                    return default;
                }
            },
                delay);
        }

        /// <summary>
        /// This method will retry the action represented by the 'action' argument,
        /// milliseconds, waiting 'delay' milliseconds after each retry and will swallow all exceptions. 
        /// If a given retry returns a value other than default(T), this value is returned.
        /// </summary>
        /// <param name="action">the action to retry</param>
        /// <param name="delay">the amount of time to wait between retries in milliseconds</param>
        /// <typeparam name="T">type of return value</typeparam>
        /// <returns>the return value of 'action'</returns>
        public static T RetryIgnoringExceptions<T>(Func<T> action, TimeSpan delay)
        {
            return RetryHelper(() =>
                {
                    try
                    {
                        return action();
                    }
                    catch (Exception)
                    {
                        return default(T);
                    }
                },
                delay);
        }

        private static T RetryHelper<T>(Func<T> action, TimeSpan delay)
        {
            while (true)
            {
                var retval = action();

                if (!Equals(default(T), retval))
                {
                    return retval;
                }

                System.Threading.Thread.Sleep(delay);
            }
        }

        private static async Task<T> RetryAsyncHelper<T>(Func<Task<T>> action, TimeSpan delay)
        {
            while (true)
            {
                var retval = await action().ConfigureAwait(true);
                if (!Equals(default(T), retval))
                {
                    return retval;
                }

                await Task.Delay(delay).ConfigureAwait(true);
            }
        }
    }
}
