﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.AddImport;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.EditorConfigSettings;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.EditorConfigSettings

{
    internal partial class CSharpEditorConfigSettingsValueHolder
    {
        private static readonly BidirectionalMap<string, AddImportPlacement> AddImportPlacementMap =
            new(new[]
            {
                KeyValuePairUtil.Create("inside_namespace", AddImportPlacement.InsideNamespace),
                KeyValuePairUtil.Create("outside_namespace", AddImportPlacement.OutsideNamespace),
            });

        private static readonly BidirectionalMap<string, PreferBracesPreference> PreferBracesPreferenceMap =
            new(new[]
            {
                KeyValuePairUtil.Create("false", PreferBracesPreference.None),
                KeyValuePairUtil.Create("when_multiline", PreferBracesPreference.WhenMultiline),
                KeyValuePairUtil.Create("true", PreferBracesPreference.Always),
            });

        private static readonly BidirectionalMap<string, NamespaceDeclarationPreference> NNamespaceDeclarationPreferencesMap =
            new(new[]
            {
                KeyValuePairUtil.Create("block_scoped", NamespaceDeclarationPreference.BlockScoped),
                KeyValuePairUtil.Create("file_scoped", NamespaceDeclarationPreference.FileScoped),
            });

        private static readonly BidirectionalMap<string, ExpressionBodyPreference> ExpressionBodyPreferenceMap =
            new(new[]
            {
                KeyValuePairUtil.Create("false", ExpressionBodyPreference.Never),
                KeyValuePairUtil.Create("true", ExpressionBodyPreference.WhenPossible),
                KeyValuePairUtil.Create("when_on_single_line", ExpressionBodyPreference.WhenOnSingleLine),
            });

        private static readonly BidirectionalMap<string, UnusedValuePreference> UnusedValuePreferenceMap =
            new(new[]
            {
                KeyValuePairUtil.Create("discard_variable", UnusedValuePreference.DiscardVariable),
                KeyValuePairUtil.Create("unused_local_variable", UnusedValuePreference.UnusedLocalVariable),
            });

        // Var Options
        public static EditorConfigData<bool> VarForBuiltInTypes = new BooleanEditorConfigData("csharp_style_var_for_built_in_types", CSharpWorkspaceResources.For_built_in_types);
        public static EditorConfigData<bool> VarWhenTypeIsApparent = new BooleanEditorConfigData("csharp_style_var_when_type_is_apparent", CSharpWorkspaceResources.When_variable_type_is_apparent);
        public static EditorConfigData<bool> VarElsewhere = new BooleanEditorConfigData("csharp_style_var_elsewhere", CSharpWorkspaceResources.Elsewhere);

        // Usings Options
        public static EditorConfigData<AddImportPlacement> PreferredUsingDirectivePlacement = new EnumEditorConfigData<AddImportPlacement>("csharp_using_directive_placement", CSharpWorkspaceResources.Preferred_using_directive_placement, AddImportPlacementMap);

        // Null Checking Options
        public static EditorConfigData<bool> PreferThrowExpression = new BooleanEditorConfigData("csharp_style_throw_expression", CSharpWorkspaceResources.Prefer_throw_expression);
        public static EditorConfigData<bool> PreferConditionalDelegateCall = new BooleanEditorConfigData("csharp_style_conditional_delegate_call", CSharpWorkspaceResources.Prefer_conditional_delegate_call);
        public static EditorConfigData<bool> PreferNullCheckOverTypeCheck = new BooleanEditorConfigData("csharp_style_prefer_null_check_over_type_check", CSharpWorkspaceResources.Prefer_null_check_over_type_check);

        // Modifier Options
        public static EditorConfigData<bool> PreferStaticLocalFunction = new BooleanEditorConfigData("csharp_prefer_static_local_function", CSharpWorkspaceResources.Prefer_static_local_functions);

        // Code Block Options
        public static EditorConfigData<bool> PreferSimpleUsingStatement = new BooleanEditorConfigData("csharp_prefer_simple_using_statement", CSharpWorkspaceResources.Prefer_simple_using_statement);
        public static EditorConfigData<PreferBracesPreference> PreferBraces = new EnumEditorConfigData<PreferBracesPreference>("csharp_prefer_braces", CSharpWorkspaceResources.Prefer_braces, PreferBracesPreferenceMap);
        public static EditorConfigData<NamespaceDeclarationPreference> NamespaceDeclarations = new EnumEditorConfigData<NamespaceDeclarationPreference>("csharp_style_namespace_declarations", CSharpWorkspaceResources.Namespace_declarations, NNamespaceDeclarationPreferencesMap);
        public static EditorConfigData<bool> PreferMethodGroupConversion = new BooleanEditorConfigData("csharp_style_prefer_method_group_conversion", CSharpWorkspaceResources.Prefer_method_group_conversion);
        public static EditorConfigData<bool> PreferTopLevelStatements = new BooleanEditorConfigData("csharp_style_prefer_top_level_statements", CSharpWorkspaceResources.Prefer_top_level_statements);

        // Expression Options
        public static EditorConfigData<bool> PreferSwitchExpression = new BooleanEditorConfigData("csharp_style_prefer_switch_expression", CSharpWorkspaceResources.Prefer_switch_expression);
        public static EditorConfigData<bool> PreferSimpleDefaultExpression = new BooleanEditorConfigData("csharp_prefer_simple_default_expression", CSharpWorkspaceResources.Prefer_simple_default_expression);
        public static EditorConfigData<bool> PreferLocalOverAnonymousFunction = new BooleanEditorConfigData("csharp_style_prefer_local_over_anonymous_function", CSharpWorkspaceResources.Prefer_local_function_over_anonymous_function);
        public static EditorConfigData<bool> PreferIndexOperator = new BooleanEditorConfigData("csharp_style_prefer_index_operator", CSharpWorkspaceResources.Prefer_index_operator);
        public static EditorConfigData<bool> PreferRangeOperator = new BooleanEditorConfigData("csharp_style_prefer_range_operator", CSharpWorkspaceResources.Prefer_range_operator);
        public static EditorConfigData<bool> ImplicitObjectCreationWhenTypeIsApparent = new BooleanEditorConfigData("csharp_style_implicit_object_creation_when_type_is_apparent", CSharpWorkspaceResources.Prefer_implicit_object_creation_when_type_is_apparent);
        public static EditorConfigData<bool> PreferTupleSwap = new BooleanEditorConfigData("csharp_style_prefer_tuple_swap", CSharpWorkspaceResources.Prefer_tuple_swap);
        public static EditorConfigData<bool> PreferUtf8StringLiterals = new BooleanEditorConfigData("csharp_style_prefer_utf8_string_literals", CSharpWorkspaceResources.Prefer_Utf8_string_literals);

        // Pattern Matching Options
        public static EditorConfigData<bool> PreferPatternMatching = new BooleanEditorConfigData("csharp_style_prefer_pattern_matching", CSharpWorkspaceResources.Prefer_pattern_matching);
        public static EditorConfigData<bool> PreferPatternMatchingOverIsWithCastCheck = new BooleanEditorConfigData("csharp_style_pattern_matching_over_is_with_cast_check", CSharpWorkspaceResources.Prefer_pattern_matching_over_is_with_cast_check);
        public static EditorConfigData<bool> PreferPatternMatchingOverAsWithNullCheck = new BooleanEditorConfigData("csharp_style_pattern_matching_over_as_with_null_check", CSharpWorkspaceResources.Prefer_pattern_matching_over_as_with_null_check);
        public static EditorConfigData<bool> PreferNotPattern = new BooleanEditorConfigData("csharp_style_prefer_not_pattern", CSharpWorkspaceResources.Prefer_pattern_matching_over_mixed_type_check);
        public static EditorConfigData<bool> PreferExtendedPropertyPattern = new BooleanEditorConfigData("csharp_style_prefer_extended_property_pattern", CSharpWorkspaceResources.Prefer_extended_property_pattern);

        // Variable Options
        public static EditorConfigData<bool> PreferInlinedVariableDeclaration = new BooleanEditorConfigData("csharp_style_inlined_variable_declaration", CSharpWorkspaceResources.Prefer_inlined_variable_declaration);
        public static EditorConfigData<bool> PreferDeconstructedVariableDeclaration = new BooleanEditorConfigData("csharp_style_deconstructed_variable_declaration", CSharpWorkspaceResources.Prefer_deconstructed_variable_declaration);

        // Expression Body Options
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedMethods = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_methods", CSharpWorkspaceResources.Use_expression_body_for_methods, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedConstructors = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_constructors", CSharpWorkspaceResources.Use_expression_body_for_constructors, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedOperators = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_operators", CSharpWorkspaceResources.Use_expression_body_for_operators, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedProperties = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_properties", CSharpWorkspaceResources.Use_expression_body_for_properties, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedIndexers = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_indexers", CSharpWorkspaceResources.Use_expression_body_for_indexers, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedAccessors = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_accessors", CSharpWorkspaceResources.Use_expression_body_for_accessors, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedLambdas = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_lambdas", CSharpWorkspaceResources.Use_expression_body_for_lambdas, ExpressionBodyPreferenceMap);
        public static EditorConfigData<ExpressionBodyPreference> PreferExpressionBodiedLocalFunctions = new EnumEditorConfigData<ExpressionBodyPreference>("csharp_style_expression_bodied_local_functions", CSharpWorkspaceResources.Use_expression_body_for_local_functions, ExpressionBodyPreferenceMap);

        // Unused Value Options
        public static EditorConfigData<UnusedValuePreference> UnusedValueAssignment = new EnumEditorConfigData<UnusedValuePreference>("csharp_style_unused_value_assignment_preference", CSharpWorkspaceResources.Avoid_unused_value_assignments, UnusedValuePreferenceMap);
        public static EditorConfigData<UnusedValuePreference> UnusedValueExpressionStatement = new EnumEditorConfigData<UnusedValuePreference>("csharp_style_unused_value_expression_statement_preference", CSharpWorkspaceResources.Avoid_expression_statements_that_implicitly_ignore_value, UnusedValuePreferenceMap);
        public static EditorConfigData<bool> AllowEmbeddedStatementsOnSameLine = new BooleanEditorConfigData("csharp_style_allow_embedded_statements_on_same_line_experimental", CSharpWorkspaceResources.Allow_embedded_statements_on_same_line);
        public static EditorConfigData<bool> AllowBlankLinesBetweenConsecutiveBraces = new BooleanEditorConfigData("csharp_style_allow_blank_line_after_colon_in_constructor_initializer_experimental", CSharpWorkspaceResources.Allow_blank_lines_between_consecutive_braces);
        public static EditorConfigData<bool> AllowBlankLineAfterColonInConstructorInitializer = new BooleanEditorConfigData("csharp_style_allow_blank_line_after_colon_in_constructor_initializer_experimental", CSharpWorkspaceResources.Allow_bank_line_after_colon_in_constructor_initializer);
    }
}
