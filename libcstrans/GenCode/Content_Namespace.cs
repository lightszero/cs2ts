using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    partial class CodeContent
    {


        /// <summary>
        /// ParseNamespace
        /// </summary>
        ///NameSpace支持嵌套
        //Dictionary<string, CleanAst.AstNode> delayNameSpace = new Dictionary<string, CleanAst.AstNode>();
        void ParseNameSpace(string names, AstNode node)
        {
            ParseNameSpaceSingle(names, node, true);
            //foreach (var dn in delayNameSpace)
            //{
            //    AppendLine();
            //    ParseNameSpaceSingle(dn.Key, dn.Value, false);
            //}
        }
        void ParseNameSpaceSingle(string names, AstNode node, bool withDepthAdd)
        {
            foreach (var v in node.Children)
            {

                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is SimpleType)
                {

                }
                else if (v is MemberType)
                {

                }
                else if (v is CSharpTokenNode)
                {
                    if (v.ToString() == "namespace")
                    {
                        if (curNamespace == names)
                        {
                            Append("module " + names);
                        }
                        else
                        {
                            Append("export module " + names);
                        }
                        break;
                    }
                    if (v.ToString() == "{")
                    {
                        Append("{");
                        space += 4;
                        break;
                    }
                    if (v.ToString() == "}")
                    {
                        space -= 4;
                        Append("}");
                        break;
                    }
                }

                else if (v is NewLineNode)
                {
                    AppendLine();
                }

                else if (v is NamespaceDeclaration)
                {
                    if (withDepthAdd == false)
                        break;
                    string snames = Tools._getFirstType(v);
                    string oldCurNameSpace = curNamespace;
                    if (string.IsNullOrEmpty(curNamespace))
                        curNamespace = snames;
                    else
                        curNamespace += "." + snames;
                    ParseNameSpaceSingle(snames, v, true);
                    //ParseNameSpaceDelay(snames, v);
                    curNamespace = oldCurNameSpace;
                }
                else if (v is TypeDeclaration)
                {
                    string cname = Tools._getIdentifier(v);
                    string oldclass = curClassName;
                    ParseClass(cname, v);
                    curClassName = oldclass;
                }

                else
                {
                    logger.LogError("not support namespace element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }
            }

        }
        //void ParseNameSpaceDelay(string names, CleanAst.AstNode node)
        //{
        //    delayNameSpace[curNamespace] = node;
        //    foreach (var v in node.Children)
        //    {
        //        switch (v.AstType)
        //        {
        //            case "NamespaceDeclaration":
        //                {
        //                    string snames = Tools._getFirstType(v);
        //                    string oldCurNameSpace = curNamespace;
        //                    if (string.IsNullOrEmpty(curNamespace))
        //                        curNamespace = snames;
        //                    else
        //                        curNamespace += "." + snames;
        //                    ParseNameSpaceDelay(snames, v);
        //                    curNamespace = oldCurNameSpace;
        //                }
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}

    }


}
