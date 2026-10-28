using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations;

[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise01_AllMethodLikeKinds_ShouldReportCorrectInformation()
    {
        const string source = """
            public class Sample
            {
                static Sample() { }

                public Sample(int value) { }

                ~Sample() { }

                public void Run() { }

                public static Sample operator +(Sample left, Sample right)
                    => left;

                public static Sample operator -(Sample value)
                    => value;

                public static implicit operator int(Sample value)
                    => 0;

                public static explicit operator Sample(int value)
                    => new Sample(value);
            }
            """;

        await Exercise01_VerifyAsync(
            source,
            [
                (
                    "Constructor 'Sample' declares 0 parameter(s)",
                    "Sample()",
                    0,
                    "Sample"
                ),
                (
                    "Constructor 'Sample' declares 1 parameter(s)",
                    "Sample(int value)",
                    0,
                    "Sample"
                ),
                (
                    "Destructor 'Sample' declares 0 parameter(s)",
                    "~Sample()",
                    1,
                    "Sample"
                ),
                (
                    "Method 'Run' declares 0 parameter(s)",
                    "Run()",
                    0,
                    "Run"
                ),
                (
                    "Operator '+' declares 2 parameter(s)",
                    "+(Sample left",
                    0,
                    "+"
                ),
                (
                    "Operator '-' declares 1 parameter(s)",
                    "-(Sample value",
                    0,
                    "-"
                ),
                (
                    "Conversion Operator 'implicit' declares 1 parameter(s)",
                    "implicit operator",
                    0,
                    "implicit"
                ),
                (
                    "Conversion Operator 'explicit' declares 1 parameter(s)",
                    "explicit operator",
                    0,
                    "explicit"
                )
            ]);
    }

    [Fact]
    public async Task Exercise01_MethodVariations_ShouldCountDeclaredParameters()
    {
        const string source = """
            public interface IService
            {
                void Execute(int value);
            }

            public abstract class Worker
            {
                protected abstract void Process(string text, int count);

                public int Calculate(int value = 10) => value;

                private static void Collect(params int[] values) { }

                public void Convert<TFirst, TSecond>(TFirst value) { }

                public class Nested
                {
                    internal void Finish(ref int value, out int result)
                    {
                        result = value;
                    }
                }
            }
            """;

        await Exercise01_VerifyAsync(
            source,
            [
                (
                    "Method 'Execute' declares 1 parameter(s)",
                    "Execute(int value)",
                    0,
                    "Execute"
                ),
                (
                    "Method 'Process' declares 2 parameter(s)",
                    "Process(string text",
                    0,
                    "Process"
                ),
                (
                    "Method 'Calculate' declares 1 parameter(s)",
                    "Calculate(int value",
                    0,
                    "Calculate"
                ),
                (
                    "Method 'Collect' declares 1 parameter(s)",
                    "Collect(params",
                    0,
                    "Collect"
                ),
                (
                    "Method 'Convert' declares 1 parameter(s)",
                    "Convert<TFirst",
                    0,
                    "Convert"
                ),
                (
                    "Method 'Finish' declares 2 parameter(s)",
                    "Finish(ref",
                    0,
                    "Finish"
                )
            ]);
    }

    [Fact]
    public async Task Exercise01_UnrelatedSyntax_ShouldNotProduceExtraDiagnostics()
    {
        const string source = """
            public delegate void Callback(int value);

            public class Container
            {
                private int stored;

                public int Value
                {
                    get { return stored; }
                    set { stored = value; }
                }

                public int this[int index]
                {
                    get { return stored; }
                    set { stored = value; }
                }

                public event Callback Changed
                {
                    add { }
                    remove { }
                }

                public void Run()
                {
                    void Local(int value) { }

                    Callback lambda = value => { };
                    Callback anonymous = delegate(int value) { };

                    Local(1);
                    lambda(2);
                    anonymous(3);

                    var other = new Container();
                }
            }
            """;

        await Exercise01_VerifyAsync(
            source,
            [
                (
                    "Method 'Run' declares 0 parameter(s)",
                    "Run()",
                    0,
                    "Run"
                )
            ]);
    }

    [Fact]
    public async Task Exercise01_PrimaryConstructorsAndPartialMethods_ShouldFollowScope()
    {
        const string source = """
            public class Primary(int value)
            {
            }

            public struct Coordinates(int x, int y)
            {
            }

            public partial class Worker
            {
                partial void Save();
            }

            public partial class Worker
            {
                partial void Save() { }
            }
            """;

        await Exercise01_VerifyAsync(
            source,
            [
                (
                    "Method 'Save' declares 0 parameter(s)",
                    "Save();",
                    0,
                    "Save"
                ),
                (
                    "Method 'Save' declares 0 parameter(s)",
                    "Save() {",
                    0,
                    "Save"
                )
            ]);
    }

    [Fact]
    public async Task Exercise01_NoMethodLikeDeclarations_ShouldProduceNoDiagnostics()
    {
        const string source = """
            public delegate int Converter(string value);

            public interface IValues
            {
                int Count { get; }
            }

            public class Data
            {
                private int value;

                public int Value
                {
                    get => value;
                    set => this.value = value;
                }
            }
            """;

        await Exercise01_VerifyAsync(source, []);
    }

    private static async Task Exercise01_VerifyAsync(
        string source,
        (
            string Message,
            string Anchor,
            int TokenOffset,
            string TokenText
        )[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp12));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            [
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations
                .Exercise01
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == expected.Length,
            $"Expected {expected.Length} diagnostics, " +
            $"but found {diagnostics.Length}. Messages: " +
            string.Join(
                " | ",
                diagnostics.Select(x => $"{x.Id}: {x.GetMessage()}")));

        // Concurrent analysis does not guarantee diagnostic order.
        Diagnostic[] orderedDiagnostics = diagnostics
            .OrderBy(x => x.Location.SourceSpan.Start)
            .ToArray();

        for (int index = 0; index < expected.Length; index++)
        {
            var item = expected[index];
            Diagnostic diagnostic = orderedDiagnostics[index];

            int anchorStart = source.IndexOf(
                item.Anchor,
                StringComparison.Ordinal);

            Assert.True(
                anchorStart >= 0,
                $"Test anchor was not found: {item.Anchor}");

            TextSpan expectedSpan = new(
                anchorStart + item.TokenOffset,
                item.TokenText.Length);

            Assert.Equal("RMP030501", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(item.Message, diagnostic.GetMessage());

            Assert.True(
                diagnostic.Location.IsInSource,
                $"Expected a source location for: {item.Message}");

            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expectedSpan, diagnostic.Location.SourceSpan);

            Assert.Equal(
                item.TokenText,
                tree.GetText().ToString(diagnostic.Location.SourceSpan));
        }
    }

    [Fact]
    public async Task Exercise02_MethodLikeDeclarations_ShouldReportParameterNamesInOrder()
    {
        const string source = """
        public class Sample
        {
            public Sample(int value, string label) { }

            public void Save(string name, int score) { }

            public static Sample operator +(Sample left, Sample right)
                => left;

            public static Sample operator -(Sample operand)
                => operand;

            public static implicit operator int(Sample instance)
                => 0;

            public static explicit operator Sample(int number)
                => new Sample(number, "");
        }
        """;

        await Exercise02_VerifyAsync(
            source,
            [
                (
                "Constructor 'Sample' declares 2 parameter(s): value, label",
                "Sample(int value",
                "Sample"
            ),
            (
                "Method 'Save' declares 2 parameter(s): name, score",
                "Save(string name",
                "Save"
            ),
            (
                "Operator '+' declares 2 parameter(s): left, right",
                "+(Sample left",
                "+"
            ),
            (
                "Operator '-' declares 1 parameter(s): operand",
                "-(Sample operand",
                "-"
            ),
            (
                "Conversion Operator 'implicit' declares 1 parameter(s): instance",
                "implicit operator",
                "implicit"
            ),
            (
                "Conversion Operator 'explicit' declares 1 parameter(s): number",
                "explicit operator",
                "explicit"
            )
            ]);
    }

    [Fact]
    public async Task Exercise02_ParameterVariations_ShouldIncludeOnlyIdentifierValues()
    {
        const string source = """
        public class Worker
        {
            public void Optional(int count = 10, string label = "default") { }

            private static void Collect(string heading, params int[] values) { }

            protected void Update(ref int current, out int result, in int limit)
            {
                result = current;
            }

            public void Escape(int @class, string @namespace) { }

            public void Convert<TFirst, TSecond>(TFirst first, TSecond second) { }

            public void Mark([System.Obsolete] int value) { }
        }
        """;

        await Exercise02_VerifyAsync(
            source,
            [
                (
                "Method 'Optional' declares 2 parameter(s): count, label",
                "Optional(int count",
                "Optional"
            ),
            (
                "Method 'Collect' declares 2 parameter(s): heading, values",
                "Collect(string heading",
                "Collect"
            ),
            (
                "Method 'Update' declares 3 parameter(s): current, result, limit",
                "Update(ref int",
                "Update"
            ),
            (
                "Method 'Escape' declares 2 parameter(s): class, namespace",
                "Escape(int @class",
                "Escape"
            ),
            (
                "Method 'Convert' declares 2 parameter(s): first, second",
                "Convert<TFirst",
                "Convert"
            ),
            (
                "Method 'Mark' declares 1 parameter(s): value",
                "Mark([System.Obsolete]",
                "Mark"
            )
            ]);
    }

    [Fact]
    public async Task Exercise02_BodyStylesAndNestedTypes_ShouldReportIndependently()
    {
        const string source = """
        public interface IService
        {
            void Execute(int request);
        }

        public abstract partial class Worker
        {
            protected abstract void Process(string text);

            internal int Calculate(int value) => value;

            partial void Save(string path);

            private class Nested
            {
                private void Finish(bool success) { }
            }
        }

        public abstract partial class Worker
        {
            partial void Save(string path) { }
        }
        """;

        await Exercise02_VerifyAsync(
            source,
            [
                (
                "Method 'Execute' declares 1 parameter(s): request",
                "Execute(int request)",
                "Execute"
            ),
            (
                "Method 'Process' declares 1 parameter(s): text",
                "Process(string text)",
                "Process"
            ),
            (
                "Method 'Calculate' declares 1 parameter(s): value",
                "Calculate(int value)",
                "Calculate"
            ),
            (
                "Method 'Save' declares 1 parameter(s): path",
                "Save(string path);",
                "Save"
            ),
            (
                "Method 'Finish' declares 1 parameter(s): success",
                "Finish(bool success)",
                "Finish"
            ),
            (
                "Method 'Save' declares 1 parameter(s): path",
                "Save(string path) {",
                "Save"
            )
            ]);
    }

    [Fact]
    public async Task Exercise02_ZeroParametersAndUnrelatedSyntax_ShouldNotReport()
    {
        const string source = """
        public delegate void Callback(int value);

        public class Primary(int value)
        {
        }

        public class Container
        {
            static Container() { }

            public Container() { }

            ~Container() { }

            public int Value { get; set; }

            public int this[int index]
            {
                get => index;
                set { }
            }

            public event Callback Changed
            {
                add { }
                remove { }
            }

            public void Run()
            {
                void Local(int value) { }

                Callback lambda = value => { };
                Callback anonymous = delegate(int value) { };

                Local(1);
                lambda(2);
                anonymous(3);

                var other = new Container();
                var primary = new Primary(4);
            }

            public void Generic<T>() { }
        }
        """;

        await Exercise02_VerifyAsync(source, []);
    }

    private static async Task Exercise02_VerifyAsync(
        string source,
        (
            string Message,
            string Anchor,
            string TokenText
        )[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp12));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations
                .Exercise02
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == expected.Length,
            $"Expected {expected.Length} diagnostics, " +
            $"but found {diagnostics.Length}. Messages: " +
            string.Join(
                " | ",
                diagnostics.Select(x => $"{x.Id}: {x.GetMessage()}")));

        Diagnostic[] orderedDiagnostics = diagnostics
            .OrderBy(x => x.Location.SourceSpan.Start)
            .ToArray();

        for (int index = 0; index < expected.Length; index++)
        {
            var item = expected[index];
            Diagnostic diagnostic = orderedDiagnostics[index];

            int anchorStart = source.IndexOf(
                item.Anchor,
                StringComparison.Ordinal);

            Assert.True(
                anchorStart >= 0,
                $"Test anchor was not found: {item.Anchor}");

            TextSpan expectedSpan = new(
                anchorStart,
                item.TokenText.Length);

            Assert.Equal("RMP030502", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(item.Message, diagnostic.GetMessage());

            Assert.True(
                diagnostic.Location.IsInSource,
                $"Expected a source location for: {item.Message}");

            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expectedSpan, diagnostic.Location.SourceSpan);

            Assert.Equal(
                item.TokenText,
                tree.GetText().ToString(diagnostic.Location.SourceSpan));
        }

    }

    [Fact]
    public async Task Exercise03_GenericMethods_ShouldReportSeparateParameterCounts()
    {
        const string source = """
        public class Worker
        {
            public void Reset<T>() { }

            public TOutput Convert<TInput, TOutput>(TInput value)
                => default;

            private void Collect<T>(int count = 10, params T[] values) { }

            public void Escape<@class, @namespace>(int value) { }

            public void @return<T>(T value) { }
        }
        """;

        await Exercise03_VerifyAsync(
            source,
            [
                (
                "Method 'Reset' declares 1 type parameter(s): T; ordinary parameter count: 0",
                "Reset<T>()",
                "Reset"
            ),
            (
                "Method 'Convert' declares 2 type parameter(s): TInput, TOutput; ordinary parameter count: 1",
                "Convert<TInput",
                "Convert"
            ),
            (
                "Method 'Collect' declares 1 type parameter(s): T; ordinary parameter count: 2",
                "Collect<T>",
                "Collect"
            ),
            (
                "Method 'Escape' declares 2 type parameter(s): class, namespace; ordinary parameter count: 1",
                "Escape<@class",
                "Escape"
            ),
            (
                "Method 'return' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "@return<T>",
                "@return"
            )
            ]);
    }

    [Fact]
    public async Task Exercise03_GenericContainingTypes_ShouldNotContributeTypeParameters()
    {
        const string source = """
        public class Container<TOuter>
        {
            public void Save(TOuter value) { }

            public void Map<TItem>(TOuter outer, TItem item) { }

            public class Nested<TNested>
            {
                private void Finish<TResult>(
                    TOuter outer,
                    TNested nested,
                    TResult result)
                {
                }
            }
        }
        """;

        await Exercise03_VerifyAsync(
            source,
            [
                (
                "Method 'Map' declares 1 type parameter(s): TItem; ordinary parameter count: 2",
                "Map<TItem>",
                "Map"
            ),
            (
                "Method 'Finish' declares 1 type parameter(s): TResult; ordinary parameter count: 3",
                "Finish<TResult>",
                "Finish"
            )
            ]);
    }

    [Fact]
    public async Task Exercise03_BodyStylesAndContainingTypes_ShouldReportGenericMethods()
    {
        const string source = """
        public interface IService
        {
            void Execute<TRequest>(TRequest request);
        }

        public abstract class Worker
        {
            protected abstract void Process<T>(T value)
                where T : class, new();

            internal T Calculate<T>(T value) => value;
        }

        public struct State
        {
            public void Update<T>(T value) { }
        }

        public record Customer
        {
            public void Save<T>(T value) { }
        }

        public record struct Position
        {
            public void Move<T>(T value) { }
        }

        public partial class Storage
        {
            partial void Write<T>(T value);
        }

        public partial class Storage
        {
            partial void Write<T>(T value) { }
        }
        """;

        await Exercise03_VerifyAsync(
            source,
            [
                (
                "Method 'Execute' declares 1 type parameter(s): TRequest; ordinary parameter count: 1",
                "Execute<TRequest>",
                "Execute"
            ),
            (
                "Method 'Process' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Process<T>",
                "Process"
            ),
            (
                "Method 'Calculate' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Calculate<T>",
                "Calculate"
            ),
            (
                "Method 'Update' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Update<T>",
                "Update"
            ),
            (
                "Method 'Save' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Save<T>",
                "Save"
            ),
            (
                "Method 'Move' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Move<T>",
                "Move"
            ),
            (
                "Method 'Write' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Write<T>(T value);",
                "Write"
            ),
            (
                "Method 'Write' declares 1 type parameter(s): T; ordinary parameter count: 1",
                "Write<T>(T value) {",
                "Write"
            )
            ]);
    }

    [Fact]
    public async Task Exercise03_NongenericMethodsAndUnrelatedSyntax_ShouldNotReport()
    {
        const string source = """
        public delegate TResult Converter<TInput, TResult>(TInput value);

        public interface IService<T>
        {
            void Execute(T value);
        }

        public class Container<TOuter>
        {
            public Container() { }

            ~Container() { }

            public TOuter Value { get; set; }

            public void Save(TOuter value) { }

            public void Run()
            {
                T Local<T>(T value) => value;

                int result = Local<int>(1);
                var other = new Container<int>();
            }

            public class Nested<TNested>
            {
            }
        }
        """;

        await Exercise03_VerifyAsync(source, []);
    }

    private static async Task Exercise03_VerifyAsync(
        string source,
        (
            string Message,
            string Anchor,
            string TokenText
        )[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp12));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations
                .Exercise03
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == expected.Length,
            $"Expected {expected.Length} diagnostics, " +
            $"but found {diagnostics.Length}. Messages: " +
            string.Join(
                " | ",
                diagnostics.Select(x => $"{x.Id}: {x.GetMessage()}")));

        Diagnostic[] orderedDiagnostics = diagnostics
            .OrderBy(x => x.Location.SourceSpan.Start)
            .ToArray();

        for (int index = 0; index < expected.Length; index++)
        {
            var item = expected[index];
            Diagnostic diagnostic = orderedDiagnostics[index];

            int anchorStart = source.IndexOf(
                item.Anchor,
                StringComparison.Ordinal);

            Assert.True(
                anchorStart >= 0,
                $"Test anchor was not found: {item.Anchor}");

            TextSpan expectedSpan = new(
                anchorStart,
                item.TokenText.Length);

            Assert.Equal("RMP030503", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(item.Message, diagnostic.GetMessage());

            Assert.True(
                diagnostic.Location.IsInSource,
                $"Expected a source location for: {item.Message}");

            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expectedSpan, diagnostic.Location.SourceSpan);

            Assert.Equal(
                item.TokenText,
                tree.GetText().ToString(diagnostic.Location.SourceSpan));
        }
    }
    [Fact]
    public async Task Exercise04_AllMethodLikeKinds_ShouldIdentifyContainingClass()
    {
        const string source = """
        public class Sample
        {
            static Sample() { }

            public Sample(int value) { }

            ~Sample() { }

            public void Run() { }

            public static Sample operator +(Sample left, Sample right)
                => left;

            public static implicit operator int(Sample value)
                => 0;

            public static explicit operator Sample(int value)
                => new Sample(value);
        }
        """;

        await Exercise04_VerifyAsync(
            source,
            [
                (
                "Constructor 'Sample' in Class 'Sample' declares 0 parameter(s)",
                "Sample()",
                0,
                "Sample"
            ),
            (
                "Constructor 'Sample' in Class 'Sample' declares 1 parameter(s)",
                "Sample(int value)",
                0,
                "Sample"
            ),
            (
                "Destructor 'Sample' in Class 'Sample' declares 0 parameter(s)",
                "~Sample()",
                1,
                "Sample"
            ),
            (
                "Method 'Run' in Class 'Sample' declares 0 parameter(s)",
                "Run()",
                0,
                "Run"
            ),
            (
                "Operator '+' in Class 'Sample' declares 2 parameter(s)",
                "+(Sample left",
                0,
                "+"
            ),
            (
                "Conversion Operator 'implicit' in Class 'Sample' declares 1 parameter(s)",
                "implicit operator",
                0,
                "implicit"
            ),
            (
                "Conversion Operator 'explicit' in Class 'Sample' declares 1 parameter(s)",
                "explicit operator",
                0,
                "explicit"
            )
            ]);
    }

    [Fact]
    public async Task Exercise04_ContainingTypeKinds_ShouldUseRequiredNames()
    {
        const string source = """
        public struct State
        {
            public State(int value) { }

            private void Reset() { }
        }

        public interface IService
        {
            void Execute(int request);
        }

        public record Player
        {
            public Player(int score) { }

            public void Play() { }
        }

        public record class Customer
        {
            public void Save(string name) { }
        }

        public record struct Position
        {
            public Position(int x, int y) { }

            public void Move(int distance) { }
        }
        """;

        await Exercise04_VerifyAsync(
            source,
            [
                (
                "Constructor 'State' in Struct 'State' declares 1 parameter(s)",
                "State(int value)",
                0,
                "State"
            ),
            (
                "Method 'Reset' in Struct 'State' declares 0 parameter(s)",
                "Reset()",
                0,
                "Reset"
            ),
            (
                "Method 'Execute' in Interface 'IService' declares 1 parameter(s)",
                "Execute(int request)",
                0,
                "Execute"
            ),
            (
                "Constructor 'Player' in Record Class 'Player' declares 1 parameter(s)",
                "Player(int score)",
                0,
                "Player"
            ),
            (
                "Method 'Play' in Record Class 'Player' declares 0 parameter(s)",
                "Play()",
                0,
                "Play"
            ),
            (
                "Method 'Save' in Record Class 'Customer' declares 1 parameter(s)",
                "Save(string name)",
                0,
                "Save"
            ),
            (
                "Constructor 'Position' in Record Struct 'Position' declares 2 parameter(s)",
                "Position(int x",
                0,
                "Position"
            ),
            (
                "Method 'Move' in Record Struct 'Position' declares 1 parameter(s)",
                "Move(int distance)",
                0,
                "Move"
            )
            ]);
    }

    [Fact]
    public async Task Exercise04_NestedGenericTypes_ShouldUseNearestContainingType()
    {
        const string source = """
        namespace TestNamespace;

        public class Outer<TOuter>
        {
            public void Start(TOuter value) { }

            public struct Inner<TInner>
            {
                public void Update<TMethod>(TOuter outer, TInner inner) { }

                public class Deep
                {
                    private void Finish() { }
                }
            }

            public interface INested
            {
                void Execute();
            }
        }
        """;

        await Exercise04_VerifyAsync(
            source,
            [
                (
                "Method 'Start' in Class 'Outer' declares 1 parameter(s)",
                "Start(TOuter",
                0,
                "Start"
            ),
            (
                "Method 'Update' in Struct 'Inner' declares 2 parameter(s)",
                "Update<TMethod>",
                0,
                "Update"
            ),
            (
                "Method 'Finish' in Class 'Deep' declares 0 parameter(s)",
                "Finish()",
                0,
                "Finish"
            ),
            (
                "Method 'Execute' in Interface 'INested' declares 0 parameter(s)",
                "Execute()",
                0,
                "Execute"
            )
            ]);
    }

    [Fact]
    public async Task Exercise04_EscapedIdentifiers_ShouldUseValueTextAndOriginalLocations()
    {
        const string source = """
        public class @class
        {
            public @class(int value) { }

            ~@class() { }

            public void @return(int value) { }
        }
        """;

        await Exercise04_VerifyAsync(
            source,
            [
                (
                "Constructor 'class' in Class 'class' declares 1 parameter(s)",
                "@class(int value)",
                0,
                "@class"
            ),
            (
                "Destructor 'class' in Class 'class' declares 0 parameter(s)",
                "~@class()",
                1,
                "@class"
            ),
            (
                "Method 'return' in Class 'class' declares 1 parameter(s)",
                "@return(int value)",
                0,
                "@return"
            )
            ]);
    }

    [Fact]
    public async Task Exercise04_BodyStylesAndPartialMethods_ShouldReportEachDeclaration()
    {
        const string source = """
        public abstract partial class Worker
        {
            protected abstract void Process(int value);

            internal int Calculate(int value = 10) => value;

            private static void Collect(params int[] values) { }

            partial void Save(string path);
        }

        public abstract partial class Worker
        {
            partial void Save(string path) { }
        }
        """;

        await Exercise04_VerifyAsync(
            source,
            [
                (
                "Method 'Process' in Class 'Worker' declares 1 parameter(s)",
                "Process(int value)",
                0,
                "Process"
            ),
            (
                "Method 'Calculate' in Class 'Worker' declares 1 parameter(s)",
                "Calculate(int value",
                0,
                "Calculate"
            ),
            (
                "Method 'Collect' in Class 'Worker' declares 1 parameter(s)",
                "Collect(params",
                0,
                "Collect"
            ),
            (
                "Method 'Save' in Class 'Worker' declares 1 parameter(s)",
                "Save(string path);",
                0,
                "Save"
            ),
            (
                "Method 'Save' in Class 'Worker' declares 1 parameter(s)",
                "Save(string path) {",
                0,
                "Save"
            )
            ]);
    }

    [Fact]
    public async Task Exercise04_UnrelatedSyntax_ShouldNotProduceExtraDiagnostics()
    {
        const string source = """
        public delegate void Callback(int value);

        public class Primary(int initial)
        {
            public int Value { get; set; }

            public int this[int index]
            {
                get => index;
                set { }
            }

            public event Callback Changed
            {
                add { }
                remove { }
            }

            public void Run()
            {
                void Local(int value) { }

                Callback lambda = value => { };
                Callback anonymous = delegate(int value) { };

                Local(1);
                lambda(2);
                anonymous(3);

                var other = new Primary(4);
            }
        }

        public record Player(int Score);
        """;

        await Exercise04_VerifyAsync(
            source,
            [
                (
                "Method 'Run' in Class 'Primary' declares 0 parameter(s)",
                "Run()",
                0,
                "Run"
            )
            ]);
    }

    [Fact]
    public async Task Exercise04_MissingContainingType_ShouldSkipDeclaration()
    {
        // Intentionally invalid C#: a method directly inside a namespace.
        const string source = """
        namespace TestNamespace
        {
            void Orphan(int value) { }
        }
        """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);

        // Verify that the parser produced the syntax shape this test needs.
        Assert.Single(
            tree.GetRoot()
                .DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>());

        await Exercise04_VerifyAsync(source, []);
    }

    private static async Task Exercise04_VerifyAsync(
        string source,
        (
            string Message,
            string Anchor,
            int TokenOffset,
            string TokenText
        )[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp12));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations
                .Exercise04
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == expected.Length,
            $"Expected {expected.Length} diagnostics, " +
            $"but found {diagnostics.Length}. Messages: " +
            string.Join(
                " | ",
                diagnostics.Select(x => $"{x.Id}: {x.GetMessage()}")));

        Diagnostic[] orderedDiagnostics = diagnostics
            .OrderBy(x => x.Location.SourceSpan.Start)
            .ToArray();

        for (int index = 0; index < expected.Length; index++)
        {
            var item = expected[index];
            Diagnostic diagnostic = orderedDiagnostics[index];

            int anchorStart = source.IndexOf(
                item.Anchor,
                StringComparison.Ordinal);

            Assert.True(
                anchorStart >= 0,
                $"Test anchor was not found: {item.Anchor}");

            TextSpan expectedSpan = new(
                anchorStart + item.TokenOffset,
                item.TokenText.Length);

            Assert.Equal("RMP030504", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(item.Message, diagnostic.GetMessage());

            Assert.True(
                diagnostic.Location.IsInSource,
                $"Expected a source location for: {item.Message}");

            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expectedSpan, diagnostic.Location.SourceSpan);

            Assert.Equal(
                item.TokenText,
                tree.GetText().ToString(diagnostic.Location.SourceSpan));
        }
    }
    [Fact]
    public async Task Exercise05_Thresholds_ShouldReportOnlyQualifyingGroups()
    {
        const string source = """
        public class Storage
        {
            public void Small(int value) { }
            public void Small(string value) { }

            public void Many() { }
            public void Many(int value) { }
            public void Many(int value, string label) { }

            public void Wide(int value) { }
            public void Wide(int a, int b, int c, int d) { }

            public void Single(int a, int b, int c, int d, int e) { }
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Many' with generic arity 0 in Class 'Storage' has 3 overload declaration(s); maximum parameter count: 2",
                "Many()",
                "Many"
            ),
            (
                "Public method group 'Wide' with generic arity 0 in Class 'Storage' has 2 overload declaration(s); maximum parameter count: 4",
                "Wide(int value)",
                "Wide"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_Grouping_ShouldUseNameAndGenericArity()
    {
        const string source = """
        public class Mapper
        {
            public void Map() { }
            public void Map(int value) { }

            public void Map<T>(T value) { }
            public void Map<TItem>(TItem value, int a, int b, int c) { }

            public void Map<TFirst, TSecond>() { }
            public void Map<TLeft, TRight>(TLeft value) { }
            public void Map<TKey, TValue>(TKey key, TValue value) { }

            public void Other(int value) { }
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Map' with generic arity 1 in Class 'Mapper' has 2 overload declaration(s); maximum parameter count: 4",
                "Map<T>(T value)",
                "Map"
            ),
            (
                "Public method group 'Map' with generic arity 2 in Class 'Mapper' has 3 overload declaration(s); maximum parameter count: 2",
                "Map<TFirst, TSecond>()",
                "Map"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_ExplicitPublicModifier_ShouldControlEligibility()
    {
        const string source = """
        public class Storage
        {
            private void Save() { }

            public void Save(int value) { }

            protected void Save(string value) { }
            internal void Save(bool value) { }
            private void Save(int a, int b, int c, int d, int e) { }

            public void Save(int value, string label) { }
            public static void Save(int value, string label, bool overwrite) { }

            public void Small(int value) { }
            public void Small(string value) { }
            private void Small(int a, int b, int c, int d) { }
        }

        public interface IService
        {
            void Execute();

            public void Execute(int value);

            void Execute(string value);

            public void Execute(int a, int b, int c, int d);

            void ImplicitOnly();
            void ImplicitOnly(int value);
            void ImplicitOnly(string value);
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Save' with generic arity 0 in Class 'Storage' has 3 overload declaration(s); maximum parameter count: 3",
                "Save(int value)",
                "Save"
            ),
            (
                "Public method group 'Execute' with generic arity 0 in Interface 'IService' has 2 overload declaration(s); maximum parameter count: 4",
                "Execute(int value)",
                "Execute"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_ContainingDeclarations_ShouldKeepGroupsSeparate()
    {
        const string source = """
        public class Outer
        {
            public void Save() { }

            public class Inner
            {
                public void Save(int value) { }
                public void Save(string value) { }
            }
        }

        public class Sibling
        {
            public void Save(bool value) { }
        }

        public partial class Storage
        {
            public void Write() { }
            public void Write(int value) { }
        }

        public partial class Storage
        {
            public void Write(string value) { }
        }
        """;

        await Exercise05_VerifyAsync(source, []);
    }

    [Fact]
    public async Task Exercise05_NestedQualifyingGroups_ShouldReportIndependently()
    {
        const string source = """
        public class Outer<TOuter>
        {
            public void Save() { }
            public void Save(int value) { }
            public void Save(string value) { }

            public class Inner<TInner>
            {
                public void Save() { }
                public void Save(int value) { }
                public void Save(string value) { }
            }
        }
        """;

        // Longer anchors distinguish identical method declarations.
        const string outerAnchor = """
        Save() { }
            public void Save(int value) { }
            public void Save(string value) { }

            public class Inner
        """;

        const string innerAnchor = """
        Save() { }
                public void Save(int value) { }
                public void Save(string value) { }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Save' with generic arity 0 in Class 'Outer' has 3 overload declaration(s); maximum parameter count: 1",
                outerAnchor,
                "Save"
            ),
            (
                "Public method group 'Save' with generic arity 0 in Class 'Inner' has 3 overload declaration(s); maximum parameter count: 1",
                innerAnchor,
                "Save"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_ContainingTypeKinds_ShouldUseRequiredLabels()
    {
        const string source = """
        public class ClassContainer
        {
            public void Review() { }
            public void Review(int value) { }
            public void Review(string value) { }
        }

        public struct StructContainer
        {
            public void Update() { }
            public void Update(int value) { }
            public void Update(string value) { }
        }

        public interface IService
        {
            public void Execute();
            public void Execute(int value);
            public void Execute(string value);
        }

        public record RecordContainer
        {
            public void Save() { }
            public void Save(int value) { }
            public void Save(string value) { }
        }

        public record class ExplicitRecordContainer
        {
            public void Load() { }
            public void Load(int value) { }
            public void Load(string value) { }
        }

        public record struct Position
        {
            public void Move() { }
            public void Move(int value) { }
            public void Move(string value) { }
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Review' with generic arity 0 in Class 'ClassContainer' has 3 overload declaration(s); maximum parameter count: 1",
                "Review()",
                "Review"
            ),
            (
                "Public method group 'Update' with generic arity 0 in Struct 'StructContainer' has 3 overload declaration(s); maximum parameter count: 1",
                "Update()",
                "Update"
            ),
            (
                "Public method group 'Execute' with generic arity 0 in Interface 'IService' has 3 overload declaration(s); maximum parameter count: 1",
                "Execute();",
                "Execute"
            ),
            (
                "Public method group 'Save' with generic arity 0 in Record Class 'RecordContainer' has 3 overload declaration(s); maximum parameter count: 1",
                "Save()",
                "Save"
            ),
            (
                "Public method group 'Load' with generic arity 0 in Record Class 'ExplicitRecordContainer' has 3 overload declaration(s); maximum parameter count: 1",
                "Load()",
                "Load"
            ),
            (
                "Public method group 'Move' with generic arity 0 in Record Struct 'Position' has 3 overload declaration(s); maximum parameter count: 1",
                "Move()",
                "Move"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_EscapedIdentifiers_ShouldGroupByValueText()
    {
        const string source = """
        public class @class
        {
            public void @Save() { }
            public void Save(int value) { }
            public void Save(string value) { }

            public void @return(int value) { }
            public void @return(int a, int b, int c, int d) { }
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Save' with generic arity 0 in Class 'class' has 3 overload declaration(s); maximum parameter count: 1",
                "@Save()",
                "@Save"
            ),
            (
                "Public method group 'return' with generic arity 0 in Class 'class' has 2 overload declaration(s); maximum parameter count: 4",
                "@return(int value)",
                "@return"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_PartialMethods_ShouldCountSyntaxDeclarationsIndependently()
    {
        const string source = """
        public partial class Worker
        {
            public partial void Save(int a, int b, int c, int d);

            public partial void Save(int a, int b, int c, int d) { }
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Save' with generic arity 0 in Class 'Worker' has 2 overload declaration(s); maximum parameter count: 4",
                "Save(int a, int b, int c, int d);",
                "Save"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_ParameterAndBodyVariations_ShouldUseDeclaredCounts()
    {
        const string source = """
        public abstract class Worker
        {
            public abstract void Process();

            public int Process(int value) => value;

            public void Process(int count = 10, params int[] values) { }

            public void Collect(int value) { }

            public void Collect(int a, int b, int c, params int[] values) { }
        }
        """;

        await Exercise05_VerifyAsync(
            source,
            [
                (
                "Public method group 'Process' with generic arity 0 in Class 'Worker' has 3 overload declaration(s); maximum parameter count: 2",
                "Process();",
                "Process"
            ),
            (
                "Public method group 'Collect' with generic arity 0 in Class 'Worker' has 2 overload declaration(s); maximum parameter count: 4",
                "Collect(int value)",
                "Collect"
            )
            ]);
    }

    [Fact]
    public async Task Exercise05_UnrelatedDeclarations_ShouldNotFormMethodGroups()
    {
        const string source = """
        public class Sample
        {
            public Sample() { }
            public Sample(int value) { }
            public Sample(string value) { }

            ~Sample() { }

            public int Value { get; set; }

            public int this[int index]
            {
                get => index;
                set { }
            }

            public delegate void Callback(int value);

            public static Sample operator +(Sample left, Sample right)
                => left;

            public static Sample operator +(Sample left, int right)
                => left;

            public static Sample operator +(int left, Sample right)
                => right;

            public static implicit operator int(Sample value) => 0;
            public static implicit operator string(Sample value) => "";
            public static implicit operator bool(Sample value) => true;

            public void Run()
            {
                void Local() { }
                void LocalOne(int value) { }
                void LocalTwo(string value) { }

                Callback lambda = value => { };
                Callback anonymous = delegate(int value) { };

                Local();
                LocalOne(1);
                LocalTwo("");

                var other = new Sample();
            }
        }
        """;

        await Exercise05_VerifyAsync(source, []);
    }

    private static async Task Exercise05_VerifyAsync(
        string source,
        (
            string Message,
            string Anchor,
            string TokenText
        )[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.CSharp12));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            [
                MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location)
            ],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson05MethodLikeDeclarationsAndBaseMethodDeclarations
                .Exercise05
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == expected.Length,
            $"Expected {expected.Length} diagnostics, " +
            $"but found {diagnostics.Length}. Messages: " +
            string.Join(
                " | ",
                diagnostics.Select(x => $"{x.Id}: {x.GetMessage()}")));

        Diagnostic[] orderedDiagnostics = diagnostics
            .OrderBy(x => x.Location.SourceSpan.Start)
            .ToArray();

        for (int index = 0; index < expected.Length; index++)
        {
            var item = expected[index];
            Diagnostic diagnostic = orderedDiagnostics[index];

            // Normalize newlines so multiline anchors work with CRLF or LF.
            string normalizedSource = source.Replace("\r\n", "\n");
            string normalizedAnchor = item.Anchor.Replace("\r\n", "\n");

            int normalizedStart = normalizedSource.IndexOf(
                normalizedAnchor,
                StringComparison.Ordinal);

            Assert.True(
                normalizedStart >= 0,
                $"Test anchor was not found: {item.Anchor}");

            // Translate the normalized position back to the original source.
            int anchorStart = 0;
            int normalizedPosition = 0;

            while (normalizedPosition < normalizedStart)
            {
                if (source[anchorStart] == '\r'
                    && anchorStart + 1 < source.Length
                    && source[anchorStart + 1] == '\n')
                {
                    anchorStart += 2;
                }
                else
                {
                    anchorStart++;
                }

                normalizedPosition++;
            }

            TextSpan expectedSpan = new(
                anchorStart,
                item.TokenText.Length);

            Assert.Equal("RMP030505", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(item.Message, diagnostic.GetMessage());

            Assert.True(
                diagnostic.Location.IsInSource,
                $"Expected a source location for: {item.Message}");

            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expectedSpan, diagnostic.Location.SourceSpan);

            Assert.Equal(
                item.TokenText,
                tree.GetText().ToString(diagnostic.Location.SourceSpan));
        }
    }
}