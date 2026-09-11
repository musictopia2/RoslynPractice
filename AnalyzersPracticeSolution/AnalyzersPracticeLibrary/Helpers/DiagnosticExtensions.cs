namespace AnalyzersPracticeLibrary.Helpers;
public static class DiagnosticExtensions
{
    extension(CompilationAnalysisContext context)
    {
        public void ReportDiagnosticWithNoneLocation(DiagnosticDescriptor item, params object[] messageArguments)
        {
            Diagnostic d = Diagnostic.Create(item, Location.None, messageArguments);
            context.ReportDiagnostic(d);
        }
        public void ReportSeveralReportDiagnosticsWithNone(params List<DiagnosticDescriptor> list)
        {
            foreach (var item in list)
            {
                Diagnostic d = Diagnostic.Create(item, Location.None);
                context.ReportDiagnostic(d);
            }
        }
        public void ReportSeveralReportDiagnosticsWithNone(string name, params List<DiagnosticDescriptor> list)
        {
            foreach (var item in list)
            {
                Diagnostic d = Diagnostic.Create(item, Location.None);
                context.ReportDiagnostic(d);
            }
        }
    }
}
