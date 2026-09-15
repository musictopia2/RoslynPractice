/*
Enter the requirements for this exercise here.
Register analysis for:

SyntaxKind.InterfaceDeclaration

Every interface produces exactly one diagnostic.

For:

public interface ICustomer
{
}

the message must be exactly:

Interface 'ICustomer' has accessibility 'Public'

If the interface does not contain the public modifier, use:

NonPublic

Therefore:

interface IService
{
}

produces:

Interface 'IService' has accessibility 'NonPublic'
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson02TypeDeclarations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor InterfaceRule { get; } = new(
    id: "SYNTAX303",
    title: "Interface Declaration Found",
    messageFormat: "Interface '{0}' has accessibility '{1}'",
    category: "Syntax",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class InterfaceDiagnosticData
    {
        public string InterfaceName { get; set; } = "";
        public string Accessibility { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        InterfaceDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.InterfaceName,
            data.Accessibility);
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [InterfaceRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            InterfaceDeclarationSyntax node = context.ToInterfaceSyntax;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            string accessible = node.IsPublic ? "Public" : "NonPublic";
            InterfaceDiagnosticData data = new()
            {
                InterfaceName = name,
                Accessibility = accessible
            };
            Diagnostic d = CreateDiagnostic(InterfaceRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.InterfaceDeclaration);
        context.EnableConcurrentExecution();
    }
}