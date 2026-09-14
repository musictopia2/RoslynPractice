/*
Enter the requirements for this exercise here.

The arguments are:

{0} = property name
{1} = property type as written in the source

For:

public string CustomerName { get; set; }

report:

Property 'CustomerName' has type 'string'

For:

public int OrderCount { get; set; }

report:

Property 'OrderCount' has type 'int'

The diagnostic location must be the entire PropertyDeclarationSyntax node.

If several properties exist—even across different classes—the analyzer reports one diagnostic for each.


ID: SYNTAX103
Title: Property Declaration Found
Category: SyntaxAnalysis
Severity: Info
Enabled by default: true

Message:
Property '{0}' has type '{1}'
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("SYNTAX103")
        .WithTitle("Property Declaration Found")
        .WithMessage("Property '{0}' has type '{1}'")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            PropertyDeclarationSyntax node = context.ToPropertySyntax;
            string name = node.Name;
            Location location = node.GetLocation();
            Diagnostic d = Diagnostic.Create(OnlyRule, location, name, node.Type.ToString());
            
            context.ReportDiagnostic(d);
        }, SyntaxKind.PropertyDeclaration);
        context.EnableConcurrentExecution();
    }
}