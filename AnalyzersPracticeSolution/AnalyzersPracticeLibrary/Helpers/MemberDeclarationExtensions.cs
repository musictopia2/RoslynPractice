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
                    return p.Identifier.ValueText;
                }
                if (node is MethodDeclarationSyntax m)
                {
                    return m.Identifier.ValueText;
                }
                return ""; //blank in all other cases (for now).
            }
        }
        public bool IsPublic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
        public bool IsStatic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
        public bool IsConstant => node.Modifiers.Any(x => x.IsKind(SyntaxKind.ConstKeyword));
        public bool IsReadOnly => node.Modifiers.Any(x => x.IsKind(SyntaxKind.ReadOnlyKeyword));

    }
}