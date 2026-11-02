/*
Exercise 01 — Report Namespace Declarations

Goal:
Practice analyzing block-scoped and file-scoped namespace declarations.

Requirements:

1. Use syntax node analysis to handle both:
   - Block-scoped namespace declarations.
   - File-scoped namespace declarations.

2. Report exactly one diagnostic for EACH namespace declaration.
   - Include empty namespaces.
   - Include nested namespaces.
   - If the same namespace is declared more than once, report each
     declaration separately.

3. Populate the diagnostic data with:
   - NamespaceName: the name written on that declaration.
   - DeclarationForm: exactly "block-scoped" or "file-scoped".

4. Obtain NamespaceName from the declaration's Name syntax using
   ToString().
   - Keep qualified names such as Example.Tools.
   - For a nested declaration, do NOT prepend its enclosing namespace.

5. Report the diagnostic on the COMPLETE namespace name.
   - For namespace Example.Tools, highlight Example.Tools.
   - Do not highlight the namespace keyword, braces, semicolon,
     or entire declaration.

6. Do not report diagnostics for:
   - Using directives.
   - Types declared outside a namespace.
   - Namespace-like text inside comments or strings.

7. Use syntax only. No semantic model or symbol analysis is required.

Diagnostic contract:
ID: NSDECL001
Title: Namespace declaration
Category: Practice
Severity: Info
Enabled by default: true
Message:
Namespace '{0}' uses a {1} declaration

Use the supplied NamespaceDiagnosticData and CreateDiagnostic method.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson08NamespaceAndUsingDeclarations.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "NSDECL001",
        title: "Namespace declaration",
        messageFormat: "Namespace '{0}' uses a {1} declaration",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private sealed class NamespaceDiagnosticData
    {
        public string NamespaceName { get; set; } = "";
        public string DeclarationForm { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        NamespaceDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.NamespaceName,
            data.DeclarationForm);
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
            string name = node.Name.ToString();
            string form = node.DeclarationForm;
            Location location = node.GetIdentifierLocation;
            NamespaceDiagnosticData data = new()
            {
                DeclarationForm = form,
                NamespaceName = name
            };
            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
        context.EnableConcurrentExecution();
    }
}