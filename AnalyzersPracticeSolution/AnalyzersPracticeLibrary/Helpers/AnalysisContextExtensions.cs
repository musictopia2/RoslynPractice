namespace AnalyzersPracticeLibrary.Helpers;
public static class AnalysisContextExtensions
{
    extension(AnalysisContext context)
    {
        public void RegisterDiagnosticRulesWithNoneLocations(params List<DiagnosticDescriptor> list)
        {
            context.RegisterCompilationAction(action => action.ReportSeveralReportDiagnosticsWithNone(list));
        }
    }
}
