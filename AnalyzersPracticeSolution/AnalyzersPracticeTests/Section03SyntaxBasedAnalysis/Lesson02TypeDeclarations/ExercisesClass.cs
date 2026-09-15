using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson02TypeDeclarations;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Theory]
    [InlineData(
        """
        class Customer
        {
        }
        """,
        "Customer")]
    [InlineData(
        """
        public class OrderService
        {
        }
        """,
        "OrderService")]
    public async Task ReportsDiagnosticForClass(
        string source,
        string expectedClassName)
    {
        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise01.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "SYNTAX301",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"Class '{expectedClassName}' was found",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        SyntaxNode root =
            await tree.GetRootAsync();

        ClassDeclarationSyntax classNode =
            root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

        Assert.Equal(
            classNode.Identifier.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task ReportsDiagnosticForEveryClass()
    {
        string source =
            """
            class Customer
            {
            }

            class Invoice
            {
            }

            class Payment
            {
            }
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise01.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            3,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Customer' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Invoice' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Payment' was found");
    }

    [Fact]
    public async Task NestedClasses_ReportSeparateDiagnostics()
    {
        string source =
            """
            class Outer
            {
                class Inner
                {
                }
            }
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise01.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        List<ClassDeclarationSyntax> classes =
            (await tree.GetRootAsync())
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();

        foreach (ClassDeclarationSyntax classNode in classes)
        {
            Diagnostic diagnostic =
                diagnostics.Single(x =>
                    x.GetMessage() ==
                    $"Class '{classNode.Identifier.Text}' was found");

            Assert.NotEqual(
                Location.None,
                diagnostic.Location);

            Assert.Same(
                tree,
                diagnostic.Location.SourceTree);

            Assert.Equal(
                classNode.Identifier.Span,
                diagnostic.Location.SourceSpan);
        }
    }

    [Fact]
    public async Task OtherTypeDeclarations_DoNotReportDiagnostics()
    {
        string source =
            """
            struct CustomerStruct
            {
            }

            interface ICustomer
            {
            }

            enum CustomerType
            {
                Standard,
                Premium
            }

            record CustomerRecord;
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise01.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task MixedTypeDeclarations_ReportsOnlyClasses()
    {
        string source =
            """
            struct Invoice
            {
            }

            class Customer
            {
            }

            interface IService
            {
            }

            class Payment
            {
            }

            enum Status
            {
                Pending,
                Complete
            }
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise01.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Customer' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Payment' was found");
    }
    [Theory]
    [InlineData(
        """
        struct Coordinates
        {
        }
        """,
        "Coordinates")]
    [InlineData(
        """
        public struct CustomerData
        {
        }
        """,
        "CustomerData")]
    public async Task ReportsDiagnosticForStruct(
        string source,
        string expectedName)
    {
        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise02.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "SYNTAX302",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"Struct '{expectedName}' was found",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        SyntaxNode root =
            await tree.GetRootAsync();

        StructDeclarationSyntax structNode =
            root.DescendantNodes()
                .OfType<StructDeclarationSyntax>()
                .Single();

        Assert.Equal(
            structNode.Identifier.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task ReportsDiagnosticsForClassesAndStructs()
    {
        string source =
            """
            class Customer
            {
            }

            struct Coordinates
            {
            }

            class Order
            {
            }

            struct Measurement
            {
            }
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise02.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            4,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Customer' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Struct 'Coordinates' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Order' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Struct 'Measurement' was found");
    }

    [Fact]
    public async Task NestedClassAndStruct_ReportSeparateDiagnostics()
    {
        string source =
            """
            class Container
            {
                struct Item
                {
                }
            }
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise02.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Diagnostic classDiagnostic =
            diagnostics.Single(x =>
                x.GetMessage() ==
                "Class 'Container' was found");

        Diagnostic structDiagnostic =
            diagnostics.Single(x =>
                x.GetMessage() ==
                "Struct 'Item' was found");

        Assert.NotEqual(
            Location.None,
            classDiagnostic.Location);

        Assert.NotEqual(
            Location.None,
            structDiagnostic.Location);

        SyntaxNode root =
            await tree.GetRootAsync();

        ClassDeclarationSyntax classNode =
            root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

        StructDeclarationSyntax structNode =
            root.DescendantNodes()
                .OfType<StructDeclarationSyntax>()
                .Single();

        Assert.Same(
            tree,
            classDiagnostic.Location.SourceTree);

        Assert.Equal(
            classNode.Identifier.Span,
            classDiagnostic.Location.SourceSpan);

        Assert.Same(
            tree,
            structDiagnostic.Location.SourceTree);

        Assert.Equal(
            structNode.Identifier.Span,
            structDiagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task ExcludedTypeDeclarations_DoNotReportDiagnostics()
    {
        string source =
            """
            interface IService
            {
            }

            enum Status
            {
                Pending,
                Complete
            }

            record Customer;
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise02.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task MixedDeclarations_ReportOnlyClassesAndStructs()
    {
        string source =
            """
            interface IService
            {
            }

            struct Coordinates
            {
            }

            enum Status
            {
                Active
            }

            class Customer
            {
            }

            record Order;
            """;

        SyntaxTree tree =
            CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise02.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Struct 'Coordinates' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Customer' was found");
    }
    [Theory]
    [InlineData(
        "public interface ICustomer { }",
        "ICustomer",
        "Public")]
    [InlineData(
        "interface IService { }",
        "IService",
        "NonPublic")]
    [InlineData(
        "internal interface IRepository { }",
        "IRepository",
        "NonPublic")]
    public async Task Exercise03_Interface_ReportsExpectedDiagnostic(
        string source,
        string expectedName,
        string expectedAccessibility)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise03.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "SYNTAX303",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"Interface '{expectedName}' has accessibility '{expectedAccessibility}'",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        InterfaceDeclarationSyntax interfaceNode =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .Single();

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            interfaceNode.Identifier.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task Exercise03_MultipleInterfaces_ReportOneDiagnosticEach()
    {
        string source =
            """
            public interface ICustomer
            {
            }

            interface IService
            {
            }

            public interface IRepository
            {
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise03.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(3, diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'ICustomer' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IService' has accessibility 'NonPublic'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IRepository' has accessibility 'Public'");
    }

    [Fact]
    public async Task Exercise03_NestedInterfaces_ReportIndependently()
    {
        string source =
            """
            class Container
            {
                public interface IPublicService
                {
                }

                private interface IPrivateService
                {
                }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise03.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IPublicService' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IPrivateService' has accessibility 'NonPublic'");
    }

    [Fact]
    public async Task Exercise03_OtherTypeDeclarations_DoNotReportDiagnostics()
    {
        string source =
            """
            public class Customer
            {
            }

            public struct Coordinates
            {
            }

            public record CustomerRecord;

            public enum Status
            {
                Active,
                Inactive
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise03.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Exercise03_MixedDeclarations_ReportOnlyInterfaces()
    {
        string source =
            """
            public class Customer
            {
            }

            public interface ICustomer
            {
            }

            public struct Coordinates
            {
            }

            interface IService
            {
            }

            public record CustomerRecord;
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise03.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'ICustomer' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IService' has accessibility 'NonPublic'");
    }
    [Fact]
    public void Exercise04_SupportedDiagnostics_ContainsExpectedRule()
    {
        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise04.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticDescriptor> rules =
            analyzer.SupportedDiagnostics;

        Assert.NotEmpty(rules);

        Assert.Contains(
            rules,
            rule => rule.Id == "SYNTAX304");
    }

    [Theory]
    [InlineData(
        "public record Customer(string Name);",
        "Customer",
        "Record Class",
        "Public")]
    [InlineData(
        "record Order(int Number);",
        "Order",
        "Record Class",
        "NonPublic")]
    [InlineData(
        "public record struct Coordinate(int X, int Y);",
        "Coordinate",
        "Record Struct",
        "Public")]
    [InlineData(
        "internal record struct Measurement(double Value);",
        "Measurement",
        "Record Struct",
        "NonPublic")]
    public async Task Exercise04_Record_ReportsExpectedDiagnostic(
        string source,
        string expectedName,
        string expectedRecordKind,
        string expectedAccessibility)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "SYNTAX304",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"{expectedRecordKind} '{expectedName}' has accessibility '{expectedAccessibility}'",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        RecordDeclarationSyntax recordNode =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<RecordDeclarationSyntax>()
                .Single();

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            recordNode.Identifier.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task Exercise04_RecordClassAndRecordStruct_ReportIndependently()
    {
        string source =
            """
        public record Customer(string Name);

        record Order(int Number);

        public record struct Coordinate(int X, int Y);

        internal record struct Measurement(double Value);
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            4,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'Customer' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'Order' has accessibility 'NonPublic'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Coordinate' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Measurement' has accessibility 'NonPublic'");
    }

    [Fact]
    public async Task Exercise04_NestedRecords_ReportIndependently()
    {
        string source =
            """
        public class Container
        {
            public record Customer(string Name);

            private record struct Coordinate(int X, int Y);
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'Customer' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Coordinate' has accessibility 'NonPublic'");
    }

    [Fact]
    public async Task Exercise04_OtherTypeDeclarations_DoNotReportDiagnostics()
    {
        string source =
            """
        public class Customer
        {
        }

        public struct Coordinate
        {
        }

        public interface IService
        {
        }

        public enum Status
        {
            Active,
            Inactive
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Exercise04_MixedDeclarations_ReportOnlyRecords()
    {
        string source =
            """
        public class Customer
        {
        }

        public record CustomerRecord;

        public interface IService
        {
        }

        record struct Coordinate(int X, int Y);

        public struct Measurement
        {
        }

        public enum Status
        {
            Active
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'CustomerRecord' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Coordinate' has accessibility 'NonPublic'");
    }
    [Fact]
    public void Exercise05_SupportedDiagnostics_ContainsExpectedRule()
    {
        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        ImmutableArray<DiagnosticDescriptor> rules =
            analyzer.SupportedDiagnostics;

        Assert.NotEmpty(rules);

        Assert.Contains(
            rules,
            rule => rule.Id == "SYNTAX305");
    }

    [Theory]
    [InlineData(
        "public class Customer { }",
        "Class",
        "Customer")]
    [InlineData(
        "public struct Coordinate { }",
        "Struct",
        "Coordinate")]
    [InlineData(
        "public interface IService { }",
        "Interface",
        "IService")]
    [InlineData(
        "public record Order;",
        "Record Class",
        "Order")]
    [InlineData(
        "public record struct Measurement;",
        "Record Struct",
        "Measurement")]
    public async Task Exercise05_EachSupportedType_ReportsExpectedKind(
        string source,
        string expectedKind,
        string expectedName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.Equal(
            "SYNTAX305",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"{expectedKind} '{expectedName}' has accessibility 'Public'",
            diagnostic.GetMessage());
    }

    [Theory]
    [InlineData(
        "class Customer { }",
        "Class",
        "Customer")]
    [InlineData(
        "internal struct Coordinate { }",
        "Struct",
        "Coordinate")]
    [InlineData(
        "interface IService { }",
        "Interface",
        "IService")]
    [InlineData(
        "internal record Order;",
        "Record Class",
        "Order")]
    [InlineData(
        "record struct Measurement;",
        "Record Struct",
        "Measurement")]
    public async Task Exercise05_NonPublicTypes_ReportNonPublic(
        string source,
        string expectedKind,
        string expectedName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Assert.Equal(
            $"{expectedKind} '{expectedName}' has accessibility 'NonPublic'",
            diagnostics[0].GetMessage());
    }

    [Fact]
    public async Task Exercise05_DiagnosticLocation_IsIdentifierOnly()
    {
        string source =
            """
        public interface ICustomerService
        {
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics[0];

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        InterfaceDeclarationSyntax interfaceNode =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .Single();

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            interfaceNode.Identifier.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task Exercise05_RecordKinds_AreDistinguished()
    {
        string source =
            """
        public record Customer;
        public record struct Coordinate;
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'Customer' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Coordinate' has accessibility 'Public'");
    }

    [Fact]
    public async Task Exercise05_NestedTypes_ReportIndependently()
    {
        string source =
            """
        public class Container
        {
            private struct Coordinate
            {
            }

            public interface IService
            {
            }

            record Order;

            public record struct Measurement;
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            5,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Class 'Container' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Struct 'Coordinate' has accessibility 'NonPublic'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IService' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'Order' has accessibility 'NonPublic'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Measurement' has accessibility 'Public'");
    }

    [Fact]
    public async Task Exercise05_EnumDoesNotReportDiagnostic()
    {
        string source =
            """
        public enum Status
        {
            Active,
            Inactive
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Exercise05_MixedClientDeclarations_ReportAllSupportedTypes()
    {
        string source =
            """
        public class Customer
        {
        }

        internal struct Coordinate
        {
        }

        public interface IRepository
        {
        }

        record Order(int Number);

        public record struct Measurement(double Value);

        public enum Status
        {
            Active
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree],
                [
                    MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
                ],
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson02TypeDeclarations.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            5,
            diagnostics.Length);

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Class 'Customer' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Struct 'Coordinate' has accessibility 'NonPublic'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Interface 'IRepository' has accessibility 'Public'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Class 'Order' has accessibility 'NonPublic'");

        Assert.Contains(
            diagnostics,
            diagnostic =>
                diagnostic.GetMessage() ==
                "Record Struct 'Measurement' has accessibility 'Public'");
    }
}