/*
Enter the requirements for this exercise here.
Create an analyzer that reports one diagnostic for every enum declaration.

The diagnostic must report:

the enum's name
the number of enum members

For this source:

public enum EnumDifficulty
{
    Easy,
    Normal,
    Hard
}

the diagnostic message must be:

Enum 'EnumDifficulty' contains 3 members

The diagnostic must be located on the enum identifier (EnumDifficulty), not on the entire enum declaration.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson03EnumDeclarations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
    bb1.Create()
        .WithId("SYNTAX301")
        .WithTitle("Enum Declaration Summary")
        .WithMessage("Enum '{0}' contains {1} members")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);

    private sealed class EnumDiagnosticData
    {
        public string EnumName { get; set; } = "";
        public  int MemberCount { get; set; }
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
            data.MemberCount);
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
            EnumDiagnosticData data = new()
            {
                EnumName = name,
                MemberCount = node.Members.Count
            };

            Diagnostic d = CreateDiagnostic(OnlyRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.EnumDeclaration);
        context.EnableConcurrentExecution();
    }
}