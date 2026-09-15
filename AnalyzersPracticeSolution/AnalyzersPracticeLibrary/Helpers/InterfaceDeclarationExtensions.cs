namespace AnalyzersPracticeLibrary.Helpers;
public static class InterfaceDeclarationExtensions
{
    extension(InterfaceDeclarationSyntax node)
    {
        public int MethodCount => node.Members.OfType<MethodDeclarationSyntax>().Count();
        public int PropertyCount => node.Members.OfType<PropertyDeclarationSyntax>().Count();

    }
}