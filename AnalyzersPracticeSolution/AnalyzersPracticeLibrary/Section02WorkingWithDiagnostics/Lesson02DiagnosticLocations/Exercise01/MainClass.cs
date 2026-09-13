/*
Enter the requirements for this exercise here.
Create an analyzer that reports one informational diagnostic for the
first syntax tree in the compilation.

Diagnostic requirements:

ID:
LOCATION101

Title:
Source File Location

Message:
The compilation contains source code at this location

Category:
Locations

Severity:
Info

Enabled by default:
Yes

Analyzer behavior:

1. Do not analyze generated code.

2. Enable concurrent execution.

3. Register a compilation action.

4. Inside the compilation action, obtain the first SyntaxTree from
   context.Compilation.SyntaxTrees.

5. If the compilation does not contain any syntax trees, do not report
   a diagnostic.

6. Obtain the root SyntaxNode of the first SyntaxTree.

7. Obtain the Location of that root SyntaxNode by using GetLocation().

8. Create one diagnostic using that Location.

9. Report the diagnostic through the compilation analysis context.

The diagnostic must therefore have a real source location rather than
Location.None.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson02DiagnosticLocations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("LOCATION101")
       .WithTitle("Source File Location")
       .WithMessage("The compilation contains source code at this location")
       .WithCategory("Locations")
       .WithSeverity(DiagnosticSeverity.Info).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }

    private Location GetLocation(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() == 0)
        {
            return Location.None;
        }
        var tree = compilation.SyntaxTrees.First();
        var node = tree.GetRoot();
        return node.GetLocation();
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            Location location = GetLocation(context.Compilation);
            if (location != Location.None)
            {
                Diagnostic d = Diagnostic.Create(OnlyRule, location);
                context.ReportDiagnostic(d);
            }
            
        });
        context.EnableConcurrentExecution();
    }
}