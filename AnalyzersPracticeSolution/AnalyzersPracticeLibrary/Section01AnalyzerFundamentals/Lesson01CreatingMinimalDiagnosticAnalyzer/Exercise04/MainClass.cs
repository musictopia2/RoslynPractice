/*
Enter the requirements for this exercise here.

*/


namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise04;

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
    public override void Initialize(AnalysisContext context)
    {
        GeneratedCodeAnalysisFlags flags = GetGeneratedCodeOptions();
        context.ConfigureGeneratedCodeAnalysis(
            flags);
        context.EnableConcurrentExecution();
    }

    public static GeneratedCodeAnalysisFlags GetGeneratedCodeOptions()
    {
        return GeneratedCodeAnalysisFlags.ReportDiagnostics.CombineFlags(GeneratedCodeAnalysisFlags.Analyze);
    }

}