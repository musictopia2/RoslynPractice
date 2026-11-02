/*
Exercise 02 — Named Attribute Arguments

Goal:
Inspect individual attribute arguments and distinguish the two forms
of named argument syntax.

Requirements:

1. Register a syntax-node action for SyntaxKind.AttributeArgument.

2. Report exactly one diagnostic for each named attribute argument:

   - Colon form:
     [Example(message: "hello")]
     ArgumentName: message
     Form: colon

   - Equals form:
     [Example(Enabled = true)]
     ArgumentName: Enabled
     Form: equals

3. Do not report ordinary arguments without a name.

   [Example("hello", 42)]
   produces no diagnostics.

4. Obtain ArgumentName from the written name's ToString().
   Preserve escaped identifiers, including their @ prefix.
   Do not resolve symbols or evaluate argument expressions.

5. Diagnostic location:
   Highlight only the argument's NAME.
   Exclude the colon or equals sign and the argument expression.

   Examples:
   [Example(message: "hello")] -> highlight message
   [Example(Enabled = true)]   -> highlight Enabled

6. Analyze named arguments in every attribute, regardless of the
   attribute name, containing declaration, or attribute target.

7. Handle multiple named arguments, multiple attributes in one list,
   and attributes in separate lists independently.

8. Attributes without arguments produce no diagnostics.

9. Diagnostic contract:
   ID: ATTRNAMED001
   Title: Named attribute argument
   Severity: Warning
   Category: Syntax
   Message:
   Attribute argument '{0}' uses {1} syntax

   Form must be exactly "colon" or "equals", in lowercase.

10. Use syntax only.
    Malformed attribute syntax is outside this exercise's scope.

Examples:

   [Example("hello", Enabled = true)]
   produces:
   Attribute argument 'Enabled' uses equals syntax

   [Example(message: "hello", Enabled = true)]
   produces:
   Attribute argument 'message' uses colon syntax
   Attribute argument 'Enabled' uses equals syntax

   [First, Second(Count = 3, Text = "hello")]
   produces:
   Attribute argument 'Count' uses equals syntax
   Attribute argument 'Text' uses equals syntax

API orientation:
AttributeArgumentSyntax has NameColon and NameEquals properties.
These identify the two named forms. Their Name properties provide
the written argument name.

Your work:
Register the action, identify named arguments, extract their names
and forms, obtain their locations, and report the diagnostics.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise02;

public class NamedArgumentDiagnosticData
{
    public string ArgumentName { get; set; } = "";
    public string Form { get; set; } = "";
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "ATTRNAMED001",
        title: "Named attribute argument",
        messageFormat: "Attribute argument '{0}' uses {1} syntax",
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
        NamedArgumentDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ArgumentName,
            data.Form);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            AttributeArgumentSyntax node = context.ToAttributeArgumentSyntax;
            string? name = node.Name;
            if (name is null)
            {
                return;
            }
            string form = node.Form!;

            NamedArgumentDiagnosticData data = new()
            {
                ArgumentName = name,
                Form = form
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.AttributeArgument);
        context.EnableConcurrentExecution();

    }
}