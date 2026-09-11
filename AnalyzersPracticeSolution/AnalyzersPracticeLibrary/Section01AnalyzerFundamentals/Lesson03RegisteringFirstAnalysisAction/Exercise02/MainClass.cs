/*
Enter the requirements for this exercise here.
Create an analyzer that registers one symbol analysis action for two kinds of symbols.

1. Keep the existing DiagnosticAnalyzer attribute.

2. SupportedDiagnostics must continue to return an empty ImmutableArray.

3. Keep ConfigureGeneratedCodeAnalysis with GeneratedCodeAnalysisFlags.None.

4. Keep concurrent execution enabled.

5. Create a private static method named RegisterAnalysisActions.

6. RegisterAnalysisActions must accept an AnalysisContext parameter.

7. Create a private static method named AnalyzeSymbol.

8. AnalyzeSymbol must accept a SymbolAnalysisContext parameter.

9. Register AnalyzeSymbol by using RegisterSymbolAction.

10. The registered action must run for both:
    - SymbolKind.NamedType
    - SymbolKind.Method

11. Register the action with a single RegisterSymbolAction call.

12. Do not call AnalyzeSymbol directly.

13. AnalyzeSymbol does not need to inspect the symbol or report a diagnostic yet.
    Its body may remain empty.

14. Initialize must call RegisterAnalysisActions to perform the registration.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson03RegisteringFirstAnalysisAction.Exercise02;

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
        SymbolKind.NamedType, SymbolKind.Method);
        
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