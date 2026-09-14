using static AnalyzersPracticeLibrary.Helpers.DiagnosticDescriptorBuilder;
namespace AnalyzersPracticeLibrary.Section03SyntaxBasedAnalysis.Lesson01SyntaxNodesAndKind.Exercise01;
public static class Extensions
{
    extension(ICategoryStep builder)
    {
        //decide what this will be.
        public DiagnosticDescriptor BuildWithSyntaxAnalysisCategory(DiagnosticSeverity severity)
        {
            return builder.WithCategory("SyntaxAnalysis").WithSeverity(severity).EnabledByDefault().Build();
        }
    }
}