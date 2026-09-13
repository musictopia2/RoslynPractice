/*
Create one diagnostic rule with the following descriptor information:

ID:
REVIEW304

Title:
Compilation Diagnostic Summary

Message:
Compilation '{0}' contains {1} syntax trees and is classified as '{2}'

Category:
DiagnosticReview

Severity:
Warning

Enabled by default:
Yes


Register a compilation action.

When the compilation contains fewer than three syntax trees:
- Do not report a diagnostic.

When the compilation contains at least three syntax trees:
- Report exactly one diagnostic.


Classification:

3 or 4 syntax trees:
Medium

5 or more syntax trees:
Large


Primary location:
- Use the root location of the first syntax tree.
- Do not use Location.None.


Additional locations:
- Include exactly two additional locations.
- The first additional location must be the root location
  of the second syntax tree.
- The second additional location must be the root location
  of the third syntax tree.


Message arguments:
- {0} = compilation assembly name.
- {1} = total number of syntax trees.
- {2} = calculated classification.

If the assembly name is null:
- Use "Unknown".


Diagnostic properties:

Key: TreeCount
Value: total syntax-tree count converted to a string

Key: Classification
Value: calculated classification

Key: PrimaryFile
Value: FilePath of the first syntax tree

Key: RelatedFileCount
Value: "2"


SupportedDiagnostics must contain the rule.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("REVIEW304")
        .WithTitle("Compilation Diagnostic Summary")
        .WithMessage("Compilation '{0}' contains {1} syntax trees and is classified as '{2}'")
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
        if (compilation.SyntaxTrees.Count() is < 3)
        {
            return null;
        }
        var tree = compilation.SyntaxTrees.First();
        var node = tree.GetRoot();
        return node.GetLocation();
    }
    private List<Location> GetSecondaryLocations(Compilation compilation)
    {
        List<Location> output = [];
        2.Times(x =>
        {
            var tree = compilation.SyntaxTrees.ElementAt(x);
            var node = tree.GetRoot();
            output.Add(node.GetLocation());
        });
        return output;
    }
    private static string GetSize(int count)
    {
        if (count is < 5)
        {
            return "Medium";
        }
        return "Large";
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
            int count = context.Compilation.SyntaxTrees.Count();

            string size = GetSize(count);

            list.Add("TreeCount", count.ToString());
            list.Add("Classification", size);
            list.Add("PrimaryFile", firstPath);
            list.Add("RelatedFileCount", "2");

            var properties = list.ToImmutableDictionary();
            var secondaryLocations = GetSecondaryLocations(context.Compilation);
            Diagnostic d = Diagnostic.Create(OnlyRule, primary, secondaryLocations, properties, assemblyName, count, size);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}