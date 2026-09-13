/*
Enter the requirements for this exercise here.
Exercise 02 — Reinforcement

Create an analyzer that reports one diagnostic for the compilation.

Use these exact descriptor values:

ID: PROPERTY102
Title: Source File Property Information
Message: Compilation contains {0} source files
Category: DiagnosticProperties
Severity: Warning
Enabled by default: true

Register a compilation action.

When the action runs:

Determine the number of syntax trees in the compilation.
Determine whether the compilation contains multiple source files:
"Yes" when the syntax-tree count is greater than 1.
"No" otherwise.
Create diagnostic properties containing exactly these two entries:
DataKind = SourceFiles
MultipleFiles = Yes or No
Create one diagnostic using:
OnlyRule;
Location.None;
the properties;
the syntax-tree count as the {0} message argument.
Report the diagnostic.

You may use your CreateImmutableDictionary helper for the two properties.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;
using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("PROPERTY102")
        .WithTitle("Source File Property Information")
        .WithMessage("Compilation contains {0} source files")
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
            int count = context.Compilation.SyntaxTrees.Count();
            string details = count is > 1 ? "Yes" : "No";
            var properties = details.CreateImmmutableDictionary("DataKind", "SourceFiles", "MultipleFiles");
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, properties, count);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}