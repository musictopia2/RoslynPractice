/*
Exercise 05 — Audit Flexible Generic Overload Groups

Client request:
Our library has public generic overloads whose signatures combine missing
constraints with default values or reference parameters. We want one
summary diagnostic for an overload group that repeatedly uses this pattern.

Scope:
- Analyze class declarations only.
- Exclude structs, interfaces, records, and record structs.
- Analyze each class declaration independently.
- For partial classes, do not combine declarations.
- Analyze nested classes independently.
- Include only methods declared directly inside the class.
- Include only methods with an explicit public modifier and at least
  one method type parameter.
- Static methods and methods without bodies are included.
- Containing class accessibility does not affect inclusion.
- Exclude constructors, operators, local functions, and other members.
- Use syntax only. Existing helpers/extensions are allowed.

Grouping:
- Group included methods by BOTH:
  1. Method identifier ValueText.
  2. Number of method type parameters.
- Use ordinal, case-sensitive name comparison.
- Parameter types and ordinary parameter counts do not affect grouping.

A method is problematic when BOTH conditions are true:
1. At least one of its own type parameters has no matching where clause.
2. It has at least one parameter with an explicit default value,
   OR at least one parameter with a ref, out, or in modifier.

Details:
- Match constraint clauses to method type parameters by identifier
  ValueText, using ordinal, case-sensitive comparison.
- Any matching where clause counts as constrained.
- Containing class constraints do not constrain method type parameters.
- Attributes alone do not count as explicit defaults.
- A ref readonly parameter counts as one reference parameter.
- params alone does not count as a reference modifier.
- Examine only each method's own parameters and type parameters.

Report a group only when:
- It contains at least two included methods.
- At least two of those methods are problematic.

Reporting:
- Produce exactly ONE diagnostic per qualifying group.
- Locate it on the identifier of the FIRST included method in that
  group, in source order—even if that first method is not problematic.
- Different qualifying groups produce separate diagnostics.

Summary counts:
- MethodCount: all included methods in the group.
- ProblematicMethodCount: problematic methods in the group.
- UnconstrainedTypeParameterCount: total unconstrained type parameters
  across problematic methods only.
- DefaultParameterCount: total parameters with explicit defaults
  across problematic methods only.
- ReferenceParameterCount: total parameters with ref, out, or in
  across problematic methods only.
- Count each parameter once within each category.
- The default and reference categories are independent.

Names:
- Use identifier ValueText for class and method names.
- Class name means the immediate containing class's identifier,
  without namespace, outer class names, or generic type parameters.
- Method name excludes generic type parameters.
- Diagnostic locations include the full escaped identifier token.

Diagnostic:
- ID: GENERICGROUP001
- Title: Generic overload group needs signature review
- Category: Practice
- Severity: Warning
- Enabled by default: true
- Message:
  Class '{className}' method group '{methodName}' with {typeParameterCount} type parameters has {problematicMethodCount} problematic methods out of {methodCount}: {unconstrainedTypeParameterCount} unconstrained type parameters, {defaultParameterCount} default-valued parameters, {referenceParameterCount} reference parameters
*/
namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson07ParametersTypeParametersConstraints.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "GENERICGROUP001",
        title: "Generic overload group needs signature review",
        messageFormat:
            "Class '{0}' method group '{1}' with {2} type parameters has {3} problematic methods out of {4}: {5} unconstrained type parameters, {6} default-valued parameters, {7} reference parameters",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [_rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {

            ClassDeclarationSyntax node = context.ToClassSyntax;
            var firsts = node.GetFilteredMethods(x => x.IsPublic && x.TypeParameterCount > 0);

            var seconds = firsts
                .GroupBy(x => (
                    Name: x.Name,
                    TypeParameterCount: x.TypeParameterCount))
                .ToList();

            string className = node.Name;

            foreach (var methodGroup in seconds)
            {
                int methodCount = methodGroup.Count();
                int unconstrainedCount = 0;
                int defaultCount = 0;
                int referenceCount = 0;
                int totalProblems = 0;
                foreach (var methodNode in methodGroup)
                {
                    bool problem = false;
                    bool unconstrained = false;
                    int tempcon;
                    int tempref;
                    int tempdefault;
                    tempcon = methodNode.GetUnconstrainedTypes().Count();
                    if (tempcon > 0)
                    {
                        unconstrained = true;
                    }
                    tempref = methodNode.GetAllParameters().Count(x => x.ReferenceModifierText is not null);
                    if (tempref > 0)
                    {
                        problem = unconstrained;
                    }
                    tempdefault = methodNode.GetAllParameters().Count(x => x.Default is not null);
                    if (tempdefault > 0)
                    {
                        problem = unconstrained;
                    }
                    if (problem)
                    {
                        totalProblems++;
                        referenceCount += tempref;
                        unconstrainedCount += tempcon;
                        defaultCount += tempdefault;
                    }
                }
                if (totalProblems < 2)
                {
                    continue;
                }
                GroupSummaryData summary = new()
                {
                    DefaultParameterCount = defaultCount,
                    ProblematicMethodCount = totalProblems,
                    ReferenceParameterCount = referenceCount,
                    UnconstrainedTypeParameterCount = unconstrainedCount
                };
                GroupDiagnosticData data = new()
                {
                    ClassName = className,
                    MethodCount = methodCount,
                    MethodName = methodGroup.Key.Name,
                    TypeParameterCount = methodGroup.Key.TypeParameterCount,
                    Summary = summary
                };
                Diagnostic d = CreateDiagnostic(_rule, methodGroup.First().GetIdentifierLocation, data);
                context.ReportDiagnostic(d);
            }            
        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
    }
    private sealed class GroupDiagnosticData
    {
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public int TypeParameterCount { get; set; }
        public int MethodCount { get; set; }

        public GroupSummaryData Summary { get; set; }
            = new GroupSummaryData();
    }

    private sealed class GroupSummaryData
    {
        public int ProblematicMethodCount { get; set; }
        public int UnconstrainedTypeParameterCount { get; set; }
        public int DefaultParameterCount { get; set; }
        public int ReferenceParameterCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        GroupDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ClassName,
            data.MethodName,
            data.TypeParameterCount,
            data.Summary.ProblematicMethodCount,
            data.MethodCount,
            data.Summary.UnconstrainedTypeParameterCount,
            data.Summary.DefaultParameterCount,
            data.Summary.ReferenceParameterCount);
    }
}