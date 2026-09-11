/*
Enter the requirements for this exercise here.
/*
Create one diagnostic rule with the following descriptor:

ID: RPA102
Title: Compilation Name
Message format: Analyzing compilation '{0}'
Category: Practice
Severity: DiagnosticSeverity.Info
Enabled by default: true

SupportedDiagnostics must contain this rule.

Configure generated-code analysis using:
GeneratedCodeAnalysisFlags.None

Enable concurrent execution.

Register a compilation analysis callback.

When the callback runs, obtain the name of the compilation being analyzed.

Create and report one diagnostic using:
Location.None

Supply the compilation's name as the message argument.

Do not hard-code the compilation name.

For example, if the compilation is named:

AccountingLibrary

the resulting diagnostic message must be:

Analyzing compilation 'AccountingLibrary'

If the same analyzer instead analyzes a compilation named:

ShippingLibrary

the resulting diagnostic message must be:

Analyzing compilation 'ShippingLibrary'

The descriptor's MessageFormat must remain:

Analyzing compilation '{0}'

Do not manually construct the final diagnostic message.
Use Roslyn's diagnostic message-argument mechanism.

Do not use syntax-node, symbol, semantic-model, or operation actions.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson01DiagnosticMessageArguments.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{

    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("RPA102")
        .WithTitle("Compilation Name")
        .WithMessage("Analyzing compilation '{0}'")
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
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, context.Compilation.AssemblyName);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}