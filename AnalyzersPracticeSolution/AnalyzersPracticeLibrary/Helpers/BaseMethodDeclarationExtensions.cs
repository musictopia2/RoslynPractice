namespace AnalyzersPracticeLibrary.Helpers;
public static class BaseMethodDeclarationExtensions
{
    extension(BaseMethodDeclarationSyntax node)
    {
        
        public TypeDeclarationSyntax? GetFirstContainingType
        {
            get
            {
                TypeDeclarationSyntax? containingType = node.Ancestors()
                    .OfType<TypeDeclarationSyntax>()
                    .FirstOrDefault();
                return containingType;
            }
        }
        public Location GetIdentifierLocation
        {
            get
            {
                if (node is OperatorDeclarationSyntax o)
                {
                    return o.OperatorToken.GetLocation();
                }
                if (node is ConversionOperatorDeclarationSyntax c1)
                {
                    return c1.ImplicitOrExplicitKeyword.GetLocation();
                }
                if (node is MethodDeclarationSyntax m)
                {
                    return m.Identifier.GetLocation();
                }
                if (node is ConstructorDeclarationSyntax c2)
                {
                    return c2.Identifier.GetLocation();
                }
                if (node is DestructorDeclarationSyntax d)
                {
                    return d.Identifier.GetLocation();
                }
                return Location.None;
            }
        }
        public int ParameterCount
        {
            get
            {
                return node.ParameterList.Parameters.Count;
            }
        }
        public string ParameterNames
        {
            get
            {
                List<string> names = node.ParameterList.Parameters
                .Select(x => x.Identifier.ValueText)
                .ToList();
                return string.Join(", ", names);
            }
        }
        public string Name
        {
            get
            {
                if (node is OperatorDeclarationSyntax o)
                {
                    return o.OperatorToken.Text;
                }
                if (node is ConversionOperatorDeclarationSyntax c1)
                {
                    return c1.ImplicitOrExplicitKeyword.Text;
                }
                if (node is MethodDeclarationSyntax m)
                {
                    return m.Identifier.ValueText;
                }
                if (node is ConstructorDeclarationSyntax c2)
                {
                    return c2.Identifier.ValueText;
                }
                if (node is DestructorDeclarationSyntax d)
                {
                    return d.Identifier.ValueText;
                }
                return "None";
            }
        }
        public string KindName
        {
            get
            {
                if (node is MethodDeclarationSyntax)
                {
                    return "Method";
                }
                if (node is ConstructorDeclarationSyntax)
                {
                    return "Constructor";
                }
                if (node is DestructorDeclarationSyntax)
                {
                    return "Destructor";
                }
                if (node is OperatorDeclarationSyntax)
                {
                    return "Operator";
                }
                if (node is ConversionOperatorDeclarationSyntax)
                {
                    return "Conversion Operator";
                }
                return "None"; //can't raise errors
            }
        }
    }
}