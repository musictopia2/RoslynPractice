/*
Lesson 06 — Property-Like and Field Declarations
Exercise 02 — Identify Property Initializers

Goal:
Practice analyzing PropertyDeclarationSyntax and distinguishing a property
initializer from other expressions associated with a property.

Requirements:

1. Register a syntax-node action for SyntaxKind.PropertyDeclaration.

2. Report one diagnostic for every property declaration that has an
   explicit property initializer.

3. Each diagnostic must:
   - Use diagnostic ID PROPINIT001.
   - Have Warning severity.
   - Highlight only the property's identifier token.
   - Use this exact message:
     Property '{name}' has an explicit initializer

4. Use the identifier's ValueText for the property name.

5. Report qualifying properties regardless of:
   - Accessibility.
   - Modifiers, including static.
   - Declared type.
   - Accessor combination.
   - Initializer expression or value.
   - Containing type.

6. Do not report diagnostics for:
   - Properties without an initializer.
   - Expression-bodied properties.
   - Properties whose getter returns an expression or value,
     but which have no property initializer.
   - Fields, events, indexers, or local variables.

7. Analyze properties in nested types as well.

8. Use syntax only. Do not use semantic information or evaluate
   initializer expressions.

9. Use the supplied PropertyDiagnosticData and CreateDiagnostic method.
   Implement the registration and analysis yourself.

Examples:

    public int Count { get; set; } = 10;
    // One diagnostic on Count.

    public string Name { get; } = "Andy";
    // One diagnostic on Name.

    public int Limit { get; init; } = 20;
    // One diagnostic on Limit.

    public static int Maximum { get; set; } = CalculateMaximum();
    // One diagnostic on Maximum.
    // The initializer does not have to be a literal.

    public int DefaultCount { get; set; } = default;
    // One diagnostic on DefaultCount.

    public int Count { get; set; }
    // No diagnostic.

    public int Count => 10;
    // No diagnostic.

    public int Count
    {
        get { return 10; }
    }
    // No diagnostic.

    private int count = 10;
    // No diagnostic.
*/

using System.Data;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson06PropertyLikeAndFieldDeclarations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "PROPINIT001",
        title: "Property has an explicit initializer",
        messageFormat: "Property '{0}' has an explicit initializer",
        category: "Style",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private sealed class PropertyDiagnosticData
    {
        public string PropertyName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        PropertyDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.PropertyName);
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
            PropertyDeclarationSyntax node = context.ToPropertySyntax;
            //string name = node.Name;


            if (node.HasInitializer == false)
            {
                return;
            }
            
            PropertyDiagnosticData data = new()
            {
                PropertyName = node.Name
            };
            Diagnostic diagnostic = CreateDiagnostic(_rule, node.Identifier.GetLocation(), data);
            context.ReportDiagnostic(diagnostic);


        }, SyntaxKind.PropertyDeclaration);
        context.EnableConcurrentExecution();

        // Register your analysis action here.
    }

    // Implement your analysis here.
}