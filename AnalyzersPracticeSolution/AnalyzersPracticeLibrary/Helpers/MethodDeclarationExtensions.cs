namespace AnalyzersPracticeLibrary.Helpers;
public static class MethodDeclarationExtensions
{
    extension(MethodDeclarationSyntax node)
    {
        public int TypeParameterCount => node.TypeParameterList is null ? 0 : node.TypeParameterList.Parameters.Count;

        public List<ParameterSyntax> GetAllParameters() => node.ParameterList.Parameters.ToList();

        public List<ParameterSyntax> GetFilteredParameters(Func<ParameterSyntax, bool> predicate) => node.ParameterList.Parameters.Where(predicate).ToList();

        public List<TypeParameterSyntax> GetAllTypeParameters() => node.TypeParameterList is null ? [] : node.TypeParameterList.Parameters.ToList();

        public List<TypeParameterSyntax> GetUnconstrainedTypes()
        {
            var realNames = node.ConstraintClauseNames;
            var firsts = node.GetAllTypeParameters();
            if (firsts.Count == realNames.Count)
            {
                return [];
            }
            List<TypeParameterSyntax> output = [];
            foreach (var item in firsts)
            {
                if (realNames.Contains(item.Name) == false)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        public string TypeParameterNames
        {
            get
            {
                if (node.TypeParameterList is null)
                {
                    return "";
                }
                List<string> names = node.TypeParameterList.Parameters
                .Select(x => x.Identifier.ValueText)
                .ToList();
                return string.Join(", ", names);
            }
        }
        public List<string> ConstraintClauseNames => node.ConstraintClauses.Select(x => x.Name.Identifier.ValueText).ToList();
    }
}