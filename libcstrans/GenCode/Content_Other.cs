using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs2ts
{
    partial class CodeContent
    {
        string ParseVariableInitializer(VariableInitializer v, string returntype)
        {
            var varname = v.Name;

            if (returntype != null)
                Append(varname + ":" + returntype);
            else
            {
                Append(varname);
            }
            if (v.Initializer != null)
            {

                Append("=");
                var rtype = ParseExpression(v.Initializer, null);
                if (rtype == null)
                    throw new Exception("rtype can not be null.");
                if (rtype != "any" && rtype != "number" && rtype != "bool" && ((v.Initializer is ObjectCreateExpression) == false) && proj.types[rtype].classtype == "struct")
                {
                    Append(".Clone()");
                }
                return rtype;
            }
            return null;
        }
        string ParseIdentifier(string classname, Identifier expr, ExpressOption option, bool bfromthis)//名称表达式
        {
            string classtype = null;
            if (option != null && option.leftPropSet)
            {
                if (isProp(classname, expr.Name))
                {
                    if (bfromthis)
                        Append("this.set_" + expr.Name);
                    else
                        Append("set_" + expr.Name);
                    option.touch++;
                }
                else
                {
                    if (bfromthis)
                        Append(getMemberFullname(classname, expr.Name, out classtype));
                    else
                        Append(getMemberName(classname, expr.Name, out classtype));
                }
            }
            else if (option != null && option.memberCall)
            {
                MethodInfo method = this.getMethodInfoByMember(classname, expr.Name, option.paramType.ToArray());
                //var method = proj.types[getCurClassName()].methods[expr.Children[0].Code];
                if (bfromthis)
                {
                    if (method._static == false)
                    {
                        Append("this." + method.olname);
                    }
                    else
                    {
                        Append(classname + "." + method.olname);
                    }
                }
                else
                {
                    Append(method.olname);
                }
                var type = proj.types[classname];

            }
            else
            {
                if (bfromthis)
                {
                    Append(getMemberFullname(classname, expr.Name, out classtype));
                }
                else
                {
                    Append(getMemberName(classname, expr.Name, out classtype));
                }
            }
            return classtype;
        }
    }


}
