/*
Enter the requirements for this exercise here.
Requirements

Create an analyzer that reports one diagnostic for every interface declaration.

For each interface, determine:

interface name;
total number of directly declared members;
number of directly declared methods;
number of directly declared properties;
first directly declared member name;
last directly declared member name.

An empty interface must still produce a diagnostic. Use "<none>" for its first and last member names.

Your responsibility

Register the appropriate syntax-node action and:

Obtain the interface declaration.
Obtain its direct Members.
Determine the total member count.
Count how many members are methods.
Count how many members are properties.
Determine the first and last member names, including the empty-interface case.
Obtain the interface identifier location.
Populate InterfaceMemberDiagnosticData.
Create and report the diagnostic.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson04ClassLikeDeclarationsAndMembers.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
    id: "RPA030403",
    title: "Interface member summary",
    messageFormat: "Interface '{0}' has {1} member(s): {2} method(s), {3} property(s). First: '{4}'. Last: '{5}'.",
    category: "Syntax Analysis",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class InterfaceMemberDiagnosticData
    {
        public string InterfaceName { get; set; } = "";
        public int MemberCount { get; set; }
        public int MethodCount { get; set; }
        public int PropertyCount { get; set; }
        public string FirstMemberName { get; set; } = "";
        public string LastMemberName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        InterfaceMemberDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.InterfaceName,
            data.MemberCount,
            data.MethodCount,
            data.PropertyCount,
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
            InterfaceMemberDiagnosticData data = new()
            {
                InterfaceName = name,
                FirstMemberName = firstName,
                LastMemberName = lastName,
                MemberCount = members.Count,
                MethodCount = node.MethodCount,
                PropertyCount = node.PropertyCount
            };

            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.InterfaceDeclaration);
        context.EnableConcurrentExecution();
    }
}