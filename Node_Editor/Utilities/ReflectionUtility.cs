using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using CabinIcarus.EditorFrame.Utils;

namespace NodeEditorFramework.Utilities
{
    public static class ReflectionUtility
    {
        private static List<Type> _outTypes;

        static ReflectionUtility()
        {
            _outTypes = new List<Type>();
        }

        public class ReflectionSearchIgnoreAttribute : Attribute
        {
            public ReflectionSearchIgnoreAttribute()
            {
            }
        }

        /// <summary>
        /// Gets all non-abstract types extending the given base type
        /// </summary>
        public static Type[] getSubTypes(Type baseType)
        {
            return AssemblyUtil.GetRuntimeType().Where(T => (T.IsClass && !T.IsAbstract)
                                        && T.IsSubclassOf(baseType)
                                        && !T.GetCustomAttributes(typeof(ReflectionSearchIgnoreAttribute), false).Any())
                .ToArray();
        }

        /// <summary>
        /// Gets all non-abstract types extending the given base type and with the given attribute
        /// </summary>
        public static Type[] getSubTypes(Type baseType, Type hasAttribute)
        {
            return AssemblyUtil.GetRuntimeType().Where(T => (T.IsClass && !T.IsAbstract)
                                        && T.IsSubclassOf(baseType)
                                        && T.GetCustomAttributes(hasAttribute, false).Any()
                                        && !T.GetCustomAttributes(typeof(ReflectionSearchIgnoreAttribute), false).Any())
                .ToArray();
        }

        /// <summary>
        /// Returns all fields that should be serialized in the given type
        /// </summary>
        public static FieldInfo[] getSerializedFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where((FieldInfo field) =>
                    (field.IsPublic && !field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Any())
                    || field.GetCustomAttributes(typeof(SerializeField), true).Any()
                    && !field.GetCustomAttributes(typeof(ReflectionSearchIgnoreAttribute), false).Any())
                .ToArray();
        }

        /// <summary>
        /// Returns all fields that should be serialized in the given type, minus the fields declared in or above the given base type
        /// </summary>
        public static FieldInfo[] getSerializedFields(Type type, Type hiddenBaseType)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where((FieldInfo field) =>
                    (hiddenBaseType == null || !field.DeclaringType.IsAssignableFrom(hiddenBaseType))
                    && ((field.IsPublic && !field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Any())
                        || field.GetCustomAttributes(typeof(SerializeField), true).Any()
                        && !field.GetCustomAttributes(typeof(ReflectionSearchIgnoreAttribute), false).Any()))
                .ToArray();
        }

        /// <summary>
        /// Gets all fields in the classType of the specified fieldType
        /// </summary>
        public static FieldInfo[] getFieldsOfType(Type classType, Type fieldType)
        {
            return classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where((FieldInfo field) =>
                    field.FieldType == fieldType || field.FieldType.IsSubclassOf(fieldType)
                    && !field.GetCustomAttributes(typeof(ReflectionSearchIgnoreAttribute), false).Any())
                .ToArray();
        }
    }
}