/*
Lesson 06 — Property-Like and Field Declarations
Exercise 01 — Identify Multiple Fields in One Declaration

Goal:
Practice analyzing FieldDeclarationSyntax and the individual variables
declared within it.

Requirements:

1. Register a syntax-node action for SyntaxKind.FieldDeclaration.

2. Report diagnostics only when a field declaration contains two or more
   variable declarators.

3. For each variable in a qualifying declaration, report one diagnostic.

4. Each diagnostic must:
   - Use diagnostic ID FIELD001.
   - Have Warning severity.
   - Highlight only that variable's identifier token.
   - Use this exact message:
     Field '{name}' is declared together with other fields

5. Use the identifier's ValueText for the field name in the message.
   For example, @class must appear as class in the message.

6. Analyze all field declarations regardless of accessibility, modifiers,
   declared type, initializer, or containing type.
   Constant fields are included.

7. Do not report diagnostics for:
   - Field declarations containing only one variable.
   - Local variable declarations.
   - Properties.
   - Events, including field-like events.

8. Use syntax only. Do not use semantic information.

9. Use the supplied FieldDiagnosticData and CreateDiagnostic method.
   Implement the registration and analysis yourself.

Examples:

    private int first, second;
    // Two diagnostics: first and second.

    private int first = 1, second = 2, third;
    // Three diagnostics: first, second, and third.

    private const int First = 1, Second = 2;
    // Two diagnostics: First and Second.

    private int single;
    // No diagnostic.

    void Example()
    {
        int first, second;
        // No diagnostic: these are local variables.
    }

    public event System.Action First, Second;
    // No diagnostic: this is an event declaration.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson06PropertyLikeAndFieldDeclarations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "FIELD001",
        title: "Declare fields separately",
        messageFormat: "Field '{0}' is declared together with other fields",
        category: "Style",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private sealed class FieldDiagnosticData
    {
        public string FieldName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        FieldDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.FieldName);
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
            FieldDeclarationSyntax node = context.ToFieldSyntax;
            //string name = node.Name;

            List<VariableDeclaratorSyntax> list = node.GetVariables;
            if (list.Count < 2)
            {
                return;
            }
            foreach (var item in list)
            {
                FieldDiagnosticData data = new()
                {
                    FieldName = item.Identifier.ValueText,

                };
                var location = item.Identifier.GetLocation();
                Diagnostic d = CreateDiagnostic(_rule, location, data);
                context.ReportDiagnostic(d);
            }

            
        }, SyntaxKind.FieldDeclaration);
        context.EnableConcurrentExecution();

        // Register your analysis action here.
    }

    // Implement your analysis here.
}