namespace AnalyzersPracticeLibrary.Helpers;
public static class IndexerDeclarationExtensions
{
    extension(IndexerDeclarationSyntax node)
    {
        public int ParameterCount => node.ParameterList.Parameters.Count();
        public Location GetIdentifierLocation => node.ThisKeyword.GetLocation();
    }
}