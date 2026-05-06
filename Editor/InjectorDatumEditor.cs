using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baruah.Nexus.Attributes;
using UnityEditor;
using UnityEngine;

namespace Baruah.Nexus.Editor
{
    [CustomPropertyDrawer(typeof(InjectorDatum))]
    public class InjectorDatumEditor : PropertyDrawer
    {
        // ── Layout constants ───────────────────────────────────────────────
        private const float LineHeight  = 20f;
        private const float Padding     = 2f;
        private const float IndentWidth = 12f;

        // ── Cached type list ───────────────────────────────────────────────
        private static List<Type> _injectableTypes;
        private static string[]   _typeLabels;

        private static void EnsureTypeCache()
        {
            if (_injectableTypes != null) return;

            _injectableTypes = TypeCache
                .GetTypesWithAttribute<InjectableAttribute>()
                .OrderBy(t => t.FullName)
                .ToList();

            var labels = _injectableTypes.Select(t => t.FullName).ToList();
            labels.Insert(0, "— Select Injectable —");
            _typeLabels = labels.ToArray();
        }

        // ── Height calculation ─────────────────────────────────────────────
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EnsureTypeCache();

            float height = LineHeight + Padding; // dropdown row

            var classNameProp = property.FindPropertyRelative("className");
            var type          = ResolveType(classNameProp.stringValue);
            if (type == null) return height;

            var ctor = PickConstructor(type);
            if (ctor == null) return height;

            foreach (var param in ctor.GetParameters())
                height += TotalRowHeight(param.ParameterType, null) + Padding;

            return height;
        }

        // ── Drawing ────────────────────────────────────────────────────────
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureTypeCache();

            EditorGUI.BeginProperty(position, label, property);

            var classNameProp  = property.FindPropertyRelative("className");
            var parametersProp = property.FindPropertyRelative("parameters");

            // ── Row 0: class dropdown ──────────────────────────────────────
            Rect dropdownRect = new Rect(position.x, position.y, position.width, LineHeight);

            string currentName = classNameProp.stringValue;
            int currentIdx = string.IsNullOrEmpty(currentName)
                ? 0
                : _injectableTypes.FindIndex(t => t.AssemblyQualifiedName == currentName) + 1;

            int newIdx = EditorGUI.Popup(dropdownRect, "Injectable Type", currentIdx, _typeLabels);

            if (newIdx != currentIdx)
            {
                classNameProp.stringValue = newIdx == 0
                    ? ""
                    : _injectableTypes[newIdx - 1].AssemblyQualifiedName;

                RebuildParameters(parametersProp, newIdx == 0 ? null : _injectableTypes[newIdx - 1]);
            }

            // ── Rows 1…n: constructor parameters ──────────────────────────
            var selectedType = ResolveType(classNameProp.stringValue);
            if (selectedType == null) { EditorGUI.EndProperty(); return; }

            var ctor = PickConstructor(selectedType);
            if (ctor == null) { EditorGUI.EndProperty(); return; }

            var ctorParams = ctor.GetParameters();

            if (parametersProp.arraySize != ctorParams.Length)
                RebuildParameters(parametersProp, selectedType);

            float y = dropdownRect.yMax + Padding;

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var   param    = ctorParams[i];
                var   elemProp = parametersProp.GetArrayElementAtIndex(i);
                float rowH     = TotalRowHeight(param.ParameterType, elemProp);

                Rect rowRect = new Rect(
                    position.x + IndentWidth,
                    y,
                    position.width - IndentWidth,
                    rowH);

