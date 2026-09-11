/*
Enter the requirements for this exercise here.
Create an analyzer that reports two diagnostics whenever a compilation is analyzed.

Requirements:

1. Create two private static `DiagnosticDescriptor` members named:

   * `FirstRule`
   * `SecondRule`

2. Configure `FirstRule` with these exact values:

   * ID: `APR0003`
   * Title: `Primary compilation check`
   * Message: `The primary compilation check completed`
   * Category: `Practice`
   * Severity: `DiagnosticSeverity.Warning`
   * Enabled by default: `true`

3. Configure `SecondRule` with these exact values:

   * ID: `APR0004`
   * Title: `Secondary compilation check`
   * Message: `The secondary compilation check completed`
   * Category: `Practice`
   * Severity: `DiagnosticSeverity.Info`
   * Enabled by default: `true`

4. `SupportedDiagnostics` must return both descriptors.

5. Keep the existing initialization behavior:

   * configure generated-code analysis with `GeneratedCodeAnalysisFlags.None`;
   * enable concurrent execution;
   * register one compilation analysis callback.

6. The compilation callback must be named:

   `AnalyzeCompilation`

7. `AnalyzeCompilation` must receive a `CompilationAnalysisContext`.

8. Inside `AnalyzeCompilation`:

   * create one `Diagnostic` from `FirstRule`;
   * create one `Diagnostic` from `SecondRule`;
   * use `Location.None` for both;
   * report both diagnostics through the provided context.

9. Each time the compilation callback runs, exactly two diagnostics must be reported.

10. Do not inspect the compilation, syntax, symbols, semantic information, or operations.

*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic.Exercise03;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor FirstRule { get; } =
        bb1.Create().WithId("APR0003")
        .WithTitle("Primary compilation check")
        .WithMessage("The primary compilation check completed")
        .WithCategory("Practice")
        .WithSeverity(DiagnosticSeverity.Warning)
        .EnabledByDefault().Build();
    private static DiagnosticDescriptor SecondRule { get; } =
        bb1.Create().WithId("APR0004")
        .WithTitle("Secondary compilation check")
        .WithMessage("The secondary compilation check completed")
        .WithCategory("Practice")
        .WithSeverity(DiagnosticSeverity.Info)
        .EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [FirstRule, SecondRule];
        }
    }
    private static void RegisterAnalysisActions(
        AnalysisContext context)
    {
        context.RegisterCompilationAction(
            AnalyzeCompilation);
    }
    private static void AnalyzeCompilation(
       CompilationAnalysisContext context)
    {
        // Analyze/report here.
        Diagnostic diagnostic = Diagnostic.Create(
           FirstRule,
           Location.None);
        context.ReportDiagnostic(diagnostic);
        diagnostic = Diagnostic.Create(
           SecondRule,
           Location.None);
        context.ReportDiagnostic(diagnostic);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        RegisterAnalysisActions(context);
        context.EnableConcurrentExecution();
    }
}