/*
Enter the requirements for this exercise here.
Client request

A client wants an analyzer that reviews each class declaration and reports a summary of its directly declared methods and properties.

Use:

ID: CLIENT401
Title: Class Structure Review
Category: SyntaxAnalysis
Severity: Warning
Enabled by default: true

Message:

Class '{0}' directly declares {1} methods and {2} properties and is classified as '{3}'

The arguments are:

{0} = class name
{1} = number of directly declared methods
{2} = number of directly declared properties
{3} = classification
Classification rules

Add the method count and property count together:

0–2 total members  → Small
3–5 total members  → Standard
6 or more          → Large

For example:

class Customer
{
    public string Name { get; set; }
    public int Age { get; set; }

    void Load()
    {
    }

    void Save()
    {
    }
}

has:

2 methods
2 properties
4 total

so report:

Class 'Customer' directly declares 2 methods and 2 properties and is classified as 'Standard'
Important direct-member rule

Nested classes must be treated separately.

For:

class Outer
{
    public int Number { get; set; }

    void First()
    {
    }

    class Inner
    {
        public string Name { get; set; }
        public int Age { get; set; }

        void Second()
        {
        }

        void Third()
        {
        }
    }
}

report:

Class 'Outer' directly declares 1 methods and 1 properties and is classified as 'Small'

Class 'Inner' directly declares 2 methods and 2 properties and is classified as 'Standard'

Outer must not count anything belonging to Inner.

Location

Each diagnostic must use the location of the entire class declaration:

Location location = node.GetLocation();
Every class gets a diagnostic

Even an empty class:

class Empty
{
}

must report:

Class 'Empty' directly declares 0 methods and 0 properties and is classified as 'Small'
Class '{0}' directly declares {1} methods and {2} properties and is classified as '{3}'


{0} = class name
{1} = number of directly declared methods
{2} = number of directly declared properties
{3} = classification
Classification rules

Add the method count and property count together:

0–2 total members  → Small
3–5 total members  → Standard
6 or more          → Large

*/

using AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;
using System.Net.NetworkInformation;

namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
        bb1.Create().WithId("CLIENT401")
        .WithTitle("Class Structure Review")
        .WithMessage("Class '{0}' directly declares {1} methods and {2} properties and is classified as '{3}'")
        .BuildWithSyntaxAnalysisCategory(DiagnosticSeverity.Warning);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    private static string Classification(int count)
    {
        if (count is < 3)
        {
            return "Small";
        }
        if (count is < 6)
        {
            return "Standard";
        }
        return "Large";
    }
    private static Details GetDetails(ClassDeclarationSyntax node)
    {
        int methods = node.MethodCount;
        int properties = node.PropertyCount;
        int totals = methods + properties;
        string details = Classification(totals);
        return new(methods, properties, details);
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(context =>
        {
            ClassDeclarationSyntax node = context.ToClassSyntax;
            string name = node.Name;
            Location location = node.GetLocation();
            Details details = GetDetails(node);
            Diagnostic d = Diagnostic.Create(OnlyRule, location, name, details.MethodCount, details.PropertyCount, details.Classification);
            context.ReportDiagnostic(d);
        }, SyntaxKind.ClassDeclaration);
        context.EnableConcurrentExecution();
    }
}