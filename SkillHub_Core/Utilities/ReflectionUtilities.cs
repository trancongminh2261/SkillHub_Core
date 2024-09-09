using System.Reflection;
using System;

namespace LMSCore.Utilities
{
    public static class ReflectionHelper
    {
        public static object GetPropValue(this object obj, string propName)
        {
            if (obj == null)
                return null;
            string[] nameParts = propName.Split('.');
            if (nameParts.Length == 1)
            {
                if (obj.GetType().GetProperty(propName) == null)
                    return null;
                return obj.GetType().GetProperty(propName).GetValue(obj, null);
            }

            foreach (string part in nameParts)
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }
    }
    public class ReflectionUtilities
    {
    }
}
