using static AnalyzersPracticeLibrary.Helpers.DiagnosticDescriptorBuilder;
namespace AnalyzersPracticeLibrary.Section02WorkingWithDiagnostics.Lesson05Review.Exercise01;
public static class Extensions
{
    extension(ICategoryStep builder)
    {
        //decide what this will be.
        public DiagnosticDescriptor BuildWithDiagnosticReviewCategory(DiagnosticSeverity severity)
        {
            return builder.WithCategory("DiagnosticReview").WithSeverity(severity).EnabledByDefault().Build();
        }
    }
}