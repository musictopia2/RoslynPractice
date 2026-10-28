/*
Lesson 05 — Method-Like Declarations and BaseMethodDeclarationSyntax
Exercise 01 — Identify Method-Like Declarations

Requirements:

1. Register syntax-node analysis for:
   - Methods
   - Constructors
   - Destructors
   - Operators
   - Conversion operators

2. Use BaseMethodDeclarationSyntax to obtain the parameter count.

3. Report exactly one diagnostic for each matching declaration,
   regardless of accessibility, modifiers, or parameter count.
   Include declarations inside nested types.

4. Populate MethodLikeDiagnosticData with these exact values:

   Declaration          Kind                  Name
   ---------------------------------------------------------------
   Method               Method                Identifier text
   Constructor          Constructor           Identifier text
   Destructor           Destructor            Identifier text
   Operator             Operator              Operator token text
   Implicit conversion  Conversion Operator   implicit
   Explicit conversion  Conversion Operator   explicit

5. Report the diagnostic at:
   - Method: identifier token.
   - Constructor: identifier token.
   - Destructor: identifier token, excluding the tilde.
   - Operator: operator token, such as + or ==.
   - Conversion operator: implicit or explicit keyword.

6. Count declared parameters, not arguments or type parameters.
   Each optional parameter and each params parameter counts as one.

7. Include declarations with block bodies, expression bodies,
   or no body, including interface and abstract methods.
   Treat separate partial-method declarations independently.

8. Do not report diagnostics for:
   - Local functions
   - Lambdas or anonymous methods
   - Property, indexer, or event accessors
   - Delegate declarations
   - Method calls or object creation expressions

9. Primary constructors are outside this exercise because their
   parameter lists belong to type declarations.

10. Use syntax analysis only. Existing syntax helpers are allowed.

Diagnostic contract:
   ID: RMP030501
   Severity: Info
   Message: "{Kind} '{Name}' declares {ParameterCount} parameter(s)"
   No trailing period.

Example messages:
   Method 'Run' declares 0 parameter(s)
   Constructor 'Sample' declares 1 parameter(s)
   Destructor 'Sample' declares 0 parameter(s)
   Operator '+' declares 2 parameter(s)
   Conversion Operator 'implicit' declares 1 parameter(s)
*/

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "RMP030501",
        title: "Describe a method-like declaration",
        messageFormat: "{0} '{1}' declares {2} parameter(s)",
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

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(context =>
        {
            var node = context.ToBaseMethodDeclaration;

            MethodLikeDiagnosticData data = new()
            {
                Kind = node.KindName,
                Name = node.Name,
                ParameterCount = node.ParameterCount
            };

            Diagnostic d = CreateDiagnostic(_rule,node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration);
    }

    public sealed class MethodLikeDiagnosticData
    {
        public string Kind { get; set; } = "";
        public string Name { get; set; } = "";
        public int ParameterCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        MethodLikeDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.Kind,
            data.Name,
            data.ParameterCount);
    }

    // TODO: Add your analysis callback.
}