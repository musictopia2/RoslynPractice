using static AnalyzersPracticeLibrary.Helpers.DiagnosticDescriptorBuilder;

namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson04DiagnosticProperties.Exercise01;

public static class Extensions
{
    extension(ICategoryStep builder)
    {
        public DiagnosticDescriptor BuildWithDiagnosticPropertiesCategory(DiagnosticSeverity severity)
        {
            return builder.WithCategory("DiagnosticProperties").WithSeverity(severity).EnabledByDefault().Build();
        }
    }
}
