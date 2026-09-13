using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
namespace AnalyzersPracticeTests.Section02WorkingWithDiagnostics.Lesson02DiagnosticLocations;
[Trait("Section", "Section02WorkingWithDiagnostics")]
public class ExercisesClass
{
    [Fact]
    public void Exercise01ReportsDiagnosticAtSourceLocation()
    {
        const string source = """
            namespace TestProject;

            public class Customer
            {
            }
            """;

        CSharpCompilation compilation = CreateCompilation(source);

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise01.MainClass());

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal("LOCATION101", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "The compilation contains source code at this location",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.True(diagnostic.Location.IsInSource);

        Assert.NotNull(diagnostic.Location.SourceTree);

        Assert.Same(
            compilation.SyntaxTrees.First(),
            diagnostic.Location.SourceTree);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        return CSharpCompilation.Create(
            "TestCompilation",
            [syntaxTree],
            [
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));
    }

    private static Diagnostic[] GetDiagnostics(
        CSharpCompilation compilation,
        DiagnosticAnalyzer analyzer)
    {
        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync()
                .GetAwaiter()
                .GetResult();

        return [.. diagnostics];
    }
    [Fact]
    public void Exercise01DoesNotReportDiagnosticWhenCompilationHasNoSyntaxTrees()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise01.MainClass());

        Assert.Empty(diagnostics);
    }
    [Fact]
    public void Exercise02ReportsDiagnosticAtSecondSyntaxTreeLocation()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText("""
            namespace TestProject;

            public class FirstClass
            {
            }
            """);

        SyntaxTree secondTree = CSharpSyntaxTree.ParseText("""
            namespace TestProject;

            public class SecondClass
            {
            }
            """);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree, secondTree],
            [
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise02.MainClass());

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal("LOCATION102", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "The selected source file is at index 1",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.True(diagnostic.Location.IsInSource);

        Assert.NotNull(diagnostic.Location.SourceTree);

        Assert.Same(
            secondTree,
            diagnostic.Location.SourceTree);
    }

    [Fact]
    public void Exercise02DoesNotReportDiagnosticWhenSecondSyntaxTreeDoesNotExist()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText("""
            namespace TestProject;

            public class FirstClass
            {
            }
            """);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree],
            [
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise02.MainClass());

        Assert.Empty(diagnostics);
    }
    [Fact]
    public void Exercise03ReportsDiagnosticAtSpecifiedTextSpan()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            "abcdefghij");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [syntaxTree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise03.MainClass());

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal("LOCATION103", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "The selected source span starts at 2 and has length 4",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.True(diagnostic.Location.IsInSource);

        Assert.Same(
            syntaxTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            2,
            diagnostic.Location.SourceSpan.Start);

        Assert.Equal(
            4,
            diagnostic.Location.SourceSpan.Length);
    }

    [Fact]
    public void Exercise03DoesNotReportDiagnosticWhenCompilationHasNoSyntaxTrees()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise03.MainClass());

        Assert.Empty(diagnostics);
    }
    [Fact]
    public void Exercise04ReportsDiagnosticAtSpecifiedSpanInThirdSyntaxTree()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText(
            "abcdefghij");

        SyntaxTree secondTree = CSharpSyntaxTree.ParseText(
            "klmnopqrst");

        SyntaxTree thirdTree = CSharpSyntaxTree.ParseText(
            "uvwxyzabcd");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree, secondTree, thirdTree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise04.MainClass());

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal("LOCATION104", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);

        Assert.Equal(
            "Syntax tree 2 contains the selected span from 3 through 8",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.True(diagnostic.Location.IsInSource);

        Assert.Same(
            thirdTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            3,
            diagnostic.Location.SourceSpan.Start);

        Assert.Equal(
            5,
            diagnostic.Location.SourceSpan.Length);

        Assert.Equal(
            8,
            diagnostic.Location.SourceSpan.End);
    }

    [Fact]
    public void Exercise04DoesNotReportDiagnosticWhenThirdSyntaxTreeDoesNotExist()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText(
            "abcdefghij");

        SyntaxTree secondTree = CSharpSyntaxTree.ParseText(
            "klmnopqrst");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree, secondTree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise04.MainClass());

        Assert.Empty(diagnostics);
    }
    [Fact]
    public void Exercise05ReportsDiagnosticAtRequestedPreviewSpan()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText(
            "abcdefghij");

        SyntaxTree secondTree = CSharpSyntaxTree.ParseText(
            "0123456789abcdef");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree, secondTree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise05.MainClass());

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal("LOCATION105", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);

        Assert.Equal(
            "Source file 1 has a preview starting at 4 with length 6",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.True(diagnostic.Location.IsInSource);

        Assert.Same(
            secondTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            4,
            diagnostic.Location.SourceSpan.Start);

        Assert.Equal(
            6,
            diagnostic.Location.SourceSpan.Length);

        Assert.Equal(
            10,
            diagnostic.Location.SourceSpan.End);
    }

    [Fact]
    public void Exercise05DoesNotReportDiagnosticWhenSecondSyntaxTreeDoesNotExist()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText(
            "abcdefghij");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise05.MainClass());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Exercise05DoesNotReportDiagnosticWhenPreviewWouldExtendPastSourceText()
    {
        SyntaxTree firstTree = CSharpSyntaxTree.ParseText(
            "abcdefghij");

        SyntaxTree secondTree = CSharpSyntaxTree.ParseText(
            "12345678");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            [firstTree, secondTree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] diagnostics = GetDiagnostics(
            compilation,
            new AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
                .Lesson02DiagnosticLocations.Exercise05.MainClass());

        Assert.Empty(diagnostics);
    }
}