/*
Create one diagnostic rule with the following descriptor information:

ID:
REVIEW302

Title:
Related Source Files

Message:
Compilation '{0}' links '{1}' with '{2}'

Category:
DiagnosticReview

Severity:
Warning

Enabled by default:
Yes


Register a compilation action.

When the compilation contains fewer than two syntax trees:
- Do not report a diagnostic.

When the compilation contains at least two syntax trees:
- Report exactly one diagnostic.

Primary location:
- The root location of the first syntax tree.

Additional locations:
- Include exactly one additional location.
- It must be the root location of the second syntax tree.

Message arguments:
- {0} = the compilation assembly name.
- {1} = the file path of the first syntax tree.
- {2} = the file path of the second syntax tree.

If the compilation assembly name is null:
- Use "Unknown".

Diagnostic properties:
- Add a property with key "RelatedFileCount".
- Its value must be "2".

SupportedDiagnostics must contain the rule.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("REVIEW302")
        .WithTitle("Related Source Files")
        .WithMessage("Compilation '{0}' links '{1}' with '{2}'")
        .BuildWithDiagnosticPropertiesCategory(DiagnosticSeverity.Warning);

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
            Dictionary<string, string?> list = [];
            string assemblyName =
                context.Compilation.AssemblyName ?? "Unknown";


            Location? primary = GetPrimaryLocation(context.Compilation);
            if (primary is null)
            {
                return;
            }
            var trees = context.Compilation.SyntaxTrees;
            string firstPath = trees.First().FilePath;
            string secondPath = trees.ElementAt(1).FilePath;
            list.Add("RelatedFileCount", "2");

            var properties = list.ToImmutableDictionary();
            var secondaryLocations = GetSecondaryLocations(context.Compilation);
            Diagnostic d = Diagnostic.Create(OnlyRule, primary, secondaryLocations, properties, assemblyName, firstPath, secondPath);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}