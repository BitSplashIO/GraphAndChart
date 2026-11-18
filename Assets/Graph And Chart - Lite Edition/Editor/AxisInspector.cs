using ChartAndGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ChartAndGraph
{
    [CustomEditor(typeof(AxisBase), true)]
    class AxisInspector : Editor
    {
        bool mMain = false;
        bool mSub = false;

        public override void OnInspectorGUI()
        {
            AxisBase axis = (AxisBase)target;

            if (axis.gameObject == null)
                return;

            AnyChart chart = axis.gameObject.GetComponent<AnyChart>();
            if (chart == null)
                return;
            if ((chart is AxisChart) == false)
            {
                EditorGUILayout.LabelField(string.Format("Chart of type {0} does not support axis", chart.GetType().Name));
                return;
            }
            SerializedProperty simpleViewProp = serializedObject.FindProperty("SimpleView");
            if (simpleViewProp == null)
                return;

            Type canvasType = (chart is ICanvas) ? typeof(CanvasAttribute) : typeof(NonCanvasAttribute);

            EditorGUILayout.BeginVertical();
            bool negate = false;
            //if (simpleViewProp.boolValue == true)
            //    negate = GUILayout.Button("Advanced View");
            //else
            //    negate = GUILayout.Button("Simple View");
            //if (negate)
            //    simpleViewProp.boolValue = !simpleViewProp.boolValue;
            bool simple = false;// simpleViewProp.boolValue;



            SerializedProperty format = serializedObject.FindProperty("format");
            EditorGUILayout.PropertyField(format);
            //SerializedProperty depth = serializedObject.FindProperty("depth");
            //if (depth != null)
            //    EditorGUILayout.PropertyField(depth);
            if (simple)
                DoSimpleView(canvasType);
            else
                DoAdvanvedView(canvasType, true);

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        void SetValue(SerializedProperty assignTo, SerializedProperty from)
        {

            if (assignTo.propertyType != from.propertyType)
            {
                Debug.LogWarning("type does not match");
                return;
            }

            if (assignTo.type == "AutoFloat")
            {
                SerializedProperty auto = assignTo.FindPropertyRelative("Automatic");
                SerializedProperty val = assignTo.FindPropertyRelative("Value");
                SerializedProperty autofrom = from.FindPropertyRelative("Automatic");
                SerializedProperty valfrom = from.FindPropertyRelative("Value");
                auto.boolValue = autofrom.boolValue;
                val.floatValue = valfrom.floatValue;
                return;
            }

            switch (assignTo.propertyType)
            {
                case SerializedPropertyType.Float:
                    assignTo.floatValue = from.floatValue;
                    break;
                case SerializedPropertyType.Integer:
                    assignTo.intValue = from.intValue;
                    break;
                case SerializedPropertyType.Enum:
                    assignTo.enumValueIndex = from.enumValueIndex;
                    break;
                case SerializedPropertyType.Boolean:
                    assignTo.boolValue = from.boolValue;
                    break;
                case SerializedPropertyType.String:
                    assignTo.stringValue = from.stringValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    assignTo.objectReferenceValue = from.objectReferenceValue;
                    break;
                default:
                    Debug.LogWarning("type cannot be set");
                    break;
            }
        }

        Type getTypeFromField(Type type, string fieldName)
        {
            FieldInfo inf = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (inf == null)
                return null;
            return inf.FieldType;
        }

        bool CompareValues(Type type, string fieldName, object a, object b)
        {
            FieldInfo inf = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            object valA = inf.GetValue(a);
            object valB = inf.GetValue(b);
            if (valA is UnityEngine.Object && valB is UnityEngine.Object)
            {
                if (((UnityEngine.Object)valA) == ((UnityEngine.Object)valB))
                    return true;
                return false;
            }
            if (valA == null && valB == null)
                return true;
            if (valA == null || valB == null)
                return false;
            return valA.Equals(valB);
        }

        // finds the backing field of a division property, so its attributes can be read
        FieldInfo FindDivisionField(string fieldName)
        {
            Type[] sources = { typeof(ChartDivisionInfo), typeof(AxisBase) };
            foreach (Type source in sources)
            {
                for (Type t = source; t != null; t = t.BaseType)
                {
                    FieldInfo inf = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (inf != null)
                        return inf;
                }
            }
            return null;
        }

        // Draws the given properties grouped by [Category]. Fields without a category come first, then each
        // category as a foldout (closed by default, state is remembered), ordered by [CategoryOrder].
        // 'draw' is called to draw a single property, so each view keeps its own drawing logic.
        void DrawCategorized(List<SerializedProperty> props, string stateKeyPrefix, Action<SerializedProperty> draw)
        {
            // [CategoryOrder] is read from the division class, the edited axis class and the axis base class
            Dictionary<string, int> categoryOrders = new Dictionary<string, int>();
            Type[] orderSources = { typeof(ChartDivisionInfo), target.GetType(), typeof(AxisBase) };
            foreach (Type orderSource in orderSources)
            {
                foreach (CategoryOrderAttribute orderAttr in orderSource.GetCustomAttributes(typeof(CategoryOrderAttribute), true))
                {
                    if (categoryOrders.ContainsKey(orderAttr.Name) == false)
                        categoryOrders.Add(orderAttr.Name, orderAttr.Order);
                }
            }

            List<SerializedProperty> uncategorized = new List<SerializedProperty>();
            List<string> groupNames = new List<string>();
            Dictionary<string, List<KeyValuePair<int, SerializedProperty>>> groups = new Dictionary<string, List<KeyValuePair<int, SerializedProperty>>>();

            foreach (SerializedProperty prop in props)
            {
                FieldInfo field = FindDivisionField(prop.name);
                if (field != null && field.IsDefined(typeof(HideInInspector), true))
                    continue;

                CategoryAttribute categoryAttr = field != null
                    ? (CategoryAttribute)Attribute.GetCustomAttribute(field, typeof(CategoryAttribute), true)
                    : null;

                if (categoryAttr == null)
                {
                    uncategorized.Add(prop);
                }
                else
                {
                    if (groups.ContainsKey(categoryAttr.Name) == false)
                    {
                        groups.Add(categoryAttr.Name, new List<KeyValuePair<int, SerializedProperty>>());
                        groupNames.Add(categoryAttr.Name);
                    }
                    groups[categoryAttr.Name].Add(new KeyValuePair<int, SerializedProperty>(categoryAttr.SortingOrder, prop));
                }
            }

            // fields without a category first
            foreach (SerializedProperty uncategorizedProp in uncategorized)
                draw(uncategorizedProp);

            // then the categories: by [CategoryOrder], unordered ones last in declaration order (OrderBy is stable)
            foreach (string groupName in groupNames.OrderBy(n => categoryOrders.TryGetValue(n, out int categoryOrder) ? categoryOrder : int.MaxValue))
            {
                EditorGUILayout.Space(2);

                string stateKey = "AxisInspector." + target.GetType().FullName + "." + stateKeyPrefix + "." + groupName;
                bool expanded = EditorPrefs.GetBool(stateKey, false);

                // the header group ignores EditorGUI.indentLevel, so indent its rect manually
                Rect headerRect = EditorGUILayout.GetControlRect();
                Rect indentedHeaderRect = EditorGUI.IndentedRect(headerRect);
                float indentWidth = indentedHeaderRect.xMin - headerRect.xMin;
                expanded = EditorGUI.BeginFoldoutHeaderGroup(indentedHeaderRect, expanded, groupName);
                EditorPrefs.SetBool(stateKey, expanded);

                if (expanded)
                {
                    // indent the box by the same amount; the fields inside it start at its left edge
                    int savedIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(indentWidth);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    foreach (KeyValuePair<int, SerializedProperty> groupEntry in groups[groupName].OrderBy(g => g.Key))
                        draw(groupEntry.Value);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel = savedIndent;
                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

        void DoSimpleView(Type canvasType)
        {
            SerializedProperty it = serializedObject.FindProperty("mainDivisions");
            SerializedProperty SubDivisions = serializedObject.FindProperty("subDivisions");
            object mainDivision = ((AxisBase)target).MainDivisions;
            object subDivision = ((AxisBase)target).SubDivisions;
            if (it == null || SubDivisions == null)
                return;
            SerializedProperty end = it.GetEndProperty();
            List<SerializedProperty> simpleProps = new List<SerializedProperty>();
            while (it.NextVisible(true) && SerializedProperty.EqualContents(end, it) == false)
            {
                if (it.name == "SimpleView")
                    continue;
                if (ChartEditorCommon.HasAttributeOfType(typeof(ChartDivisionInfo), it.name, canvasType) == false)
                    if (ChartEditorCommon.HasAttributeOfType(typeof(AxisBase), it.name, canvasType) == false)
                        continue;
                if (ChartEditorCommon.HasAttributeOfType(typeof(AxisBase), it.name, typeof(SimpleAttribute)) == false)
                    if (ChartEditorCommon.HasAttributeOfType(typeof(ChartDivisionInfo), it.name, typeof(SimpleAttribute)) == false)
                        continue;
                simpleProps.Add(it.Copy());
            }

            // each simple property is edited on the main divisions and copied to the sub divisions
            DrawCategorized(simpleProps, "Simple", (SerializedProperty prop) =>
            {
                SerializedProperty clone = SubDivisions.FindPropertyRelative(prop.name);
                if (clone == null)
                    Debug.LogWarning("can't find property " + prop.name);
                bool equal = CompareValues(typeof(ChartDivisionInfo), clone.name, mainDivision, subDivision);
                Type t = getTypeFromField(typeof(ChartDivisionInfo), clone.name);
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = !equal;
                DoMixedFiled(prop, t);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                    SetValue(clone, prop);
            });

            DoAdvanvedView(canvasType, false);
        }
        public void DoMixedFiled(SerializedProperty prop, Type type)
        {
            if (prop.type == "AutoFloat")
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(prop.displayName);
                SerializedProperty auto = prop.FindPropertyRelative("Automatic");
                SerializedProperty val = prop.FindPropertyRelative("Value");
                auto.boolValue = EditorGUILayout.ToggleLeft("Auto", auto.boolValue);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                if (auto.boolValue == false && EditorGUI.showMixedValue == false)
                    val.floatValue = EditorGUILayout.FloatField("Value", val.floatValue);
                EditorGUI.indentLevel--;

                return;
            }
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Float:
                    if (prop.name == "FontSharpness")
                        prop.floatValue = EditorGUILayout.Slider(prop.displayName, prop.floatValue, 1f, 3f);
                    else
                        prop.floatValue = EditorGUILayout.FloatField(prop.displayName, prop.floatValue);
                    break;
                case SerializedPropertyType.Integer:
                    if (prop.name == "FractionDigits")
                        prop.intValue = EditorGUILayout.IntSlider(prop.displayName, prop.intValue, 0, 7);
                    else
                        prop.intValue = EditorGUILayout.IntField(prop.displayName, prop.intValue);
                    break;
                case SerializedPropertyType.Enum:
                    ChartDivisionAligment selected = (ChartDivisionAligment)Enum.Parse(typeof(ChartDivisionAligment), prop.enumNames[prop.enumValueIndex]);
                    Enum res = EditorGUILayout.EnumPopup(prop.displayName, selected);
                    string newName = Enum.GetName(typeof(ChartDivisionAligment), res);
                    for (int i = 0; i < prop.enumNames.Length; ++i)
                    {
                        if (prop.enumNames[i] == newName)
                        {
                            prop.enumValueIndex = i;
                            break;
                        }
                    }
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = EditorGUILayout.TextField(prop.displayName, prop.stringValue);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = EditorGUILayout.Toggle(prop.displayName, prop.boolValue);
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (type != null)
                        prop.objectReferenceValue = EditorGUILayout.ObjectField(prop.displayName, prop.objectReferenceValue, type, true);
                    break;
                default:
                    Debug.LogWarning("type cannot be set");
                    break;
            }
        }

        void DoAdvanvedView(Type canvasType, bool includeSimple)
        {
            SerializedProperty mainDivisions = serializedObject.FindProperty("mainDivisions");
            SerializedProperty subDivisions = serializedObject.FindProperty("subDivisions");
            mMain = mainDivisions.isExpanded = EditorGUILayout.Foldout(mainDivisions.isExpanded, "Main Divisions");
            if (mMain)
            {
                EditorGUI.indentLevel++;

                var spacing = mainDivisions.FindPropertyRelative("spacingType");
                EditorGUILayout.PropertyField(spacing);
                //EditorGUI.indentLevel++;
                if (spacing.intValue == 0)
                    EditorGUILayout.PropertyField(mainDivisions.FindPropertyRelative("divsionCount"));
                else
                    EditorGUILayout.PropertyField(mainDivisions.FindPropertyRelative("unitsPerDivision"));
                //EditorGUI.indentLevel--;
                SerializedProperty end = mainDivisions.GetEndProperty();
                List<SerializedProperty> mainProps = new List<SerializedProperty>();
                while (mainDivisions.NextVisible(true) && SerializedProperty.EqualContents(mainDivisions, end) == false)
                {
                    if (mainDivisions.name == "spacingType")
                        continue;
                    if (mainDivisions.name == "unitsPerDivision")
                        continue;
                    if (mainDivisions.name == "divsionCount")
                        continue;
                    if (ChartEditorCommon.HasAttributeOfType(typeof(ChartDivisionInfo), mainDivisions.name, canvasType) == false)
                        if (ChartEditorCommon.HasAttributeOfType(typeof(AxisBase), mainDivisions.name, canvasType) == false)
                            continue;
                    if (includeSimple == false)
                    {
                        if (ChartEditorCommon.HasAttributeOfType(typeof(ChartDivisionInfo), mainDivisions.name, typeof(SimpleAttribute)))
                            continue;
                    }
                    mainProps.Add(mainDivisions.Copy());
                }
                DrawCategorized(mainProps, "Main", (SerializedProperty prop) => EditorGUILayout.PropertyField(prop));
                EditorGUI.indentLevel--;
            }
            mSub = subDivisions.isExpanded = EditorGUILayout.Foldout(subDivisions.isExpanded, "Sub Divisions");
            if (mSub)
            {
                EditorGUI.indentLevel++;
                SerializedProperty end = subDivisions.GetEndProperty();
                List<SerializedProperty> subProps = new List<SerializedProperty>();
                while (subDivisions.NextVisible(true) && SerializedProperty.EqualContents(subDivisions, end) == false)
                {
                    if (subDivisions.name == "spacingType")
                        continue;
                    if (subDivisions.name == "unitsPerDivision")
                        continue;
                    if (ChartEditorCommon.HasAttributeOfType(typeof(ChartDivisionInfo), subDivisions.name, canvasType) == false)
                        if (ChartEditorCommon.HasAttributeOfType(typeof(AxisBase), subDivisions.name, canvasType) == false)
                            continue;
                    if (includeSimple == false)
                    {
                        if (ChartEditorCommon.HasAttributeOfType(typeof(ChartDivisionInfo), subDivisions.name, typeof(SimpleAttribute)))
                            continue;
                    }
                    subProps.Add(subDivisions.Copy());
                }
                DrawCategorized(subProps, "Sub", (SerializedProperty prop) => EditorGUILayout.PropertyField(prop));
                EditorGUI.indentLevel--;
            }
        }
    }
}