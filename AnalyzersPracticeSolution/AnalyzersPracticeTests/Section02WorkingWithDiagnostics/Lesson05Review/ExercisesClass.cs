using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section02WorkingWithDiagnostics.Lesson05Review;
[Trait("Section", "Section02WorkingWithDiagnostics")]
public class ExercisesClass
{
    [Fact]
    public async Task NoSyntaxTrees_DoesNotReportDiagnostic()
    {
        var compilation = CSharpCompilation.Create(
            "EmptyLibrary");

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise01
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task SyntaxTrees_ReportsCompilationSummary(int treeCount)
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < treeCount; index++)
        {
            string source =
                $$"""
                namespace Sample{{index}};

                public class Class{{index}}
                {
                }
                """;

            trees.Add(CSharpSyntaxTree.ParseText(source));
        }

        var compilation = CSharpCompilation.Create(
            "ReviewLibrary",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise01
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("REVIEW301", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);

        string expectedMessage =
            $"Compilation 'ReviewLibrary' contains {treeCount} syntax trees";

        Assert.Equal(expectedMessage, diagnostic.GetMessage());

        Location expectedLocation = trees[0].GetRoot().GetLocation();

        Assert.Equal(
            expectedLocation.SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Same(
            trees[0],
            diagnostic.Location.SourceTree);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "TreeCount",
                out string? propertyValue));

