/*
Exercise 01 — Explicit Public Static Declarations

Goal:
Practice inspecting modifiers shared by different declaration kinds.

Declarations to analyze:
- ClassDeclarationSyntax
- MethodDeclarationSyntax
- PropertyDeclarationSyntax

Report one diagnostic for each analyzed declaration whose own modifier
list explicitly contains BOTH public and static.

Rules:
1. Modifier order does not matter.
2. Additional modifiers do not prevent reporting.
3. Inspect only the declaration's own modifiers.
   Do not infer modifiers from its containing type.
4. Analyze nested declarations too.
5. Ignore every other declaration kind, including structs, records,
   interfaces, fields, constructors, events, and local functions.
6. Treat each declaration separately. Do not combine partial declarations.
7. Use syntax only. No semantic model or symbols are needed.

Required diagnostic data:
- DeclarationKind: exactly "class", "method", or "property".
- DeclarationName: the identifier's ValueText, without generic parameters.

Diagnostic location:
- Highlight ONLY the declaration's identifier.
- Do not highlight public, static, the return type, or the whole declaration.

Diagnostic:
- ID: MOD001
- Severity: Warning
- Message:
  Public static {DeclarationKind} '{DeclarationName}' is declared

Implementation requirement:
Inspect the Modifiers collection. You may put that inspection in a
reusable helper or extension and use your existing naming/location helpers.

Examples:
public static class Utilities { }
    Reports on Utilities.

public static void Run() { }
    Reports on Run.

public static int Count { get; }
    Reports on Count.

public class Regular { }
    No diagnostic: static is absent.

internal static class InternalUtilities { }
    No diagnostic: public is absent.

A public instance method inside a static class does not qualify merely
because its containing class is static. Only its own modifiers matter.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson10CommonDeclarationComponentsAndModifiers.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "MOD001",
        title: "Public static declaration",
        messageFormat: "Public static {0} '{1}' is declared",
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
            if (node.IsPublic == false)
            {
                return;
            }
            if (node.IsStatic == false)
            {
                return;
            }
            string name = node.Name;
            string kind = node.KindName;
            DeclarationDiagnosticData data = new()
            {
                DeclarationKind = kind,
                DeclarationName = name
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
            data.DeclarationName);
    }
}

public class DeclarationDiagnosticData
{
    public string DeclarationKind { get; set; } = "";
    public string DeclarationName { get; set; } = "";
}