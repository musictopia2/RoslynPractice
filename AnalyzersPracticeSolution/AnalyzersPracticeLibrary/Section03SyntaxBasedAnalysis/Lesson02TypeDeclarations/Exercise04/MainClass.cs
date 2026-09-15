/*
Enter the requirements for this exercise here.
What you need to determine

Both registered kinds give you a:

RecordDeclarationSyntax

So you can keep the local naming simple:

RecordDeclarationSyntax node = ...

You'll need to determine whether that node is:

SyntaxKind.RecordDeclaration

or:

SyntaxKind.RecordStructDeclaration

and populate RecordKind with exactly:

Record Class

or:

Record Struct

For accessibility, your newer IsPublic property is perfectly appropriate if you've defined it for RecordDeclarationSyntax.

Classes, ordinary structs, interfaces, and enums should produce no diagnostics in Exercise 04.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson02TypeDeclarations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor RecordRule { get; } = new(
    id: "SYNTAX304",
    title: "Record Declaration Found",
    messageFormat: "{0} '{1}' has accessibility '{2}'",
    category: "Syntax",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class RecordDiagnosticData
    {
        public string RecordKind { get; set; } = "";
        public string RecordName { get; set; } = "";
        public string Accessibility { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        RecordDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.RecordKind,
            data.RecordName,
            data.Accessibility);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [RecordRule];
        }
    }
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            RecordDeclarationSyntax node = context.ToRecordSyntax;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            string accessible = node.IsPublic ? "Public" : "NonPublic";
            string category = node.IsRecordStruct ? "Record Struct" : "Record Class";
            RecordDiagnosticData data = new()
            {
                RecordName = name,
                RecordKind = category,
                Accessibility = accessible
            };
            Diagnostic d = CreateDiagnostic(RecordRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.RecordStructDeclaration, SyntaxKind.RecordDeclaration);
        context.EnableConcurrentExecution();
    }
}