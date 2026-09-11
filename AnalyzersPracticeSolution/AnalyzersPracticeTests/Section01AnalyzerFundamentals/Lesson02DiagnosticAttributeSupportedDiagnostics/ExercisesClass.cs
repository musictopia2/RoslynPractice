namespace AnalyzersPracticeTests.Section01AnalyzerFundamentals.Lesson02DiagnosticAttributeSupportedDiagnostics;
[Trait("Section", "Section01AnalyzerFundamentals")]
public class ExercisesClass
{
    [Fact]
    public void Exercise01_HasExpectedSupportedDiagnostic()
    {
        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson02DiagnosticAttributeSupportedDiagnostics
            .Exercise01.MainClass analyzer = new();

        var diagnostics = analyzer.SupportedDiagnostics;

        Assert.Single(diagnostics);

        DiagnosticDescriptor rule = diagnostics[0];

        Assert.Equal("RPA001", rule.Id);
        Assert.Equal(
            "Analyzer practice diagnostic",
            rule.Title.ToString());

        Assert.Equal(
            "Analyzer practice diagnostic was reported",
            rule.MessageFormat.ToString());

        Assert.Equal("Practice", rule.Category);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            rule.DefaultSeverity);

        Assert.True(rule.IsEnabledByDefault);
    }

    [Fact]
    public void Exercise01_IsRegisteredForCSharp()
    {
        Type analyzerType =
            typeof(AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                .Lesson02DiagnosticAttributeSupportedDiagnostics
                .Exercise01.MainClass);

        DiagnosticAnalyzerAttribute? attribute =
            analyzerType
                .GetCustomAttributes(
                    typeof(DiagnosticAnalyzerAttribute),
                    false)
                .Cast<DiagnosticAnalyzerAttribute>()
                .SingleOrDefault();

        Assert.NotNull(attribute);

        Assert.Contains(
            LanguageNames.CSharp,
            attribute.Languages);
    }

    [Fact]
    public void Exercise02_HasExpectedSupportedDiagnostic()
    {
        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson02DiagnosticAttributeSupportedDiagnostics
            .Exercise02.MainClass analyzer = new();

        var diagnostics = analyzer.SupportedDiagnostics;

        Assert.Single(diagnostics);

        DiagnosticDescriptor rule = diagnostics[0];

        Assert.Equal("RPA002", rule.Id);
        Assert.Equal(
            "Code requires review",
            rule.Title.ToString());

        Assert.Equal(
            "This code requires manual review",
            rule.MessageFormat.ToString());

        Assert.Equal("Review", rule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            rule.DefaultSeverity);

        Assert.True(rule.IsEnabledByDefault);
    }

    [Fact]
    public void Exercise02_IsRegisteredForCSharp()
    {
        Type analyzerType =
            typeof(AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                .Lesson02DiagnosticAttributeSupportedDiagnostics
                .Exercise02.MainClass);

        DiagnosticAnalyzerAttribute? attribute =
            analyzerType
                .GetCustomAttributes(
                    typeof(DiagnosticAnalyzerAttribute),
                    false)
                .Cast<DiagnosticAnalyzerAttribute>()
                .SingleOrDefault();

        Assert.NotNull(attribute);

        Assert.Contains(
            LanguageNames.CSharp,
            attribute.Languages);
    }
    [Fact]
    public void Exercise03_HasExpectedSupportedDiagnostics()
    {
        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson02DiagnosticAttributeSupportedDiagnostics
            .Exercise03.MainClass analyzer = new();

        var diagnostics = analyzer.SupportedDiagnostics;

        Assert.Equal(2, diagnostics.Length);

        DiagnosticDescriptor namingRule =
            diagnostics.Single(x => x.Id == "RPA003");

        DiagnosticDescriptor structureRule =
            diagnostics.Single(x => x.Id == "RPA004");

        Assert.Equal(
            "Naming review required",
            namingRule.Title.ToString());

        Assert.Equal(
            "This name requires review",
            namingRule.MessageFormat.ToString());

        Assert.Equal("Naming", namingRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            namingRule.DefaultSeverity);

        Assert.True(namingRule.IsEnabledByDefault);

        Assert.Equal(
            "Structure review required",
            structureRule.Title.ToString());

        Assert.Equal(
            "This code structure requires review",
            structureRule.MessageFormat.ToString());

        Assert.Equal("Structure", structureRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            structureRule.DefaultSeverity);

        Assert.True(structureRule.IsEnabledByDefault);
    }
    [Fact]
    public void Exercise04_HasExpectedSupportedDiagnostics()
    {
        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson02DiagnosticAttributeSupportedDiagnostics
            .Exercise04.MainClass analyzer = new();

        var diagnostics = analyzer.SupportedDiagnostics;

        Assert.Equal(3, diagnostics.Length);

        DiagnosticDescriptor performanceRule =
            diagnostics.Single(x => x.Id == "RPA005");

        DiagnosticDescriptor readabilityRule =
            diagnostics.Single(x => x.Id == "RPA006");

        DiagnosticDescriptor experimentalRule =
            diagnostics.Single(x => x.Id == "RPA007");

        Assert.Equal(
            "Performance review required",
            performanceRule.Title.ToString());

        Assert.Equal(
            "This code may require performance review",
            performanceRule.MessageFormat.ToString());

        Assert.Equal("Performance", performanceRule.Category);
        Assert.Equal(
            DiagnosticSeverity.Warning,
            performanceRule.DefaultSeverity);
        Assert.True(performanceRule.IsEnabledByDefault);

        Assert.Equal(
            "Readability review required",
            readabilityRule.Title.ToString());

        Assert.Equal(
            "This code may require readability review",
            readabilityRule.MessageFormat.ToString());

        Assert.Equal("Readability", readabilityRule.Category);
        Assert.Equal(
            DiagnosticSeverity.Info,
            readabilityRule.DefaultSeverity);
        Assert.True(readabilityRule.IsEnabledByDefault);

        Assert.Equal(
            "Disabled experimental rule",
            experimentalRule.Title.ToString());

        Assert.Equal(
            "This experimental rule is currently disabled",
            experimentalRule.MessageFormat.ToString());

        Assert.Equal("Experimental", experimentalRule.Category);
        Assert.Equal(
            DiagnosticSeverity.Warning,
            experimentalRule.DefaultSeverity);
        Assert.False(experimentalRule.IsEnabledByDefault);
    }
    [Fact]
    public void Exercise05_HasExpectedSupportedDiagnostics()
    {
        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson02DiagnosticAttributeSupportedDiagnostics
            .Exercise05.MainClass analyzer = new();

        var diagnostics = analyzer.SupportedDiagnostics;

        Assert.Equal(3, diagnostics.Length);

        DiagnosticDescriptor correctnessRule =
            diagnostics.Single(x => x.Id == "RPA008");

        DiagnosticDescriptor maintainabilityRule =
            diagnostics.Single(x => x.Id == "RPA009");

        DiagnosticDescriptor experimentalRule =
            diagnostics.Single(x => x.Id == "RPA010");

        Assert.Equal(
            "Possible correctness issue",
            correctnessRule.Title.ToString());

        Assert.Equal(
            "This code may contain a correctness issue",
            correctnessRule.MessageFormat.ToString());

        Assert.Equal(
            "Correctness",
            correctnessRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Error,
            correctnessRule.DefaultSeverity);

        Assert.True(
            correctnessRule.IsEnabledByDefault);

        Assert.Equal(
            "Maintainability review suggested",
            maintainabilityRule.Title.ToString());

        Assert.Equal(
            "This code may be difficult to maintain",
            maintainabilityRule.MessageFormat.ToString());

        Assert.Equal(
            "Maintainability",
            maintainabilityRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            maintainabilityRule.DefaultSeverity);

        Assert.True(
            maintainabilityRule.IsEnabledByDefault);

        Assert.Equal(
            "Experimental analysis available",
            experimentalRule.Title.ToString());

        Assert.Equal(
            "Experimental analysis is available for this code",
            experimentalRule.MessageFormat.ToString());

        Assert.Equal(
            "Experimental",
            experimentalRule.Category);

        Assert.Equal(
            DiagnosticSeverity.Info,
            experimentalRule.DefaultSeverity);

        Assert.False(
            experimentalRule.IsEnabledByDefault);
    }
}