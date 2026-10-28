namespace AnalyzersPracticeLibrary.Helpers;
public static class ClassDeclarationExtensions
{
    extension(ClassDeclarationSyntax node)
    {
        public List<IndexerDeclarationSyntax> GetAllIndexers => node.Members.OfType<IndexerDeclarationSyntax>().ToList();
        public List<VariableDeclaratorSyntax> GetAllFieldVariables
        {
            get
            {
                List<VariableDeclaratorSyntax> output = [];

                var firsts = node.Members.OfType<FieldDeclarationSyntax>().ToList();
                foreach (var item in firsts)
                {
                    output.AddRange(item.GetVariables);
                }
                return output;
            }
        }
        public List<FieldDeclarationSyntax> GetAllFieldDeclarations =>
            node.Members.OfType<FieldDeclarationSyntax>().ToList();
        public List<PropertyDeclarationSyntax> GetAllProperties => node.Members.OfType<PropertyDeclarationSyntax>().ToList();
    }
}