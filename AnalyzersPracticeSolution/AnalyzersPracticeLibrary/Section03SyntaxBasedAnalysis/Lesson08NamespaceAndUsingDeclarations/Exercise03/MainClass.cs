/*
Exercise 03 — Classify Global Using Directives

Goal:
Recognize global using directives and distinguish their import forms.

Requirements:

1. Register syntax node analysis for SyntaxKind.UsingDirective.
   Use UsingDirectiveSyntax.

2. Report exactly one diagnostic for each global using directive
   that does NOT contain an alias assignment.

3. Include both:
   - Global namespace imports:
     global using System.Text;

   - Global static imports:
     global using static System.Math;

4. Exclude:
   - All non-global using directives, including static imports.
   - All alias directives, including global aliases.
   - Resource-management using statements and declarations.
   - Comments and strings.

5. Populate the supplied data:
   - ImportedName: Name.ToString().
   - ImportKind: exactly "namespace" or "static".

6. Preserve the imported name as written.
   Include qualified names, escaped identifiers, and global::
   prefixes when present.

7. Report on the COMPLETE imported name only.
   Exclude global, using, static, the semicolon, and surrounding trivia.

8. Report each qualifying directive separately.
   Do not combine or deduplicate repeated imports.

9. Distinguish the global modifier from the global:: name prefix:
   - using global::System.Text; is excluded.
   - global using global::System.Text; is included.

10. Use syntax only.
    Do not check whether the target exists or is used.
    Assume syntactically valid input.
    Skip directives whose Name is null.

Diagnostic contract:
ID: GLOBALIMPORT001
Title: Global import
Category: Practice
Severity: Info
Enabled by default: true
Message:
Global using imports '{0}' as a {1} import

Use the supplied data model and CreateDiagnostic method.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson08NamespaceAndUsingDeclarations.Exercise03;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "GLOBALIMPORT001",
        title: "Global import",
        messageFormat: "Global using imports '{0}' as a {1} import",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private sealed class GlobalImportDiagnosticData
    {
        public string ImportedName { get; set; } = "";
        public string ImportKind { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        GlobalImportDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.ImportedName,
            data.ImportKind);
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
            UsingDirectiveSyntax node = context.ToUsingDirectiveSyntax;

            if (node.Name is null)
            {
                return;
            }
            if (node.IsGlobal == false)
            {
                return;
            }
            if (node.IsAlias)
            {
                return;
            }
            string kind = "";
            if (node.IsStatic)
            {
                kind = "static";
            }
            else
            {
                kind = "namespace";
            }


            string name = node.Name.ToString();
            Location location = node.GetIdentifierLocation!;



            GlobalImportDiagnosticData data = new()
            {
                ImportedName = name,
                ImportKind = kind
            };
            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.UsingDirective);
        context.EnableConcurrentExecution();

    }
}