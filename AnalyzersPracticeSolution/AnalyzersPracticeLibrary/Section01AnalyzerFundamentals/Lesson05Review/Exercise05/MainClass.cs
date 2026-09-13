/*
Enter the requirements for this exercise here.
Scenario

A client is building an internal compilation-checking tool. They want an analyzer containing three supported diagnostic rules, but different compilation callbacks are responsible for different rules.

Diagnostic rules

Rule 1 — CLIENT201

Title: Compilation Validation
Message: Compilation validation completed
Category: ClientReview
Severity: DiagnosticSeverity.Warning
Enabled by default: true
Location: Location.None

Rule 2 — CLIENT202

Title: Compilation Tracking
Message: Compilation tracking completed
Category: ClientReview
Severity: DiagnosticSeverity.Info
Enabled by default: true
Location: Location.None

Rule 3 — CLIENT203

Title: Available Client Rule
Message: Additional client rule available
Category: ClientReview
Severity: DiagnosticSeverity.Info
Enabled by default: true
Requirements

All three descriptors must appear in SupportedDiagnostics.

Configure generated-code analysis using:

GeneratedCodeAnalysisFlags.None

and enable concurrent execution.

Register two separate compilation analysis callbacks. Do not use one callback to report both diagnostics.

The first callback must report:

CLIENT201

The second callback must report:

CLIENT202

Both reported diagnostics must use Location.None.

CLIENT203 is a supported rule but must not be reported by either callback.

For any valid C# compilation, the final analyzer output therefore contains exactly two diagnostics:

CLIENT201: Compilation validation completed
CLIENT202: Compilation tracking completed

The order does not matter.

Do not use syntax-node, symbol, semantic-model, or operation actions.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson05Review.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [ValidationRule, TrackingRule, AvailableRule];
        }
    }
    private static DiagnosticDescriptor ValidationRule { get; } =
            bb1.Create()
                .WithId("CLIENT201")
                .WithTitle("Compilation Validation")
                .WithMessage("Compilation validation completed")
                .BuildWithClientReviewCategory(DiagnosticSeverity.Warning);
    private static DiagnosticDescriptor TrackingRule { get; } =
        bb1.Create().WithId("CLIENT202").WithTitle("Compilation Tracking")
        .WithMessage("Compilation tracking completed")
        .BuildWithClientReviewCategory(DiagnosticSeverity.Info);
    private static DiagnosticDescriptor AvailableRule { get; } =
        bb1.Create().WithId("CLIENT203").WithTitle("Available Client Rule")
        .WithMessage("Additional client rule available")
        .BuildWithClientReviewCategory(DiagnosticSeverity.Info);
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterDiagnosticRulesWithNoneLocations(ValidationRule);
        context.RegisterDiagnosticRulesWithNoneLocations(TrackingRule);
        context.EnableConcurrentExecution();
    }
}