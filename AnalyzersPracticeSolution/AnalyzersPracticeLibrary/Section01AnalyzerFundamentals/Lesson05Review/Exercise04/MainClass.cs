/*
Enter the requirements for this exercise here.
Requirement

Create an analyzer with two supported compilation-level rules. Both rules must be reported for every valid C# compilation.

Rule 1
ID: REVIEW201
Title: Compilation Warning
Message: Compilation review warning
Category: Review
Severity: DiagnosticSeverity.Warning
Enabled by default: true
Location: Location.None
Rule 2
ID: REVIEW202
Title: Compilation Information
Message: Compilation review information
Category: Review
Severity: DiagnosticSeverity.Info
Enabled by default: true
Location: Location.None
Analyzer requirements
Return both rules from SupportedDiagnostics.
Configure generated-code analysis with GeneratedCodeAnalysisFlags.None.
Enable concurrent execution.
Register compilation analysis behavior.
Report both diagnostics for each compilation.
Each diagnostic must use its matching descriptor.
Both diagnostics must use Location.None.
Do not use syntax-node, symbol, semantic-model, or operation actions.
Expected behavior

Given:

class Sample
{
}

the analyzer should produce exactly two diagnostics:

REVIEW201: Compilation review warning
REVIEW202: Compilation review information

The order of the two diagnostics does not matter.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson05Review.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [WarningRule, InfoRule];
        }
    }
    private static string _category = "Review";
    private static DiagnosticDescriptor WarningRule { get; } =
        bb1.Create().WithId("REVIEW201").WithTitle("Compilation Warning")
        .WithMessage("Compilation review warning")
        .WithCategory(_category).WithSeverity(DiagnosticSeverity.Warning)
        .EnabledByDefault().Build();
    private static DiagnosticDescriptor InfoRule { get; } =
        bb1.Create().WithId("REVIEW202").WithTitle("Compilation Information")
        .WithMessage("Compilation review information")
        .WithCategory(_category).WithSeverity(DiagnosticSeverity.Info)
        .EnabledByDefault().Build();
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterDiagnosticRulesWithNoneLocations(WarningRule, InfoRule);
        context.EnableConcurrentExecution();
    }
}