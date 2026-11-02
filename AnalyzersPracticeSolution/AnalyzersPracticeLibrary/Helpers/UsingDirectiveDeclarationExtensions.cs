namespace AnalyzersPracticeLibrary.Helpers;
public static class UsingDirectiveDeclarationExtensions
{
    extension(UsingDirectiveSyntax node)
    {
        public Location? GetIdentifierLocation => node.Name?.GetLocation();
        public bool IsGlobal => node.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
        public bool IsStatic => node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
        public bool IsAlias => node.Alias is not null;
        public bool IsOrdinary => node.IsGlobal == false && node.IsStatic == false && node.IsAlias == false;
    }
}