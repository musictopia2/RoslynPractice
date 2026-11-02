using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson09AttributesAndAttributeArguments;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Theory]
    [InlineData("[[|Example|]] class Worker { }", 0)]
    [InlineData("[[|Example|]()] class Worker { }", 0)]
    [InlineData("[[|Example|](42)] class Worker { }", 1)]
    [InlineData("[[|Example|](1, 2, 3)] class Worker { }", 3)]
    public async Task Exercise01_BasicArgumentCounts(string source, int count)
    {
        await Exercise01_VerifyAsync(source, count);
    }

    [Theory]
    [InlineData("[[|Example|](message: \"hello\")] class Worker { }", 1)]
    [InlineData("[[|Example|](Enabled = true)] class Worker { }", 1)]
    [InlineData("[[|Example|](\"hello\", Enabled = true)] class Worker { }", 2)]
    [InlineData("[[|Example|](message: \"hello\", Enabled = true, Number = 3)] class Worker { }", 3)]
    public async Task Exercise01_NamedAndOrdinaryArguments_AllCount(string source, int count)
    {
        await Exercise01_VerifyAsync(source, count);
    }

    [Fact]
    public async Task Exercise01_NestedExpressions_CountOnlyDirectArguments()
    {
        await Exercise01_VerifyAsync("""
            [[|Example|](new int[] { 1, 2, 3 }, typeof(Dictionary<string, int>), Text = "a,b,c")]
            class Worker { }
            """, 3);
    }

    [Fact]
    public async Task Exercise01_MultipleAttributesInOneList_ReportIndividually()
    {
        await Exercise01_VerifyAsync("""
            [[|First|], [|Second|](1), [|Third|](1, 2)]
            class Worker { }
            """, 0, 1, 2);
    }

    [Fact]
    public async Task Exercise01_SeparateListsAndRepeatedAttributes_ReportEachOccurrence()
    {
        await Exercise01_VerifyAsync("""
            [[|Example|]]
            [[|Example|](1, 2)]
            class First { }
            [[|Example|]()]
            class Second { }
            """, 0, 2, 0);
    }

    [Theory]
    [InlineData("[[|Tools.Example|](1)] class Worker { }")]
    [InlineData("[[|Tools.ExampleAttribute|](1)] class Worker { }")]
    [InlineData("[[|global::Tools.ExampleAttribute|](1)] class Worker { }")]
    [InlineData("[[|ExampleAttribute|](1)] class Worker { }")]
    [InlineData("[[|@Example|](1)] class Worker { }")]
    public async Task Exercise01_NameTextAndLocation_PreserveEntireWrittenName(string source)
    {
        await Exercise01_VerifyAsync(source, 1);
    }

    [Fact]
    public async Task Exercise01_NameLocation_ExcludesSurroundingTriviaAndArguments()
    {
        await Exercise01_VerifyAsync("""
            [ /* before */ [|Tools.Example|] /* after */ (
                1,
                Enabled = true
            ) ]
            class Worker { }
            """, 2);
    }

    [Fact]
    public async Task Exercise01_AssemblyModuleAndReturnTargets_AreIncluded()
    {
        await Exercise01_VerifyAsync("""
            [assembly: [|AssemblyExample|](1)]
            [module: [|ModuleExample|]]
            class Worker
            {
                [return: [|ReturnExample|](Enabled = true)]
                int GetValue() => 1;
            }
            """, 1, 0, 1);
    }

    [Fact]
    public async Task Exercise01_TypesMembersParametersAndTypeParameters_AreIncluded()
    {
        await Exercise01_VerifyAsync("""
            [[|TypeExample|]]
            class Worker<[[|TypeParameterExample|](1)] T>
            {
                [[|FieldExample|](1, 2)] int field;
                [[|PropertyExample|]] int Property { get; set; }
                [[|ConstructorExample|]()] Worker() { }
                [[|MethodExample|](1)]
                void Run<[[|MethodTypeParameterExample|]] U>(
                    [[|ParameterExample|](1, 2, 3)] int value) { }
                [[|NestedTypeExample|]] class Nested { }
            }
            [[|StructExample|]] struct Other { }
            [[|InterfaceExample|]] interface IWorker { }
            [[|EnumExample|]] enum Choice
            {
                [[|EnumMemberExample|](1)] First
            }
            """, 0, 1, 2, 0, 0, 1, 0, 3, 0, 0, 0, 0, 1);
    }

    [Fact]
    public async Task Exercise01_ExplicitPropertyFieldAndParameterTargets_AreIncluded()
    {
        await Exercise01_VerifyAsync("""
            record Worker(
                [property: [|PropertyExample|](1)]
                [field: [|FieldExample|]]
                [param: [|ParameterExample|](1, 2)]
                int Value);
            """, 1, 0, 2);
    }

    [Fact]
    public async Task Exercise01_UnknownAttributeNames_StillUseSyntax()
    {
        // These attributes intentionally have no definitions.
        // Compiler binding errors do not change the syntax-only contract.
        await Exercise01_VerifyAsync("""
            [[|Unknown.Tools.CustomAttribute|]("text", Enabled = true)]
            class Worker { }
            """, 2);
    }

    [Fact]
    public async Task Exercise01_NoAttributes_ReportsNothing()
    {
        await Exercise01_VerifyAsync("""
            // [Example(1, 2)]
            /* [Example] */
            class Worker
            {
                string text = "[Example(1)]";
                int[] values = new int[] { 1, 2 };
                int GetValue() => values[0];
            }
            """);
    }

    [Fact]
    public async Task Exercise01_EmptyFile_ReportsNothing()
    {
        await Exercise01_VerifyAsync("");
    }

    [Fact]
    public void Exercise01_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise01.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("ATTRCOUNT001", rule.Id);
        Assert.Equal("Attribute argument count", rule.Title.ToString());
        Assert.Equal("Syntax", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Attribute '{0}' has {1} argument(s)", rule.MessageFormat.ToString());
    }

    // [|...|] marks only the expected attribute name, inside the real brackets.
    // Argument counts are explicit expectations in marker order.
    // This helper does not inspect attribute syntax or use the student's extensions.
    private static async Task Exercise01_VerifyAsync(
        string markedSource, params int[] argumentCounts)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string name = match.Groups[1].Value;
            expected.Add((new TextSpan(source.Length, name.Length), name));
            source.Append(name);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(expected.Count, argumentCounts.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise01TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise01.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include every analyzer diagnostic so analyzer crashes (AD0001) fail too.
        // Compiler binding diagnostics are outside this syntax-only exercise.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("ATTRCOUNT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Attribute '{expected[i].Name}' has {argumentCounts[i]} argument(s)",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("[Example([|message|]: \"hello\")] class Worker { }", "colon")]
    [InlineData("[Example([|Enabled|] = true)] class Worker { }", "equals")]
    public async Task Exercise02_NamedForms_ReportNameAndForm(string source, string form)
    {
        await Exercise02_VerifyAsync(source, form);
    }

    [Theory]
    [InlineData("[Example] class Worker { }")]
    [InlineData("[Example()] class Worker { }")]
    [InlineData("[Example(\"hello\", 42, true)] class Worker { }")]
    [InlineData("")]
    public async Task Exercise02_NoNamedArguments_ReportsNothing(string source)
    {
        await Exercise02_VerifyAsync(source);
    }

    [Fact]
    public async Task Exercise02_MixedArguments_ReportsOnlyNamedArguments()
    {
        await Exercise02_VerifyAsync("""
            [Example("hello", 42, [|enabled|]: true, [|Number|] = 3, [|Text|] = "value")]
            class Worker { }
            """, "colon", "equals", "equals");
    }

    [Fact]
    public async Task Exercise02_MultipleColonArguments_ReportEachOne()
    {
        await Exercise02_VerifyAsync("""
            [Example([|message|]: "hello", [|number|]: 42)]
            class Worker { }
            """, "colon", "colon");
    }

    [Fact]
    public async Task Exercise02_MultipleListsAndAttributes_ReportIndependently()
    {
        await Exercise02_VerifyAsync("""
            [First, Second([|Count|] = 3), Third([|text|]: "hello")]
            [Fourth(1, [|Enabled|] = true)]
            class Worker { }
            [Second([|Count|] = 4)]
            class Other { }
            """, "equals", "colon", "equals", "equals");
    }

    [Theory]
    [InlineData("[Example([|@class|]: 1)] class Worker { }", "colon")]
    [InlineData("[Example([|@class|] = 1)] class Worker { }", "equals")]
    public async Task Exercise02_EscapedNames_PreserveAtPrefix(string source, string form)
    {
        await Exercise02_VerifyAsync(source, form);
    }

    [Fact]
    public async Task Exercise02_Locations_ExcludeTriviaPunctuationAndExpressions()
    {
        await Exercise02_VerifyAsync("""
            [Example(
                /* before */ [|message|] /* after */ : "hello",
                /* before */ [|Enabled|] /* after */ = true)]
            class Worker { }
            """, "colon", "equals");
    }

    [Fact]
    public async Task Exercise02_QualifiedAndUnknownAttributeNames_DoNotFilterArguments()
    {
        await Exercise02_VerifyAsync("""
            [Unknown.Tools.ExampleAttribute([|message|]: "hello")]
            [global::Unknown.Other([|Enabled|] = true)]
            class Worker { }
            """, "colon", "equals");
    }

    [Fact]
    public async Task Exercise02_AttributeTargets_AreIncluded()
    {
        await Exercise02_VerifyAsync("""
            [assembly: Example([|text|]: "assembly")]
            [module: Example([|Enabled|] = true)]
            class Worker
            {
                [return: Example([|Value|] = 1)]
                int Run([Example([|number|]: 2)] int value) => value;
            }
            record Other(
                [property: Example([|Name|] = "value")]
                [field: Example([|Flag|] = true)]
                [param: Example([|text|]: "parameter")]
                int Value);
            """, "colon", "equals", "equals", "colon", "equals", "equals", "colon");
    }

    [Fact]
    public async Task Exercise02_DeclarationKinds_DoNotFilterArguments()
    {
        await Exercise02_VerifyAsync("""
            [Example([|Type|] = 1)]
            class Worker<[Example([|TypeParameter|] = 2)] T>
            {
                [Example([|Field|] = 3)] int field;
                [Example([|Property|] = 4)] int Property { get; set; }
                [Example([|Constructor|] = 5)] Worker() { }
                [Example([|Method|] = 6)]
                void Run<[Example([|MethodParameter|] = 7)] U>() { }
                [Example([|Nested|] = 8)] class Nested { }
            }
            [Example([|Interface|] = 9)] interface IWorker { }
            [Example([|Struct|] = 10)] struct Other { }
            [Example([|Enum|] = 11)] enum Choice
            {
                [Example([|Member|] = 12)] First
            }
            """, "equals", "equals", "equals", "equals", "equals", "equals",
            "equals", "equals", "equals", "equals", "equals", "equals");
    }

    [Fact]
    public async Task Exercise02_ArgumentExpressions_DoNotAddDiagnostics()
    {
        await Exercise02_VerifyAsync("""
            [Example(typeof(string), new int[] { 1, 2, 3 },
                [|Text|] = "name: value = true",
                [|Numbers|] = new int[] { 4, 5 })]
            class Worker { }
            """, "equals", "equals");
    }

    [Fact]
    public async Task Exercise02_NamedSyntaxOutsideAttributes_ReportsNothing()
    {
        await Exercise02_VerifyAsync("""
            // [Example(Name = "hello")]
            /* [Example(message: "hello")] */
            class Worker
            {
                string text = "[Example(Name = true)]";
                int Value { get; set; }
                void Run()
                {
                    Call(message: "hello");
                    var other = new Worker { Value = 1 };
                    var pair = (name: "hello", number: 42);
                }
                void Call(string message) { }
            }
            """);
    }

    [Fact]
    public void Exercise02_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise02.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("ATTRNAMED001", rule.Id);
        Assert.Equal("Named attribute argument", rule.Title.ToString());
        Assert.Equal("Syntax", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Attribute argument '{0}' uses {1} syntax", rule.MessageFormat.ToString());
    }

    // Mark only expected argument names. Forms are explicit, in marker order.
    private static async Task Exercise02_VerifyAsync(
        string markedSource, params string[] forms)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string name = match.Groups[1].Value;
            expected.Add((new TextSpan(source.Length, name.Length), name));
            source.Append(name);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(expected.Count, forms.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise02TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise02.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include every analyzer diagnostic so analyzer crashes (AD0001) fail too.
        // Compiler binding diagnostics are outside this syntax-only exercise.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("ATTRNAMED001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Attribute argument '{expected[i].Name}' uses {forms[i]} syntax",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("[[|assembly|]: Example]", 1)]
    [InlineData("[[|module|]: Example()]", 1)]
    [InlineData("[[|assembly|]: First, Second]", 2)]
    [InlineData("[[|assembly|]: First, Second, Third]", 3)]
    public async Task Exercise03_ExplicitTargets_ReportListCount(string source, int count)
    {
        await Exercise03_VerifyAsync(source, count);
    }

    [Fact]
    public async Task Exercise03_Arguments_DoNotIncreaseAttributeCount()
    {
        await Exercise03_VerifyAsync("""
            [[|assembly|]: First(1, 2, 3, Enabled = true), Second(message: "hello")]
            [[|module|]: Third(new int[] { 1, 2, 3 })]
            """, 2, 1);
    }

    [Fact]
    public async Task Exercise03_SeparateLists_AreNotCombined()
    {
        await Exercise03_VerifyAsync("""
            class Worker
            {
                [[|return|]: First, Second]
                [[|return|]: Third]
                [Fourth]
                int Run() => 1;
            }
            """, 2, 1);
    }

    [Fact]
    public async Task Exercise03_RepeatedAttributes_CountEveryOccurrence()
    {
        await Exercise03_VerifyAsync("""
            [[|assembly|]: Example, Example(), Example(1)]
            """, 3);
    }

    [Fact]
    public async Task Exercise03_TargetNames_AreNotFiltered()
    {
        await Exercise03_VerifyAsync("""
            [[|assembly|]: Example]
            [[|module|]: Example]
            [[|type|]: Example]
            class Worker<[[|typevar|]: Example] T>
            {
                [[|field|]: Example] int field;
                [[|property|]: Example] int Property { get; set; }
                [[|event|]: Example] event System.Action Changed;
                [[|method|]: Example]
                [[|return|]: Example]
                int Run([[|param|]: Example] int value) => value;
            }
            """, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
    }

    [Fact]
    public async Task Exercise03_RecordParameterLists_KeepTheirOwnTargetsAndCounts()
    {
        await Exercise03_VerifyAsync("""
            record Worker(
                [[|property|]: First, Second]
                [[|field|]: Third]
                [[|param|]: Fourth, Fifth, Sixth]
                int Value);
            """, 2, 1, 3);
    }

    [Fact]
    public async Task Exercise03_Location_ExcludesColonBracketsAndTrivia()
    {
        await Exercise03_VerifyAsync("""
            [ /* before */ [|assembly|] /* after */ : Example ]
            class Worker
            {
                [
                    [|return|]
                    : First,
                      Second
                ]
                int Run() => 1;
            }
            """, 1, 2);
    }

    [Fact]
    public async Task Exercise03_QualifiedUnknownAttributes_AreStillCounted()
    {
        await Exercise03_VerifyAsync("""
            [[|assembly|]: Unknown.Tools.FirstAttribute, global::Unknown.Second(1)]
            """, 2);
    }

    [Fact]
    public async Task Exercise03_ImplicitTargets_AreNotInferred()
    {
        await Exercise03_VerifyAsync("""
            [First, Second]
            class Worker<[Example] T>
            {
                [Example] int field;
                [Example] int Property { get; set; }
                [Example] Worker() { }
                [Example] int Run([Example] int value) => value;
                [Example] class Nested { }
            }
            """);
    }

    [Fact]
    public async Task Exercise03_AttributeArgumentNames_AreNotTargets()
    {
        await Exercise03_VerifyAsync("""
            [Example(assembly: "hello", module: 1, property = true)]
            class Worker { }
            """);
    }

    [Fact]
    public async Task Exercise03_MixedExplicitAndImplicitLists_ReportOnlyExplicitOnes()
    {
        await Exercise03_VerifyAsync("""
            [Example]
            class Worker
            {
                [First, Second]
                [[|return|]: Third, Fourth]
                int Run([Fifth] int value) => value;
                [[|field|]: Sixth]
                [Seventh]
                int field;
            }
            """, 2, 1);
    }

    [Fact]
    public async Task Exercise03_NoAttributes_ReportsNothing()
    {
        await Exercise03_VerifyAsync("""
            // [assembly: Example]
            /* [return: First, Second] */
            class Worker
            {
                string text = "[property: Example]";
                int[] values = new int[] { 1, 2 };
                int Run() => values[0];
            }
            """);
    }

    [Fact]
    public async Task Exercise03_EmptyFile_ReportsNothing()
    {
        await Exercise03_VerifyAsync("");
    }

    [Fact]
    public void Exercise03_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise03.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("ATTRTARGET001", rule.Id);
        Assert.Equal("Explicit attribute target", rule.Title.ToString());
        Assert.Equal("Syntax", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Attribute target '{0}' applies to {1} attribute(s)", rule.MessageFormat.ToString());
    }


    // Mark only target identifiers; counts are explicit in marker order.
    private static async Task Exercise03_VerifyAsync(
        string markedSource, params int[] attributeCounts)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string name = match.Groups[1].Value;
            expected.Add((new TextSpan(source.Length, name.Length), name));
            source.Append(name);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(expected.Count, attributeCounts.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise03TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise03.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include every analyzer diagnostic so analyzer crashes (AD0001) fail too.
        // Compiler binding diagnostics are outside this syntax-only exercise.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("ATTRTARGET001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Attribute target '{expected[i].Name}' applies to {attributeCounts[i]} attribute(s)",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("[First, Second(Enabled = true)]", 2, 1)]
    [InlineData("[First][Second(message: \"hello\")]", 2, 1)]
    [InlineData("[First(message: \"hello\", Enabled = true), Second(Count = 3)]", 2, 3)]
    [InlineData("[First, First(), First(Enabled = true)]", 3, 1)]
    [InlineData("[First(1, 2, 3), Second(4, Enabled = true), Third()]", 3, 1)]
    public async Task Exercise04_IncludedAttributesAndNamedArguments_AreCounted(
        string attributes, int attributeCount, int namedCount)
    {
        await Exercise04_VerifyAsync(
            "class Worker { " + attributes + " public void [|Run|]() { } }",
            (attributeCount, namedCount));
    }

    [Theory]
    [InlineData("")]
    [InlineData("[First]")]
    [InlineData("[First(Enabled = true, Count = 3)]")]
    [InlineData("[First, Second]")]
    [InlineData("[First(1), Second(2, 3)]")]
    public async Task Exercise04_ReportingThresholds_RequireBothConditions(string attributes)
    {
        await Exercise04_VerifyAsync(
            "class Worker { " + attributes + " public void Run() { } }");
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("")]
    public async Task Exercise04_MethodsWithoutExplicitPublic_AreIgnored(string modifiers)
    {
        await Exercise04_VerifyAsync(
            "class Worker { [First, Second(Enabled = true)] " + modifiers + " void Run() { } }");
    }

    [Fact]
    public async Task Exercise04_InterfaceAccessibility_MustBeExplicit()
    {
        await Exercise04_VerifyAsync("""
            interface IWorker
            {
                [First, Second(Enabled = true)] void Implicit();
                [First, Second(Enabled = true)] public void [|Explicit|]();
            }
            """, (2, 1));
    }

    [Fact]
    public async Task Exercise04_PublicMethodVariations_AreIncluded()
    {
        await Exercise04_VerifyAsync("""
            abstract class Worker
            {
                [First, Second(Flag = true)] public static void [|Static|]() { }
                [First, Second(text: "value")] public abstract void [|Abstract|]();
                [First, Second(Flag = true)] public virtual void [|Virtual|]() { }
                [First, Second(Flag = true)] public T [|Generic|]<T>(T value) => value;
            }
            """, (2, 1), (2, 1), (2, 1), (2, 1));
    }

    [Fact]
    public async Task Exercise04_ImplicitAndMethodTargets_AreCombined()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First(message: "hello")]
                [method: Second, Third(Enabled = true)]
                [Fourth]
                public void [|Run|]() { }
            }
            """, (4, 2));
    }

    [Fact]
    public async Task Exercise04_ReturnAttributes_ContributeToNeitherCount()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First, Second(Enabled = true)]
                [return: Third(Other = true), Fourth(text: "value")]
                public int [|Run|]() => 1;
            }
            """, (2, 1));
    }

    [Fact]
    public async Task Exercise04_ExcludedLists_CannotSatisfyThresholds()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First]
                [return: Second(Enabled = true)]
                public int OneIncluded() => 1;

                [First, Second]
                [return: Third(Enabled = true)]
                public int NoIncludedNamedArguments() => 1;

                [return: First, Second(Enabled = true)]
                public int OnlyReturnAttributes() => 1;
            }
            """);
    }

    [Fact]
    public async Task Exercise04_OtherExplicitTargets_AreExcludedWithoutValidation()
    {
        // Target validity is deliberately not checked by this syntax-only exercise.
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First, Second(Enabled = true)]
                [property: Third(Other = true)]
                [field: Fourth(text: "value"), Fifth]
                public void [|Run|]() { }

                [field: First, Second(Enabled = true)]
                public void ExcludedOnly() { }
            }
            """, (2, 1));
    }

    [Fact]
    public async Task Exercise04_ContainingTypeParametersAndLocalDeclarations_AreExcluded()
    {
        await Exercise04_VerifyAsync("""
            [Outer(Flag = true), OtherOuter]
            class Worker
            {
                [First, Second(Enabled = true)]
                public void [|Run|]<[TypeParameter(Name = "T")] T>(
                    [Parameter(text: "value")] int value)
                {
                    [Local(Flag = true), OtherLocal]
                    void Inner() { }
                }

                public void Unattributed<[TypeParameter(Flag = true)] T>(
                    [Parameter(Flag = true)] int value)
                {
                    [Local(Flag = true), OtherLocal]
                    void Inner() { }
                }
            }
            """, (2, 1));
    }

    [Fact]
    public async Task Exercise04_OverloadsAndNestedTypes_AreIndependent()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First, Second(Enabled = true)]
                public void [|Run|]() { }
                [First, Second(text: "value", Enabled = true), Third]
                public void [|Run|](int value) { }
                [First(Enabled = true)]
                public void Run(string value) { }
                class Nested
                {
                    [First, Second(Enabled = true)]
                    public void [|Run|]() { }
                }
            }
            class Other
            {
                [First] public void Run() { }
            }
            """, (2, 1), (3, 2), (2, 1));
    }

    [Fact]
    public async Task Exercise04_SeparateMethods_CannotCombineToMeetThresholds()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First(Enabled = true)] public void Run() { }
                [Second] public void Run(int value) { }
                [Third, Fourth] public void Unnamed() { }
                [Fifth(text: "hello")] public void Named() { }
            }
            """);
    }

    [Fact]
    public async Task Exercise04_MethodLocation_ExcludesTriviaAndOtherDeclarationParts()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First, Second(Enabled = true)]
                public static T /* before */ [|ReadValue|] /* after */ <T>(T value) => value;
            }
            """, (2, 1));
    }

    [Fact]
    public async Task Exercise04_EscapedMethodName_UsesValueTextButHighlightsWholeIdentifier()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First, Second(Enabled = true)]
                public void [|@class|]() { }
            }
            """, (2, 1));
    }

    [Fact]
    public async Task Exercise04_QualifiedUnknownAttributes_AreIncluded()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [Unknown.FirstAttribute]
                [method: global::Unknown.Second(text: "hello", Enabled = true)]
                public void [|Run|]() { }
            }
            """, (2, 2));
    }

    [Fact]
    public async Task Exercise04_ConstructorsPropertiesAndLocalFunctions_AreNotMethods()
    {
        await Exercise04_VerifyAsync("""
            class Worker
            {
                [First, Second(Enabled = true)] public Worker() { }
                [First, Second(Enabled = true)] public int Property { get; set; }
                public void Run()
                {
                    [First, Second(Enabled = true)] void Local() { }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise04_EmptyFile_ReportsNothing()
    {
        await Exercise04_VerifyAsync("");
    }

    [Fact]
    public void Exercise04_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise04.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("METHODATTR001", rule.Id);
        Assert.Equal("Public method attribute summary", rule.Title.ToString());
        Assert.Equal("Syntax", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Public method '{0}' has {1} attribute(s) with {2} named argument(s)",
            rule.MessageFormat.ToString());
    }

    // Mark only expected method identifiers. Supply explicit counts in marker order.
    // The optional @ prefix is omitted from the message, but remains in the location.
    private static async Task Exercise04_VerifyAsync(
        string markedSource, params (int Attributes, int NamedArguments)[] counts)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string identifier = match.Groups[1].Value;
            string name = identifier.StartsWith("@", StringComparison.Ordinal)
                ? identifier.Substring(1) : identifier;
            expected.Add((new TextSpan(source.Length, identifier.Length), name));
            source.Append(identifier);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(expected.Count, counts.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise04TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise04.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // All analyzer diagnostics are checked, including analyzer crashes (AD0001).
        // Attribute definitions and compiler binding diagnostics are not required.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("METHODATTR001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Public method '{expected[i].Name}' has {counts[i].Attributes} attribute(s) with {counts[i].NamedArguments} named argument(s)",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }


    [Theory]
    [InlineData("[[|Example|]][Example(Enabled = true)]")]
    [InlineData("[[|Example|](Enabled = true)][Example]")]
    [InlineData("[[|Example|](), Example(message: \"hello\")]")]
    [InlineData("[[|Example|](42), Example(Enabled = true)]")]
    public async Task Exercise05_MixedStyles_ReportAtFirstOccurrence(string attributes)
    {
        await Exercise05_VerifyAsync(
            "class Worker { " + attributes + " public void Run() { } }",
            ("Run", 1, 1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("[Example]")]
    [InlineData("[Example(Enabled = true)]")]
    [InlineData("[Example, Example(), Example(42)]")]
    [InlineData("[Example(Enabled = true), Example(message: \"hello\")]")]
    [InlineData("[First, Second(Enabled = true)]")]
    public async Task Exercise05_UniformOrSingleOccurrenceGroups_ReportNothing(string attributes)
    {
        await Exercise05_VerifyAsync(
            "class Worker { " + attributes + " public void Run() { } }");
    }

    [Fact]
    public async Task Exercise05_CountsAreOccurrences_NotArguments()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [[|Example|](Enabled = true, Count = 3, Text = "hello")]
                [Example(42), Example(), Example]
                [Example(message: "value", Flag = true)]
                public void Run() { }
            }
            """, ("Run", 2, 3));
    }

    [Fact]
    public async Task Exercise05_MultipleGroups_ProduceIndependentDiagnostics()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [[|First|], [|Second|](Enabled = true), Uniform]
                [First(text: "hello"), Second(42), Uniform()]
                [First(Enabled = true, Count = 3), Single(Enabled = true)]
                public void Run() { }
            }
            """, ("Run", 2, 1), ("Run", 1, 1));
    }

    [Fact]
    public async Task Exercise05_ExactWrittenNames_AreNotNormalized()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [Example, Example()]
                [ExampleAttribute(Enabled = true)]
                [Tools.Example(Enabled = true)]
                [example(Enabled = true)]
                [@Example(Enabled = true)]
                [global::Tools.Example(Enabled = true)]
                public void Run() { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_QualifiedAndEscapedNames_PreserveTextAndLocation()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [[|Tools.ExampleAttribute|](Enabled = true)]
                [Tools.ExampleAttribute]
                [[|@Example|]]
                [@Example(message: "hello")]
                [[|global::Tools.Other|]]
                [global::Tools.Other(Enabled = true)]
                public void Run() { }
            }
            """, ("Run", 1, 1), ("Run", 1, 1), ("Run", 1, 1));
    }

    [Fact]
    public async Task Exercise05_ImplicitAndMethodLists_Combine()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [method: [|Example|](Enabled = true)]
                [Example]
                [method: Example(42)]
                public void Run() { }
            }
            """, ("Run", 1, 2));
    }

    [Fact]
    public async Task Exercise05_ExcludedLists_DoNotAffectCountsOrFirstLocation()
    {
        // Unsupported target/declaration combinations are binding concerns,
        // deliberately outside the syntax-only contract.
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [return: Example(Enabled = true)]
                [field: Example]
                [property: Example(Flag = true)]
                [[|Example|]]
                [method: Example(text: "hello")]
                [return: Example]
                public int Run() => 1;
            }
            """, ("Run", 1, 1));
    }

    [Fact]
    public async Task Exercise05_ExcludedOccurrences_CannotCreateMixedGroup()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [Example]
                [return: Example(Enabled = true)]
                public int OnlyUnnamed() => 1;
                [Example(Enabled = true)]
                [return: Example]
                public int OnlyNamed() => 1;
                [return: Example, Example(Enabled = true)]
                public int OnlyReturn() => 1;
            }
            """);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("")]
    public async Task Exercise05_MethodsWithoutExplicitPublic_AreIgnored(string modifiers)
    {
        await Exercise05_VerifyAsync(
            "class Worker { [Example, Example(Flag = true)] " + modifiers + " void Run() { } }");
    }

    [Fact]
    public async Task Exercise05_PublicMethodVariations_AreIncluded()
    {
        await Exercise05_VerifyAsync("""
            abstract class Worker
            {
                [[|Example|], Example(Flag = true)] public static void Static() { }
                [[|Example|], Example(Flag = true)] public abstract void Abstract();
                [[|Example|], Example(Flag = true)] public virtual void Virtual() { }
                [[|Example|], Example(Flag = true)] public T Generic<T>(T value) => value;
            }
            interface IWorker
            {
                [Example, Example(Flag = true)] void Implicit();
                [[|Example|], Example(Flag = true)] public void Explicit();
            }
            """, ("Static", 1, 1), ("Abstract", 1, 1), ("Virtual", 1, 1),
            ("Generic", 1, 1), ("Explicit", 1, 1));
    }

    [Fact]
    public async Task Exercise05_OverloadsTypesAndPartialDeclarations_AreNotCombined()
    {
        await Exercise05_VerifyAsync("""
            partial class Worker
            {
                [Example] public void Run() { }
                [Example(Flag = true)] public void Run(int value) { }
                [Example] public partial void Split();
                class Nested
                {
                    [Example(Flag = true)] public void Run() { }
                }
            }
            partial class Worker
            {
                [Example(Flag = true)] public partial void Split() { }
            }
            class Other
            {
                [Example(Flag = true)] public void Run() { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_MultipleMethods_KeepCountsAndLocationsIndependent()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [[|Example|], Example(Flag = true)]
                public void Run() { }
                [[|Example|](text: "hello"), Example(), Example(42)]
                public void Run(int value) { }
                class Nested
                {
                    [[|Example|](Flag = true), Example]
                    public void Read() { }
                }
            }
            """, ("Run", 1, 1), ("Run", 1, 2), ("Read", 1, 1));
    }

    [Fact]
    public async Task Exercise05_NonMethodAttributeLocations_AreExcluded()
    {
        await Exercise05_VerifyAsync("""
            [Example, Example(Flag = true)]
            class Worker
            {
                [Example, Example(Flag = true)] public Worker() { }
                [Example, Example(Flag = true)] public int Value { get; set; }
                [Example]
                public void Run<[Example(Flag = true)] T>([Example(Flag = true)] int value)
                {
                    [Example, Example(Flag = true)] void Local() { }
                }
                [[|Example|], Example(Flag = true)]
                public void Included<[Example(Flag = true)] T>([Example] int value) { }
            }
            """, ("Included", 1, 1));
    }

    [Fact]
    public async Task Exercise05_FirstNameLocation_ExcludesTriviaAndOtherSyntax()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                [method: /* before */ [|Tools.Example|] /* after */ (Flag = true)]
                [Tools.Example]
                public void @class() { }
            }
            """, ("class", 1, 1));
    }

    [Fact]
    public async Task Exercise05_NoAttributesOrEmptyFile_ReportNothing()
    {
        await Exercise05_VerifyAsync("");
        await Exercise05_VerifyAsync("""
            class Worker
            {
                // [Example, Example(Flag = true)]
                public void Run()
                {
                    string text = "[Example, Example(Flag = true)]";
                }
            }
            """);
    }

    [Fact]
    public void Exercise05_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise05.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("ATTRMIX001", rule.Id);
        Assert.Equal("Inconsistent repeated attribute configuration", rule.Title.ToString());
        Assert.Equal("Syntax", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Public method '{0}' mixes configuration for attribute '{1}': " +
            "{2} occurrence(s) with named arguments, {3} without",
            rule.MessageFormat.ToString());
    }

    // Mark the complete name of the FIRST INCLUDED occurrence of each reported group.
    // Explicit expectations are in marker order, not derived from the syntax tree.
    private static async Task Exercise05_VerifyAsync(
        string markedSource, params (string Method, int WithNamed, int WithoutNamed)[] counts)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string AttributeName)>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string name = match.Groups[1].Value;
            expected.Add((new TextSpan(source.Length, name.Length), name));
            source.Append(name);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(expected.Count, counts.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise05TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson09AttributesAndAttributeArguments.Exercise05.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include AD0001 crashes and unexpected analyzer diagnostics.
        // Compiler binding diagnostics are outside this syntax-only contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("ATTRMIX001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Public method '{counts[i].Method}' mixes configuration for attribute '{expected[i].AttributeName}': " +
                $"{counts[i].WithNamed} occurrence(s) with named arguments, {counts[i].WithoutNamed} without",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

}