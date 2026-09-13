/*
Enter the requirements for this exercise here.
Create an analyzer that reports one informational diagnostic at a
specific text span within the first syntax tree.

Diagnostic requirements:

ID:
LOCATION103

Title:
Source Text Span Location

Message:
The selected source span starts at {0} and has length {1}

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

4. If the compilation contains no syntax trees, do not report
   a diagnostic.

5. Obtain the first SyntaxTree.

6. Create a TextSpan with:
   Start = 2
   Length = 4

7. Use SyntaxTree.GetLocation(TextSpan) to obtain the Location.

8. Create one diagnostic at that Location.

9. Supply the span's Start and Length as the message arguments.

10. Report the diagnostic through the compilation analysis context.
*/


namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson02DiagnosticLocations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("LOCATION103")
       .WithTitle("Source Text Span Location")
       .WithMessage("The selected source span starts at {0} and has length {1}")
       .WithCategory("Locations")
       .WithSeverity(DiagnosticSeverity.Info).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    private Location? GetLocation(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() == 0)
        {
            return null;
        }
        var tree = compilation.SyntaxTrees.First();
        TextSpan span = new(2, 4);
        return tree.GetLocation(span);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            Location? location = GetLocation(context.Compilation);
            if (location is not null)
            {
                Diagnostic d = Diagnostic.Create(OnlyRule, location, 2, 4);
                context.ReportDiagnostic(d);
            }

        });
        context.EnableConcurrentExecution();
    }
}