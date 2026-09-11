using static AnalyzersPracticeLibrary.Helpers.DiagnosticDescriptorBuilder;

namespace AnalyzersPracticeLibrary.Section01AnalyzerFundamentals.Lesson05Review.Exercise05;

public static class Extensions
{
    extension(ICategoryStep builder)
    {
        public DiagnosticDescriptor BuildWithClientReviewCategory(DiagnosticSeverity severity)
        {
            return builder.WithCategory("ClientReview").WithSeverity(severity).EnabledByDefault().Build();
        }
    }
}
