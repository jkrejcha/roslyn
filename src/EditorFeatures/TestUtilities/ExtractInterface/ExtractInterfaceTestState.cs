﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeCleanup;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.ExtractInterface;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.ExtractInterface
{
    internal class ExtractInterfaceTestState : IDisposable
    {
        public static readonly TestComposition Composition = EditorTestCompositions.EditorFeatures.AddParts(
            typeof(TestExtractInterfaceOptionsService));

        private readonly TestHostDocument _testDocument;
        public TestWorkspace Workspace { get; }
        public Document ExtractFromDocument { get; }
        public AbstractExtractInterfaceService ExtractInterfaceService { get; }
        public Solution OriginalSolution { get; }
        public string ErrorMessage { get; private set; }
        public NotificationSeverity ErrorSeverity { get; private set; }

        public static ExtractInterfaceTestState Create(
            string markup,
            string languageName,
            CompilationOptions compilationOptions = null,
            ParseOptions parseOptions = null,
            OptionsCollection options = null)
        {
            var workspace = languageName == LanguageNames.CSharp
                ? TestWorkspace.CreateCSharp(markup, composition: Composition, compilationOptions: compilationOptions, parseOptions: parseOptions)
                : TestWorkspace.CreateVisualBasic(markup, composition: Composition, compilationOptions: compilationOptions, parseOptions: parseOptions);

            options?.SetGlobalOptions(workspace.GlobalOptions);

            return new ExtractInterfaceTestState(workspace);
        }

        public ExtractInterfaceTestState(TestWorkspace workspace)
        {
            Workspace = workspace;

            OriginalSolution = Workspace.CurrentSolution;
            _testDocument = Workspace.Documents.SingleOrDefault(d => d.CursorPosition.HasValue);

            if (_testDocument == null)
            {
                throw new ArgumentException("markup does not contain a cursor position", nameof(workspace));
            }

            ExtractFromDocument = Workspace.CurrentSolution.GetDocument(_testDocument.Id);
            ExtractInterfaceService = ExtractFromDocument.GetLanguageService<AbstractExtractInterfaceService>();
        }

        public TestExtractInterfaceOptionsService TestExtractInterfaceOptionsService
        {
            get
            {
                return (TestExtractInterfaceOptionsService)ExtractFromDocument.Project.Solution.Workspace.Services.GetService<IExtractInterfaceOptionsService>();
            }
        }

        public Task<ExtractInterfaceTypeAnalysisResult> GetTypeAnalysisResultAsync(TypeDiscoveryRule typeDiscoveryRule)
        {
            return ExtractInterfaceService.AnalyzeTypeAtPositionAsync(
                ExtractFromDocument,
                _testDocument.CursorPosition.Value,
                typeDiscoveryRule,
                Workspace.GlobalOptions.CreateProvider(),
                CancellationToken.None);
        }

        public Task<ExtractInterfaceResult> ExtractViaCommandAsync()
        {
            return ExtractInterfaceService.ExtractInterfaceAsync(
                ExtractFromDocument,
                _testDocument.CursorPosition.Value,
                Workspace.GlobalOptions.CreateProvider(),
                (errorMessage, severity) =>
                {
                    this.ErrorMessage = errorMessage;
                    this.ErrorSeverity = severity;
                },
                CancellationToken.None);
        }

        public async Task<Solution> ExtractViaCodeAction()
        {
            var actions = await ExtractInterfaceService.GetExtractInterfaceCodeActionAsync(
                ExtractFromDocument,
                new TextSpan(_testDocument.CursorPosition.Value, 1),
                Workspace.GlobalOptions.CreateProvider(),
                CancellationToken.None);

            var action = actions.Single();

            var options = (ExtractInterfaceOptionsResult)action.GetOptions(CancellationToken.None);
            var changedOptions = new ExtractInterfaceOptionsResult(
                options.IsCancelled,
                options.IncludedMembers,
                options.InterfaceName,
                options.FileName,
                ExtractInterfaceOptionsResult.ExtractLocation.SameFile,
                options.FallbackOptions);

            var operations = await action.GetOperationsAsync(changedOptions, CancellationToken.None);
            foreach (var operation in operations)
            {
                operation.Apply(Workspace, CancellationToken.None);
            }

            return Workspace.CurrentSolution;
        }

        public void Dispose()
        {
            Workspace?.Dispose();
        }
    }
}
