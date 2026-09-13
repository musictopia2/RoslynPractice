/*
Create one diagnostic rule with the following descriptor information:

ID:
REVIEW303

Title:
Compilation Size Classification

Message:
Compilation '{0}' is classified as '{1}'

Category:
DiagnosticReview

Severity:
Info

Enabled by default:
Yes


Register a compilation action.

When the compilation contains no syntax trees:
- Do not report a diagnostic.

When the compilation contains at least one syntax tree:
- Report exactly one diagnostic.

Primary location:
- Use the root location of the first syntax tree.
- Do not use Location.None.

Determine a classification from the number of syntax trees:

1 or 2 syntax trees:
Small

3 or 4 syntax trees:
Medium

5 or more syntax trees:
Large


Message arguments:
- {0} = compilation assembly name.
- {1} = calculated classification.

If the assembly name is null:
- Use "Unknown".


Diagnostic properties:

Add all three properties:

Key: TreeCount
Value: total syntax-tree count converted to a string

Key: Classification
Value: the calculated classification

Key: AssemblyName
Value: the same assembly name used for message argument {0}


SupportedDiagnostics must contain the rule.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("REVIEW303")
        .WithTitle("Compilation Size Classification")
        .WithMessage("Compilation '{0}' is classified as '{1}'")
        .BuildWithDiagnosticPropertiesCategory(DiagnosticSeverity.Info);

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
    private static string GetSize(int count)
    {
        if (count is < 3)
        {
            return "Small";
        }
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


            int count = context.Compilation.SyntaxTrees.Count();
            if (count is < 1)
            {
                return;
            }

            string size = GetSize(count);

            list.Add("TreeCount", count.ToString());
            list.Add("Classification", size);
            list.Add("AssemblyName", assemblyName);

            var properties = list.ToImmutableDictionary();
            //now needs single location.
            Location location = GetLocation(context.Compilation);
            Diagnostic d = Diagnostic.Create(OnlyRule, location, properties, assemblyName, size);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}