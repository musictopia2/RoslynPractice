/*
Create one diagnostic rule with the following descriptor:

ID: RPA104
Title: Compilation Summary
Message format:
Compilation '{0}' contains {1} syntax trees and {2} references

Category: Practice
Severity: DiagnosticSeverity.Info
Enabled by default: true

SupportedDiagnostics must contain this rule.

Configure generated-code analysis using:
GeneratedCodeAnalysisFlags.None

Enable concurrent execution.

Register a compilation analysis callback.

Use the Compilation supplied by CompilationAnalysisContext to obtain:

1. The compilation's assembly name.
2. The number of syntax trees.
3. The number of metadata references.

Create and report exactly one diagnostic using:
Location.None

Supply all three values as message arguments in the correct order.

For example, a compilation named AccountingLibrary containing
3 syntax trees and 4 metadata references must produce:

Compilation 'AccountingLibrary' contains 3 syntax trees and 4 references

Do not hard-code any of the three values.

The descriptor's MessageFormat must remain:

Compilation '{0}' contains {1} syntax trees and {2} references

Do not manually construct the final message.
Use Diagnostic.Create and Roslyn's message-argument mechanism.

Do not use syntax-node, symbol, semantic-model, or operation actions.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson01DiagnosticMessageArguments.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("RPA104")
        .WithTitle("Compilation Summary")
        .WithMessage("Compilation '{0}' contains {1} syntax trees and {2} references")
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
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, context.Compilation.AssemblyName, context.Compilation.SyntaxTrees.Count(), context.Compilation.References.Count());
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}