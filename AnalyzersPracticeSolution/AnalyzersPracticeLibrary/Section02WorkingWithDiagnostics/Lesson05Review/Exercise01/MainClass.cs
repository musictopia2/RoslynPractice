/*
Create one diagnostic rule with the following descriptor information:

ID:
REVIEW301

Title:
Compilation Source Summary

Message:
Compilation '{0}' contains {1} syntax trees

Category:
DiagnosticReview

Severity:
Warning

Enabled by default:
Yes


Register a compilation action.

When the compilation contains no syntax trees:
- Do not report a diagnostic.

When the compilation contains at least one syntax tree:
- Report exactly one diagnostic.
- The primary location must be the root location of the first syntax tree.
- Message argument {0} must be the compilation's assembly name.
- Message argument {1} must be the total number of syntax trees.

Attach a diagnostic property with:

Key:
TreeCount

Value:
The total number of syntax trees converted to a string.


SupportedDiagnostics must contain the rule.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("REVIEW301")
        .WithTitle("Compilation Source Summary")
        .WithMessage("Compilation '{0}' contains {1} syntax trees")
        .BuildWithDiagnosticPropertiesCategory(DiagnosticSeverity.Warning);

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
            Dictionary<string, string?> list = [];
            string assemblyName =
                context.Compilation.AssemblyName ?? "Unknown";


            int count = context.Compilation.SyntaxTrees.Count();
            if (count is < 1)
            {
                return;
            }
            list.Add("TreeCount", count.ToString());

            var properties = list.ToImmutableDictionary();
            //now needs single location.
            Location location = GetLocation(context.Compilation);
            Diagnostic d = Diagnostic.Create(OnlyRule, location, properties, assemblyName, count);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}