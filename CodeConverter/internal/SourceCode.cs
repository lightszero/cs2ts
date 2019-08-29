using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeConverter
{
    class SourceCode
    {
        public string srcfile;
        public SyntaxTree syntaxTree;
        public SemanticModel semanticModel;
    }
}
