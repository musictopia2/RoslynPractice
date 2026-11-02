/*
Exercise 05 — Client Requirement:
Inconsistent Repeated Attribute Configuration

Client request:

Our team sometimes repeats an attribute on a public method.
We want a syntax-only review diagnostic when occurrences of the
same written attribute name mix these two styles:

- Occurrences with at least one named argument.
- Occurrences with no named arguments.

This is a consistency check, not a claim that the C# is invalid.

Scope:

1. Analyze methods with an explicitly written public modifier.
   Include public static, abstract, virtual, and generic methods.
   Ignore methods without an explicitly written public modifier.

2. Include only attribute lists directly attached to the method
   whose target is:
   - Absent.
   - Explicitly "method".

   Exclude return-value lists and every other explicit target.
   Exclude attributes on containing types, parameters, type parameters,
   and declarations inside the method body.

Grouping:

3. Within each method, group included attributes by Name.ToString().

   Compare those strings exactly and case-sensitively.
   Do not remove qualification, an @ prefix, or an Attribute suffix.
   Do not resolve aliases or attribute types.

   These are separate groups:
   Example
   ExampleAttribute
   Tools.Example

4. Combine matching occurrences across included lists attached to
   the SAME method.

   Do not combine different methods, overloads, or partial declarations.

Classifying each occurrence:

5. An attribute occurrence is "with named arguments" if at least one
   of its direct arguments has NameColon or NameEquals.

   Both named forms qualify:
   [Example(message: "hello")]
   [Example(Enabled = true)]

6. Otherwise, the occurrence is "without named arguments".

   All of these belong to that category:
   [Example]
   [Example()]
   [Example(42)]

7. Count ATTRIBUTE OCCURRENCES in each category, not arguments.

   [Example(Enabled = true, Count = 3)]
   contributes ONE occurrence with named arguments.

Reporting:

8. Report exactly one diagnostic for each attribute-name group
   containing BOTH:
   - At least one occurrence with named arguments.
   - At least one occurrence without named arguments.

   Groups containing only one style produce no diagnostic.

9. A method may produce multiple diagnostics when multiple groups
   meet that condition.

10. Diagnostic location:
    Highlight the entire attribute NAME of the FIRST included
    occurrence in the reported group, in source order.

    Exclude brackets, target specifiers, parentheses, and arguments.
    The first occurrence may belong to either configuration category.

11. Diagnostic data:
    MethodName:
        Method identifier's ValueText.
    AttributeName:
        The group's exact Name.ToString() text.
    WithNamedCount:
        Number of occurrences with named arguments.
    WithoutNamedCount:
        Number of occurrences without named arguments.

12. Diagnostic contract:
    ID: ATTRMIX001
    Title: Inconsistent repeated attribute configuration
    Severity: Warning
    Category: Syntax
    Message:
    Public method '{0}' mixes configuration for attribute '{1}': {2} occurrence(s) with named arguments, {3} without

13. Use syntax only.
    Attribute definitions and semantic validity are irrelevant.
    Malformed declaration or attribute syntax is outside this exercise.

Examples:

   [Example]
   [Example(Enabled = true, Count = 3)]
   public void Run() { }

   One diagnostic, highlighting the FIRST Example:
   Public method 'Run' mixes configuration for attribute 'Example': 1 occurrence(s) with named arguments, 1 without


   [Example(42), Example(), Example(message: "hello")]
   public void Run() { }

   One diagnostic, highlighting the FIRST Example:
   Public method 'Run' mixes configuration for attribute 'Example': 1 occurrence(s) with named arguments, 2 without


   [Example(Enabled = true)]
   [method: Example]
   [return: Example]
   public int Read() => 1;

   One diagnostic, highlighting the FIRST Example:
   Public method 'Read' mixes configuration for attribute 'Example': 1 occurrence(s) with named arguments, 1 without

   The return-value occurrence contributes nothing.


   [Example, Example(42)]
   public void Run() { }

   No diagnostic: both occurrences lack named arguments.


   [Example(Enabled = true), Example(message: "hello")]
   public void Run() { }

   No diagnostic: both occurrences have named arguments.


   [Example]
   [ExampleAttribute(Enabled = true)]
   public void Run() { }

   No diagnostic: these are different written-name groups.

Implementation choices:

Choose the analysis registration and supporting helpers yourself.
You may reuse previous helpers and ordinary C# grouping techniques.
No new semantic-analysis or parent-navigation API is required.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise04;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise05;

public class AttributeConfigurationDiagnosticData
{
    public string MethodName { get; set; } = "";
    public string AttributeName { get; set; } = "";
    public int WithNamedCount { get; set; }
    public int WithoutNamedCount { get; set; }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "ATTRMIX001",
        title: "Inconsistent repeated attribute configuration",
        messageFormat:
            "Public method '{0}' mixes configuration for attribute '{1}': " +
            "{2} occurrence(s) with named arguments, {3} without",
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
        AttributeConfigurationDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.MethodName,
            data.AttributeName,
            data.WithNamedCount,
            data.WithoutNamedCount);
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            MethodDeclarationSyntax node = context.ToMethodSyntax;
            if (node.IsPublic == false)
            {
                return;
            }
            string name = node.Name;
            int attributes = node.AttributeCount;
            List<AttributeSyntax> list = [];

            foreach (var item in node.AttributeLists)
            {
                if (item.TargetName is null || item.TargetName == "method")
                {
                    foreach (var seconds in item.Attributes)
                    {
                        list.Add(seconds);
                    }
                }
                
            }


            var firsts = node.AttributeLists.ToList();
            var seconds1 = list.GroupBy(x => x.Name.ToString());
            foreach (var item in seconds1)
            {


                int names = 0;

                int nones = 0;
                foreach (var fins in item)
                {
                    bool hasNamedArguments = fins.HasNamedArguments;
                    if (hasNamedArguments)
                    {
                        names++;
                    }
                    else
                    {
                        nones++;
                    }
                    
                    
                    
                }


                if (names > 0 && nones > 0)
                {
                    AttributeConfigurationDiagnosticData data = new()
                    {
                        MethodName = node.Name,
                        AttributeName = item.Key,
                        WithNamedCount = names,
                        WithoutNamedCount = nones
                    };
                    Diagnostic d = CreateDiagnostic(_rule, item.First().GetIdentifierLocation, data);
                    context.ReportDiagnostic(d);
                }

            }            
        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();

    }
}