/*
Enter the requirements for this exercise here.
Create an analyzer that reports one diagnostic for the compilation.

Use these exact descriptor values:

ID: PROPERTY103
Title: Entry Point Property Information
Message: Compilation entry point information was recorded
Category: DiagnosticProperties
Severity: Info
Enabled by default: true

Register a compilation action.

When the action runs:

Obtain the compilation's entry point by calling:
context.Compilation.GetEntryPoint(
    context.CancellationToken)

This returns:

IMethodSymbol?
Create diagnostic properties containing exactly these two entries:
DataKind = EntryPoint
EntryPointName = entry point method name, or null when there is no entry point

If an entry point exists, use its Name property. For example, a normal Main method would result in:

EntryPointName = Main

If GetEntryPoint(...) returns null, the property must still exist, but its value must be null:

EntryPointName = null
Create one diagnostic using:
OnlyRule;
Location.None;
the property dictionary.
Report the diagnostic.
*/

using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;
using AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise01;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("PROPERTY103")
        .WithTitle("Entry Point Property Information")
        .WithMessage("Compilation entry point information was recorded")
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
            string? entryPoint;
            var method = context.Compilation.GetEntryPoint(context.CancellationToken);
            if (method is null)
            {
                entryPoint = null;
            }
            else
            {
                entryPoint = method.Name;
            }
            var properties = entryPoint.CreateImmmutableDictionary("DataKind", "EntryPoint", "EntryPointName");
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, properties);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}