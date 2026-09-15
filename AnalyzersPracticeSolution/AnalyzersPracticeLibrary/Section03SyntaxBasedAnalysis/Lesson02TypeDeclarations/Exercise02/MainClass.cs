/*
Enter the requirements for this exercise here.
The new syntax type is:

StructDeclarationSyntax

and the corresponding syntax kind is:

SyntaxKind.StructDeclaration

It follows the same important identifier pattern you just learned:

StructDeclarationSyntax
    ↓
Identifier
    ↓
SyntaxToken
    ↓
identifier.GetLocation()



*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson02TypeDeclarations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor TypeRule { get; } = new(
    id: "SYNTAX302",
    title: "Type Declaration Found",
    messageFormat: "{0} '{1}' was found",
    category: "Syntax",
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true);

    private sealed class TypeDiagnosticData
    {
        public string TypeKind { get; set; } = "";
        public string TypeName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        TypeDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.TypeKind,
            data.TypeName);
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [TypeRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            //this shows where its either class or struct declaration.
            if (context.Node.IsKind(SyntaxKind.StructDeclaration))
            {
                StructDeclarationSyntax node = context.ToStructSyntax;
                string name = node.Name;
                Location location = node.GetIdentifierLocation;
                TypeDiagnosticData data = new()
                {
                    TypeName = name,
                    TypeKind = "Struct"
                };
                Diagnostic d = CreateDiagnostic(TypeRule, location, data);
                context.ReportDiagnostic(d);
            }
            if (context.Node.IsKind(SyntaxKind.ClassDeclaration))
            {
                ClassDeclarationSyntax node = context.ToClassSyntax;
                string name = node.Name;
                Location location = node.GetIdentifierLocation;
                TypeDiagnosticData data = new()
                {
                    TypeName = name,
                    TypeKind = "Class"
                };
                Diagnostic d = CreateDiagnostic(TypeRule, location, data);
                context.ReportDiagnostic(d);
            }
        }, SyntaxKind.StructDeclaration, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
    }
}