/*
Exercise 02 — Report Ordinary Using Directives

Goal:
Recognize ordinary using directives and report their imported names.

Requirements:

1. Register syntax node analysis for SyntaxKind.UsingDirective.
   Its node type is UsingDirectiveSyntax.

2. Report exactly one diagnostic for each ordinary using directive.
   For this exercise, "ordinary" means:
   - No alias assignment.
   - No static keyword.
   - No global keyword before using.

3. Include ordinary using directives wherever they appear:
   - At compilation-unit level.
   - Inside block-scoped namespaces, including nested namespaces.
   - Inside file-scoped namespaces.

4. Report each qualifying directive separately.
   Do not combine or deduplicate repeated imports.

5. NamespaceName must be the directive's Name.ToString().
   Preserve the name as written, including qualified names,
   escaped identifiers, and a global:: prefix when present.

6. Report on the COMPLETE imported name only.
   For using Example.Tools; highlight Example.Tools.
   Exclude the using keyword, semicolon, and surrounding trivia.

7. Do NOT report:
   - Alias directives: using Text = System.String;
   - Static directives: using static System.Math;
   - Global directives: global using System;
   - Any combination of those excluded forms.
   - Namespace declarations.
   - Resource-management using statements or using declarations.
   - Using-like text inside comments or strings.

8. Important distinction:
   using global::System.Text; IS included.
   global using System.Text; IS excluded.

   The global:: prefix is part of the imported name.
   It is not the global modifier before using.

9. Use syntax only.
   Do not resolve whether the imported namespace exists or is used.
   Assume syntactically valid input; malformed directives are
   outside this exercise's requirements.

Diagnostic contract:
ID: USINGIMPORT001
Title: Ordinary namespace import
Category: Practice
Severity: Info
Enabled by default: true
Message:
Using directive imports namespace '{0}'

Use the supplied data model and CreateDiagnostic method.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson08NamespaceAndUsingDeclarations.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "USINGIMPORT001",
        title: "Ordinary namespace import",
        messageFormat: "Using directive imports namespace '{0}'",
        category: "Practice",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private sealed class UsingDiagnosticData
    {
        public string NamespaceName { get; set; } = "";
    }

    private static Diagnostic CreateDiagnostic(
        DiagnosticDescriptor rule,
        Location location,
        UsingDiagnosticData data)
    {
        return Diagnostic.Create(
            rule,
            location,
            data.NamespaceName);
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
            if (node.IsStatic || node.IsAlias || node.IsGlobal)
            {
                return;
            }

            string name = node.Name.ToString();
            Location location = node.GetIdentifierLocation!;
            UsingDiagnosticData data = new()
            {
                NamespaceName = name
            };
            Diagnostic d = CreateDiagnostic(_rule, location, data);
            context.ReportDiagnostic(d);
        }, SyntaxKind.UsingDirective);
        context.EnableConcurrentExecution();
    }
}