/*
Scenario

A client wants a lightweight compilation summary diagnostic for an
internal analysis tool.

They want the message to describe the compilation using information
available from the Compilation supplied by Roslyn.

Diagnostic rule

ID:
CLIENT301

Title:
Compilation Analysis Summary

Message format:
Compilation '{0}' has {1} syntax trees, {2} references, and is '{3}'

Category:
ClientReview

Severity:
DiagnosticSeverity.Info

Enabled by default:
true

Requirements

SupportedDiagnostics must contain exactly this rule.

Configure generated-code analysis using:

GeneratedCodeAnalysisFlags.None

Enable concurrent execution.

Register a compilation analysis callback.

Use the Compilation supplied by CompilationAnalysisContext to obtain:

1. The compilation's assembly name.
2. The number of syntax trees.
3. The number of metadata references.

Determine a size description using the syntax-tree count:

0 or 1 syntax trees:
"Small"

2 or 3 syntax trees:
"Medium"

4 or more syntax trees:
"Large"

Create and report exactly one diagnostic using:

Location.None

Supply the following message arguments in this exact order:

1. Assembly name
2. Syntax-tree count
3. Metadata-reference count
4. Size description

Examples

A compilation named:

AccountingLibrary

with:

1 syntax tree
2 references

must report:

Compilation 'AccountingLibrary' has 1 source files, 2 references, and is 'Small'


A compilation named:

ShippingLibrary

with:

3 syntax trees
4 references

must report:

Compilation 'ShippingLibrary' has 3 source files, 4 references, and is 'Medium'


A compilation named:

InventorySystem

with:

6 syntax trees
5 references

must report:

Compilation 'InventorySystem' has 6 source files, 5 references, and is 'Large'

Do not hard-code any compilation-specific values.

Do not manually construct the final diagnostic message.

Use Diagnostic.Create and Roslyn's message-argument mechanism.

Do not use syntax-node, symbol, semantic-model, or operation actions.
*/
namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson01DiagnosticMessageArguments.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("CLIENT301")
        .WithTitle("Compilation Analysis Summary")
        .WithMessage("Compilation '{0}' has {1} syntax trees, {2} references, and is '{3}'")
        .WithCategory("ClientReview")
        .WithSeverity(DiagnosticSeverity.Info).EnabledByDefault().Build();
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
            string info = GetInfo(count);
            context.ReportDiagnosticWithNoneLocation(OnlyRule, context.Compilation.AssemblyName!, context.Compilation.SyntaxTrees.Count(), 
                context.Compilation.References.Count(), info);
        });
        context.EnableConcurrentExecution();
    }
    private static string GetInfo(int count)
    {
        if (count is <= 1)
        {
            return "Small";
        }
        if (count is >=2 and <= 3)
        {
            return "Medium";
        }
        return "Large";
    }
}