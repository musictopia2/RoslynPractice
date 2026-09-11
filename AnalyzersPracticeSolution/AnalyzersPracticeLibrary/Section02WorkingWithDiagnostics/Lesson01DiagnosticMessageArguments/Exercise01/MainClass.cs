/*
Enter the requirements for this exercise here.
Create one diagnostic rule with the following descriptor:

ID: RPA101
Title: Compilation Item
Message format: Compilation contains '{0}'
Category: Practice
Severity: DiagnosticSeverity.Info
Enabled by default: true

SupportedDiagnostics must contain this rule.

Configure generated-code analysis using:
GeneratedCodeAnalysisFlags.None

Enable concurrent execution.

Register a compilation analysis callback.

When the callback runs, create and report one diagnostic using:
Location.None

Supply the message argument:
"Main Compilation"

For any valid C# compilation, the analyzer must therefore report exactly
one diagnostic whose final message is:

Compilation contains 'Main Compilation'

The descriptor's MessageFormat must remain:

Compilation contains '{0}'

Do not build the final message yourself before creating the diagnostic.
Use Roslyn's diagnostic message-argument mechanism.

Do not use syntax-node, symbol, semantic-model, or operation actions.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson01DiagnosticMessageArguments.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("RPA101")
        .WithTitle("Compilation Item")
        .WithMessage("Compilation contains '{0}'")
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
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, "Main Compilation");
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}