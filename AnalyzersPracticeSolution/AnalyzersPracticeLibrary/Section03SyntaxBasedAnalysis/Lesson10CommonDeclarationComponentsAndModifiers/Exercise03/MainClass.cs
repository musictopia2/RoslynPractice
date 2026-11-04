/*
Exercise 03 — Explicit Accessibility

Goal:
Identify explicitly written accessibility, including combinations
represented by two modifier tokens.

Declarations to analyze:
- ClassDeclarationSyntax
- MethodDeclarationSyntax
- PropertyDeclarationSyntax

Reporting rule:
Report one diagnostic for each analyzed declaration that has explicitly
written accessibility modifiers.

Required Accessibility values:
Written modifiers       Accessibility value
public                  "public"
private                 "private"
protected               "protected"
internal                "internal"
protected internal      "protected internal"
private protected       "private protected"

Rules:
1. Recognize combined accessibility as ONE accessibility value.
   Report only one diagnostic for that declaration.
2. Modifier order does not matter. For example, internal protected
   must produce the value "protected internal".
3. Other modifiers, such as static, abstract, or partial, do not affect
   the accessibility value or prevent reporting.
4. If no accessibility modifier is written, report nothing.
   Do not infer default accessibility.
5. Inspect only the declaration's own modifiers.
6. Analyze nested declarations independently.
7. Treat each partial declaration separately.
8. Ignore declaration kinds other than classes, methods, and properties.
   Methods and properties inside structs, records, and interfaces
   are still included.
9. Use syntax only. Do not determine effective accessibility from
   containing types.
10. Inputs will not contain duplicate or conflicting accessibility
    modifiers. You do not need to validate modifier legality.
11. Your existing helpers and extensions are allowed.

Required diagnostic data:
- DeclarationKind: exactly "class", "method", or "property".
- DeclarationName: identifier ValueText, without generic parameters.
- Accessibility: exactly one of the six values listed above.

Diagnostic location:
Highlight ONLY the declaration's identifier.

Diagnostic:
- ID: ACCESS001
- Severity: Warning
- Message:
  The {DeclarationKind} '{DeclarationName}' explicitly declares '{Accessibility}' accessibility

Examples:

public class Worker { }
    Reports on Worker with accessibility "public".

protected internal static void Run() { }
    Reports on Run with accessibility "protected internal".

private protected int Count { get; }
    Reports on Count with accessibility "private protected".

class Worker { }
    No diagnostic: accessibility is not explicitly written.

interface IWorker
{
    void Implicit();
    public void Explicit();
}
    No diagnostic on IWorker or Implicit.
    Reports on Explicit with accessibility "public".

public int Count { get; private set; }
    Reports once on Count with accessibility "public".
    The setter's accessibility is not the property's accessibility.
*/

using System.Data;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson10CommonDeclarationComponentsAndModifiers.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "ACCESS001",
        title: "Explicit declaration accessibility",
        messageFormat:
            "The {0} '{1}' explicitly declares '{2}' accessibility",
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
            string accessibility = node.ExplicitAccessibilityText;
            if (accessibility == "")
            {
                return;
            }
            DeclarationDiagnosticData data = new()
            {
                DeclarationKind = kind,
                DeclarationName = name,
                Accessibility = accessibility
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
            data.Accessibility);
    }
}

public class DeclarationDiagnosticData
{
    public string DeclarationKind { get; set; } = "";
    public string DeclarationName { get; set; } = "";
    public string Accessibility { get; set; } = "";
}