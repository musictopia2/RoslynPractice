namespace AnalyzersPracticeLibrary.Helpers;

public static class MethodDeclarationExtensions
{
    extension(MethodDeclarationSyntax node)
    {
        public string Name => node.Identifier.Text;
    }
}