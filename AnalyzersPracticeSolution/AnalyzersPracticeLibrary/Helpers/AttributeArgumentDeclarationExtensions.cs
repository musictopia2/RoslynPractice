namespace AnalyzersPracticeLibrary.Helpers;
public static class AttributeArgumentDeclarationExtensions
{
    extension(AttributeArgumentSyntax node)
    {
        public string? Name
        {
            get
            {
                if (node.NameColon is not null)
                {
                    return node.NameColon.Name.ToString();
                }
                if (node.NameEquals is not null)
                {
                    return node.NameEquals.Name.ToString();
                }
                return null;
            }
        }
        public string? Form
        {
            get
            {
                if (node.NameColon is not null)
                {
                    return "colon";
                }
                if (node.NameEquals is not null)
                {
                    return "equals";
                }
                return null;
            }
        }
        public Location GetIdentifierLocation
        {
            get
            {
                if (node.NameColon is not null)
                {
                    return node.NameColon.Name.GetLocation();
                }
                if (node.NameEquals is not null)
                {
                    return node.NameEquals.Name.GetLocation();
                }
                return Location.None;
            }
        }
    }

}