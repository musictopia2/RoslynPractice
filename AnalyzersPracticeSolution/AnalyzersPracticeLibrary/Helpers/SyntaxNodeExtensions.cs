namespace AnalyzersPracticeLibrary.Helpers;
public static class SyntaxNodeExtensions
{
    extension(SyntaxNodeAnalysisContext context)
    {
        public ClassDeclarationSyntax ToClassSyntax => (ClassDeclarationSyntax)context.Node;
        public MethodDeclarationSyntax ToMethodSyntax => (MethodDeclarationSyntax)context.Node;
        public PropertyDeclarationSyntax ToPropertySyntax => (PropertyDeclarationSyntax)context.Node;
        public StructDeclarationSyntax ToStructSyntax => (StructDeclarationSyntax)context.Node;
        public InterfaceDeclarationSyntax ToInterfaceSyntax => (InterfaceDeclarationSyntax)context.Node;
        public RecordDeclarationSyntax ToRecordSyntax => (RecordDeclarationSyntax)context.Node;
        //the above is needed just in case you need to know 
        public BaseTypeDeclarationSyntax ToBaseTypeDeclaration => (BaseTypeDeclarationSyntax)context.Node;

    }
}