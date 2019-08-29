using System;
using System.Collections.Generic;
using System.Text;

namespace helloha
{
    public class ShowMTable
    {
        public void Show()
        {
            for (var i = 0; i < 9; i++)
            {
                string line = "";
                for (var j = 0; j < 9; j++)
                {
                    var result = (i * j).ToString();
                    if (result.Length == 1)
                        result += "  ";
                    else
                        result += " ";
                    line += (i + "*" + j + "=" + result);
                }
                line += "\n";
                Console.WriteLine(line);
            }
        }
    }
}
