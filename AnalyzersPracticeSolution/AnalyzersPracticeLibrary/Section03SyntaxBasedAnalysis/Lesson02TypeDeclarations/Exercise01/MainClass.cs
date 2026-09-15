/*
Enter the requirements for this exercise here.
Scenario

A client wants an analyzer that reports every class declaration so they can verify that their tooling correctly discovers classes before adding more complicated class rules later.

Starting with the code you posted, add the following supplied diagnostic infrastructure. You do not need to design or reconstruct this plumbing. That follows the new course rule that previously learned diagnostic machinery shouldn't distract from the syntax concept.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise05;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson02TypeDeclarations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor ClassRule { get; } = new(
    id: "SYNTAX301",
    title: "Class Declaration Found",
    messageFormat: "Class '{0}' was found",
    category: "Syntax",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class ClassDiagnosticData
    {
        public string ClassName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ClassName);
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [ClassRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            ClassDeclarationSyntax node = context.ToClassSyntax;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            ClassDiagnosticData data = new()
            {
                ClassName = name
            };
            Diagnostic d = CreateDiagnostic(ClassRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.ClassDeclaration);



        context.EnableConcurrentExecution();
    }
}