/*
Lesson 05 — Method-Like Declarations and BaseMethodDeclarationSyntax
Exercise 05 — Public Overload Complexity Review

Client requirement:

Identify public method overload groups that may be becoming
difficult for callers to understand.

Scope:

1. Examine ordinary method declarations directly belonging to
   classes, structs, interfaces, record classes, and record structs.

2. Only include methods with an explicitly written public modifier.
   An interface method without that modifier is outside this rule.

3. Exclude constructors, destructors, operators, conversion operators,
   local functions, accessors, and delegates.

Grouping:

4. Group eligible methods by BOTH:
   - Method identifier ValueText
   - Number of type parameters declared by the method

   Examples:
   Save(int value)
   Save(string value)
       => Same group: Save, generic arity 0

   Save<T>(T value)
   Save<TItem>(TItem value, int count)
       => Same group: Save, generic arity 1

   Save<TFirst, TSecond>(TFirst first, TSecond second)
       => Separate group: Save, generic arity 2

   Type parameter names do not affect grouping.

5. Groups are restricted to one containing type declaration.
   Do not combine methods from:
   - Nested and outer types
   - Sibling types
   - Separate partial-type declarations

6. Count syntax declarations independently.
   Do not use symbols to merge partial-method declarations.
   Static and instance methods can belong to the same group.

Reporting:

7. A group qualifies only when BOTH conditions are true:
   a. It contains at least two eligible method declarations.
   b. It contains at least three declarations OR its maximum
      ordinary parameter count is at least four.

   Examples:
   - Two overloads with 1 and 2 parameters: no diagnostic.
   - Three overloads with 0, 1, and 2 parameters: diagnostic.
   - Two overloads with 1 and 4 parameters: diagnostic.
   - One method with 5 parameters: no diagnostic.

8. Report exactly ONE diagnostic per qualifying group.

9. Report at the identifier token of the FIRST eligible method
   in that group, in source order.

10. OverloadCount is the number of eligible declarations in the group.

11. MaximumParameterCount is the largest ordinary parameter count
    among those declarations.
    Optional and params parameters each count as one.

12. Use these containing-type Kind values:
    Class, Struct, Interface, Record Class, Record Struct

13. Use ValueText for method and containing-type names.
    Exclude namespaces and containing-type generic parameter lists.

14. Include all method body styles.
    Method bodies and constraint clauses do not affect grouping.

15. Use syntax analysis only. Existing helpers are allowed.
    Choose an appropriate registration and implementation structure.

Diagnostic contract:
   ID: RMP030505
   Severity: Warning
   Message:
   "Public method group '{MethodName}' with generic arity {TypeParameterCount} in {ContainingType.Kind} '{ContainingType.Name}' has {OverloadCount} overload declaration(s); maximum parameter count: {MaximumParameterCount}"

Example:
   Public method group 'Save' with generic arity 0 in Class 'Storage' has 3 overload declaration(s); maximum parameter count: 2
*/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using static AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise02.MainClass;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "RMP030505",
        title: "Review public overload group complexity",
        messageFormat:
            "Public method group '{0}' with generic arity {1} in {2} '{3}' has {4} overload declaration(s); maximum parameter count: {5}",
        category: "SyntaxAnalysis",
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
            var node = context.ToTypeDeclaration;

            var methods = node.GetFilteredMethods(m => m.IsPublic);

            var seconds = methods
                .GroupBy(x => (Name: x.Name, TypeParameterCount: x.TypeParameterCount))
                .ToList();
            ContainingTypeDiagnosticData contain = new()
            {
                Kind = node.KindName,
                Name = node.Name
            };


            

            foreach (var item in seconds)
            {
                if (item.Count() < 2)
                {
                    continue;
                }
                int maxs = item.Max(x => x.ParameterCount);

                bool canReport = false;
                if (item.Count() >= 3)
                {
                    canReport = true;
                }
                if (maxs >= 4)
                {
                    canReport = true;
                }
                if (canReport == false)
                {
                    continue;
                }
                

                OverloadGroupDiagnosticData data = new()
                {
                    MethodName = item.First().Name,
                    TypeParameterCount = item.First().TypeParameterCount,
                    ContainingType = contain,
                    OverloadCount = item.Count(),
                    MaximumParameterCount = item.Max(x => x.ParameterCount)
                };
                Diagnostic d = CreateDiagnostic(_rule, item.First().GetIdentifierLocation, data);
                context.ReportDiagnostic(d);
            }


        }, SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);

        context.EnableConcurrentExecution();

    }

    public sealed class ContainingTypeDiagnosticData
    {
        public string Kind { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public sealed class OverloadGroupDiagnosticData
    {
        public string MethodName { get; set; } = "";
        public int TypeParameterCount { get; set; }
        public ContainingTypeDiagnosticData ContainingType { get; set; } = new();
        public int OverloadCount { get; set; }
        public int MaximumParameterCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        OverloadGroupDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.MethodName,
            data.TypeParameterCount,
            data.ContainingType.Kind,
            data.ContainingType.Name,
            data.OverloadCount,
            data.MaximumParameterCount);
    }

}