﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.LanguageServices.DocumentOutline;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.LanguageServices
{
    internal partial class DocumentOutlineControl
    {
        private IWpfTextView? GetLastActiveIWpfTextView()
        {
            ThreadingContext.ThrowIfNotOnUIThread();

            if (ErrorHandler.Failed(CodeWindow.GetLastActiveView(out var textView)))
                return null;

            return EditorAdaptersFactoryService.GetWpfTextView(textView);
        }

        /// <summary>
        /// Starts a new task to get the current document model.
        /// </summary>
        private void StartComputeModelTask()
        {
            // 'true' value is unused.  this just signals to the queue that we have work to do.
            _computeModelQueue.AddWork(true);
        }

        /// <summary>
        /// Fetches and processes the current document model.
        /// </summary>
        private async ValueTask<DocumentSymbolModel?> ComputeModelAsync(ImmutableSegmentedList<bool> unused, CancellationToken cancellationToken)
        {
            // Jump to the UI thread to get the currently active text view.
            await ThreadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var activeTextView = GetLastActiveIWpfTextView();
            if (activeTextView is null)
                return null;

            var lspSnapshot = activeTextView.TextSnapshot;
            var textBuffer = activeTextView.TextBuffer;

            var filePath = GetFilePath();
            if (filePath is null)
                return null;

            // Ensure we switch to the threadpool before calling DocumentSymbolsRequestAsync.  It ensures
            // that fetching and processing the document model is not done on the UI thread.
            await TaskScheduler.Default;

            var model = await ComputeModelAsync().ConfigureAwait(false);

            if (model is not null)
                StartUpdateUITask();

            return model;

            async Task<DocumentSymbolModel?> ComputeModelAsync()
            {
                var response = await DocumentOutlineHelper.DocumentSymbolsRequestAsync(
                    textBuffer, LanguageServiceBroker, filePath, cancellationToken).ConfigureAwait(false);

                if (response is null)
                    return null;

                var responseBody = response.ToObject<DocumentSymbol[]>();
                if (responseBody is null)
                    return null;

                var documentSymbols = DocumentOutlineHelper.GetNestedDocumentSymbols(responseBody);
                var documentSymbolItems = DocumentOutlineHelper.GetDocumentSymbolModels(documentSymbols);
                return new DocumentSymbolModel(documentSymbolItems, lspSnapshot);
            }

            string? GetFilePath()
            {
                ThreadingContext.ThrowIfNotOnUIThread();
                if (EditorAdaptersFactoryService.GetBufferAdapter(textBuffer) is IPersistFileFormat persistFileFormat &&
                    ErrorHandler.Succeeded(persistFileFormat.GetCurFile(out var filePath, out _)))
                {
                    return filePath;
                }

                return null;
            }
        }

        /// <summary>
        /// Starts a new task to update the UI.
        /// </summary>
        private void StartUpdateUITask()
        {
            // 'true' value is unused.  this just signals to the queue that we have work to do.
            _updateUIQueue.AddWork(true);
        }

        /// <summary>
        /// Filters and sorts the DocumentSymbolItems then updates the UI.
        /// </summary>
        private async ValueTask<DocumentSymbolModel?> UpdateUIAsync(ImmutableSegmentedList<bool> unused, CancellationToken cancellationToken)
        {
            var model = await _computeModelQueue.WaitUntilCurrentBatchCompletesAsync().ConfigureAwait(false);
            if (model is null)
                return null;

            // Switch to the UI thread to get the current search query and latest active text view.
            await ThreadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var searchQuery = searchBox.Text;

            var activeTextView = GetLastActiveIWpfTextView();
            if (activeTextView is null)
                return null;

            // Switch to the threadpool to filter and sort the DocumentSymbolItems.
            await TaskScheduler.Default;

            var updatedDocumentSymbolItems = model.DocumentSymbolItems;

            if (!string.IsNullOrWhiteSpace(searchQuery))
                updatedDocumentSymbolItems = DocumentOutlineHelper.Search(updatedDocumentSymbolItems, searchQuery);

            updatedDocumentSymbolItems = DocumentOutlineHelper.Sort(updatedDocumentSymbolItems, SortOption);

            // Switch to the UI thread to update the view.
            await ThreadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            symbolTree.ItemsSource = updatedDocumentSymbolItems;

            StartHightlightNodeTask();

            return new DocumentSymbolModel(updatedDocumentSymbolItems, model.LspSnapshot);
        }

        /// <summary>
        /// Starts a new task to highlight the symbol node corresponding to the current caret position in the editor.
        /// </summary>
        private void StartHightlightNodeTask()
        {
            _highlightNodeQueue.AddWork();
        }

        /// <summary>
        /// Highlights the symbol node corresponding to the current caret position in the editor.
        /// </summary>
        private async ValueTask HightlightNodeAsync(CancellationToken cancellationToken)
        {
            var model = await _updateUIQueue.WaitUntilCurrentBatchCompletesAsync().ConfigureAwait(false);
            if (model is null)
                return;

            // Switch to the UI thread to get the current caret point, latest active text view, and currently selected tree node.
            await ThreadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var activeTextView = GetLastActiveIWpfTextView();
            if (activeTextView is null)
                return;

            var caretPoint = activeTextView.GetCaretPoint(activeTextView.TextBuffer);
            if (!caretPoint.HasValue)
                return;

            var selectedDocumentSymbolItem = (DocumentSymbolItem?)symbolTree.SelectedItem;

            DocumentOutlineHelper.SelectDocumentNode(
                ThreadingContext, model, selectedDocumentSymbolItem, activeTextView.TextSnapshot, caretPoint.Value.Position);
        }

        /// <summary>
        /// Starts a new task to select code when a symbol node is clicked.
        /// </summary>
        private void StartJumpToContent(DocumentSymbolItem symbol)
        {
            _jumpToContentQueue.AddWork(symbol);
        }

        /// <summary>
        /// Given a DocumentSymbolItem, moves the caret to its position in the latest active text view.
        /// </summary>
        private async ValueTask JumpToContentAsync(ImmutableSegmentedList<DocumentSymbolItem> symbol, CancellationToken cancellationToken)
        {
            if (symbol.IsDefault || symbol.IsEmpty)
                return;

            var model = await _computeModelQueue.WaitUntilCurrentBatchCompletesAsync().ConfigureAwait(false);
            if (model is null)
                return;

            // Switch to the UI thread to update the latest active text view.
            await ThreadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var activeTextView = GetLastActiveIWpfTextView();
            if (activeTextView is null)
                return;

            // When the user clicks on a symbol node in the window, we want to move the cursor to that line in the editor. If we don't
            // unsubscribe from Caret_PositionChanged first, we will call StartHightlightNodeTask() once we move the cursor ourselves.
            // This is not ideal because we would be doing extra work to highlight a node that's already highlighted.
            activeTextView.Caret.PositionChanged -= Caret_PositionChanged;

            try
            {
                // Get the original position of the start of the line of the symbol.
                var originalPosition = model.LspSnapshot.GetLineFromLineNumber(symbol.First().StartPosition.Line).Start.Position;

                // Map this position to a span in the current textview.
                var originalSpan = new SnapshotSpan(model.LspSnapshot, Span.FromBounds(originalPosition, originalPosition));
                var currentSpan = originalSpan.TranslateTo(activeTextView.TextSnapshot, SpanTrackingMode.EdgeExclusive);

                // Set the active text view selection to this span.
                activeTextView.SetSelection(currentSpan);
                activeTextView.ViewScroller.EnsureSpanVisible(currentSpan);
            }
            finally
            {
                // Resubscribe to Caret_PositionChanged again so that when the user clicks somewhere else, we can highlight that node.
                activeTextView.Caret.PositionChanged += Caret_PositionChanged;
            }
        }
    }
}
