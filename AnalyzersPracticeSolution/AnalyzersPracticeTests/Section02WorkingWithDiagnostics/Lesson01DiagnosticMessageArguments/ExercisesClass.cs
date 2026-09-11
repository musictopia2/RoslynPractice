using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section02WorkingWithDiagnostics.Lesson01DiagnosticMessageArguments;
[Trait("Section", "Section02WorkingWithDiagnostics")]
public class ExercisesClass
{
    [Fact]
    public void Exercise01SupportedDiagnosticHasCorrectDescriptor()
    {
        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise01.MainClass analyzer = new();

        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal("RPA101", rule.Id);
        Assert.Equal("Compilation Item", rule.Title.ToString());
        Assert.Equal("Compilation contains '{0}'", rule.MessageFormat.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
    }

    [Fact]
    public async Task Exercise01ReportsFormattedDiagnostic()
    {
        string source =
            """
            public class TestClass
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        MetadataReference objectReference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestCompilation",
                [syntaxTree],
                [objectReference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise01.MainClass analyzer = new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RPA101", diagnostic.Id);
        Assert.Equal(
            "Compilation contains 'Main Compilation'",
            diagnostic.GetMessage());

        Assert.Equal(Location.None, diagnostic.Location);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
    }
    [Fact]
    public void Exercise02SupportedDiagnosticHasCorrectDescriptor()
    {
        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise02.MainClass analyzer = new();

        DiagnosticDescriptor rule =
            Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal("RPA102", rule.Id);
        Assert.Equal("Compilation Name", rule.Title.ToString());
        Assert.Equal(
            "Analyzing compilation '{0}'",
            rule.MessageFormat.ToString());

        Assert.Equal("Practice", rule.Category);
        Assert.Equal(
            DiagnosticSeverity.Info,
            rule.DefaultSeverity);

        Assert.True(rule.IsEnabledByDefault);
    }

    [Theory]
    [InlineData("AccountingLibrary")]
    [InlineData("ShippingLibrary")]
    [InlineData("InventorySystem")]
    public async Task Exercise02UsesCompilationNameAsMessageArgument(
        string compilationName)
    {
        string source =
            """
            public class TestClass
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        MetadataReference objectReference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                compilationName,
                [syntaxTree],
                [objectReference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise02.MainClass analyzer = new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal("RPA102", diagnostic.Id);

        string expectedMessage =
            $"Analyzing compilation '{compilationName}'";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);
    }

    [Fact]
    public void Exercise03SupportedDiagnosticHasCorrectDescriptor()
    {
        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise03.MainClass analyzer = new();

        DiagnosticDescriptor rule =
            Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal("RPA103", rule.Id);
        Assert.Equal("Compilation Details", rule.Title.ToString());

        Assert.Equal(
            "Compilation '{0}' contains {1} syntax trees",
            rule.MessageFormat.ToString());

        Assert.Equal("Practice", rule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            rule.DefaultSeverity);

        Assert.True(rule.IsEnabledByDefault);
    }

    [Theory]
    [InlineData("AccountingLibrary", 1)]
    [InlineData("ShippingLibrary", 2)]
    [InlineData("InventorySystem", 4)]
    public async Task Exercise03UsesCompilationDetailsAsMessageArguments(
        string compilationName,
        int syntaxTreeCount)
    {
        List<SyntaxTree> syntaxTrees = [];

        for (int index = 0; index < syntaxTreeCount; index++)
        {
            string source =
                $$"""
                public class TestClass{{index}}
                {
                }
                """;

            SyntaxTree syntaxTree =
                CSharpSyntaxTree.ParseText(source);

            syntaxTrees.Add(syntaxTree);
        }

        MetadataReference objectReference =
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                compilationName,
                syntaxTrees,
                [objectReference],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise03.MainClass analyzer = new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal("RPA103", diagnostic.Id);

        string expectedMessage =
            $"Compilation '{compilationName}' contains {syntaxTreeCount} syntax trees";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);
    }
    [Fact]
    public void Exercise04SupportedDiagnosticHasCorrectDescriptor()
    {
        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise04.MainClass analyzer = new();

        DiagnosticDescriptor rule =
            Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal("RPA104", rule.Id);
        Assert.Equal("Compilation Summary", rule.Title.ToString());

        Assert.Equal(
            "Compilation '{0}' contains {1} syntax trees and {2} references",
            rule.MessageFormat.ToString());

        Assert.Equal("Practice", rule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            rule.DefaultSeverity);

        Assert.True(rule.IsEnabledByDefault);
    }

