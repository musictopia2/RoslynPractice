namespace AnalyzersPracticeLibrary.Helpers;
public static class ParameterExtensions
{
    extension(ParameterSyntax node)
    {
        public string Name => node.Identifier.ValueText;
        public Location GetIdentifierLocation => node.Identifier.GetLocation();

        public bool HasKind(SyntaxKind kind) => node.Modifiers.Any(x => x.IsKind(kind));

        public string? ReferenceModifierText
        {
            get
            {
                if (node.Modifiers.Count == 0)
                {
                    return null;
                }
                if (node.Modifiers.Any(x => x.IsKind(SyntaxKind.InKeyword)))
                {
                    return "in";
                }
                if (node.Modifiers.Any(x => x.IsKind(SyntaxKind.OutKeyword)))
                {
                    return "out";
                }
                if (node.Modifiers.Any(x => x.IsKind(SyntaxKind.RefKeyword)))
                {
                    return "ref";
                }
                return null;
            }
        }
        public string? ModifierAllText
        {
            get
            {
                if (node.Modifiers.Count == 0)
                {
                    return null;
                }
                var list = node.Modifiers.Select(x => x.ValueText).ToList();
                return string.Join(" ", list);
            }
        }
    }
}