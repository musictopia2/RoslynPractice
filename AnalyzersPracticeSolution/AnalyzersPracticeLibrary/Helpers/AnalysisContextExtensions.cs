namespace AnalyzersPracticeLibrary.Helpers;
public static class AnalysisContextExtensions
{
    extension(AnalysisContext context)
    {
        public void RegisterDiagnosticRulesWithNoneLocations(params List<DiagnosticDescriptor> list)
        {
            context.RegisterCompilationAction(action => action.ReportSeveralReportDiagnosticsWithNone(list));
        }

        private Location? GetPrimaryLocation(Compilation compilation,int index, int needed)
        {
            if (compilation.SyntaxTrees.Count() < needed)
            {
                return null;
            }
            var tree = compilation.SyntaxTrees.ElementAt(index);
            var node = tree.GetRoot();
            return node.GetLocation();
        }
        private List<Location> GetSecondaryLocations(Compilation compilation, List<int> others)
        {
            List<Location> output = [];
            foreach (var item in others)
            {
                var tree = compilation.SyntaxTrees.ElementAt(item);
                var node = tree.GetRoot();

                output.Add(node.GetLocation());
            }
            return output;
        }
        public void RegisterDiagnosticRuleWithSeveralLocations(DiagnosticDescriptor rule,
            int firstSyntaxIndex, params List<int> others)            
        {
            var tempList = others.ToList();
            tempList.Add(firstSyntaxIndex);
            int needed = tempList.Max();
            needed++; //because 0 based.
            context.RegisterCompilationAction(action =>
            {
                Location? primary = GetPrimaryLocation(context, action.Compilation, firstSyntaxIndex, needed);
                if (primary is null)
                {
                    return;
                }
                List<Location> secondaryLocations = GetSecondaryLocations(context, action.Compilation, others);
                Diagnostic d = Diagnostic.Create(rule, primary, secondaryLocations);
                action.ReportDiagnostic(d);
            });
        }
    }
}