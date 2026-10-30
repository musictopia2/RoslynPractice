/*
Exercise 04 — Public Generic Methods with Defaults and Missing Constraints

Create a syntax-only analyzer that reports a method only when ALL
three conditions are true:

1. The method explicitly contains the public modifier.
2. At least one of its own type parameters has no matching where clause.
3. At least one of its own ordinary parameters has an explicit default value.

Analysis:
- Register a syntax node action for SyntaxKind.MethodDeclaration.
- Report exactly one diagnostic per qualifying method.
- Methods without an explicit public modifier are excluded.
  This includes interface methods that are public only implicitly.
- Explicitly public interface methods are included.
- Static methods and methods without bodies are included.
- Examine only parameters and type parameters declared by that method.
- Exclude local functions and all non-method declarations.
- Containing type accessibility does not affect qualification.
- Containing type constraints do not constrain method type parameters.
- Match constraint clause names using identifier ValueText and ordinal,
  case-sensitive comparison.
- Any matching where clause counts as constrained.
- An explicit default means an "= expression" clause exists.
  Attributes alone do not count.
- Use syntax only. Existing helpers/extensions are allowed.

Diagnostic data:
- Include only unconstrained method type parameter names, in declaration order.
- Include only ordinary parameter names having explicit defaults,
  in declaration order.
- Join each list with ", " (comma followed by one space).
- Use identifier ValueText for all names.
- Use the method's identifier name without generic type parameters
  or its containing type's name.

Diagnostic:
- ID: GENERICDEFAULT001
- Title: Public generic method combines defaults with missing constraints
- Category: Practice
- Severity: Warning
- Enabled by default: true
- Message:
  Public method '{methodName}' has unconstrained type parameters [{unconstrainedTypeParameterNames}] and default-valued parameters [{defaultParameterNames}]

Diagnostic location:
- Highlight only the METHOD'S identifier.
- Do not highlight any parameter or type parameter.
- For an escaped method identifier, include the entire token in the
  location but omit the @ prefix from its name in the message.

Example:
    public void Convert<TInput, TOutput, TContext>(
        int required,
        int count = 5,
        bool enabled = false)
        where TInput : class
    {
    }

Produces exactly one diagnostic on Convert:

Public method 'Convert' has unconstrained type parameters [TOutput, TContext] and default-valued parameters [count, enabled]

No diagnostic if:
- The method has no explicit public modifier.
- Every method type parameter has a matching where clause.
- The method has no type parameters.
- No ordinary parameter has an explicit default value.
*/
namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson07ParametersTypeParametersConstraints.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "GENERICDEFAULT001",
        title: "Public generic method combines defaults with missing constraints",
        messageFormat:
            "Public method '{0}' has unconstrained type parameters [{1}] and default-valued parameters [{2}]",
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

            if (node.IsPublic == false)
            {
                return; //only public methods.
            }
            var parameters = node.GetFilteredParameters(x => x.Default is not null);
            var types = node.GetAllTypeParameters();

            //var clauses = node.ConstraintClauseNames;
            var unConstrains = node.GetUnconstrainedTypes();
            if (unConstrains.Count == 0)
            {
                return;
            }

            if (parameters.Count == 0)
            {
                return;
            }


            List<string> fins = unConstrains.Select(x => x.Name).ToList();

            string defaultText = parameters.Select(x => x.Name).TextList;

            string unConstrained = fins.TextList;

            MethodDiagnosticData data = new()
            {
                MethodName = node.Name,
                DefaultParameterNames = defaultText,
                UnconstrainedTypeParameterNames = unConstrained
            };
            Diagnostic d = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();

    }

    private sealed class MethodDiagnosticData
    {
        public string MethodName { get; set; } = "";
        public string UnconstrainedTypeParameterNames { get; set; } = "";
        public string DefaultParameterNames { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        MethodDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.MethodName,
            data.UnconstrainedTypeParameterNames,
            data.DefaultParameterNames);
    }
}