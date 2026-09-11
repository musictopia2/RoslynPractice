/*
Enter the requirements for this exercise here.

*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson01DiagnosticIdsAndRuleIdentity.Exercise03;

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
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();
    }
}