/*
Enter the requirements for this exercise here.
Requirements

Create a private static read-only property named:

ReviewRule

Use the property form we just discussed, so the descriptor is created once rather than recreated on every access:

private static DiagnosticDescriptor ReviewRule { get; } = new(
    ...
);

Configure it with these exact values:

Diagnostic ID: RPA002
Title: Code requires review
Message: This code requires manual review
Category: Review
Default severity: Info
Enabled by default: true

Update SupportedDiagnostics so the analyzer advertises ReviewRule.

Keep:

[DiagnosticAnalyzer(LanguageNames.CSharp)]

and leave Initialize unchanged.

There should be exactly one supported diagnostic.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson02DiagnosticAttributeSupportedDiagnostics.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor ReviewRule
    {
        get
        {
            return DiagnosticDescriptorBuilder.Create()
                .WithId("RPA002")
                .WithTitle("Code requires review")
                .WithMessage("This code requires manual review")
                .WithCategory("Review")
                .WithSeverity(DiagnosticSeverity.Info)
                .EnabledByDefault()
                .Build();
        }
    }
    

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [ReviewRule];
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();
    }
}