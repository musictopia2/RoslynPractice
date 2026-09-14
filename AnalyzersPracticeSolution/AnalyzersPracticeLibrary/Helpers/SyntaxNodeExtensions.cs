namespace AnalyzersPracticeLibrary.Helpers;
public static class SyntaxNodeExtensions
{
    extension(SyntaxNodeAnalysisContext context)
    {
        public ClassDeclarationSyntax ToClassSyntax => (ClassDeclarationSyntax)context.Node;
        public MethodDeclarationSyntax ToMethodSyntax => (MethodDeclarationSyntax)context.Node;
        public PropertyDeclarationSyntax ToPropertySyntax => (PropertyDeclarationSyntax)context.Node;
    }
}