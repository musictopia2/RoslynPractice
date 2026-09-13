/*
Enter the requirements for this exercise here.
Requirements

Create an analyzer that reports one diagnostic when a compilation contains at least two syntax trees.

Use this descriptor information:

ID: ADDLOC001
Title: Multiple Source Files
Message: Compilation contains multiple source files
Category: DiagnosticLocations
Severity: Info
Enabled by default: true

In Initialize:

Keep the existing generated-code configuration.
Keep concurrent execution enabled.
Register a compilation action.

Inside the compilation action:

Obtain the compilation's syntax trees.
If the compilation contains fewer than 2 syntax trees, report no diagnostic.
Use the root location of the first syntax tree as the diagnostic's primary location.
Use the root location of the second syntax tree as the diagnostic's only additional location.
Create one diagnostic using those locations.
No diagnostic properties are required, so the properties argument should contain no properties.
Report the diagnostic.

The resulting diagnostic should conceptually contain:

Diagnostic ADDLOC001
├── Location
│   └── first syntax tree
│
└── AdditionalLocations
    └── second syntax tree

Do not report two separate diagnostics. The point of this exercise is specifically to practice attaching a second relevant location to the same Diagnostic.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson03AdditionalDiagnosticLocations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("ADDLOC001")
       .WithTitle("Multiple Source Files")
       .WithMessage("Compilation contains multiple source files")
       .WithCategory("DiagnosticLocations")
       .WithSeverity(DiagnosticSeverity.Info).EnabledByDefault().Build();
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
        var node = tree.GetRoot();
        return node.GetLocation();
    }
    private List<Location> GetSecondaryLocations(Compilation compilation)
    {
        var tree = compilation.SyntaxTrees.ElementAt(1);
        var node = tree.GetRoot();
        Location location = node.GetLocation();
        return [location];
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
                Diagnostic d = Diagnostic.Create(OnlyRule, primary,secondary);
                context.ReportDiagnostic(d);
            }
        });
        context.EnableConcurrentExecution();
    }
}