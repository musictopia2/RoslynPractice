/*
Lesson 05 — Method-Like Declarations and BaseMethodDeclarationSyntax
Exercise 03 — Generic Method Inventory

Requirements:

1. Analyze MethodDeclarationSyntax nodes only.

2. Report exactly one diagnostic for each method that declares
   at least one type parameter.

3. Do not report methods that declare no type parameters,
   even when their containing type is generic.

   Example:
   class Container<T>
   {
       public void Save(T value) { }
   }

   Save does not declare its own type parameters, so do not report it.

4. Collect the method's type parameter names in declaration order.
   Join them using exactly ", " between names.
   Use Identifier.ValueText so escaped identifiers lose the @ prefix.

5. TypeParameterCount counts only type parameters declared by
   the method. Do not include the containing type's type parameters.

6. ParameterCount counts the method's ordinary declared parameters.
   Optional parameters and params parameters each count as one.

7. MethodName is the method identifier's ValueText.

8. Report the diagnostic at the method identifier token only.

9. Include generic methods regardless of:
   - Accessibility or modifiers
   - Block body, expression body, or no body
   - Whether they belong to a class, struct, record, or interface
   - Whether their containing type is nested

10. Treat separate partial-method declarations independently.

11. Constraint clauses do not change either count.
    You do not need to inspect or describe their contents.

12. Do not report generic local functions, delegate declarations,
    generic type declarations, or generic method calls.

13. Use syntax analysis only. Existing helpers are allowed.

Diagnostic contract:
   ID: RMP030503
   Severity: Info
   Message:
   "Method '{MethodName}' declares {TypeParameterCount} type parameter(s): {TypeParameterNames}; ordinary parameter count: {ParameterCount}"

Examples:

   void Reset<T>() { }

   Method 'Reset' declares 1 type parameter(s): T; ordinary parameter count: 0


   TOutput Convert<TInput, TOutput>(TInput value) => default;

   Method 'Convert' declares 2 type parameter(s): TInput, TOutput; ordinary parameter count: 1
*/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using static AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise02.MainClass;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "RMP030503",
        title: "Describe generic method parameters",
        messageFormat:
            "Method '{0}' declares {1} type parameter(s): {2}; ordinary parameter count: {3}",
        category: "SyntaxAnalysis",
        defaultSeverity: DiagnosticSeverity.Info,
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
            var node = context.ToMethodSyntax;
            if (node.TypeParameterCount == 0)
            {
                return;
            }
            GenericMethodDiagnosticData data = new()
            {
                MethodName = node.Identifier.ValueText,
                TypeParameterCount = node.TypeParameterCount,
                ParameterCount = node.ParameterCount,
                TypeParameterNames = node.TypeParameterNames
            };

            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();

    }

    public sealed class GenericMethodDiagnosticData
    {
        public string MethodName { get; set; } = "";
        public int TypeParameterCount { get; set; }
        public string TypeParameterNames { get; set; } = "";
        public int ParameterCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        GenericMethodDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.MethodName,
            data.TypeParameterCount,
            data.TypeParameterNames,
            data.ParameterCount);
    }

    // TODO: Implement your analysis.
}