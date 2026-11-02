namespace AnalyzersPracticeLibrary.Helpers;
public static class BaseNamespaceDeclarationExtensions
{
    extension (BaseNamespaceDeclarationSyntax node)
    {
        public bool IsFileScoped => node is FileScopedNamespaceDeclarationSyntax;
        public string DeclarationForm
        {
            get
            {
                if (node.IsFileScoped)
                {
                    return "file-scoped";
                }
                return "block-scoped";
            }
        }
        public Location GetIdentifierLocation => node.Name.GetLocation();

        public List<UsingDirectiveSyntax> GetUsings => node.Usings.ToList();

    }
}