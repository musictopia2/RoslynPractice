namespace AnalyzersPracticeLibrary.Helpers;
public static class BaseTypeDeclarationExtensions
{
    extension(BaseTypeDeclarationSyntax node)
    {
        public string Name => node.Identifier.Text;
        public bool HasMembers => node is TypeDeclarationSyntax;
        public Location GetIdentifierLocation => node.Identifier.GetLocation();
        public TypeDeclarationSyntax ToTypeDeclaration => (TypeDeclarationSyntax)node;
        public bool IsPublic => node.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
        public bool IsRecordStruct => node.IsKind(SyntaxKind.RecordStructDeclaration);
        public bool IsRecordClass => node.IsKind(SyntaxKind.RecordDeclaration);
        public string KindName
        {
            get
            {
                if (node.IsRecordClass)
                {
                    return "Record Class";
                }
                if (node.IsRecordStruct)
                {
                    return "Record Struct";
                }
                if (node.IsKind(SyntaxKind.InterfaceDeclaration))
                {
                    return "Interface";
                }
                if (node.IsKind(SyntaxKind.ClassDeclaration))
                {
                    return "Class";
                }
                if (node.IsKind(SyntaxKind.StructDeclaration))
                {
                    return "Struct";
                }
                return "None"; //can't raise errors
            }
        }

        //well see what other things about this i will need in future.
    }
}
