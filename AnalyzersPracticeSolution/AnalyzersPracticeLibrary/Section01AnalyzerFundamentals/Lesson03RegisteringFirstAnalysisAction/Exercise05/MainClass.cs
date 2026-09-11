/*
Enter the requirements for this exercise here.
A development team is preparing an analyzer that will eventually perform
different checks on types, their members, and source files.

For now, your job is only to configure and register the analysis callbacks.
The actual analysis behavior will be implemented in later lessons.

Requirements:

1. Keep the existing DiagnosticAnalyzer attribute.

2. SupportedDiagnostics must return an empty ImmutableArray.

3. Generated code must not be analyzed.

4. Concurrent execution must be enabled.

5. Keep all analysis-action registrations outside Initialize in a private
   static method named RegisterAnalysisActions.

6. The analyzer needs one callback for analyzing type declarations.
   Name this callback AnalyzeType.
   It must use the appropriate context type for a symbol action.

7. AnalyzeType must be registered only for SymbolKind.NamedType.

8. The analyzer needs a separate callback for analyzing members.
   Name this callback AnalyzeMember.
   It must use the appropriate context type for a symbol action.

9. AnalyzeMember must handle all three of these symbol kinds using a single
   registration:
   - SymbolKind.Method
   - SymbolKind.Property
   - SymbolKind.Field

10. The analyzer also needs a callback named AnalyzeSourceFile for analysis
    associated with syntax trees.
    It must use the appropriate syntax-tree analysis context.

11. Register AnalyzeSourceFile using the syntax-tree registration mechanism
    learned earlier in this lesson.

12. The three analysis callbacks may have empty bodies.

13. Do not call AnalyzeType, AnalyzeMember, or AnalyzeSourceFile directly.

14. Initialize must configure the analyzer and cause all required analysis
    actions to be registered.

15. Register exactly:
    - two symbol actions
    - one syntax-tree action
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson03RegisteringFirstAnalysisAction.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static void AnalyzeSourceFile(
    SyntaxTreeAnalysisContext context)
    {
        //a delegate so eventually code would be filled out.
    }

    private static void RegisterAnalysisActions(
        AnalysisContext context)
    {
        context.RegisterSymbolAction(
        AnalyzeMember,
        SymbolKind.Method, SymbolKind.Field, SymbolKind.Property);
        context.RegisterSymbolAction(
        AnalyzeType,
        SymbolKind.NamedType);
        context.RegisterSyntaxTreeAction(
            AnalyzeSourceFile);
    }
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        //a delegate so eventually code would be filled out.
    }
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        
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