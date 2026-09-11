/*
Enter the requirements for this exercise here.
This exercise introduces the reason SupportedDiagnostics is an array rather than just a single DiagnosticDescriptor.

Your analyzer supports two different diagnostic rules.

Create the first rule as:

ID: RPA003
Title: Naming review required
Message: This name requires review
Category: Naming
Severity: Warning
Enabled by default: true

Create the second rule as:

ID: RPA004
Title: Structure review required
Message: This code structure requires review
Category: Structure
Severity: Info
Enabled by default: true

Use your new guided DiagnosticDescriptorBuilder for both descriptors. Give the read-only properties the descriptive names:

NamingRule
StructureRule
*/

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson02DiagnosticAttributeSupportedDiagnostics.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor NamingRule { get; } =
        bb1.Create()
            .WithId("RPA003")
            .WithTitle("Naming review required")
            .WithMessage("This name requires review")
            .WithCategory("Naming")
            .WithSeverity(DiagnosticSeverity.Warning)
            .EnabledByDefault()
            .Build();
    private static DiagnosticDescriptor StructureRule { get; } =
        bb1.Create()
                .WithId("RPA004")
                .WithTitle("Structure review required")
                .WithMessage("This code structure requires review")
                .WithCategory("Structure")
                .WithSeverity(DiagnosticSeverity.Info)
                .EnabledByDefault()
                .Build();
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [NamingRule, StructureRule];
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();
    }
}