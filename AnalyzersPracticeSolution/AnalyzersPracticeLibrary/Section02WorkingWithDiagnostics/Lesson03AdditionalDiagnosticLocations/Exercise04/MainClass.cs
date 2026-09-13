/*
Enter the requirements for this exercise here.
Requirements

Register a compilation action. Examine the first class declaration in syntax tree 0 and the first class declaration in syntax tree 1.

If there are fewer than two syntax trees, or either of those trees contains no class declaration, report nothing.

When both classes exist:

The first class declaration is the primary location.
The second class declaration is the only additional location.
{0} is the name of the first class.
{1} is the name of the second class.
Report exactly one diagnostic.

You've already used ClassDeclarationSyntax. To obtain its declared name, use:

classDeclaration.Identifier.Text

For source containing:

public class Customer
{
}

and:

public class CustomerAccount
{
}

the resulting diagnostic message must be:

Class 'Customer' is related to class 'CustomerAccount'

ID: ADDLOC004
Title: Related Classes
Message: Class '{0}' is related to class '{1}'
Category: DiagnosticLocations
Severity: Info
Enabled by default: true

*/

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson03AdditionalDiagnosticLocations.Exercise04;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("ADDLOC004")
       .WithTitle("Related Classes")
       .WithMessage("Class '{0}' is related to class '{1}'")
       .WithCategory("DiagnosticLocations")
       .WithSeverity(DiagnosticSeverity.Info).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    private DetailsClass? GetPrimaryDetails(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() is < 2)
        {
            return null;
        }
        var tree = compilation.SyntaxTrees.First();
        var root = tree.GetRoot();

        ClassDeclarationSyntax? classDeclaration =
            root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
        if (classDeclaration is null)
        {
            return null;
        }
        Location location = classDeclaration.GetLocation();
        string name = classDeclaration.Identifier.Text;
        return new(location, name);
    }
    private List<DetailsClass> GetSecondaryDetails(Compilation compilation)
    {
        var tree = compilation.SyntaxTrees.ElementAt(1);
        var root = tree.GetRoot();
        ClassDeclarationSyntax? classDeclaration =
           root.DescendantNodes()
               .OfType<ClassDeclarationSyntax>()
               .FirstOrDefault();
        if (classDeclaration is null)
        {
            return [];
        }
        Location location = classDeclaration.GetLocation();
        string name = classDeclaration.Identifier.Text;
        DetailsClass details = new(location, name);

        return [details];
    }
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(context =>
        {
            DetailsClass? primary = GetPrimaryDetails(context.Compilation);
            if (primary is not null)
            {
                var secondary = GetSecondaryDetails(context.Compilation);
                if (secondary.Count is not 1)
                {
                    return;
                }
                DetailsClass second = secondary.Single();
                List<Location> locations = [second.Location];
                string secondName = second.ClassName;
                Diagnostic d = Diagnostic.Create(OnlyRule, primary.Value.Location, locations, primary.Value.ClassName,secondName);
                context.ReportDiagnostic(d);
            }
        });
        context.EnableConcurrentExecution();
    }
}