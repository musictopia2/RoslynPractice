/*
Enter the requirements for this exercise here.
Create an analyzer that reports a diagnostic identifying a preview
span within a selected source file.

Diagnostic contract:

ID:
LOCATION105

Title:
Source Preview Location

Message:
Source file {0} has a preview starting at {1} with length {2}

Category:
ClientLocations

Severity:
Warning

Enabled by default:
Yes


Client requirements:

The client wants to inspect syntax tree index 1.

The preview begins at character position 4 and has a length of 6.

Report exactly one diagnostic whose source location represents that
preview span.

The diagnostic message must identify:

- the syntax tree index;
- the preview's starting position;
- the preview's length.

If syntax tree index 1 does not exist, report no diagnostic.

If the selected syntax tree does not contain enough source text for
the entire requested preview span, report no diagnostic.

Do not analyze generated code.

Enable concurrent execution.


Implementation requirements:

Use a compilation action.

Use TextSpan to represent the preview.

Use SyntaxTree.GetLocation(TextSpan) to create the diagnostic location.

Do not use Location.None as the location of a reported diagnostic.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson02DiagnosticLocations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("LOCATION105")
       .WithTitle("Source Preview Location")
       .WithMessage("Source file {0} has a preview starting at {1} with length {2}")
       .WithCategory("ClientLocations")
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
        if (compilation.SyntaxTrees.Count() is < 2)
        {
            return null;
        }
        var tree = compilation.SyntaxTrees.ElementAt(1);
        var text = tree.GetText();
        int length = text.Length;
        if (length < span.End)
        {
            return null;
        }
        return tree.GetLocation(span);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            TextSpan span = new(4, 6);
            
            Location? location = GetLocation(context.Compilation, span);
            if (location is not null)
            {
                Diagnostic d = Diagnostic.Create(OnlyRule, location, 1, span.Start, span.Length);
                context.ReportDiagnostic(d);
            }

        });
        context.EnableConcurrentExecution();
    }
}