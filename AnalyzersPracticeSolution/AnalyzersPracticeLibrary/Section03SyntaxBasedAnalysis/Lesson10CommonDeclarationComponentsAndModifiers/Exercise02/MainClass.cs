/*
Exercise 02 — Declaration Modifier Counts

Goal:
Practice examining the complete Modifiers collection rather than
checking only whether particular modifiers are present.

Declarations to analyze:
- ClassDeclarationSyntax
- MethodDeclarationSyntax
- PropertyDeclarationSyntax

Reporting rule:
Report one diagnostic for each analyzed declaration whose own Modifiers
collection contains THREE OR MORE modifier tokens.

Rules:
1. Count every modifier token in that declaration's Modifiers collection.
2. No particular modifier is required.
   Public declarations and nonpublic declarations are both eligible.
3. Modifier order does not affect eligibility.
4. Count combined accessibility as separate tokens:
   - protected internal contributes 2.
   - private protected contributes 2.
5. Do not count attributes, return types, identifiers, type parameters,
   constraints, or modifiers belonging to other declarations.
6. Analyze nested declarations independently.
7. Treat each partial declaration separately.
8. Ignore all declaration kinds other than classes, methods, and properties.
   Qualifying methods and properties inside structs, records, or interfaces
   are still included.
9. Use syntax only. Do not validate whether modifier combinations are legal.
10. You may use your existing helpers and extensions.

Required diagnostic data:
- DeclarationKind: exactly "class", "method", or "property".
- DeclarationName: the identifier's ValueText, without generic parameters.
- ModifierCount: the number of tokens in that declaration's Modifiers.

Diagnostic location:
Highlight ONLY the declaration's identifier.

Diagnostic:
- ID: MODCOUNT001
- Severity: Warning
- Message:
  The {DeclarationKind} '{DeclarationName}' has {ModifierCount} explicit modifier(s)

Examples:

public static class Utilities { }
    No diagnostic: 2 modifiers.

public static partial class Utilities { }
    Reports on Utilities: 3 modifiers.

protected internal static void Run() { }
    Reports on Run: 3 modifiers.

private protected static int Count { get; }
    Reports on Count: 3 modifiers.

[Obsolete]
public static void Run() { }
    No diagnostic: the attribute does not count as a modifier.

A property with a private setter:
public static int Count { get; private set; }
    No diagnostic: the property's own modifier count is 2.
    The setter's private modifier does not belong to the property.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson10CommonDeclarationComponentsAndModifiers.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "MODCOUNT001",
        title: "Declaration modifier count",
        messageFormat:
            "The {0} '{1}' has {2} explicit modifier(s)",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

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
            MemberDeclarationSyntax node = context.ToMemberSyntax;
            
            string name = node.Name;
            string kind = node.KindName;
            int count = node.Modifiers.Count();
            if (count < 3)
            {
                return;
            }
            DeclarationDiagnosticData data = new()
            {
                DeclarationKind = kind,
                DeclarationName = name,
                ModifierCount = count
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.ClassDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration);

        context.EnableConcurrentExecution();

    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        DeclarationDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.DeclarationKind,
            data.DeclarationName,
            data.ModifierCount);
    }
}

public class DeclarationDiagnosticData
{
    public string DeclarationKind { get; set; } = "";
    public string DeclarationName { get; set; } = "";
    public int ModifierCount { get; set; }
}