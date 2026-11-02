using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson08NamespaceAndUsingDeclarations;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise01_BlockScopedNamespace_ReportsName()
    {
        await Exercise01_VerifyAsync("""
            namespace [|Example|]
            {
                public class Worker { }
            }
            """, "block-scoped");
    }

    [Fact]
    public async Task Exercise01_FileScopedNamespace_ReportsName()
    {
        await Exercise01_VerifyAsync("""
            namespace [|Example.Tools|];
            public class Worker { }
            """, "file-scoped");
    }

    [Theory]
    [InlineData("namespace [|Empty|] { }", "block-scoped")]
    [InlineData("namespace [|Empty|];", "file-scoped")]
    public async Task Exercise01_EmptyNamespace_IsIncluded(string source, string form)
    {
        await Exercise01_VerifyAsync(source, form);
    }

    [Fact]
    public async Task Exercise01_QualifiedBlockName_HighlightsCompleteName()
    {
        await Exercise01_VerifyAsync("""
            namespace [|Company.Product.Tools|]
            {
            }
            """, "block-scoped");
    }

    [Fact]
    public async Task Exercise01_NestedNamespaces_UseEachDeclaredName()
    {
        await Exercise01_VerifyAsync("""
            namespace [|Outer|]
            {
                namespace [|Inner.Tools|]
                {
                    namespace [|Deep|]
                    {
                        class Worker { }
                    }
                }
            }
            """, "block-scoped", "block-scoped", "block-scoped");
    }

    [Fact]
    public async Task Exercise01_SiblingAndRepeatedNamespaces_ReportEveryDeclaration()
    {
        await Exercise01_VerifyAsync("""
            namespace [|Example.Tools|] { class First { } }
            namespace [|Other|] { }
            namespace [|Example.Tools|] { class Second { } }
            """, "block-scoped", "block-scoped", "block-scoped");
    }

    [Theory]
    [InlineData("namespace /* before */ [|Example.Tools|] /* after */ { }", "block-scoped")]
    [InlineData("namespace /* before */ [|Example.Tools|] /* after */ ;", "file-scoped")]
    public async Task Exercise01_NameLocation_ExcludesSurroundingTrivia(
        string source, string form)
    {
        await Exercise01_VerifyAsync(source, form);
    }

    [Theory]
    [InlineData("namespace [|@class.Tools|] { }", "block-scoped")]
    [InlineData("namespace [|@class.Tools|];", "file-scoped")]
    public async Task Exercise01_EscapedName_PreservesNameSyntaxText(
        string source, string form)
    {
        await Exercise01_VerifyAsync(source, form);
    }

    [Fact]
    public async Task Exercise01_UsingsAndMultipleTypes_DoNotAddDiagnostics()
    {
        await Exercise01_VerifyAsync("""
            using System;
            using Text = System.String;
            using static System.Math;
            namespace [|Example|]
            {
                using System.Collections.Generic;
                class First { class Nested { } }
                struct Second { }
                interface IThird { }
                enum Fourth { Value }
            }
            """, "block-scoped");
    }

    [Fact]
    public async Task Exercise01_FileScopedNamespaceWithUsings_ReportsOnlyNamespace()
    {
        await Exercise01_VerifyAsync("""
            global using System;
            using Text = System.String;
            namespace [|Example.Tools|];
            using System.Collections.Generic;
            class First { }
            class Second { }
            """, "file-scoped");
    }

    [Fact]
    public async Task Exercise01_NoNamespaceDeclarations_ReportsNothing()
    {
        await Exercise01_VerifyAsync("""
            using System;
            using Text = System.String;
            // namespace Fake.Comment { }
            /* namespace Fake.Block; */
            class Outside
            {
                string text = "namespace Fake.String { }";
                class Nested { }
            }
            struct Other { }
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
            .Lesson08NamespaceAndUsingDeclarations.Exercise01.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("NSDECL001", rule.Id);
        Assert.Equal("Namespace declaration", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Namespace '{0}' uses a {1} declaration", rule.MessageFormat.ToString());
    }

    // Mark the complete expected namespace name with [|...|].
    // Forms are supplied explicitly in marker order; expectations are not
    // inferred by walking namespace syntax or calling the student's helpers.
    private static async Task Exercise01_VerifyAsync(
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
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise01TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise01.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer diagnostics, including AD0001 analyzer crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("NSDECL001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(
                $"Namespace '{expected[i].Name}' uses a {forms[i]} declaration",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("using [|System|];")]
    [InlineData("using [|System.Collections.Generic|];")]
    [InlineData("using [|global::System.Text|];")]
    [InlineData("using [|@class.Tools|];")]
    [InlineData("using /* before */ [|Example.Tools|] /* after */ ;")]
    public async Task Exercise02_OrdinaryImport_ReportsCompleteName(string source)
    {
        await Exercise02_VerifyAsync(source);
    }

    [Fact]
    public async Task Exercise02_AllNamespaceLocations_AreIncluded()
    {
        await Exercise02_VerifyAsync("""
            using [|System|];
            namespace Outer
            {
                using [|System.Text|];
                namespace Inner
                {
                    using [|System.Collections.Generic|];
                    class Worker { }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_FileScopedNamespace_IncludesUsingsOnBothSides()
    {
        await Exercise02_VerifyAsync("""
            using [|System|];
            namespace Example;
            using [|System.Text|];
            class Worker { }
            """);
    }

    [Fact]
    public async Task Exercise02_RepeatedImports_AreNotDeduplicated()
    {
        await Exercise02_VerifyAsync("""
            using [|System|];
            using [|System|];
            namespace Example
            {
                using [|System|];
            }
            """);
    }

    [Theory]
    [InlineData("using Text = System.String;")]
    [InlineData("using Collections = System.Collections;")]
    [InlineData("using static System.Math;")]
    [InlineData("global using System;")]
    [InlineData("global using static System.Math;")]
    [InlineData("global using Text = System.String;")]
    [InlineData("using Text = global::System.String;")]
    [InlineData("using static global::System.Math;")]
    [InlineData("using Pair = (int X, int Y);")]
    public async Task Exercise02_ExcludedDirectiveForms_ReportNothing(string source)
    {
        await Exercise02_VerifyAsync(source);
    }

    [Fact]
    public async Task Exercise02_MixedForms_ReportsOnlyOrdinaryImports()
    {
        await Exercise02_VerifyAsync("""
            global using System.Collections;
            global using static System.Math;
            global using Text = System.String;
            using [|System|];
            using OtherText = System.String;
            using static System.Math;
            namespace Example
            {
                using [|global::System.Text|];
                using Builder = System.Text.StringBuilder;
                using static System.Math;
                class Worker { }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_ResourceManagementAndText_ReportNothing()
    {
        await Exercise02_VerifyAsync("""
            // using Fake.Comment;
            /* global using Fake.Block; */
            namespace Example
            {
                sealed class Resource : System.IDisposable
                {
                    public void Dispose() { }
                }
                class Worker
                {
                    string text = "using Fake.String;";
                    void Run()
                    {
                        using (var first = new Resource()) { }
                        using var second = new Resource();
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_EmptyFile_ReportsNothing()
    {
        await Exercise02_VerifyAsync("");
    }

    [Fact]
    public void Exercise02_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise02.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("USINGIMPORT001", rule.Id);
        Assert.Equal("Ordinary namespace import", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Using directive imports namespace '{0}'", rule.MessageFormat.ToString());
    }

    // Mark only included imported names with [|...|].
    // Expected names and spans come from markers, not an analyzer syntax walk.
    private static async Task Exercise02_VerifyAsync(
        string markedSource)
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

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise02TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise02.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer diagnostics, including AD0001 analyzer crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("USINGIMPORT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(
                $"Using directive imports namespace '{expected[i].Name}'",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("global using [|System|];", "namespace")]
    [InlineData("global using [|System.Collections.Generic|];", "namespace")]
    [InlineData("global using [|global::System.Text|];", "namespace")]
    [InlineData("global using [|@class.Tools|];", "namespace")]
    [InlineData("global using static [|System.Math|];", "static")]
    [InlineData("global using static [|global::System.Math|];", "static")]
    [InlineData("global using static [|Example.Outer.Inner|];", "static")]
    public async Task Exercise03_GlobalImports_ClassifyAndHighlightName(
        string source, string kind)
    {
        await Exercise03_VerifyAsync(source, kind);
    }

    [Theory]
    [InlineData("global /* a */ using /* b */ [|System.Text|] /* c */ ;", "namespace")]
    [InlineData("global /* a */ using /* b */ static /* c */ [|System.Math|] /* d */ ;", "static")]
    public async Task Exercise03_Location_ExcludesKeywordsAndSurroundingTrivia(
        string source, string kind)
    {
        await Exercise03_VerifyAsync(source, kind);
    }

    [Fact]
    public async Task Exercise03_ClientExample_ReportsOnlyNonAliasGlobalImports()
    {
        await Exercise03_VerifyAsync("""
            global using [|System.Text|];
            global using static [|global::System.Math|];
            global using Text = System.String;
            using global::System.Collections;
            using static System.Math;
            """, "namespace", "static");
    }

    [Fact]
    public async Task Exercise03_GlobalPrefixAlone_DoesNotMakeDirectiveGlobal()
    {
        await Exercise03_VerifyAsync("""
            global using [|global::System.Text|];
            global using static [|global::System.Math|];
            using global::System.Text;
            using static global::System.Math;
            """, "namespace", "static");
    }

    [Fact]
    public async Task Exercise03_RepeatedImports_ReportEachDeclaration()
    {
        await Exercise03_VerifyAsync("""
            global using [|System|];
            global using static [|System.Math|];
            global using [|System|];
            global using static [|System.Math|];
            """, "namespace", "static", "namespace", "static");
    }

    [Theory]
    [InlineData("using System;")]
    [InlineData("using static System.Math;")]
    [InlineData("using global::System.Text;")]
    [InlineData("using Text = System.String;")]
    [InlineData("global using Text = System.String;")]
    [InlineData("global using Collections = System.Collections;")]
    [InlineData("global using Text = global::System.String;")]
    [InlineData("global using Pair = (int X, int Y);")]
    public async Task Exercise03_ExcludedForms_ReportNothing(string source)
    {
        await Exercise03_VerifyAsync(source);
    }

    [Fact]
    public async Task Exercise03_BlockNamespace_LocalImportsRemainExcluded()
    {
        await Exercise03_VerifyAsync("""
            global using [|System|];
            namespace Outer
            {
                using System.Text;
                using static System.Math;
                namespace Inner
                {
                    using global::System.Collections;
                    class Worker { }
                }
            }
            """, "namespace");
    }

    [Fact]
    public async Task Exercise03_FileScopedNamespace_LocalImportsRemainExcluded()
    {
        await Exercise03_VerifyAsync("""
            global using static [|System.Math|];
            namespace Example;
            using System.Text;
            using static System.Math;
            class Worker { }
            """, "static");
    }

    [Fact]
    public async Task Exercise03_CommentsStringsAndResourceManagement_ReportNothing()
    {
        await Exercise03_VerifyAsync("""
            // global using Fake.Comment;
            /* global using static Fake.Block; */
            namespace Example
            {
                sealed class Resource : System.IDisposable
                {
                    public void Dispose() { }
                }
                class Worker
                {
                    string text = "global using Fake.String;";
                    void Run()
                    {
                        using (var first = new Resource()) { }
                        using var second = new Resource();
                    }
                }
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
            .Lesson08NamespaceAndUsingDeclarations.Exercise03.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("GLOBALIMPORT001", rule.Id);
        Assert.Equal("Global import", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Global using imports '{0}' as a {1} import", rule.MessageFormat.ToString());
    }

    // Mark the complete expected imported name with [|...|].
    // Import kinds are supplied explicitly in marker order; expectations are not
    // inferred by walking namespace syntax or calling the student's helpers.
    private static async Task Exercise03_VerifyAsync(
        string markedSource, params string[] kinds)
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
        Assert.Equal(expected.Count, kinds.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise03TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise03.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer diagnostics, including AD0001 analyzer crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("GLOBALIMPORT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(
                $"Global using imports '{expected[i].Name}' as a {kinds[i]} import",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Exercise04_Threshold_RequiresAtLeastThreeDirectImports(int count)
    {
        string imports = string.Concat(Enumerable.Repeat("using System;\n", count));
        if (count < 3)
        {
            await Exercise04_VerifyAsync("namespace Example { " + imports + " }");
        }
        else
        {
            await Exercise04_VerifyAsync("namespace [|Example|] { " + imports + " }",
                ("block-scoped", count, 0, 0));
        }
    }

    [Fact]
    public async Task Exercise04_MixedImports_CountEachCategory()
    {
        await Exercise04_VerifyAsync("""
            using System.Collections;
            namespace [|Outer|]
            {
                using System;
                using static System.Math;
                using Text = System.String;
                namespace Inner
                {
                    using System.Text;
                    using System.Collections.Generic;
                }
            }
            """, ("block-scoped", 1, 1, 1));
    }

    [Theory]
    [InlineData("using static System.Math; using static System.Console; using static System.Math;", 0, 3, 0)]
    [InlineData("using A = System.String; using B = System.Int32; using Pair = (int X, int Y);", 0, 0, 3)]
    [InlineData("using global::System; using System.Text; using System.Collections;", 3, 0, 0)]
    public async Task Exercise04_EachCategoryAlone_CanReachThreshold(
        string imports, int ordinary, int statics, int aliases)
    {
        await Exercise04_VerifyAsync("namespace [|Example|] { " + imports + " }",
            ("block-scoped", ordinary, statics, aliases));
    }

    [Fact]
    public async Task Exercise04_FileScopedNamespace_CountsOnlyItsOwnImports()
    {
        await Exercise04_VerifyAsync("""
            global using System.Collections;
            using System;
            using static System.Console;
            using OutsideAlias = System.String;
            namespace [|Example.Tools|];
            using global::System.Text;
            using static global::System.Math;
            using Pair = (int X, int Y);
            class Worker { }
            """, ("file-scoped", 1, 1, 1));
    }

    [Fact]
    public async Task Exercise04_OutsideImports_DoNotCompleteThreshold()
    {
        await Exercise04_VerifyAsync("""
            using System;
            using System.Text;
            using System.Collections;
            namespace Example;
            using static System.Math;
            using Text = System.String;
            class Worker { }
            """);
    }

    [Fact]
    public async Task Exercise04_ParentAndNestedNamespace_ReportIndependentCounts()
    {
        await Exercise04_VerifyAsync("""
            namespace [|Outer|]
            {
                using System;
                using System.Text;
                using static System.Math;
                namespace [|Inner.Tools|]
                {
                    using Text = System.String;
                    using Number = System.Int32;
                    using Pair = (int X, int Y);
                    using System.Collections;
                }
            }
            """, ("block-scoped", 2, 1, 0), ("block-scoped", 1, 0, 3));
    }

    [Fact]
    public async Task Exercise04_NestedImports_DoNotCompleteParentThreshold()
    {
        await Exercise04_VerifyAsync("""
            namespace Outer
            {
                using System;
                namespace [|Inner|]
                {
                    using System.Text;
                    using static System.Math;
                    using Text = System.String;
                }
            }
            """, ("block-scoped", 1, 1, 1));
    }

    [Fact]
    public async Task Exercise04_EnclosingImports_DoNotCompleteNestedThreshold()
    {
        await Exercise04_VerifyAsync("""
            namespace [|Outer|]
            {
                using System;
                using System.Text;
                using System.Collections;
                namespace Inner
                {
                    using static System.Math;
                }
            }
            """, ("block-scoped", 3, 0, 0));
    }

    [Fact]
    public async Task Exercise04_RepeatedDeclarations_DoNotCombineCounts()
    {
        await Exercise04_VerifyAsync("""
            namespace Example { using System; using System.Text; }
            namespace Example { using System.Collections; }
            namespace [|Example|]
            {
                using System;
                using static System.Math;
                using Text = System.String;
            }
            """, ("block-scoped", 1, 1, 1));
    }

    [Theory]
    [InlineData("namespace /* before */ [|Example.Tools|] /* after */ { using System; using System; using System; }", "block-scoped")]
    [InlineData("namespace /* before */ [|@class.Tools|] /* after */ ; using System; using System; using System;", "file-scoped")]
    public async Task Exercise04_NameAndLocation_UseCompleteDeclarationName(
        string source, string form)
    {
        await Exercise04_VerifyAsync(source, (form, 3, 0, 0));
    }

    [Fact]
    public async Task Exercise04_OtherSyntax_DoesNotIncreaseCounts()
    {
        await Exercise04_VerifyAsync("""
            namespace Example
            {
                using System;
                using System.Text;
                // using System.Collections;
                /* using static System.Math; */
                sealed class Resource : System.IDisposable
                {
                    public void Dispose() { }
                }
                class Worker
                {
                    string text = "using System.Collections;";
                    void Run()
                    {
                        using (var first = new Resource()) { }
                        using var second = new Resource();
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise04_EmptySource_ReportsNothing()
    {
        await Exercise04_VerifyAsync("");
    }

    [Fact]
    public void Exercise04_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise04.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("NSIMPORTSUMMARY001", rule.Id);
        Assert.Equal("Namespace import summary", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Info, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Namespace '{0}' ({1}) has direct imports: ordinary={2}, static={3}, alias={4}",
            rule.MessageFormat.ToString());
    }

    // Mark the complete expected namespace name with [|...|].
    // Forms and counts are supplied explicitly in marker order; expectations are not
    // inferred by walking namespace syntax or calling the student's helpers.
    private static async Task Exercise04_VerifyAsync(
        string markedSource,
        params (string Form, int Ordinary, int Static, int Alias)[] details)
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
        Assert.Equal(expected.Count, details.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise04TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise04.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer diagnostics, including AD0001 analyzer crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("NSIMPORTSUMMARY001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(
                $"Namespace '{expected[i].Name}' ({details[i].Form}) has direct imports: " +
                $"ordinary={details[i].Ordinary}, static={details[i].Static}, alias={details[i].Alias}",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Exercise05_TotalCount_ReportsOnceOnSecondOccurrence(int count)
    {
        string source = "using System.Text;\nusing [|System.Text|];\n" +
            string.Concat(Enumerable.Repeat("using System.Text;\n", count - 2));
        await Exercise05_VerifyAsync(source, ("<compilation unit>", "namespace", count));
    }

    [Fact]
    public async Task Exercise05_StaticImport_ReportsSecondCompleteName()
    {
        await Exercise05_VerifyAsync("""
            namespace Example.Tools;
            using static global::System.Math;
            using static /* before */ [|global::System.Math|] /* after */ ;
            """, ("Example.Tools", "static", 2));
    }

    [Fact]
    public async Task Exercise05_InterleavedGroups_ReportEachSecondOccurrence()
    {
        await Exercise05_VerifyAsync("""
            using System;
            using static System.Math;
            using [|System|];
            using System.Text;
            using static [|System.Math|];
            using System;
            using [|System.Text|];
            """, ("<compilation unit>", "namespace", 3),
            ("<compilation unit>", "static", 2),
            ("<compilation unit>", "namespace", 2));
    }

    [Fact]
    public async Task Exercise05_ImportKind_IsPartOfGroupingKey()
    {
        // Semantic validity is irrelevant: both forms are syntactically valid.
        await Exercise05_VerifyAsync("""
            using Example.Target;
            using static Example.Target;
            using [|Example.Target|];
            using static [|Example.Target|];
            """, ("<compilation unit>", "namespace", 2),
            ("<compilation unit>", "static", 2));
    }

    [Fact]
    public async Task Exercise05_ExactNameText_IsPartOfGroupingKey()
    {
        await Exercise05_VerifyAsync("""
            using Example.Tools;
            using Example.tools;
            using global::Example.Tools;
            using @Example.Tools;
            using [|Example.Tools|];
            using [|global::Example.Tools|];
            using [|@Example.Tools|];
            """, ("<compilation unit>", "namespace", 2),
            ("<compilation unit>", "namespace", 2),
            ("<compilation unit>", "namespace", 2));
    }

    [Fact]
    public async Task Exercise05_ExcludedForms_DoNotAffectCountOrSecondLocation()
    {
        await Exercise05_VerifyAsync("""
            global using System.Text;
            global using System.Text;
            global using static System.Math;
            global using static System.Math;
            global using G = System.Text;
            using A = System.Text;
            using System.Text;
            using B = System.Text;
            using static System.Math;
            using [|System.Text|];
            using static [|System.Math|];
            """, ("<compilation unit>", "namespace", 2),
            ("<compilation unit>", "static", 2));
    }

    [Theory]
    [InlineData("global using System; global using System;")]
    [InlineData("global using static System.Math; global using static System.Math;")]
    [InlineData("using A = System.String; using B = System.String;")]
    [InlineData("global using A = System.String; global using B = System.String;")]
    [InlineData("using A = (int X, int Y); using B = (int X, int Y);")]
    [InlineData("using System; using System.Text; using static System.Math;")]
    [InlineData("using Example.Tools; using Example.tools; using global::Example.Tools;")]
    [InlineData("")]
    public async Task Exercise05_NoQualifyingGroups_ReportNothing(string source)
    {
        await Exercise05_VerifyAsync(source);
    }

    [Fact]
    public async Task Exercise05_CompilationUnitAndNestedScopes_AreIndependent()
    {
        await Exercise05_VerifyAsync("""
            using System;
            using [|System|];
            namespace Outer
            {
                using System;
                using [|System|];
                namespace Inner.Tools
                {
                    using System;
                    using [|System|];
                    using System;
                }
            }
            """, ("<compilation unit>", "namespace", 2),
            ("Outer", "namespace", 2), ("Inner.Tools", "namespace", 3));
    }

    [Fact]
    public async Task Exercise05_SingleImportInEachScope_DoesNotCombine()
    {
        await Exercise05_VerifyAsync("""
            using System;
            namespace Outer
            {
                using System;
                namespace Inner { using System; }
            }
            namespace Sibling { using System; }
            """);
    }

    [Fact]
    public async Task Exercise05_FileScopedNamespace_IsSeparateFromCompilationUnit()
    {
        await Exercise05_VerifyAsync("""
            using System;
            using [|System|];
            namespace Example;
            using System;
            using [|System|];
            """, ("<compilation unit>", "namespace", 2), ("Example", "namespace", 2));
    }

    [Fact]
    public async Task Exercise05_RepeatedNamespaceDeclarations_AreIndependent()
    {
        await Exercise05_VerifyAsync("""
            namespace Example { using System; }
            namespace Example { using System; }
            namespace Example { using System; using [|System|]; }
            namespace Example { using System; using [|System|]; using System; }
            """, ("Example", "namespace", 2), ("Example", "namespace", 3));
    }

    [Fact]
    public async Task Exercise05_SeparateFiles_DoNotCombineSingleOccurrences()
    {
        await Exercise05_VerifyFilesAsync(new[]
        {
            "using System; namespace Example { using System.Text; }",
            "using System; namespace Example { using System.Text; }"
        });
    }

    [Fact]
    public async Task Exercise05_SeparateFiles_ReportTheirOwnGroupsAndCounts()
    {
        await Exercise05_VerifyFilesAsync(new[]
        {
            "using System; using [|System|];",
            "using System; using [|System|]; using System;"
        }, ("<compilation unit>", "namespace", 2),
           ("<compilation unit>", "namespace", 3));
    }

    [Fact]
    public async Task Exercise05_CommentsStringsAndResourceUsings_DoNotCreateGroups()
    {
        await Exercise05_VerifyAsync("""
            using System;
            // using System;
            /* using System; */
            namespace Example
            {
                sealed class Resource : System.IDisposable
                {
                    public void Dispose() { }
                }
                class Worker
                {
                    string text = "using System; using System;";
                    void Run()
                    {
                        using (var first = new Resource()) { }
                        using var second = new Resource();
                    }
                }
            }
            """);
    }

    [Fact]
    public void Exercise05_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise05.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("DUPIMPORT001", rule.Id);
        Assert.Equal("Duplicate import within a scope", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Scope '{0}' contains {1} occurrences of {2} import '{3}'",
            rule.MessageFormat.ToString());
    }

    private static Task Exercise05_VerifyAsync(
        string markedSource,
        params (string Scope, string Kind, int Count)[] details)
    {
        return Exercise05_VerifyFilesAsync(new[] { markedSource }, details);
    }

    // Mark expected second occurrences explicitly. Details follow marker order,
    // first by file and then by source position. No syntax walk infers expectations.
    private static async Task Exercise05_VerifyFilesAsync(
        string[] markedSources,
        params (string Scope, string Kind, int Count)[] details)
    {
        var trees = new List<SyntaxTree>();
        var expected = new List<(int File, TextSpan Span, string Name)>();
        for (int file = 0; file < markedSources.Length; file++)
        {
            string markedSource = markedSources[file];
            var source = new StringBuilder();
            int previousEnd = 0;
            foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
            {
                source.Append(markedSource, previousEnd, match.Index - previousEnd);
                string name = match.Groups[1].Value;
                expected.Add((file, new TextSpan(source.Length, name.Length), name));
                source.Append(name);
                previousEnd = match.Index + match.Length;
            }
            source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
            trees.Add(CSharpSyntaxTree.ParseText(source.ToString(),
                new CSharpParseOptions(LanguageVersion.Preview), path: $"Input{file}.cs"));
        }
        Assert.Equal(expected.Count, details.Length);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise05TestAssembly",
            syntaxTrees: trees,
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson08NamespaceAndUsingDeclarations.Exercise05.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include unexpected analyzer diagnostics and AD0001 analyzer crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics
            .OrderBy(d => d.Location.SourceTree is null ? -1 : trees.IndexOf(d.Location.SourceTree))
            .ThenBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("DUPIMPORT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Scope '{details[i].Scope}' contains {details[i].Count} occurrences of " +
                $"{details[i].Kind} import '{expected[i].Name}'",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(trees[expected[i].File], diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }
}