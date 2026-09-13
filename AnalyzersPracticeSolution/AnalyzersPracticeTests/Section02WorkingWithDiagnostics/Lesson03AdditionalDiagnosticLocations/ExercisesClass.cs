using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section02WorkingWithDiagnostics.Lesson03AdditionalDiagnosticLocations;
[Trait("Section", "Section02WorkingWithDiagnostics")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise01ReportsPrimaryAndAdditionalLocation()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class FirstClass
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public class SecondClass
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise01.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "ADDLOC001",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            firstTree,
            diagnostic.Location.SourceTree);

        Assert.Single(
            diagnostic.AdditionalLocations);

        Assert.Equal(
            secondTree,
            diagnostic.AdditionalLocations[0].SourceTree);
    }

    [Fact]
    public async Task Exercise01DoesNotReportWithOnlyOneSyntaxTree()
    {
        const string source =
            """
            namespace OnlyFile;

            public class OnlyClass
            {
            }
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise01.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }
    [Fact]
    public async Task Exercise02ReportsPrimaryAndTwoAdditionalLocations()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class FirstClass
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public class SecondClass
            {
            }
            """;

        const string thirdSource =
            """
            namespace ThirdFile;

            public class ThirdClass
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        SyntaxTree thirdTree =
            CSharpSyntaxTree.ParseText(thirdSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree, thirdTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise02.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "ADDLOC002",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.Equal(
            firstTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            2,
            diagnostic.AdditionalLocations.Count);

        Assert.Equal(
            secondTree,
            diagnostic.AdditionalLocations[0].SourceTree);

        Assert.Equal(
            thirdTree,
            diagnostic.AdditionalLocations[1].SourceTree);
    }

    [Fact]
    public async Task Exercise02DoesNotReportWithOnlyTwoSyntaxTrees()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class FirstClass
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public class SecondClass
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise02.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Exercise03ReportsClassDeclarationLocations()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class FirstClass
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public class SecondClass
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise03.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "ADDLOC003",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        ClassDeclarationSyntax firstClass =
            firstTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        ClassDeclarationSyntax secondClass =
            secondTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        Assert.Equal(
            firstClass.GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Equal(
            firstTree,
            diagnostic.Location.SourceTree);

        Assert.Single(
            diagnostic.AdditionalLocations);

        Assert.Equal(
            secondClass.GetLocation().SourceSpan,
            diagnostic.AdditionalLocations[0].SourceSpan);

        Assert.Equal(
            secondTree,
            diagnostic.AdditionalLocations[0].SourceTree);
    }

    [Fact]
    public async Task Exercise03DoesNotReportWhenSecondTreeHasNoClass()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class FirstClass
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public struct SecondStruct
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise03.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }
    [Fact]
    public async Task Exercise04ReportsRelatedClassesWithMessageArguments()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class Customer
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public class CustomerAccount
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise04.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "ADDLOC004",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            "Class 'Customer' is related to class 'CustomerAccount'",
            diagnostic.GetMessage());

        ClassDeclarationSyntax firstClass =
            firstTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        ClassDeclarationSyntax secondClass =
            secondTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        Assert.Equal(
            firstTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            firstClass.GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Single(
            diagnostic.AdditionalLocations);

        Assert.Equal(
            secondTree,
            diagnostic.AdditionalLocations[0].SourceTree);

        Assert.Equal(
            secondClass.GetLocation().SourceSpan,
            diagnostic.AdditionalLocations[0].SourceSpan);
    }

    [Fact]
    public async Task Exercise04DoesNotReportWhenSecondTreeHasNoClass()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class Customer
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public struct CustomerAccount
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise04.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }
    [Fact]
    public async Task Exercise05ReportsWhenAllThreeClassNamesMatch()
    {
        const string firstSource =
            """
            namespace FirstFile;

            public class Customer
            {
            }
            """;

        const string secondSource =
            """
            namespace SecondFile;

            public class Customer
            {
            }
            """;

        const string thirdSource =
            """
            namespace ThirdFile;

            public class Customer
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        SyntaxTree thirdTree =
            CSharpSyntaxTree.ParseText(thirdSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree, thirdTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "ADDLOC005",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.Equal(
            "Class 'Customer' appears in three source files",
            diagnostic.GetMessage());

        ClassDeclarationSyntax firstClass =
            firstTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        ClassDeclarationSyntax secondClass =
            secondTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        ClassDeclarationSyntax thirdClass =
            thirdTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

        Assert.Equal(
            firstTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            firstClass.GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Equal(
            2,
            diagnostic.AdditionalLocations.Count);

        Assert.Equal(
            secondTree,
            diagnostic.AdditionalLocations[0].SourceTree);

        Assert.Equal(
            secondClass.GetLocation().SourceSpan,
            diagnostic.AdditionalLocations[0].SourceSpan);

        Assert.Equal(
            thirdTree,
            diagnostic.AdditionalLocations[1].SourceTree);

        Assert.Equal(
            thirdClass.GetLocation().SourceSpan,
            diagnostic.AdditionalLocations[1].SourceSpan);
    }

    [Fact]
    public async Task Exercise05DoesNotReportWhenOnlyTwoNamesMatch()
    {
        const string firstSource =
            """
            public class Customer
            {
            }
            """;

        const string secondSource =
            """
            public class Customer
            {
            }
            """;

        const string thirdSource =
            """
            public class Order
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        SyntaxTree thirdTree =
            CSharpSyntaxTree.ParseText(thirdSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree, thirdTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Exercise05DoesNotReportWhenOneTreeHasNoClass()
    {
        const string firstSource =
            """
            public class Customer
            {
            }
            """;

        const string secondSource =
            """
            public struct Customer
            {
            }
            """;

        const string thirdSource =
            """
            public class Customer
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        SyntaxTree thirdTree =
            CSharpSyntaxTree.ParseText(thirdSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [firstTree, secondTree, thirdTree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson03AdditionalDiagnosticLocations
                .Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }
}