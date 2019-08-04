using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeConverter
{
    public class Converter
    {
        public class MyDoc
        {
            public string srcfile;
            public SyntaxTree syntaxTree;
            public SemanticModel semanticModel;
        }
        List<MyDoc> docs = new List<MyDoc>();
        public void AddBuildedDocument(string srcfilepath, SemanticModel model)
        {
            docs.Add(new MyDoc() { srcfile = srcfilepath, syntaxTree = model.SyntaxTree, semanticModel = model });
        }
        public void AddSingleFile(string code, string fielname = "default.cs")
        {
            var syntax = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("HelloWorld").AddReferences(
                                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                    MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location)
                                    );//只要引用了就找得到
            compilation = compilation.AddSyntaxTrees(syntax);
            var semanticmodel = compilation.GetSemanticModel(syntax);
            this.AddBuildedDocument(fielname, semanticmodel);
        }
        public void AddProject(string projfile)
        {
            AnalyzerManager manager = new AnalyzerManager();
            ProjectAnalyzer analyzer = manager.GetProject(projfile);
            var project = analyzer.GetWorkspace(true).CurrentSolution.Projects.First();
            foreach (var f in project.Documents)
            {
                var result = f.GetSemanticModelAsync();
                result.Wait();
                this.AddBuildedDocument(f.FilePath, result.Result);
            }
        }
        public void AddSolution(string slnfile)
        {

        }
        public Dictionary<string, string> result
        {
            get;
            private set;
        }
        public void Convert()
        {
            result = new Dictionary<string, string>();
            foreach (var f in docs)
            {
                BuildContext buildContext = new BuildContext();
                CodeBuilder.Build(buildContext, f);
                result[f.srcfile] = buildContext.TypeScriptCode;

                //Console.WriteLine("dumpfile:" + f.srcfile);
                //DumpSynataxNode(f.semanticModel, f.syntaxTree.GetRoot(), 0);
            }
        }
        public void DumpResult()
        {
            foreach(var r in result)
            {
                Console.WriteLine("===dumpfile:" + r.Key);
                Console.WriteLine(r.Value);
            }
        }
        static void DumpSynataxNode(SemanticModel model, Microsoft.CodeAnalysis.SyntaxNode node, int space)
        {
            string spacestr = "";
            for (var i = 0; i < space; i++)
                spacestr += "    ";
            Console.WriteLine(spacestr + "<" + node.GetType().Name + ">");
            node.GetAnnotatedTrivia();
            node.GetLeadingTrivia();
            node.GetTrailingTrivia();
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax;
                foreach (var _using in unit.Usings)
                {
                    Console.WriteLine(spacestr + "CompilationUnitSyntax:using:" + _using.Name);
                }
                foreach (var _member in unit.Members)
                {
                    DumpSynataxNode(model, _member, space + 1);
                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax;
                Console.WriteLine(spacestr + "namespace:" + unit.Name);
                foreach (var _member in unit.Members)
                {
                    DumpSynataxNode(model, _member, space + 1);
                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
                Console.WriteLine(spacestr + "class:" + unit.Identifier.ValueText);
                foreach (var _member in unit.Members)
                {
                    DumpSynataxNode(model, _member, space + 1);
                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)
            {

                Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;
                Console.WriteLine(spacestr + "method:" + unit.Identifier.ValueText);
                DumpSynataxNode(model, unit.Body, space + 1);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax;
                Console.WriteLine(spacestr + "{");
                foreach (var _member in unit.Statements)
                {
                    DumpSynataxNode(model, _member, space + 1);
                }
                Console.WriteLine(spacestr + "}");
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax;
                DumpSynataxNode(model, unit.Expression, space + 1);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;

                DumpSynataxNode(model, unit.Expression, space + 1);

                DumpSynataxNode(model, unit.ArgumentList, space + 1);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax;
                var type = model.GetTypeInfo(unit);
                var r = model.GetSymbolInfo(unit);
                var t = model.GetDeclaredSymbol(unit);
                var mg = model.GetMemberGroup(unit);
                DumpSynataxNode(model, unit.Expression, space + 1);
                Console.WriteLine(spacestr + "    .");
                DumpSynataxNode(model, unit.Name, space + 1);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
                var type = model.GetTypeInfo(unit);
                var strtype = type.Type == null ? "" : type.ConvertedType.ToDisplayString();
                Console.WriteLine(spacestr + unit.Identifier.ValueText + "<" + strtype + ">");
                if (strtype == "?")
                {
                    //找不到类型，怎么办？
                    var name = unit.Identifier.ValueText;
                    var pos = unit.FullSpan.Start;
                    var allnamespaces = model.LookupNamespacesAndTypes(pos);
                    foreach (var n in allnamespaces)
                    {
                        if (n.Kind == SymbolKind.Namespace)
                        {

                            var newname = n.Name + "." + name;
                            var rr = model.LookupNamespacesAndTypes(pos, null, newname);
                            if (rr != null)
                            {

                            }
                        }
                    }

                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax;
                Console.WriteLine(spacestr + unit.OpenParenToken.ValueText);
                foreach (var _member in unit.Arguments)
                {
                    DumpSynataxNode(model, _member, space + 1);
                }
                Console.WriteLine(spacestr + unit.CloseParenToken.ValueText);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax;
                DumpSynataxNode(model, unit.Expression, space + 1);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax;
                Console.WriteLine(spacestr + unit.Token.Text);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax;
                DumpSynataxNode(model, unit.Declaration, space + 1);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax;
                DumpSynataxNode(model, unit.Type, space + 1);
                foreach (var v in unit.Variables)
                {
                    DumpSynataxNode(model, v, space + 1);
                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax;

                Console.WriteLine(spacestr + unit.Identifier.ValueText);
                if (unit.ArgumentList != null)
                    DumpSynataxNode(model, unit.ArgumentList, space + 1);
                if (unit.Initializer != null)
                    DumpSynataxNode(model, unit.Initializer, space + 1);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax;
                Console.WriteLine(spacestr + unit.EqualsToken.ValueText);
                DumpSynataxNode(model, unit.Value, space + 1);
            }

            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax;
                Console.WriteLine(spacestr + unit.Keyword.ValueText);
            }


            else
            {
                Console.WriteLine("not parse type!!!");
            }
            // Console.WriteLine(node.Span)
        }

    }
}
