namespace AnalyzersPracticeLibrary.Helpers;
public static class MemberDeclarationExtensions
{
    extension(MemberDeclarationSyntax node)
    {
        public string Name
        {
            get
            {
                if (node is FieldDeclarationSyntax f)
                {
                    return f.Declaration.Variables.First().Identifier.Text;
                }
                if (node is PropertyDeclarationSyntax p)
                {
                    return p.Identifier.Text;
                }
                if (node is MethodDeclarationSyntax m)
                {
                    return m.Identifier.Text;
                }
                return ""; //blank in all other cases (for now).
            }
        }
        public bool IsPublic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
    }
}