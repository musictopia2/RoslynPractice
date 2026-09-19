/*
Enter the requirements for this exercise here.
Exercise 04 — Combined Class-Like Member Analysis

Now we'll combine the declaration forms from Exercises 01–03. This is the Combination exercise, so it should reuse what you've already learned rather than introduce a new Roslyn concept.

Requirements

Create one analyzer that reports a diagnostic for every:

class
struct
interface
record class
record struct

For each declaration, report:

declaration kind;
declaration name;
total number of directly declared members;
number of methods;
number of properties;
number of fields;
first directly declared member name;
last directly declared member name.



*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson04ClassLikeDeclarationsAndMembers.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
    id: "RPA030404",
    title: "Class-like declaration summary",
    messageFormat: "{0} '{1}' has {2} member(s): {3} method(s), {4} property(s), {5} field(s). First: '{6}'. Last: '{7}'.",
    category: "Syntax Analysis",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class ClassLikeDeclarationDiagnosticData
    {
        public string DeclarationKind { get; set; } = "";
        public string DeclarationName { get; set; } = "";
        public int MemberCount { get; set; }
        public int MethodCount { get; set; }
        public int PropertyCount { get; set; }
        public int FieldCount { get; set; }
        public string FirstMemberName { get; set; } = "";
        public string LastMemberName { get; set; } = "";
    }
    
    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassLikeDeclarationDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.DeclarationKind,
            data.DeclarationName,
            data.MemberCount,
            data.MethodCount,
            data.PropertyCount,
            data.FieldCount,
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
            ClassLikeDeclarationDiagnosticData data = new()
            {
                DeclarationName = name,
                DeclarationKind = node.KindName,
                FirstMemberName = firstName,
                LastMemberName = lastName,
                MemberCount = members.Count,
                MethodCount = node.MethodCount,
                PropertyCount = node.PropertyCount,
                FieldCount = node.FieldCount
            };

            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);

        }, SyntaxKind.StructDeclaration, 
        SyntaxKind.RecordDeclaration, 
        SyntaxKind.RecordStructDeclaration,
        SyntaxKind.ClassDeclaration,
        SyntaxKind.InterfaceDeclaration
        );
        context.EnableConcurrentExecution();
    }
}