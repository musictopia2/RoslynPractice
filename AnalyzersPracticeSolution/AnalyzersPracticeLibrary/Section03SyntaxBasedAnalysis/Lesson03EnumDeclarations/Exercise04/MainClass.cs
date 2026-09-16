/*
Enter the requirements for this exercise here.
Report one diagnostic for each enum member that has an explicitly assigned value.

For:

public enum EnumPermissions
{
    None = 0,
    Read = 1,
    Write,
    ReadWrite = Read | Write
}

you must report three diagnostics:

Enum member 'None' explicitly uses value '0'
Enum member 'Read' explicitly uses value '1'
Enum member 'ReadWrite' explicitly uses value 'Read | Write'

Write produces no diagnostic because it has no EqualsValue.

Location

Each diagnostic must be located on the enum member's identifier, not the enum identifier.

So the three locations above are:

None
Read
ReadWrite

This combines your previous location knowledge with the enum-member syntax you're learning now.
*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson03EnumDeclarations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
    bb1.Create()
        .WithId("SYNTAX304")
        .WithTitle("Explicit Enum Member Value")
        .WithMessage(
            "Enum member '{0}' explicitly uses value '{1}'")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);

    private sealed class EnumMemberDiagnosticData
    {
        public string MemberName { get; set; } = "";
        public string ExplicitValue { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        EnumMemberDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.MemberName,
            data.ExplicitValue);
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
            Location location = node.GetIdentifierLocation;
            var members = node.Members;
            members.ForCoditionalItems(x => x.HasExplicitValue, member =>
            {
                string name = member.Name;
                Location location = member.GetIdentifierLocation;
                string value = member.ExplicitValue!;
                EnumMemberDiagnosticData data = new()
                {
                    ExplicitValue = value,
                    MemberName = name
                };
                Diagnostic d = CreateDiagnostic(OnlyRule, location, data);
                context.ReportDiagnostic(d);
            });            
        }, SyntaxKind.EnumDeclaration);
        context.EnableConcurrentExecution();
    }
}