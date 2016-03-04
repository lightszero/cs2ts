using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    static class Tools
    {
        /// <summary>
        /// 得到类型定义的全名
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string _getTypeFullName(AstNode node)
        {
            string name = null;
            foreach (var n in node.Children)
            {
                if (n is Identifier)
                {
                    name = (n as Identifier).Name;
                    break;
                }
            }

            while (node.Parent != null)
            {
                if (node.Parent is NamespaceDeclaration)
                {
                    string pname = "";
                    foreach (var n in node.Parent.Children)
                    {
                        if (n is MemberType)
                        {
                            pname = n.ToString();
                            break;
                        }
                        else if (n is SimpleType)
                        {
                            pname = n.ToString();
                            break;
                        }
                    }
                    name = pname + "." + name;
                }
                else if (node.Parent is TypeDeclaration)
                {
                    string sname = null;
                    foreach (var n in node.Parent.Children)
                    {
                        if (n is Identifier)
                        {
                            sname = (n as Identifier).Name;
                            break;
                        }
                    }
                    name = sname + "_" + name;
                }
                node = node.Parent;
            }
            return name;
        }
        public static string _getClassFullName(AstNode node)
        {
            string name = null;
            foreach (var n in node.Children)
            {
                if (n is Identifier)
                {
                    name = (n as Identifier).Name;
                    break;
                }
            }

            while (node.Parent != null)
            {
                if (node.Parent is TypeDeclaration)
                {
                    string pname = "";
                    foreach (var n in node.Parent.Children)
                    {
                        if (n is Identifier)
                        {
                            pname = n.ToString();
                            break;
                        }
                    }
                    name = pname + "." + name;
                }
                node = node.Parent;
            }
            return name;
        }


        public static string _getUsingFullName(AstNode node)
        {
            return _getFirstType(node);
        }
        public static string _getFirstType(AstNode node)
        {
            string name = null;
            foreach (var n in node.Children)
            {
                if (n is SimpleType)
                {
                    name = (n as SimpleType).Identifier;
                    break;
                }
                else if (n is MemberType)
                {
                    name = n.ToString();
                    break;
                }
            }
            return name;
        }
        public static string _getIdentifier(AstNode node)
        {
            string name = null;
            foreach (var n in node.Children)
            {
                if (n is Identifier)
                {
                    return (n as Identifier).Name;
                }
            }
            return name;
        }
        public static string _getFieldName(AstNode node)
        {

            foreach (var n in node.Children)
            {
                if (n is VariableInitializer)
                {
                    return n.Children.First().ToString();
                }
            }
            return null;
        }

        public static string _getClassFullnameInContent(string name, AstNode node)
        {
            return name;
        }
        public static FieldInfo _getPropertyInfo(AstNode node)
        {
            FieldInfo info = new FieldInfo();
            foreach (var n in node.Children)
            {
                if (n is CSharpModifierToken)
                {
                    if((n as CSharpModifierToken).Modifier== Modifiers.Static)
                        info._static = true;
                }
                if (n is Identifier)
                {
                    info.name = n.ToString();
                }
                if (n is PrimitiveType)
                {
                    info.type = getPrimitiveTypeName(n.ToString());
                }
                if (n is SimpleType)
                {
                    info.type = _getClassFullnameInContent(n.ToString(), node);
                }
            }
            return info;
        }
        public static FieldInfo _getFieldInfo(AstNode node)
        {
            FieldInfo info = new FieldInfo();
            foreach (var n in node.Children)
            {
                if (n is CSharpModifierToken)
                {
                    if (n.ToString() == "static")
                        info._static = true;
                }
                if (n is PrimitiveType)
                {
                    info.type = getPrimitiveTypeName(n.ToString());
                }
                if (n is VariableInitializer)
                {
                    info.name = (n as VariableInitializer).Name;
                }
                if (n is SimpleType)
                {
                    info.type = _getClassFullnameInContent(n.ToString(), node);
                }
            }
            return info;
        }
        public static MethodInfo _getMethodInfo(AstNode node)
        {

            MethodInfo info = new MethodInfo();
            string name = null;
            List<string> param = null;
            foreach (var n in node.Children)
            {
                if (n is ParameterDeclaration)
                {
                    if (param != null)
                    {
                        foreach (var sn in n.Children)
                        {
                            if (sn is PrimitiveType)
                            {
                                param.Add(getPrimitiveTypeName(sn.ToString()));
                            }
                            if (sn is SimpleType)
                            {
                                param.Add(_getClassFullnameInContent(sn.ToString(), node));
                            }
                        }
                    }
                }

                if (n is CSharpModifierToken)
                {
                    if (n.ToString() == "static")
                        info._static = true;

                }
                if (n is CSharpTokenNode)
                {
                    if (n.ToString() == "(")
                    {
                        //paramstart;
                        param = new List<string>();
                    }
                    if (n.ToString() == ")")
                    {
                        info._param = param.ToArray();
                        param = null;
                        //paramend;
                    }
                }
                if (n is Identifier)
                {
                    info.name = n.ToString();
                }
                if (n is PrimitiveType)
                {
                    info.type = getPrimitiveTypeName(n.ToString());
                }
                if (n is SimpleType)
                {
                    info.type = _getClassFullnameInContent(n.ToString(), node);
                }
            }
            return info;
        }
        public static string getPrimitiveTypeName(string type)
        {
            switch (type)
            {
                case "var":
                    return null;
                case "string":
                case "String":
                    return "string";
                case "bool":
                case "Boolean":
                    return "boolean";
                case "void":
                    return "void";
                case "int":
                case "uint":
                case "short":
                case "ushort":
                case "long":
                case "ulong":
                case "byte":
                case "sbyte":
                case "float":
                case "double":
                    return "number";

                default:
                    return "any";
            }

        }

    }
}
