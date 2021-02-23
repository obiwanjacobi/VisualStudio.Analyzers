using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Jacobi.VisualStudio.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class HandleReturnValueAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JA101";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Handle Method return values.",
            "Handle the return value of '{0}'.",
            "Correctness",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The return value of a method call must be used or explicitly discarded.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            if (context.Operation is IInvocationOperation invocation &&
                !invocation.TargetMethod.ReturnsVoid)
            {
                bool retValHandled = false;
                var parent = context.Operation.Parent;

                while (parent != null && parent.Kind != OperationKind.Block)
                {
                    if (// x = method()
                        parent.Kind == OperationKind.SimpleAssignment ||
                        // var v = method()
                        parent.Kind == OperationKind.VariableInitializer ||
                        // call(method())
                        parent.Kind == OperationKind.Argument ||
                        // return method()
                        parent.Kind == OperationKind.Return ||
                        // 2 + method() / method() == x
                        parent.Kind == OperationKind.Binary)
                    {
                        retValHandled = true;
                        break;
                    }

                    // await voidAsync()
                    if (parent.Kind == OperationKind.Await &&
                        invocation.TargetMethod.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task")
                    {
                        retValHandled = true;
                        break;
                    }

                    parent = parent.Parent;
                }

                if (!retValHandled)
                {
                    var diag = Diagnostic.Create(
                        Rule,
                        Location.Create(invocation.Syntax.SyntaxTree, invocation.Syntax.Span),
                        invocation.Syntax.GetText().ToString().Trim()
                        );
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}
