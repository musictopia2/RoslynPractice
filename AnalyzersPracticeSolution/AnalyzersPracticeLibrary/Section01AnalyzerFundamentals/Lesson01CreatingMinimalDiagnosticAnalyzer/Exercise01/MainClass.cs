/*
Enter the requirements for this exercise here.

*/



namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [];
        }
    }
//    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
//    {
//        get
//        {
//            return [
//                GeneratedCodeAnalysisFlags.Analyze,
//GeneratedCodeAnalysisFlags.ReportDiagnostics
//                ];
//        }
//    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }

    public static GeneratedCodeAnalysisFlags GetGeneratedCodeOptions()
    {
        return GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics;
    }
    
}
