/*
Enter the requirements for this exercise here.
Put the following requirements in your comment block.

Your analyzer must report one diagnostic for the compilation.

Use these exact descriptor values:

ID: PROPERTY101
Title: Compilation Property Information
Message: Compilation '{0}' has diagnostic property data
Category: DiagnosticProperties
Severity: Info
Enabled by default: true

Register a compilation action.

When that action runs:

Obtain the compilation's assembly name.
If the assembly name is null, use "Unknown" instead.
Create an immutable diagnostic-property dictionary containing exactly these two entries:
DataKind = Compilation
AssemblyName = the compilation assembly name
Create the diagnostic using:
your descriptor;
Location.None;
the property dictionary;
the assembly name as the {0} message argument.
Report the diagnostic.

For this exercise, you must use the Diagnostic.Create overload that accepts an ImmutableDictionary of diagnostic properties. That restriction exists because diagnostic properties are the concept being practiced.

You do not need additional locations for this exercise.
*/

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("PROPERTY101")
        .WithTitle("Compilation Property Information")
        .WithMessage("Compilation '{0}' has diagnostic property data")
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

            string assembyName;
            if (context.Compilation.AssemblyName is null)
            {
                assembyName = "Unknown";
            }
            else
            {
                assembyName = context.Compilation.AssemblyName;
            }
            var properties = assembyName.CreateImmmutableDictionary("DataKind", "Compilation", "AssemblyName");
            Diagnostic d = Diagnostic.Create(OnlyRule, Location.None, properties, assembyName);
            context.ReportDiagnostic(d);
        });
        context.EnableConcurrentExecution();
    }
}