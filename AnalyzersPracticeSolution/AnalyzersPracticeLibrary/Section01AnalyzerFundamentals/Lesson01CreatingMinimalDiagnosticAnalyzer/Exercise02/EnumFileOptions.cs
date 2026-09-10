namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02;

[Flags]
public enum EnumFileOptions
{
    None = 0,
    CanOpen = 1,
    CanSave = 2,
    CanPrint = 4
}