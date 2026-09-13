/*
Enter the requirements for this exercise here.
A client has several generated-style source files that are expected to contain differently named top-level classes. They want an analyzer that detects when the first class in each of the first three source files uses the same class name.

Create the analyzer using only concepts you've already practiced.

Use this descriptor:

ID: ADDLOC005
Title: Duplicate Class Names Across Files
Message: Class '{0}' appears in three source files
Category: DiagnosticLocations
Severity: Warning
Enabled by default: true

The analyzer must register a compilation action.

For each of the first three syntax trees, find the first ClassDeclarationSyntax.

Report no diagnostic when any of these conditions is true:

The compilation contains fewer than 3 syntax trees.
Any of the first three syntax trees contains no class declaration.
The three class names are not all identical.

For example, this should not report:

class Customer { }
class Customer { }
class Order { }

But these three files should produce one diagnostic:

class Customer { }
class Customer { }
class Customer { }

When the three names match:

Use the first class declaration as the primary location.
Use the second and third class declarations as the two additional locations, in that order.
Pass the matching class name as {0}.
Report exactly one diagnostic.

The resulting diagnostic should conceptually be:

ADDLOC005
Message: Class 'Customer' appears in three source files

Primary location
└── Customer class in syntax tree 0

Additional locations
├── Customer class in syntax tree 1
└── Customer class in syntax tree 2
*/

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson03AdditionalDiagnosticLocations.Exercise05;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor OnlyRule { get; } =
       bb1.Create().WithId("ADDLOC005")
       .WithTitle("Duplicate Class Names Across Files")
       .WithMessage("Class '{0}' appears in three source files")
       .WithCategory("DiagnosticLocations")
       .WithSeverity(DiagnosticSeverity.Warning).EnabledByDefault().Build();
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [OnlyRule];
        }
    }
    private DetailsClass? GetPrimaryDetails(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() is < 3)
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

    private DetailsClass? GetDetailsByIndex(Compilation compilation, int index)
    {
        var tree = compilation.SyntaxTrees.ElementAt(index);
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
        DetailsClass output = new(location, name);
        return output;
    }

    private List<DetailsClass> GetSecondaryDetails(Compilation compilation)
    {
        List<DetailsClass> output = [];
        2.Times(x =>
        {
            DetailsClass? details = GetDetailsByIndex(compilation, x);
            if (details is null)
            {
                output.Clear();
                return;
            }
            output.Add(details.Value);
        });
        return output;
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
                if (secondary.Count is not 2)
                {
                    return;
                }
                List<string> names = [];
                names.Add(primary.Value.ClassName);
                foreach (var item in secondary)
                {
                    names.Add(item.ClassName);
                }
                if (names.Distinct().Count() is not 1)
                {
                    return;
                }
                List<Location> locations = [];
                foreach (var item in secondary)
                {
                    locations.Add(item.Location);
                }
                Diagnostic d = Diagnostic.Create(OnlyRule, primary.Value.Location, locations, primary.Value.ClassName);
                context.ReportDiagnostic(d);
            }
        });
        context.EnableConcurrentExecution();
    }
}