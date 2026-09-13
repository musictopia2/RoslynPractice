/*
Enter the requirements for this exercise here.
Create an analyzer that reports one warning diagnostic at a specific
text span within the third syntax tree in the compilation.

Diagnostic requirements:

ID:
LOCATION104

Title:
Selected File Span

Message:
Syntax tree {0} contains the selected span from {1} through {2}

Category:
Locations

Severity:
Warning

Enabled by default:
Yes

Analyzer behavior:

1. Do not analyze generated code.

2. Enable concurrent execution.

3. Register a compilation action.

4. Use syntax tree index 2.

5. If the compilation contains fewer than 3 syntax trees,
   do not report a diagnostic.

6. Obtain the SyntaxTree at index 2.

7. Create a TextSpan with:
   Start = 3
   Length = 5

8. Obtain the Location by calling SyntaxTree.GetLocation(TextSpan).

9. Create one diagnostic at that Location.

10. Supply these message arguments:

    {0} = 2

    {1} = the TextSpan Start value

    {2} = the TextSpan End value

11. Report the diagnostic through the compilation analysis context.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson02DiagnosticLocations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("LOCATION104")
       .WithTitle("Selected File Span")
       .WithMessage("Syntax tree {0} contains the selected span from {1} through {2}")
       .WithCategory("Locations")
       .WithSeverity(DiagnosticSeverity.Warning).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    private Location? GetLocation(Compilation compilation, TextSpan span)
    {
        if (compilation.SyntaxTrees.Count() is < 3)
        {
            return null;
        }
        var tree = compilation.SyntaxTrees.ElementAt(2);
        //TextSpan span = new(3, 5);
        return tree.GetLocation(span);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            TextSpan span = new(3, 5);
            Location? location = GetLocation(context.Compilation, span);
            if (location is not null)
            {
                Diagnostic d = Diagnostic.Create(OnlyRule, location, 2, span.Start, span.End);
                context.ReportDiagnostic(d);
            }

        });
        context.EnableConcurrentExecution();
    }
}