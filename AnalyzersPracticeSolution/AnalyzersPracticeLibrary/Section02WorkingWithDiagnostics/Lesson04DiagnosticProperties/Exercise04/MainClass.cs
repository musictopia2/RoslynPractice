/*
Enter the requirements for this exercise here.

Register a compilation action.

When the action runs:

Obtain the compilation assembly name.
If the assembly name is null, use "Unknown".
Count the compilation's syntax trees.
Determine whether the compilation has an entry point using:
context.Compilation.GetEntryPoint(
    context.CancellationToken)

Remember that CompilationAnalysisContext provides:

context.CancellationToken

so you do not need to use default.

Create diagnostic properties containing exactly these three entries:
DataKind = CompilationSummary
HasEntryPoint = Yes or No
TreeCount = syntax-tree count converted to text
Create one diagnostic using:
OnlyRule;
Location.None;
the property dictionary;
assembly name as {0};
syntax-tree count as {1}.
Report the diagnostic.


ID: PROPERTY104
Title: Compilation Summary Properties
Message: Compilation '{0}' contains {1} syntax trees
Category: DiagnosticProperties
Severity: Warning
Enabled by default: true
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;
using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("PROPERTY104")
        .WithTitle("Compilation Summary Properties")
        .WithMessage("Compilation '{0}' contains {1} syntax trees")
        .BuildWithDiagnosticPropertiesCategory(DiagnosticSeverity.Warning);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            Dictionary<string, string?> list = [];
            var method = context.Compilation.GetEntryPoint(context.CancellationToken);
            string hadEntry = method is null ? "No" : "Yes";
            list.Add("DataKind", "CompilationSummary");
            int count = context.Compilation.SyntaxTrees.Count();
            list.Add("HasEntryPoint", hadEntry);
            list.Add("TreeCount", count.ToString());
            var properties = list.ToImmutableDictionary();
            string assemblyName =
                context.Compilation.AssemblyName ?? "Unknown";
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, properties, assemblyName, count);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}