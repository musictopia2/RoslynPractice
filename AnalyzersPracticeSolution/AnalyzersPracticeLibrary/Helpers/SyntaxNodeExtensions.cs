using System.Xml.Linq;

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
        public TypeDeclarationSyntax ToTypeDeclaration => (TypeDeclarationSyntax)context.Node;
        //the above is needed just in case you need to know 
        public BaseTypeDeclarationSyntax ToBaseTypeDeclaration => (BaseTypeDeclarationSyntax)context.Node;
        public BaseMethodDeclarationSyntax ToBaseMethodDeclaration => (BaseMethodDeclarationSyntax)context.Node;
        

        public EnumDeclarationSyntax ToEnumSyntax => (EnumDeclarationSyntax)context.Node;
        public FieldDeclarationSyntax ToFieldSyntax => (FieldDeclarationSyntax)context.Node;
        public IndexerDeclarationSyntax ToIndexSyntax => (IndexerDeclarationSyntax)context.Node;
        public BaseNamespaceDeclarationSyntax ToBaseNamespaceSyntax => (BaseNamespaceDeclarationSyntax) context.Node;
        public UsingDirectiveSyntax ToUsingDirectiveSyntax => (UsingDirectiveSyntax)context.Node;
        public CompilationUnitSyntax ToCompilationUnitSyntax => (CompilationUnitSyntax)context.Node;
        public AttributeSyntax ToAttributeSyntax => (AttributeSyntax)context.Node;
        public AttributeArgumentSyntax ToAttributeArgumentSyntax => (AttributeArgumentSyntax)context.Node;
        public AttributeListSyntax ToAttributeListSyntax => (AttributeListSyntax)context.Node;
    }
}