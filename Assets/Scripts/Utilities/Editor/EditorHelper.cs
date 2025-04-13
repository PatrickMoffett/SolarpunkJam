using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorHelper{
    
    #region Reflection helpers.
    public static object GetTargetObjectParentOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        List<string> stringList = new List<string>(elements);
        stringList.RemoveAt(stringList.Count-1);
        foreach (var element in stringList)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    public static MethodInfo GetMethod(object target, string methodName)
    {
        return GetAllMethods(target, m => m.Name.Equals(methodName, 
            StringComparison.InvariantCulture)).FirstOrDefault();
    }

    public static FieldInfo GetField(object target, string fieldName)
    {
        return GetAllFields(target, f => f.Name.Equals(fieldName, 
            StringComparison.InvariantCulture)).FirstOrDefault();
    }
    public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, 
        bool> predicate)
    {
        List<Type> types = new List<Type>()
        {
            target.GetType()
        };

        while (types.Last().BaseType != null)
        {
            types.Add(types.Last().BaseType);
        }

        for (int i = types.Count - 1; i >= 0; i--)
        {
            IEnumerable<FieldInfo> fieldInfos = types[i]
                .GetFields(BindingFlags.Instance | BindingFlags.Static | 
                           BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(predicate);

            foreach (var fieldInfo in fieldInfos)
            {
                yield return fieldInfo;
            }
        }
    }
    public static IEnumerable<MethodInfo> GetAllMethods(object target, 
        Func<MethodInfo, bool> predicate)
    {
        IEnumerable<MethodInfo> methodInfos = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | 
                        BindingFlags.NonPublic | BindingFlags.Public)
            .Where(predicate);

        return methodInfos;
    }
    public static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }
    public static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }
    #endregion
}
