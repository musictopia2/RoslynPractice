/*
Exercise 03 — Method Type Parameters Without Constraints

Create a syntax-only analyzer that reports method type parameters
that have no corresponding where clause.

Analysis:
- Register a syntax node action for SyntaxKind.MethodDeclaration.
- Examine only type parameters declared by that method.
- Report once for each method type parameter that has no matching
  constraint clause.
- Match the type parameter name to the name after "where".
- Compare identifier ValueText using ordinal, case-sensitive comparison.
- Any matching where clause counts as constrained, regardless of
  which constraints it contains.
- Do not inspect or evaluate individual constraints.
- Methods without type parameters produce no diagnostics.
- Analyze methods regardless of accessibility, static modifier,
  return type, or whether they have a body.
- Include interface methods.
- Exclude type parameters declared by classes, structs, interfaces,
  records, delegates, or local functions.
- A containing type's constraints do not constrain a method's own
  type parameters.
- Use syntax only. Existing helpers/extensions are allowed.

Diagnostic:
- ID: TYPEPARAM001
- Title: Method type parameter has no constraint clause
- Category: Practice
- Severity: Warning
- Enabled by default: true
- Message:
  Type parameter '{typeParameterName}' in method '{methodName}' has no constraint clause

Diagnostic location:
- Highlight only the unconstrained type parameter's identifier
  in the method's type parameter list.
- Do not highlight the method name, the whole type parameter list,
  or a name in a where clause.
- Escaped identifiers use ValueText in messages, while their
  locations include the entire identifier token.
- Use the method's identifier name without generic type parameters
  or its containing type's name.

Example:
    void Convert<TInput, TOutput, TContext>()
        where TInput : class
        where TContext : new()
    {
    }

Produces exactly one diagnostic:
- Highlight TOutput in <TInput, TOutput, TContext>.
- Message:
  Type parameter 'TOutput' in method 'Convert' has no constraint clause

Success:
Each method type parameter without a matching where clause produces
exactly one diagnostic. Constrained type parameters produce none.
*/
namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson07ParametersTypeParametersConstraints.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "TYPEPARAM001",
        title: "Method type parameter has no constraint clause",
        messageFormat:
            "Type parameter '{0}' in method '{1}' has no constraint clause",
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
            MethodDeclarationSyntax node = context.ToMethodSyntax;
            var typeParameters = node.GetAllTypeParameters();
            var clauses = node.ConstraintClauseNames;
            foreach (var item in typeParameters)
            {
                
                if (clauses.Contains(item.Name) == false)
                {
                    Location location = item.GetIdentifierLocation;
                    TypeParameterDiagnosticData data = new()
                    {
                        MethodName = node.Name,
                        TypeParameterName = item.Name,
                    };
                    Diagnostic d = CreateDiagnostic(_rule, location, data);
                    context.ReportDiagnostic(d);
                }
            }
        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();
    }

    private sealed class TypeParameterDiagnosticData
    {
        public string TypeParameterName { get; set; } = "";
        public string MethodName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        TypeParameterDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.TypeParameterName,
            data.MethodName);
    }
}