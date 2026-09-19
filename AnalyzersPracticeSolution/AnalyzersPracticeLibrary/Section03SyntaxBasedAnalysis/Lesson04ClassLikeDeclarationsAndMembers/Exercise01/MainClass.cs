/*
Enter the requirements for this exercise here.
Create an analyzer that reports one diagnostic for every class declaration.

For each class, determine:

the class name;
the total number of members directly declared in that class;
the name of the first directly declared member;
the name of the last directly declared member.

For this exercise, the test classes will contain only these member types:

methods
properties
fields

A class with zero members must still produce a diagnostic. For an empty class, use "<none>" for both the first and last member names.

Only examine direct members of the class. If a class contains a nested class, do not count the nested class's members as members of the outer class. The nested class itself is outside the member types being tested in Exercise 01.

Obtain the location of the class identifier.

Report the diagnostic at the class identifier.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson04ClassLikeDeclarationsAndMembers.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{

    private static readonly DiagnosticDescriptor _rule = new(
    id: "RPA030401",
    title: "Class member summary",
    messageFormat: "Class '{0}' has {1} member(s). First: '{2}'. Last: '{3}'.",
    category: "Syntax Analysis",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class ClassMemberDiagnosticData
    {
        public string ClassName { get; set; } = "";
        public int MemberCount { get; set; }
        public string FirstMemberName { get; set; } = "";
        public string LastMemberName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassMemberDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ClassName,
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
            
            ClassDeclarationSyntax node = context.ToClassSyntax;
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
            ClassMemberDiagnosticData data = new()
            {
                ClassName = name,
                FirstMemberName = firstName,
                LastMemberName = lastName,
                MemberCount = members.Count
            };

            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
        context.EnableConcurrentExecution();
    }
    
}