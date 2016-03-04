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
        /// ParseClass
        /// </summary>
        /// <param name="node"></param>
        Dictionary<string, AstNode> delayClass = new Dictionary<string, AstNode>();
        void ParseClass(string name, AstNode node)
        {
            delayClass.Clear();
            ParseClassSingle(name, node, true);
            if (delayClass.Count > 0)
            {
                AppendLine();
                foreach (var dc in delayClass)
                {
                    string newkey = dc.Key.Replace('.', '_');
                    ParseClassSingle(newkey, dc.Value, false);
                }
            }
        }

        void ParseClassSingle(string classname, AstNode node, bool withDepthAdd)
        {
            List<TypeInfo> basetype = null;
            int constructorcount = 0;
            bool bExport = false;
            foreach (var v in node.Children)
            {
                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is CSharpModifierToken)
                {
                    if (v.ToString() == "public")
                    {
                        if (string.IsNullOrEmpty(curNamespace))
                        {

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(curNamespace) == false)
                            {
                                Append("export ");
                                bExport = true;
                            }
                        }
                    }
                    else if (v.ToString() == "private")
                        break;
                    else
                        logger.LogWarnning("not support modifier,skip it:" + v.ToString() + "|" + v.StartLocation.Line);
                }
                else if (v is CSharpTokenNode)
                {
                    //export everything
                    if (bExport == false && string.IsNullOrEmpty(curNamespace) == false)
                    {
                        Append("export ");
                        bExport = true;
                    }

                    if (v.ToString() == "class")
                    {
                        Append("class ");
                    }
                    else if (v.ToString() == "struct")
                    {
                        logger.Log("Find a struct:" + classname);
                        Append("class ");
                    }
                    else if (v.ToString() == "interface")
                        Append("interface ");
                    else if (v.ToString() == "{")
                    {
                        if (basetype != null)
                        {
                            ParseClassBase(basetype);
                            basetype = null;
                        }
                        Append("{");
                        curClassName = classname;
                        space += 4;
                    }
                    else if (v.ToString() == "}")
                    {
                        ParseClassEnd();

                        space -= 4;
                        Append("}");
                    }
                    else if (v.ToString() == ":")//有继承
                    {
                        basetype = new List<TypeInfo>();
                    }
                }
                else if (v is SimpleType)
                {
                    var type = this.proj.FindType((v as SimpleType).Identifier, curNamespace, curClassName, this.listUsing, this.listUsinAlias);
                    basetype.Add(proj.getTypeInfo(type));
                }
                else if (v is Identifier)
                {
                    //classname = v.Code;

                    Append(classname);
                }
                else if (v is NewLineNode)
                {
                    if (basetype != null)
                    {
                        ParseClassBase(basetype);
                        basetype = null;
                    }
                    AppendLine();
                }
                else if (v is FieldDeclaration)
                {
                    ParseClassMember_Field(v);
                }
                else if (v is MethodDeclaration)
                {
                    ParseClassMember_Method(v, false);

                }
                else if (v is ConstructorDeclaration)
                {
                    if (constructorcount > 0)
                    {
                        logger.LogError("Can't use more than one Constructor in class. for typescript.");
                    }
                    ParseClassConstructor(v);
                    constructorcount++;
                }
                else if (v is PropertyDeclaration)
                {
                    ParseClassMember_Property(v);
                }
                else if (v is TypeDeclaration)
                {
                    if (withDepthAdd)
                    {
                        string fullname = Tools._getClassFullName(v);
                        ParseClassDelay(fullname, node);
                    }
                }
                else
                {
                    logger.LogError("not support class element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }


            }
        }
        void ParseClassConstructor(AstNode node)
        {
            ParseClassMember_Method(node, true);
        }
        void ParseClassDelay(string names, AstNode node)
        {
            delayClass[names] = node;
            foreach (var v in node.Children)
            {
                if (v is TypeDeclaration)
                {
                    string fullname = Tools._getClassFullName(v);
                    ParseClassDelay(fullname, v);
                }

            }
        }


        /// <summary>
        /// 有可能插入代码
        /// </summary>
        /// <param name="classname"></param>

        void ParseClassBase(List<TypeInfo> basetype)
        {
            foreach (var b in basetype)
            {
                if (b.classtype == "struct" || b.classtype == "class")
                {
                    Append(" extends " + b.fullname);
                }
            }
            bool firstimplements = true;
            foreach (var b in basetype)
            {
                if (b.classtype == "interface")
                {
                    if (firstimplements)
                    {
                        Append(" implements " + b.fullname);
                        firstimplements = false;
                    }
                    else
                    {
                        Append(" , " + b.fullname);
                    }
                }
            }
        }
        void ParseClassEnd()
        {
            var type = getCurClassName();
            var info = proj.types[type];
            if (info.classtype == "struct")
            {//插入struct Clone代码
             //检查是否有Clone函数，没有则报错

                foreach (var i in info.methods.Values)
                {
                    if (i.name == "Clone" && i._param.Length == 0)
                    {
                        return;
                    }
                }
                logger.LogError("Struct:" + info.fullname + " has no Clone Method.so can't be convert To TypeScript");
                //如果只有一个空参构造，也是可以自动嵌入代码的

                //Append("__clone__():" + curClassName); AppendLine();
                //Append("{"); AppendLine();
                //space += 4;
                //Append("var _c = new " + curClassName + "();"); AppendLine();
                //foreach (var v in info.Field)
                //{
                //    if (v.Value._static)
                //        continue;
                //    Append("_c." + v.Key + "=this." + v.Key + ";"); AppendLine();

                //}
                //foreach (var p in info.Prop)
                //{
                //    if (p.Value._static)
                //        continue;
                //    Append("_c.set_" + p.Key + "(this." + p.Key + ");"); AppendLine();
                //}
                //Append("return _c;"); AppendLine();
                //space -= 4;
                //Append("}"); AppendLine();

            }
            curClassName = null;

        }
        ///field
        ///

        void ParseClassMember_Field(AstNode field)
        {
            string returntype = "";
            int bpublic = 0;
            bool bstatic = false;
            foreach (var v in field.Children)
            {

                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is CSharpModifierToken)
                {
                    if (v.ToString() == "public")
                    {
                        bpublic = 1;
                    }
                    else if (v.ToString() == "private")
                    {
                        bpublic = -1;
                    }
                    else if (v.ToString() == "static")
                    {
                        bstatic = true;
                    }
                    else
                    {
                        logger.LogWarnning("not support modifier,skip it:" + v.ToString() + "|" + v.StartLocation.Line);
                    }

                }
                else if (v is PrimitiveType)
                    returntype = Tools.getPrimitiveTypeName(v.ToString());
                else if (v is MemberType)
                    returntype = getMemberTypeName(v.ToString());
                else if (v is VariableInitializer)
                {
                    if (bpublic < 0)
                        Append("private ");
                    else if (bpublic > 0)
                        Append("public ");
                    if (bstatic)
                        Append("static ");
                    ParseVariableInitializer(v as VariableInitializer, returntype);


                }
                else if (v is CSharpTokenNode)
                {
                    if (v.ToString() == ";")
                    {
                        Append(";");
                        break;
                    }
                }
                else if (v is NewLineNode)
                {
                    AppendLine();
                }
                else
                {
                    logger.LogError("not support class field element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }
                break;

            }
        }

        string ParseClassMember_Method(AstNode method, bool bConstructor)
        {
            string returntype = "";
            string methodname = null;
            int bpublic = 0;
            bool bstatic = false;
            List<string> _param = new List<string>();
            StringBuilder old = null;
            foreach (var v in method.Children)
            {

                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is CSharpModifierToken)
                {
                    if (v.ToString() == "public")
                    {
                        bpublic = 1;
                    }
                    else if (v.ToString() == "private")
                    {
                        bpublic = -1;
                    }
                    else if (v.ToString() == "static")
                    {
                        bstatic = true;
                    }
                    else
                    {
                        logger.LogWarnning("not support modifier,skip it:" + v.ToString() + "|" + v.StartLocation.Line);
                    }

                }
                else if (v is PrimitiveType)
                    returntype = Tools.getPrimitiveTypeName(v.ToString());
                else if (v is MemberType)
                    returntype = getMemberTypeName(v.ToString());
                else if (v is Identifier)
                {
                    methodname = v.ToString();
                    if (bpublic < 0)
                        Append("private ");
                    else if (bpublic > 0)
                        Append("public ");
                    if (bstatic)
                        Append("static ");

                }
                else if (v is CSharpTokenNode)
                {
                    if (v.ToString() == "(")//param begin;
                    {
                        Append("");
                        old = this.builder;
                        this.builder = new StringBuilder();
                        Append("(");
                        this.PushValueLayer();

                    }
                    else if (v.ToString() == ")")//paramend;
                    {
                        if (string.IsNullOrEmpty(returntype))
                        {
                            Append(")");
                        }
                        else
                        {
                            Append("):" + returntype);
                        }
                        string paramstr = builder.ToString();
                        this.builder = old;

                        if (bConstructor)
                            Append("constructor");
                        else
                        {
                            var _method = getMethodInfoByMember(getCurClassName(), methodname, _param.ToArray());

                            Append(_method.olname);
                        }
                        Append(paramstr);

                    }
                    else if (v.ToString() == ";")
                    {
                        Append(";");
                        break;
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
                    ParseBlockStatement(v, null);//代码执行完毕
                    this.PopValueLayer();
                }
                else if (v is ParameterDeclaration)
                {
                    string ptype = ParseClassMember_MethodParam(v);
                    _param.Add(ptype);
                }
                else
                {
                    logger.LogError("not support class method element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }


            }
            return returntype;
        }

        string ParseClassMember_MethodParam(AstNode node)
        {
            string returntype = "";
            foreach (var v in node.Children)
            {

                if (v is PrimitiveType)
                {
                    returntype = Tools.getPrimitiveTypeName(v.ToString());
                }
                else if (v is MemberType)
                {
                    returntype = getMemberTypeName(v.ToString());
                }
                else if (v is Identifier)
                {
                    this.DefineValue(v.ToString(), returntype);
                    Append(v.ToString() + ":" + returntype);
                }
                else
                {
                    logger.LogError("not support ParseClassMember_MethodParam element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }
                break;

            }
            return returntype;
        }
        void ParseClassMember_Property(AstNode property)
        {
            string returntype = "";
            string methodname = "";
            int getpublic = 0;
            int setpublic = 0;
            bool bstatic = false;
            bool get = false;
            bool set = false;
            AstNode getBlock = null;
            AstNode setBlock = null;
            foreach (var v in property.Children)
            {
                if (v is Comment)
                {
                    Append(v.ToString().Replace("\r\n", ""));
                }
                else if (v is CSharpModifierToken)
                {
                    if (v.ToString() == "public")
                    {
                        getpublic = setpublic = 1;
                    }
                    else if (v.ToString() == "private")
                    {
                        getpublic = setpublic = -1;
                    }
                    else if (v.ToString() == "static")
                    {
                        bstatic = true;
                    }
                    else
                    {
                        logger.LogWarnning("not support modifier,skip it:" + v.ToString() + "|" + v.StartLocation.Line);
                    }

                }
                else if (v is PrimitiveType)
                    returntype = Tools.getPrimitiveTypeName(v.ToString());
                else if (v is MemberType)
                    returntype = getMemberTypeName(v.ToString());
                else if (v is Identifier)
                {
                    methodname = v.ToString();
                }
                else if (v is CSharpTokenNode)
                {
                    if (v.ToString() == "{")//param begin;
                    {
                        //property begin;
                    }
                    else if (v.ToString() == "}")//paramend;
                    {
                        //property End;
                        var typeinfo = proj.types[getCurClassName()];
                        if (typeinfo.classtype == "interface")
                        {//接口不嵌入代码
                            if (get)
                            {
                                if (getpublic < 0)
                                    Append("private ");
                                else if (getpublic > 0)
                                    Append("public ");
                                if (bstatic)
                                    Append("static ");

                                //只能转化成函数了，typescript interface 不支持访问器
                                Append("get_" + methodname + "():" + returntype + ";");
                                AppendLine();
                            }
                            if (set)
                            {
                                if (setpublic < 0)
                                    Append("private ");
                                else if (setpublic > 0)
                                    Append("public ");
                                if (bstatic)
                                    Append("static ");

                                Append("set_" + methodname + "(value:" + returntype + ");");
                                AppendLine();
                            }
                        }
                        else
                        {//嵌入代码
                            if (getBlock == null && setBlock == null)
                            {
                                Append("_" + methodname + ":" + returntype + ";");
                                AppendLine();
                            }
                            if (get)
                            {
                                if (getpublic < 0)
                                    Append("private ");
                                else if (getpublic > 0)
                                    Append("public ");
                                if (bstatic)
                                    Append("static ");

                                Append("get_" + methodname + "():" + returntype);
                                AppendLine();
                                if (getBlock != null)
                                {
                                    ParseBlockStatement(getBlock, null);
                                    AppendLine();
                                }
                                else
                                {//自动嵌入
                                    Append("{"); AppendLine();
                                    Append("   return this._" + methodname + ";"); AppendLine();
                                    Append("}"); AppendLine();
                                }
                            }
                            if (set)
                            {
                                if (setpublic < 0)
                                    Append("private ");
                                else if (setpublic > 0)
                                    Append("public ");
                                if (bstatic)
                                    Append("static ");

                                Append("set_" + methodname + "(value:" + returntype + ")");
                                AppendLine();
                                if (setBlock != null)
                                {
                                    ExpressOption option = new ExpressOption();
                                    option.insetfunc = returntype;
                                    ParseBlockStatement(setBlock, option);
                                    AppendLine();
                                }
                                else
                                {//自动嵌入
                                    Append("{"); AppendLine();
                                    Append("   this._" + methodname + "=value;"); AppendLine();
                                    Append("}"); AppendLine();
                                }
                            }
                        }
                    }

                    else
                    {
                        Append(v.ToString());
                    }
                }
                else if (v is NewLineNode)
                {
                    //AppendLine();
                }
                else if (v is Accessor)
                {
                    bool _get = false;
                    foreach (var sv in v.Children)
                    {
                        int m = 0;
                        if (sv is CSharpModifierToken)
                        {
                            if (sv.ToString() == "public")
                            {
                                m = 1;
                            }
                            else if (sv.ToString() == "private")
                            {
                                m = -1;
                            }
                        }
                        else if (sv is CSharpTokenNode)
                        {
                            if (sv.ToString() == "get")
                            {
                                _get = true;
                                get = true;
                                if (m != 0)
                                    getpublic = m;
                            }
                            else if (sv.ToString() == "set")
                            {
                                _get = false;
                                set = true;
                                if (m != 0)
                                    setpublic = m;
                            }
                        }
                        else if (sv is BlockStatement)
                        {
                            if (_get)
                                getBlock = sv;
                            else
                                setBlock = sv;
                        }
                    }

                }
                else
                {
                    logger.LogError("not support class property element:" + v.GetType().Name + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }
            }
        }
    }


}
