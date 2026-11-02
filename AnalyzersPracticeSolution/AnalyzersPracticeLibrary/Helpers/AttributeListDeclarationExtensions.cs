namespace AnalyzersPracticeLibrary.Helpers;
public static class AttributeListDeclarationExtensions
{
    extension(AttributeListSyntax node)
    {
        public string? TargetName => node.Target is null ? null : node.Target.Identifier.ValueText;
        public int AttributeCount => node.Attributes.Count;
        public Location GetIdentifierLocation => node.Target is null ? Location.None : node.Target.Identifier.GetLocation();

        public int NamedArgumentCount
        {
            get
            {
                int output = 0;
                if (node.TargetName is null || node.TargetName == "method")
                {
                    foreach (var seconds in node.Attributes)
                    {
                        if (seconds.ArgumentList is not null)
                        {
                            foreach (var argument in seconds.ArgumentList.Arguments)
                            {
                                if (argument.Name is not null)
                                {
                                    output++;
                                }
                            }
                        }
                    }
                }
                return output;
            }
        }
       
    }
}