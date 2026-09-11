/*
Enter the requirements for this exercise here.
A development team wants a temporary analyzer that confirms three separate compilation-level validation stages are running while they develop their analyzer infrastructure.

Create the analyzer according to the following client requirements.

The analyzer must support these three diagnostics:

**APR0007 — Configuration validation**

* Title: `Configuration validation completed`
* Message: `The analyzer configuration was validated`
* Category: `Validation`
* Severity: `DiagnosticSeverity.Info`
* Enabled by default: `true`

**APR0008 — Project validation**

* Title: `Project validation completed`
* Message: `The project-level validation completed`
* Category: `Validation`
* Severity: `DiagnosticSeverity.Warning`
* Enabled by default: `true`

**APR0009 — Compilation validation**

* Title: `Compilation validation completed`
* Message: `The compilation-level validation completed`
* Category: `Validation`
* Severity: `DiagnosticSeverity.Info`
* Enabled by default: `true`

The analyzer must advertise all three diagnostics through `SupportedDiagnostics`.

The client wants the checks organized into **two independent compilation analysis callbacks**.

One callback is responsible for the configuration and project validations. When it runs, it must report both `APR0007` and `APR0008`.

The other callback is responsible for compilation validation. When it runs, it must report `APR0009`.

All three diagnostics must use `Location.None`.

For each compilation, the finished analyzer must therefore report exactly:

* one `APR0007`;
* one `APR0008`;
* one `APR0009`.

Continue configuring generated-code analysis with `GeneratedCodeAnalysisFlags.None` and enabling concurrent execution.

Do not inspect syntax, symbols, semantic information, operations, or compilation contents.

You may choose your own names for the two analysis callback methods and for any helper method used to register them.

*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic.Exercise05;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [ConfigurationRule, ProjectRule, CompilationRule];
        }
    }
    private static DiagnosticDescriptor ConfigurationRule { get; } =
            bb1.Create()
                .WithId("APR0007")
                .WithTitle("Configuration validation completed")
                .WithMessage("The analyzer configuration was validated")
                .BuildWithValidationCategory(DiagnosticSeverity.Info);
    private static DiagnosticDescriptor ProjectRule { get; } =
        bb1.Create().WithId("APR0008").WithTitle("Project validation completed")
        .WithMessage("The project-level validation completed")
        .BuildWithValidationCategory(DiagnosticSeverity.Warning);
    private static DiagnosticDescriptor CompilationRule { get; } =
        bb1.Create().WithId("APR0009").WithTitle("Compilation validation completed")
        .WithMessage("The compilation-level validation completed")
        .BuildWithValidationCategory(DiagnosticSeverity.Info);
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterDiagnosticRulesWithNoneLocations(ConfigurationRule, ProjectRule);
        context.RegisterDiagnosticRulesWithNoneLocations(CompilationRule);
        context.EnableConcurrentExecution();
    }
}