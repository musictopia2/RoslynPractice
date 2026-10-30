using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson07ParametersTypeParametersConstraints;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise05_ClientExample_ReportOnFirstIncludedMethodEvenWhenClean()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void [|Run|]<T>() where T : class { }
                public void Run<T>(int count = 5) { }
                public void Run<T>(ref int value) { }
            }
            """, ("Sample", 1, 3, 2, 2, 1, 1));
    }

    [Fact]
    public async Task Exercise05_TooFewProblematicMethods_ReportNothing()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void Single<T>(int count = 5) { }
                public void Mixed<T>(int count = 5) { }
                public void Mixed<T>() where T : class { }
                public void Neither<T>() { }
                public void Neither<T>(int count) { }
            }
            """);
    }

    [Theory]
    [InlineData("ref", "")]
    [InlineData("out", "value = 0;")]
    [InlineData("in", "")]
    [InlineData("ref readonly", "")]
    [InlineData("scoped ref", "")]
    public async Task Exercise05_ReferenceModifierForms_MakeMethodProblematic(
        string modifier, string body)
    {
        await Exercise05_VerifyAsync(
            "class Sample { public void [|Run|]<T>(int count = 5) { } " +
            "public void Run<T>(" + modifier + " int value) { " + body + " } }",
            ("Sample", 1, 2, 2, 2, 1, 1));
    }

    [Fact]
    public async Task Exercise05_DefaultsAloneOrReferencesAlone_CanQualifyGroup()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void [|Defaults|]<T>(int count = 5) { }
                public void Defaults<T>(bool enabled = false) { }
                public void [|References|]<T>(ref int value) { }
                public void References<T>(in bool value) { }
            }
            """, ("Sample", 1, 2, 2, 2, 2, 0), ("Sample", 1, 2, 2, 2, 0, 2));
    }

    [Fact]
    public async Task Exercise05_SummaryCounts_OnlyProblematicMethodsContributeDetailTotals()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void [|Run|]<T, U>(int count = 5, bool enabled = false)
                    where U : class { }
                public void Run<T, U>(ref int value, out bool success, string name = null)
                {
                    success = true;
                }
                public void Run<T, U>(in int limit, int count = 5,
                    bool enabled = false, string name = null)
                    where T : class where U : class { }
                public void Run<T, U>() { }
            }
            """, ("Sample", 2, 4, 2, 3, 3, 2));
    }

    [Fact]
    public async Task Exercise05_Grouping_UsesBothNameAndTypeParameterCount()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void Run<T>(int count = 5) { }
                public void Run<T, U>(int count = 5) { }
                public void Other<T>(bool enabled = false) { }
                public void Other<T, U>(bool enabled = false) { }
                public void [|Pair|]<T, U>(int count = 5) { }
                public void Pair<T, U>(string name = null) { }
            }
            """, ("Sample", 2, 2, 2, 4, 2, 0));
    }

    [Fact]
    public async Task Exercise05_GroupNames_AreCaseSensitiveAndUseValueText()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void Run<T>(int count = 5) { }
                public void run<T>(bool enabled = false) { }
                public void [|@Work|]<T>(int count = 5) { }
                public void Work<T>(bool enabled = false) { }
            }
            """, ("Sample", 1, 2, 2, 2, 2, 0));
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("")]
    public async Task Exercise05_NonPublicMethod_DoesNotCompleteGroup(string modifier)
    {
        await Exercise05_VerifyAsync(
            "class Sample { public void Run<T>(int count = 5) { } " +
            modifier + " void Run<T>(bool enabled = false) { } }");
    }

    [Fact]
    public async Task Exercise05_FirstIncludedMethod_SkipsEarlierExcludedMethods()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                private void Run<T>(string name = null) { }
                public void Run(int count = 5) { }
                public void [|Run|]<T>(int count = 5) { }
                public void Run<T>(bool enabled = false) { }
            }
            """, ("Sample", 1, 2, 2, 2, 2, 0));
    }

    [Fact]
    public async Task Exercise05_NonGenericMethods_DoNotCompleteGroup()
    {
        await Exercise05_VerifyAsync("""
            class Sample<TOuter>
            {
                public void Run(int count = 5) { }
                public void Run(bool enabled = false) { }
                public void Run<T>(string name = null) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_MethodBodiesStaticAndClassAccessibility_DoNotExcludeMethods()
    {
        await Exercise05_VerifyAsync("""
            internal abstract class Sample
            {
                public abstract int [|Read|]<T>(int count = 5);
                public static string Read<T>(string name = null) => name;
            }
            """, ("Sample", 1, 2, 2, 2, 2, 0));
    }

    [Theory]
    [InlineData("struct")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("record struct")]
    public async Task Exercise05_NonClassDeclarations_AreExcluded(string kind)
    {
        await Exercise05_VerifyAsync(
            kind + " Sample { public void Run<T>(int count = 5) { } " +
            "public void Run<T>(bool enabled = false) { } }");
    }

    [Fact]
    public async Task Exercise05_NestedClassInsideExcludedType_IsStillAnalyzed()
    {
        await Exercise05_VerifyAsync("""
            struct Container
            {
                private class Inner
                {
                    public void [|Run|]<T>(int count = 5) { }
                    public void Run<T>(bool enabled = false) { }
                }
            }
            """, ("Inner", 1, 2, 2, 2, 2, 0));
    }

    [Fact]
    public async Task Exercise05_MethodsInDifferentClasses_DoNotCombine()
    {
        await Exercise05_VerifyAsync("""
            class Outer
            {
                public void Run<T>(int count = 5) { }
                class Inner
                {
                    public void Run<T>(bool enabled = false) { }
                }
            }
            class Other
            {
                public void Run<T>(string name = null) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_NestedClasses_ReportIndependentGroups()
    {
        await Exercise05_VerifyAsync("""
            class Outer
            {
                public void [|Run|]<T>(int count = 5) { }
                public void Run<T>(bool enabled = false) { }
                class Inner
                {
                    public void [|Run|]<T>(ref int value) { }
                    public void Run<T>(in bool value) { }
                }
            }
            """, ("Outer", 1, 2, 2, 2, 2, 0), ("Inner", 1, 2, 2, 2, 0, 2));
    }

    [Fact]
    public async Task Exercise05_PartialDeclarations_DoNotCombineMethods()
    {
        await Exercise05_VerifyAsync("""
            partial class Sample
            {
                public void Run<T>(int count = 5) { }
            }
            partial class Sample
            {
                public void Run<T>(bool enabled = false) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_PartialDeclarations_WithOwnGroupsReportSeparately()
    {
        await Exercise05_VerifyAsync("""
            partial class Sample
            {
                public void [|Run|]<T>(int count = 5) { }
                public void Run<T>(bool enabled = false) { }
            }
            partial class Sample
            {
                public void [|Run|]<T>(ref string value) { }
                public void Run<T>(in long value) { }
            }
            """, ("Sample", 1, 2, 2, 2, 2, 0), ("Sample", 1, 2, 2, 2, 0, 2));
    }

    [Fact]
    public async Task Exercise05_LocalDefaultsAndReferenceParameters_DoNotMakeHostProblematic()
    {
        await Exercise05_VerifyAsync("""
            delegate void Callback(ref int value);
            class Sample
            {
                public void Run<T>()
                {
                    void Local<U>(int count = 5) { }
                    Callback lambda = (ref int value) => { };
                    Callback anonymous = delegate(ref int value) { };
                }
                public void Run<T>(bool enabled = false) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_ContainingAndLocalConstraints_DoNotConstrainMethodParameters()
    {
        await Exercise05_VerifyAsync("""
            class Sample<T> where T : class
            {
                public void [|Run|]<T>(int count = 5)
                {
                    void Local<T>() where T : class { }
                }
                public void Run<T>(bool enabled = false) { }
            }
            """, ("Sample", 1, 2, 2, 2, 2, 0));
    }

    [Fact]
    public async Task Exercise05_ConstraintNames_AreCaseSensitiveAndMatchClauseOwner()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void [|Run|]<T, t>(int count = 5) where T : class { }
                public void Run<T, t>(bool enabled = false) where t : struct { }
                public void [|Convert|]<T, U>(int count = 5) where T : U { }
                public void Convert<T, U>(bool enabled = false) where T : class { }
            }
            """, ("Sample", 2, 2, 2, 2, 2, 0), ("Sample", 2, 2, 2, 2, 2, 0));
    }

    [Theory]
    [InlineData("class")]
    [InlineData("struct")]
    [InlineData("new()")]
    [InlineData("unmanaged")]
    [InlineData("notnull")]
    [InlineData("System.IDisposable")]
    public async Task Exercise05_AnyMatchingClause_PreventsFullyConstrainedMethodQualifying(
        string constraint)
    {
        await Exercise05_VerifyAsync(
            "class Sample { public void Run<T>(int count = 5) where T : " + constraint +
            " { } public void Run<T>(bool enabled = false) { } }");
    }

    [Fact]
    public async Task Exercise05_ParamsAttributesAndReferenceReturns_DoNotCountAsParameterFeatures()
    {
        await Exercise05_VerifyAsync("""
            using System.Runtime.InteropServices;
            class Sample
            {
                public void Run<T>(params int[] values) { }
                public void Run<T>([Optional, DefaultParameterValue(5)] int count) { }
                public ref int Run<T>(int[] values, bool enabled) => ref values[0];
                public void Run<T>(string name = null) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_DefaultAndReferenceCategories_AreCountedIndependently()
    {
        await Exercise05_VerifyAsync("""
            class Sample
            {
                public void [|Run|]<T>(in int limit = 5) { }
                public void Run<T>(ref readonly long limit, bool enabled = false) { }
            }
            """, ("Sample", 1, 2, 2, 2, 2, 2));
    }

    [Fact]
    public async Task Exercise05_EscapedClassAndMethodNames_UseValueTextAndExactMethodLocation()
    {
        await Exercise05_VerifyAsync("""
            class @class<TOuter>
            {
                public void /* before */ [|@event|] /* after */ <T, @in>(int count = 5)
                    where @T : class { }
                public void @event<T, @in>(ref int value) where T : struct { }
            }
            """, ("class", 2, 2, 2, 2, 1, 1));
    }

    [Fact]
    public async Task Exercise05_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise05.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("GENERICGROUP001", rule.Id);
        Assert.Equal("Generic overload group needs signature review", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Class '{0}' method group '{1}' with {2} type parameters has {3} problematic methods out of {4}: {5} unconstrained type parameters, {6} default-valued parameters, {7} reference parameters",
            rule.MessageFormat.ToString());
    }

    // Mark only the FIRST INCLUDED METHOD identifier for each qualifying group.
    // All expected summary values are supplied explicitly, in marker order.
    private static async Task Exercise05_VerifyAsync(
        string markedSource,
        params (string ClassName, int Arity, int Methods, int Problematic,
            int Unconstrained, int Defaults, int References)[] summaries)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
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
        Assert.Equal(expected.Count, summaries.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise05TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise05.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        string[] messages = summaries.Select((summary, i) =>
            $"Class '{summary.ClassName}' method group '{expected[i].Name}' with {summary.Arity} type parameters has {summary.Problematic} problematic methods out of {summary.Methods}: {summary.Unconstrained} unconstrained type parameters, {summary.Defaults} default-valued parameters, {summary.References} reference parameters")
            .ToArray();
        string context = Environment.NewLine + "Source under test:" + Environment.NewLine + source +
            Environment.NewLine + "Expected analyzer messages:" + Environment.NewLine +
            (messages.Length == 0 ? "(none)" : string.Join(Environment.NewLine, messages)) +
            Environment.NewLine + "Actual analyzer diagnostics:" + Environment.NewLine +
            (diagnostics.Length == 0 ? "(none)" : string.Join(Environment.NewLine, diagnostics));

        // All analyzer output is checked; AD0001 crashes cannot pass silently.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} group diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + "If diagnostics are missing, check class/method inclusion, " +
            "group name and generic arity, the two-problematic-method threshold, and that " +
            "the created diagnostic reaches context.ReportDiagnostic." +
            Environment.NewLine + "If there are extras, check declaration boundaries and " +
            "that each qualifying group reports exactly once." + context);

        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.True(diagnostic.Id == "GENERICGROUP001",
                "Unexpected diagnostic ID; analyzer exceptions must also fail." + context);
            Assert.True(diagnostic.Severity == DiagnosticSeverity.Warning,
                "Expected Warning severity." + context);
            Assert.True(diagnostic.GetMessage() == messages[i],
                "Group summary mismatch. MethodCount includes every included method; " +
                "the three detail totals include problematic methods only. " +
                "Check each parameter category independently and count ref readonly once." + context);
            Assert.True(diagnostic.Location.IsInSource &&
                diagnostic.Location.SourceTree == tree &&
                diagnostic.Location.SourceSpan == expected[i].Span,
                "Location mismatch. Highlight only the first included method's identifier " +
                "in this group, even when that method is clean. " +
                $"Expected span {expected[i].Span}; actual span {diagnostic.Location.SourceSpan}." + context);
        }
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, false)]
    public async Task Exercise04_AllThreeConditions_AreRequired(
        bool isPublic, bool isUnconstrained, bool hasDefault, bool shouldReport)
    {
        string source = "class Sample { " + (isPublic ? "public " : "private ") +
            "void " + (shouldReport ? "[|Run|]" : "Run") + "<T>(int count" +
            (hasDefault ? " = 5" : "") + ") " +
            (isUnconstrained ? "" : "where T : class ") + "{ } }";
        if (shouldReport)
        {
            await Exercise04_VerifyAsync(source, ("T", "count"));
        }
        else
        {
            await Exercise04_VerifyAsync(source);
        }
    }

    [Fact]
    public async Task Exercise04_Example_ReportsOneSummaryOnMethodIdentifier()
    {
        await Exercise04_VerifyAsync("""
            class Sample
            {
                public void [|Convert|]<TInput, TOutput, TContext>(
                    int required, int count = 5, bool enabled = false)
                    where TInput : class
                {
                }
            }
            """, ("TOutput, TContext", "count, enabled"));
    }

    [Fact]
    public async Task Exercise04_ListNames_IncludeOnlyQualifyingNamesInDeclarationOrder()
    {
        await Exercise04_VerifyAsync("""
            class Sample
            {
                public void [|Run|]<TZ, TConstrained, TA, TM>(
                    int required, int zebra = 0, bool alpha = false, string middle = null)
                    where TConstrained : class { }
            }
            """, ("TZ, TA, TM", "zebra, alpha, middle"));
    }

    [Theory]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("")]
    public async Task Exercise04_MethodWithoutExplicitPublic_IsExcluded(string modifier)
    {
        await Exercise04_VerifyAsync(
            "class Sample { " + modifier + " void Run<T>(int count = 5) { } }");
    }

    [Fact]
    public async Task Exercise04_InterfaceMethods_RequireExplicitPublicModifier()
    {
        await Exercise04_VerifyAsync("""
            interface IContract
            {
                void Implicit<T>(int count = 5);
                public void [|Explicit|]<T>(int count = 5);
                public void AllConstrained<T>(int count = 5) where T : class;
            }
            """, ("T", "count"));
    }

    [Fact]
    public async Task Exercise04_StaticAbstractAndExpressionBodiedMethods_AreIncluded()
    {
        await Exercise04_VerifyAsync("""
            abstract class Sample
            {
                public static void [|Static|]<T>(int count = 5) { }
                public abstract int [|NoBody|]<T>(bool enabled = false);
                public string [|Expression|]<T>(string name = null) => name;
            }
            """, ("T", "count"), ("T", "enabled"), ("T", "name"));
    }

    [Fact]
    public async Task Exercise04_ContainingTypeAccessibility_DoesNotExcludePublicMethods()
    {
        await Exercise04_VerifyAsync("""
            internal class Outer
            {
                public void [|OuterMethod|]<T>(int count = 5) { }
                private class Inner
                {
                    public void [|InnerMethod|]<U>(bool enabled = false) { }
                }
            }
            """, ("T", "count"), ("U", "enabled"));
    }

    [Fact]
    public async Task Exercise04_NonGenericMethodsAndContainingTypeParameters_DoNotQualify()
    {
        await Exercise04_VerifyAsync("""
            class Sample<TOuter>
            {
                public void NonGeneric(int count = 5) { }
                public void Constrained<T>(int count = 5) where T : class { }
            }
            """);
    }

    [Fact]
    public async Task Exercise04_ContainingTypeConstraint_DoesNotConstrainMethodParameter()
    {
        await Exercise04_VerifyAsync("""
            class Sample<T> where T : class
            {
                public void [|Run|]<T>(int count = 5) { }
            }
            """, ("T", "count"));
    }

    [Fact]
    public async Task Exercise04_ConstraintMatching_UsesCaseSensitiveNamesAndClauseOwner()
    {
        await Exercise04_VerifyAsync("""
            class Sample
            {
                public void [|First|]<T, t>(int count = 5) where T : class { }
                public void [|Second|]<T, t>(bool enabled = false) where t : struct { }
                public void [|Third|]<TInput, TOutput>(string name = null)
                    where TInput : TOutput { }
            }
            """, ("t", "count"), ("T", "enabled"), ("TOutput", "name"));
    }

    [Theory]
    [InlineData("class")]
    [InlineData("class?")]
    [InlineData("struct")]
    [InlineData("new()")]
    [InlineData("unmanaged")]
    [InlineData("notnull")]
    [InlineData("System.IDisposable")]
    [InlineData("class, System.IDisposable, new()")]
    public async Task Exercise04_AnyMatchingConstraintClause_ExcludesThatTypeParameter(
        string constraint)
    {
        await Exercise04_VerifyAsync(
            "class Sample { public void [|Run|]<T, U>(int count = 5) where T : " +
            constraint + " { } }", ("U", "count"));
    }

    [Theory]
    [InlineData("5")]
    [InlineData("0")]
    [InlineData("default")]
    [InlineData("default(int)")]
    [InlineData("1 + 2")]
    public async Task Exercise04_DefaultExpressionValue_DoesNotAffectQualification(
        string expression)
    {
        await Exercise04_VerifyAsync(
            "class Sample { public void [|Run|]<T>(int count = " + expression + ") { } }",
            ("T", "count"));
    }

    [Fact]
    public async Task Exercise04_AttributesWithoutEqualsClause_DoNotCountAsDefaults()
    {
        await Exercise04_VerifyAsync("""
            using System.Runtime.InteropServices;
            class Sample
            {
                public void AttributesOnly<T>([Optional, DefaultParameterValue(5)] int count) { }
                public void [|Mixed|]<T>([Optional] int ignored, int count = 5) { }
            }
            """, ("T", "count"));
    }

    [Fact]
    public async Task Exercise04_NestedFunctions_DoNotSupplyMethodDefaultsOrConstraints()
    {
        await Exercise04_VerifyAsync("""
            class Sample
            {
                public void NoOwnDefault<T>()
                {
                    void Local(int count = 5) { }
                    var lambda = (int value = 10) => value;
                }
                public void [|Host|]<T>(int outer = 5)
                {
                    void Local<T>(int inner = 10) where T : class { }
                }
                public void FullyConstrained<T>(int count = 5) where T : class
                {
                    void Local<U>(int value = 10) { }
                }
            }
            """, ("T", "outer"));
    }

    [Fact]
    public async Task Exercise04_NonMethodDeclarations_AreExcluded()
    {
        await Exercise04_VerifyAsync("""
            public delegate void Callback<T>(int count = 5);
            public class Sample<T>
            {
                public Sample(int count = 5) { }
                public int this[int index = 0] => index;
                void Host()
                {
                    void Local<U>(int value = 5) { }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise04_MultipleMethods_ReportSeparateSummaries()
    {
        await Exercise04_VerifyAsync("""
            class Sample
            {
                public void [|First|]<T, U>(int count = 5) where U : class { }
                public void Safe<T>(int count = 5) where T : struct { }
                public void [|Second|]<V>(bool enabled = false, string name = null) { }
            }
            """, ("T", "count"), ("V", "enabled, name"));
    }

    [Fact]
    public async Task Exercise04_EscapedNamesAndTrivia_VerifyMethodLocationAndValueText()
    {
        await Exercise04_VerifyAsync("""
            class Sample
            {
                public void /* before */ [|@class|] /* after */ <@event, @in, T>(
                    int @ref = 5, bool @out = false)
                    where @T : struct { }
            }
            """, ("event, in", "ref, out"));
    }

    [Fact]
    public async Task Exercise04_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise04.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("GENERICDEFAULT001", rule.Id);
        Assert.Equal("Public generic method combines defaults with missing constraints",
            rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Public method '{0}' has unconstrained type parameters [{1}] and default-valued parameters [{2}]",
            rule.MessageFormat.ToString());
    }

    // Mark expected METHOD identifiers with [|...|].
    // Supply both expected name lists explicitly, in marker order.
    private static async Task Exercise04_VerifyAsync(
        string markedSource,
        params (string UnconstrainedNames, string DefaultNames)[] details)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
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
        Assert.Equal(expected.Count, details.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise04TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise04.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check every analyzer diagnostic, including unexpected output and crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("GENERICDEFAULT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Public method '{expected[i].Name}' has unconstrained type parameters [{details[i].UnconstrainedNames}] and default-valued parameters [{details[i].DefaultNames}]",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Fact]
    public async Task Exercise03_NoConstraints_ReportEveryMethodTypeParameter()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Convert<[|TInput|], [|TOutput|], [|TContext|]>() { }
            }
            """, "Convert", "Convert", "Convert");
    }

    [Fact]
    public async Task Exercise03_PartialConstraints_ReportOnlyMissingClause()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Convert<TInput, [|TOutput|], TContext>()
                    where TInput : class
                    where TContext : new()
                {
                }
            }
            """, "Convert");
    }

    [Theory]
    [InlineData("class")]
    [InlineData("class?")]
    [InlineData("struct")]
    [InlineData("new()")]
    [InlineData("unmanaged")]
    [InlineData("notnull")]
    [InlineData("System.IDisposable")]
    [InlineData("class, System.IDisposable, new()")]
    public async Task Exercise03_AnyConstraintKind_CountsAsMatchingClause(string constraint)
    {
        await Exercise03_VerifyAsync(
            "class Sample { void Run<T>() where T : " + constraint + " { } }");
    }

    [Fact]
    public async Task Exercise03_AllTypeParametersConstrained_ReportNothing()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Convert<TInput, TOutput, TContext>()
                    where TInput : class
                    where TOutput : struct
                    where TContext : new()
                {
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise03_TypeParameterUsedInsideAnotherConstraint_StillNeedsOwnClause()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Convert<TInput, [|TOutput|]>() where TInput : TOutput { }
            }
            """, "Convert");
    }

    [Fact]
    public async Task Exercise03_NameMatching_IsCaseSensitive()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void First<T, [|t|]>() where T : class { }
                void Second<[|T|], t>() where t : struct { }
            }
            """, "First", "Second");
    }

    [Fact]
    public async Task Exercise03_ClauseCountDoesNotReplaceMatchingByName()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void First<[|TFirst|], TSecond, [|TThird|]>()
                    where TSecond : class { }
                void Second<TFirst, [|TSecond|], TThird>()
                    where TFirst : class
                    where TThird : struct { }
            }
            """, "First", "First", "Second");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("internal")]
    [InlineData("protected")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("public static")]
    [InlineData("public virtual")]
    public async Task Exercise03_MethodModifiers_DoNotExcludeTypeParameters(string modifiers)
    {
        await Exercise03_VerifyAsync(
            "class Sample { " + modifiers + " void Run<[|T|]>() { } }", "Run");
    }

    [Fact]
    public async Task Exercise03_MethodBodiesReturnTypesAndInterfaceMethods_AreIncluded()
    {
        await Exercise03_VerifyAsync("""
            abstract class Sample
            {
                int Block<[|T|]>() { return 0; }
                string Expression<[|T|]>() => "hello";
                public abstract void NoBody<[|T|]>();
            }
            interface IContract
            {
                void Read<[|T|]>();
                void Constrained<T>() where T : class;
            }
            """, "Block", "Expression", "NoBody", "Read");
    }

    [Fact]
    public async Task Exercise03_ContainingTypeConstraints_DoNotConstrainMethodParameters()
    {
        await Exercise03_VerifyAsync("""
            class Outer<T> where T : class
            {
                void Run<[|T|], [|U|]>() { }
                void Constrained<T>() where T : struct { }
                class Inner<V> where V : new()
                {
                    void Other<[|V|]>() { }
                }
            }
            """, "Run", "Run", "Other");
    }

    [Fact]
    public async Task Exercise03_SeparateMethods_DoNotShareConstraintClauses()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Constrained<T>() where T : class { }
                void Unconstrained<[|T|]>() { }
                void Mixed<T, [|U|]>() where T : struct { }
                void Another<[|T|], U>() where U : class { }
            }
            """, "Unconstrained", "Mixed", "Another");
    }

    [Fact]
    public async Task Exercise03_NestedLocalFunctions_DoNotSupplyOrReceiveMethodConstraints()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Host<[|T|]>()
                {
                    void Local<T>() where T : class { }
                    void Other<U>() { }
                }
                void Constrained<T>() where T : class
                {
                    void Local<U>() { }
                }
            }
            """, "Host");
    }

    [Fact]
    public async Task Exercise03_NonMethodTypeParametersAndNonGenericMethods_ReportNothing()
    {
        await Exercise03_VerifyAsync("""
            delegate void Callback<T>();
            interface IContract<T>
            {
                void Run();
            }
            struct Container<T> { }
            record RecordContainer<T>;
            record struct ValueContainer<T>;
            class Sample<T>
            {
                void Run(int count) { }
                void Host()
                {
                    void Local<TLocal>() { }
                }
                class Inner<TInner> { }
            }
            """);
    }

    [Fact]
    public async Task Exercise03_EscapedIdentifiers_CompareValueTextAndHighlightWholeToken()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void @class<@event, [|@in|], T>()
                    where @event : class
                    where @T : struct { }
                void Other<@U>() where U : new() { }
            }
            """, "class");
    }

    [Fact]
    public async Task Exercise03_Trivia_HighlightOnlyUnconstrainedDeclarationIdentifier()
    {
        await Exercise03_VerifyAsync("""
            class Sample
            {
                void Convert<
                    /* before */ [|TInput|] /* after */,
                    TOutput,
                    [|TContext|]
                >()
                    where /* clause */ TOutput : class
                {
                }
            }
            """, "Convert", "Convert");
    }

    [Fact]
    public async Task Exercise03_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise03.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("TYPEPARAM001", rule.Id);
        Assert.Equal("Method type parameter has no constraint clause", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Type parameter '{0}' in method '{1}' has no constraint clause",
            rule.MessageFormat.ToString());
    }

    // Mark expected type parameter declaration identifiers with [|...|].
    // Expected method names are explicit, in marker order.
    private static async Task Exercise03_VerifyAsync(
        string markedSource, params string[] methodNames)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
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
        Assert.Equal(expected.Count, methodNames.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise03TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise03.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check all analyzer output, including unexpected diagnostics and crashes.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("TYPEPARAM001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Type parameter '{expected[i].Name}' in method '{methodNames[i]}' has no constraint clause",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Fact]
    public async Task Exercise02_MixedModifiers_ReportOnlyReferenceParameters()
    {
        await Exercise02_VerifyAsync("""
            class Sample
            {
                void Update(ref int [|count|], out bool [|success|], in int [|limit|], int other)
                {
                    success = true;
                }
            }
            """, ("Update", "ref"), ("Update", "out"), ("Update", "in"));
    }

    [Theory]
    [InlineData("ref", "ref")]
    [InlineData("out", "out")]
    [InlineData("in", "in")]
    [InlineData("ref readonly", "ref")]
    public async Task Exercise02_ModifierForms_ReportOnceWithQualifyingKeyword(
        string modifiers, string expectedModifier)
    {
        await Exercise02_VerifyAsync(
            "interface IContract { void Run(" + modifiers + " int [|value|]); }",
            ("Run", expectedModifier));
    }

    [Fact]
    public async Task Exercise02_RefReadonlyAndScoped_ReportOncePerParameter()
    {
        await Exercise02_VerifyAsync("""
            interface IContract
            {
                void Read(ref readonly int [|first|], scoped ref int [|second|],
                    scoped in int [|third|], scoped ref readonly int [|fourth|]);
            }
            """, ("Read", "ref"), ("Read", "ref"), ("Read", "in"), ("Read", "ref"));
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("internal")]
    [InlineData("protected")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("public static")]
    [InlineData("public virtual")]
    public async Task Exercise02_MethodModifiers_DoNotExcludeParameters(string modifiers)
    {
        await Exercise02_VerifyAsync(
            "class Sample { " + modifiers + " void Run(ref int [|value|]) { } }",
            ("Run", "ref"));
    }

    [Fact]
    public async Task Exercise02_MethodBodiesAndReturnTypes_DoNotExcludeParameters()
    {
        await Exercise02_VerifyAsync("""
            abstract class Sample
            {
                int Block(ref int [|value|]) { return value; }
                int Expression(in int [|value|]) => value;
                public abstract void NoBody(out bool [|success|]);
            }
            interface IContract
            {
                void Read(in int [|value|]);
            }
            """, ("Block", "ref"), ("Expression", "in"), ("NoBody", "out"), ("Read", "in"));
    }

    [Fact]
    public async Task Exercise02_GenericMethodsAndNestedTypes_UseOwningMethodIdentifier()
    {
        await Exercise02_VerifyAsync("""
            class Outer<T>
            {
                void Convert<TInput, TOutput>(ref TInput [|input|], out TOutput [|output|])
                {
                    output = default(TOutput);
                }
                class Inner
                {
                    void Read(in int [|value|]) { }
                }
            }
            """, ("Convert", "ref"), ("Convert", "out"), ("Read", "in"));
    }

    [Fact]
    public async Task Exercise02_EscapedIdentifiers_UseValueTextAndHighlightEntireToken()
    {
        await Exercise02_VerifyAsync("""
            class Sample
            {
                void @class(ref int [|@event|], in int [|@in|]) { }
            }
            """, ("class", "ref"), ("class", "in"));
    }

    [Fact]
    public async Task Exercise02_AttributesDefaultsAndTrivia_DoNotChangeQualificationOrLocation()
    {
        await Exercise02_VerifyAsync("""
            using System;
            [AttributeUsage(AttributeTargets.Parameter)]
            sealed class MarkerAttribute : Attribute { }
            class Sample
            {
                void Read(
                    [Marker] in int /* before */ [|limit|] /* after */ = 5,
                    [Marker] int ordinary = 10) { }
                void Write([Marker] ref int [|value|]) { }
            }
            """, ("Read", "in"), ("Write", "ref"));
    }

    [Fact]
    public async Task Exercise02_ValueParametersDefaultsAndParams_ReportNothing()
    {
        await Exercise02_VerifyAsync("""
            class Sample
            {
                void Empty() { }
                void Ordinary(int count, string name) { }
                void Defaults(int count = 5, bool enabled = false) { }
                void Many(params int[] values) { }
                void MisleadingNames(int @ref, int @out, int @in) { }
                ref int ReturnByReference(int[] values) => ref values[0];
            }
            """);
    }

    [Fact]
    public async Task Exercise02_NonMethodParameterOwners_ReportNothing()
    {
        await Exercise02_VerifyAsync("""
            delegate void Callback(ref int value);
            class Sample
            {
                public Sample(ref int value) { }
                public int this[in int index] => index;
                void Host()
                {
                    void Local(ref int value) { }
                    Callback lambda = (ref int value) => { };
                    Callback anonymous = delegate(ref int value) { };
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_NestedFunctions_OnlyReportDirectMethodParameters()
    {
        await Exercise02_VerifyAsync("""
            delegate void Callback(ref int value);
            class Sample
            {
                void Host(ref int [|outer|])
                {
                    void Local(in int inner)
                    {
                        void Deeper(out int result) { result = 0; }
                    }
                    Callback lambda = (ref int other) => { };
                    Callback anonymous = delegate(ref int another) { };
                }
            }
            """, ("Host", "ref"));
    }

    [Fact]
    public async Task Exercise02_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise02.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("PARAMREF001", rule.Id);
        Assert.Equal("Parameter is passed by reference", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Parameter '{0}' in method '{1}' uses the '{2}' modifier",
            rule.MessageFormat.ToString());
    }

    // Expected method and modifier names are explicit, in marker order.
    private static async Task Exercise02_VerifyAsync(
        string markedSource, params (string Method, string Modifier)[] details)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
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
        Assert.Equal(expected.Count, details.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise02TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise02.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Unexpected diagnostics and analyzer crashes must fail.
        // Compiler diagnostics are outside this syntax-only exercise contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("PARAMREF001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Parameter '{expected[i].Name}' in method '{details[i].Method}' uses the '{details[i].Modifier}' modifier",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }

    [Fact]
    public async Task Exercise01_MultipleDefaults_ReportEachParameterIdentifier()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                void Configure(int required, int [|count|] = 5,
                    bool [|enabled|] = false, string [|name|] = null) { }
            }
            """, "Configure", "Configure", "Configure");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("5")]
    [InlineData("-1")]
    [InlineData("default")]
    [InlineData("default(int)")]
    [InlineData("1 + 2")]
    public async Task Exercise01_DefaultExpressionForms_ReportParameter(string expression)
    {
        await Exercise01_VerifyAsync(
            "class Sample { void Run(int [|count|] = " + expression + ") { } }", "Run");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("internal")]
    [InlineData("protected")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    [InlineData("public static")]
    [InlineData("public virtual")]
    public async Task Exercise01_MethodModifiers_DoNotExcludeParameters(string modifiers)
    {
        await Exercise01_VerifyAsync(
            "class Sample { " + modifiers + " void Run(int [|count|] = 5) { } }", "Run");
    }

    [Fact]
    public async Task Exercise01_MethodBodiesAndReturnTypes_DoNotExcludeParameters()
    {
        await Exercise01_VerifyAsync("""
            abstract class Sample
            {
                int Block(int [|count|] = 5) { return count; }
                string Expression(string [|name|] = "hello") => name;
                public abstract void NoBody(bool [|enabled|] = true);
            }
            interface IContract
            {
                int Read(int [|count|] = 10);
            }
            """, "Block", "Expression", "NoBody", "Read");
    }

    [Fact]
    public async Task Exercise01_GenericMethods_UseMethodNameWithoutTypeParameters()
    {
        await Exercise01_VerifyAsync("""
            class Container<T>
            {
                void Convert<TInput, TOutput>(int [|count|] = 5) { }
            }
            interface IContract
            {
                void Process<T>(bool [|enabled|] = false) where T : class;
            }
            """, "Convert", "Process");
    }

    [Fact]
    public async Task Exercise01_OverloadsAndNestedTypes_UseEachOwningMethodName()
    {
        await Exercise01_VerifyAsync("""
            class Outer
            {
                void Run(int [|count|] = 5) { }
                void Run(string [|name|] = null) { }
                class Inner
                {
                    void Other(bool [|enabled|] = true) { }
                }
            }
            """, "Run", "Run", "Other");
    }

    [Fact]
    public async Task Exercise01_EscapedIdentifiers_UseValueTextAndHighlightEntireToken()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                void @class(int [|@event|] = 5) { }
            }
            """, "class");
    }

    [Fact]
    public async Task Exercise01_TriviaAndAttributes_HighlightOnlyParameterIdentifier()
    {
        await Exercise01_VerifyAsync("""
            using System;
            [AttributeUsage(AttributeTargets.Parameter)]
            sealed class MarkerAttribute : Attribute { }
            class Sample
            {
                void Configure(
                    [Marker] int /* before */ [|count|] /* after */ =
                        5,
                    string
                        [|name|]
                        = "hello") { }
            }
            """, "Configure", "Configure");
    }

    [Fact]
    public async Task Exercise01_ParametersWithoutDefaults_ReportNothing()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                void Empty() { }
                void Run(int count, string name, bool enabled) { }
                void Modify(ref int count, out int result) { result = count; }
                void Read(in int count) { }
                void Many(params int[] values) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise01_AttributesWithoutEqualsClause_ReportNothing()
    {
        await Exercise01_VerifyAsync("""
            using System.Runtime.InteropServices;
            class Sample
            {
                void Run([Optional] int count,
                    [Optional, DefaultParameterValue(5)] int other) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise01_NonMethodParameterOwners_ReportNothing()
    {
        await Exercise01_VerifyAsync("""
            delegate void Callback(int count = 5);
            class Sample
            {
                public Sample(int count = 5) { }
                public int this[int index = 0] => index;
                void Host()
                {
                    void Local(int count = 5) { }
                    var lambda = (int count = 5) => count;
                    System.Action<int> anonymous = delegate(int count) { };
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise01_NestedFunctions_OnlyReportDirectMethodParameters()
    {
        await Exercise01_VerifyAsync("""
            class Sample
            {
                void Host(int [|outer|] = 5)
                {
                    void Local(int inner = 10)
                    {
                        void Deeper(bool enabled = false) { }
                    }
                    var lambda = (int other = 20) => other;
                }
            }
            """, "Host");
    }

    [Fact]
    public async Task Exercise01_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise01.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("PARAMDEFAULT001", rule.Id);
        Assert.Equal("Parameter has an explicit default value", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Parameter '{0}' in method '{1}' has an explicit default value",
            rule.MessageFormat.ToString());
    }

    // Mark expected parameter identifiers with [|...|].
    // Supply expected owning method names explicitly in marker order.
    // No syntax walk is used to infer which parameters should qualify.
    private static async Task Exercise01_VerifyAsync(
        string markedSource, params string[] methodNames)
    {
        var source = new StringBuilder();
        var expected = new List<(TextSpan Span, string Name)>();
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
        Assert.Equal(expected.Count, methodNames.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise01TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson07ParametersTypeParametersConstraints.Exercise01.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Check every analyzer diagnostic, including AD0001 analyzer crashes.
        // Compiler diagnostics are intentionally outside this syntax-only contract.
        Assert.True(expected.Count == diagnostics.Length,
            $"Expected {expected.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));

        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < expected.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("PARAMDEFAULT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Parameter '{expected[i].Name}' in method '{methodNames[i]}' has an explicit default value",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(expected[i].Span, diagnostic.Location.SourceSpan);
        }
    }
}