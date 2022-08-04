﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Rename.ConflictEngine;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Rename
{
    internal readonly record struct RenameRewriterParameters(
        ISet<TextSpan> ConflictLocationSpans,
        Solution OriginalSolution,
        SyntaxTree OriginalSyntaxTree,
        RenamedSpansTracker RenameSpansTracker,
        SyntaxNode SyntaxRoot,
        Document Document,
        SemanticModel SemanticModel,
        AnnotationTable<RenameAnnotation> RenameAnnotations,
        ImmutableArray<LocationRenameContext> TokenTextSpanRenameContexts,
        ImmutableArray<LocationRenameContext> StringAndCommentsTextSpanRenameContexts,
        ImmutableArray<RenamedSymbolContext> RenameSymbolContexts,
        CancellationToken CancellationToken);
}
