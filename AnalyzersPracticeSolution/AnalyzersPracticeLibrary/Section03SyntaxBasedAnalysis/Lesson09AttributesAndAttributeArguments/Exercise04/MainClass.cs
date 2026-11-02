/*
Exercise 04 — Public Method Attribute Summaries

Goal:
Summarize attributes and named attribute arguments directly applied
to explicitly public methods.

Requirements:

1. Register a syntax-node action for SyntaxKind.MethodDeclaration.
   Work with MethodDeclarationSyntax.

2. Analyze only methods with an explicitly written public modifier.

   Include public static, abstract, virtual, and generic methods.
   Ignore methods without an explicitly written public modifier,
   including interface methods whose accessibility is implicit.

3. Examine only attribute lists directly attached to that method.

   Include:
   - Lists without an explicit target: [Example]
   - Lists explicitly targeting the method: [method: Example]

   Exclude:
   - Return-value lists: [return: Example]
   - Lists with any other explicit target
   - Attributes on parameters or type parameters
   - Attributes on the containing type
   - Attributes on declarations inside the method body

4. Across all included lists for one method, calculate:

   AttributeCount:
   The total number of attributes.
   Count every occurrence, including repeated attribute names.

   NamedArgumentCount:
   The total number of named arguments across those attributes.
   Count both colon-form and equals-form arguments.
   Do not count ordinary unnamed arguments.

   Attributes without arguments still contribute to AttributeCount.

5. Report exactly ONE diagnostic for a method when BOTH are true:
   - AttributeCount is at least 2.
   - NamedArgumentCount is at least 1.

   Otherwise, report nothing for that method.

6. Collect MethodName from the method identifier's ValueText.
   Preserve its capitalization.
   Do not include an escaped identifier's @ prefix.

7. Analyze each method independently.
   Do not combine overloads or methods in different types.

8. Diagnostic location:
   Highlight only the METHOD IDENTIFIER.
   Exclude attributes, return type, type parameters, and parameters.

9. Diagnostic contract:
   ID: METHODATTR001
   Title: Public method attribute summary
   Severity: Warning
   Category: Syntax
   Message:
   Public method '{0}' has {1} attribute(s) with {2} named argument(s)

10. Use syntax only.
    Do not resolve attribute types or validate their permitted targets.
    Malformed declaration or attribute syntax is outside this exercise.

Examples:

   [First]
   [Second(Enabled = true)]
   public void Run() { }

   Reports on Run:
   Public method 'Run' has 2 attribute(s) with 1 named argument(s)


   [method: First(message: "hello"), Second(Enabled = true)]
   [return: Third(Other = true)]
   public int Read() => 1;

   Reports on Read:
   Public method 'Read' has 2 attribute(s) with 2 named argument(s)

   The return attribute contributes to neither count.


   [First(Enabled = true)]
   public void Single() { }

   No diagnostic: only one included attribute.


   [First, Second(42)]
   public void Unnamed() { }

   No diagnostic: no named arguments.


   [First, Second(Enabled = true)]
   private void Hidden() { }

   No diagnostic: the method is not explicitly public.

API orientation:
MethodDeclarationSyntax.AttributeLists contains the lists directly
attached to the method. Each included list has an Attributes collection.
Each attribute may have an ArgumentList containing Arguments.

You may reuse or extend your existing helpers.

Your work:
Register the action, filter methods and lists, calculate both counts,
apply the reporting conditions, and report at the method identifier.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments.Exercise04;

public class MethodAttributeDiagnosticData
{
    public string MethodName { get; set; } = "";
    public int AttributeCount { get; set; }
    public int NamedArgumentCount { get; set; }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "METHODATTR001",
        title: "Public method attribute summary",
        messageFormat:
            "Public method '{0}' has {1} attribute(s) with {2} named argument(s)",
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
        MethodAttributeDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.MethodName,
            data.AttributeCount,
            data.NamedArgumentCount);
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
            int namedArguments = node.NamedArgumentCount;
            if (namedArguments == 0 || attributes < 2)
            {
                return;
            }
            MethodAttributeDiagnosticData data = new()
            {
                MethodName = name,
                AttributeCount = attributes,
                NamedArgumentCount = namedArguments
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();

    }
}