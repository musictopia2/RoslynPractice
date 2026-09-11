/*
Enter the requirements for this exercise here.
Complete MainClass so that the analyzer registers a compilation analysis action.

Create a private method named:

AnalyzeCompilation

The method must use the appropriate Roslyn context type for a compilation analysis callback.

In Initialize:

Keep generated-code analysis configured as GeneratedCodeAnalysisFlags.None.
Keep concurrent execution enabled.
Register AnalyzeCompilation as a compilation analysis action.

For this exercise, AnalyzeCompilation does not need to report a diagnostic. Its purpose is to review the registration/callback relationship you practiced earlier.

Keep:

SupportedDiagnostics

returning an empty ImmutableArray<DiagnosticDescriptor>.

Completion behavior

After your changes, Roslyn should have a compilation callback registered, but the analyzer should still report zero diagnostics.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson05Review.Exercise01;

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
        AnalyzeCompilation(context);
        context.EnableConcurrentExecution();
    }
    private static void AnalyzeSample(CompilationAnalysisContext context)
    {
       
    }
    private void AnalyzeCompilation(AnalysisContext context)
    {
        context.RegisterCompilationAction(AnalyzeSample);
    }
}