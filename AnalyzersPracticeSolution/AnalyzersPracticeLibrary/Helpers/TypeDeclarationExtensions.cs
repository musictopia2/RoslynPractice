namespace AnalyzersPracticeLibrary.Helpers;
public static class TypeDeclarationExtensions
{
    extension(TypeDeclarationSyntax node)
    {
        public int MethodCount => node.Members.OfType<MethodDeclarationSyntax>().Count();
        public int PropertyCount => node.Members.OfType<PropertyDeclarationSyntax>().Count();

        public int FieldCount => node.Members.OfType<FieldDeclarationSyntax>().Count();


        public List<MethodDeclarationSyntax> GetAllMethods => node.Members.OfType<MethodDeclarationSyntax>().ToList();

        public List<MethodDeclarationSyntax> GetFilteredMethods(Func<MethodDeclarationSyntax, bool> c) => node.Members.OfType<MethodDeclarationSyntax>().Where(c).ToList();

        //well see what other things about this i will need in future.
    }
}
