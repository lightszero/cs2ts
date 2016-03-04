using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    partial class CodeContent
    {
        ILogger logger;
        //CleanAst.CodeFile file;
        SyntaxTree file;
        StringBuilder builder;
        ConvProj proj;
        public CodeContent(ConvProj proj, SyntaxTree file, ILogger logger, StringBuilder sb)
        {
            this.proj = proj;
            this.file = file;
            this.logger = logger;
            this.builder = sb;
        }
        List<string> listUsing = new List<string>();
        Dictionary<string, string> listUsinAlias = new Dictionary<string, string>();
        public string curNamespace = "";
        public string curClassName;

        int space = 0;
        bool newline = true;
        void Append(string str)
        {
            if (newline)
                for (int i = 0; i < space; i++)
                    builder.Append(" ");
            builder.Append(str);
            newline = false;
        }
        void AppendLine()
        {
            builder.AppendLine();
            newline = true;
        }

        public void ParseFile()
        {
            foreach (var v in file.Children)
            {
                if (v is Comment)
                {
                    Comment _v = v as Comment;

                    Append(_v.ToString().Replace("\r\n", ""));
                }
                else if (v is UsingDeclaration)
                {
                    UsingDeclaration _v = v as UsingDeclaration;
                    string usingname = Tools._getUsingFullName(v);
                    listUsing.Add(usingname);
                    Append("//<C#>using " + usingname + ";");
                }
                else if (v is UsingAliasDeclaration)
                {
                    string usingname = Tools._getUsingFullName(v);
                    string id = Tools._getIdentifier(v);
                    listUsinAlias.Add(id, usingname);
                    Append("import " + id + "=" + usingname + ";");
                }
                else if (v is NewLineNode)
                {
                    AppendLine();
                }
                else if (v is NamespaceDeclaration)
                {
                    string names = Tools._getFirstType(v);
                    string oldCurNameSpace = curNamespace;
                    if (string.IsNullOrEmpty(curNamespace))
                        curNamespace = names;
                    else
                        curNamespace += "." + names;
                    ParseNameSpace(names, v);
                    curNamespace = oldCurNameSpace;
                }
                else if (v is TypeDeclaration)
                {
                    var cname = Tools._getIdentifier(v);
                    string oldclass = curClassName;
                    ParseClass(cname, v);//处理完恢复一下当前classname
                    curClassName = oldclass;
                }
                else
                {
                    logger.LogError("not support file element:" + v.GetType() + "|" + v.NodeType + "|" + v.StartLocation.Line);
                }
            }
        }


        MethodInfo getMethodInfoByMember(string classname, string membvername, string[] param)
        {
            //搜索当前类型
            string type = classname;
            foreach (var m in proj.types[type].methods.Values)
            {
                if (m.name == membvername)
                {
                    if (m._param.Length == param.Length)
                    {
                        bool bmatch = true;
                        for (int i = 0; i < param.Length; i++)
                        {
                            if (m._param[i] == "any" || m._param[i] == param[i])
                            {

                            }
                            else
                            {
                                bmatch = false;
                                break;
                            }
                        }

                        if (bmatch)
                        {
                            return m;
                        }
                    }
                }
            }


            return null;
        }
        //valueLayer
        List<Dictionary<string, string>> valueLayer = new List<Dictionary<string, string>>();
        void PushValueLayer()
        {
            valueLayer.Insert(0, new Dictionary<string, string>());
        }
        void PopValueLayer()
        {
            valueLayer.RemoveAt(0);
        }
        void DefineValue(string name, string type)
        {
            if (type == null)
                throw new Exception("can not define null type value:" + name);
            valueLayer[0].Add(name, type);
        }
        string getValueTypeInLayer(string name)
        {
            foreach (var v in valueLayer)
            {
                if (v.ContainsKey(name))
                    return v[name];
            }
            return null;
        }

        //HelpFunc
        string getMemberTypeName(string type)
        {
            return type;
        }
        string getSimpleTypeName(string type)
        {
            if (type == "var")
                return null;
            string typeinuser = proj.FindType(type, this.curNamespace, this.curClassName, listUsing, listUsinAlias);
            if (typeinuser != null)
                return typeinuser;

            return null;
        }
        string getCurClassName()
        {
            if (string.IsNullOrEmpty(curNamespace))
                return curClassName;
            else
                return curNamespace + "." + curClassName;
        }
        bool isProp(string classname, string name)
        {
            if (classname == null)
                classname = getCurClassName();
            if (proj.types.ContainsKey(classname) == false)
                return false;
            var type = proj.types[getCurClassName()];
            return type.Prop.ContainsKey(name);
        }
        string getMemberFullname(string classname, string membername, out string classtype)
        {
            classtype = getValueTypeInLayer(membername);
            if (classtype != null)
                return membername;
            if (classname == null)
                classname = getCurClassName();
            if (proj.types.ContainsKey(classname) == false)
                return membername;
            var type = proj.types[classname];
            if (type.Field.ContainsKey(membername))
            {
                classtype = type.Field[membername].type;
                return "this." + membername;
            }
            if (type.Prop.ContainsKey(membername))
            {
                classtype = type.Prop[membername].type;
                return "this.get_" + membername + "()";
            }
            //if (membername == null)
            {
                classtype = getSimpleTypeName(membername);
                if (classtype != null)
                    return membername;
            }
            throw new Exception("not find member:" + classname + ":" + membername);

            return membername;
        }
        string getMemberName(string classname, string membername, out string classtype)
        {
            classtype = null;
            if (classname == null)
                classname = getCurClassName();
            if (proj.types.ContainsKey(classname) == false)
            {
                //classtype = proj.types[classname].classtype;
                return membername;
            }
            var type = proj.types[classname];
            if (type.Field.ContainsKey(membername))
            {
                classtype = type.Field[membername].type;
                return membername;
            }
            if (type.Prop.ContainsKey(membername))
            {
                classtype = type.Prop[membername].type;
                return "get_" + membername + "()";
            }
            throw new Exception("not find member:" + classname + ":" + membername);
            return membername;
        }
        string getClassNameFromID(string id)
        {
            //检查临时变量区
            string name = getValueTypeInLayer(id);
            if (name != null)
                return name;
            //检查this变量区
            var type = proj.types[getCurClassName()];
            if (type != null)
            {
                if (type.Field.ContainsKey(id))
                    return type.Field[id].type;
                if (type.Prop.ContainsKey(id))
                    return type.Prop[id].type;
            }
            //检查全局类型
            name = proj.FindType(id, curNamespace, curClassName, listUsing, listUsinAlias);
            if (name != null)
                return name;
            //检查C#类型
            //return "any";//找不到的类型就是any
            throw new Exception("can not got the type about:" + id);
        }
    }


}
