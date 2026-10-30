/*
Exercise 02 — Parameters Passed by Reference

Create a syntax-only analyzer that reports method parameters containing
a ref, out, or in modifier.

Analysis:
- Register a syntax node action for SyntaxKind.MethodDeclaration.
- Examine only each method's own parameter list.
- Report exactly one diagnostic for each parameter containing a ref,
  out, or in modifier.
- Ignore parameters without those modifiers.
- A params modifier alone does not qualify.
- Default values and attributes do not affect whether a parameter qualifies.
- Analyze methods regardless of accessibility, static modifier, return
  type, generic type parameters, or whether they have a body.
- Include interface methods.
- Exclude constructor, delegate, indexer, lambda, anonymous-method,
  and local-function parameters.
- Use syntax only. Existing helpers/extensions are allowed.

Modifier name:
- Use "ref", "out", or "in", matching the qualifying keyword.
- A "ref readonly" parameter qualifies and uses "ref" in the message.
- Report once per parameter, not once per modifier.

Diagnostic:
- ID: PARAMREF001
- Title: Parameter is passed by reference
- Category: Practice
- Severity: Warning
- Enabled by default: true
- Message:
  Parameter '{parameterName}' in method '{methodName}' uses the '{modifierName}' modifier

Diagnostic location:
- Highlight only the parameter's identifier.
- Do not highlight its modifier, type, or the entire parameter.
- For @event, highlight the entire @event token but use "event"
  in the diagnostic message.
- Use the method's identifier name without generic type parameters
  or its containing type's name.

Example:
    void Update(ref int count, out bool success, in int limit, int other)
    {
        success = true;
    }

Produces three diagnostics:
- On count: modifier name "ref".
- On success: modifier name "out".
- On limit: modifier name "in".
- No diagnostic on other.

Success:
Every qualifying parameter produces exactly one diagnostic with the
specified message and identifier location. Other parameters produce none.
*/
namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson07ParametersTypeParametersConstraints.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "PARAMREF001",
        title: "Parameter is passed by reference",
        messageFormat:
            "Parameter '{0}' in method '{1}' uses the '{2}' modifier",
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
            var list = node.GetAllParameters();
            foreach (var item in list)
            {
                string? text = item.ReferenceModifierText;
                if (text is not null)
                {
                    Location location = item.GetIdentifierLocation;
                    ParameterDiagnosticData data = new()
                    {
                        MethodName = node.Name,
                        ParameterName = item.Name,
                        ModifierName = text
                    };
                    Diagnostic d = CreateDiagnostic(_rule, location, data);
                    context.ReportDiagnostic(d);
                }

            }
        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();

        // Your analysis goes here.
    }

    private sealed class ParameterDiagnosticData
    {
        public string ParameterName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string ModifierName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ParameterDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ParameterName,
            data.MethodName,
            data.ModifierName);
    }
}