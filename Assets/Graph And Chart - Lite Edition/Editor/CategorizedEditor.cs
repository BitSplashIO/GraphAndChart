using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CategorizedEditor : Editor
{
    private class Entry
    {
        public SerializedProperty Property;
        public int SortingOrder;
    }

    private class CategoryGroup
    {
        public string Name;
        public int Order;
        public int Appearance;
        public readonly List<Entry> Entries = new List<Entry>();
    }

    protected void DrawCustomizedInspector()
    {
        serializedObject.UpdateIfRequiredOrScript();

        var categoryOrders = new Dictionary<string, int>();
        foreach (CategoryOrderAttribute attr in
                 target.GetType().GetCustomAttributes(typeof(CategoryOrderAttribute), true))
        {
            if (!categoryOrders.ContainsKey(attr.Name))
                categoryOrders.Add(attr.Name, attr.Order);
        }

        var uncategorized = new List<SerializedProperty>();
        var groups = new Dictionary<string, CategoryGroup>();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.propertyPath == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.PropertyField(iterator, true);
                continue;
            }

            FieldInfo field = null;
            for (Type t = target.GetType(); t != null && field == null; t = t.BaseType)
            {
                field = t.GetField(iterator.name,
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            }

            if (field != null && field.IsDefined(typeof(HideInInspector), true))
                continue;

            CategoryAttribute category = field != null
                ? (CategoryAttribute)Attribute.GetCustomAttribute(field, typeof(CategoryAttribute), true)
                : null;

            SerializedProperty copy = iterator.Copy();

            if (category == null)
            {
                uncategorized.Add(copy);
                continue;
            }

            if (!groups.TryGetValue(category.Name, out CategoryGroup group))
            {
                group = new CategoryGroup
                {
                    Name = category.Name,
                    Order = categoryOrders.TryGetValue(category.Name, out int order) ? order : int.MaxValue,
                    Appearance = groups.Count
                };
                groups.Add(category.Name, group);
            }

            group.Entries.Add(new Entry { Property = copy, SortingOrder = category.SortingOrder });
        }

        foreach (SerializedProperty property in uncategorized)
            EditorGUILayout.PropertyField(property, true);

        foreach (CategoryGroup group in groups.Values.OrderBy(g => g.Order).ThenBy(g => g.Appearance))
        {
            EditorGUILayout.Space(2);


            string stateKey = "CategorizedEditor." + target.GetType().FullName + "." + group.Name;
            bool expanded = EditorPrefs.GetBool(stateKey, false);

            expanded = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, group.Name);
            EditorPrefs.SetBool(stateKey, expanded);

            if (expanded)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                foreach (Entry entry in group.Entries.OrderBy(e => e.SortingOrder))
                    EditorGUILayout.PropertyField(entry.Property, true);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}