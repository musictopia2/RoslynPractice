/*
Enter the requirements for this exercise here.
Create an analyzer that reports one informational diagnostic at the
root location of a specific syntax tree in the compilation.

Diagnostic requirements:

ID:
LOCATION102

Title:
Selected Source File Location

Message:
The selected source file is at index {0}

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

4. Use syntax tree index 1.

5. If the compilation contains fewer than 2 syntax trees,
   do not report a diagnostic.

6. Obtain the SyntaxTree at index 1.

7. Obtain that SyntaxTree's root SyntaxNode.

8. Obtain the Location of that root SyntaxNode.

9. Create one diagnostic at that Location.

10. Supply 1 as the message argument for {0}.

11. Report the diagnostic through the compilation analysis context.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson02DiagnosticLocations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
   bb1.Create().WithId("LOCATION102")
   .WithTitle("Selected Source File Location")
   .WithMessage("The selected source file is at index {0}")
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
        if (compilation.SyntaxTrees.Count() is < 2)
        {
            return null;
        }

        var tree = compilation.SyntaxTrees.ElementAt(1);
        var node = tree.GetRoot();
        return node.GetLocation();
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
                Diagnostic d = Diagnostic.Create(
                    OnlyRule,
                    location,
                    1);

                context.ReportDiagnostic(d);
            }
        });

        context.EnableConcurrentExecution();
    }
}