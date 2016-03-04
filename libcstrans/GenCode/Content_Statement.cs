using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    partial class CodeContent
    {
        //各种Statement

        void ParseBlockStatement(AstNode block, ExpressOption option)
        {
            this.PushValueLayer();
            if (option != null && option.insetfunc != null)
            {
                this.DefineValue("value", option.insetfunc);
                option = null;
            }
            foreach (var v in block.Children)
            {

                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is NewLineNode)
                {
                    AppendLine();
                }
                else if (v is CSharpTokenNode)
                {

                    if (v.ToString() == "{")//bodybegin
                    {
                        Append("{");
                        space += 4;
                    }
                    else if (v.ToString() == "}")//bodyend
                    {
                        space -= 4;
                        Append("}");

                    }
                    else
                    {
                        Append(v.ToString());
                    }
                }
                else if (v is ReturnStatement)
                {
                    ParseReturnStatement(v);
                }
                else if (v is ExpressionStatement)
                {
                    ParseExpressionStatement(v);

                }
                else if (v is VariableDeclarationStatement)
                {
                    ParseVariableDeclarationStatement(v);
                }
                else
                {
                    logger.LogError("not support BlockStatement element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }

            }
            this.PopValueLayer();
        }
        void ParseReturnStatement(AstNode state)
        {
            foreach (var v in state.Children)
            {

                if (v is CSharpTokenNode)

                {
                    if (v.ToString() == "return")
                        Append("return ");
                    else
                        Append(v.ToString());
                }

                else
                {
                    if (v is Expression)
                    {
                        ParseExpression(v, null);
                        break;
                    }
                    logger.LogError("not support ParseReturnStatement element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }
            }
        }
        void ParseVariableDeclarationStatement(AstNode state)
        {
            string returntype = null;
            foreach (var v in state.Children)
            {
                if (v is SimpleType)
                {
                    returntype = getSimpleTypeName((v as SimpleType).Identifier);
                    Append("var ");

                }
                else if (v is MemberType)
                {
                    returntype = getMemberTypeName(v.ToString());

                    Append("var ");
                }
                else if (v is VariableInitializer)
                {
                    var rtype = ParseVariableInitializer(v as VariableInitializer, returntype);
                    //if (returntype == null)
                    //    returntype = "any";
                    if (rtype != null)
                        returntype = rtype;
                    DefineValue((v as VariableInitializer).Name, returntype);
                }
                else if (v is NewLineNode)
                {
                    AppendLine();
                }
                else if (v is CSharpTokenNode)
                {
                    Append(v.ToString());
                }
                else if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
            }

            //Append(state.Children[2].ToString());
        }
        string ParseExpressionStatement(AstNode block)
        {
            foreach (var v in block.Children)
            {
                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is NewLineNode)
                {
                    AppendLine();
                }
                else if (v is CSharpTokenNode)
                {

                    if (v.ToString() == "{")//bodybegin
                    {
                        Append("{");
                    }
                    else if (v.ToString() == "}")//bodyend
                    {
                        Append("}");
                    }
                    else if (v.ToString() == ";")
                    {
                        Append(";");

                    }
                    else
                    {
                        Append(v.ToString());
                    }
                }
                else if (v is ExpressionStatement)
                {
                    ParseExpressionStatement(v);
                }
                else
                {
                    if (v is Expression)
                    {
                        ParseExpression(v, null);
                        break;
                    }
                    logger.LogError("not support ParseExpressionStatement element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                    break;
                }
            }
            return null;
        }



    }


}
