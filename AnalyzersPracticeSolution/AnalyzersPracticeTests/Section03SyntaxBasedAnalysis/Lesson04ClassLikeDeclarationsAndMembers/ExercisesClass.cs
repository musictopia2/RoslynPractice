using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace AnalyzersPracticeTests.Section03SyntaxBasedAnalysis.Lesson04ClassLikeDeclarationsAndMembers;
[Trait("Section", "Section03SyntaxBasedAnalysis")]
public class ExercisesClass
{
    [Fact]
    public async Task Exercise01_AnalyzeClasses_ShouldReportCorrectMemberInformation()
    {
        const string source = """
            namespace TestNamespace;

            public class FirstClass
            {
                private int first, second;

                public string Name { get; set; } = "";

                public void Run()
                {
                }
            }

            public class EmptyClass
            {
            }

            public class ThirdClass
            {
                public void Start()
                {
                }

                public int Score { get; set; }

                private string ending = "";
            }
            """;

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

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson04ClassLikeDeclarationsAndMembers
                .Exercise01
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == 3,
            $"Expected 3 diagnostics, but found {diagnostics.Length}. " +
            $"Messages: {string.Join(" | ", diagnostics.Select(x => x.GetMessage()))}");

        Diagnostic? first = diagnostics.FirstOrDefault(x =>
            x.GetMessage().StartsWith("Class 'FirstClass'"));

        Assert.True(
            first is not null,
            "Expected a diagnostic for FirstClass.");

        Assert.Equal("RPA030401", first.Id);
        Assert.Equal(DiagnosticSeverity.Info, first.Severity);

        Assert.Equal(
            "Class 'FirstClass' has 3 member(s). First: 'first'. Last: 'Run'.",
            first.GetMessage());

        Assert.True(
            first.Location != Location.None,
            "FirstClass diagnostic should have a source location.");

        Assert.Equal(
            "FirstClass",
            first.Location.SourceTree!
                .GetText()
                .ToString(first.Location.SourceSpan));

        Diagnostic? empty = diagnostics.FirstOrDefault(x =>
            x.GetMessage().StartsWith("Class 'EmptyClass'"));

        Assert.True(
            empty is not null,
            "Expected a diagnostic for EmptyClass.");

        Assert.Equal("RPA030401", empty.Id);
        Assert.Equal(DiagnosticSeverity.Info, empty.Severity);

        Assert.Equal(
            "Class 'EmptyClass' has 0 member(s). First: '<none>'. Last: '<none>'.",
            empty.GetMessage());

        Assert.True(
            empty.Location != Location.None,
            "EmptyClass diagnostic should have a source location.");

        Assert.Equal(
            "EmptyClass",
            empty.Location.SourceTree!
                .GetText()
                .ToString(empty.Location.SourceSpan));

        Diagnostic? third = diagnostics.FirstOrDefault(x =>
            x.GetMessage().StartsWith("Class 'ThirdClass'"));

        Assert.True(
            third is not null,
            "Expected a diagnostic for ThirdClass.");

        Assert.Equal("RPA030401", third.Id);
        Assert.Equal(DiagnosticSeverity.Info, third.Severity);

        Assert.Equal(
            "Class 'ThirdClass' has 3 member(s). First: 'Start'. Last: 'ending'.",
            third.GetMessage());

        Assert.True(
            third.Location != Location.None,
            "ThirdClass diagnostic should have a source location.");