        Assert.Equal(
            treeCount.ToString(),
            propertyValue);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task FewerThanTwoSyntaxTrees_DoesNotReportDiagnostic(int treeCount)
    {
        List<SyntaxTree> trees = [];

        if (treeCount == 1)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    "public class FirstClass { }",
                    path: "FirstFile.cs"));
        }

        var compilation = CSharpCompilation.Create(
            "ReviewLibrary",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise02
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task TwoSyntaxTrees_ReportsExpectedDiagnostic()
    {
        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(
                "public class FirstClass { }",
                path: "FirstFile.cs");

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(
                "public class SecondClass { }",
                path: "SecondFile.cs");

        var compilation = CSharpCompilation.Create(
            "ReviewLibrary",
            [firstTree, secondTree]);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise02
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("REVIEW302", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);

        string expectedMessage =
            "Compilation 'ReviewLibrary' links 'FirstFile.cs' with 'SecondFile.cs'";

        Assert.Equal(expectedMessage, diagnostic.GetMessage());

        Assert.True(
            diagnostic.Location != Location.None,
            "The diagnostic must have a primary source location and cannot use Location.None.");

        Assert.NotNull(diagnostic.Location.SourceTree);

        Assert.Same(
            firstTree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            firstTree.GetRoot().GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Location additionalLocation =
            Assert.Single(diagnostic.AdditionalLocations);

        Assert.True(
            additionalLocation != Location.None,
            "The additional location must be a real source location and cannot use Location.None.");

        Assert.NotNull(additionalLocation.SourceTree);

        Assert.Same(
            secondTree,
            additionalLocation.SourceTree);

        Assert.Equal(
            secondTree.GetRoot().GetLocation().SourceSpan,
            additionalLocation.SourceSpan);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "RelatedFileCount",
                out string? propertyValue),
            "The diagnostic must contain the RelatedFileCount property.");

        Assert.Equal("2", propertyValue);
    }
    [Fact]
    public async Task Exercise03_NoSyntaxTrees_DoesNotReportDiagnostic()
    {
        var compilation = CSharpCompilation.Create(
            "EmptyLibrary");

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise03
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(1, "Small")]
    [InlineData(2, "Small")]
    [InlineData(3, "Medium")]
    [InlineData(4, "Medium")]
    [InlineData(5, "Large")]
    [InlineData(7, "Large")]
    public async Task Exercise03_SyntaxTrees_ReportExpectedClassification(
        int treeCount,
        string expectedClassification)
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < treeCount; index++)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    $"public class Class{index} {{ }}",
                    path: $"File{index}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            "ReviewLibrary",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise03
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("REVIEW303", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        string expectedMessage =
            $"Compilation 'ReviewLibrary' is classified as '{expectedClassification}'";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Location != Location.None,
            "Exercise 03 requires a real primary source location. Location.None is not allowed.");

        Assert.NotNull(diagnostic.Location.SourceTree);

        Assert.Same(
            trees[0],
            diagnostic.Location.SourceTree);

        Assert.Equal(
            trees[0].GetRoot().GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Empty(diagnostic.AdditionalLocations);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "TreeCount",
                out string? treeCountValue),
            "Exercise 03 requires the TreeCount diagnostic property.");

        Assert.Equal(
            treeCount.ToString(),
            treeCountValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "Classification",
                out string? classificationValue),
            "Exercise 03 requires the Classification diagnostic property.");

        Assert.Equal(
            expectedClassification,
            classificationValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "AssemblyName",
                out string? assemblyNameValue),
            "Exercise 03 requires the AssemblyName diagnostic property.");

        Assert.Equal(
            "ReviewLibrary",
            assemblyNameValue);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Exercise04_FewerThanThreeSyntaxTrees_DoesNotReportDiagnostic(
    int treeCount)
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < treeCount; index++)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    $"public class Class{index} {{ }}",
                    path: $"File{index}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            "ReviewLibrary",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise04
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(3, "Medium")]
    [InlineData(4, "Medium")]
    [InlineData(5, "Large")]
    [InlineData(7, "Large")]
    public async Task Exercise04_CompilationSummary_ReportsExpectedDiagnostic(
        int treeCount,
        string expectedClassification)
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < treeCount; index++)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    $"public class Class{index} {{ }}",
                    path: $"File{index}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            "ReviewLibrary",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise04
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("REVIEW304", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);

        string expectedMessage =
            $"Compilation 'ReviewLibrary' contains {treeCount} syntax trees and is classified as '{expectedClassification}'";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Location != Location.None,
            "Exercise 04 requires a real primary source location. Location.None is not allowed.");

        Assert.NotNull(diagnostic.Location.SourceTree);

        Assert.Same(
            trees[0],
            diagnostic.Location.SourceTree);

        Assert.Equal(
            trees[0].GetRoot().GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Equal(
            2,
            diagnostic.AdditionalLocations.Count);

        Location firstAdditionalLocation =
            diagnostic.AdditionalLocations[0];

        Assert.True(
            firstAdditionalLocation != Location.None,
            "Exercise 04 requires the first additional location to be a real source location.");

        Assert.Same(
            trees[1],
            firstAdditionalLocation.SourceTree);

        Assert.Equal(
            trees[1].GetRoot().GetLocation().SourceSpan,
            firstAdditionalLocation.SourceSpan);

        Location secondAdditionalLocation =
            diagnostic.AdditionalLocations[1];

        Assert.True(
            secondAdditionalLocation != Location.None,
            "Exercise 04 requires the second additional location to be a real source location.");

        Assert.Same(
            trees[2],
            secondAdditionalLocation.SourceTree);

        Assert.Equal(
            trees[2].GetRoot().GetLocation().SourceSpan,
            secondAdditionalLocation.SourceSpan);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "TreeCount",
                out string? treeCountValue),
            "Exercise 04 requires the TreeCount diagnostic property.");

        Assert.Equal(
            treeCount.ToString(),
            treeCountValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "Classification",
                out string? classificationValue),
            "Exercise 04 requires the Classification diagnostic property.");

        Assert.Equal(
            expectedClassification,
            classificationValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "PrimaryFile",
                out string? primaryFileValue),
            "Exercise 04 requires the PrimaryFile diagnostic property.");

        Assert.Equal(
            "File0.cs",
            primaryFileValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "RelatedFileCount",
                out string? relatedFileCountValue),
            "Exercise 04 requires the RelatedFileCount diagnostic property.");

        Assert.Equal(
            "2",
            relatedFileCountValue);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Exercise05_FewerThanFourSyntaxTrees_DoesNotReportDiagnostic(
    int treeCount)
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < treeCount; index++)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    $"public class Class{index} {{ }}",
                    path: $"File{index}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            "ClientPackage",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise05
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(4, "Standard")]
    [InlineData(5, "Standard")]
    [InlineData(6, "Expanded")]
    [InlineData(7, "Expanded")]
    [InlineData(8, "Large")]
    [InlineData(10, "Large")]
    public async Task Exercise05_SourcePackageReview_ReportsExpectedDiagnostic(
        int treeCount,
        string expectedClassification)
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < treeCount; index++)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    $"public class Class{index} {{ }}",
                    path: $"File{index}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            "ClientPackage",
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise05
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal(
            "CLIENT305",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        string expectedMessage =
            $"Package 'ClientPackage' contains {treeCount} source files, is classified as '{expectedClassification}', and has 3 related review files";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Location != Location.None,
            "Exercise 05 requires a real primary source location. Location.None is not allowed.");

        Assert.NotNull(
            diagnostic.Location.SourceTree);

        Assert.Same(
            trees[0],
            diagnostic.Location.SourceTree);

        Assert.Equal(
            trees[0].GetRoot().GetLocation().SourceSpan,
            diagnostic.Location.SourceSpan);

        Assert.Equal(
            3,
            diagnostic.AdditionalLocations.Count);

        Location firstAdditionalLocation =
            diagnostic.AdditionalLocations[0];

        Assert.True(
            firstAdditionalLocation != Location.None,
            "Exercise 05 requires the first additional location to be a real source location.");

        Assert.Same(
            trees[1],
            firstAdditionalLocation.SourceTree);

        Assert.Equal(
            trees[1].GetRoot().GetLocation().SourceSpan,
            firstAdditionalLocation.SourceSpan);

        Location secondAdditionalLocation =
            diagnostic.AdditionalLocations[1];

        Assert.True(
            secondAdditionalLocation != Location.None,
            "Exercise 05 requires the second additional location to be a real source location.");

        Assert.Same(
            trees[2],
            secondAdditionalLocation.SourceTree);

        Assert.Equal(
            trees[2].GetRoot().GetLocation().SourceSpan,
            secondAdditionalLocation.SourceSpan);

        Location thirdAdditionalLocation =
            diagnostic.AdditionalLocations[2];

        Assert.True(
            thirdAdditionalLocation != Location.None,
            "Exercise 05 requires the third additional location to be a real source location.");

        Assert.Same(
            trees[3],
            thirdAdditionalLocation.SourceTree);

        Assert.Equal(
            trees[3].GetRoot().GetLocation().SourceSpan,
            thirdAdditionalLocation.SourceSpan);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "PackageName",
                out string? packageNameValue),
            "Exercise 05 requires the PackageName diagnostic property.");

        Assert.Equal(
            "ClientPackage",
            packageNameValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "TreeCount",
                out string? treeCountValue),
            "Exercise 05 requires the TreeCount diagnostic property.");

        Assert.Equal(
            treeCount.ToString(),
            treeCountValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "Classification",
                out string? classificationValue),
            "Exercise 05 requires the Classification diagnostic property.");

        Assert.Equal(
            expectedClassification,
            classificationValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "PrimaryFile",
                out string? primaryFileValue),
            "Exercise 05 requires the PrimaryFile diagnostic property.");

        Assert.Equal(
            "File0.cs",
            primaryFileValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "FirstRelatedFile",
                out string? firstRelatedFileValue),
            "Exercise 05 requires the FirstRelatedFile diagnostic property.");

        Assert.Equal(
            "File1.cs",
            firstRelatedFileValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "LastRelatedFile",
                out string? lastRelatedFileValue),
            "Exercise 05 requires the LastRelatedFile diagnostic property.");

        Assert.Equal(
            "File3.cs",
            lastRelatedFileValue);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "RelatedFileCount",
                out string? relatedFileCountValue),
            "Exercise 05 requires the RelatedFileCount diagnostic property.");

        Assert.Equal(
            "3",
            relatedFileCountValue);
    }

    [Fact]
    public async Task Exercise05_NullAssemblyName_UsesUnknown()
    {
        List<SyntaxTree> trees = [];

        for (int index = 0; index < 4; index++)
        {
            trees.Add(
                CSharpSyntaxTree.ParseText(
                    $"public class Class{index} {{ }}",
                    path: $"File{index}.cs"));
        }

        var compilation = CSharpCompilation.Create(
            null,
            trees);

        var analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson05Review
                .Exercise05
                .MainClass();

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

        var diagnostics = await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal(
            "Package 'Unknown' contains 4 source files, is classified as 'Standard', and has 3 related review files",
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "PackageName",
                out string? packageNameValue),
            "Exercise 05 requires the PackageName diagnostic property.");

        Assert.Equal(
            "Unknown",
            packageNameValue);
    }
}