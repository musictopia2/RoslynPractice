using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties;
[Trait("Section", "Section02WorkingWithDiagnostics")]
public class ExercisesClass
{
    [Fact]
    public async Task ReportsExpectedDiagnosticProperties()
    {
        string source =
            """
            namespace SampleProject;

            public class SampleClass
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "PropertyExerciseAssembly",
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise01
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "PROPERTY101",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            "Compilation 'PropertyExerciseAssembly' has diagnostic property data",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        Assert.Equal(
            2,
            diagnostic.Properties.Count);

        bool foundDataKind =
            diagnostic.Properties.TryGetValue(
                "DataKind",
                out string? dataKind);

        Assert.True(
            foundDataKind,
            "The diagnostic should contain a DataKind property.");

        Assert.Equal(
            "Compilation",
            dataKind);

        bool foundAssemblyName =
            diagnostic.Properties.TryGetValue(
                "AssemblyName",
                out string? assemblyName);

        Assert.True(
            foundAssemblyName,
            "The diagnostic should contain an AssemblyName property.");

        Assert.Equal(
            "PropertyExerciseAssembly",
            assemblyName);
    }

    [Fact]
    public async Task UsesUnknownWhenAssemblyNameIsNull()
    {
        string source =
            """
            public class SampleClass
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                null,
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise01
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "Compilation 'Unknown' has diagnostic property data",
            diagnostic.GetMessage());

        bool foundAssemblyName =
            diagnostic.Properties.TryGetValue(
                "AssemblyName",
                out string? assemblyName);

        Assert.True(
            foundAssemblyName,
            "The diagnostic should contain an AssemblyName property.");

