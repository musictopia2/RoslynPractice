/*
Enter the requirements for this exercise here.
Create an analyzer that registers two separate compilation analysis callbacks, with each callback reporting its own diagnostic.

Requirements:

1. Create two private static `DiagnosticDescriptor` members named:

   * `FirstRule`
   * `SecondRule`

2. Configure `FirstRule` with these exact values:

   * ID: `APR0005`
   * Title: `First compilation check`
   * Message: `The first compilation check completed`
   * Category: `Practice`
   * Severity: `DiagnosticSeverity.Warning`
   * Enabled by default: `true`

3. Configure `SecondRule` with these exact values:

   * ID: `APR0006`
   * Title: `Second compilation check`
   * Message: `The second compilation check completed`
   * Category: `Practice`
   * Severity: `DiagnosticSeverity.Info`
   * Enabled by default: `true`

4. `SupportedDiagnostics` must return both rules.

5. Create a private static method named:

   `RegisterAnalysisActions`

6. `RegisterAnalysisActions` must register two compilation actions:

   * `AnalyzeFirstCheck`
   * `AnalyzeSecondCheck`

7. Both analysis methods must receive a `CompilationAnalysisContext`.

8. `AnalyzeFirstCheck` must:

   * create one diagnostic using `FirstRule`;
   * use `Location.None`;
   * report that diagnostic.

9. `AnalyzeSecondCheck` must:

   * create one diagnostic using `SecondRule`;
   * use `Location.None`;
   * report that diagnostic.

10. `Initialize` must:

* configure generated-code analysis with `GeneratedCodeAnalysisFlags.None`;
* call `RegisterAnalysisActions`;
* enable concurrent execution.

11. Exactly two diagnostics must be reported for a compilation: one from each callback.

12. Do not inspect the compilation, syntax, symbols, semantic information, or operations.

*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor FirstRule { get; } =
        bb1.Create().WithId("APR0005")
        .WithTitle("First compilation check")
        .WithMessage("The first compilation check completed")
        .WithCategory("Practice")
        .WithSeverity(DiagnosticSeverity.Warning)
        .EnabledByDefault().Build();
    private static DiagnosticDescriptor SecondRule { get; } =
        bb1.Create().WithId("APR0006")
        .WithTitle("Second compilation check")
        .WithMessage("The second compilation check completed")
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
            AnalyzeFirstCheck);
        context.RegisterCompilationAction(AnalyzeSecondCheck);
    }

    private static void AnalyzeFirstCheck(CompilationAnalysisContext context)
    {
        Diagnostic diagnostic = Diagnostic.Create(
           FirstRule,
           Location.None);
        context.ReportDiagnostic(diagnostic);
    }
    private static void AnalyzeSecondCheck(CompilationAnalysisContext context)
    {
        Diagnostic diagnostic = Diagnostic.Create(
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
/*
Enter the requirements for this exercise here.
Create an analyzer that registers two separate compilation analysis callbacks, with each callback reporting its own diagnostic.

Requirements:

1. Create two private static `DiagnosticDescriptor` members named:

   * `FirstRule`
   * `SecondRule`

2. Configure `FirstRule` with these exact values:

   * ID: `APR0005`
   * Title: `First compilation check`
   * Message: `The first compilation check completed`
   * Category: `Practice`
   * Severity: `DiagnosticSeverity.Warning`
   * Enabled by default: `true`

3. Configure `SecondRule` with these exact values:

   * ID: `APR0006`
   * Title: `Second compilation check`
   * Message: `The second compilation check completed`
   * Category: `Practice`
   * Severity: `DiagnosticSeverity.Info`
   * Enabled by default: `true`

4. `SupportedDiagnostics` must return both rules.

5. Create a private static method named:

   `RegisterAnalysisActions`

6. `RegisterAnalysisActions` must register two compilation actions:

   * `AnalyzeFirstCheck`
   * `AnalyzeSecondCheck`

7. Both analysis methods must receive a `CompilationAnalysisContext`.

8. `AnalyzeFirstCheck` must:

   * create one diagnostic using `FirstRule`;
   * use `Location.None`;
   * report that diagnostic.

9. `AnalyzeSecondCheck` must:

   * create one diagnostic using `SecondRule`;
   * use `Location.None`;
   * report that diagnostic.

10. `Initialize` must:

* configure generated-code analysis with `GeneratedCodeAnalysisFlags.None`;
* call `RegisterAnalysisActions`;
* enable concurrent execution.

11. Exactly two diagnostics must be reported for a compilation: one from each callback.

12. Do not inspect the compilation, syntax, symbols, semantic information, or operations.

*/