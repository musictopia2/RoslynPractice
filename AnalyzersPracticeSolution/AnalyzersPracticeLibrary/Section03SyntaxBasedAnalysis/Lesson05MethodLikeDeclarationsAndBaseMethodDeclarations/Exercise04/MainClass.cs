/*
Lesson 05 — Method-Like Declarations and BaseMethodDeclarationSyntax
Exercise 04 — Containing Type Inventory

Requirements:

1. Analyze all five method-like declaration kinds from Exercise 01.

2. Report exactly one diagnostic for every matching declaration,
   including declarations with zero parameters.

3. Identify the nearest containing class-like declaration:
   - Class
   - Struct
   - Interface
   - Record class
   - Record struct

4. For nested types, use the immediate containing type only.
   Do not use the outermost type or build a qualified name.

   Example:
   class Outer
   {
       class Inner
       {
           void Run() { }
       }
   }

   Run belongs to Class 'Inner'.

5. Use these exact containing-type Kind values:
   - "Class"
   - "Struct"
   - "Interface"
   - "Record Class"
   - "Record Struct"

   Both "record Example" and "record class Example" use "Record Class".

6. ContainingType.Name is the type identifier's ValueText.
   Exclude its namespace and type parameter list.

7. Declaration.Kind uses the same values as Exercise 01:
   - "Method"
   - "Constructor"
   - "Destructor"
   - "Operator"
   - "Conversion Operator"

8. Declaration.Name:
   - Method, constructor, destructor: Identifier.ValueText
   - Operator: OperatorToken.Text
   - Conversion operator: ImplicitOrExplicitKeyword.Text

9. ParameterCount counts ordinary declared parameters.
   Do not include type parameters or primary-constructor parameters.

10. Use the same diagnostic token locations as Exercise 01.

11. Include all accessibility levels, modifiers, and body styles.
    Treat separate partial-method declarations independently.

12. Exclude local functions, lambdas, anonymous methods, accessors,
    delegates, calls, object creation, and primary constructors.

13. Use syntax analysis only. Existing helpers are allowed.
    If no containing class-like declaration is found, skip the node.

Diagnostic contract:
   ID: RMP030504
   Severity: Info
   Message:
   "{Declaration.Kind} '{Declaration.Name}' in {ContainingType.Kind} '{ContainingType.Name}' declares {ParameterCount} parameter(s)"

Examples:
   Method 'Run' in Class 'Inner' declares 0 parameter(s)
   Constructor 'Player' in Record Class 'Player' declares 1 parameter(s)
   Method 'Move' in Record Struct 'Position' declares 2 parameter(s)
*/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using static AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise02.MainClass;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "RMP030504",
        title: "Describe a method-like declaration and its containing type",
        messageFormat:
            "{0} '{1}' in {2} '{3}' declares {4} parameter(s)",
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


            DeclarationDiagnosticData firsts = new()
            {
                Kind = node.KindName,
                Name = node.Name
            };
            TypeDeclarationSyntax? contains = node.GetFirstContainingType;
            if (contains is null)
            {
                return;
            }

            ContainingTypeDiagnosticData others = new()
            {
                Kind = contains.KindName,
                Name = contains.Name
            };


            ContainingTypeInventoryDiagnosticData data = new()
            {
                ParameterCount = node.ParameterCount,
                Declaration = firsts,
                ContainingType = others
            };

            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration);
        context.EnableConcurrentExecution();

        // TODO: Register your analysis.
    }

    public sealed class DeclarationDiagnosticData
    {
        public string Kind { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public sealed class ContainingTypeDiagnosticData
    {
        public string Kind { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public sealed class ContainingTypeInventoryDiagnosticData
    {
        public DeclarationDiagnosticData Declaration { get; set; } = new();
        public ContainingTypeDiagnosticData ContainingType { get; set; } = new();
        public int ParameterCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ContainingTypeInventoryDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.Declaration.Kind,
            data.Declaration.Name,
            data.ContainingType.Kind,
            data.ContainingType.Name,
            data.ParameterCount);
    }

    // TODO: Implement your analysis.
}