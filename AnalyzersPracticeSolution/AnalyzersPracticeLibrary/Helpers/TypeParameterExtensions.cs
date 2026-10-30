namespace AnalyzersPracticeLibrary.Helpers;
public static class TypeParameterExtensions
{
    extension(TypeParameterSyntax node)
    {
        public string Name => node.Identifier.ValueText;
        public Location GetIdentifierLocation => node.Identifier.GetLocation();
    }
}