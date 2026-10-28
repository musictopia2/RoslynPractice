/*
Lesson 06 — Property-Like and Field Declarations
Exercise 03 — Identify Indexers with Multiple Parameters

Goal:
Practice analyzing IndexerDeclarationSyntax, including its parameter list
and the token used to identify an indexer in source.

Requirements:

1. Register a syntax-node action for SyntaxKind.IndexerDeclaration.

2. Report one diagnostic for each indexer that declares two or more
   parameters.

3. Each diagnostic must:
   - Use diagnostic ID INDEXER001.
   - Have Warning severity.
   - Highlight only the indexer's "this" keyword.
   - Use this exact message:
     Indexer declares {count} parameters

4. The count must be the number of parameters declared by the indexer.

5. Analyze indexers regardless of:
   - Accessibility or modifiers.
   - Return type.
   - Parameter names or types.
   - Accessor combination.
   - Whether the indexer uses accessors or an expression body.
   - Containing type, including nested types.

6. Do not report diagnostics for:
   - Indexers with only one parameter.
   - Properties.
   - Methods, regardless of their parameter count.
   - Fields or events.

7. Count declared parameters, not arguments or items inside a parameter's
   type. A single parameter with a generic type is still one parameter.

8. Use syntax only. Do not use semantic information.

9. Use the supplied IndexerDiagnosticData and CreateDiagnostic method.
   Implement the registration and analysis yourself.

Examples:

    public int this[int row, int column] => 0;
    // One diagnostic on "this".
    // Message: Indexer declares 2 parameters

    public int this[int x, int y, int z]
    {
        get { return 0; }
        set { }
    }
    // One diagnostic on "this".
    // Message: Indexer declares 3 parameters

    public int this[int index] => 0;
    // No diagnostic.

    public int this[System.Collections.Generic.Dictionary<int, string> map]
        => 0;
    // No diagnostic: only one parameter.

    public int Calculate(int row, int column) => 0;
    // No diagnostic: this is a method.
*/

using System.Data;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson06PropertyLikeAndFieldDeclarations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "INDEXER001",
        title: "Indexer declares multiple parameters",
        messageFormat: "Indexer declares {0} parameters",
        category: "Style",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private sealed class IndexerDiagnosticData
    {
        public int ParameterCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        IndexerDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ParameterCount);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [_rule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            IndexerDeclarationSyntax node = context.ToIndexSyntax;
            IndexerDiagnosticData data = new()
            {
                ParameterCount = node.ParameterCount
            };
            if (data.ParameterCount < 2)
            {
                return;
            }
            Diagnostic diagnostic = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(diagnostic);
        }, SyntaxKind.IndexerDeclaration);
        context.EnableConcurrentExecution();
    }
}