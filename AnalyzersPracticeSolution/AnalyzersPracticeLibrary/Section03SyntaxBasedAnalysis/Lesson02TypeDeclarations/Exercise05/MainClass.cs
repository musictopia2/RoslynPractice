/*
Enter the requirements for this exercise here.
A client wants an analyzer that reviews the type declarations in a source file. They want a diagnostic for every class, struct, interface, record class, and record struct, showing its type category and whether it explicitly declares itself public.

Enums remain excluded because they're covered in their own lesson.

The important Roslyn connection

You'll register five syntax kinds:

SyntaxKind.ClassDeclaration
SyntaxKind.StructDeclaration
SyntaxKind.InterfaceDeclaration
SyntaxKind.RecordDeclaration
SyntaxKind.RecordStructDeclaration

But those five kinds correspond to only four syntax-node classes:

ClassDeclaration        → ClassDeclarationSyntax
StructDeclaration       → StructDeclarationSyntax
InterfaceDeclaration    → InterfaceDeclarationSyntax

RecordDeclaration       ─┐
                         ├→ RecordDeclarationSyntax
RecordStructDeclaration ─┘

That last pair is deliberately included again so the distinction from Exercise 04 gets reinforced.


Exact client rules

Every supported declaration produces exactly one diagnostic. Use these exact TypeKind values:

Class
Struct
Interface
Record Class
Record Struct

Accessibility has exactly two possible values:

Public
NonPublic

As before, this is syntax-only. Public means the declaration's Modifiers actually contains public. Anything else is NonPublic.

For example:

public class Customer
{
}

internal struct Coordinate
{
}

public interface IService
{
}

record Order(int Number);

public record struct Measurement(double Value);

must produce:

Class 'Customer' has accessibility 'Public'
Struct 'Coordinate' has accessibility 'NonPublic'
Interface 'IService' has accessibility 'Public'
Record Class 'Order' has accessibility 'NonPublic'
Record Struct 'Measurement' has accessibility 'Public'
Location requirement

Every diagnostic must point to only the declaration identifier:

public class Customer
             ^^^^^^^^

Nested supported types also receive their own diagnostics independently.

Enums produce no diagnostic.

*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson02TypeDeclarations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor TypeReviewRule { get; } = new(
    id: "SYNTAX305",
    title: "Type Declaration Review",
    messageFormat: "{0} '{1}' has accessibility '{2}'",
    category: "Syntax",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class TypeReviewDiagnosticData
    {
        public string TypeKind { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string Accessibility { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        TypeReviewDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.TypeKind,
            data.TypeName,
            data.Accessibility);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [TypeReviewRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            BaseTypeDeclarationSyntax  node = context.ToBaseTypeDeclaration;
            string name = node.Name;
            Location location = node.GetIdentifierLocation;
            string accessible = node.IsPublic ? "Public" : "NonPublic";
            string kind = node.KindName;

            TypeReviewDiagnosticData data = new()
            {
                TypeName = name,
                TypeKind = kind,
                Accessibility = accessible
            };
            Diagnostic d = CreateDiagnostic(TypeReviewRule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.RecordStructDeclaration,
        SyntaxKind.RecordDeclaration,
        SyntaxKind.ClassDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.StructDeclaration);
        context.EnableConcurrentExecution();
    }
}