                DrawParameterField(rowRect, elemProp, param.ParameterType, param.Name);
                y += rowH + Padding;
            }

            EditorGUI.EndProperty();
        }

        // ── Draw a single parameter (recursive) ───────────────────────────
        private void DrawParameterField(Rect rect, SerializedProperty elemProp, Type t, string label)
        {
            var typeNameProp   = elemProp.FindPropertyRelative("typeName");
            var sourceKindProp = elemProp.FindPropertyRelative("sourceKind");
            var paramNameProp  = elemProp.FindPropertyRelative("parameterName");

            typeNameProp.stringValue  = t.AssemblyQualifiedName;
            paramNameProp.stringValue = label;

            // ── [Injectable] → resolved from container ─────────────────────
            if (IsInjectable(t))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Injectable;
                GUI.enabled = false;
                EditorGUI.LabelField(rect, label, $"({t.Name})  resolved from container");
                GUI.enabled = true;
                return;
            }

            // ── Unity Object ───────────────────────────────────────────────
            if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.UnityObject;
                var valueProp = elemProp.FindPropertyRelative("unityObjectValue");
                EditorGUI.ObjectField(rect, valueProp, t, new GUIContent(label));
                return;
            }

            // ── Primitives ─────────────────────────────────────────────────
            if (t == typeof(int))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Primitive;
                var p = elemProp.FindPropertyRelative("intValue");
                p.intValue = EditorGUI.IntField(rect, label, p.intValue);
                return;
            }
            if (t == typeof(float))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Primitive;
                var p = elemProp.FindPropertyRelative("floatValue");
                p.floatValue = EditorGUI.FloatField(rect, label, p.floatValue);
                return;
            }
            if (t == typeof(bool))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Primitive;
                var p = elemProp.FindPropertyRelative("boolValue");
                p.boolValue = EditorGUI.Toggle(rect, label, p.boolValue);
                return;
            }
            if (t == typeof(string))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Primitive;
                var p = elemProp.FindPropertyRelative("stringValue");
                p.stringValue = EditorGUI.TextField(rect, label, p.stringValue);
                return;
            }

            if (t == typeof(Color))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Primitive;
                var p = elemProp.FindPropertyRelative("colorValue");
                rect.height = EditorGUIUtility.singleLineHeight;
                p.colorValue = EditorGUI.ColorField(rect, label, p.colorValue);
                return;
            }

            // ── Custom class/struct ────────────────────────────────────────
            if (IsCustomClass(t))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.CustomClass;

                var childProp = elemProp.FindPropertyRelative("childParameters");
                var fields    = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

                if (childProp.arraySize != fields.Length)
                    RebuildChildParameters(childProp, t);

                // Foldout header on first line
                Rect foldoutRect = new Rect(rect.x, rect.y, rect.width, LineHeight);
                elemProp.isExpanded = EditorGUI.Foldout(foldoutRect, elemProp.isExpanded, label, true);

                if (elemProp.isExpanded)
                {
                    float y = foldoutRect.yMax + Padding;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        var   field     = fields[i];
                        var   childElem = childProp.GetArrayElementAtIndex(i);
                        float rowH      = TotalRowHeight(field.FieldType, childElem);

                        Rect childRect = new Rect(
                            rect.x + IndentWidth,
                            y,
                            rect.width - IndentWidth,
                            rowH);

                        DrawParameterField(childRect, childElem, field.FieldType, field.Name);
                        y += rowH + Padding;
                    }
                }
                return;
            }

            // ── Unsupported ────────────────────────────────────────────────
            GUI.enabled = false;
            EditorGUI.LabelField(rect, label, $"({t.Name})  unsupported type");
            GUI.enabled = true;
        }

        // ── Height helpers ─────────────────────────────────────────────────

        /// Total height for a parameter row, including expanded children.
        private float TotalRowHeight(Type t, SerializedProperty elemProp)
        {
            if (!IsCustomClass(t))
                return LineHeight;

            // Always reserve the foldout header line
            float height = LineHeight + Padding;

            // Only add children height if the foldout is open
            if (elemProp != null && elemProp.isExpanded)
            {
                var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var childProp = elemProp.FindPropertyRelative("childParameters");

                for (int i = 0; i < fields.Length; i++)
                {
                    SerializedProperty childElem = (childProp != null && i < childProp.arraySize)
                        ? childProp.GetArrayElementAtIndex(i)
                        : null;

                    height += TotalRowHeight(fields[i].FieldType, childElem) + Padding;
                }
            }

            return height;
        }

        // ── Rebuild helpers ────────────────────────────────────────────────

        private void RebuildParameters(SerializedProperty parametersProp, Type type)
        {
            if (type == null) { parametersProp.ClearArray(); return; }

            var ctor = PickConstructor(type);
            if (ctor == null) { parametersProp.ClearArray(); return; }

            var ctorParams = ctor.GetParameters();
            parametersProp.arraySize = ctorParams.Length;

            for (int i = 0; i < ctorParams.Length; i++)
                InitParamElement(parametersProp.GetArrayElementAtIndex(i), ctorParams[i].Name, ctorParams[i].ParameterType);
        }

        private void RebuildChildParameters(SerializedProperty childProp, Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            childProp.arraySize = fields.Length;

            for (int i = 0; i < fields.Length; i++)
                InitParamElement(childProp.GetArrayElementAtIndex(i), fields[i].Name, fields[i].FieldType);
        }

        private void InitParamElement(SerializedProperty elemProp, string name, Type t)
        {
            elemProp.FindPropertyRelative("parameterName").stringValue = name;
            elemProp.FindPropertyRelative("typeName").stringValue      = t.AssemblyQualifiedName;

            int kind = IsInjectable(t)                                    ? (int)ParameterSourceKind.Injectable  :
                       typeof(UnityEngine.Object).IsAssignableFrom(t)     ? (int)ParameterSourceKind.UnityObject :
                       IsCustomClass(t)                                    ? (int)ParameterSourceKind.CustomClass :
                                                                             (int)ParameterSourceKind.Primitive;

            elemProp.FindPropertyRelative("sourceKind").enumValueIndex = kind;

            // Recursively init children for custom classes
            if (IsCustomClass(t))
                RebuildChildParameters(elemProp.FindPropertyRelative("childParameters"), t);
        }

        // ── Utility ────────────────────────────────────────────────────────

        private static ConstructorInfo PickConstructor(Type type)
        {
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (ctors.Length == 0) return null;
            return ctors.FirstOrDefault(c => c.GetCustomAttribute<InjectAttribute>() != null) ?? ctors[0];
        }

        private static Type ResolveType(string assemblyQualifiedName) =>
            string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName);

        private static bool IsInjectable(Type t) =>
            t.GetCustomAttribute<InjectableAttribute>() != null;

        /// Any plain C# class or struct that isn't primitive, Unity object, or [Injectable].
        private static bool IsCustomClass(Type t) =>
            !t.IsPrimitive &&
            t != typeof(string) &&
            !typeof(UnityEngine.Object).IsAssignableFrom(t) &&
            !IsInjectable(t) &&
            (t.IsClass || t.IsValueType) &&
            !t.IsEnum;
    }
}
