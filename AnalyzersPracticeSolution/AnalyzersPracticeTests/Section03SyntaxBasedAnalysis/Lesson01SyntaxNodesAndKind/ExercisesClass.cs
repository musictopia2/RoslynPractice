using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind;
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
        class OrderService
        {
        }
        """,
        "OrderService")]
    public async Task ReportsDiagnosticForClass(
        string source,
        string expectedClassName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson01SyntaxNodesAndKind.Exercise01.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Single(diagnostics);

        Diagnostic diagnostic = diagnostics.Single();

        Assert.Equal("SYNTAX101", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        Assert.Equal(
            $"Class '{expectedClassName}' was found",
            diagnostic.GetMessage());

        Assert.NotEqual(Location.None, diagnostic.Location);

        SyntaxNode root = await tree.GetRootAsync();

        ClassDeclarationSyntax classNode =
            root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

        Assert.Equal(
            classNode.Span,
            diagnostic.Location.SourceSpan);

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);
    }

    [Fact]
    public async Task ReportsDiagnosticForEveryClass()
    {
        string source =
            """
            class First
            {
            }

            class Second
            {
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson01SyntaxNodesAndKind.Exercise01.MainClass analyzer = new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() == "Class 'First' was found");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() == "Class 'Second' was found");
    }
    [Theory]
    [InlineData(
       """
        public class TestClass
        {
            public void Start()
            {
            }
        }
        """,
       "Start")]
    [InlineData(
       """
        public class TestClass
        {
            private int Calculate()
            {
                return 10;
            }
        }
        """,
       "Calculate")]
    [InlineData(
       """
        public class TestClass
        {
            protected string GetName()
            {
                return "Andy";
            }
        }
        """,
       "GetName")]
    public async Task ReportsDiagnosticForMethod(
       string source,
       string expectedMethodName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson01SyntaxNodesAndKind
                .Exercise02
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("SYNTAX102", diagnostic.Id);
        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"Method '{expectedMethodName}' was found",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        SyntaxNode root = await tree.GetRootAsync();

        MethodDeclarationSyntax methodNode =
            root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            methodNode.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task ReportsDiagnosticForEveryMethod()
    {
        string source =
            """
            public class FirstClass
            {
                public void Start()
                {
                }

                private int Calculate()
                {
                    return 10;
                }
            }

            public class SecondClass
            {
                internal void Finish()
                {
                }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson01SyntaxNodesAndKind
                .Exercise02
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(3, diagnostics.Length);

        List<string> messages =
            diagnostics
                .Select(x => x.GetMessage())
                .ToList();

        Assert.Contains(
            "Method 'Start' was found",
            messages);

        Assert.Contains(
            "Method 'Calculate' was found",
            messages);

        Assert.Contains(
            "Method 'Finish' was found",
            messages);
    }

    [Theory]
    [InlineData(
        """
        public class TestClass
        {
            public string CustomerName { get; set; }
        }
        """,
        "CustomerName",
        "string")]
    [InlineData(
        """
        public class TestClass
        {
            private int OrderCount { get; set; }
        }
        """,
        "OrderCount",
        "int")]
    [InlineData(
        """
        using System.Collections.Generic;

        public class TestClass
        {
            public List<int> Scores { get; set; }
        }
        """,
        "Scores",
        "List<int>")]
    public async Task ReportsDiagnosticForProperty(
        string source,
        string expectedPropertyName,
        string expectedPropertyType)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson01SyntaxNodesAndKind
                .Exercise03
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal(
            "SYNTAX103",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);

        Assert.Equal(
            $"Property '{expectedPropertyName}' has type '{expectedPropertyType}'",
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        SyntaxNode root = await tree.GetRootAsync();

        PropertyDeclarationSyntax propertyNode =
            root.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Single();

        Assert.Same(
            tree,
            diagnostic.Location.SourceTree);

        Assert.Equal(
            propertyNode.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task ReportsDiagnosticForEveryProperty()
    {
        string source =
            """
            public class Customer
            {
                public string Name { get; set; }

                public int Age { get; set; }
            }

            public class Order
            {
                public decimal Total { get; set; }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation =
            CSharpCompilation.Create(
                "TestAssembly",
                [tree]);

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson01SyntaxNodesAndKind
                .Exercise03
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            3,
            diagnostics.Length);

        List<string> messages =
            diagnostics
                .Select(x => x.GetMessage())
                .ToList();

        Assert.Contains(
            "Property 'Name' has type 'string'",
            messages);

        Assert.Contains(
            "Property 'Age' has type 'int'",
            messages);

        Assert.Contains(
            "Property 'Total' has type 'decimal'",
            messages);
    }
    [Theory]
    [InlineData(
        """
        class Customer
        {
            void Load()
            {
            }

            void Save()
            {
            }
        }
        """,
        "Class 'Customer' directly declares 2 methods")]
    [InlineData(
        """
        class Empty
        {
        }
        """,
        "Class 'Empty' directly declares 0 methods")]
    public async Task ReportsDirectMethodCount(
        string source,
        string expectedMessage)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            [
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson01SyntaxNodesAndKind.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(diagnostics);

        Assert.Equal("SYNTAX104", diagnostic.Id);
        Assert.Equal(
            DiagnosticSeverity.Info,
            diagnostic.Severity);
        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Equal(
            tree,
            diagnostic.Location.SourceTree);

        ClassDeclarationSyntax classNode =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

        Assert.Equal(
            classNode.Span,
            diagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task CountsMethodsOnlyFromTheirDirectClass()
    {
        string source =
            """
            class Outer
            {
                void First()
                {
                }

                class Inner
                {
                    void Second()
                    {
                    }

                    void Third()
                    {
                    }
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
            .Lesson01SyntaxNodesAndKind.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Outer' directly declares 1 methods");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Inner' directly declares 2 methods");

        List<ClassDeclarationSyntax> classes =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();

        foreach (ClassDeclarationSyntax classNode in classes)
        {
            Diagnostic diagnostic =
                diagnostics.Single(x =>
                    x.GetMessage().Contains(
                        $"Class '{classNode.Identifier.Text}'"));

            Assert.NotEqual(
                Location.None,
                diagnostic.Location);

            Assert.Equal(
                tree,
                diagnostic.Location.SourceTree);

            Assert.Equal(
                classNode.Span,
                diagnostic.Location.SourceSpan);
        }
    }

    [Fact]
    public async Task ReportsEveryClassAcrossMultipleClasses()
    {
        string source =
            """
            class First
            {
                void A()
                {
                }
            }

            class Second
            {
                void B()
                {
                }

                void C()
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
            .Lesson01SyntaxNodesAndKind.Exercise04.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(2, diagnostics.Length);

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'First' directly declares 1 methods");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Second' directly declares 2 methods");
    }
    [Theory]
    [InlineData(
        """
        class Empty
        {
        }
        """,
        "Class 'Empty' directly declares 0 methods and 0 properties and is classified as 'Small'")]
    [InlineData(
        """
        class Customer
        {
            public string Name { get; set; }

            void Save()
            {
            }
        }
        """,
        "Class 'Customer' directly declares 1 methods and 1 properties and is classified as 'Small'")]
    [InlineData(
        """
        class Customer
        {
            public string Name { get; set; }
            public int Age { get; set; }

            void Load()
            {
            }

            void Save()
            {
            }
        }
        """,
        "Class 'Customer' directly declares 2 methods and 2 properties and is classified as 'Standard'")]
    public async Task Exercise05ReportsExpectedClassSummary(
        string source,
        string expectedMessage)
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
            .Lesson01SyntaxNodesAndKind.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        Assert.Equal(
            "CLIENT401",
            diagnostic.Id);

        Assert.Equal(
            DiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.NotEqual(
            Location.None,
            diagnostic.Location);

        Assert.Equal(
            tree,
            diagnostic.Location.SourceTree);

        ClassDeclarationSyntax classNode =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

        Assert.Equal(
            classNode.Span,
            diagnostic.Location.SourceSpan);
    }

    [Theory]
    [InlineData(0, 0, "Small")]
    [InlineData(2, 0, "Small")]
    [InlineData(1, 2, "Standard")]
    [InlineData(3, 2, "Standard")]
    [InlineData(4, 2, "Large")]
    public async Task Exercise05UsesCorrectClassificationBoundaries(
        int methodCount,
        int propertyCount,
        string expectedClassification)
    {
        List<string> members = [];

        for (int x = 1; x <= methodCount; x++)
        {
            members.Add(
                $$"""
                void Method{{x}}()
                {
                }
                """);
        }

        for (int x = 1; x <= propertyCount; x++)
        {
            members.Add(
                $"public int Property{x} {{ get; set; }}");
        }

        string source =
            $$"""
            class Customer
            {
                {{string.Join(
                    Environment.NewLine,
                    members)}}
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
            .Lesson01SyntaxNodesAndKind.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic =
            Assert.Single(diagnostics);

        string expectedMessage =
            $"Class 'Customer' directly declares {methodCount} methods " +
            $"and {propertyCount} properties and is classified as " +
            $"'{expectedClassification}'";

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());
    }

    [Fact]
    public async Task Exercise05NestedClassesCountOnlyTheirOwnDirectMembers()
    {
        string source =
            """
            class Outer
            {
                public int Number { get; set; }

                void First()
                {
                }

                class Inner
                {
                    public string Name { get; set; }
                    public int Age { get; set; }

                    void Second()
                    {
                    }

                    void Third()
                    {
                    }
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
            .Lesson01SyntaxNodesAndKind.Exercise05.MainClass analyzer =
            new();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers([analyzer]);

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers
                .GetAnalyzerDiagnosticsAsync();

        Assert.Equal(
            2,
            diagnostics.Length);

        Diagnostic outerDiagnostic =
            diagnostics.Single(x =>
                x.GetMessage().StartsWith(
                    "Class 'Outer'"));

        Diagnostic innerDiagnostic =
            diagnostics.Single(x =>
                x.GetMessage().StartsWith(
                    "Class 'Inner'"));

        Assert.Equal(
            "Class 'Outer' directly declares 1 methods and 1 properties and is classified as 'Small'",
            outerDiagnostic.GetMessage());

        Assert.Equal(
            "Class 'Inner' directly declares 2 methods and 2 properties and is classified as 'Standard'",
            innerDiagnostic.GetMessage());

        List<ClassDeclarationSyntax> classes =
            tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();

        ClassDeclarationSyntax outerNode =
            classes.Single(x =>
                x.Identifier.Text == "Outer");

        ClassDeclarationSyntax innerNode =
            classes.Single(x =>
                x.Identifier.Text == "Inner");

        Assert.NotEqual(
            Location.None,
            outerDiagnostic.Location);

        Assert.Equal(
            tree,
            outerDiagnostic.Location.SourceTree);

        Assert.Equal(
            outerNode.Span,
            outerDiagnostic.Location.SourceSpan);

        Assert.NotEqual(
            Location.None,
            innerDiagnostic.Location);

        Assert.Equal(
            tree,
            innerDiagnostic.Location.SourceTree);

        Assert.Equal(
            innerNode.Span,
            innerDiagnostic.Location.SourceSpan);
    }

    [Fact]
    public async Task Exercise05ReportsEveryClassAcrossMultipleClasses()
    {
        string source =
            """
            class First
            {
                public int Number { get; set; }

                void A()
                {
                }
            }

            class Second
            {
                public int One { get; set; }
                public int Two { get; set; }
                public int Three { get; set; }

                void B()
                {
                }

                void C()
                {
                }

                void D()
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
            .Lesson01SyntaxNodesAndKind.Exercise05.MainClass analyzer =
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
                "Class 'First' directly declares 1 methods and 1 properties and is classified as 'Small'");

        Assert.Contains(
            diagnostics,
            x => x.GetMessage() ==
                "Class 'Second' directly declares 3 methods and 3 properties and is classified as 'Large'");
    }
}