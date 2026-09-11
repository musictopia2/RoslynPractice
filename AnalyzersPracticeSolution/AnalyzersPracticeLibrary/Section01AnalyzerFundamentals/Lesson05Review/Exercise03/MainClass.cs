/*
Enter the requirements for this exercise here.
Requirement

Create an analyzer that supports two diagnostic rules, but reports only one of them during compilation analysis.

Use these exact rules.

Rule 1

ID: REVIEW101
Title: Primary Review
Message: Primary review diagnostic
Category: Review
Severity: DiagnosticSeverity.Warning
Enabled by default: true

Rule 2

ID: REVIEW102
Title: Secondary Review
Message: Secondary review diagnostic
Category: Review
Severity: DiagnosticSeverity.Info
Enabled by default: true
Analyzer requirements

Your analyzer must:

Return both rules from SupportedDiagnostics.
Configure generated-code analysis with GeneratedCodeAnalysisFlags.None.
Enable concurrent execution.
Register a compilation analysis action.
During compilation analysis, report only REVIEW101.
Use Location.None.
Do not report REVIEW102.
Do not use symbol, syntax-node, semantic-model, or operation actions.
Expected behavior

Any valid C# compilation should produce exactly one analyzer diagnostic:

REVIEW101: Primary review diagnostic

REVIEW102 must still appear in SupportedDiagnostics, even though it is not reported.

This is useful review because it separates two ideas that are easy to blur together:

supported rules
rules actually reported during a particular analysis
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson05Review.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [PrimaryRule, SecondaryRule];
        }
    }
    private static string _category = "Review";
    private static DiagnosticDescriptor PrimaryRule { get; } =
        bb1.Create().WithId("REVIEW101").WithTitle("Primary Review")
        .WithMessage("Primary review diagnostic")
        .WithCategory(_category).WithSeverity(DiagnosticSeverity.Warning)
        .EnabledByDefault().Build();
    private static DiagnosticDescriptor SecondaryRule { get; } =
        bb1.Create().WithId("REVIEW102").WithTitle("Secondary Review")
        .WithMessage("Secondary review diagnostic")
        .WithCategory(_category).WithSeverity(DiagnosticSeverity.Info)
        .EnabledByDefault().Build();
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterDiagnosticRulesWithNoneLocations(PrimaryRule);
        context.EnableConcurrentExecution();
    }
}