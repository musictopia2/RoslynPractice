namespace AnalyzersPracticeLibrary.Helpers;
public static class DiagnosticExtensions
{
    extension(CompilationAnalysisContext context)
    {
        public void ReportSeveralReportDiagnosticsWithNone(params List<DiagnosticDescriptor> list)
        {
            foreach (var item in list)
            {
                Diagnostic d = Diagnostic.Create(item, Location.None);
                context.ReportDiagnostic(d);
            }
        }
    }
}
