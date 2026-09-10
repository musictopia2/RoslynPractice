/*
Enter the requirements for this exercise here.
Try this one yourself:

[Flags]
public enum EnumFileOptions
{
    None = 0,
    CanOpen = 1,
    CanSave = 2,
    CanPrint = 4
}

Write:

public static EnumFileOptions GetFileOptions()

with this requirement:

Return a single EnumFileOptions value that contains both CanOpen and CanPrint, but does not contain CanSave.
*/

using AnalyzersPracticeLibrary.Helpers;

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson01CreatingMinimalDiagnosticAnalyzer.Exercise02;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MainClass : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            return [];
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        //List<GeneratedCodeAnalysisFlags> options =
        //    [
        //    GeneratedCodeAnalysisFlags.Analyze,
        //    GeneratedCodeAnalysisFlags.ReportDiagnostics
        //    ];
        //FlagsBuilder<GeneratedCodeAnalysisFlags> flags = new(options);

        //context.ConfigureGeneratedCodeAnalysis(flags.GetValue());

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze.CombineFlags(GeneratedCodeAnalysisFlags.ReportDiagnostics));

        context.EnableConcurrentExecution();
    }


    public static EnumFileOptions GetFileOptions()
    {
        EnumFileOptions options = EnumFileOptions.CanOpen.CombineFlags(EnumFileOptions.CanPrint);
        return options;
        //List<EnumFileOptions> options =
        //    [
        //    EnumFileOptions.CanOpen,
        //    EnumFileOptions.CanPrint
        //    ];
        //FlagsBuilder<EnumFileOptions> list = new(options);
        //return list.GetValue();
        //return EnumFileOptions.CanOpen | EnumFileOptions.CanPrint;
    }

    //i did have my custom bit operator one.  not sure if i can use here.



}