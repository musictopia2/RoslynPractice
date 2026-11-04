/*
Exercise 05 — Overload Modifier Consistency

Client requirement:
Identify overload groups whose methods use inconsistent modifiers.

Analyze:
Each ClassDeclarationSyntax independently.

Eligible methods:
Only MethodDeclarationSyntax members declared DIRECTLY inside the class
that have explicitly written accessibility.

Include all six accessibility values from Exercise 03:
public, private, protected, internal,
protected internal, private protected.

Exclude methods without explicitly written accessibility.

Group identity:
Methods belong to the same group when BOTH match:
- Identifier ValueText.
- Number of method type parameters.

Parameter counts and parameter types do not affect grouping.

Examples:
Run() and Run(int value) belong to the same group.
Run<T>() and Run<U>(int value) belong to the same group.
Run() and Run<T>() belong to DIFFERENT groups.

For each group, determine:
- Total eligible method count.
- Static method count.
- Instance method count.
- Number of distinct normalized explicit accessibility values.

An instance method means its own modifiers do not contain static.

Reporting rule:
Report ONE diagnostic per group when:
1. The group contains at least TWO eligible methods.
AND
2. At least ONE of these inconsistencies exists:
   - The group contains both static and instance methods.
   - The group contains more than one distinct accessibility value.

If both inconsistencies exist, still report only ONE diagnostic.

Location:
Highlight ONLY the identifier of the FIRST ELIGIBLE METHOD in that group,
in source order.

Do not report on the class identifier.
Do not report separately on every method in the group.

Other rules:
1. The class itself has no accessibility or static requirement.
2. Only direct methods contribute. Exclude constructors, operators,
   local functions, properties, and all other declaration kinds.
3. Analyze nested classes independently.
4. Do not combine methods from separate classes or partial class parts.
5. Ignore other modifiers, attributes, method bodies, and constraints
   when determining eligibility and consistency.
6. Normalize combined accessibility regardless of keyword order:
   internal protected and protected internal are the same value.
7. Use ordinal, case-sensitive comparison for method names.
8. Use syntax only. Do not inspect inherited members or resolve types.
9. Inputs will not contain duplicate or conflicting accessibility
   modifiers on an individual method.
10. Existing helpers and extensions are allowed.

Required diagnostic data:
- ClassName: class identifier ValueText, without type parameters.
- Group.Name: method identifier ValueText, without type parameters.
- Group.TypeParameterCount: number of METHOD type parameters.
- Group.MethodCount: total eligible methods in the group.
- Group.StaticCount
- Group.InstanceCount
- Group.AccessibilityCount: distinct normalized accessibility count.

Diagnostic:
- ID: OVERLOADMOD001
- Severity: Warning
- Message:
  Class '{ClassName}' has inconsistent modifiers for method group '{Group.Name}' with {Group.TypeParameterCount} type parameter(s): {Group.MethodCount} method(s), {Group.StaticCount} static, {Group.InstanceCount} instance, {Group.AccessibilityCount} accessibility value(s)

Examples:

class Worker
{
    public static void Run() { }
    public void Run(int value) { }
}
    Reports once on the FIRST Run identifier.
    MethodCount = 2
    StaticCount = 1
    InstanceCount = 1
    AccessibilityCount = 1

class Worker
{
    public void Run() { }
    private void Run(int value) { }
}
    Reports once on the FIRST Run identifier.
    MethodCount = 2
    StaticCount = 0
    InstanceCount = 2
    AccessibilityCount = 2

class Worker
{
    public void Run() { }
    public void Run(int value) { }
}
    No diagnostic: the group is consistent.

class Worker
{
    void Run() { }
    public static void Run(int value) { }
}
    No diagnostic: only one method is eligible.
    The method without explicit accessibility is excluded entirely.

class Worker
{
    public static void Run() { }
    private void Run<T>() { }
}
    No diagnostic: these are separate groups containing one method each.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson10CommonDeclarationComponentsAndModifiers.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "OVERLOADMOD001",
        title: "Inconsistent overload modifiers",
        messageFormat:
            "Class '{0}' has inconsistent modifiers for method group '{1}' " +
            "with {2} type parameter(s): {3} method(s), " +
            "{4} static, {5} instance, {6} accessibility value(s)",
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

            var methods = node.GetAllMethods;
            methods.RemoveAll(x => x.ExplicitAccessibilityText == "");

            var groups = methods.GroupBy(x => (
                Name: x.Name,
                TypeParameterCount: x.TypeParameterCount));

            foreach (var item in groups)
            {
                if (item.Count() < 2)
                {
                    continue;
                }
                bool anyInstance = item.Any(x => x.IsInstance);
                bool anyStatic = item.Any(x => x.IsStatic);
                bool needsToReport = false;
                if (anyStatic && anyInstance)
                {
                    needsToReport = true;
                }
                if (needsToReport == false)
                {

                    var nexts = item.GroupBy(x => x.ExplicitAccessibilityText).Count();
                    if (nexts > 1)
                    {
                        needsToReport = true;
                    }


                }
                if (needsToReport)
                {
                    MethodGroupDiagnosticData method = new()
                    {
                        AccessibilityCount = item.GroupBy(x => x.ExplicitAccessibilityText).Count(),
                        InstanceCount = item.Count(x => x.IsInstance),
                        StaticCount = item.Count(x => x.IsStatic),
                        MethodCount = item.Count(),
                        TypeParameterCount = item.Key.TypeParameterCount,
                        Name = item.Key.Name,


                    };
                    ClassDiagnosticData data = new()
                    {
                        ClassName = node.Name,
                        Group = method
                    };
                    Diagnostic d = CreateDiagnostic(_rule, item.First().GetIdentifierLocation, data);
                    context.ReportDiagnostic(d);
                }
            }
            
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
            data.Group.Name,
            data.Group.TypeParameterCount,
            data.Group.MethodCount,
            data.Group.StaticCount,
            data.Group.InstanceCount,
            data.Group.AccessibilityCount);
    }
}

public class ClassDiagnosticData
{
    public string ClassName { get; set; } = "";
    public MethodGroupDiagnosticData Group { get; set; } = new();
}

public class MethodGroupDiagnosticData
{
    public string Name { get; set; } = "";
    public int TypeParameterCount { get; set; }
    public int MethodCount { get; set; }
    public int StaticCount { get; set; }
    public int InstanceCount { get; set; }
    public int AccessibilityCount { get; set; }
}