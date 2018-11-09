using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Test3
{
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Test3CodeFixProvider)), Shared]
    public class Test3CodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Test3Analyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => MakeConstAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> MakeConstAsync(Document contextDocument, LocalDeclarationStatementSyntax declaration, CancellationToken cancellationToken)
        {
            var firstToken = declaration.GetFirstToken();
            var trivia = firstToken.LeadingTrivia;
            var trimmedLocal = declaration.ReplaceToken(
                firstToken,
                firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

            var constToken = SyntaxFactory.Token(
                trivia,
                SyntaxKind.ConstKeyword,
                SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

            var newModifiers = trimmedLocal.Modifiers.Insert(0, constToken);
            var newLocal = trimmedLocal.WithModifiers(newModifiers);
            var formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

            var root = await contextDocument.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, formattedLocal);
            return contextDocument.WithSyntaxRoot(newRoot);
        }
    }
}
