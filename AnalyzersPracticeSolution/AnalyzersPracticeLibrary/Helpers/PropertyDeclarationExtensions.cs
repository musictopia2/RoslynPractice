namespace AnalyzersPracticeLibrary.Helpers;
public static class PropertyDeclarationExtensions
{
    extension(PropertyDeclarationSyntax node)
    {
        public string Name => node.Identifier.Text;
    }
}