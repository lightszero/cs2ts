using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    public interface ILogger
    {
        void Log(string str);
        void LogWarnning(string str);
        void LogError(string str);
    }
    public class ConvProj
    {
        ILogger logger;
        //翻译的单位
        public Dictionary<string, SyntaxTree> mapCode = new Dictionary<string, SyntaxTree>();
        public Dictionary<string, byte[]> dlls;

        public Dictionary<string, TypeInfo> types = new Dictionary<string, TypeInfo>();
        public TypeInfo getTypeInfo(string fullname)
        {
            if (types.ContainsKey(fullname))
                return types[fullname];
            else
            {

                Type t = Type.GetType(fullname);
                if (t == null)
                    return null;
                RegSysType(fullname, t);

                return types[fullname];
            }
            return null;
        }
        void RegSysType(string fullname, Type t)
        {
            TypeInfo info = new TypeInfo();
            info.fullname = fullname;
            info.isusercode = false;
            if (t.BaseType == typeof(Delegate) || t.BaseType == typeof(MulticastDelegate))
            {
                info.classtype = "delegate";
            }
            else if (t.IsValueType)
                info.classtype = "struct";
            else if (t.IsInterface)
                info.classtype = "interface";
            else
                info.classtype = "class";
            types[fullname] = info;
        }
        public string FindType(string type, string _namespace, string _curClass, List<string> _using, Dictionary<string, string> _usingAlias)
        {
            //UserType
            if (types.ContainsKey(type))//==
                return type;

            if (types.ContainsKey(_namespace + "." + type))//==namespace+type
                return _namespace + "." + type;

            if (types.ContainsKey(_namespace + "." + _curClass + "." + type))//==namespace+type
                return _namespace + "." + _curClass + "." + type;
            if (_usingAlias != null)//别名，直接返回
            {
                if (_usingAlias.ContainsKey(type))
                    return type;
            }
            if (_using != null)
            {
                foreach (var u in _using)
                {
                    if (types.ContainsKey(u + "." + type))
                    {
                        return u + "." + type;
                    }
                }
                //SysType

                foreach (var u in _using)
                {
                    string sstype = u + "." + type;
                    var _t = Type.GetType(sstype);
                    if (_t != null)
                    {
                        RegSysType(sstype, _t);
                        if (_t.BaseType == typeof(Delegate) || _t.BaseType == typeof(MulticastDelegate))
                        {
                            return "any";
                        }
                        if (_t == typeof(string))
                            return "string";
                        if (_t == typeof(object))
                            return "any";
                        if (_t == typeof(bool))
                            return "boolean";
                        if (_t == typeof(int) || _t == typeof(uint) || _t == typeof(long) || _t == typeof(ulong)
                            || _t == typeof(short) || _t == typeof(ushort)
                            || _t == typeof(byte) || _t == typeof(sbyte)
                            || _t == typeof(float) || _t == typeof(double))
                            return "number";

                        return sstype;
                    }
                }
            }

            return null;
        }

        public string GenTypeScript(string srcfile)
        {
            if (mapCode.ContainsKey(srcfile) == false)
            {
                return null;
            }
            var file = mapCode[srcfile];
            StringBuilder sb = new StringBuilder();
            CodeContent content = new CodeContent(this, file, logger, sb);
            content.ParseFile();
            return sb.ToString();
        }
    }
}
