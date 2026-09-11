/*
Enter the requirements for this exercise here.
Your analyzer should support three diagnostics.

Create these rules:

ID: RPA005
Title: Performance review required
Message: This code may require performance review
Category: Performance
Severity: Warning
Enabled by default: true
ID: RPA006
Title: Readability review required
Message: This code may require readability review
Category: Readability
Severity: Info
Enabled by default: true
ID: RPA007
Title: Disabled experimental rule
Message: This experimental rule is currently disabled
Category: Experimental
Severity: Warning
Enabled by default: false

Use your guided builder for all three.

SupportedDiagnostics must advertise all three rules.

For Initialize, configure the analyzer so that:

Generated code is analyzed but diagnostics are not reported for generated code.
Concurrent execution is enabled.

So unlike the previous exercises, you need to think back to the GeneratedCodeAnalysisFlags work from Lesson 01.

Do not register any syntax, symbol, semantic, or operation analysis actions yet.
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson02DiagnosticAttributeSupportedDiagnostics.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [
                bb1.Create()
                .WithId("RPA005")
                .WithTitle("Performance review required")
                .WithMessage("This code may require performance review")
                .WithCategory("Performance")
                .WithSeverity(DiagnosticSeverity.Warning)
                .EnabledByDefault()
                .Build(),
                bb1.Create()
                .WithId("RPA006")
                .WithTitle("Readability review required")
                .WithMessage("This code may require readability review")
                .WithCategory("Readability")
                .WithSeverity(DiagnosticSeverity.Info)
                .EnabledByDefault().Build(),
                bb1.Create()
                .WithId("RPA007")
                .WithTitle("Disabled experimental rule")
                .WithMessage("This experimental rule is currently disabled")
                .WithCategory("Experimental")
                .WithSeverity(DiagnosticSeverity.Warning)
                .DisabledByDefault().Build()
                ];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze);

        context.EnableConcurrentExecution();
    }
}