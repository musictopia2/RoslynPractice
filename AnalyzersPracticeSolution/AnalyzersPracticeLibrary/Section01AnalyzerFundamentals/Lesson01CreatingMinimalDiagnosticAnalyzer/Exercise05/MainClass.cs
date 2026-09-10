/*
Enter the requirements for this exercise here.

*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise05;

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
        GeneratedCodeAnalysisFlags flags = GetGeneratedCodeOptions(false);
        context.ConfigureGeneratedCodeAnalysis(
            flags);
        context.EnableConcurrentExecution();
        context.EnableConcurrentExecution();
    }
    public static GeneratedCodeAnalysisFlags GetGeneratedCodeOptions(
    bool includeGeneratedCode)
    {
        if (includeGeneratedCode == false)
        {
            return GeneratedCodeAnalysisFlags.None;
        }
        return GeneratedCodeAnalysisFlags.Analyze.CombineFlags(GeneratedCodeAnalysisFlags.ReportDiagnostics);
    }
}