using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic;
[Trait("Section", "Section01AnalyzerFundamentals")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise01ReportsCompilationDiagnostic()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            "public class SampleClass { }");

        MetadataReference reference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestProject",
                [syntaxTree],
                [reference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson04CreateReportFirstDiagnostic.Exercise01.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "APR0001",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.Equal(
            "The compilation was analyzed",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);
    }
    [Fact]
    public async Task Exercise02ReportsCompilationDiagnostic()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            "public class SampleClass { }");

        MetadataReference reference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestProject",
                [syntaxTree],
                [reference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson04CreateReportFirstDiagnostic.Exercise02.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "APR0002",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            "Compilation analysis completed successfully",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);
    }
    [Fact]
    public async Task Exercise03ReportsTwoCompilationDiagnostics()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            "public class SampleClass { }");

        MetadataReference reference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestProject",
                [syntaxTree],
                [reference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson04CreateReportFirstDiagnostic.Exercise03.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Diagnostic firstDiagnostic =
            diagnostics.Single(x => x.Id == "APR0003");

        Diagnostic secondDiagnostic =
            diagnostics.Single(x => x.Id == "APR0004");

        Assert.Equal(
            DiagnosticSeverity.Warning,
            firstDiagnostic.Severity);

        Assert.Equal(
            "The primary compilation check completed",
            firstDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            firstDiagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Info,
            secondDiagnostic.Severity);

        Assert.Equal(
            "The secondary compilation check completed",
            secondDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            secondDiagnostic.Location);
    }
    [Fact]
    public async Task Exercise04ReportsDiagnosticFromEachCompilationCallback()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            "public class SampleClass { }");

        MetadataReference reference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestProject",
                [syntaxTree],
                [reference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson04CreateReportFirstDiagnostic.Exercise04.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Diagnostic firstDiagnostic =
            diagnostics.Single(x => x.Id == "APR0005");

        Diagnostic secondDiagnostic =
            diagnostics.Single(x => x.Id == "APR0006");

        Assert.Equal(
            DiagnosticSeverity.Warning,
            firstDiagnostic.Severity);

        Assert.Equal(
            "The first compilation check completed",
            firstDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            firstDiagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Info,
            secondDiagnostic.Severity);

        Assert.Equal(
            "The second compilation check completed",
            secondDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            secondDiagnostic.Location);
    }
    [Fact]
    public async Task Exercise05ReportsThreeValidationDiagnostics()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            "public class SampleClass { }");

        MetadataReference reference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestProject",
                [syntaxTree],
                [reference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson04CreateReportFirstDiagnostic.Exercise05.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            3,
            diagnostics.Length);

        Diagnostic configurationDiagnostic =
            diagnostics.Single(x => x.Id == "APR0007");

        Diagnostic projectDiagnostic =
            diagnostics.Single(x => x.Id == "APR0008");

        Diagnostic compilationDiagnostic =
            diagnostics.Single(x => x.Id == "APR0009");

        Assert.Equal(
            DiagnosticSeverity.Info,
            configurationDiagnostic.Severity);

        Assert.Equal(
            "The analyzer configuration was validated",
            configurationDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            configurationDiagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            projectDiagnostic.Severity);

        Assert.Equal(
            "The project-level validation completed",
            projectDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            projectDiagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Info,
            compilationDiagnostic.Severity);

        Assert.Equal(
            "The compilation-level validation completed",
            compilationDiagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            compilationDiagnostic.Location);
    }
}