namespace AnalyzersPracticeLibrary.Helpers;
public static class PropertyDeclarationExtensions
{
    extension(PropertyDeclarationSyntax node)
    {
        public bool HasInitializer => node.Initializer is not null;
        public bool HasAccessor => node.AccessorList is not null;
        public bool HasInitAccessor =>
            node.AccessorList?.Accessors.Any(x =>
            x.IsKind(SyntaxKind.InitAccessorDeclaration)) == true;
        //public bool HasAccessors => node.AccessorList is not null;
        //public bool HasPrivateSet =>
        //    node.AccessorList?.Accessors.Any(x =>
        //        x.IsKind(SyntaxKind.SetAccessorDeclaration) &&
        //        x.Modifiers.Any(SyntaxKind.PrivateKeyword)) == true;

        //public bool HasProtectedSet =>
        //    node.AccessorList?.Accessors.Any(x =>
        //        x.IsKind(SyntaxKind.SetAccessorDeclaration) &&
        //        x.Modifiers.Any(SyntaxKind.ProtectedKeyword)) == true;
        //public bool HasInternalSet =>
        //    node.AccessorList?.Accessors.Any(x =>
        //        x.IsKind(SyntaxKind.SetAccessorDeclaration) &&
        //        x.Modifiers.Any(SyntaxKind.InternalKeyword)) == true;

        //public bool HasInitSet =>
        //    node.AccessorList?.Accessors.Any(x =>
        //        x.IsKind(SyntaxKind.SetAccessorDeclaration) &&
        //        x.Modifiers.Any(SyntaxKind.InitKeyword)) == true;


        public bool HasUnrestrictedSetter =>
            node.AccessorList?.Accessors.Any(x =>
            x.IsKind(SyntaxKind.SetAccessorDeclaration) &&
            x.Modifiers.Count == 0) == true;

    }
}