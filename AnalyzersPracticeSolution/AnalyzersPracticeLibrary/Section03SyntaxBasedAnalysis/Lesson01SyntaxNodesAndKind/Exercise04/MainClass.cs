/*
Enter the requirements for this exercise here.

Register for:

SyntaxKind.ClassDeclaration

For each class declaration, determine how many methods are declared directly in that class.

Report:

ID: SYNTAX104
Title: Class Method Count
Category: SyntaxAnalysis
Severity: Info
Enabled by default: true

Message:
Class '{0}' directly declares {1} methods

The arguments are:

{0} = class name
{1} = number of MethodDeclarationSyntax members directly in that class

For:

class Customer
{
    void Load()
    {
    }

    void Save()
    {
    }
}

report:

Class 'Customer' directly declares 2 methods

A class with no methods must still produce a diagnostic:

class Empty
{
}

reports:

Class 'Empty' directly declares 0 methods

ID: SYNTAX104
Title: Class Method Count
Category: SyntaxAnalysis
Severity: Info
Enabled by default: true

Message:
Class '{0}' directly declares {1} methods

{0} = class name
{1} = number of MethodDeclarationSyntax members directly in that class


ClassDeclarationSyntax
node.Name                 // your extension
node.Members
OfType<MethodDeclarationSyntax>()
Count()
node.GetLocation()
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("SYNTAX104")
        .WithTitle("Class Method Count")
        .WithMessage("Class '{0}' directly declares {1} methods")
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
            ClassDeclarationSyntax node = context.ToClassSyntax;
            string name = node.Name;
            Location location = node.GetLocation();
            var methods = node.Members.OfType<MethodDeclarationSyntax>().Count();
            Diagnostic d = Diagnostic.Create(OnlyRule, location, name, methods);
            context.ReportDiagnostic(d);
        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
    }
}