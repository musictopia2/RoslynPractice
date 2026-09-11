/*
Enter the requirements for this exercise here.
Requirements

Create a private static readonly DiagnosticDescriptor field named:

Rule

The descriptor must use these exact values:

Diagnostic ID: RPA001
Title: Analyzer practice diagnostic
Message: Analyzer practice diagnostic was reported
Category: Practice
Default severity: Warning
Enabled by default: true

Then change SupportedDiagnostics so that it contains the Rule descriptor.

Restrictions

For this exercise:

Keep [DiagnosticAnalyzer(LanguageNames.CSharp)].
Keep the existing Initialize method unchanged.
Do not register any analysis actions.
Do not report the diagnostic yet.
There should be exactly one supported diagnostic.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson02DiagnosticAttributeSupportedDiagnostics.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    //ai generated so i can learn.
    private static DiagnosticDescriptor DemoRule => new(
    id: "DEMO001",
    title: "Demo diagnostic",
    messageFormat: "This is a demonstration diagnostic",
    category: "Demo",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);
    //my work.
    private static DiagnosticDescriptor Rule { get; } = new(
    id: "RPA001",
    title: "Analyzer practice diagnostic",
    messageFormat: "Analyzer practice diagnostic was reported",
    category: "Practice",
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [Rule];
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();
    }
    
}