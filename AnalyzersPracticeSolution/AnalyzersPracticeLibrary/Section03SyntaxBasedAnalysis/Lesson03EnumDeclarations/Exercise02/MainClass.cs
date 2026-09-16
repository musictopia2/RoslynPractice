/*
Enter the requirements for this exercise here.
Exercise 01 treated Members only as a collection to count. Now you'll actually inspect the individual EnumMemberDeclarationSyntax nodes.

Requirement

Report one diagnostic for every enum declaration containing:

the enum's name;
the first member's name;
the last member's name.

For:

public enum EnumDifficulty
{
    Easy,
    Normal,
    Hard
}

report:

Enum 'EnumDifficulty' starts with 'Easy' and ends with 'Hard'

The diagnostic location must again be the enum identifier.

For this exercise, an empty enum should produce no diagnostic:

public enum EnumEmpty
{
}

You must obtain the first and last names from the enum's Members collection and its EnumMemberDeclarationSyntax nodes. No semantic model is needed.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson03EnumDeclarations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
    bb1.Create()
        .WithId("SYNTAX302")
        .WithTitle("Enum Member Range")
        .WithMessage("Enum '{0}' starts with '{1}' and ends with '{2}'")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);

    private sealed class EnumDiagnosticData
    {
        public string EnumName { get; set; } = "";
        public string FirstMemberName { get; set; } = "";
        public string LastMemberName { get; set; } = "";
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
            data.FirstMemberName,
            data.LastMemberName);
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
            if (members.Count == 0)
            {
                return;
            }
            var firsts = members.First().Name;
            var lasts = members.Last().Name;

            EnumDiagnosticData data = new()
            {
                EnumName = name,
                FirstMemberName = firsts,
                LastMemberName = lasts
            };

            Diagnostic d = CreateDiagnostic(OnlyRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.EnumDeclaration);
        context.EnableConcurrentExecution();
    }
}