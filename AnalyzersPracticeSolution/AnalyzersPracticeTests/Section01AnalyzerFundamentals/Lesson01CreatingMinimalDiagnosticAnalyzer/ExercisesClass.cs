namespace AnalyzersPracticeTests.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer;
[Trait("Section", "Section01AnalyzerFundamentals")]
public class ExercisesClass
{
    [Fact]
    public void GetFileOptions_ReturnsExpectedFlags()
    {
        AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
            .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02
            .EnumFileOptions result =
            AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02
                .MainClass.GetFileOptions();

        Assert.True(
            result.HasFlag(
                AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                    .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02
                    .EnumFileOptions.CanOpen),
            "CanOpen should be included.");

        Assert.True(
            result.HasFlag(
                AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                    .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02
                    .EnumFileOptions.CanPrint),
            "CanPrint should be included.");

        Assert.False(
            result.HasFlag(
                AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                    .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02
                    .EnumFileOptions.CanSave),
            "CanSave should not be included.");
    }
    [Fact]
    public void GetGeneratedCodeOptions_ReturnsExpectedFlags()
    {
        Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags result =
            AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise04
                .MainClass.GetGeneratedCodeOptions();

        Assert.True(
            result.HasFlag(
                Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags.Analyze),
            "Analyze should be included.");

        Assert.True(
            result.HasFlag(
                Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags.ReportDiagnostics),
            "ReportDiagnostics should be included.");
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetGeneratedCodeOptionsExercise05_ReturnsExpectedFlags(
        bool includeGeneratedCode)
    {
        Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags result =
            AnalyzersPracticeLibrary.Section01AnalyzerFundamentals
                .Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise05
                .MainClass.GetGeneratedCodeOptions(includeGeneratedCode);

        if (includeGeneratedCode)
        {
            Assert.True(
                result.HasFlag(
                    Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags.Analyze),
                "Analyze should be included when generated code is enabled.");

            Assert.True(
                result.HasFlag(
                    Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags.ReportDiagnostics),
                "ReportDiagnostics should be included when generated code is enabled.");

            return;
        }

        Assert.Equal(
            Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags.None,
            result);
    }
}