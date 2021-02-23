using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jacobi.VisualStudio.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HandleReturnValueCodeFix)), Shared]
    public sealed class HandleReturnValueCodeFix : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(HandleReturnValueAnalyzer.DiagnosticId);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var invocation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use discard '_'",
                    createChangedDocument: c => UseDiscardAsync(context.Document, invocation, c),
                    equivalenceKey: $"{HandleReturnValueAnalyzer.DiagnosticId}CodeFix"),
                diagnostic);
        }

        // old:
        // InvocationExpression (diagnostics)

        // new:
        // ExpressionStatement
        //  SimpleAssignmentExpression
        //   IdentifierToken
        //    Lead (whitespace from InvocationExpression)
        //    Trail: 1 space
        //   EqualsToken
        //    Trail: 1 space
        //   InvocationExpression (diagnostic)
        private async Task<Document> UseDiscardAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var newInvocation = SyntaxFactory.InvocationExpression(
                invocation.Expression, invocation.ArgumentList);

            var replace =
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName("_"),
                    newInvocation
                )
                // whitespace formatting
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var newRoot = root.ReplaceNode(invocation, replace);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
