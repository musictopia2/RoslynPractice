/*
Enter the requirements for this exercise here.
Create an analyzer that registers two different analysis callbacks.

1. Keep the existing DiagnosticAnalyzer attribute.

2. SupportedDiagnostics must return an empty ImmutableArray.

3. Configure generated-code analysis with GeneratedCodeAnalysisFlags.None.

4. Enable concurrent execution.

5. Create a private static method named RegisterAnalysisActions.
   It must accept an AnalysisContext parameter.

6. Create a private static method named AnalyzeSymbol.
   It must accept a SymbolAnalysisContext parameter.

7. Register AnalyzeSymbol with ONE RegisterSymbolAction call.

8. That single registration must listen for all three of these symbol kinds:
   - SymbolKind.NamedType
   - SymbolKind.Method
   - SymbolKind.Property

9. Create a private static method named AnalyzeSyntaxTree.
   It must accept a SyntaxTreeAnalysisContext parameter.

10. Register AnalyzeSyntaxTree using RegisterSyntaxTreeAction.

11. AnalyzeSymbol and AnalyzeSyntaxTree may have empty bodies.

12. Do not call either analysis callback directly.

13. Initialize must call RegisterAnalysisActions.

14. Register exactly one symbol action and exactly one syntax-tree action.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson03RegisteringFirstAnalysisAction.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{


    private static void AnalyzeSyntaxTree(
    SyntaxTreeAnalysisContext context)
    {
        //a delegate so eventually code would be filled out.
    }

    private static void RegisterAnalysisActions(
        AnalysisContext context)
    {
        context.RegisterSymbolAction(
        AnalyzeSymbol,
        SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property);
        context.RegisterSyntaxTreeAction(
            AnalyzeSyntaxTree);
    }
    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        //a delegate so eventually code would be filled out.
    }

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
        RegisterAnalysisActions(context);
        context.EnableConcurrentExecution();
    }
}