using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    //所有的表达式必须有返回类型，少数表达式是无类型的。
    partial class CodeContent
    {
        class ExpressOption
        {
            public bool leftPropSet = false;//左边是属性
            public int touch = 0;
            public string insetfunc = null;//在set函数中
            public bool memberCall = false;//处理成员调用
            public List<string> paramType = null;
        }

        string ParseExpression(AstNode expr, ExpressOption option)
        {

            if (expr is MemberReferenceExpression)
                return ParseMemberReferenceExpression(expr as MemberReferenceExpression, option);
            else if (expr is PrimitiveExpression)
                return ParsePrimitiveExpression(expr);
            else if (expr is BinaryOperatorExpression)
                return ParseBinaryOperatorExpression(expr);
            else if (expr is IdentifierExpression)
                return ParseIdentifierExpression(expr as IdentifierExpression, option);
            else if (expr is AssignmentExpression)
                return ParseBinaryOperatorExpression(expr);
            else if (expr is ExpressionStatement)
                return ParseExpressionStatement(expr);
            else if (expr is ThisReferenceExpression)
                return ParseThisReferenceExpression(expr);
            else if (expr is ObjectCreateExpression)
                return ParseObjectCreateExpression(expr);
            else if (expr is InvocationExpression)
                return ParseInvocationExpression(expr);
            else if (expr is LambdaExpression)
                return ParseLambdaExpression(expr);
            else
            {
                logger.LogError("not support Expression element:" + expr.GetType().Name + "|" + expr.NodeType + "|" + expr.StartLocation.Line);
                return null;
            }

        }
        string ParseLambdaExpression(AstNode expr)
        {
            foreach (var p in expr.Children)
            {
                if (p is CSharpTokenNode)
                {
                    Append(p.ToString());
                }
                else if (p is BlockStatement)
                {
                    ParseBlockStatement(p, null);
                }
                else if (p is Expression)
                {
                    ParseExpression(expr, null);
                }
            }
            return "any";
        }
        string ParseInvocationExpression(AstNode expr)//处理函数调用
        {
            string returntype = null;
            ExpressOption option = new ExpressOption();
            option.paramType = null;
            option.memberCall = true;
            StringBuilder old = null;
            foreach (var v in expr.Children)
            {
                if (v is IdentifierExpression)
                {
                    if (option.paramType == null)
                    {
                        Append("");
                        continue;//delaycall
                    }
                    else
                    {
                        option.paramType.Add(ParseIdentifierExpression(v as IdentifierExpression, null));
                    }
                }
                else if (v is MemberReferenceExpression)
                {
                    if (option.paramType == null)
                    {
                        Append("");
                        continue;//delaycall
                    }
                    else
                    {
                        option.paramType.Add(ParseMemberReferenceExpression(v as MemberReferenceExpression, null));
                    }
                }
                if (v is CSharpTokenNode)
                {
                    if (v.ToString() == "(")
                    {//将参数处理到自定义的地方
                        old = this.builder;
                        this.builder = new StringBuilder();
                        option.paramType = new List<string>();
                    }
                    Append(v.ToString());
                    if (v.ToString() == ")")
                    {
                        string callbody = this.builder.ToString();
                        this.builder = old;


                        //此时再去处理
                        foreach (var vv in expr.Children)
                        {
                            if (vv is IdentifierExpression)
                            {


                                ParseIdentifierExpression(vv as IdentifierExpression, option);
                                break;
                            }
                            else if (vv is MemberReferenceExpression)
                            {


                                ParseMemberReferenceExpression(vv as MemberReferenceExpression, option);
                                break;
                            }
                        }
                        Append(callbody);
                    }
                }
                else if (v is PrimitiveExpression)
                {
                    option.paramType.Add(ParsePrimitiveExpression(v));
                }
                else
                {
                    logger.LogError("not support ParseInvocationExpression element:" + v.GetType().Name + "|" + expr.NodeType + "|" + expr.StartLocation.Line);

                }
            }


            return returntype;
        }
        /// <summary>
        /// 各种表达式
        /// </summary>
        /// <param name="expr"></param>
        string ParseObjectCreateExpression(AstNode expr)
        {
            string newtype = "";

            foreach (var v in expr.Children)
            {
                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is PrimitiveType)
                {
                    newtype = Tools.getPrimitiveTypeName(v.ToString());
                    Append(newtype);
                }
                else if (v is MemberType)
                {
                    newtype = getMemberTypeName(v.ToString());
                    Append(newtype);
                }
                else if (v is SimpleType)
                {
                    newtype = getSimpleTypeName((v as SimpleType).Identifier);
                    Append(newtype);
                }
                else if (v is CSharpTokenNode)
                {
                    if (v.ToString() == "(")//param begin;
                    {
                        Append("(");
                    }
                    else if (v.ToString() == ")")//paramend;
                    {
                        Append(")");
                    }

                    else if (v.ToString() == "new")
                    {
                        Append("new ");
                    }
                    else
                    {
                        Append(v.ToString());
                    }
                }
                else if (v is NewLineNode)
                {
                    AppendLine();
                }
                else if (v is BlockStatement)
                {
                    ParseBlockStatement(v, null);
                }
                else if (v is IdentifierExpression)
                {//add param
                    ParseIdentifierExpression(v as IdentifierExpression, null);
                }


                else
                {
                    logger.LogError("not support ParseObjectCreateExpression element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }

            }
            return newtype;
        }

        string ParseThisReferenceExpression(AstNode expr)//this
        {
            Append("this");
            return getCurClassName();
        }

        string ParseAssignmentExpression(AstNode expr, ExpressOption option)
        {
            foreach (var v in expr.Children)
            {
                if (v is IdentifierExpression)
                {
                    ParseIdentifierExpression(v as IdentifierExpression, option);
                }
                else if (v is PrimitiveExpression)
                {
                    ParsePrimitiveExpression(v);
                }
                else if (v is Comment)
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
                else if (v is AssignmentExpression)
                {
                    ParseAssignmentExpression(v, option);
                }
                else
                {
                    logger.LogError("not support ParseAssignmentExpression element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }


            }
            return null;
        }

        string ParsePrimitiveExpression(AstNode expr)//直接上
        {
            Append(expr.ToString());
            if (expr.ToString()[0] == '"' || expr.ToString()[0] == '@')
                return "string";
            else if (expr.ToString()[0] == '-' || char.IsNumber(expr.ToString()[0]))
                return "number";
            else if (expr.ToString() == "true" || expr.ToString() == "false")
                return "bool";
            throw new Exception("do not known what is:" + expr.ToString());
        }
        string ParseIdentifierExpression(IdentifierExpression expr, ExpressOption option)//名称表达式
        {
            //走这个表达式的时候说明是独立的一个调用
            return ParseIdentifier(getCurClassName(), expr.IdentifierToken, option, true);

            //string classtype = null;
            //if (option != null && option.leftPropSet)
            //{
            //    if (isProp(null, expr.Children[0].Code))
            //    {
            //        Append("this.set_" + expr.Children[0].Code);
            //        option.touch++;

            //    }
            //    else
            //    {
            //        Append(getMemberFullname(null, expr.Children[0].Code, out classtype));
            //    }
            //}
            //else if (option != null && option.memberCall)
            //{
            //    MethodInfo method = this.getMethodInfoByMember(getCurClassName(), expr.Children[0].Code, option.paramType.ToArray());
            //    //var method = proj.types[getCurClassName()].methods[expr.Children[0].Code];
            //    if (method._static == false)
            //    {
            //        Append("this." + method.olname);
            //    }
            //    else
            //    {
            //        Append(getCurClassName() + "." + method.olname);
            //    }
            //    //Append(expr.Children[0].Code);
            //}
            //else
            //{
            //    Append(getMemberFullname(null, expr.Children[0].Code, out classtype));
            //}
            //return classtype;
        }
        string ParseBinaryOperatorExpression(AstNode expr)//二值表达式
        {


            ExpressOption option = null;
            List<AstNode> child = new List<AstNode>(expr.Children);

            if (child[1].ToString() == "=")//赋值转换特殊处理
            {
                option = new ExpressOption();
                option.leftPropSet = true;

                ParseExpression(child[0], option);

                if (option.touch > 0)
                {
                    Append("(");
                }
                else
                {
                    Append("=");
                }
                string rtype = ParseExpression(child[2], null);

                if (rtype != "number" && rtype != "bool" && ((child[2] is ObjectCreateExpression) == false) && proj.types[rtype].classtype == "struct")
                {
                    Append(".Clone()");
                }
                if (option.touch > 0)
                {
                    Append(");");
                }
            }
            else
            {
                string ltype = ParseExpression(child[0], null);
                Append(child[1].ToString());
                string rtype = ParseExpression(child[2], null);
                if (ltype == rtype)
                    return rtype;
                else
                    throw new Exception("not parse this.");
            }
            return null;
            //Append(expr.ToString());

        }
        string ParseMemberReferenceExpression(MemberReferenceExpression expr, ExpressOption option)//成员引用表达式
        {
            string lefttype = ParseExpression(expr.Target, null);
            string _class = null;
            if (expr.Target is ThisReferenceExpression)
            {
                _class = getCurClassName();
            }
            else if (expr.Target is IdentifierExpression)
            {
                _class = getClassNameFromID((expr.Target as IdentifierExpression).Identifier);
            }
            else
            {
                logger.LogError("not parse this typefinder" + expr.Target.GetType().Name);
            }
            //根据左侧找到类型
            Append(expr.DotToken.ToString());
            //判断右侧是否是属性

            string idtype = ParseIdentifier(_class, expr.MemberNameToken , option, false);
            //Append(expr.Children[2].Code);
            return idtype;

        }
    }


}
