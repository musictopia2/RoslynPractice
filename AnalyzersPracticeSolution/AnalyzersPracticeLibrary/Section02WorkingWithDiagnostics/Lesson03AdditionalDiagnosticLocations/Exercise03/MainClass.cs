/*
Enter the requirements for this exercise here.
Requirements
Register a compilation action.
If there are fewer than 2 syntax trees, report nothing.
Find the first class declaration in syntax tree 0.
Find the first class declaration in syntax tree 1.
If either tree does not contain a class declaration, report nothing.
Use the first class declaration's location as the primary location.
Use the second class declaration's location as the only additional location.
Report exactly one diagnostic.

Conceptually:

Diagnostic ADDLOC003
├── Location
│   └── first class declaration in tree 0
│
└── AdditionalLocations
    └── first class declaration in tree 1


ID: ADDLOC003
Title: Related Class Declarations
Message: Two related class declarations were found
Category: DiagnosticLocations
Severity: Warning
Enabled by default: true

This exercise is deliberately different from the first two: the diagnostic locations should no longer cover the entire syntax-tree roots. They should cover the actual class declarations.
*/

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson03AdditionalDiagnosticLocations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("ADDLOC003")
       .WithTitle("Related Class Declarations")
       .WithMessage("Two related class declarations were found")
       .WithCategory("DiagnosticLocations")
       .WithSeverity(DiagnosticSeverity.Warning).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    private Location? GetPrimaryLocation(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() is < 2)
        {
            return null;
        }
        var tree = compilation.SyntaxTrees.First();
        var root = tree.GetRoot();

        ClassDeclarationSyntax? classDeclaration =
            root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
        if (classDeclaration is null)
        {
            return null;
        }
        return classDeclaration.GetLocation();
    }
    private List<Location> GetSecondaryLocations(Compilation compilation)
    {
        var tree = compilation.SyntaxTrees.ElementAt(1);
        var root = tree.GetRoot();


        ClassDeclarationSyntax? classDeclaration =
           root.DescendantNodes()
               .OfType<ClassDeclarationSyntax>()
               .FirstOrDefault();
        if (classDeclaration is null)
        {
            return [];
        }
        return [classDeclaration.GetLocation()];
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            Location? primary = GetPrimaryLocation(context.Compilation);
            if (primary is not null)
            {
                var secondary = GetSecondaryLocations(context.Compilation);
                if (secondary.Count == 0)
                {
                    return;
                }
                Diagnostic d = Diagnostic.Create(OnlyRule, primary, secondary);
                context.ReportDiagnostic(d);
            }
        });
        context.EnableConcurrentExecution();
    }
}