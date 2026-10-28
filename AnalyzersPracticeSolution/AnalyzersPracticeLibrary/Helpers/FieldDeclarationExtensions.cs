using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzersPracticeLibrary.Helpers;

public static class FieldDeclarationExtensions
{
    extension(FieldDeclarationSyntax node)
    {
        public List<VariableDeclaratorSyntax> GetVariables =>
        node.Declaration.Variables.ToList();
    }
}