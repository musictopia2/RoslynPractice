/*
CLIENT REQUIREMENT

The client wants every compilation containing at least four source files
to produce one source-package review diagnostic.

Create the following diagnostic rule.

ID:
CLIENT305

Title:
Source Package Review

Message:
Package '{0}' contains {1} source files, is classified as '{2}', and has {3} related review files

Category:
DiagnosticReview

Severity:
Warning

Enabled by default:
Yes


ANALYSIS

Register a compilation action.

If the compilation contains fewer than four syntax trees:
- Do not report a diagnostic.

If the compilation contains four or more syntax trees:
- Report exactly one diagnostic.


PACKAGE NAME

Use the compilation assembly name as the package name.

If the assembly name is null:
- Use "Unknown".


CLASSIFICATION

Determine the classification from the total syntax-tree count.

4 or 5 syntax trees:
Standard

6 or 7 syntax trees:
Expanded

8 or more syntax trees:
Large


PRIMARY LOCATION

The diagnostic must have a real primary source location.

Use:
- The root location of the first syntax tree.

Do not use Location.None.


ADDITIONAL LOCATIONS

The diagnostic must contain exactly three additional locations.

In this exact order:

1. Root location of the second syntax tree
2. Root location of the third syntax tree
3. Root location of the fourth syntax tree


MESSAGE ARGUMENTS

Supply the message arguments in this order:

{0}
Package name

{1}
Total syntax-tree count

{2}
Calculated classification

{3}
The number 3


DIAGNOSTIC PROPERTIES

Attach all of the following properties.

Key:
PackageName

Value:
The same package name used for message argument {0}


Key:
TreeCount

Value:
The total syntax-tree count converted to a string


Key:
Classification

Value:
The same calculated classification used for message argument {2}


Key:
PrimaryFile

Value:
FilePath of the first syntax tree


Key:
FirstRelatedFile

Value:
FilePath of the second syntax tree


Key:
LastRelatedFile

Value:
FilePath of the fourth syntax tree


Key:
RelatedFileCount

Value:
"3"


SUPPORTED DIAGNOSTICS

SupportedDiagnostics must contain the CLIENT305 rule.


CLIENT ACCEPTANCE EXAMPLES

3 syntax trees:
No diagnostic

4 syntax trees:
Classification = Standard
3 additional locations

5 syntax trees:
Classification = Standard
3 additional locations

6 syntax trees:
Classification = Expanded
3 additional locations

7 syntax trees:
Classification = Expanded
3 additional locations

8 syntax trees:
Classification = Large
3 additional locations

10 syntax trees:
Classification = Large
3 additional locations


The client expects all message data, properties, and locations to describe
the same compilation consistently.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("CLIENT305")
        .WithTitle("Source Package Review")
        .WithMessage("Package '{0}' contains {1} source files, is classified as '{2}', and has {3} related review files")
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
        if (compilation.SyntaxTrees.Count() is < 4)
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
        3.Times(x =>
        {
            var tree = compilation.SyntaxTrees.ElementAt(x);
            var node = tree.GetRoot();
            output.Add(node.GetLocation());
        });
        return output;
    }
    private static string GetSize(int count)
    {
        if (count is < 6)
        {
            return "Standard";
        }
        if (count is < 8)
        {
            return "Expanded";
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
            string finalPath = trees.ElementAt(3).FilePath;
            int count = context.Compilation.SyntaxTrees.Count();

            string size = GetSize(count);
            list.Add("PackageName", assemblyName);
            list.Add("TreeCount", count.ToString());
            list.Add("Classification", size);
            list.Add("PrimaryFile", firstPath);
            list.Add("FirstRelatedFile", secondPath);
            list.Add("LastRelatedFile", finalPath);
            list.Add("RelatedFileCount", "3");

            var properties = list.ToImmutableDictionary();
            var secondaryLocations = GetSecondaryLocations(context.Compilation);
            Diagnostic d = Diagnostic.Create(OnlyRule, primary, secondaryLocations, properties, assemblyName, count, size, 3);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}