using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    }
    class CodeBuilder
    {
        public static void Build(BuildContext builder, CodeConverter.Converter.MyDoc document)
        {
            BuildNode(builder, document.semanticModel, document.syntaxTree.GetRoot(), 0); ;
        }
        static void BuildExpression(BuildContext builder, SemanticModel model, ExpressionSyntax expression)

        {
            var lt = expression.GetLeadingTrivia();

            //前置注释
            if (lt.Count > 0)
            {
                builder.AppendLine(lt.ToFullString());
            }

            if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)
            {
                BuildExpression_Invocation(builder, model, expression);
            }
            else if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)
            {
                BuildExpression_MemberAccess(builder, model, expression);
            }
            else if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax)
            {
                BuildExpression_Identifier(builder, model, expression);
            }

        }
        static void BuildNode(BuildContext builder, SemanticModel model, Microsoft.CodeAnalysis.SyntaxNode node, int space)
        {
            string spacestr = "";
            for (var i = 0; i < space; i++)
                spacestr += "    ";
            Console.WriteLine(spacestr + "<" + node.GetType().Name + ">");
            var lt = node.GetLeadingTrivia();

            //前置注释
            if (lt.Count > 0)
            {

                builder.AppendLine(spacestr, lt.ToFullString());

            }
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)
            {//that's a basic
                Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax;

                //using is not support in typescript
                foreach (var _using in unit.Usings)
                {
                    Console.WriteLine(spacestr + "CompilationUnitSyntax:using:" + _using.Name);
                }
                foreach (var _member in unit.Members)
                {
                    BuildNode(builder, model, _member, space);
                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax)
            {
                BuildNode_Namespace(builder, model, node, space, spacestr);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)
            {
                BuildNode_Class(builder, model, node, space, spacestr);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)
            {
                BuildNode_Method(builder, model, node, space, spacestr);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax)
            {
                BuildSynatx_Block(builder, model, node, space, spacestr);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax)
            {
                BuildSynatx_Statement(builder, model, node, space, spacestr);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax;
                Console.WriteLine(spacestr + unit.Token.Text);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax)
            {
                BuildSynatx_LocalDeclarationStatement(builder, model, node, space, spacestr);
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax;
                BuildNode(builder, model, unit.Type, space + 1);
                foreach (var v in unit.Variables)
                {
                    BuildNode(builder, model, v, space + 1);
                }
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax;

                Console.WriteLine(spacestr + unit.Identifier.ValueText);
                if (unit.ArgumentList != null)
                    BuildNode(builder, model, unit.ArgumentList, space + 1);
                if (unit.Initializer != null)
                    BuildNode(builder, model, unit.Initializer, space + 1);

            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax;
                Console.WriteLine(spacestr + unit.EqualsToken.ValueText);
                BuildNode(builder, model, unit.Value, space + 1);
            }

            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax;
                Console.WriteLine(spacestr + unit.Keyword.ValueText);
            }


            else
            {
                Console.WriteLine(spacestr + "!!!up here is not parse type!!!");
            }
            var tt = node.GetTrailingTrivia();

            //后置注释
            if (tt.Count > 0)
            {
                builder.tscodeBuilder.AppendLine(spacestr + tt.ToFullString());
            }
            // Console.WriteLine(node.Span)
        }

        private static void BuildSynatx_LocalDeclarationStatement(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax;
            BuildNode(builder, model, unit.Declaration, space + 1);
            builder.Append(unit.SemicolonToken.ValueText, "\n");
        }

        private static void BuildExpression_Identifier(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
            var type = model.GetTypeInfo(unit);
            var d = model.GetDeclaredSymbol(unit);
            var o = model.GetOperation(unit);
            var s = model.GetSymbolInfo(unit);
            if (s.Symbol.Kind == SymbolKind.NamedType)//这是个类型名
            {
                builder.Append(GetTypeString(type.Type));
            }
            else if (s.Symbol.Kind == SymbolKind.Local)//这是个本地变量
            {
                builder.Append(unit.Identifier.ValueText);
            }
            else if (s.Symbol.Kind == SymbolKind.Parameter)//这是个参数
            {
                builder.Append(unit.Identifier.ValueText);
            }
            else
            {
                builder.Append(unit.Identifier.ValueText, "<", s.Symbol.Kind.ToString(), ">");
            }


        }

        private static void BuildExpression_MemberAccess(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            //a . b
            Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax;
            BuildExpression(builder, model, unit.Expression);
            //simple name
            builder.Append(".", unit.Name.Identifier.ValueText);
            //BuildNode(builder, model, unit.Name, space + 1);
        }

        private static void BuildExpression_Invocation(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            //make a call
            Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;

            //call part
            BuildExpression(builder, model, unit.Expression);


            builder.Append("(");
            //param part
            for (var i = 0; i < unit.ArgumentList.Arguments.Count; i++)
            {
                if (i > 0)
                    builder.Append(",");
                var arg = unit.ArgumentList.Arguments[i].Expression;
                BuildExpression(builder, model, arg);
            }
            builder.Append(")");
        }

        private static void BuildSynatx_Statement(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {//donothing for now.
            Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax;
            builder.Append(spacestr);
            BuildExpression(builder, model, unit.Expression);
            builder.Append(unit.SemicolonToken.ValueText, "\n");
        }

        private static void BuildSynatx_Block(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax;
            builder.AppendLine(spacestr, unit.OpenBraceToken.ValueText);
            foreach (var _member in unit.Statements)
            {
                BuildNode(builder, model, _member, space + 1);
            }
            builder.AppendLine(spacestr, unit.CloseBraceToken.ValueText);
        }


        private static void BuildNode_Namespace(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            //namespace
            Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax;
            builder.AppendLine(spacestr, "namespace " + unit.Name);
            builder.AppendLine(spacestr, "{");
            foreach (var _member in unit.Members)
            {
                BuildNode(builder, model, _member, space + 1);
            }
            builder.AppendLine(spacestr, "}");
        }

        private static void BuildNode_Class(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            //define a class
            Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
            string modifiers = "";
            foreach (var m in unit.Modifiers)
            {
                var modify = m.ValueText;
                if (modify == "public")
                    modify = "export";
                modifiers += modify + " ";
            }
            builder.AppendLine(spacestr, modifiers, "class ", unit.Identifier.ValueText);
            builder.AppendLine(spacestr, "{");
            foreach (var _member in unit.Members)
            {
                BuildNode(builder, model, _member, space + 1);
            }
            builder.AppendLine(spacestr, "}");
        }
        static string GetTypeString(ITypeSymbol type)
        {
            var returnstr = "";
            if (type.SpecialType == SpecialType.System_Void)
                returnstr = "void";
            else if (type.SpecialType == SpecialType.System_Boolean)
                returnstr = "boolean";
            else if (type.SpecialType == SpecialType.System_String)
                returnstr = "string";
            else if (type.SpecialType == SpecialType.None)
            {//other type
                if (type.TypeKind == TypeKind.Array)
                {
                    IArrayTypeSymbol at = type as IArrayTypeSymbol;
                    var estr = GetTypeString(at.ElementType);
                    return estr + "[]";
                }
                else if (type.TypeKind == TypeKind.Class)
                {
                    return type.ToDisplayString();
                }
                else
                {
                    returnstr = "<other type>:" + type.ToDisplayParts();
                }
            }
            else
            {//other special type
                returnstr = "<other special type>:" + type.SpecialType.ToString();
            }
            return returnstr;
        }
        static string GetTypeString(SemanticModel model, SyntaxNode node)
        {
            TypeInfo type = model.GetTypeInfo(node);
            return GetTypeString(type.Type);
        }
        private static void BuildNode_Method(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            //method
            Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;
            var paramstr = "";
            for (var i = 0; i < unit.ParameterList.Parameters.Count; i++)
            {
                ParameterSyntax p = unit.ParameterList.Parameters[i];
                var m = p.Modifiers;
                var id = p.Identifier.ValueText;
                var typestr = GetTypeString(model, p.Type);
                if (i != 0)
                    paramstr += ",";
                foreach (var modify in p.Modifiers)
                {
                    if (modify.ValueText == "params")
                        paramstr += "...";
                }
                paramstr += id + ":" + typestr;

                if (p.Default != null)
                {
                    throw new Exception("not write this for now.");
                }
            }
            var returnstr = GetTypeString(model, unit.ReturnType);

            string modifiers = "";
            foreach (var m in unit.Modifiers)
            {
                var modify = m.ValueText;
                if (modify == "public")
                    continue;//method default is public
                modifiers += modify + " ";
            }


            builder.AppendLine(spacestr, modifiers, unit.Identifier.ValueText, "(", paramstr, "):", returnstr);


            //body
            // builder.AppendLine(spacestr, "{");//括号有body呢
            BuildNode(builder, model, unit.Body, space);
            /// builder.AppendLine(spacestr, "}");

        }
    }
}
