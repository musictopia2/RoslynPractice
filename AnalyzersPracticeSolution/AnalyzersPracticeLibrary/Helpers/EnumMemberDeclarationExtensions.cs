namespace AnalyzersPracticeLibrary.Helpers;
public static class EnumMemberDeclarationExtensions
{
    extension(EnumMemberDeclarationSyntax node)
    {
        public string Name => node.Identifier.Text;
        public bool HasExplicitValue => node.EqualsValue is not null;
        public string ExplicitValue => node.EqualsValue!.Value.ToString(); //you are on your own here (should use other first)
        public Location GetIdentifierLocation => node.Identifier.GetLocation();
    }
}