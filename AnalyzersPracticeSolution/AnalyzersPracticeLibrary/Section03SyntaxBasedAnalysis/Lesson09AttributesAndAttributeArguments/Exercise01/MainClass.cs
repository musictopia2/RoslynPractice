/*
Exercise 01 — Attribute Argument Counts

Goal:
Practice inspecting individual attributes using syntax analysis.

Requirements:

1. Register a syntax-node action for SyntaxKind.Attribute.

2. Report exactly one diagnostic for every attribute.
   Include attributes on any declaration or target, including assembly,
   return-value, and parameter attributes.

3. Collect the following information:
   - AttributeName: the attribute's Name.ToString().
     Preserve qualification and any explicit "Attribute" suffix.
     Do not resolve the attribute's type.
   - ArgumentCount: the number of arguments written directly inside
     that attribute's parentheses.

4. Attributes without parentheses have zero arguments.
   Attributes with empty parentheses also have zero arguments.

5. Count every argument, including:
   - Ordinary arguments: [Example("text")]
   - Constructor arguments with names: [Example(message: "text")]
   - Property or field assignments: [Example(Enabled = true)]

   Each entry counts as one argument.
   Do not count expressions nested inside an argument separately.

6. When an attribute list contains multiple attributes, report each
   attribute separately.

7. Diagnostic location:
   Report on the attribute's entire NAME only.
   Exclude brackets, parentheses, arguments, and target specifiers.

   Examples:
   [Example(1)]             -> highlight Example
   [Tools.Example(1)]       -> highlight Tools.Example
   [return: Example]        -> highlight Example

8. Diagnostic contract:
   ID: ATTRCOUNT001
   Title: Attribute argument count
   Severity: Warning
   Category: Syntax
   Message:
   Attribute '{0}' has {1} argument(s)

9. Use syntax only. Do not use semantic analysis.
   No filtering by attribute name or containing declaration is required.
   Malformed attribute syntax is outside this exercise's scope.

Examples:

   Attribute source                    Name              Count
   [Example]                           Example           0
   [Example()]                         Example           0
   [Example("hello")]                  Example           1
   [Example("hello", Enabled = true)]  Example           2
   [Tools.ExampleAttribute(10)]         Tools.ExampleAttribute  1

   [First, Second(1)]
   produces two diagnostics:
   Attribute 'First' has 0 argument(s)
   Attribute 'Second' has 1 argument(s)

Your work:
Register the analysis action, inspect each attribute, populate the
provided data model, obtain the required location, and report the
diagnostic using CreateDiagnostic.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise01;

public class AttributeDiagnosticData
{
    public string AttributeName { get; set; } = "";
    public int ArgumentCount { get; set; }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "ATTRCOUNT001",
        title: "Attribute argument count",
        messageFormat: "Attribute '{0}' has {1} argument(s)",
        category: "Syntax",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [_rule];
        }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        AttributeDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.AttributeName,
            data.ArgumentCount);
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            AttributeSyntax node = context.ToAttributeSyntax;
            string name = node.Name.ToString();
            int count = node.ArgumentCount;
            AttributeDiagnosticData data = new()
            {
                AttributeName = name,
                ArgumentCount = count
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.Attribute);
        context.EnableConcurrentExecution();
    }
}