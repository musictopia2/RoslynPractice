/*
Enter the requirements for this exercise here.

*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise03;

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
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
    }
}