        Assert.Equal(
            "ThirdClass",
            third.Location.SourceTree!
                .GetText()
                .ToString(third.Location.SourceSpan));
    }
    [Fact]
    public async Task Exercise02_AnalyzeStructsAndRecords_ShouldReportCorrectMemberInformation()
    {
        const string source = """
        namespace TestNamespace;

        public struct GameState
        {
            private int first, second;

            public int Score { get; set; }

            public void Reset()
            {
            }
        }

        public struct EmptyStruct
        {
        }

        public record Player
        {
            public string Name { get; set; } = "";

            public void Save()
            {
            }
        }

        public record struct Coordinates
        {
            public int X { get; set; }

            private int ending;
        }

        public record struct EmptyRecordStruct
        {
        }

        public class IgnoredClass
        {
            public void Run()
            {
            }
        }

        public interface IIgnored
        {
            void Execute();
        }
        """;

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

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson04ClassLikeDeclarationsAndMembers
                .Exercise02
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == 5,
            $"Expected 5 diagnostics, but found {diagnostics.Length}. " +
            $"Messages: {string.Join(" | ", diagnostics.Select(x => x.GetMessage()))}");

        Diagnostic? gameState = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'GameState'"));

        Assert.True(
            gameState is not null,
            "Expected a diagnostic for GameState.");

        Assert.Equal("RPA030402", gameState.Id);
        Assert.Equal(DiagnosticSeverity.Info, gameState.Severity);

        Assert.Equal(
            "Struct 'GameState' has 3 member(s). First: 'first'. Last: 'Reset'.",
            gameState.GetMessage());

        Assert.True(
            gameState.Location != Location.None,
            "GameState diagnostic should have a source location.");

        Assert.Equal(
            "GameState",
            gameState.Location.SourceTree!
                .GetText()
                .ToString(gameState.Location.SourceSpan));

        Diagnostic? emptyStruct = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'EmptyStruct'"));

        Assert.True(
            emptyStruct is not null,
            "Expected a diagnostic for EmptyStruct.");

        Assert.Equal(
            "Struct 'EmptyStruct' has 0 member(s). First: '<none>'. Last: '<none>'.",
            emptyStruct.GetMessage());

        Assert.True(
            emptyStruct.Location != Location.None,
            "EmptyStruct diagnostic should have a source location.");

        Assert.Equal(
            "EmptyStruct",
            emptyStruct.Location.SourceTree!
                .GetText()
                .ToString(emptyStruct.Location.SourceSpan));

        Diagnostic? player = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'Player'"));

        Assert.True(
            player is not null,
            "Expected a diagnostic for Player.");

        Assert.Equal(
            "Record Class 'Player' has 2 member(s). First: 'Name'. Last: 'Save'.",
            player.GetMessage());

        Assert.True(
            player.Location != Location.None,
            "Player diagnostic should have a source location.");

        Assert.Equal(
            "Player",
            player.Location.SourceTree!
                .GetText()
                .ToString(player.Location.SourceSpan));

        Diagnostic? coordinates = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'Coordinates'"));

        Assert.True(
            coordinates is not null,
            "Expected a diagnostic for Coordinates.");

        Assert.Equal(
            "Record Struct 'Coordinates' has 2 member(s). First: 'X'. Last: 'ending'.",
            coordinates.GetMessage());

        Assert.True(
            coordinates.Location != Location.None,
            "Coordinates diagnostic should have a source location.");

        Assert.Equal(
            "Coordinates",
            coordinates.Location.SourceTree!
                .GetText()
                .ToString(coordinates.Location.SourceSpan));

        Diagnostic? emptyRecordStruct = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'EmptyRecordStruct'"));

        Assert.True(
            emptyRecordStruct is not null,
            "Expected a diagnostic for EmptyRecordStruct.");

        Assert.Equal(
            "Record Struct 'EmptyRecordStruct' has 0 member(s). First: '<none>'. Last: '<none>'.",
            emptyRecordStruct.GetMessage());

        Assert.True(
            emptyRecordStruct.Location != Location.None,
            "EmptyRecordStruct diagnostic should have a source location.");

        Assert.Equal(
            "EmptyRecordStruct",
            emptyRecordStruct.Location.SourceTree!
                .GetText()
                .ToString(emptyRecordStruct.Location.SourceSpan));
    }

    [Fact]
    public async Task Exercise03_AnalyzeInterfaces_ShouldReportCorrectMemberInformation()
    {
        const string source = """
        namespace TestNamespace;

        public interface IGame
        {
            void Start();

            string Name { get; }

            void Stop();
        }

        public interface IProperties
        {
            int Score { get; set; }

            bool IsFinished { get; }
        }

        public interface IEmpty
        {
        }

        public class IgnoredClass
        {
            public void Run()
            {
            }

            public int Score { get; set; }
        }

        public struct IgnoredStruct
        {
            public void Reset()
            {
            }
        }
        """;

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

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson04ClassLikeDeclarationsAndMembers
                .Exercise03
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == 3,
            $"Expected 3 diagnostics, but found {diagnostics.Length}. " +
            $"Messages: {string.Join(" | ", diagnostics.Select(x => x.GetMessage()))}");

        Diagnostic? game = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'IGame'"));

        Assert.True(
            game is not null,
            "Expected a diagnostic for IGame.");

        Assert.Equal("RPA030403", game.Id);
        Assert.Equal(DiagnosticSeverity.Info, game.Severity);

        Assert.Equal(
            "Interface 'IGame' has 3 member(s): 2 method(s), 1 property(s). " +
            "First: 'Start'. Last: 'Stop'.",
            game.GetMessage());

        Assert.True(
            game.Location != Location.None,
            "IGame diagnostic should have a source location.");

        Assert.Equal(
            "IGame",
            game.Location.SourceTree!
                .GetText()
                .ToString(game.Location.SourceSpan));

        Diagnostic? properties = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'IProperties'"));

        Assert.True(
            properties is not null,
            "Expected a diagnostic for IProperties.");

        Assert.Equal("RPA030403", properties.Id);
        Assert.Equal(DiagnosticSeverity.Info, properties.Severity);

        Assert.Equal(
            "Interface 'IProperties' has 2 member(s): 0 method(s), 2 property(s). " +
            "First: 'Score'. Last: 'IsFinished'.",
            properties.GetMessage());

        Assert.True(
            properties.Location != Location.None,
            "IProperties diagnostic should have a source location.");

        Assert.Equal(
            "IProperties",
            properties.Location.SourceTree!
                .GetText()
                .ToString(properties.Location.SourceSpan));

        Diagnostic? empty = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'IEmpty'"));

        Assert.True(
            empty is not null,
            "Expected a diagnostic for IEmpty.");

        Assert.Equal("RPA030403", empty.Id);
        Assert.Equal(DiagnosticSeverity.Info, empty.Severity);

        Assert.Equal(
            "Interface 'IEmpty' has 0 member(s): 0 method(s), 0 property(s). " +
            "First: '<none>'. Last: '<none>'.",
            empty.GetMessage());

        Assert.True(
            empty.Location != Location.None,
            "IEmpty diagnostic should have a source location.");

        Assert.Equal(
            "IEmpty",
            empty.Location.SourceTree!
                .GetText()
                .ToString(empty.Location.SourceSpan));
    }
    [Fact]
    public async Task Exercise04_AnalyzeAllClassLikeDeclarations_ShouldReportCorrectMemberInformation()
    {
        const string source = """
        namespace TestNamespace;

        public class Game
        {
            private int first, second;

            public string Name { get; set; } = "";

            public void Start()
            {
            }
        }

        public struct Score
        {
            public int Value { get; set; }

            public void Reset()
            {
            }
        }

        public interface IPlayer
        {
            string Name { get; }

            void Play();
        }

        public record Customer
        {
            private int id;

            public string Name { get; set; } = "";
        }

        public record struct Coordinates
        {
            public int X { get; set; }

            private int ending;
        }

        public class EmptyClass
        {
        }

        public class Outer
        {
            private int first;

            public class Inner
            {
                public void Run()
                {
                public int Value { get; set; }
            }

            public void Finish()
            {
            }
        }
        """;

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

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson04ClassLikeDeclarationsAndMembers
                .Exercise04
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == 8,
            $"Expected 8 diagnostics, but found {diagnostics.Length}. " +
            $"Messages: {string.Join(" | ", diagnostics.Select(x => x.GetMessage()))}");

        VerifyDiagnostic(
            diagnostics,
            "Game",
            "Class 'Game' has 3 member(s): 1 method(s), 1 property(s), 1 field(s). " +
            "First: 'first'. Last: 'Start'.");

        VerifyDiagnostic(
            diagnostics,
            "Score",
            "Struct 'Score' has 2 member(s): 1 method(s), 1 property(s), 0 field(s). " +
            "First: 'Value'. Last: 'Reset'.");

        VerifyDiagnostic(
            diagnostics,
            "IPlayer",
            "Interface 'IPlayer' has 2 member(s): 1 method(s), 1 property(s), 0 field(s). " +
            "First: 'Name'. Last: 'Play'.");

        VerifyDiagnostic(
            diagnostics,
            "Customer",
            "Record Class 'Customer' has 2 member(s): 0 method(s), 1 property(s), 1 field(s). " +
            "First: 'id'. Last: 'Name'.");

        VerifyDiagnostic(
            diagnostics,
            "Coordinates",
            "Record Struct 'Coordinates' has 2 member(s): 0 method(s), 1 property(s), 1 field(s). " +
            "First: 'X'. Last: 'ending'.");

        VerifyDiagnostic(
            diagnostics,
            "EmptyClass",
            "Class 'EmptyClass' has 0 member(s): 0 method(s), 0 property(s), 0 field(s). " +
            "First: '<none>'. Last: '<none>'.");

        VerifyDiagnostic(
            diagnostics,
            "Outer",
            "Class 'Outer' has 3 member(s): 1 method(s), 0 property(s), 1 field(s). " +
            "First: 'first'. Last: 'Finish'.");

        VerifyDiagnostic(
            diagnostics,
            "Inner",
            "Class 'Inner' has 2 member(s): 1 method(s), 1 property(s), 0 field(s). " +
            "First: 'Run'. Last: 'Value'.");
    }

    private static void VerifyDiagnostic(
        ImmutableArray<Diagnostic> diagnostics,
        string declarationName,
        string expectedMessage)
    {
        Diagnostic? diagnostic = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains($"'{declarationName}'"));

        Assert.True(
            diagnostic is not null,
            $"Expected a diagnostic for {declarationName}.");

        Assert.Equal("RPA030404", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);

        Assert.Equal(
            expectedMessage,
            diagnostic.GetMessage());

        Assert.True(
            diagnostic.Location != Location.None,
            $"{declarationName} diagnostic should have a source location.");

        Assert.Equal(
            declarationName,
            diagnostic.Location.SourceTree!
                .GetText()
                .ToString(diagnostic.Location.SourceSpan));
    }
    [Fact]
    public async Task Exercise05_MemberOrdering_ShouldReportOnlyIncorrectDeclarations()
    {
        const string source = """
        namespace TestNamespace;

        // Correct: all five categories in order.
        public class GoodClass
        {
            private int first;
            private int second;

            public int Score { get; set; }
            public string Name { get; set; } = "";

            public GoodClass()
            {
            }

            public void Start()
            {
            }

            public void Stop()
            {
            }

            public class State
            {
            }
        }

        // Incorrect: field appears after method.
        public struct BadStruct
        {
            public void Run()
            {
            }

            private int value;
        }

        // Correct: categories may be missing.
        public interface IGoodInterface
        {
            int Score { get; }

            void Play();
        }

        // Incorrect: property appears after method.
        public record BadRecord
        {
            public void Save()
            {
            }

            public string Name { get; set; } = "";
        }

        // Correct: property followed by method.
        public record struct GoodRecordStruct
        {
            public int Value { get; set; }

            public void Reset()
            {
            }
        }

        // Incorrect in several ways, but should receive one diagnostic.
        public class VeryBadClass
        {
            public void Run()
            {
            }

            public int Score { get; set; }

            private int value;
        }

        // Outer is correctly ordered.
        // Nested BadNested is independently incorrect.
        public class Outer
        {
            private int value;

            public class BadNested
            {
                public void Run()
                {
                }

                private int later;
            }
        }
        """;

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

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson04ClassLikeDeclarationsAndMembers
                .Exercise05
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == 4,
            $"Expected 4 diagnostics, but found {diagnostics.Length}. " +
            $"Messages: {string.Join(" | ", diagnostics.Select(x => x.GetMessage()))}");

        VerifyDiagnostic2(
            diagnostics,
            "BadStruct",
            "Struct 'BadStruct' contains members that are not in the required order");

        VerifyDiagnostic2(
            diagnostics,
            "BadRecord",
            "Record Class 'BadRecord' contains members that are not in the required order");

        VerifyDiagnostic2(
            diagnostics,
            "VeryBadClass",
            "Class 'VeryBadClass' contains members that are not in the required order");

        VerifyDiagnostic2(
            diagnostics,
            "BadNested",
            "Class 'BadNested' contains members that are not in the required order");

        // These declarations are correctly ordered and must not be reported.
        AssertNoDiagnostic(diagnostics, "GoodClass");
        AssertNoDiagnostic(diagnostics, "IGoodInterface");
        AssertNoDiagnostic(diagnostics, "GoodRecordStruct");
        AssertNoDiagnostic(diagnostics, "State");
        AssertNoDiagnostic(diagnostics, "Outer");
    }

    private static void VerifyDiagnostic2(
        ImmutableArray<Diagnostic> diagnostics,
        string declarationName,
        string expectedMessage)
    {
        Diagnostic? diagnostic = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains($"'{declarationName}'"));

        Assert.True(
            diagnostic is not null,
            $"Expected a diagnostic for {declarationName}.");

        Assert.Equal("RPA030405", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal(expectedMessage, diagnostic.GetMessage());

        Assert.True(
            diagnostic.Location != Location.None,
            $"{declarationName} diagnostic should have a source location.");

        Assert.Equal(
            declarationName,
            diagnostic.Location.SourceTree!
                .GetText()
                .ToString(diagnostic.Location.SourceSpan));
    }

    private static void AssertNoDiagnostic(
        ImmutableArray<Diagnostic> diagnostics,
        string declarationName)
    {
        Diagnostic? diagnostic = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains($"'{declarationName}'"));

        Assert.True(
            diagnostic is null,
            $"Did not expect a diagnostic for {declarationName}, but found: " +
            $"{diagnostic?.GetMessage()}");
    }

    [Fact]
    public async Task Exercise05_NestedTypes_ShouldParticipateInMemberOrdering()
    {
        const string source = """
        namespace TestNamespace;

        // Invalid: a field cannot appear after a nested type.
        public class BadNestedClassOrder
        {
            public class Nested
            {
            }

            private int value;
        }

        // Invalid: a property cannot appear after a nested struct.
        public class BadNestedStructOrder
        {
            public struct Nested
            {
            }

            public int Value { get; set; }
        }

        // Invalid: a method cannot appear after a nested interface.
        public class BadNestedInterfaceOrder
        {
            public interface INested
            {
            }

            public void Run()
            {
            }
        }

        // Invalid: a field cannot appear after a nested record class.
        public class BadNestedRecordOrder
        {
            public record Nested
            {
            }

            private int value;
        }

        // Invalid: a property cannot appear after a nested record struct.
        public class BadNestedRecordStructOrder
        {
            public record struct Nested
            {
            }

            public int Value { get; set; }
        }

        // Valid: nested types are last.
        public class GoodNestedOrder
        {
            private int value;

            public int Value { get; set; }

            public void Run()
            {
            }

            public class Nested
            {
            }
        }
        """;

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

        DiagnosticAnalyzer analyzer =
            new AnalyzersPracticeLibrary
                .Section03SyntaxBasedAnalysis
                .Lesson04ClassLikeDeclarationsAndMembers
                .Exercise05
                .MainClass();

        CompilationWithAnalyzers compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create(analyzer));

        ImmutableArray<Diagnostic> diagnostics =
            await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.True(
            diagnostics.Length == 5,
            $"Expected 5 diagnostics, but found {diagnostics.Length}. " +
            $"Messages: {string.Join(" | ", diagnostics.Select(x => x.GetMessage()))}");

        string[] expectedNames =
        [
            "BadNestedClassOrder",
        "BadNestedStructOrder",
        "BadNestedInterfaceOrder",
        "BadNestedRecordOrder",
        "BadNestedRecordStructOrder"
        ];

        foreach (string expectedName in expectedNames)
        {
            Diagnostic? diagnostic = diagnostics.FirstOrDefault(x =>
                x.GetMessage().Contains($"'{expectedName}'"));

            Assert.True(
                diagnostic is not null,
                $"Expected a diagnostic for {expectedName}.");

            Assert.Equal("RPA030405", diagnostic.Id);

            Assert.True(
                diagnostic.Location != Location.None,
                $"{expectedName} diagnostic should have a source location.");

            Assert.Equal(
                expectedName,
                diagnostic.Location.SourceTree!
                    .GetText()
                    .ToString(diagnostic.Location.SourceSpan));
        }

        Diagnostic? goodDiagnostic = diagnostics.FirstOrDefault(x =>
            x.GetMessage().Contains("'GoodNestedOrder'"));

        Assert.True(
            goodDiagnostic is null,
            $"Did not expect a diagnostic for GoodNestedOrder, but found: " +
            $"{goodDiagnostic?.GetMessage()}");
    }

}