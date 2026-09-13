/*
Enter the requirements for this exercise here.
his time, practice the same API shape again, but with more than one additional location.

Create an analyzer that reports one diagnostic when a compilation contains at least three syntax trees.

Use this descriptor information:

ID: ADDLOC002
Title: Multiple Related Files
Message: Compilation contains three related source files
Category: DiagnosticLocations
Severity: Warning
Enabled by default: true

In Initialize, keep the existing generated-code configuration, keep concurrent execution enabled, and register a compilation action.

Inside the compilation action:

If the compilation contains fewer than 3 syntax trees, report no diagnostic.
Use the root location of the first syntax tree as the primary diagnostic location.
Use the root locations of the second and third syntax trees as additional locations.
Create one diagnostic.
Report it.

The resulting diagnostic should conceptually look like this:

Diagnostic ADDLOC002
├── Location
│   └── first syntax tree
│
└── AdditionalLocations
    ├── second syntax tree
    └── third syntax tree
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson03AdditionalDiagnosticLocations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("ADDLOC002")
       .WithTitle("Multiple Related Files")
       .WithMessage("Compilation contains three related source files")
       .WithCategory("DiagnosticLocations")
       .WithSeverity(DiagnosticSeverity.Warning).EnabledByDefault().Build();
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
        context.RegisterDiagnosticRuleWithSeveralLocations(OnlyRule, 0, 1, 2);
        context.EnableConcurrentExecution();
    }
}