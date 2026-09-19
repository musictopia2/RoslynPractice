/*
Enter the requirements for this exercise here.
Requirements

Create an analyzer that reports one diagnostic for each:

struct
record class
record struct

Do not report diagnostics for ordinary classes or interfaces in this exercise.

For every analyzed declaration, determine:

declaration name;
declaration kind;
number of directly declared members;
first directly declared member name;
last directly declared member name.

For DeclarationKind, use exactly:

Struct
Record Class
Record Struct



The same rules from Exercise 01 continue to apply:

Only directly declared members count.
An empty declaration has MemberCount = 0.
Empty declarations use "<none>" for both member names.
A field such as int first, second; counts as one member, and its member name is first.
Report the diagnostic on the declaration's identifier.
*/

using System.Data;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson04ClassLikeDeclarationsAndMembers.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
    id: "RPA030402",
    title: "Class-like member summary",
    messageFormat: "{0} '{1}' has {2} member(s). First: '{3}'. Last: '{4}'.",
    category: "Syntax Analysis",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class ClassLikeMemberDiagnosticData
    {
        public string DeclarationKind { get; set; } = "";
        public string DeclarationName { get; set; } = "";
        public int MemberCount { get; set; }
        public string FirstMemberName { get; set; } = "";
        public string LastMemberName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassLikeMemberDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.DeclarationKind,
            data.DeclarationName,
            data.MemberCount,
            data.FirstMemberName,
            data.LastMemberName);
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [_rule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {

            var node = context.ToTypeDeclaration;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            var members = node.Members;
            string firstName;
            string lastName;
            if (members.Count == 0)
            {
                firstName = "<none>";
                lastName = firstName;
            }
            else
            {
                firstName = members.First().Name;
                lastName = members.Last().Name;
            }
            ClassLikeMemberDiagnosticData data = new()
            {
                DeclarationName = name,
                FirstMemberName = firstName,
                LastMemberName = lastName,
                MemberCount = members.Count,
                DeclarationKind = node.KindName
            };

            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
        context.EnableConcurrentExecution();
    }
}