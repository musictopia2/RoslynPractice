/*
Enter the requirements for this exercise here.
Requirement

Create an analyzer that reports exactly one diagnostic for every compilation it analyzes.

Use these exact diagnostic values:

Diagnostic ID: REVIEW001
Title: Compilation Reviewed
Message: The compilation was analyzed
Category: Review
Severity: DiagnosticSeverity.Info
Enabled by default: true
Location: Location.None
Analyzer requirements

Your analyzer must:

Have the normal DiagnosticAnalyzer attribute for C#.
Create a DiagnosticDescriptor containing the values above.
Return that descriptor from SupportedDiagnostics.
Configure generated-code analysis with:
GeneratedCodeAnalysisFlags.None
Enable concurrent execution.
Register a compilation analysis action.
The registered callback must create and report the diagnostic.
The diagnostic must use the descriptor from this exercise.
Do not use symbol, syntax-node, operation, or semantic-model actions.
Expected behavior

It should not matter what valid C# source is compiled.

For example:

class Sample
{
}

should produce exactly one diagnostic:

REVIEW001: The compilation was analyzed
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson05Review.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
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
        context.RegisterDiagnosticRulesWithNoneLocations(OnlyRule); //i assume this was needed too.
        context.EnableConcurrentExecution();
    }
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("REVIEW001")
        .WithTitle("Compilation Reviewed")
        .WithMessage("The compilation was analyzed")
        .WithCategory("Review").WithSeverity(DiagnosticSeverity.Info)
        .EnabledByDefault().Build();

}
/*
Enter the requirements for this exercise here.
Requirement

Create an analyzer that reports exactly one diagnostic for every compilation it analyzes.

Use these exact diagnostic values:

Diagnostic ID: REVIEW001
Title: Compilation Reviewed
Message: The compilation was analyzed
Category: Review
Severity: DiagnosticSeverity.Info
Enabled by default: true
Location: Location.None
Analyzer requirements

Your analyzer must:

Have the normal DiagnosticAnalyzer attribute for C#.
Create a DiagnosticDescriptor containing the values above.
Return that descriptor from SupportedDiagnostics.
Configure generated-code analysis with:
GeneratedCodeAnalysisFlags.None
Enable concurrent execution.
Register a compilation analysis action.
The registered callback must create and report the diagnostic.
The diagnostic must use the descriptor from this exercise.
Do not use symbol, syntax-node, operation, or semantic-model actions.
Expected behavior

It should not matter what valid C# source is compiled.

For example:

class Sample
{
}

should produce exactly one diagnostic:

REVIEW001: The compilation was analyzed
*/