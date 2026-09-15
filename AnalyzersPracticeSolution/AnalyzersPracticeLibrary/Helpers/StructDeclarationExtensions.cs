namespace AnalyzersPracticeLibrary.Helpers;
public static class StructDeclarationExtensions
{

    extension(StructDeclarationSyntax node)
    {
        public string Name => node.Identifier.Text;
        public int MethodCount => node.Members.OfType<MethodDeclarationSyntax>().Count();
        public int PropertyCount => node.Members.OfType<PropertyDeclarationSyntax>().Count();
        public Location GetIdentifierLocation => node.Identifier.GetLocation();
        public bool IsPublic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));

        //well see what other things about this i will need in future.
    }
}