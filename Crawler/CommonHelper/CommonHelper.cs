using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper
{
    public static class AttributeHelper
    {
        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof (T), false).OfType<T>().FirstOrDefault();
        }

        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof (T), false).OfType<T>().FirstOrDefault();
        }

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return GetAttribute<T>(type) != null;
        }

        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return GetAttribute<T>(member) != null;
        }
    }

}
