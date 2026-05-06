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

        // ── Cached type list (refreshed once per domain reload) ────────────
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
                height += RowHeight(param.ParameterType) + Padding;

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
            int    currentIdx  = string.IsNullOrEmpty(currentName)
                ? 0
                : _injectableTypes.FindIndex(t => t.AssemblyQualifiedName == currentName) + 1;

            int newIdx = EditorGUI.Popup(dropdownRect, "Injectable Type", currentIdx, _typeLabels);

            if (newIdx != currentIdx)
            {
                classNameProp.stringValue = newIdx == 0
                    ? ""
                    : _injectableTypes[newIdx - 1].AssemblyQualifiedName;

                // Rebuild parameter array for the new type
                RebuildParameters(parametersProp, newIdx == 0 ? null : _injectableTypes[newIdx - 1]);
            }

            // ── Rows 1…n: constructor parameters ──────────────────────────
            var selectedType = ResolveType(classNameProp.stringValue);
            if (selectedType == null) { EditorGUI.EndProperty(); return; }

            var ctor = PickConstructor(selectedType);
            if (ctor == null) { EditorGUI.EndProperty(); return; }

            var ctorParams = ctor.GetParameters();

            // Ensure array length matches (handles undo/paste edge cases)
            if (parametersProp.arraySize != ctorParams.Length)
                RebuildParameters(parametersProp, selectedType);

            float y = dropdownRect.yMax + Padding;

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var   param     = ctorParams[i];
                var   elemProp  = parametersProp.GetArrayElementAtIndex(i);
                float rowH      = RowHeight(param.ParameterType);

                Rect rowRect = new Rect(
                    position.x + IndentWidth,
                    y,
                    position.width - IndentWidth,
                    rowH);

                DrawParameterField(rowRect, elemProp, param);
                y += rowH + Padding;
            }

            EditorGUI.EndProperty();
        }

        // ── Draw a single parameter row ────────────────────────────────────
        private void DrawParameterField(Rect rect, SerializedProperty elemProp, ParameterInfo param)
        {
            var typeNameProp    = elemProp.FindPropertyRelative("typeName");
            var sourceKindProp  = elemProp.FindPropertyRelative("sourceKind");
            var paramNameProp   = elemProp.FindPropertyRelative("parameterName");

            // Keep metadata in sync
            typeNameProp.stringValue   = param.ParameterType.AssemblyQualifiedName;
            paramNameProp.stringValue  = param.Name;

            Type t = param.ParameterType;

            // ── [Injectable] type → resolved from container ────────────────
            if (IsInjectable(t))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Injectable;
                GUI.enabled = false;
                EditorGUI.LabelField(rect, param.Name, $"({t.Name})  resolved from container");
                GUI.enabled = true;
                return;
            }

            // ── Unity Object ───────────────────────────────────────────────
            if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                sourceKindProp.enumValueIndex = (int)ParameterSourceKind.UnityObject;
                var valueProp = elemProp.FindPropertyRelative("unityObjectValue");
                EditorGUI.ObjectField(rect, valueProp, t, new GUIContent(param.Name));
                return;
            }

            // ── Primitives ─────────────────────────────────────────────────
            sourceKindProp.enumValueIndex = (int)ParameterSourceKind.Primitive;

            if (t == typeof(int))
            {
                var p = elemProp.FindPropertyRelative("intValue");
                p.intValue = EditorGUI.IntField(rect, param.Name, p.intValue);
            }
            else if (t == typeof(float))
            {
                var p = elemProp.FindPropertyRelative("floatValue");
                p.floatValue = EditorGUI.FloatField(rect, param.Name, p.floatValue);
            }
            else if (t == typeof(bool))
            {
                var p = elemProp.FindPropertyRelative("boolValue");
                p.boolValue = EditorGUI.Toggle(rect, param.Name, p.boolValue);
            }
            else if (t == typeof(string))
            {
                var p = elemProp.FindPropertyRelative("stringValue");
                p.stringValue = EditorGUI.TextField(rect, param.Name, p.stringValue);
            }
            else
            {
                // Unsupported type — show greyed label
                GUI.enabled = false;
                EditorGUI.LabelField(rect, param.Name, $"({t.Name})  unsupported type");
                GUI.enabled = true;
            }
        }

        // ── Rebuild the parameters array to match the selected constructor ─
        private void RebuildParameters(SerializedProperty parametersProp, Type type)
        {
            if (type == null)
            {
                parametersProp.ClearArray();
                return;
            }

            var ctor = PickConstructor(type);
            if (ctor == null)
            {
                parametersProp.ClearArray();
                return;
            }

            var ctorParams = ctor.GetParameters();
            parametersProp.arraySize = ctorParams.Length;

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var elemProp      = parametersProp.GetArrayElementAtIndex(i);
                var param         = ctorParams[i];

                elemProp.FindPropertyRelative("parameterName").stringValue =
                    param.Name;
                elemProp.FindPropertyRelative("typeName").stringValue =
                    param.ParameterType.AssemblyQualifiedName;

                bool isInjectableType = IsInjectable(param.ParameterType);
                bool isUnityObject    = typeof(UnityEngine.Object)
                    .IsAssignableFrom(param.ParameterType);

                elemProp.FindPropertyRelative("sourceKind").enumValueIndex =
                    isInjectableType ? (int)ParameterSourceKind.Injectable  :
                    isUnityObject    ? (int)ParameterSourceKind.UnityObject  :
                                       (int)ParameterSourceKind.Primitive;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        /// Prefer constructor tagged [Inject]; fall back to the first public one.
        private static ConstructorInfo PickConstructor(Type type)
        {
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (ctors.Length == 0) return null;

            return ctors.FirstOrDefault(c =>
                       c.GetCustomAttribute<InjectAttribute>() != null)
                   ?? ctors[0];
        }

        private static Type ResolveType(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return null;
            return Type.GetType(assemblyQualifiedName);
        }

        private static bool IsInjectable(Type t) =>
            t.GetCustomAttribute<InjectableAttribute>() != null;

        /// Single-line for everything; could be expanded for Vector3 etc.
        private static float RowHeight(Type t) => LineHeight;
    }
}
