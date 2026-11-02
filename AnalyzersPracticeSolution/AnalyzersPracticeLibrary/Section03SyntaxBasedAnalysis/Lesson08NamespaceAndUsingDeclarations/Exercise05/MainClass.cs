/*
Exercise 05 — Duplicate Imports Within a Scope

Client goal:
Find repeated non-global namespace and static imports within
individual syntax scopes.

1. Analyze these scopes independently:
   - The compilation unit: imports outside namespace declarations.
   - Each block-scoped namespace declaration.
   - Each file-scoped namespace declaration.

2. Examine only the using directives directly belonging to each scope.
   Never combine imports from parent, child, or sibling scopes.
   Repeated declarations of the same namespace are separate scopes.
   Separate source files are also independent.

3. Include:
   - Ordinary non-global imports.
   - Static non-global imports.

4. Exclude:
   - Every alias directive.
   - Every global using directive.
   - Directives whose Name is null.
   - Resource-management using statements/declarations.

5. Group included directives using BOTH:
   - ImportedName: Name.ToString().
   - ImportKind: exactly "namespace" or "static".

   Compare names using ordinal, case-sensitive string comparison.
   Do not normalize names or resolve symbols.

   Consequently:
   - System.Text and global::System.Text are different names.
   - Example.Tools and Example.tools are different names.
   - Ordinary and static imports are different groups even if
     their imported-name text matches.

6. A group qualifies when it contains at least TWO directives.
   Report exactly ONE diagnostic for each qualifying group.

7. Location:
   Highlight the COMPLETE imported name on the SECOND directive
   in that group, in source order.

   Do not report on the first directive.
   Do not report again for third or later occurrences.

8. Diagnostic data:
   - ImportedName: the group's imported-name text.
   - ImportKind: "namespace" or "static".
   - OccurrenceCount: ALL included directives in that group,
     including the first.
   - ScopeName:
     "<compilation unit>" for imports outside namespaces;
     otherwise, the owning namespace declaration's Name.ToString().

   For nested namespaces, do not prepend enclosing namespace names.

9. Do not require a duplicate import to be unused.
   This is syntax-only analysis; no semantic model is required.
   Assume syntactically valid input.

Diagnostic contract:
ID: DUPIMPORT001
Title: Duplicate import within a scope
Category: Practice
Severity: Warning
Enabled by default: true
Message:
Scope '{0}' contains {1} occurrences of {2} import '{3}'

Use the supplied data model and CreateDiagnostic method.
You choose the analysis registration and supporting helpers.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson08NamespaceAndUsingDeclarations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "DUPIMPORT001",
        title: "Duplicate import within a scope",
        messageFormat:
            "Scope '{0}' contains {1} occurrences of {2} import '{3}'",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private sealed class DuplicateImportDiagnosticData
    {
        public string ScopeName { get; set; } = "";
        public string ImportedName { get; set; } = "";
        public string ImportKind { get; set; } = "";
        public int OccurrenceCount { get; set; }
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        DuplicateImportDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ScopeName,
            data.OccurrenceCount,
            data.ImportKind,
            data.ImportedName);
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
            BaseNamespaceDeclarationSyntax node = context.ToBaseNamespaceSyntax;
            if (node.Name is null)
            {
                return;
            }
            var list = node.GetUsings;
            list.RemoveAll(x => x.IsGlobal);
            list.RemoveAll(x => x.IsAlias);
            list.RemoveAll(x => x.Name is null);

            var groups = list.GroupBy(usingNode => (
                ImportedName: usingNode.Name!.ToString(),
                ImportKind: usingNode.IsStatic ? "static" : "namespace"));
            foreach (var item in groups)
            {
                if (item.Count() < 2)
                {
                    continue;
                }
                int counts = item.Count();
                string name = item.Key.ImportedName;
                string kind = item.Key.ImportKind;
                string scope = node.Name.ToString();
                Location location = item.ElementAt(1).GetIdentifierLocation!;
                DuplicateImportDiagnosticData data = new()
                {
                    ImportedName = name,
                    ImportKind = kind,
                    OccurrenceCount = counts,
                    ScopeName = scope
                };
                Diagnostic d = CreateDiagnostic(_rule, location, data);
                context.ReportDiagnostic(d);
            }
        }, SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);


        context.RegisterSyntaxNodeAction(context =>
        {
            CompilationUnitSyntax node = context.ToCompilationUnitSyntax;

            string scope = "<compilation unit>";


            List<UsingDirectiveSyntax> list = node.Usings.ToList();
            list.RemoveAll(x => x.IsGlobal);
            list.RemoveAll(x => x.IsAlias);
            list.RemoveAll(x => x.Name is null);

            var groups = list.GroupBy(usingNode => (
                ImportedName: usingNode.Name!.ToString(),
                ImportKind: usingNode.IsStatic ? "static" : "namespace"));
            foreach (var item in groups)
            {
                if (item.Count() < 2)
                {
                    continue;
                }
                int counts = item.Count();
                string name = item.Key.ImportedName;
                string kind = item.Key.ImportKind;
                Location location = item.ElementAt(1).GetIdentifierLocation!;
                DuplicateImportDiagnosticData data = new()
                {
                    ImportedName = name,
                    ImportKind = kind,
                    OccurrenceCount = counts,
                    ScopeName = scope
                };
                Diagnostic d = CreateDiagnostic(_rule, location, data);
                context.ReportDiagnostic(d);
            }
        }, SyntaxKind.CompilationUnit);
        context.EnableConcurrentExecution();
    }
}