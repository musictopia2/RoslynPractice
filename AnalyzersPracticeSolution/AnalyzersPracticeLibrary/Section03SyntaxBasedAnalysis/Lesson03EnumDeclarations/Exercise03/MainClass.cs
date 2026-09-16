/*
Enter the requirements for this exercise here.
Exercise 03 — Explicit Enum Values

Now we'll use the EqualsValue property from the API introduction.

Requirement

Report one diagnostic for every enum declaration containing:

the enum's name;
the number of members that have an explicitly assigned value;
the total number of members.

For:

public enum EnumDifficulty
{
    Easy,
    Normal = 5,
    Hard,
    Extreme = 20
}

the diagnostic message must be:

Enum 'EnumDifficulty' has 2 explicitly assigned members out of 4 total members

The diagnostic location must be the enum identifier.

An enum with no explicit values still reports a diagnostic. For example:

public enum EnumDirection
{
    North,
    South,
    East,
    West
}

must report:

Enum 'EnumDirection' has 0 explicitly assigned members out of 4 total members

An empty enum also reports:

Enum 'EnumEmpty' has 0 explicitly assigned members out of 0 total members
What counts as explicitly assigned?

Remember from the API introduction that each:

EnumMemberDeclarationSyntax

has:

EqualsValue

with type:

EqualsValueClauseSyntax?

Therefore:

First

has no explicit value, while:

Second = 10

does.

You only need to determine whether EqualsValue exists. You do not need to inspect or evaluate the expression after =.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson03EnumDeclarations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
    bb1.Create()
        .WithId("SYNTAX303")
        .WithTitle("Explicit Enum Values")
        .WithMessage(
            "Enum '{0}' has {1} explicitly assigned members out of {2} total members")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);

    private sealed class EnumDiagnosticData
    {
        public string EnumName { get; set; } = "";
        public int ExplicitMemberCount { get; set; }
        public int TotalMemberCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        EnumDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.EnumName,
            data.ExplicitMemberCount,
            data.TotalMemberCount);
    }
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
        context.RegisterSyntaxNodeAction(context =>
        {
            EnumDeclarationSyntax node = context.ToEnumSyntax;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            var members = node.Members;
            
            var numberCount = members.Count(x => x.HasExplicitValue);
            EnumDiagnosticData data = new()
            {
                EnumName = name,
                TotalMemberCount = members.Count(),
                ExplicitMemberCount = numberCount
            };

            Diagnostic d = CreateDiagnostic(OnlyRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.EnumDeclaration);
        context.EnableConcurrentExecution();
    }
}