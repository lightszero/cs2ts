using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeConverter
{

    class CodeBuilder
    {
        static void ConsoleLog(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
        }
        static void ConsoleWarn(string text)
        {
            var oldc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        static void ConsoleErr(string text)
        {
            var oldc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        public static void Build(BuildContext builder, SourceCode document)
        {
            BuildNode(builder, document.semanticModel, document.syntaxTree.GetRoot(), 0); ;
        }
        static void BuildExpression(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            Console.WriteLine("BuildExpression<" + expression.GetType().Name + ">");


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
            else if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax)
            {
                PredefinedTypeSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax;
                var typestr = GetTypeString(model, unit);
                builder.Append(typestr);
            }
            else if (expression is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax;
                builder.Append(unit.Token.Text);
            }
            else if (expression is ObjectCreationExpressionSyntax)
            {
                BuildExpression_Creation(builder, model, expression);
            }
            else if (expression is BinaryExpressionSyntax)
            {
                BuildExpression_Binary(builder, model, expression);
            }
            else if (expression is PostfixUnaryExpressionSyntax)
            {
                BuildExpression_PostfixUnary(builder, model, expression);
            }
            else if (expression is ParenthesizedExpressionSyntax)
            {
                ParenthesizedExpressionSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax;
                builder.Append(unit.OpenParenToken.ValueText);
                BuildExpression(builder, model, unit.Expression);
                builder.Append(unit.CloseParenToken.ValueText);
            }
            else if (expression is AssignmentExpressionSyntax)
            {
                BuildExpression_Assignment(builder, model, expression);
            }
            else
            {
                ConsoleWarn("BuildExpression!!!up here is not parse type!!!");
            }


        }
        static void BuildNode(BuildContext builder, SemanticModel model, Microsoft.CodeAnalysis.SyntaxNode node, int space)
        {
            var fullcode = model.SyntaxTree.GetText().ToString().Substring(node.FullSpan.Start, node.FullSpan.Length);
            string spacestr = "";
            for (var i = 0; i < space; i++)
                spacestr += "    ";
            Console.WriteLine(spacestr + "<" + node.GetType().Name + ">");
            //前置注释
            var lt = node.GetLeadingTrivia();
            if (lt.Count > 0)
            {
                builder.AppendTriviaClean(lt);
                builder.AppendLine();
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
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax)
            {
                BuildSynatx_LocalDeclarationStatement(builder, model, node, space, spacestr);
            }
            else if (node is ForStatementSyntax)
            {
                BuildSynatx_ForStatement(builder, model, node, space, spacestr);
            }
            else if (node is IfStatementSyntax)
            {
                BuildSynatx_IfStatement(builder, model, node, space, spacestr);

            }
            else if (node is VariableDeclarationSyntax)
            {
                BuildSynatx_VariableDeclaration(builder, model, node, space, spacestr);
            }
            //else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax)
            //{
            //    Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax;
            //    BuildNode(builder, model, unit.Type, space + 1);
            //    foreach (var v in unit.Variables)
            //    {
            //        BuildNode(builder, model, v, space + 1);
            //    }
            //}
            //else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax)
            //{
            //    Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax;

            //    Console.WriteLine(spacestr + unit.Identifier.ValueText);
            //    if (unit.ArgumentList != null)
            //        BuildNode(builder, model, unit.ArgumentList, space + 1);
            //    if (unit.Initializer != null)
            //        BuildNode(builder, model, unit.Initializer, space + 1);

            //}
            //else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax)
            //{
            //    Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax;
            //    Console.WriteLine(spacestr + unit.EqualsToken.ValueText);
            //    BuildNode(builder, model, unit.Value, space + 1);
            //}




            else
            {
                ConsoleWarn(spacestr + "BuildNode!!!up here is not parse type!!!");
            }
            var tt = node.GetTrailingTrivia();

            //后置注释
            if (tt.Count > 0)
            {
                builder.AppendTriviaClean(tt);
                builder.AppendLine();
            }
            // Console.WriteLine(node.Span)
        }
        private static void BuildSynatx_VariableDeclaration(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {

            VariableDeclarationSyntax unit = node as VariableDeclarationSyntax;
            //builder.Append(spacestr);

            var typestr = GetTypeString(model, unit.Type);
            builder.Append("let ");
            for (var i = 0; i < unit.Variables.Count; i++)
            {
                if (i > 0) builder.Append(",");
                VariableDeclaratorSyntax v = unit.Variables[i];
                var id = v.Identifier.ValueText;
                builder.Append(id, ":", typestr);
                if (v.Initializer != null)
                {
                    EqualsValueClauseSyntax e = v.Initializer;
                    builder.Append(e.EqualsToken.ValueText);
                    BuildExpression(builder, model, e.Value);
                }
            }
        }

        private static void BuildSynatx_LocalDeclarationStatement(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax;
            builder.Append(spacestr);

            var typestr = GetTypeString(model, unit.Declaration.Type);
            for (var i = 0; i < unit.Declaration.Variables.Count; i++)
            {
                VariableDeclaratorSyntax v = unit.Declaration.Variables[i];
                var id = v.Identifier.ValueText;
                builder.Append("let ", id, ":", typestr);
                if (v.ArgumentList != null)
                {
                    BuildNode(builder, model, v.ArgumentList, 0);
                }
                if (v.Initializer != null)
                {
                    EqualsValueClauseSyntax e = v.Initializer;
                    builder.Append(e.EqualsToken.ValueText);
                    BuildExpression(builder, model, e.Value);
                }

            }
            builder.Append(unit.SemicolonToken.ValueText);//, "\n");
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

            var typestr = GetTypeString(model, unit.Expression);
            var newtype = NameConverter.ConvertTypeName(typestr);
            if (newtype == null)
                BuildExpression(builder, model, unit.Expression);
            else
                builder.Append(newtype);

            var namestr = unit.Name.Identifier.ValueText;
            var newname = NameConverter.ConvertMemberName(typestr, namestr);
            //simple name
            if (newname == null)
            {
                newname = unit.Name.Identifier.ValueText;
            }
            builder.Append(".", newname);
            //BuildNode(builder, model, unit.Name, space + 1);
        }

        private static void BuildExpression_Invocation(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            //make a call
            Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax unit = expression as Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;

            //call part


            BuildExpression_MemberAccess(builder, model, unit.Expression);


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

        private static void BuildExpression_Creation(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            ObjectCreationExpressionSyntax unit = expression as ObjectCreationExpressionSyntax;
            //unit.NewKeyword;
            builder.Append(unit.NewKeyword.Text);
            builder.Append(" ");
            var typestr = GetTypeString(model, unit);
            builder.Append(typestr);
            builder.Append("(");
            for (var i = 0; i < unit.ArgumentList.Arguments.Count; i++)
            {
                if (i > 0)
                    builder.Append(",");
                var arg = unit.ArgumentList.Arguments[i].Expression;
                BuildExpression(builder, model, arg);

            }
            builder.Append(")");
        }
        private static void BuildExpression_Binary(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            BinaryExpressionSyntax unit = expression as BinaryExpressionSyntax;
            BuildExpression(builder, model, unit.Left);
            builder.Append(unit.OperatorToken.ValueText);
            BuildExpression(builder, model, unit.Right);

        }
        private static void BuildExpression_PostfixUnary(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            PostfixUnaryExpressionSyntax unit = expression as PostfixUnaryExpressionSyntax;
            BuildExpression(builder, model, unit.Operand);
            builder.Append(unit.OperatorToken.ValueText);
        }
        private static void BuildExpression_Assignment(BuildContext builder, SemanticModel model, ExpressionSyntax expression)
        {
            AssignmentExpressionSyntax unit = expression as AssignmentExpressionSyntax;
            BuildExpression(builder, model, unit.Left);
            builder.Append(unit.OperatorToken.ValueText);
            BuildExpression(builder, model, unit.Right);
        }
        static void AppendToken(BuildContext builder, SyntaxToken token, string spacestr)
        {
            //前置注释
            var lt = token.LeadingTrivia;
            if (lt.Count > 0)
            {
                builder.AppendTriviaClean(lt);
            }


            builder.AppendLine(spacestr, token.ValueText);

            //后置注释
            var tt = token.TrailingTrivia;
            if (tt.Count > 0)
            {
                builder.AppendTriviaClean(tt);
            }
        }
        private static void BuildSynatx_Statement(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {//donothing for now.
            Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax;
            builder.Append(spacestr);
            BuildExpression(builder, model, unit.Expression);
            builder.Append(unit.SemicolonToken.ValueText);//, "\n");
        }
        private static void BuildSynatx_ForStatement(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {//donothing for now.
            Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax;
            builder.Append(spacestr);
            builder.Append(unit.ForKeyword.ValueText);//for
            builder.Append(unit.OpenParenToken.ValueText);//(

            if (unit.Declaration != null)
            {
                BuildSynatx_VariableDeclaration(builder, model, unit.Declaration, space, spacestr);
            }
            for (var i = 0; i < unit.Initializers.Count; i++)
            {
                if (i > 0 || unit.Declaration != null)
                    builder.Append(",");
                var init = unit.Initializers[i];
                BuildExpression(builder, model, init);
            }

            builder.Append(unit.FirstSemicolonToken.ValueText);//,

            BuildExpression(builder, model, unit.Condition);

            builder.Append(unit.SecondSemicolonToken.ValueText);//,

            for (var i = 0; i < unit.Incrementors.Count; i++)//i++
            {
                if (i > 0)
                    builder.Append(",");
                var expr = unit.Incrementors[i];
                BuildExpression(builder, model, expr);

            }
            builder.Append(unit.CloseParenToken.ValueText);


            BuildNode(builder, model, unit.Statement, space);
        }
        private static void BuildSynatx_IfStatement(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {//donothing for now.
            IfStatementSyntax unit = node as IfStatementSyntax;
            builder.Append(spacestr);
            builder.Append(unit.IfKeyword.ValueText);//if
            builder.Append(unit.OpenParenToken.ValueText);//(


            BuildExpression(builder, model, unit.Condition);

            builder.Append(unit.CloseParenToken.ValueText);


            BuildNode(builder, model, unit.Statement, space);

            if (unit.Else != null)
            {
                builder.Append(spacestr + unit.Else.ElseKeyword.ValueText);
                BuildNode(builder, model, unit.Else.Statement, space);
            }
        }
        private static void BuildSynatx_Block(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax;
            AppendToken(builder, unit.OpenBraceToken, spacestr);
            foreach (var _member in unit.Statements)
            {
                BuildNode(builder, model, _member, space + 1);
            }
            AppendToken(builder, unit.CloseBraceToken, spacestr);
        }


        private static void BuildNode_Namespace(BuildContext builder, SemanticModel model, SyntaxNode node, int space, string spacestr)
        {
            //namespace
            Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax unit = node as Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax;
            builder.AppendLine(spacestr, "namespace " + unit.Name);
            AppendToken(builder, unit.OpenBraceToken, spacestr);
            foreach (var _member in unit.Members)
            {
                BuildNode(builder, model, _member, space + 1);
            }
            AppendToken(builder, unit.CloseBraceToken, spacestr);
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
            AppendToken(builder, unit.OpenBraceToken, spacestr);
            foreach (var _member in unit.Members)
            {
                BuildNode(builder, model, _member, space + 1);
            }
            AppendToken(builder, unit.CloseBraceToken, spacestr);
        }
        static string GetTypeString(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Void:
                    return "void";
                case SpecialType.System_Boolean:
                    return "boolean";
                case SpecialType.System_String:
                    return "string";
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return "number";
                case SpecialType.System_Char:
                    return "string";
                case SpecialType.System_Byte:
                    return "number";
                case SpecialType.System_UInt64:
                    return "<very danger>Long";
                case SpecialType.None:
                    break;
                default:
                    return "<other special type>:" + type.SpecialType.ToString();
            }
            //other type
            switch (type.TypeKind)
            {
                case TypeKind.Array:
                    {
                        IArrayTypeSymbol at = type as IArrayTypeSymbol;
                        var estr = GetTypeString(at.ElementType);
                        return estr + "[]";
                    }
                case TypeKind.Class:
                    return type.ToDisplayString();
                default:
                    return "<other type>:" + type.ToDisplayParts();
            }

        }
        static string GetTypeString(SemanticModel model, SyntaxNode node)
        {
            TypeInfo type = model.GetTypeInfo(node);
            return GetTypeString(type.Type);
        }
        static string GetTypeStringByExpression(SemanticModel model, ExpressionSyntax expression)
        {
            BuildContext typec = new BuildContext();
            BuildExpression(typec, model, expression);
            var typestr = typec.TypeScriptCode;
            return typestr;
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
