using System;
using System.Collections.Generic;
using System.Text;

namespace cs2ts
{
    public struct FieldInfo
    {
        public string name;
        public string type;
        public bool _static;
    }
    public class MethodInfo
    {
        public string name;
        public string olname;
        public string type;
        public string[] _param;
        public bool _static;
    }
    public class TypeInfo
    {
        public string modifier = "";
        public string fullname;
        public string classtype;
        public bool isusercode;


        public Dictionary<string, FieldInfo> Prop = new Dictionary<string, FieldInfo>();
        public Dictionary<string, FieldInfo> Field = new Dictionary<string, FieldInfo>();
        public Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        public void AddProp(string name, string type, bool _static)
        {
            FieldInfo f = new FieldInfo();
            f.name = name;
            f.type = type;
            f._static = _static;
            this.Prop[name] = f;
        }
        public void AddField(string name, string type, bool _static)
        {
            FieldInfo f = new FieldInfo();
            f.name = name;
            f.type = type;
            f._static = _static;
            this.Field[name] = f;
        }
        public void AddMethod(string name, string type, bool _static, string[] _param)
        {
            MethodInfo f = new MethodInfo();
            f.name = name;

            f.type = type;
            f._static = _static;
            f._param = _param;
            string key = f.name;
            int i = 1;
            while (methods.ContainsKey(key))
            {
                i++;
                key = f.name + "__" + i;
            }
            f.olname = key;
            this.methods.Add(key, f);
        }
    }

}
