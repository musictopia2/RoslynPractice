using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson10CommonDeclarationComponentsAndModifiers;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Theory]
    [InlineData("public", "private")]
    [InlineData("public", "protected")]
    [InlineData("public", "internal")]
    [InlineData("public", "protected internal")]
    [InlineData("public", "private protected")]
    [InlineData("private", "protected")]
    [InlineData("private", "internal")]
    [InlineData("private", "protected internal")]
    [InlineData("private", "private protected")]
    [InlineData("protected", "internal")]
    [InlineData("protected", "protected internal")]
    [InlineData("protected", "private protected")]
    [InlineData("internal", "protected internal")]
    [InlineData("internal", "private protected")]
    [InlineData("protected internal", "private protected")]
    public async Task Exercise05_EveryDifferentAccessibilityPair_Reports(
        string first, string second)
    {
        // Test both source orders without deriving expectations from analyzer logic.
        await Exercise05_VerifyAsync(
            "class Worker { " + first + " void [|Run|]() { } " +
            second + " void Run(int value) { } }",
            ("Worker", "Run", 0, 2, 0, 2, 2));
        await Exercise05_VerifyAsync(
            "class Worker { " + second + " void [|Run|]() { } " +
            first + " void Run(int value) { } }",
            ("Worker", "Run", 0, 2, 0, 2, 2));
    }

    [Theory]
    [InlineData("public", "public")]
    [InlineData("private", "private")]
    [InlineData("protected", "protected")]
    [InlineData("internal", "internal")]
    [InlineData("protected internal", "internal protected")]
    [InlineData("private protected", "protected private")]
    public async Task Exercise05_SameNormalizedAccessibilityAndSameStaticness_ReportsNothing(
        string first, string second)
    {
        await Exercise05_VerifyAsync(
            "class Worker { " + first + " void Run() { } " +
            second + " void Run(int value) { } }");
        await Exercise05_VerifyAsync(
            "class Worker { " + first + " static void Run() { } " +
            second + " static void Run(int value) { } }");
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public async Task Exercise05_StaticAndInstanceMix_ReportsWithOneAccessibility(string accessibility)
    {
        await Exercise05_VerifyAsync(
            "class Worker { " + accessibility + " static void [|Run|]() { } " +
            accessibility + " void Run(int value) { } }",
            ("Worker", "Run", 0, 2, 1, 1, 1));
    }

    [Fact]
    public async Task Exercise05_BothInconsistencies_ReportOnlyOnce()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                public static void [|Run|]() { }
                private void Run(int value) { }
            }
            """, ("Worker", "Run", 0, 2, 1, 1, 2));
    }

    [Fact]
    public async Task Exercise05_AllStaticWithDifferentAccessibility_Reports()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                public static void [|Run|]() { }
                private static void Run(int value) { }
            }
            """, ("Worker", "Run", 0, 2, 2, 0, 2));
    }

    [Fact]
    public async Task Exercise05_RepeatedAndAllSixAccessibilityValues_AreCountedOnceEach()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                public static void [|Run|]() { }
                private void Run(int a) { }
                protected void Run(string a) { }
                internal static void Run(bool a) { }
                protected internal void Run(double a) { }
                private protected void Run(decimal a) { }
                internal protected static void Run(long a) { }
                protected private void Run(char a) { }
                public void Run(float a) { }
            }
            """, ("Worker", "Run", 0, 9, 3, 6, 6));
    }

    [Fact]
    public async Task Exercise05_NoExplicitAccessibility_IsExcludedFromCountsAndLocation()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                void Run() { }
                public void [|Run|](int a) { }
                static void Run(string a) { }
                private static void Run(bool a) { }
                public void Run(double a) { }
            }
            """, ("Worker", "Run", 0, 3, 1, 2, 2));
    }

    [Fact]
    public async Task Exercise05_FewerThanTwoEligibleMethods_ReportsNothing()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                public static void One() { }
                void One(int a) { }
                private void Two() { }
                static void Two(int a) { }
                void Three() { }
                static void Three(int a) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_NameAndMethodTypeParameterCount_DefineSeparateGroups()
    {
        await Exercise05_VerifyAsync("""
            class Worker<TOuter>
            {
                public static void [|Run|]() { }
                public void Run(int a) { }

                public void [|Run|]<T>() { }
                private void Run<U>(string a, int b) { }

                public static void Run<T, U>() { }
                public static void Run<X, Y>(bool a) { }

                public void Run<T, U, V>() { }

                public void Other() { }
                private void Elsewhere() { }
                public static void run() { }
            }
            """, ("Worker", "Run", 0, 2, 1, 1, 1),
            ("Worker", "Run", 1, 2, 0, 2, 2));
    }

    [Fact]
    public async Task Exercise05_CaseSensitiveNames_DoNotCombine()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                public static void Run() { }
                private void run(int value) { }
                protected void RUN(string value) { }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_EscapedAndOrdinaryIdentifierSpellings_ShareValueText()
    {
        await Exercise05_VerifyAsync("""
            class @class
            {
                public static void [|@Run|]<T>() { }
                private void Run<U>(U value) { }
            }
            """, ("class", "Run", 1, 2, 1, 1, 2));
    }

    [Fact]
    public async Task Exercise05_InterleavedGroups_ReportAtFirstEligibleIdentifier()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                void Alpha() { }
                public void [|Beta|]() { }
                public static void [|Alpha|](int a) { }
                private void Beta(int a) { }
                public void Alpha(string a) { }
            }
            """, ("Worker", "Beta", 0, 2, 0, 2, 2),
            ("Worker", "Alpha", 0, 2, 1, 1, 1));
    }

    [Fact]
    public async Task Exercise05_NestedClasses_HaveIndependentGroups()
    {
        await Exercise05_VerifyAsync("""
            class Outer
            {
                public void [|Run|]() { }
                private void Run(int a) { }
                private class Inner
                {
                    public static void [|Run|]() { }
                    public void Run(int a) { }
                }
            }
            """, ("Outer", "Run", 0, 2, 0, 2, 2),
            ("Inner", "Run", 0, 2, 1, 1, 1));
    }

    [Fact]
    public async Task Exercise05_SeparateClassesPartialPartsAndInheritedMembers_DoNotCombine()
    {
        await Exercise05_VerifyAsync("""
            class First { public static void Run() { } }
            class Second { private void Run(int a) { } }
            partial class Split { public static void Run() { } }
            partial class Split { private void Run(int a) { } }
            class Base { public static void Run() { } }
            class Derived : Base { private void Run(int a) { } }
            class Outer
            {
                public static void Run() { }
                class Inner { private void Run(int a) { } }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_OtherModifiersAttributesBodiesAndConstraints_DoNotExcludeMethods()
    {
        await Exercise05_VerifyAsync("""
            [Unknown]
            internal abstract class Worker
            {
                [Unknown]
                public abstract T [|Run|]<T>() where T : class;
                [Unknown]
                private new static U Run<U>(U value) where U : struct => value;
            }
            """, ("Worker", "Run", 1, 2, 1, 1, 2));
    }

    [Fact]
    public async Task Exercise05_OtherMembersAndLocalFunctions_CannotCreateGroups()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                public static void Run() { }
                public int Run { get; }
                public Worker() { }
                private Worker(int a) { }
                public int this[int index] => index;
                public event System.Action Changed;
                public static Worker operator +(Worker a, Worker b) => a;
                public static implicit operator int(Worker value) => 0;
                void Container()
                {
                    void Run(int a) { }
                    static void Run(string a) { }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise05_OtherTypeKinds_AreNotTargetsButTheirNestedClassesAre()
    {
        await Exercise05_VerifyAsync("""
            struct Value
            {
                public static void Run() { }
                private void Run(int a) { }
                class InStruct
                {
                    public static void [|Run|]() { }
                    public void Run(int a) { }
                }
            }
            record Data
            {
                public static void Run() { }
                private void Run(int a) { }
                class InRecord
                {
                    internal void [|Run|]() { }
                    private void Run(int a) { }
                }
            }
            interface IWorker
            {
                public static void Run() { }
                private static void Run(int a) { }
            }
            """, ("InStruct", "Run", 0, 2, 1, 1, 1),
            ("InRecord", "Run", 0, 2, 0, 2, 2));
    }

    [Fact]
    public async Task Exercise05_Location_ExcludesAttributesReturnTypeAndTypeParameters()
    {
        await Exercise05_VerifyAsync("""
            class Worker
            {
                void Run<T>() { }
                [Unknown]
                public static T /* before */ [|Run|] /* after */ <T>(T value) => value;
                private U Run<U>(U value, int a) => value;
            }
            """, ("Worker", "Run", 1, 2, 1, 1, 2));
    }

    [Fact]
    public async Task Exercise05_EmptyFileCommentsAndStrings_ReportsNothing()
    {
        await Exercise05_VerifyAsync("");
        await Exercise05_VerifyAsync("""
            // public void Run() { }
            /* private static void Run(int a) { } */
            class Worker { string text = "public void Run() { }"; }
            """);
    }

    [Fact]
    public void Exercise05_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise05.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("OVERLOADMOD001", rule.Id);
        Assert.Equal("Inconsistent overload modifiers", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal(
            "Class '{0}' has inconsistent modifiers for method group '{1}' with {2} type parameter(s): {3} method(s), {4} static, {5} instance, {6} accessibility value(s)",
            rule.MessageFormat.ToString());
    }

    // Mark only the first eligible method identifier for each reportable group.
    // Supply all expected message data explicitly in marker/source order.
    private static async Task Exercise05_VerifyAsync(
        string markedSource,
        params (string ClassName, string Name, int TypeParameterCount, int MethodCount,
            int StaticCount, int InstanceCount, int AccessibilityCount)[] expectedData)
    {
        var source = new StringBuilder();
        var spans = new List<TextSpan>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string identifier = match.Groups[1].Value;
            spans.Add(new TextSpan(source.Length, identifier.Length));
            source.Append(identifier);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(spans.Count, expectedData.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise05TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise05.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include analyzer crashes (AD0001); compiler binding errors are outside scope.
        Assert.True(spans.Count == diagnostics.Length,
            $"Expected {spans.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < spans.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            var data = expectedData[i];
            Assert.Equal("OVERLOADMOD001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Class '{data.ClassName}' has inconsistent modifiers for method group '{data.Name}' with {data.TypeParameterCount} type parameter(s): {data.MethodCount} method(s), {data.StaticCount} static, {data.InstanceCount} instance, {data.AccessibilityCount} accessibility value(s)",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(spans[i], diagnostic.Location.SourceSpan);
        }
    }


    [Theory]
    [InlineData("public static void A() { } public void B() { }", 1, 1, 0, 0)]
    [InlineData("public static int A => 1; public int B => 2;", 0, 0, 1, 1)]
    [InlineData("public static void A() { } public int B => 2;", 1, 0, 0, 1)]
    [InlineData("public void A() { } public static int B => 2;", 0, 1, 1, 0)]
    public async Task Exercise04_StaticAndInstanceAcrossEitherMemberKind_Report(
        string members, int staticMethods, int instanceMethods, int staticProperties, int instanceProperties)
    {
        await Exercise04_VerifyAsync("class [|Worker|] { " + members + " }",
            ("Worker", staticMethods, instanceMethods, staticProperties, instanceProperties));
    }

    [Theory]
    [InlineData("")]
    [InlineData("public static void Run() { }")]
    [InlineData("public void Run() { }")]
    [InlineData("public static void Run() { } public static int Count => 1;")]
    [InlineData("public void Run() { } public int Count => 1;")]
    [InlineData("public static void Run() { } private void Hidden() { }")]
    [InlineData("public void Run() { } private static int Hidden => 1;")]
    public async Task Exercise04_BothCategoriesRequired_ReportsNothing(string members)
    {
        await Exercise04_VerifyAsync("class Worker { " + members + " }");
    }

    [Fact]
    public async Task Exercise04_AllFourCounts_AreIndependentAndExact()
    {
        await Exercise04_VerifyAsync("""
            internal class [|Worker|]
            {
                public static void A() { }
                public static void A(int value) { }
                public void B() { }
                public void B(int value) { }
                public void B(string value) { }
                public static int C1 => 1;
                public static int C2 => 2;
                public static int C3 => 3;
                public static int C4 => 4;
                public int D1 => 1;
                public int D2 => 2;
                public int D3 => 3;
                public int D4 => 4;
                public int D5 => 5;
            }
            """, ("Worker", 2, 3, 4, 5));
    }

    [Fact]
    public async Task Exercise04_OnlyExplicitPublicMembers_Count()
    {
        await Exercise04_VerifyAsync("""
            class [|Worker|]
            {
                public static void A() { }
                public int B => 1;
                void ImplicitMethod() { }
                static int ImplicitProperty => 1;
                private static void PrivateMethod() { }
                protected void ProtectedMethod() { }
                internal static int InternalProperty => 1;
                protected internal void ProtectedInternalMethod() { }
                private protected int PrivateProtectedProperty => 1;
            }
            """, ("Worker", 1, 0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_AccessorCountAndPrivateSetters_DoNotChangeCounts()
    {
        await Exercise04_VerifyAsync("""
            class [|Worker|]
            {
                public static int A { get; private set; }
                public int B { get; set; }
                public int C { get; private set; }
                public int D => 1;
            }
            """, ("Worker", 0, 0, 1, 3));
    }

    [Fact]
    public async Task Exercise04_AdditionalModifiers_DoNotExcludeMembers()
    {
        await Exercise04_VerifyAsync("""
            abstract class [|Worker|]
            {
                public new static void A() { }
                public abstract void B();
                public virtual int C => 1;
                static public int D => 1;
            }
            """, ("Worker", 1, 1, 1, 1));
    }

    [Fact]
    public async Task Exercise04_ExcludedMemberKinds_DoNotAffectCounts()
    {
        await Exercise04_VerifyAsync("""
            class [|Worker|]
            {
                public static void A() { }
                public int B => 1;
                public static int StaticField;
                public int InstanceField;
                public Worker() { }
                static Worker() { }
                public event System.Action Changed;
                public static event System.Action StaticChanged;
                public event System.Action CustomChanged { add { } remove { } }
                public int this[int index] => index;
                public static Worker operator +(Worker a, Worker b) => a;
                public static implicit operator int(Worker value) => 0;
                public static class Utilities { }
                public class Data { }
            }
            """, ("Worker", 1, 0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_ExcludedMembers_CannotSatisfyReportingConditions()
    {
        await Exercise04_VerifyAsync("""
            class StaticOnly
            {
                public static void A() { }
                public int Field;
                public StaticOnly() { }
                public event System.Action Changed;
                public int this[int index] => index;
            }
            class InstanceOnly
            {
                public void A() { }
                public static int Field;
                public static event System.Action Changed;
                public static InstanceOnly operator +(InstanceOnly a, InstanceOnly b) => a;
            }
            """);
    }

    [Fact]
    public async Task Exercise04_NestedClasses_ReportIndependentlyWithoutLeakingCounts()
    {
        await Exercise04_VerifyAsync("""
            class [|Outer|]
            {
                public static void A() { }
                public int B => 1;
                private class [|Inner|]
                {
                    public void A() { }
                    public static int B => 1;
                }
            }
            """, ("Outer", 1, 0, 0, 1), ("Inner", 0, 1, 1, 0));
    }

    [Fact]
    public async Task Exercise04_NestedAndInheritedMembers_CannotCreateAMixedClass()
    {
        await Exercise04_VerifyAsync("""
            class Base
            {
                public static void A() { }
            }
            class Derived : Base
            {
                public void B() { }
            }
            class Outer
            {
                public static void A() { }
                class Inner { public void B() { } }
                struct Value { public void C() { } }
                record Data { public int D => 1; }
            }
            """);
    }

    [Fact]
    public async Task Exercise04_PartialParts_AreNotCombined()
    {
        await Exercise04_VerifyAsync("""
            partial class Split { public static void A() { } }
            partial class Split { public void B() { } }
            partial class [|Mixed|]
            {
                public static int A => 1;
                public int B => 2;
            }
            partial class Mixed { public static void C() { } }
            """, ("Mixed", 0, 0, 1, 1));
    }

    [Fact]
    public async Task Exercise04_OtherTargetTypes_AreIgnoredButNestedClassesAreAnalyzed()
    {
        await Exercise04_VerifyAsync("""
            struct Value
            {
                public static void A() { }
                public void B() { }
                class [|InStruct|]
                {
                    public static void A() { }
                    public void B() { }
                }
            }
            record Data
            {
                public static int A => 1;
                public int B => 2;
                class [|InRecord|]
                {
                    public static int A => 1;
                    public int B => 2;
                }
            }
            interface IWorker
            {
                public static void A() { }
                public void B();
                class [|InInterface|]
                {
                    public static void A() { }
                    public int B => 1;
                }
            }
            """, ("InStruct", 1, 1, 0, 0), ("InRecord", 0, 0, 1, 1),
            ("InInterface", 1, 0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_AttributesLocalFunctionsAndExplicitImplementations_DoNotCount()
    {
        await Exercise04_VerifyAsync("""
            interface IWorker { void Hidden(); int HiddenProperty { get; } }
            [Unknown]
            class [|Worker|] : IWorker
            {
                [Unknown] public static void A() { }
                [Unknown] public int B => 1;
                void IWorker.Hidden() { }
                int IWorker.HiddenProperty => 0;
                void Container()
                {
                    static void LocalStatic() { }
                    void LocalInstance() { }
                }
            }
            """, ("Worker", 1, 0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_ClassNameAndLocation_UseIdentifierOnly()
    {
        await Exercise04_VerifyAsync("""
            [Unknown]
            internal partial class /* before */ [|@class|] /* after */ <T> where T : class
            {
                public static U Echo<U>(U value) => value;
                public T Item { get; private set; }
            }
            """, ("class", 1, 0, 0, 1));
    }

    [Fact]
    public async Task Exercise04_EmptyCommentsAndStrings_ReportsNothing()
    {
        await Exercise04_VerifyAsync("");
        await Exercise04_VerifyAsync("""
            // class Fake { public static void A() { } public void B() { } }
            class Worker
            {
                string text = "public static void A() { } public int B => 1;";
            }
            """);
    }

    [Fact]
    public void Exercise04_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise04.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("MIXEDMEMBERS001", rule.Id);
        Assert.Equal("Mixed public static and instance members", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Class '{0}' mixes public static and instance members: methods {1} static, {2} instance; properties {3} static, {4} instance",
            rule.MessageFormat.ToString());
    }

    // Expectations are explicit and independent of the student's member helpers.
    private static async Task Exercise04_VerifyAsync(
        string markedSource,
        params (string Name, int StaticMethods, int InstanceMethods, int StaticProperties, int InstanceProperties)[] expectedData)
    {
        var source = new StringBuilder();
        var spans = new List<TextSpan>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string identifier = match.Groups[1].Value;
            spans.Add(new TextSpan(source.Length, identifier.Length));
            source.Append(identifier);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(spans.Count, expectedData.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise04TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise04.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // All analyzer diagnostics are checked, including analyzer crashes.
        // Compiler binding diagnostics are outside this syntax-only exercise.
        Assert.True(spans.Count == diagnostics.Length,
            $"Expected {spans.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < spans.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            var data = expectedData[i];
            Assert.Equal("MIXEDMEMBERS001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Class '{data.Name}' mixes public static and instance members: methods {data.StaticMethods} static, {data.InstanceMethods} instance; properties {data.StaticProperties} static, {data.InstanceProperties} instance",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(spans[i], diagnostic.Location.SourceSpan);
        }
    }

    private static async Task Exercise03_VerifyAsync(
        string markedSource, params (string Kind, string Name, string Accessibility)[] expectedData)
    {
        var source = new StringBuilder();
        var spans = new List<TextSpan>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string identifier = match.Groups[1].Value;
            spans.Add(new TextSpan(source.Length, identifier.Length));
            source.Append(identifier);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(spans.Count, expectedData.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise03TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise03.MainClass();
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
            Assert.Equal("ACCESS001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"The {expectedData[i].Kind} '{expectedData[i].Name}' explicitly declares '{expectedData[i].Accessibility}' accessibility",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(spans[i], diagnostic.Location.SourceSpan);
        }
    }


    [Theory]
    [InlineData("public", "public")]
    [InlineData("private", "private")]
    [InlineData("protected", "protected")]
    [InlineData("internal", "internal")]
    [InlineData("protected internal", "protected internal")]
    [InlineData("internal protected", "protected internal")]
    [InlineData("private protected", "private protected")]
    [InlineData("protected private", "private protected")]
    public async Task Exercise03_AllAccessibilityForms_AcrossAllThreeKinds(
        string modifiers, string accessibility)
    {
        await Exercise03_VerifyAsync(
            "class Outer { " +
            modifiers + " class [|Nested|] { } " +
            modifiers + " void [|Run|]() { } " +
            modifiers + " int [|Count|] { get; } }",
            ("class", "Nested", accessibility),
            ("method", "Run", accessibility),
            ("property", "Count", accessibility));
    }

    [Fact]
    public async Task Exercise03_OtherModifiersAndOrder_DoNotChangeAccessibility()
    {
        await Exercise03_VerifyAsync("""
            static public partial class [|Utilities|]
            {
                internal new static protected void [|Run|]() { }
                protected static private int [|Count|] => 1;
            }
            """, ("class", "Utilities", "public"),
            ("method", "Run", "protected internal"),
            ("property", "Count", "private protected"));
    }

    [Fact]
    public async Task Exercise03_NoExplicitAccessibility_ReportsNothing()
    {
        await Exercise03_VerifyAsync("""
            static partial class Utilities
            {
                static void Run() { }
                static int Count { get; }
                class Nested { }
            }
            interface IWorker
            {
                void Run();
                int Count { get; }
            }
            """);
    }

    [Fact]
    public async Task Exercise03_ContainingAccessibility_IsNotInheritedOrCombined()
    {
        await Exercise03_VerifyAsync("""
            public class [|Outer|]
            {
                void ImplicitMethod() { }
                int ImplicitProperty { get; }
                class ImplicitNested { }
                private class [|Inner|]
                {
                    public void [|Run|]() { }
                    internal int [|Count|] => 1;
                }
            }
            """, ("class", "Outer", "public"), ("class", "Inner", "private"),
            ("method", "Run", "public"), ("property", "Count", "internal"));
    }

    [Fact]
    public async Task Exercise03_AccessorModifiers_DoNotAffectPropertyAccessibility()
    {
        await Exercise03_VerifyAsync("""
            class Worker
            {
                public int [|First|] { get; private set; }
                protected internal int [|Second|] { get; private protected set; }
                int Implicit { get; private set; }
            }
            """, ("property", "First", "public"),
            ("property", "Second", "protected internal"));
    }

    [Fact]
    public async Task Exercise03_MembersInsideStructsRecordsAndInterfaces_AreIncluded()
    {
        await Exercise03_VerifyAsync("""
            public struct Value
            {
                internal void [|Run|]() { }
                public int [|Count|] => 1;
            }
            public record Data
            {
                private static void [|Run|]() { }
                protected int [|Count|] => 1;
            }
            public interface IWorker
            {
                void ImplicitMethod();
                int ImplicitProperty { get; }
                public void [|ExplicitMethod|]();
                public int [|ExplicitProperty|] { get; }
            }
            """, ("method", "Run", "internal"), ("property", "Count", "public"),
            ("method", "Run", "private"), ("property", "Count", "protected"),
            ("method", "ExplicitMethod", "public"), ("property", "ExplicitProperty", "public"));
    }

    [Fact]
    public async Task Exercise03_PartialDeclarations_AreIndependent()
    {
        await Exercise03_VerifyAsync("""
            public partial class [|Worker|] { }
            partial class Worker { }
            public partial class [|Worker|] { }
            """, ("class", "Worker", "public"), ("class", "Worker", "public"));
    }

    [Fact]
    public async Task Exercise03_ExcludedDeclarations_ReportsNothing()
    {
        await Exercise03_VerifyAsync("""
            public struct Value { }
            public record Data;
            public record struct Point;
            public interface IWorker { }
            public enum Choice { First }
            public delegate void Callback();
            class Worker
            {
                public int Field;
                public event System.Action Changed;
                protected internal event System.Action CustomChanged { add { } remove { } }
                public Worker() { }
                public int this[int index] => index;
                public static Worker operator +(Worker a, Worker b) => a;
                public static implicit operator int(Worker value) => 0;
                void Container() { void Local() { } }
            }
            """);
    }

    [Fact]
    public async Task Exercise03_IdentifierNamesAndLocations_ExcludeOtherDeclarationParts()
    {
        await Exercise03_VerifyAsync("""
            [Unknown]
            public partial class /* before */ [|@class|] /* after */ <T>
                where T : class
            {
                [Unknown]
                protected /* between */ internal U /* before */ [|@event|] /* after */ <U>(U value) => value;
                [Unknown]
                private protected int /* before */ [|@namespace|] /* after */ => 1;
            }
            """, ("class", "class", "public"),
            ("method", "event", "protected internal"),
            ("property", "namespace", "private protected"));
    }

    [Fact]
    public async Task Exercise03_OverloadsAndMultipleProperties_ReportSeparately()
    {
        await Exercise03_VerifyAsync("""
            class Worker
            {
                public void [|Run|]() { }
                private void [|Run|](int value) { }
                protected int [|First|] => 1;
                internal int [|Second|] => 2;
            }
            """, ("method", "Run", "public"), ("method", "Run", "private"),
            ("property", "First", "protected"), ("property", "Second", "internal"));
    }

    [Fact]
    public async Task Exercise03_EmptyFileCommentsAndStrings_ReportsNothing()
    {
        await Exercise03_VerifyAsync("");
        await Exercise03_VerifyAsync("""
            // public class Fake { }
            /* protected internal void Fake() { } */
            class Worker { string text = "private protected int Fake { get; }"; }
            """);
    }

    [Fact]
    public void Exercise03_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise03.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("ACCESS001", rule.Id);
        Assert.Equal("Explicit declaration accessibility", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("The {0} '{1}' explicitly declares '{2}' accessibility",
            rule.MessageFormat.ToString());
    }


    [Theory]
    [InlineData("public static partial class [|Utilities|] { }", "class", "Utilities", 3)]
    [InlineData("class Worker { public new static void [|Run|]() { } }", "method", "Run", 3)]
    [InlineData("class Worker { private protected static int [|Count|] => 1; }", "property", "Count", 3)]
    [InlineData("class Worker { protected internal static void [|Run|]() { } }", "method", "Run", 3)]
    [InlineData("class Worker { protected internal new static int [|Count|] => 1; }", "property", "Count", 4)]
    [InlineData("public new static partial class [|Utilities|] { }", "class", "Utilities", 4)]
    [InlineData("class Worker { private new static extern void [|Run|](); }", "method", "Run", 4)]
    public async Task Exercise02_ThresholdAndHigherCounts_Report(
        string source, string kind, string name, int count)
    {
        await Exercise02_VerifyAsync(source, (kind, name, count));
    }

    [Theory]
    [InlineData("")]
    [InlineData("public")]
    [InlineData("static")]
    [InlineData("public static")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public async Task Exercise02_FewerThanThreeModifiers_ReportsNothing(string modifiers)
    {
        await Exercise02_VerifyAsync(
            modifiers + " class Utilities { } class Worker { " +
            modifiers + " void Run() { } " +
            modifiers + " int Count { get; } }");
    }

    [Theory]
    [InlineData("public static partial")]
    [InlineData("static public partial")]
    public async Task Exercise02_ModifierOrder_DoesNotChangeCount(string modifiers)
    {
        await Exercise02_VerifyAsync(
            modifiers + " class [|Utilities|] { }", ("class", "Utilities", 3));
    }

    [Fact]
    public async Task Exercise02_NoPublicOrStaticRequirement()
    {
        await Exercise02_VerifyAsync("""
            internal abstract partial class [|Worker|]
            {
                protected internal abstract void [|Run|]();
                private protected abstract int [|Count|] { get; }
            }
            """, ("class", "Worker", 3), ("method", "Run", 3), ("property", "Count", 3));
    }

    [Fact]
    public async Task Exercise02_AttributesTypesAndConstraints_DoNotCount()
    {
        await Exercise02_VerifyAsync("""
            [First, Second, Third]
            public partial class Utilities<T> where T : class, new()
            {
                [First, Second, Third]
                public static T Read<U>(U value) where U : class => default(T);
                [First, Second, Third]
                public static int Count { get; private set; }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_OwnModifiersOnly_NestedDeclarationsAreIndependent()
    {
        await Exercise02_VerifyAsync("""
            public static partial class [|Outer|]
            {
                public static void Two() { }
                public static int TwoProperty { get; private set; }
                public class TwoNested { }
                private new static void [|Three|]() { }
                private protected static int [|ThreeProperty|] { get; set; }
                private static partial class [|Inner|] { }
            }
            """, ("class", "Outer", 3), ("method", "Three", 3),
            ("property", "ThreeProperty", 3), ("class", "Inner", 3));
    }

    [Fact]
    public async Task Exercise02_PartialDeclarations_AreNotCombined()
    {
        await Exercise02_VerifyAsync("""
            public partial class Split { }
            static partial class Split { }
            public static partial class [|Both|] { }
            public static partial class [|Both|] { }
            """, ("class", "Both", 3), ("class", "Both", 3));
    }

    [Fact]
    public async Task Exercise02_OtherDeclarationKinds_AreExcluded()
    {
        // Modifier legality is deliberately outside the syntax-only contract.
        await Exercise02_VerifyAsync("""
            public static partial struct IgnoredStruct { }
            public static partial record IgnoredRecord;
            public static partial record struct IgnoredRecordStruct;
            public static partial interface IIgnored { }
            public new static enum IgnoredEnum { First }
            public new static delegate void IgnoredDelegate();
            class Worker
            {
                private static readonly int Field;
                public new static event System.Action Changed;
                public new static event System.Action CustomChanged { add { } remove { } }
                public new static Worker() { }
                public new static int this[int index] => index;
                public new static Worker operator +(Worker a, Worker b) => a;
                public new static implicit operator int(Worker value) => 0;
                void Container()
                {
                    static async System.Threading.Tasks.Task Local() { }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise02_MembersInsideOtherTypes_AreIncluded()
    {
        await Exercise02_VerifyAsync("""
            struct Value
            {
                public new static void [|Run|]() { }
            }
            record Data
            {
                public new static int [|Count|] => 1;
            }
            interface IWorker
            {
                public static abstract void [|Run|]();
                public static abstract int [|Count|] { get; }
            }
            """, ("method", "Run", 3), ("property", "Count", 3),
            ("method", "Run", 3), ("property", "Count", 3));
    }

    [Fact]
    public async Task Exercise02_GenericAndEscapedNames_UseValueText()
    {
        await Exercise02_VerifyAsync("""
            public static partial class [|@class|]<T> where T : class
            {
                private new static U [|@event|]<U>(U value) => value;
                private protected static int [|@namespace|] => 1;
            }
            """, ("class", "class", 3), ("method", "event", 3),
            ("property", "namespace", 3));
    }

    [Fact]
    public async Task Exercise02_Locations_ExcludeTriviaAttributesAndTypeParameters()
    {
        await Exercise02_VerifyAsync("""
            [Unknown]
            public /* comment */ static partial class /* before */ [|Utilities|] /* after */ <T>
            {
                [Unknown]
                private new static U /* before */ [|Read|] /* after */ <U>(U value) => value;
                [Unknown]
                private protected static int /* before */ [|Count|] /* after */ => 1;
            }
            """, ("class", "Utilities", 3), ("method", "Read", 3), ("property", "Count", 3));
    }

    [Fact]
    public async Task Exercise02_EmptyFileCommentsAndStrings_ReportsNothing()
    {
        await Exercise02_VerifyAsync("");
        await Exercise02_VerifyAsync("""
            // public static partial class Fake { }
            /* private new static void Fake() { } */
            class Worker { string text = "public static partial class Fake { }"; }
            """);
    }

    [Fact]
    public void Exercise02_Descriptor_MatchesContract()
    {
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise02.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("MODCOUNT001", rule.Id);
        Assert.Equal("Declaration modifier count", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("The {0} '{1}' has {2} explicit modifier(s)", rule.MessageFormat.ToString());
    }

    // Expected counts, kinds and names are explicit; no student helpers are used.
    private static async Task Exercise02_VerifyAsync(
        string markedSource, params (string Kind, string Name, int Count)[] expectedData)
    {
        var source = new StringBuilder();
        var spans = new List<TextSpan>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string identifier = match.Groups[1].Value;
            spans.Add(new TextSpan(source.Length, identifier.Length));
            source.Append(identifier);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(spans.Count, expectedData.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise02TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise02.MainClass();
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
            Assert.Equal("MODCOUNT001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"The {expectedData[i].Kind} '{expectedData[i].Name}' has {expectedData[i].Count} explicit modifier(s)",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(spans[i], diagnostic.Location.SourceSpan);
        }
    }

    [Theory]
    [InlineData("public static class [|Utilities|] { }", "class", "Utilities")]
    [InlineData("class Worker { public static void [|Run|]() { } }", "method", "Run")]
    [InlineData("class Worker { public static int [|Count|] { get; } }", "property", "Count")]
    public async Task Exercise01_EachDeclarationKind_Reports(
        string source, string kind, string name)
    {
        await Exercise01_VerifyAsync(source, (kind, name));
    }

    [Theory]
    [InlineData("static public class [|Utilities|] { }", "class", "Utilities")]
    [InlineData("class Worker { static public void [|Run|]() { } }", "method", "Run")]
    [InlineData("class Worker { static public int [|Count|] => 1; }", "property", "Count")]
    [InlineData("public static partial class [|Utilities|] { }", "class", "Utilities")]
    [InlineData("class Worker { public new static void [|Run|]() { } }", "method", "Run")]
    [InlineData("class Worker { public new static int [|Count|] => 1; }", "property", "Count")]
    public async Task Exercise01_ModifierOrderAndAdditionalModifiers_DoNotPreventReporting(
        string source, string kind, string name)
    {
        await Exercise01_VerifyAsync(source, (kind, name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("public")]
    [InlineData("static")]
    [InlineData("internal static")]
    [InlineData("private static")]
    [InlineData("protected static")]
    [InlineData("protected internal static")]
    [InlineData("private protected static")]
    public async Task Exercise01_MissingPublicOrStatic_ReportsNothing(string modifiers)
    {
        // Accessibility validity is outside this syntax-only exercise.
        await Exercise01_VerifyAsync(
            modifiers + " class Utilities { } " +
            "class Worker { " +
            modifiers + " void Run() { } " +
            modifiers + " int Count { get; } }");
    }

    [Fact]
    public async Task Exercise01_MultipleDeclarations_ReportIndividually()
    {
        await Exercise01_VerifyAsync("""
            public static class [|Utilities|]
            {
                public static void [|Run|]() { }
                public static void [|Run|](int value) { }
                public static int [|Count|] => 1;
            }
            public static class [|Other|] { }
            """,
            ("class", "Utilities"), ("method", "Run"), ("method", "Run"),
            ("property", "Count"), ("class", "Other"));
    }

    [Fact]
    public async Task Exercise01_ContainingTypeModifiers_AreNotInherited()
    {
        // An instance member in a static class is a compiler binding error,
        // but the requirement still depends only on its own written modifiers.
        await Exercise01_VerifyAsync("""
            public static class [|Outer|]
            {
                public void InstanceMethod() { }
                public int InstanceProperty { get; }
                static void NonPublicMethod() { }
                static int NonPublicProperty { get; }
                public class InstanceNested { }
                static class NonPublicNested { }
                public static class [|Both|] { }
            }
            """, ("class", "Outer"), ("class", "Both"));
    }

    [Fact]
    public async Task Exercise01_NestedDeclarations_DoNotRequirePublicContainingTypes()
    {
        await Exercise01_VerifyAsync("""
            internal class Outer
            {
                private class Inner
                {
                    public static void [|Run|]() { }
                    public static int [|Count|] { get; set; }
                    public static class [|Utilities|] { }
                }
            }
            """, ("method", "Run"), ("property", "Count"), ("class", "Utilities"));
    }

    [Fact]
    public async Task Exercise01_ExcludedDeclarationKinds_ReportsNothing()
    {
        // Some excluded declarations have invalid modifier combinations.
        // They remain excluded: this exercise does not validate C# modifiers.
        await Exercise01_VerifyAsync("""
            public static struct IgnoredStruct { }
            public static record IgnoredRecord;
            public static record struct IgnoredRecordStruct;
            public static interface IIgnored { }
            public static enum IgnoredEnum { First }
            public static delegate void IgnoredDelegate();
            class Worker
            {
                public static int Field;
                public static event System.Action Changed;
                public static event System.Action CustomChanged
                {
                    add { }
                    remove { }
                }
                public static Worker() { }
                public static Worker operator +(Worker a, Worker b) => a;
                public static implicit operator int(Worker value) => 0;
                public static int this[int index] => index;
                void Container()
                {
                    static void Local() { }
                }
            }
            """);
    }

    [Fact]
    public async Task Exercise01_MethodsAndPropertiesInsideOtherTypeKinds_StillQualify()
    {
        await Exercise01_VerifyAsync("""
            struct Value
            {
                public static void [|Run|]() { }
                public static int [|Count|] => 1;
            }
            record Data
            {
                public static int [|Total|] => 2;
            }
            interface IWorker
            {
                static void ImplicitPublic() { }
                static int ImplicitCount => 0;
                public static void [|Explicit|]() { }
                public static int [|ExplicitCount|] => 1;
            }
            """, ("method", "Run"), ("property", "Count"), ("property", "Total"),
            ("method", "Explicit"), ("property", "ExplicitCount"));
    }

    [Fact]
    public async Task Exercise01_PartialClasses_AreNotCombined()
    {
        await Exercise01_VerifyAsync("""
            public partial class Split { }
            static partial class Split { }

            public static partial class [|Both|] { }
            public static partial class [|Both|] { }
            """, ("class", "Both"), ("class", "Both"));
    }

    [Fact]
    public async Task Exercise01_GenericNames_ExcludeTypeParameters()
    {
        await Exercise01_VerifyAsync("""
            public static class [|Utilities|]<T> where T : class
            {
                public static U [|Echo|]<U>(U value) => value;
            }
            """, ("class", "Utilities"), ("method", "Echo"));
    }

    [Fact]
    public async Task Exercise01_EscapedIdentifiers_UseValueTextWithWholeTokenLocation()
    {
        await Exercise01_VerifyAsync("""
            public static class [|@class|]
            {
                public static void [|@event|]() { }
                public static int [|@namespace|] => 1;
            }
            """, ("class", "class"), ("method", "event"), ("property", "namespace"));
    }

    [Fact]
    public async Task Exercise01_Locations_ExcludeAttributesModifiersAndTrivia()
    {
        await Exercise01_VerifyAsync("""
            [Unknown]
            public /* modifier comment */ static class /* before */ [|Utilities|] /* after */
            {
                [Unknown]
                public static int /* before */ [|Read|] /* after */ () => 1;

                [Unknown]
                public static int /* before */ [|Count|] /* after */ { get; set; }
            }
            """, ("class", "Utilities"), ("method", "Read"), ("property", "Count"));
    }

    [Fact]
    public async Task Exercise01_CommentsAndStrings_AreNotDeclarations()
    {
        await Exercise01_VerifyAsync("""
            // public static class Fake { }
            /* public static void FakeMethod() { } */
            class Worker
            {
                string text = "public static int FakeProperty { get; }";
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
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise01.MainClass();
        DiagnosticDescriptor rule = Assert.Single(analyzer.SupportedDiagnostics);
        Assert.Equal("MOD001", rule.Id);
        Assert.Equal("Public static declaration", rule.Title.ToString());
        Assert.Equal("Practice", rule.Category);
        Assert.Equal(DiagnosticSeverity.Warning, rule.DefaultSeverity);
        Assert.True(rule.IsEnabledByDefault);
        Assert.Equal("Public static {0} '{1}' is declared", rule.MessageFormat.ToString());
    }

    // Mark only expected identifier tokens with [|...|].
    // Expected kind and ValueText are supplied explicitly in marker order.
    // No student helper or declaration-analysis logic is used to derive expectations.
    private static async Task Exercise01_VerifyAsync(
        string markedSource, params (string Kind, string Name)[] expectedData)
    {
        var source = new StringBuilder();
        var spans = new List<TextSpan>();
        int previousEnd = 0;
        foreach (Match match in Regex.Matches(markedSource, @"\[\|(.*?)\|\]", RegexOptions.Singleline))
        {
            source.Append(markedSource, previousEnd, match.Index - previousEnd);
            string identifier = match.Groups[1].Value;
            spans.Add(new TextSpan(source.Length, identifier.Length));
            source.Append(identifier);
            previousEnd = match.Index + match.Length;
        }
        source.Append(markedSource, previousEnd, markedSource.Length - previousEnd);
        Assert.Equal(spans.Count, expectedData.Length);

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(),
            new CSharpParseOptions(LanguageVersion.Preview));
        Assert.Empty(tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Exercise01TestAssembly",
            syntaxTrees: [tree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        DiagnosticAnalyzer analyzer = new AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis
            .Lesson10CommonDeclarationComponentsAndModifiers.Exercise01.MainClass();
        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        // Include all analyzer diagnostics, including crashes (AD0001).
        // Compiler binding errors are outside this syntax-only exercise.
        Assert.True(spans.Count == diagnostics.Length,
            $"Expected {spans.Count} diagnostic(s), got {diagnostics.Length}." +
            Environment.NewLine + string.Join(Environment.NewLine, diagnostics));
        Diagnostic[] ordered = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        for (int i = 0; i < spans.Count; i++)
        {
            Diagnostic diagnostic = ordered[i];
            Assert.Equal("MOD001", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal(
                $"Public static {expectedData[i].Kind} '{expectedData[i].Name}' is declared",
                diagnostic.GetMessage());
            Assert.True(diagnostic.Location.IsInSource);
            Assert.Same(tree, diagnostic.Location.SourceTree);
            Assert.Equal(spans[i], diagnostic.Location.SourceSpan);
        }
    }

}