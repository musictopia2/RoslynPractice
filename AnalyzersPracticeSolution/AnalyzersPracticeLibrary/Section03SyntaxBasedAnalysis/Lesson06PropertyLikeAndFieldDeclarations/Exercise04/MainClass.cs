/*
Lesson 06 — Property-Like and Field Declarations
Exercise 04 — Summarize Direct Class Members

Goal:
Count fields, properties, and indexers declared directly inside a class.

Requirements:

1. Register a syntax-node action for SyntaxKind.ClassDeclaration.

2. Examine the class's Members collection.
   Count only members declared directly inside that class.

3. Calculate these three counts:

   FieldCount:
   - Count individual variables in field declarations.
   - "int first, second;" counts as two fields.
   - Include constants.
   - Exclude events, including field-like events.

   PropertyCount:
   - Count each property declaration once.
   - Include properties with or without initializers.
   - Include expression-bodied properties.
   - Do not count indexers as properties.

   IndexerCount:
   - Count each indexer declaration once.
   - Include indexers with any number of parameters.
   - Include accessor-based and expression-bodied indexers.

4. Report one diagnostic for a class if at least one of these three
   counts is greater than zero.

5. Each diagnostic must:
   - Use diagnostic ID MEMBERS001.
   - Have Info severity.
   - Highlight only the class's identifier token.
   - Use this exact message:
     Class '{name}' declares {fields} fields, {properties} properties, and {indexers} indexers

   Use this wording even when a count is 1.

6. Use the class identifier's ValueText for its name.

7. Count qualifying members regardless of accessibility, modifiers,
   declared types, initializers, or bodies.

8. Do not include members of nested types in an outer class's counts.
   Analyze each nested class independently.

9. Analyze each partial class declaration independently.
   Do not combine members from separate partial declarations.

10. Do not report diagnostics for structs, records, or interfaces.
    A class nested inside one of those types is still analyzed.

11. Methods, constructors, nested type declarations, and other member
    kinds do not contribute to these counts.
    Local variables do not contribute.

12. Use syntax only. Do not use semantic information.

13. Use the supplied ClassDiagnosticData, MemberCounts, and
    CreateDiagnostic method.

Example:

    class Outer
    {
        private int first, second;
        private const int Limit = 10;

        public int Count { get; set; }
        public string Name => "Example";

        public int this[int index] => 0;

        public event System.Action Changed;

        void Method()
        {
            int local = 0;
        }

        class Inner
        {
            public bool Enabled { get; set; }
        }
    }

Expected diagnostics:

    On Outer:
    Class 'Outer' declares 3 fields, 2 properties, and 1 indexers

    On Inner:
    Class 'Inner' declares 0 fields, 1 properties, and 0 indexers

A class containing only methods or nested type declarations produces
no diagnostic for itself.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson06PropertyLikeAndFieldDeclarations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "MEMBERS001",
        title: "Class member summary",
        messageFormat:
            "Class '{0}' declares {1} fields, {2} properties, and {3} indexers",
        category: "Structure",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private sealed class ClassDiagnosticData
    {
        public  string ClassName { get; set; } = "";
        public MemberCounts Counts { get; set; } = null!;
    }

    private sealed class MemberCounts
    {
        public  int FieldCount { get; set; }
        public int PropertyCount { get; set; }
        public int IndexerCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        ClassDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ClassName,
            data.Counts.FieldCount,
            data.Counts.PropertyCount,
            data.Counts.IndexerCount);
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


            int indexCount = node.GetAllIndexers.Count;
            int propertyCount = node.GetAllProperties.Count;
            int fieldCount = node.GetAllFieldVariables.Count;
            if (indexCount == 0 && propertyCount == 0 && fieldCount == 0)
            {
                return;
            }
            MemberCounts counts = new()
            {
                FieldCount = fieldCount,
                IndexerCount = indexCount,
                PropertyCount = propertyCount
            };
            ClassDiagnosticData data = new()
            {
                ClassName = node.Name,
                Counts = counts
            };
            Diagnostic diagnostic = CreateDiagnostic(_rule, node.GetIdentifierLocation, data);
            context.ReportDiagnostic(diagnostic);
        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
    }
}