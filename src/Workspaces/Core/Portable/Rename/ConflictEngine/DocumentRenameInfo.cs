﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Rename.ConflictEngine;

/// <summary>
/// Contains all the immutable information to rename a document.
/// </summary>
internal record DocumentRenameInfo(
    ImmutableDictionary<TextSpan, LocationRenameContext> TextSpanToLocationContexts,
    ImmutableDictionary<SymbolKey, RenamedSymbolContext> RenamedSymbolContexts,
    MultiDictionary<TextSpan, StringAndCommentRenameInfo> TextSpanToStringAndCommentRenameContexts,
    ImmutableHashSet<string> AllReplacementTexts,
    ImmutableHashSet<string> AllOriginalText,
    ImmutableHashSet<string> AllPossibleConflictNames)
{
    public (DocumentRenameInfo newDocumentRenameInfo, bool isOverlappingLocation) WithLocationRenameContext(LocationRenameContext locationRenameContext)
    {
        RoslynDebug.Assert(!locationRenameContext.RenameLocation.IsRenameInStringOrComment);
        var textSpan = locationRenameContext.RenameLocation.Location.SourceSpan;
        if (TextSpanToLocationContexts.TryGetValue(textSpan, out var existingLocationContext))
        {
            return (this, !locationRenameContext.Equals(existingLocationContext));
        }
        else
        {
            return (this with { TextSpanToLocationContexts = TextSpanToLocationContexts.Add(textSpan, locationRenameContext ) }, false );
        }
    }

    public (DocumentRenameInfo newDocumentRenameInfo, bool isOverlappingLocation) WithStringAndCommentRenameContext(LocationRenameContext locationRenameContext)
    {
        RoslynDebug.Assert(locationRenameContext.RenameLocation.IsRenameInStringOrComment);
        var containingLocationSpan = locationRenameContext.RenameLocation.ContainingLocationForStringOrComment;
        if (TextSpanToStringAndCommentRenameContexts.TryGetValue(containingLocationSpan, out var replacementLocations))
        {
            if (replacementLocations.Contains(locationRenameContext))
            {
                return (this, true);
            }

        }
        else
        {

        }

    }
}
