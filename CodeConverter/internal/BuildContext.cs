using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeConverter
{
    class BuildContext
    {

        public StringBuilder tscodeBuilder = new StringBuilder();
        public string TypeScriptCode
        {
            get
            {
                return tscodeBuilder.ToString();
            }
        }
        public void Append(params string[] text)
        {
            foreach (var t in text)
                tscodeBuilder.Append(t);
        }
        public void AppendLine(string space, params string[] text)
        {
            tscodeBuilder.Append(space);

            foreach (var t in text)
                tscodeBuilder.Append(t);
            tscodeBuilder.AppendLine();
        }
        public void AppendLine()
        {
            tscodeBuilder.AppendLine();
        }
        public void AppendTriviaClean(IEnumerable<SyntaxTrivia> trivias)
        {
            foreach (var t in trivias)
            {
                var str = t.ToFullString();
                if (string.IsNullOrWhiteSpace(str))
                {
                    continue;
                }
                if (str == "\r\n")
                    continue;
                else
                    tscodeBuilder.Append(str);
            }

        }
    }
}
