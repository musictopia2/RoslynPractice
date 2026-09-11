/*
Enter the requirements for this exercise here.
this time, needed so much help ai generated most of this one.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule { get; } = new(
        id: "APR0001",
        title: "Compilation analyzed",
        messageFormat: "The compilation was analyzed",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    public override void Initialize(
    AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        RegisterAnalysisActions(context);

        context.EnableConcurrentExecution();
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

}