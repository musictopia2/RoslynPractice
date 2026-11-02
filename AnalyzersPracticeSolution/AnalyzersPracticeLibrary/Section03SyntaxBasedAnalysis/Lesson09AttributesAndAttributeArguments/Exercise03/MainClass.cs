/*
Exercise 03 — Explicit Attribute Targets

Goal:
Inspect attribute lists that explicitly specify a target.

Requirements:

1. Register a syntax-node action for SyntaxKind.AttributeList.
   Work with AttributeListSyntax.

2. Report exactly one diagnostic for each attribute list that has
   an explicit target specifier.

   Examples of explicit targets:
   [assembly: Example]
   [return: Example]
   [property: Example]

3. Ignore attribute lists without an explicit target.

   [Example]
   produces no diagnostic.

   Do not infer a target from the containing declaration.

4. Collect:
   - TargetName: the target identifier's ValueText.
   - AttributeCount: the number of attributes in that individual list.

   Count attributes, not their arguments.

5. A list containing several attributes produces ONE diagnostic.

   [assembly: First, Second(1, 2)]
   TargetName: assembly
   AttributeCount: 2

6. Separate attribute lists are analyzed independently.
   Do not combine lists attached to the same declaration.

7. Include every explicitly written target.
   Do not filter by target name, attribute name, or declaration kind.

8. Diagnostic location:
   Highlight only the TARGET IDENTIFIER.
   Exclude brackets, the colon, attributes, and surrounding trivia.

   [assembly: Example] -> highlight assembly
   [return: Example]   -> highlight return

9. Diagnostic contract:
   ID: ATTRTARGET001
   Title: Explicit attribute target
   Severity: Warning
   Category: Syntax
   Message:
   Attribute target '{0}' applies to {1} attribute(s)

10. Use syntax only.
    Do not resolve attribute types or validate whether an attribute
    is permitted on the specified target.
    Malformed attribute syntax is outside this exercise's scope.

Examples:

   [assembly: First, Second(1, 2)]
   produces:
   Attribute target 'assembly' applies to 2 attribute(s)

   [return: First]
   [return: Second]
   produces two diagnostics:
   Attribute target 'return' applies to 1 attribute(s)
   Attribute target 'return' applies to 1 attribute(s)

   [First, Second]
   produces no diagnostic because no target is explicitly written.

API orientation:
AttributeListSyntax.Attributes contains the attributes in that list.
AttributeListSyntax.Target is null when no target is written.
When present, Target.Identifier is the target's identifier token.

Your work:
Register the action, identify explicitly targeted lists, obtain the
target text and attribute count, locate the target identifier, and
report the diagnostic.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise03;

public class AttributeTargetDiagnosticData
{
    public string TargetName { get; set; } = "";
    public int AttributeCount { get; set; }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "ATTRTARGET001",
        title: "Explicit attribute target",
        messageFormat: "Attribute target '{0}' applies to {1} attribute(s)",
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
        AttributeTargetDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.TargetName,
            data.AttributeCount);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            AttributeListSyntax node = context.ToAttributeListSyntax;
            string? targetName = node.TargetName;
            if (targetName is null)
            {
                return;
            }
            int count = node.AttributeCount;
            AttributeTargetDiagnosticData data = new()
            {
                AttributeCount = count,
                TargetName = targetName
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.AttributeList);
        context.EnableConcurrentExecution();
    }
}