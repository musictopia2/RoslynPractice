/*
Exercise 04 — Summarize Imports Within a Namespace

Goal:
Combine namespace analysis, direct-child selection, and import
classification.

Requirements:

1. Analyze both block-scoped and file-scoped namespace declarations
   using syntax node analysis.

2. For each namespace, examine only using directives written
   DIRECTLY inside that declaration.
   - Do not include compilation-unit-level imports.
   - Do not include imports from enclosing namespaces.
   - Do not include imports from nested namespaces.

3. Classify each direct using directive into exactly one category:
   - Alias: contains an alias assignment.
   - Static: has static and is not an alias.
   - Ordinary: has neither an alias assignment nor static.

   Count aliases even when their Name property is null.
   This exercise counts directives, not their target names.

4. Count every direct directive separately.
   Do not deduplicate repeated imports.

5. Report exactly one diagnostic for a namespace when:
   OrdinaryCount + StaticCount + AliasCount >= 3.

   Otherwise, report nothing for that namespace.

6. Analyze nested namespaces independently.
   Repeated declarations of the same namespace are also independent.
   Do not combine counts across declarations or source files.

7. Populate:
   - NamespaceName: this declaration's Name.ToString().
     Do not prepend enclosing namespace names.
   - DeclarationForm: "block-scoped" or "file-scoped".
   - Imports: the three direct import counts.

8. Report on the COMPLETE namespace name only.
   Exclude the namespace keyword and surrounding punctuation/trivia.

9. Namespace declarations, types, comments, strings, and resource-
   management using statements/declarations do not contribute counts.

10. Use syntax only. Do not resolve imports or check whether they
    are used. Assume syntactically valid input.
    Global using directives cannot legally appear inside namespaces,
    so malformed placements are outside this exercise's requirements.

Diagnostic contract:
ID: NSIMPORTSUMMARY001
Title: Namespace import summary
Category: Practice
Severity: Info
Enabled by default: true
Message:
Namespace '{0}' ({1}) has direct imports: ordinary={2}, static={3}, alias={4}

Use the supplied data models and CreateDiagnostic method.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson08NamespaceAndUsingDeclarations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "NSIMPORTSUMMARY001",
        title: "Namespace import summary",
        messageFormat:
            "Namespace '{0}' ({1}) has direct imports: ordinary={2}, static={3}, alias={4}",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private sealed class ImportCountsDiagnosticData
    {
        public int OrdinaryCount { get; set; }
        public int StaticCount { get; set; }
        public int AliasCount { get; set; }
    }

    private sealed class NamespaceImportDiagnosticData
    {
        public string NamespaceName { get; set; } = "";
        public string DeclarationForm { get; set; } = "";

        public ImportCountsDiagnosticData Imports { get; set; }
            = new ImportCountsDiagnosticData();
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        NamespaceImportDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.NamespaceName,
            data.DeclarationForm,
            data.Imports.OrdinaryCount,
            data.Imports.StaticCount,
            data.Imports.AliasCount);
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
            int ordinaryCount = list.Count(x => x.IsOrdinary);
            int staticCount = list.Count(x => x.IsStatic);
            int aliasCount = list.Count(x => x.IsAlias);
            if (ordinaryCount + staticCount + aliasCount < 3)
            {
                return;
            }
            string name = node.Name.ToString();
            string form = node.DeclarationForm;
            Location location = node.GetIdentifierLocation!;
            ImportCountsDiagnosticData imports = new()
            {
                AliasCount = aliasCount,
                StaticCount = staticCount,
                OrdinaryCount = ordinaryCount
            };
            NamespaceImportDiagnosticData data = new()
            {
                DeclarationForm = form,
                NamespaceName = name,
                Imports = imports
            };
            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
        context.EnableConcurrentExecution();
    }
}