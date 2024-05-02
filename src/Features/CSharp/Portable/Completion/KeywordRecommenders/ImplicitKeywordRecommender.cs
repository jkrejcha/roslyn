﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.CSharp.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Completion.KeywordRecommenders;

internal sealed class ImplicitKeywordRecommender() : AbstractSyntacticSingleKeywordRecommender(SyntaxKind.ImplicitKeyword)
{
    private static readonly ISet<SyntaxKind> s_validNonInterfaceMemberModifiers = new HashSet<SyntaxKind>(SyntaxFacts.EqualityComparer)
        {
            SyntaxKind.StaticKeyword,
            SyntaxKind.PublicKeyword,
            SyntaxKind.ExternKeyword,
            SyntaxKind.UnsafeKeyword,
        };

    private static readonly ISet<SyntaxKind> s_validInterfaceMemberModifiers = new HashSet<SyntaxKind>(SyntaxFacts.EqualityComparer)
        {
            SyntaxKind.StaticKeyword,
            SyntaxKind.PublicKeyword,
            SyntaxKind.AbstractKeyword,
            SyntaxKind.UnsafeKeyword,
        };
    private static readonly ISet<SyntaxKind> s_validExtensionModifiers = new HashSet<SyntaxKind>(SyntaxFacts.EqualityComparer)
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword,
            SyntaxKind.PrivateKeyword,
            SyntaxKind.UnsafeKeyword,
            SyntaxKind.FileKeyword,
            SyntaxKind.NewKeyword,
            SyntaxKind.StaticKeyword,
        };

    protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
    {
        // 'implicit extension' is legal, so check if we're in an appropriate type declaration context
        if (context.IsTypeDeclarationContext(s_validExtensionModifiers, validTypeDeclarations: SyntaxKindSet.NonEnumTypeDeclarations, canBePartial: true, cancellationToken))
        {
            return true;
        }
        else if (context.IsMemberDeclarationContext(validModifiers: s_validNonInterfaceMemberModifiers, validTypeDeclarations: SyntaxKindSet.ClassStructRecordTypeDeclarations, canBePartial: false, cancellationToken: cancellationToken))
        {
            // operators must be both public and static
            var modifiers = context.PrecedingModifiers;

            return
                modifiers.Contains(SyntaxKind.PublicKeyword) &&
                modifiers.Contains(SyntaxKind.StaticKeyword);
        }
        else if (context.IsMemberDeclarationContext(validModifiers: s_validInterfaceMemberModifiers, validTypeDeclarations: SyntaxKindSet.InterfaceOnlyTypeDeclarations, canBePartial: false, cancellationToken: cancellationToken))
        {
            // operators must be both abstract and static
            var modifiers = context.PrecedingModifiers;

            return
                modifiers.Contains(SyntaxKind.AbstractKeyword) &&
                modifiers.Contains(SyntaxKind.StaticKeyword);
        }

        return false;
    }
}