    [Theory]
    [InlineData("AccountingLibrary", 1, 1)]
    [InlineData("ShippingLibrary", 2, 2)]
    [InlineData("InventorySystem", 4, 3)]
    public async Task Exercise04UsesCompilationSummaryAsMessageArguments(
        string compilationName,
        int syntaxTreeCount,
        int referenceCount)
    {
        List<SyntaxTree> syntaxTrees = [];

        for (int index = 0; index < syntaxTreeCount; index++)
        {
            string source =
                $$"""
                public class TestClass{{index}}
                {
                }
                """;

            SyntaxTree syntaxTree =
                CSharpSyntaxTree.ParseText(source);

            syntaxTrees.Add(syntaxTree);
        }

        List<MetadataReference> references = [];

        references.Add(
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location));

        if (referenceCount >= 2)
        {
            references.Add(
                MetadataReference.CreateFromFile(
                    typeof(Enumerable).Assembly.Location));
        }

        if (referenceCount >= 3)
        {
            references.Add(
                MetadataReference.CreateFromFile(
                    typeof(List<>).Assembly.Location));
        }

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                compilationName,
                syntaxTrees,
                references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise04.MainClass analyzer = new();

        ImmutableArray<DiagnosticAnalyzer> analyzers =
            [analyzer];

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(analyzers);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal("RPA104", diagnostic.Id);

        string expectedMessage =
            $"Compilation '{compilationName}' contains {syntaxTreeCount} syntax trees and {referenceCount} references";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);
    }
    [Fact]
    public void Exercise05SupportedDiagnosticHasCorrectDescriptor()
    {
        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise05.MainClass analyzer = new();

        DiagnosticDescriptor rule =
            Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal("CLIENT301", rule.Id);
        Assert.Equal(
            "Compilation Analysis Summary",
            rule.Title.ToString());

        Assert.Equal(
            "Compilation '{0}' has {1} syntax trees, {2} references, and is '{3}'",
            rule.MessageFormat.ToString());

        Assert.Equal(
            "ClientReview",
            rule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            rule.DefaultSeverity);

        Assert.True(
            rule.IsEnabledByDefault);
    }

    [Theory]
    [InlineData("TinyLibrary", 1, 1, "Small")]
    [InlineData("MediumLibrary", 2, 2, "Medium")]
    [InlineData("AnotherMediumLibrary", 3, 1, "Medium")]
    [InlineData("LargeLibrary", 4, 3, "Large")]
    [InlineData("VeryLargeLibrary", 6, 2, "Large")]
    public async Task Exercise05ReportsCorrectCompilationSummary(
        string compilationName,
        int syntaxTreeCount,
        int referenceCount,
        string expectedSize)
    {
        List<SyntaxTree> syntaxTrees = [];

        for (int index = 0; index < syntaxTreeCount; index++)
        {
            string source =
                $$"""
                public class TestClass{{index}}
                {
                }
                """;

            SyntaxTree syntaxTree =
                CSharpSyntaxTree.ParseText(source);

            syntaxTrees.Add(syntaxTree);
        }

        List<MetadataReference> references = [];

        references.Add(
            MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location));

        if (referenceCount >= 2)
        {
            references.Add(
                MetadataReference.CreateFromFile(
                    typeof(Enumerable).Assembly.Location));
        }

        if (referenceCount >= 3)
        {
            references.Add(
                MetadataReference.CreateFromFile(
                    typeof(List<>).Assembly.Location));
        }

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                compilationName,
                syntaxTrees,
                references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics
            .Lesson01DiagnosticMessageArguments.Exercise05.MainClass analyzer = new();

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
            "CLIENT301",
            diagnostic.Id);

        string expectedMessage =
            $"Compilation '{compilationName}' has {syntaxTreeCount} syntax trees, {referenceCount} references, and is '{expectedSize}'";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);
    }

}