        Assert.Equal(
            "Unknown",
            assemblyName);
    }
    [Fact]
    public async Task ReportsNoForSingleSourceFile()
    {
        string source =
            """
            namespace SampleProject;

            public class SampleClass
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "SingleSourceAssembly",
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise02
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "PROPERTY102",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.Equal(
            "Compilation contains 1 source files",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value}"));

        Assert.True(
            diagnostic.Properties.Count == 2,
            $"Expected 2 properties but found {diagnostic.Properties.Count}. Actual properties: {propertyOutput}");

        bool foundDataKind =
            diagnostic.Properties.TryGetValue(
                "DataKind",
                out string? dataKind);

        Assert.True(
            foundDataKind,
            $"DataKind was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "SourceFiles",
            dataKind);

        bool foundMultipleFiles =
            diagnostic.Properties.TryGetValue(
                "MultipleFiles",
                out string? multipleFiles);

        Assert.True(
            foundMultipleFiles,
            $"MultipleFiles was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "No",
            multipleFiles);
    }

    [Fact]
    public async Task ReportsYesForMultipleSourceFiles()
    {
        string firstSource =
            """
            public class FirstClass
            {
            }
            """;

        string secondSource =
            """
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
                "MultipleSourceAssembly",
                [firstTree, secondTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise02
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "Compilation contains 2 source files",
            diagnostic.GetMessage());

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value}"));

        bool foundMultipleFiles =
            diagnostic.Properties.TryGetValue(
                "MultipleFiles",
                out string? multipleFiles);

        Assert.True(
            foundMultipleFiles,
            $"MultipleFiles was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "Yes",
            multipleFiles);

        Assert.Equal(
            2,
            diagnostic.Properties.Count);
    }

    [Fact]
    public async Task ReportsMainAsEntryPointName()
    {
        string source =
            """
            public class Program
            {
                public static void Main()
                {
                }
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "EntryPointAssembly",
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise03
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "PROPERTY103",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            "Compilation entry point information was recorded",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.True(
            diagnostic.Properties.Count == 2,
            $"Expected 2 properties but found {diagnostic.Properties.Count}. Actual properties: {propertyOutput}");

        bool foundDataKind =
            diagnostic.Properties.TryGetValue(
                "DataKind",
                out string? dataKind);

        Assert.True(
            foundDataKind,
            $"DataKind was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "EntryPoint",
            dataKind);

        bool foundEntryPointName =
            diagnostic.Properties.TryGetValue(
                "EntryPointName",
                out string? entryPointName);

        Assert.True(
            foundEntryPointName,
            $"EntryPointName was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "Main",
            entryPointName);
    }

    [Fact]
    public async Task ReportsNullWhenCompilationHasNoEntryPoint()
    {
        string source =
            """
            public class SampleClass
            {
                public void Run()
                {
                }
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "LibraryAssembly",
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise03
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.Equal(
            2,
            diagnostic.Properties.Count);

        bool foundEntryPointName =
            diagnostic.Properties.TryGetValue(
                "EntryPointName",
                out string? entryPointName);

        Assert.True(
            foundEntryPointName,
            $"EntryPointName was missing. Actual properties: {propertyOutput}");

        Assert.Null(entryPointName);

        Assert.True(
            diagnostic.Properties.ContainsKey("EntryPointName"),
            $"EntryPointName property must exist even when its value is null. Actual properties: {propertyOutput}");
    }
    [Fact]
    public async Task ReportsExpectedCompilationSummaryProperties()
    {
        string firstSource =
            """
            public class Program
            {
                public static void Main()
                {
                }
            }
            """;

        string secondSource =
            """
            public class HelperClass
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "CompilationSummaryAssembly",
                [firstTree, secondTree],
                options: new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise04
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "PROPERTY104",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.Equal(
            "Compilation 'CompilationSummaryAssembly' contains 2 syntax trees",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.True(
            diagnostic.Properties.Count == 3,
            $"Expected 3 properties but found {diagnostic.Properties.Count}. Actual properties: {propertyOutput}");

        bool foundDataKind =
            diagnostic.Properties.TryGetValue(
                "DataKind",
                out string? dataKind);

        Assert.True(
            foundDataKind,
            $"DataKind was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "CompilationSummary",
            dataKind);

        bool foundHasEntryPoint =
            diagnostic.Properties.TryGetValue(
                "HasEntryPoint",
                out string? hasEntryPoint);

        Assert.True(
            foundHasEntryPoint,
            $"HasEntryPoint was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "Yes",
            hasEntryPoint);

        bool foundTreeCount =
            diagnostic.Properties.TryGetValue(
                "TreeCount",
                out string? treeCount);

        Assert.True(
            foundTreeCount,
            $"TreeCount was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "2",
            treeCount);
    }

    [Fact]
    public async Task ReportsNoEntryPointForLibraryCompilation()
    {
        string source =
            """
            public class SampleClass
            {
                public void Run()
                {
                }
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "LibrarySummaryAssembly",
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise04
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.Equal(
            "Compilation 'LibrarySummaryAssembly' contains 1 syntax trees",
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "HasEntryPoint",
                out string? hasEntryPoint),
            $"HasEntryPoint was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "No",
            hasEntryPoint);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "TreeCount",
                out string? treeCount),
            $"TreeCount was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "1",
            treeCount);

        Assert.Equal(
            3,
            diagnostic.Properties.Count);
    }


    [Fact]
    public async Task Exercise04UsesUnknownWhenAssemblyNameIsNull()
    {
        string source =
            """
        public class SampleClass
        {
        }
        """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                null,
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise04
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "Compilation 'Unknown' contains 1 syntax trees",
            diagnostic.GetMessage());
    }
    [Fact]
    public async Task ReportsExpectedProjectAnalysisMetadata()
    {
        string firstSource =
            """
            public class Program
            {
                public static void Main()
                {
                }
            }
            """;

        string secondSource =
            """
            public class HelperClass
            {
            }
            """;

        SyntaxTree firstTree =
            CSharpSyntaxTree.ParseText(firstSource);

        SyntaxTree secondTree =
            CSharpSyntaxTree.ParseText(secondSource);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "ClientProjectAssembly",
                [firstTree, secondTree],
                options: new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise05
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "PROPERTY105",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            "Project 'ClientProjectAssembly' analysis metadata was recorded",
            diagnostic.GetMessage());

        Assert.Equal(
            Location.None,
            diagnostic.Location);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.True(
            diagnostic.Properties.Count == 4,
            $"Expected 4 properties but found {diagnostic.Properties.Count}. Actual properties: {propertyOutput}");

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "DataKind",
                out string? dataKind),
            $"DataKind was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "ProjectAnalysis",
            dataKind);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "AssemblyName",
                out string? assemblyName),
            $"AssemblyName was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "ClientProjectAssembly",
            assemblyName);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "SourceFileCount",
                out string? sourceFileCount),
            $"SourceFileCount was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "2",
            sourceFileCount);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "HasEntryPoint",
                out string? hasEntryPoint),
            $"HasEntryPoint was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "Yes",
            hasEntryPoint);
    }

    [Fact]
    public async Task ReportsNoEntryPointForLibraryProject()
    {
        string source =
            """
            public class SampleClass
            {
                public void Run()
                {
                }
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "LibraryClientProject",
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise05
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "HasEntryPoint",
                out string? hasEntryPoint),
            $"HasEntryPoint was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "No",
            hasEntryPoint);

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "SourceFileCount",
                out string? sourceFileCount),
            $"SourceFileCount was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "1",
            sourceFileCount);

        Assert.Equal(
            4,
            diagnostic.Properties.Count);
    }

    [Fact]
    public async Task Exercise05UsesUnknownWhenAssemblyNameIsNull()
    {
        string source =
            """
            public class SampleClass
            {
            }
            """;

        SyntaxTree syntaxTree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                null,
                [syntaxTree],
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section02WorkingWithDiagnostics
                .Lesson04DiagnosticProperties
                .Exercise05
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        string propertyOutput =
            string.Join(
                ", ",
                diagnostic.Properties.Select(
                    x => $"{x.Key}={x.Value ?? "<null>"}"));

        Assert.Equal(
            "Project 'Unknown' analysis metadata was recorded",
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Properties.TryGetValue(
                "AssemblyName",
                out string? assemblyName),
            $"AssemblyName was missing. Actual properties: {propertyOutput}");

        Assert.Equal(
            "Unknown",
            assemblyName);
    }
}