using static AnalyzersPracticeLibrary.Helpers.DiagnosticDescriptorBuilder;

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson04CreateReportFirstDiagnostic.Exercise05;

public static class Extensions
{
    extension(ICategoryStep builder)
    {
        public DiagnosticDescriptor BuildWithValidationCategory(DiagnosticSeverity severity)
        {
            return builder.WithCategory("Validation").WithSeverity(severity).EnabledByDefault().Build();
        }
    }
}
