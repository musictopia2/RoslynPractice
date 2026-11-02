using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzersPracticeLibrary.Helpers;

public static class AttributeDeclarationExtensions
{
    extension (AttributeSyntax node)
    {
        public int ArgumentCount => node.ArgumentList is null ? 0 : node.ArgumentList.Arguments.Count;
        public Location GetIdentifierLocation => node.Name.GetLocation();
        public bool HasNamedArguments => node.ArgumentList is null ? false : node.ArgumentList.Arguments.Any(x => x.Name is not null);
    }
}