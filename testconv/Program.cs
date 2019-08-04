using System;

namespace testconv
{
    class Program
    {
        static string srccode = @"using System;//dd
                       using System.Collections.Generic;
                       using System.Text;
                       // help me
                       namespace HelloWorld
                       {
                           public class A
{
    public static void WriteLine(string a)
   {
Console.WriteLine(a);
    }
}
                           class Program
                           {
                               static void Main(params string[] args)
                               {
                                   int i=5;//123
                                   var ss = i.ToString();
                                   A.WriteLine(""Hello, World!"");
/// hhh;
                               }
                           }
                       }";
        static void Main(string[] args)
        {
            Console.WriteLine("====make single test.");
            CodeConverter.Converter conv1 = new CodeConverter.Converter();
            conv1.AddSingleFile(srccode);
            conv1.Convert();
            conv1.DumpResult();

            Console.WriteLine("====Press Enter to continue.");
            Console.ReadLine();

            var srcproj = System.IO.Path.GetFullPath("../../../../testproj_helloha/testproj_helloha.csproj");
            Console.WriteLine("make csproj test." + srcproj);
            CodeConverter.Converter conv2 = new CodeConverter.Converter();
            conv2.AddProject(srcproj);
            conv2.Convert();
            conv2.DumpResult();

            Console.WriteLine("====Press Enter to exit.");
            Console.ReadLine();

        }
    }
}
