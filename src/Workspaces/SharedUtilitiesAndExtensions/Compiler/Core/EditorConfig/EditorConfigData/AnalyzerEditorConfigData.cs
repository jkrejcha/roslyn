﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.EditorConfigSettings
{
    internal class AnalyzerEditorConfigData : EditorConfigData<DiagnosticSeverity>
    {
        private readonly BidirectionalMap<string, DiagnosticSeverity> ValueToSettingName;

        public AnalyzerEditorConfigData(string settingName, BidirectionalMap<string, DiagnosticSeverity> valueToSettingName, bool allowsMultipleValues = false)
            : base(settingName, "", allowsMultipleValues)
        {
            ValueToSettingName = valueToSettingName;
        }

        public override string GetSettingName() => SettingName;

        public override string GetSettingNameDocumentation() => SettingNameDocumentation;

        public override bool GetAllowsMultipleValues() => AllowsMultipleValues;

        public override ImmutableArray<string> GetAllSettingValues()
        {
            return ValueToSettingName.Keys.ToImmutableArray();
        }

        public override string? GetSettingValueDocumentation(string key)
        {
            throw new NotImplementedException();
        }

        public override string GetEditorConfigStringFromValue(DiagnosticSeverity value)
        {
            ValueToSettingName.TryGetKey(value, out var key);
            return key ?? "";
        }

        public override Optional<DiagnosticSeverity> GetValueFromEditorConfigString(string key)
        {
            ValueToSettingName.TryGetValue(key.Trim(), out var value);
            return value;
        }
    }
}
