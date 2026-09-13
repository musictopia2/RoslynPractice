/*
Enter the requirements for this exercise here.
A client wants a compilation-level diagnostic that records information that another Roslyn component could inspect later.

Create an analyzer that reports one diagnostic per compilation.

The diagnostic descriptor contract is:

ID: PROPERTY105
Title: Project Analysis Metadata
Message: Project '{0}' analysis metadata was recorded
Category: DiagnosticProperties
Severity: Info
Enabled by default: true

The client requires the diagnostic to contain exactly these four properties:

DataKind
AssemblyName
SourceFileCount
HasEntryPoint

Their required values are:

DataKind = ProjectAnalysis
AssemblyName = compilation assembly name, or Unknown if null
SourceFileCount = number of syntax trees converted to text
HasEntryPoint = Yes if the compilation has an entry point; otherwise No

The diagnostic message's {0} must also use the assembly name, including "Unknown" when the compilation assembly name is null.

Report the diagnostic at:

Location.None
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;
using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("PROPERTY105")
        .WithTitle("Project Analysis Metadata")
        .WithMessage("Project '{0}' analysis metadata was recorded")
        .BuildWithDiagnosticPropertiesCategory(DiagnosticSeverity.Info);

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
            string assemblyName =
                context.Compilation.AssemblyName ?? "Unknown";
            list.Add("DataKind", "ProjectAnalysis");
            list.Add("AssemblyName", assemblyName);
            int count = context.Compilation.SyntaxTrees.Count();
            list.Add("SourceFileCount", count.ToString());
            list.Add("HasEntryPoint", hadEntry);
            
            var properties = list.ToImmutableDictionary();
            
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, properties, assemblyName);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}