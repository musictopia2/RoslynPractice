using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson06PropertyLikeAndFieldDeclarations;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise01_TwoFields_ReportBothIdentifiers()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                private int [|first|], [|second|];
            }
            """);
    }

    [Fact]
    public async Task Exercise01_InitializersAndMultipleDeclarations_ReportEveryQualifyingField()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                private int [|first|] = 1, [|second|] = 2, [|third|];
                private string [|left|] = "a,b", [|right|] = "c";
                private int single = 10;
                private int anotherSingle;
            }
            """);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("public")]
    [InlineData("protected internal")]
    [InlineData("private static")]
    [InlineData("private readonly")]
    [InlineData("private volatile")]
    [InlineData("public const")]
    public async Task Exercise01_Modifiers_DoNotExcludeFields(string modifiers)
    {
        await Exercise01_VerifyAsync(
            "class Sample { " + modifiers + " int [|first|] = 1, [|second|] = 2; }");
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    [InlineData("record")]
    [InlineData("record struct")]
    public async Task Exercise01_ContainingType_DoesNotExcludeFields(string typeKind)
    {
        await Exercise01_VerifyAsync(
            typeKind + " Sample { private int [|first|], [|second|]; }");
    }

    [Fact]
    public async Task Exercise01_NestedTypes_AnalyzeTheirFieldsIndependently()
    {
        await Exercise01_VerifyAsync("""
            class Outer
            {
                int [|outerFirst|], [|outerSecond|];
                class Inner
                {
                    int single;
                    int [|innerFirst|], [|innerSecond|];
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise01_EscapedIdentifiers_UseValueTextAndHighlightWholeToken()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                int [|@class|], [|@event|];
            }
            """);
    }

    [Fact]
    public async Task Exercise01_SingleFieldsLocalsPropertiesAndEvents_ReportNothing()
    {
        await Exercise01_VerifyAsync("""
            delegate void Callback();
            class Sample
            {
                int single;
                const int OnlyConstant = 1;
                int[] values = new int[] { 1, 2, 3 };
                public int Property { get; set; }
                public int ExpressionProperty => 42;
                public event Callback FirstEvent, SecondEvent;
                public event Callback CustomEvent
                {
                    add { }
                    remove { }
                }
                void Method()
                {
                    int first = 1, second = 2;
                    const int LocalFirst = 1, LocalSecond = 2;
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise01_CommentsAndLineBreaks_PreserveExactIdentifierLocations()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                int /* before */ [|first|] /* after */ = 1,
                    // next variable
                    [|second|] = 2;
            }
            """);
    }

    // [|...|] marks exactly one expected diagnostic span.
    // This helper derives expectations from those markers, not from analyzer syntax.
    private static async Task Exercise01_VerifyAsync(string markedSource)
    {
        var source = new StringBuilder();
        var expected = new System.Collections.Generic.List<(TextSpan Span, string Name)>();
        int previousEnd = 0;

        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]"))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string tokenText = match.Groups[1].Value;
            expected.Add((new TextSpan(source.Length, tokenText.Length), tokenText.TrimStart('@')));
            source.Append(tokenText);
            previousEnd = match.Index + match.Length;
        }

        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString());
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise01TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson06PropertyLikeAndFieldDeclarations.Exercise01.MainClass();

        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer output: unexpected diagnostics and analyzer crashes must fail.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));

        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("FIELD001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Field '{expected[i].Name}' is declared together with other fields",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("get; set;")]
    [InlineData("get;")]
    [InlineData("get; init;")]
    [InlineData("get; private set;")]
    public async Task Exercise02_AccessorCombinations_ReportInitializedProperty(string accessors)
    {
        await Exercise02_VerifyAsync(
            "class Sample { public int [|Count|] { " + accessors + " } = 10; }");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("default")]
    [InlineData("default(int)")]
    [InlineData("Calculate()")]
    [InlineData("1 + 2")]
    public async Task Exercise02_InitializerExpressions_ReportWithoutEvaluating(string expression)
    {
        await Exercise02_VerifyAsync(
            "class Sample { public int [|Count|] { get; set; } = " + expression +
            "; static int Calculate() => 42; }");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("internal")]
    [InlineData("protected")]
    [InlineData("protected internal")]
    [InlineData("public static")]
    [InlineData("public virtual")]
    public async Task Exercise02_Modifiers_DoNotExcludeProperties(string modifiers)
    {
        await Exercise02_VerifyAsync(
            "class Sample { " + modifiers + " int [|Count|] { get; set; } = 10; }");
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    [InlineData("record")]
    [InlineData("record struct")]
    public async Task Exercise02_ContainingTypes_DoNotExcludeProperties(string typeKind)
    {
        await Exercise02_VerifyAsync(typeKind + " Sample { public Sample() { } " +
            "public int [|Count|] { get; set; } = 10; }");
    }

    [Fact]
    public async Task Exercise02_MultiplePropertiesAndNestedTypes_ReportEachInitializer()
    {
        await Exercise02_VerifyAsync("""
            class Outer
            {
                public int [|Count|] { get; set; } = 10;
                public string [|Name|] { get; } = "Andy";
                public int Uninitialized { get; set; }
                public object [|Item|] { get; set; } = null;
                public int[] [|Values|] { get; } = new int[] { 1, 2 };
                class Inner
                {
                    public bool [|Enabled|] { get; init; } = false;
                    public int Uninitialized { get; set; }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_EscapedNameAndTrivia_UseValueTextAndExactIdentifierSpan()
    {
        await Exercise02_VerifyAsync("""
            class Sample
            {
                public int /* before */ [|@class|] /* after */
                {
                    get;
                    set;
                }
                = 10;
            }
            """);
    }

    [Fact]
    public async Task Exercise02_PropertiesWithoutInitializers_ReportNothing()
    {
        await Exercise02_VerifyAsync("""
            class Sample
            {
                private int backing;
                public int Auto { get; set; }
                public int ReadOnly { get; }
                public int InitOnly { get; init; }
                public int Expression => 10;
                public int BlockGetter { get { return 20; } }
                public int ExpressionGetter { get => 30; }
                public int Custom
                {
                    get { return backing; }
                    set { backing = value; }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_FieldsEventsIndexersAndLocals_ReportNothing()
    {
        await Exercise02_VerifyAsync("""
            delegate void Callback();
            class Sample
            {
                private int first = 1, second = 2;
                private string name = "Andy";
                public event Callback FirstEvent = null, SecondEvent = null;
                public event Callback CustomEvent
                {
                    add { }
                    remove { }
                }
                public int this[int index] => index;
                public int this[string key] { get { return 10; } }
                void Method()
                {
                    int first = 1, second = 2;
                }
            }
            """);
    }

    // [|...|] marks exactly one expected diagnostic span.
    // This helper derives expectations from those markers, not from analyzer syntax.
    private static async Task Exercise02_VerifyAsync(string markedSource)
    {
        var source = new StringBuilder();
        var expected = new System.Collections.Generic.List<(TextSpan Span, string Name)>();
        int previousEnd = 0;

        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]"))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string tokenText = match.Groups[1].Value;
            expected.Add((new TextSpan(source.Length, tokenText.Length), tokenText.TrimStart('@')));
            source.Append(tokenText);
            previousEnd = match.Index + match.Length;
        }

        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString());
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise02TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson06PropertyLikeAndFieldDeclarations.Exercise02.MainClass();

        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer output: unexpected diagnostics and analyzer crashes must fail.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));

        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("PROPINIT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Property '{expected[i].Name}' has an explicit initializer",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("int row, int column", 2)]
    [InlineData("int x, int y, int z", 3)]
    [InlineData("int a, int b, int c, int d", 4)]
    [InlineData("int row, int column = 0", 2)]
    [InlineData("Pair<int, string> pair, int index", 2)]
    public async Task Exercise03_MultipleParameters_ReportExactCount(string parameters, int count)
    {
        await Exercise03_VerifyAsync(
            "class Pair<TLeft, TRight> { } class Sample { public int [|this|][" +
            parameters + "] => 0; }", count);
    }

    [Theory]
    [InlineData("=> 0;")]
    [InlineData("{ get { return 0; } }")]
    [InlineData("{ get => 0; }")]
    [InlineData("{ get { return 0; } set { } }")]
    [InlineData("{ set { } }")]
    [InlineData("{ get => 0; init { } }")]
    public async Task Exercise03_AccessorAndBodyForms_ReportOnce(string body)
    {
        await Exercise03_VerifyAsync(
            "class Sample { public int [|this|][int row, int column] " + body + " }", 2);
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("internal")]
    [InlineData("protected")]
    [InlineData("protected internal")]
    [InlineData("public virtual")]
    public async Task Exercise03_Modifiers_DoNotExcludeIndexers(string modifiers)
    {
        await Exercise03_VerifyAsync(
            "class Sample { " + modifiers + " int [|this|][int row, int column] => 0; }", 2);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    [InlineData("record")]
    [InlineData("record struct")]
    public async Task Exercise03_ContainingTypes_DoNotExcludeIndexers(string typeKind)
    {
        await Exercise03_VerifyAsync(
            typeKind + " Sample { public int [|this|][int row, int column] => 0; }", 2);
    }

    [Fact]
    public async Task Exercise03_MultipleIndexersAndNestedTypes_ReportIndependently()
    {
        await Exercise03_VerifyAsync("""
            class Outer
            {
                public int [|this|][int row, int column] => 0;
                public string [|this|][string a, string b, string c] => "";
                public int this[int index] => 0;
                class Inner
                {
                    public object [|this|][int a, int b, int c, int d] => null;
                    public int this[string key] => 0;
                }
            }
            """, 2, 3, 4);
    }

    [Fact]
    public async Task Exercise03_InterfaceAndExplicitImplementation_ReportBothDeclarations()
    {
        await Exercise03_VerifyAsync("""
            interface IGrid
            {
                int [|this|][int row, int column] { get; }
            }
            class Grid : IGrid
            {
                int IGrid.[|this|][int row, int column] => 0;
            }
            """, 2, 2);
    }

    [Fact]
    public async Task Exercise03_CommentsAndLineBreaks_HighlightOnlyThisKeyword()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                public int /* before */ [|this|] /* after */
                [
                    int row,
                    int column
                ]
                => 0;
            }
            """, 2);
    }

    [Fact]
    public async Task Exercise03_SingleParameterIncludingGenericAndTupleTypes_ReportNothing()
    {
        await Exercise03_VerifyAsync("""
            class Pair<TLeft, TRight> { }
            class Sample
            {
                public int this[int index] => 0;
                public int this[Pair<int, string> pair] => 0;
                public int this[(int row, int column) position] => 0;
                public int this[params string[] keys] => 0;
            }
            """);
    }

    [Fact]
    public async Task Exercise03_FieldsPropertiesMethodsAndEvents_ReportNothing()
    {
        await Exercise03_VerifyAsync("""
            delegate void Callback(int row, int column);
            class Sample
            {
                private int first, second;
                public int Property { get; set; } = 10;
                public int ExpressionProperty => 20;
                public event Callback FirstEvent, SecondEvent;
                public int Calculate(int row, int column) => 0;
                public int Calculate(int x, int y, int z) => 0;
            }
            """);
    }

    // Each [|this|] marker gives an exact expected source span.
    // Parameter counts are supplied explicitly in source order.
    private static async Task Exercise03_VerifyAsync(string markedSource, params int[] counts)
    {
        var source = new StringBuilder();
        var spans = new System.Collections.Generic.List<TextSpan>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]"))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string tokenText = match.Groups[1].Value;
            Assert.Equal("this", tokenText);
            spans.Add(new TextSpan(source.Length, tokenText.Length));
            source.Append(tokenText);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(counts.Length, spans.Count);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString());
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise03TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson06PropertyLikeAndFieldDeclarations.Exercise03.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        Assert.True(spans.Count == diagnostics.Length,
            $"Expected {spans.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < spans.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("INDEXER001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal($"Indexer declares {counts[i]} parameters", diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(spans[i], diagnostic.Location.SourceSpan);
        }
    }

    [Fact]
    public async Task Exercise04_MixedMembers_ReportDirectCounts()
    {
        await Exercise04_VerifyAsync("""
            delegate void Callback();
            class [|Sample|]
            {
                private int first, second;
                private const int Limit = 10;
                public int Count { get; set; }
                public string Name => "Example";
                public int this[int index] => 0;
                public event Callback Changed;
                void Method() { int local = 0; }
            }
            """, (3, 2, 1));
    }

    [Theory]
    [InlineData("int first, second, third;", 3, 0, 0)]
    [InlineData("const int First = 1, Second = 2;", 2, 0, 0)]
    [InlineData("public int Value { get; set; }", 0, 1, 0)]
    [InlineData("public int Value { get; } = 10;", 0, 1, 0)]
    [InlineData("public int Value => 10;", 0, 1, 0)]
    [InlineData("public int this[int index] => 0;", 0, 0, 1)]
    [InlineData("public int this[int a, int b] { get { return 0; } set { } }", 0, 0, 1)]
    public async Task Exercise04_SingleMemberCategory_IsEnoughToReport(
        string members, int fields, int properties, int indexers)
    {
        await Exercise04_VerifyAsync(
            "class [|Sample|] { " + members + " }", (fields, properties, indexers));
    }

    [Fact]
    public async Task Exercise04_ModifiersTypesAndInitializers_DoNotChangeCounts()
    {
        await Exercise04_VerifyAsync("""
            class [|Sample|]
            {
                private readonly string first = "a", second = "b";
                public static object Item;
                internal volatile int counter;
                protected const int Limit = 10;
                public static int Count { get; set; } = 1;
                private string Name { get; } = "Example";
                public virtual object this[string key] => null;
                private bool this[int x, int y, int z] { get { return false; } }
            }
            """, (5, 2, 2));
    }

    [Fact]
    public async Task Exercise04_NestedClasses_KeepCountsSeparate()
    {
        await Exercise04_VerifyAsync("""
            class [|Outer|]
            {
                int first, second;
                class [|Inner|]
                {
                    public int Value { get; set; }
                    class [|Deep|]
                    {
                        public int this[int index] => 0;
                    }
                }
                struct NestedStruct { int ignored; }
                record NestedRecord { public int Ignored { get; set; } }
            }
            """, (2, 0, 0), (0, 1, 0), (0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_OuterWithOnlyNestedType_DoesNotReportForOuter()
    {
        await Exercise04_VerifyAsync("""
            class Outer
            {
                class [|Inner|] { int value; }
            }
            """, (1, 0, 0));
    }

    [Fact]
    public async Task Exercise04_PartialDeclarationsInSameFile_AreIndependent()
    {
        await Exercise04_VerifyAsync("""
            partial class [|Sample|] { int first, second; }
            partial class [|Sample|] { public int Value { get; set; } }
            partial class Sample { void Method() { } }
            """, (2, 0, 0), (0, 1, 0));
    }

    [Fact]
    public async Task Exercise04_PartialDeclarationsAcrossFiles_AreIndependent()
    {
        await Exercise04_VerifySourcesAsync(
            [
                "partial class [|Sample|] { int first, second; }",
                "partial class [|Sample|] { public int this[int i] => 0; }",
                "partial class Sample { void Method() { } }"
            ],
            (2, 0, 0), (0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_NonClassTypes_AreIgnoredButNestedClassesAreAnalyzed()
    {
        await Exercise04_VerifyAsync("""
            struct Container
            {
                int ignored;
                public int Value { get; set; }
                public int this[int i] => 0;
                public class [|InsideStruct|] { int first, second; }
            }
            record RecordContainer
            {
                int ignored;
                public int Value { get; set; }
                public class [|InsideRecord|] { public int Value => 0; }
            }
            interface IContainer
            {
                int Value { get; }
                int this[int i] { get; }
                public class [|InsideInterface|] { public int this[int i] => 0; }
            }
            record struct RecordStructContainer
            {
                int ignored;
                public class [|InsideRecordStruct|] { int value; }
            }
            """, (2, 0, 0), (0, 1, 0), (0, 0, 1), (1, 0, 0));
    }

    [Fact]
    public async Task Exercise04_EmptyClassesAndUnrelatedMembers_ReportNothing()
    {
        await Exercise04_VerifyAsync("""
            delegate void Callback();
            class Empty { }
            class OtherMembers
            {
                public OtherMembers() { int local = 0; }
                ~OtherMembers() { }
                void Method() { int first = 1, second = 2; }
                public event Callback First, Second;
                public event Callback Custom { add { } remove { } }
                class Nested { void Method() { } }
                struct NestedStruct { int field; }
                enum Choice { First, Second }
                delegate void NestedCallback();
            }
            """);
    }

    [Fact]
    public async Task Exercise04_EscapedClassNameAndTrivia_UseValueTextAndExactIdentifierSpan()
    {
        await Exercise04_VerifyAsync("""
            class /* before */ [|@class|] /* after */
            {
                int value;
            }
            """, (1, 0, 0));
    }

    private static Task Exercise04_VerifyAsync(
        string markedSource,
        params (int Fields, int Properties, int Indexers)[] counts)
    {
        return Exercise04_VerifySourcesAsync([markedSource], counts);
    }

    // Mark each expected class identifier with [|...|].
    // Counts are explicit, in file order and then source order within each file.
    private static async Task Exercise04_VerifySourcesAsync(
        string[] markedSources,
        params (int Fields, int Properties, int Indexers)[] counts)
    {
        var trees = new System.Collections.Generic.List<SyntaxTree>();
        var expected = new System.Collections.Generic.List<
            (SyntaxTree Tree, TextSpan Span, string Name)>();

        foreach (string markedSource in markedSources)
        {
            var source = new StringBuilder();
            var markers = new System.Collections.Generic.List<(TextSpan Span, string Name)>();
            int previousEnd = 0;
            foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]"))
            {
                source.Append(markedSource, previousEnd, match.Index - previousEnd);
                string tokenText = match.Groups[1].Value;
                markers.Add((new TextSpan(source.Length, tokenText.Length), tokenText.TrimStart('@')));
                source.Append(tokenText);
                previousEnd = match.Index + match.Length;
            }
            source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
                path: $"Exercise04Source{trees.Count}.cs");
            trees.Add(tree);
            foreach (var marker in markers)
            {
                expected.Add((tree, marker.Span, marker.Name));
            }
        }
        Assert.Equal(counts.Length, expected.Count);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise04TestAssembly",
            syntaxTrees: trees,
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson06PropertyLikeAndFieldDeclarations.Exercise04.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics
            .OrderBy(d => d.Location.SourceTree is null ? -1 : trees.IndexOf(d.Location.SourceTree))
            .ThenBy(d => d.Location.SourceSpan.Start)
            .ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            var count = counts[i];
            Assert.Equal("MEMBERS001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(
                $"Class '{expected[i].Name}' declares {count.Fields} fields, " +
                $"{count.Properties} properties, and {count.Indexers} indexers",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(expected[i].Tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }
    [Theory]
    [InlineData("public int first, second;", 2, 0, 0)]
    [InlineData("public int value = 10;", 1, 0, 0)]
    [InlineData("public volatile int value;", 1, 0, 0)]
    [InlineData("public string first = \"a\", second = \"b\";", 2, 0, 0)]
    [InlineData("public readonly int value;", 0, 0, 0)]
    [InlineData("public const int First = 1, Second = 2;", 0, 0, 0)]
    [InlineData("public static int first, second;", 0, 0, 0)]
    [InlineData("public static readonly int value;", 0, 0, 0)]
    [InlineData("private int value;", 0, 0, 0)]
    [InlineData("protected int value;", 0, 0, 0)]
    [InlineData("internal int value;", 0, 0, 0)]
    [InlineData("int value;", 0, 0, 0)]
    public async Task Exercise05_Fields_CountOnlyPublicInstanceMutableVariables(string members, int fields, int properties, int indexers)
    {
        string className = fields + properties + indexers > 0 ? "[|Sample|]" : "Sample";
        await Exercise05_VerifyAsync(
            "class SamplePair<A, B> { } class " + className + " { " + members + " }",
            fields + properties + indexers > 0
                ? new[] { (fields, properties, indexers) }
                : Array.Empty<(int, int, int)>());
    }

    [Theory]
    [InlineData("public int Value { get; set; }", 0, 1, 0)]
    [InlineData("public int Value { set { } }", 0, 1, 0)]
    [InlineData("public int Value { get => 0; set { } }", 0, 1, 0)]
    [InlineData("public int Value { get; set; } = 10;", 0, 1, 0)]
    [InlineData("public int Value { private get; set; }", 0, 1, 0)]
    [InlineData("public int Value { get; private set; }", 0, 0, 0)]
    [InlineData("public int Value { get; protected set; }", 0, 0, 0)]
    [InlineData("public int Value { get; internal set; }", 0, 0, 0)]
    [InlineData("public int Value { get; protected internal set; }", 0, 0, 0)]
    [InlineData("public int Value { get; private protected set; }", 0, 0, 0)]
    [InlineData("public int Value { get; init; }", 0, 0, 0)]
    [InlineData("public int Value { get; } = 10;", 0, 0, 0)]
    [InlineData("public int Value => 10;", 0, 0, 0)]
    [InlineData("public int Value { get { return 10; } }", 0, 0, 0)]
    [InlineData("public static int Value { get; set; }", 0, 0, 0)]
    [InlineData("internal int Value { get; set; }", 0, 0, 0)]
    [InlineData("private int Value { get; set; }", 0, 0, 0)]
    [InlineData("int Value { get; set; }", 0, 0, 0)]
    public async Task Exercise05_Properties_CountOnlyPublicInstanceUnrestrictedSetters(string members, int fields, int properties, int indexers)
    {
        string className = fields + properties + indexers > 0 ? "[|Sample|]" : "Sample";
        await Exercise05_VerifyAsync(
            "class SamplePair<A, B> { } class " + className + " { " + members + " }",
            fields + properties + indexers > 0
                ? new[] { (fields, properties, indexers) }
                : Array.Empty<(int, int, int)>());
    }

    [Theory]
    [InlineData("public int this[int x, int y] => 0;", 0, 0, 1)]
    [InlineData("public int this[int x, int y, int z] { get { return 0; } set { } }", 0, 0, 1)]
    [InlineData("public int this[int x, int y] { set { } }", 0, 0, 1)]
    [InlineData("public int this[SamplePair<int, string> pair, int index] => 0;", 0, 0, 1)]
    [InlineData("public int this[int index] => 0;", 0, 0, 0)]
    [InlineData("public int this[SamplePair<int, string> pair] => 0;", 0, 0, 0)]
    [InlineData("public int this[(int x, int y) position] => 0;", 0, 0, 0)]
    [InlineData("private int this[int x, int y] => 0;", 0, 0, 0)]
    [InlineData("internal int this[int x, int y] => 0;", 0, 0, 0)]
    [InlineData("int this[int x, int y] => 0;", 0, 0, 0)]
    public async Task Exercise05_Indexers_CountOnlyPublicInstanceMultiParameterDeclarations(string members, int fields, int properties, int indexers)
    {
        string className = fields + properties + indexers > 0 ? "[|Sample|]" : "Sample";
        await Exercise05_VerifyAsync(
            "class SamplePair<A, B> { } class " + className + " { " + members + " }",
            fields + properties + indexers > 0
                ? new[] { (fields, properties, indexers) }
                : Array.Empty<(int, int, int)>());
    }

    [Fact]
    public async Task Exercise05_MixedMembers_ReportOneCompleteSummary()
    {
        await Exercise05_VerifyAsync("""
            delegate void Callback();
            class [|Example|]
            {
                public int first, second;
                public readonly int SafeField;
                public const int Limit = 10;
                public static int SharedField;
                public int Count { get; set; }
                public int Maximum { get; private set; }
                public string Name { get; init; }
                public int this[int row, int column] => 0;
                public int this[int index] => 0;
                public static int Shared { get; set; }
                public event Callback FirstEvent, SecondEvent;
                public event Callback Custom { add { } remove { } }
                public void Method(int x, int y) { int local = 1; }
            }
            """, (2, 1, 1));
    }

    [Fact]
    public async Task Exercise05_MultipleViolationsAndNestedTypes_KeepCountsSeparate()
    {
        await Exercise05_VerifyAsync("""
            class [|Outer|]
            {
                public int first, second;
                public int A { get; set; }
                public int B { set { } }
                public int this[int x, int y] => 0;
                public string this[string x, string y, string z] => "";
                class [|Inner|]
                {
                    public int value;
                    public int Value { get; set; }
                }
                struct Ignored { public int value; }
            }
            """, (2, 2, 2), (1, 1, 0));
    }

    [Fact]
    public async Task Exercise05_OuterWithoutOwnViolations_ReportsOnlyNestedClass()
    {
        await Exercise05_VerifyAsync("""
            class Outer
            {
                private int hidden;
                public int Safe { get; init; }
                class [|Inner|] { public int value; }
            }
            """, (1, 0, 0));
    }

    [Fact]
    public async Task Exercise05_PartialDeclarationsInSameFile_AreIndependent()
    {
        await Exercise05_VerifyAsync("""
            partial class [|Sample|] { public int first, second; }
            partial class [|Sample|] { public int Value { get; set; } }
            partial class Sample { public static int Shared; }
            """, (2, 0, 0), (0, 1, 0));
    }

    [Fact]
    public async Task Exercise05_PartialDeclarationsAcrossFiles_AreIndependent()
    {
        await Exercise05_VerifySourcesAsync(
            [
                "partial class [|Sample|] { public int first, second; }",
                "partial class [|Sample|] { public int this[int x, int y] => 0; }",
                "partial class Sample { public int Safe { get; private set; } }"
            ], (2, 0, 0), (0, 0, 1));
    }

    [Fact]
    public async Task Exercise05_NonClassTypesAreIgnored_NestedClassesStillAnalyzed()
    {
        await Exercise05_VerifyAsync("""
            struct Container
            {
                public int field;
                public int Value { get; set; }
                public int this[int x, int y] => 0;
                public class [|Nested|] { public int value; }
            }
            record RecordContainer
            {
                public int field;
                public int Value { get; set; }
                public class [|Nested|] { public int Value { get; set; } }
            }
            record struct RecordStructContainer
            {
                public int field;
                public int Value { get; set; }
            }
            interface IContainer
            {
                public int Value { get; set; }
                public int this[int x, int y] { get; }
                public class [|Nested|] { public int this[int x, int y] => 0; }
            }
            """, (1, 0, 0), (0, 1, 0), (0, 0, 1));
    }

    [Fact]
    public async Task Exercise05_NoViolationsOrImplicitPublicMembers_ReportNothing()
    {
        await Exercise05_VerifyAsync("""
            interface IContract
            {
                int Value { get; set; }
                int this[int x, int y] { get; }
            }
            class Empty { }
            class Sample : IContract
            {
                public readonly int SafeField;
                public const int Limit = 10;
                public int SafeProperty { get; init; }
                public static int Shared { get; set; }
                int IContract.Value { get; set; }
                int IContract.this[int x, int y] => 0;
                public void Method() { int first = 1, second = 2; }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_EscapedClassIdentifierAndTrivia_VerifyNameAndExactSpan()
    {
        await Exercise05_VerifyAsync("""
            public class /* before */ [|@class|] /* after */
            {
                public int value;
            }
            """, (1, 0, 0));
    }

    private static Task Exercise05_VerifyAsync(
        string markedSource,
        params (int Fields, int Properties, int Indexers)[] counts)
    {
        return Exercise05_VerifySourcesAsync([markedSource], counts);
    }

    // Mark each expected class identifier with [|...|].
    // Counts are explicit, in file order and then source order within each file.
    private static async Task Exercise05_VerifySourcesAsync(
        string[] markedSources,
        params (int Fields, int Properties, int Indexers)[] counts)
    {
        var trees = new System.Collections.Generic.List<SyntaxTree>();
        var expected = new System.Collections.Generic.List<
            (SyntaxTree Tree, TextSpan Span, string Name)>();

        foreach (string markedSource in markedSources)
        {
            var source = new StringBuilder();
            var markers = new System.Collections.Generic.List<(TextSpan Span, string Name)>();
            int previousEnd = 0;
            foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]"))
            {
                source.Append(markedSource, previousEnd, match.Index - previousEnd);
                string tokenText = match.Groups[1].Value;
                markers.Add((new TextSpan(source.Length, tokenText.Length), tokenText.TrimStart('@')));
                source.Append(tokenText);
                previousEnd = match.Index + match.Length;
            }
            source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
                path: $"Exercise05Source{trees.Count}.cs");
            trees.Add(tree);
            foreach (var marker in markers)
            {
                expected.Add((tree, marker.Span, marker.Name));
            }
        }
        Assert.Equal(counts.Length, expected.Count);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise05TestAssembly",
            syntaxTrees: trees,
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson06PropertyLikeAndFieldDeclarations.Exercise05.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics
            .OrderBy(d => d.Location.SourceTree is null ? -1 : trees.IndexOf(d.Location.SourceTree))
            .ThenBy(d => d.Location.SourceSpan.Start)
            .ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            var count = counts[i];
            Assert.Equal("APIAUDIT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Class '{expected[i].Name}' exposes {count.Fields} mutable fields, " +
                $"{count.Properties} writable properties, and {count.Indexers} multi-parameter indexers",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(expected[i].Tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

}