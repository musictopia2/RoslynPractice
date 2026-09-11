/*
Enter the requirements for this exercise here.
Create the analyzer's first registered analysis action.

1. Keep the existing DiagnosticAnalyzer attribute, SupportedDiagnostics property,
   generated-code configuration, and concurrent execution configuration.

2. Create a private static method named AnalyzeCompilation.

3. AnalyzeCompilation must accept one CompilationAnalysisContext parameter.

4. Do not call AnalyzeCompilation directly from Initialize.

5. In Initialize, register AnalyzeCompilation as a compilation action by using
   RegisterCompilationAction.

6. AnalyzeCompilation does not need to inspect the compilation or report any
   diagnostics yet. Its body may remain empty.

7. Do not register any other type of analysis action in this exercise.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson03RegisteringFirstAnalysisAction.Exercise01;

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
    private static void RegisterAnalysisActions(AnalysisContext context)
    {
        context.RegisterSymbolAction(
        AnalyzeSymbol,
        SymbolKind.NamedType);
    }
    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {

    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        RegisterAnalysisActions(context);
        context.EnableConcurrentExecution();
    }
}