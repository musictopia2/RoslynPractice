/*
Enter the requirements for this exercise here.
Given:

public enum EnumPermissions
{
    None = 0,
    Read = 1,
    Write,
    Execute,
    ReadWrite = Read | Write
}

report one diagnostic for the enum:

Enum 'EnumPermissions' mixes value styles: 3 explicit and 2 implicit members; first explicit member is 'None = 0' and last explicit member is 'ReadWrite = Read | Write'

The diagnostic location must be the enum identifier:

EnumPermissions
Rules

Report exactly one diagnostic only when the enum contains at least one explicit member AND at least one implicit member.

Therefore:

public enum EnumAllImplicit
{
    First,
    Second,
    Third
}

produces no diagnostic.

public enum EnumAllExplicit
{
    First = 1,
    Second = 2,
    Third = 3
}

produces no diagnostic.

An empty enum also produces no diagnostic.

You need to capture the first and last members among the explicitly assigned members, not simply the first and last members of the enum.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson03EnumDeclarations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private sealed class EnumDiagnosticData
    {
        public string EnumName { get; set; } = "";
        public int ExplicitCount { get; set; }
        public int ImplicitCount { get; set; }

        public string FirstExplicitName { get; set; } = "";
        public string FirstExplicitValue { get; set; } = "";

        public string LastExplicitName { get; set; } = "";
        public string LastExplicitValue { get; set; } = "";
    }
    private static DiagnosticDescriptor OnlyRule { get; } =
    bb1.Create()
        .WithId("SYNTAX305")
        .WithTitle("Mixed Enum Value Styles")
        .WithMessage(
            "Enum '{0}' mixes value styles: {1} explicit and {2} implicit members; first explicit member is '{3} = {4}' and last explicit member is '{5} = {6}'")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);
    private static Diagnostic CreateDiagnostic(
    DiagnosticDescriptor rule,
    Location location,
    EnumDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.EnumName,
            data.ExplicitCount,
            data.ImplicitCount,
            data.FirstExplicitName,
            data.FirstExplicitValue,
            data.LastExplicitName,
            data.LastExplicitValue);
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
            var hasAnyExplicit = members.Any(x => x.HasExplicitValue);
            var hasAnyWithout = members.Any(x => x.HasExplicitValue == false);
            if (hasAnyExplicit == false || hasAnyWithout == false)
            {
                return;
            }
            var explicitCount = members.Count(x => x.HasExplicitValue);
            var implicitCount = members.Count(x => x.HasExplicitValue == false);
            string firstExplicitName = members.First(x => x.HasExplicitValue).Name;
            string firstExplicitValue = members.First(x => x.HasExplicitValue).ExplicitValue;
            string lastExplicitName = members.Last(x => x.HasExplicitValue).Name;
            string lastExplicitValue = members.Last(x => x.HasExplicitValue).ExplicitValue;
            EnumDiagnosticData data = new()
            {
                EnumName = name,
                ExplicitCount = explicitCount,
                ImplicitCount = implicitCount,
                FirstExplicitName = firstExplicitName,
                FirstExplicitValue = firstExplicitValue,
                LastExplicitName = lastExplicitName,
                LastExplicitValue = lastExplicitValue
            };

            Diagnostic d = CreateDiagnostic(OnlyRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.EnumDeclaration);
        context.EnableConcurrentExecution();
    }
}