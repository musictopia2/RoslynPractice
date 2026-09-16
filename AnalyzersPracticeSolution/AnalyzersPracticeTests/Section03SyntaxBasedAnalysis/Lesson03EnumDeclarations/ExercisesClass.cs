using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson03EnumDeclarations;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise01ReportsEnumNameAndMemberCount()
    {
        string source =
            """
        public enum EnumDifficulty
        {
            Easy,
            Normal,
            Hard
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            path: "EnumDifficulty.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise01.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("SYNTAX301", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "Enum 'EnumDifficulty' contains 3 members",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        string diagnosticText =
            source.Substring(
                diagnostic.Location.SourceSpan.Start,
                diagnostic.Location.SourceSpan.Length);

        Assert.Equal(
            "EnumDifficulty",
            diagnosticText);
    }
    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise02ReportsFirstAndLastEnumMembers()
    {
        string source =
            """
        public enum EnumDifficulty
        {
            Easy,
            Normal,
            Hard
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            path: "EnumDifficulty.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise02.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("SYNTAX302", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "Enum 'EnumDifficulty' starts with 'Easy' and ends with 'Hard'",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        string diagnosticText =
            source.Substring(
                diagnostic.Location.SourceSpan.Start,
                diagnostic.Location.SourceSpan.Length);

        Assert.Equal(
            "EnumDifficulty",
            diagnosticText);
    }

    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise02DoesNotReportForEmptyEnum()
    {
        string source =
            """
        public enum EnumEmpty
        {
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise02.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }
    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise03ReportsExplicitAndTotalMemberCounts()
    {
        string source =
            """
        public enum EnumDifficulty
        {
            Easy,
            Normal = 5,
            Hard,
            Extreme = 20
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            path: "EnumDifficulty.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise03.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("SYNTAX303", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "Enum 'EnumDifficulty' has 2 explicitly assigned members out of 4 total members",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        string diagnosticText =
            source.Substring(
                diagnostic.Location.SourceSpan.Start,
                diagnostic.Location.SourceSpan.Length);

        Assert.Equal(
            "EnumDifficulty",
            diagnosticText);
    }

    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise03ReportsZeroWhenNoMembersHaveExplicitValues()
    {
        string source =
            """
        public enum EnumDirection
        {
            North,
            South,
            East,
            West
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise03.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal(
            "Enum 'EnumDirection' has 0 explicitly assigned members out of 4 total members",
            diagnostic.GetMessage());
    }

    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise03ReportsForEmptyEnum()
    {
        string source =
            """
        public enum EnumEmpty
        {
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise03.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal(
            "Enum 'EnumEmpty' has 0 explicitly assigned members out of 0 total members",
            diagnostic.GetMessage());
    }
    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task Exercise04ReportsEachExplicitEnumMemberValue()
    {
        string source =
            """
        public enum EnumPermissions
        {
            None = 0,
            Read = 1,
            Write,
            ReadWrite = Read | Write
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            path: "EnumPermissions.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson03EnumDeclarations.Exercise04.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(3, diagnostics.Length);

        Diagnostic first = diagnostics[0];
        Diagnostic second = diagnostics[1];
        Diagnostic third = diagnostics[2];

        Assert.Equal("SYNTAX304", first.Id);
        Assert.Equal("SYNTAX304", second.Id);
        Assert.Equal("SYNTAX304", third.Id);

        Assert.Equal(DiagnosticSeverity.Info, first.Severity);
        Assert.Equal(DiagnosticSeverity.Info, second.Severity);
        Assert.Equal(DiagnosticSeverity.Info, third.Severity);

        Assert.Equal(
            "Enum member 'None' explicitly uses value '0'",
            first.GetMessage());

        Assert.Equal(
            "Enum member 'Read' explicitly uses value '1'",
            second.GetMessage());

        Assert.Equal(
            "Enum member 'ReadWrite' explicitly uses value 'Read | Write'",
            third.GetMessage());

        Assert.NotEqual(
            Location.None,
            first.Location);

        Assert.NotEqual(
            Location.None,
            second.Location);

        Assert.NotEqual(
            Location.None,
            third.Location);

        Assert.Same(tree, first.Location.SourceTree);
        Assert.Same(tree, second.Location.SourceTree);
        Assert.Same(tree, third.Location.SourceTree);

        string firstLocationText =
            source.Substring(
                first.Location.SourceSpan.Start,
                first.Location.SourceSpan.Length);

        string secondLocationText =
            source.Substring(
                second.Location.SourceSpan.Start,
                second.Location.SourceSpan.Length);

        string thirdLocationText =
            source.Substring(
                third.Location.SourceSpan.Start,
                third.Location.SourceSpan.Length);

        Assert.Equal("None", firstLocationText);
        Assert.Equal("Read", secondLocationText);
        Assert.Equal("ReadWrite", thirdLocationText);
    }
    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task MixedExplicitAndImplicitMembers_ReportsDiagnostic()
    {
        string source = """
            public enum EnumPermissions
            {
                None = 0,
                Read = 1,
                Write,
                Execute,
                ReadWrite = Read | Write
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
                .Lesson03EnumDeclarations.Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("SYNTAX305", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            "Enum 'EnumPermissions' mixes value styles: 3 explicit and 2 implicit members; first explicit member is 'None = 0' and last explicit member is 'ReadWrite = Read | Write'",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        string locationText =
            diagnostic.Location.SourceTree!
                .GetText()
                .ToString(diagnostic.Location.SourceSpan);

        Assert.Equal(
            "EnumPermissions",
            locationText);
    }

    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task AllImplicitMembers_DoesNotReportDiagnostic()
    {
        string source = """
            public enum EnumAllImplicit
            {
                First,
                Second,
                Third
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
                .Lesson03EnumDeclarations.Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task AllExplicitMembers_DoesNotReportDiagnostic()
    {
        string source = """
            public enum EnumAllExplicit
            {
                First = 1,
                Second = 2,
                Third = 3
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
                .Lesson03EnumDeclarations.Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    [Trait("Section", "Section03SyntaxBasedAnalysis")]
    public async Task EmptyEnum_DoesNotReportDiagnostic()
    {
        string source = """
            public enum EnumEmpty
            {
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
                .Lesson03EnumDeclarations.Exercise05.MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }
}