using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section01AnalyzerFundamentals.Lesson05Review;
[Trait("Section", "Section01AnalyzerFundamentals")]
public class ExercisesClass
{
    [Fact]
    public void Exercise01SupportedDiagnosticsShouldBeEmpty()
    {
        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise01
            .MainClass analyzer = new();

        Assert.Empty(analyzer.SupportedDiagnostics);
    }
    [Fact]
    public void Exercise02SupportedDiagnosticsShouldContainRule()
    {
        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise02
            .MainClass analyzer = new();

        DiagnosticDescriptor rule =
            Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal("REVIEW001", rule.Id);
        Assert.Equal("Compilation Reviewed", rule.Title.ToString());
        Assert.Equal(
            "The compilation was analyzed",
            rule.MessageFormat.ToString());
        Assert.Equal("Review", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
    }

    [Fact]
    public async Task Exercise02ShouldReportCompilationDiagnostic()
    {
        const string source =
            """
            class Sample
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [syntaxTree]);

        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise02
            .MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal("REVIEW001", diagnostic.Id);
        Assert.Equal(
            "The compilation was analyzed",
            diagnostic.GetMessage());

        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        Assert.Equal(Location.None, diagnostic.Location);
    }
    [Fact]
    public void Exercise03SupportedDiagnosticsShouldContainBothRules()
    {
        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise03
            .MainClass analyzer = new();

        ImmutableArray<DiagnosticDescriptor> rules =
            analyzer.SupportedDiagnostics;

        Assert.Equal(2, rules.Length);

        DiagnosticDescriptor primaryRule =
            Assert.Single(rules, x => x.Id == "REVIEW101");

        DiagnosticDescriptor secondaryRule =
            Assert.Single(rules, x => x.Id == "REVIEW102");

        Assert.Equal("Primary Review", primaryRule.Title.ToString());
        Assert.Equal(
            "Primary review diagnostic",
            primaryRule.MessageFormat.ToString());
        Assert.Equal("Review", primaryRule.Category);
        Assert.Equal(
            DiagnosticSeverity.Warning,
            primaryRule.DefaultSeverity);
        Assert.True(primaryRule.IsEnabledByDefault);

        Assert.Equal("Secondary Review", secondaryRule.Title.ToString());
        Assert.Equal(
            "Secondary review diagnostic",
            secondaryRule.MessageFormat.ToString());
        Assert.Equal("Review", secondaryRule.Category);
        Assert.Equal(
            DiagnosticSeverity.Info,
            secondaryRule.DefaultSeverity);
        Assert.True(secondaryRule.IsEnabledByDefault);
    }

    [Fact]
    public async Task Exercise03ShouldReportOnlyPrimaryRule()
    {
        const string source =
            """
            class Sample
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [syntaxTree]);

        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise03
            .MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal("REVIEW101", diagnostic.Id);
        Assert.Equal(
            "Primary review diagnostic",
            diagnostic.GetMessage());
        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);
        Assert.Equal(Location.None, diagnostic.Location);
    }
    [Fact]
    public void Exercise04SupportedDiagnosticsShouldContainBothRules()
    {
        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise04
            .MainClass analyzer = new();

        ImmutableArray<DiagnosticDescriptor> rules =
            analyzer.SupportedDiagnostics;

        Assert.Equal(2, rules.Length);

        DiagnosticDescriptor warningRule =
            Assert.Single(rules, x => x.Id == "REVIEW201");

        DiagnosticDescriptor informationRule =
            Assert.Single(rules, x => x.Id == "REVIEW202");

        Assert.Equal(
            "Compilation Warning",
            warningRule.Title.ToString());

        Assert.Equal(
            "Compilation review warning",
            warningRule.MessageFormat.ToString());

        Assert.Equal(
            "Review",
            warningRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            warningRule.DefaultSeverity);

        Assert.True(
            warningRule.IsEnabledByDefault);

        Assert.Equal(
            "Compilation Information",
            informationRule.Title.ToString());

        Assert.Equal(
            "Compilation review information",
            informationRule.MessageFormat.ToString());

        Assert.Equal(
            "Review",
            informationRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            informationRule.DefaultSeverity);

        Assert.True(
            informationRule.IsEnabledByDefault);
    }

    [Fact]
    public async Task Exercise04ShouldReportBothDiagnostics()
    {
        const string source =
            """
            class Sample
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [syntaxTree]);

        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise04
            .MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Diagnostic warningDiagnostic =
            Assert.Single(
                diagnostics,
                x => x.Id == "REVIEW201");

        Diagnostic informationDiagnostic =
            Assert.Single(
                diagnostics,
                x => x.Id == "REVIEW202");

        Assert.Equal(
            "Compilation review warning",
            warningDiagnostic.GetMessage());

        Assert.Equal(
            DiagnosticSeverity.Warning,
            warningDiagnostic.Severity);

        Assert.Equal(
            Location.None,
            warningDiagnostic.Location);

        Assert.Equal(
            "Compilation review information",
            informationDiagnostic.GetMessage());

        Assert.Equal(
            DiagnosticSeverity.Info,
            informationDiagnostic.Severity);

        Assert.Equal(
            Location.None,
            informationDiagnostic.Location);
    }
    [Fact]
    public void Exercise05SupportedDiagnosticsShouldContainAllThreeRules()
    {
        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise05
            .MainClass analyzer = new();

        ImmutableArray<DiagnosticDescriptor> rules =
            analyzer.SupportedDiagnostics;

        Assert.Equal(3, rules.Length);

        DiagnosticDescriptor validationRule =
            Assert.Single(rules, x => x.Id == "CLIENT201");

        DiagnosticDescriptor trackingRule =
            Assert.Single(rules, x => x.Id == "CLIENT202");

        DiagnosticDescriptor availableRule =
            Assert.Single(rules, x => x.Id == "CLIENT203");

        Assert.Equal(
            "Compilation Validation",
            validationRule.Title.ToString());

        Assert.Equal(
            "Compilation validation completed",
            validationRule.MessageFormat.ToString());

        Assert.Equal(
            "ClientReview",
            validationRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            validationRule.DefaultSeverity);

        Assert.True(validationRule.IsEnabledByDefault);

        Assert.Equal(
            "Compilation Tracking",
            trackingRule.Title.ToString());

        Assert.Equal(
            "Compilation tracking completed",
            trackingRule.MessageFormat.ToString());

        Assert.Equal(
            "ClientReview",
            trackingRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            trackingRule.DefaultSeverity);

        Assert.True(trackingRule.IsEnabledByDefault);

        Assert.Equal(
            "Available Client Rule",
            availableRule.Title.ToString());

        Assert.Equal(
            "Additional client rule available",
            availableRule.MessageFormat.ToString());

        Assert.Equal(
            "ClientReview",
            availableRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            availableRule.DefaultSeverity);

        Assert.True(availableRule.IsEnabledByDefault);
    }

    [Fact]
    public async Task Exercise05ShouldReportOnlyTwoDiagnostics()
    {
        const string source =
            """
            class Sample
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [syntaxTree]);

        AnalyzersPracticeLibrary
            .Section01AnalyzerFundamentals
            .Lesson05Review
            .Exercise05
            .MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Diagnostic validationDiagnostic =
            Assert.Single(
                diagnostics,
                x => x.Id == "CLIENT201");

        Diagnostic trackingDiagnostic =
            Assert.Single(
                diagnostics,
                x => x.Id == "CLIENT202");

        Assert.Equal(
            "Compilation validation completed",
            validationDiagnostic.GetMessage());

        Assert.Equal(
            DiagnosticSeverity.Warning,
            validationDiagnostic.Severity);

        Assert.Equal(
            Location.None,
            validationDiagnostic.Location);

        Assert.Equal(
            "Compilation tracking completed",
            trackingDiagnostic.GetMessage());

        Assert.Equal(
            DiagnosticSeverity.Info,
            trackingDiagnostic.Severity);

        Assert.Equal(
            Location.None,
            trackingDiagnostic.Location);

        Assert.DoesNotContain(
            diagnostics,
            x => x.Id == "CLIENT203");
    }
}