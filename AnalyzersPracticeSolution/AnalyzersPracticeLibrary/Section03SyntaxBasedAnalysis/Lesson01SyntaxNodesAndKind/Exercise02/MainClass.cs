/*
Enter the requirements for this exercise here.



ID: SYNTAX102
Title: Method Declaration Found
Category: SyntaxAnalysis
Severity: Info
Enabled by default: true

Message:
Method '{0}' was found
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("SYNTAX102")
        .WithTitle("Method Declaration Found")
        .WithMessage("Method '{0}' was found")
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
            MethodDeclarationSyntax node = context.ToMethodSyntax;
            string name = node.Name;
            Location location = node.GetLocation();
            Diagnostic d = Diagnostic.Create(OnlyRule, location, name);
            context.ReportDiagnostic(d);
        }, SyntaxKind.MethodDeclaration);
        context.EnableConcurrentExecution();
    }
}