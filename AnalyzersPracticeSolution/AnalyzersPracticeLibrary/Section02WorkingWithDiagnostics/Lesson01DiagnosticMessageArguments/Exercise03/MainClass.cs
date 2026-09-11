/*
Create one diagnostic rule with the following descriptor:

ID: RPA103
Title: Compilation Details
Message format: Compilation '{0}' contains {1} syntax trees
Category: Practice
Severity: DiagnosticSeverity.Info
Enabled by default: true

SupportedDiagnostics must contain this rule.

Configure generated-code analysis using:
GeneratedCodeAnalysisFlags.None

Enable concurrent execution.

Register a compilation analysis callback.

Use the Compilation supplied by the CompilationAnalysisContext to obtain:

1. The compilation's assembly name.
2. The number of syntax trees in the compilation.

Create and report exactly one diagnostic using:
Location.None

Supply those two values as message arguments in the correct order.

For example, if the compilation is named:

AccountingLibrary

and contains 3 syntax trees, the resulting message must be:

Compilation 'AccountingLibrary' contains 3 syntax trees

Do not hard-code either value.

The descriptor's MessageFormat must remain:

Compilation '{0}' contains {1} syntax trees

Do not manually construct the final message.
Use Diagnostic.Create and Roslyn's message-argument mechanism.

Do not use syntax-node, symbol, semantic-model, or operation actions.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson01DiagnosticMessageArguments.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("RPA103")
        .WithTitle("Compilation Details")
        .WithMessage("Compilation '{0}' contains {1} syntax trees")
        .WithCategory("Practice")
        .WithSeverity(DiagnosticSeverity.Info).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, context.Compilation.AssemblyName, context.Compilation.SyntaxTrees.Count());
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}