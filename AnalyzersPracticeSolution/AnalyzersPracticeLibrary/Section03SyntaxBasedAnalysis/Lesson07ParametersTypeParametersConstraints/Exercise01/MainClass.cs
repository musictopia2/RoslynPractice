/*
Exercise 01 — Parameters with Explicit Default Values

Create a syntax-only analyzer that reports parameters with an explicit
default value, but only when they belong directly to a method declaration.

Analysis:
- Register a syntax node action for method declarations.
- Examine each method's own parameter list.
- Report one diagnostic for each parameter with an explicit default value.
- Analyze methods regardless of accessibility, static modifier, return
  type, or whether they have a body.
- Include generic methods and methods declared in interfaces.
- Do not analyze constructor, delegate, indexer, lambda, anonymous-method,
  or local-function parameters.
- Do not use semantic analysis.
- Existing syntax helpers/extensions are allowed.

An explicit default value means the parameter contains an "= expression"
clause. The expression's value does not matter.

Examples that qualify:
    int count = 5
    string name = null
    bool enabled = false
    int count = default
    int count = default(int)

A parameter without an "= expression" clause does not qualify.
Attributes alone do not qualify.

Diagnostic:
- ID: PARAMDEFAULT001
- Title: Parameter has an explicit default value
- Category: Practice
- Severity: Warning
- Enabled by default: true
- Message:
  Parameter '{parameterName}' in method '{methodName}' has an explicit default value

Diagnostic location:
- Highlight only the parameter's identifier.
- Do not highlight its type, default expression, or the whole parameter.
- For an escaped identifier such as @event, highlight the entire @event
  token, but use "event" in the diagnostic message.
- Use the method's identifier name without type parameters or its
  containing type's name.

Example:
    void Configure(int count = 5, bool enabled = false, string name = null)

Produces three diagnostics, located on:
    count
    enabled
    name

Success:
Every qualifying parameter produces exactly one diagnostic with the
specified message and identifier location. Other parameters produce none.
*/
namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson07ParametersTypeParametersConstraints.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "PARAMDEFAULT001",
        title: "Parameter has an explicit default value",
        messageFormat:
            "Parameter '{0}' in method '{1}' has an explicit default value",
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
                if (item.Default is not null)
                {
                    Location location = item.GetIdentifierLocation;
                    ParameterDiagnosticData data = new()
                    {
                        MethodName = node.Name,
                        ParameterName = item.Name
                    };
                    Diagnostic d = CreateDiagnostic(_rule, location, data);
                    context.ReportDiagnostic(d);
                }
                
            }
        }, SyntaxKind.MethodDeclaration);

        context.EnableConcurrentExecution();

    }

    private sealed class ParameterDiagnosticData
    {
        public string ParameterName { get; set; } = "";
        public string MethodName { get; set; } = "";
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
            data.MethodName);
    }
}