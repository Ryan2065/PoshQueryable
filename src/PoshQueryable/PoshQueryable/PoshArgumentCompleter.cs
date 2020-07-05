using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace PoshQueryable
{
    public static class PoshArgumentCompleter
    {
        public static object GetGenericObject(object InputArray)
        {
            var ienum = (IEnumerable)InputArray;
            var qu = ienum.AsQueryable();
            var arguments = qu.GetType().GetTypeInfo().GenericTypeArguments;
            if (arguments.Count() > 0)
            {
                var genericType = arguments[0];
                return FormatterServices.GetUninitializedObject(genericType);
            }
            return null;
        }
    }
}
