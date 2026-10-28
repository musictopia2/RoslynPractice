namespace AnalyzersPracticeLibrary.Helpers;

public static class MethodDeclarationExtensions
{
    extension(MethodDeclarationSyntax node)
    {
        public int TypeParameterCount => node.TypeParameterList is null ? 0 : node.TypeParameterList.Parameters.Count;


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

        //public string Name => node.Identifier.Text;
    }
}