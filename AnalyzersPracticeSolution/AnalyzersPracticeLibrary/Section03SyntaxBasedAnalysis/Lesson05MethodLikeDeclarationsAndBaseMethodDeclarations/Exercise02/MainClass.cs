/*
Lesson 05 — Method-Like Declarations and BaseMethodDeclarationSyntax
Exercise 02 — Parameter Name Inventory

Requirements:

1. Analyze these five declaration kinds:
   - Methods
   - Constructors
   - Destructors
   - Operators
   - Conversion operators

2. Report exactly one diagnostic for each matching declaration
   that has at least one declared parameter.

3. Do not report declarations with zero parameters.
   This also means destructors receive no diagnostic.

4. Obtain the parameters through BaseMethodDeclarationSyntax.

5. Collect every parameter's identifier in declaration order.
   Join the names with exactly ", " between names.
   Do not add a leading or trailing separator.

   Examples:
   - Run(int count)
     ParameterNames: "count"

   - Save(string name, int score)
     ParameterNames: "name, score"

   - operator +(Sample left, Sample right)
     ParameterNames: "left, right"

6. Include only parameter names in ParameterNames.
   Exclude types, attributes, modifiers, and default values.

   Optional parameters and params parameters are included normally.
   Generic type parameters are not method parameters.

7. For escaped parameter identifiers, use ValueText:
   - int @class produces the parameter name "class".

8. Use the same declaration Kind, Name, and diagnostic location
   conventions as Exercise 01.
   Your existing declaration helpers are allowed.

9. Include matching declarations regardless of accessibility,
   modifiers, or body style, including declarations in nested types.
   Treat separate partial-method declarations independently.

10. Exclude local functions, lambdas, anonymous methods, accessors,
    delegates, calls, object creation, and primary constructors.

11. Use syntax analysis only.

Diagnostic contract:
   ID: RMP030502
   Severity: Info
   Message:
   "{Kind} '{Name}' declares {ParameterCount} parameter(s): {ParameterNames}"

Example messages:
   Method 'Save' declares 2 parameter(s): name, score
   Constructor 'Sample' declares 1 parameter(s): value
   Operator '+' declares 2 parameter(s): left, right
   Conversion Operator 'implicit' declares 1 parameter(s): value
*/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using static AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise01.MainClass;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "RMP030502",
        title: "Describe declared parameter names",
        messageFormat: "{0} '{1}' declares {2} parameter(s): {3}",
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
            var node = context.ToBaseMethodDeclaration;
            if (node.ParameterCount == 0)
            {
                return;
            }
            ParameterInventoryDiagnosticData data = new()
            {
                Kind = node.KindName,
                Name = node.Name,
                ParameterCount = node.ParameterCount,
                ParameterNames = node.ParameterNames
            };

            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration);
        context.EnableConcurrentExecution();
    }

    public sealed class ParameterInventoryDiagnosticData
    {
        public string Kind { get; set; } = "";
        public string Name { get; set; } = "";
        public int ParameterCount { get; set; }
        public string ParameterNames { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ParameterInventoryDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.Kind,
            data.Name,
            data.ParameterCount,
            data.ParameterNames);
    }

    // TODO: Implement your analysis.
}