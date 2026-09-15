namespace AnalyzersPracticeLibrary.Helpers;
public static class RecordDeclarationExtensions
{
    extension(RecordDeclarationSyntax node)
    {
        public int MethodCount => node.Members.OfType<MethodDeclarationSyntax>().Count();
        public int PropertyCount => node.Members.OfType<PropertyDeclarationSyntax>().Count();

        //well see what other things about this i will need in future.
    }
}