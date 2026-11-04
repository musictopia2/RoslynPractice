/*
Exercise 04 — Mixed Public Static and Instance Members

Goal:
Summarize explicitly public methods and properties according to whether
their own modifiers contain static.

Declaration to analyze:
- ClassDeclarationSyntax

Members to examine:
Only methods and properties declared DIRECTLY inside that class.

Eligible members:
A MethodDeclarationSyntax or PropertyDeclarationSyntax whose own
Modifiers collection explicitly contains public.

For each class, calculate:
- Public static method count.
- Public instance method count.
- Public static property count.
- Public instance property count.

For this exercise, an eligible member is an instance member when its own
Modifiers collection does NOT contain static.

Reporting rule:
Report ONE diagnostic on the class when BOTH are true:
1. At least one eligible static member exists.
2. At least one eligible instance member exists.

Methods and properties contribute together to these conditions.
For example, one public static method and one public instance property
are enough to report.

Rules:
1. The class itself does not need to be public.
2. Count only explicitly public methods and properties.
3. Ignore fields, constructors, events, indexers, operators, and other
   member kinds.
4. Do not include members of nested types in the containing class's counts.
5. Analyze nested classes independently.
6. Count each method declaration separately, including overloads.
7. Count each property once, regardless of its accessor count.
8. A private setter does not exclude a public property.
9. Other modifiers do not exclude an otherwise eligible member.
10. Treat each partial class declaration separately; do not combine parts.
11. Do not analyze structs, records, or interfaces as the target declaration.
    A class nested inside one of those types is still analyzed.
12. Use syntax only. Do not inspect inherited members or infer accessibility.
13. Existing helpers and extensions are allowed.

Required diagnostic data:
- ClassName: class identifier ValueText, without type parameters.
- Methods.StaticCount
- Methods.InstanceCount
- Properties.StaticCount
- Properties.InstanceCount

Diagnostic location:
Highlight ONLY the class identifier.

Diagnostic:
- ID: MIXEDMEMBERS001
- Severity: Warning
- Message:
  Class '{ClassName}' mixes public static and instance members: methods {Methods.StaticCount} static, {Methods.InstanceCount} instance; properties {Properties.StaticCount} static, {Properties.InstanceCount} instance

Examples:

class Worker
{
    public static void Run() { }
    public int Count { get; private set; }
}
    Reports on Worker:
    methods 1 static, 0 instance; properties 0 static, 1 instance.

class Worker
{
    public static void Run() { }
    private void Hidden() { }
}
    No diagnostic: no eligible instance member.

class Worker
{
    public void Run() { }
    public int Count { get; }
}
    No diagnostic: no eligible static member.
*/

using System.Data;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson10CommonDeclarationComponentsAndModifiers.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "MIXEDMEMBERS001",
        title: "Mixed public static and instance members",
        messageFormat:
            "Class '{0}' mixes public static and instance members: " +
            "methods {1} static, {2} instance; " +
            "properties {3} static, {4} instance",
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
            ClassDeclarationSyntax node = context.ToClassSyntax;
            string name = node.Name;


            int methodInstances = node.GetFilteredMethods(x => x.IsPublic).Count(x => x.IsInstance);
            int methodStatics = node.GetFilteredMethods(x => x.IsPublic).Count(x => x.IsStatic);
            int propertyInstances = node.GetAllProperties.Count(x => x.IsInstance && x.IsPublic);
            int propertyStatics = node.GetAllProperties.Count(x => x.IsStatic && x.IsPublic);
            if (methodInstances == 0 && propertyInstances == 0)
            {
                return;
            }
            if (methodStatics == 0 && propertyStatics == 0)
            {
                return;
            }
            MemberCounts properties = new()
            {
                InstanceCount = propertyInstances,
                StaticCount = propertyStatics
            };
            MemberCounts methods = new()
            {
                InstanceCount = methodInstances,
                StaticCount = methodStatics
            };
            ClassDiagnosticData data = new()
            {
                ClassName = name,
                Methods = methods,
                Properties = properties
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.ClassDeclaration);

        context.EnableConcurrentExecution();

    }
    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ClassName,
            data.Methods.StaticCount,
            data.Methods.InstanceCount,
            data.Properties.StaticCount,
            data.Properties.InstanceCount);
    }
}

public class ClassDiagnosticData
{
    public string ClassName { get; set; } = "";
    public MemberCounts Methods { get; set; } = new();
    public MemberCounts Properties { get; set; } = new();
}

public class MemberCounts
{
    public int StaticCount { get; set; }
    public int InstanceCount { get; set; }
}