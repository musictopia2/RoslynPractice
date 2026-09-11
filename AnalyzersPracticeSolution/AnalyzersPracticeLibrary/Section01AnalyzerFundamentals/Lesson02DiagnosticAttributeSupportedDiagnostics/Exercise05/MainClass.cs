/*
Enter the requirements for this exercise here.
A development team wants an analyzer package to expose a small set of diagnostics for different review situations.

This exercise is intentionally less prescriptive than the earlier ones, but every required value is still part of the contract.

Create three supported diagnostic rules.

Use these property names and make them cached static get-only properties:

CorrectnessRule
MaintainabilityRule
ExperimentalRule

Use your guided DiagnosticDescriptorBuilder for all three.

The rules are:

CorrectnessRule
ID: RPA008
Title: Possible correctness issue
Message: This code may contain a correctness issue
Category: Correctness
Severity: Error
Enabled by default: true
MaintainabilityRule
ID: RPA009
Title: Maintainability review suggested
Message: This code may be difficult to maintain
Category: Maintainability
Severity: Warning
Enabled by default: true
ExperimentalRule
ID: RPA010
Title: Experimental analysis available
Message: Experimental analysis is available for this code
Category: Experimental
Severity: Info
Enabled by default: false

SupportedDiagnostics must advertise all three rules.

For Initialize, the client has these requirements:

Generated code must not be analyzed.
Concurrent execution must be enabled.

Do not register any analysis actions yet.

The extra challenge compared with Exercise 04 is that you now need to keep the three cached descriptors organized, choose the correct severity and enabled state for each one, and correctly combine that with the required analyzer initialization behavior.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson02DiagnosticAttributeSupportedDiagnostics.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [CorrectnessRule, MaintainabilityRule, ExperimentalRule];
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();
    }
    private static DiagnosticDescriptor CorrectnessRule { get; } =
        bb1.Create().WithId("RPA008")
        .WithTitle("Possible correctness issue")
        .WithMessage("This code may contain a correctness issue")
        .WithCategory("Correctness")
        .WithSeverity(DiagnosticSeverity.Error)
        .EnabledByDefault().Build();
    private static DiagnosticDescriptor MaintainabilityRule { get; } =
        bb1.Create().WithId("RPA009")
        .WithTitle("Maintainability review suggested")
        .WithMessage("This code may be difficult to maintain")
        .WithCategory("Maintainability")
        .WithSeverity(DiagnosticSeverity.Warning)
        .EnabledByDefault().Build();
    private static DiagnosticDescriptor ExperimentalRule { get; } =
        bb1.Create().WithId("RPA010")
        .WithTitle("Experimental analysis available")
        .WithMessage("Experimental analysis is available for this code")
        .WithCategory("Experimental")
        .WithSeverity(DiagnosticSeverity.Info)
        .DisabledByDefault().Build();

}