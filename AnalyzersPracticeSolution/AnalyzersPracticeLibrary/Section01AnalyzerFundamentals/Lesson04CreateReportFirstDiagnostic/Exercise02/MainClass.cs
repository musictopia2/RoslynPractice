/*
Enter the requirements for this exercise here.
Create and report a diagnostic from a compilation analysis callback.

Requirements:

Create a private static DiagnosticDescriptor named Rule.
Configure it with these exact values:
ID: APR0002
Title: Compilation check completed
Message: Compilation analysis completed successfully
Category: Practice
Severity: DiagnosticSeverity.Info
Enabled by default: true
Return Rule from SupportedDiagnostics.
Keep the existing analyzer initialization calls for:
ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)
EnableConcurrentExecution()

Register a compilation action that calls a private static method named:

AnalyzeCompilation

AnalyzeCompilation must receive a CompilationAnalysisContext.
Inside AnalyzeCompilation:
create one Diagnostic using Diagnostic.Create;
use the existing Rule;
use Location.None;
report the diagnostic through the provided context.
Report exactly one diagnostic each time the compilation callback runs.
Do not inspect the compilation, syntax, symbols, semantic information, or operations.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule { get; } =
        bb1.Create().WithId("APR0002")
        .WithTitle("Compilation check completed")
        .WithMessage("Compilation analysis completed successfully")
        .WithCategory("Practice")
        .WithSeverity(DiagnosticSeverity.Info)
        .EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
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
           Rule,
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