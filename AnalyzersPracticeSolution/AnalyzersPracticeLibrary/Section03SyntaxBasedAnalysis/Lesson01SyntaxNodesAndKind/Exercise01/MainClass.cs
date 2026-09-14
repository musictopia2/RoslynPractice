/*
Enter the requirements for this exercise here.
Requirement

Create an analyzer that reports a diagnostic for every class declaration in the analyzed source.

Register a syntax-node action for:

SyntaxKind.ClassDeclaration

Your callback must obtain the ClassDeclarationSyntax from context.Node.

Diagnostic descriptor

Use exactly:

ID: SYNTAX101
Title: Class Declaration Found
Category: SyntaxAnalysis
Severity: Info
Enabled by default: true

Message format:

Class '{0}' was found

The {0} argument must be the class's declared identifier.

For:

class Customer
{
}

the message must therefore be:

Class 'Customer' was found
Diagnostic location

The diagnostic must use the location of the entire ClassDeclarationSyntax node.

In other words, use the location represented by the class syntax node itself, rather than Location.None or only the identifier's location.

Multiple classes

Report one diagnostic for every class declaration.

For example:

class First
{
}

class Second
{
}

must produce two diagnostics:

Class 'First' was found
Class 'Second' was found
What this exercise is practicing

There are really only four new operations:

RegisterSyntaxNodeAction
        ↓
SyntaxKind.ClassDeclaration
        ↓
context.Node
        ↓
ClassDeclarationSyntax

Everything involving the descriptor and Diagnostic.Create is material you already know.
*/

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("SYNTAX101")
        .WithTitle("Class Declaration Found")
        .WithMessage("Class '{0}' was found")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Info);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            //this runs for every class found.   works well in unit tests.  not sure if it will work for reals (i don't want to have to manually write my classes into an editor)

            /*
            this was ai response.
            It will work for real analyzer use too. 
            Visual Studio/compiler supplies the actual syntax tree from the project being analyzed. 
            You do not manually type classes into some analyzer editor. 
            The unit test just simulates that environment by providing source text.
        }
        */


            ClassDeclarationSyntax node = context.ToClassSyntax;
            string name = node.Name;
            Location location = node.GetLocation();
            Diagnostic d = Diagnostic.Create(OnlyRule, location, name);
            context.ReportDiagnostic(d);
        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
    }
}