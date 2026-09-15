namespace AnalyzersPracticeLibrary.Helpers;
public static class PropertyDeclarationExtensions
{
    extension(PropertyDeclarationSyntax node)
    {
        public string Name => node.Identifier.Text;
        public bool IsPublic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
    }
}