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
                if (node is BaseTypeDeclarationSyntax firsts)
                {
                    return firsts.Name;
                }
                return ""; //blank in all other cases (for now).
            }
        }
        public Location GetIdentifierLocation
        {
            get
            {
                if (node is BaseTypeDeclarationSyntax firsts)
                {
                    return firsts.GetIdentifierLocation;
                }
                if (node is PropertyDeclarationSyntax p)
                {
                    return p.GetIdentifierLocation;
                }
                if (node is MethodDeclarationSyntax m)
                {
                    return m.GetIdentifierLocation;
                }
                return Location.None;
            }
        }
        public bool IsPublic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
        public bool IsProtected => node.Modifiers.Any(x => x.IsKind(SyntaxKind.ProtectedKeyword));
        public bool IsInternal => node.Modifiers.Any(x => x.IsKind(SyntaxKind.InternalKeyword));
        public bool IsPrivate => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword));
        public bool IsStatic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
        public bool IsInstance => node.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)) == false;
        public bool IsConstant => node.Modifiers.Any(x => x.IsKind(SyntaxKind.ConstKeyword));
        public bool IsReadOnly => node.Modifiers.Any(x => x.IsKind(SyntaxKind.ReadOnlyKeyword));
        public string KindName
        {
            get
            {
                if (node is PropertyDeclarationSyntax)
                {
                    return "property";
                }
                if (node is ClassDeclarationSyntax)
                {
                    return "class";
                }
                if (node is MethodDeclarationSyntax)
                {
                    return "method";
                }
                return "None"; //for now until more are added.
            }
        }

        public string ExplicitAccessibilityText
        {
            get
            {
                if (node.IsPublic)
                {
                    return "public";
                }
                bool isInternal = node.IsInternal;
                bool isPrivate = node.IsPrivate;
                bool isProtected = node.IsProtected;
                if (isProtected && isPrivate)
                {
                    return "private protected";
                }
                if (isProtected && isInternal)
                {
                    return "protected internal";
                }
                if (isPrivate)
                {
                    return "private";
                }
                if (isProtected)
                {
                    return "protected";
                }
                if (isInternal)
                {
                    return "internal";
                }
                return "";
            }
        }


    }
}