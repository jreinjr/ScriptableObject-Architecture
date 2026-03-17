using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace ScriptableObjectArchitecture.Editor
{
    [CustomEditor(typeof(BaseCollection), true)]
    public class CollectionEditor : UnityEditor.Editor
    {
        private BaseCollection Target { get { return (BaseCollection)target; } }
        private SerializedProperty CollectionItemsProperty
        {
            get { return serializedObject.FindProperty(LIST_PROPERTY_NAME);}
        }

        private ReorderableList _reorderableList;
        private StackTrace _stackTrace;

        // UI
        private const bool DISABLE_ELEMENTS = false;
        private const bool ELEMENT_DRAGGABLE = true;
        private const bool LIST_DISPLAY_HEADER = true;
        private const bool LIST_DISPLAY_ADD_BUTTON = true;
        private const bool LIST_DISPLAY_REMOVE_BUTTON = true;

        private GUIContent _titleGUIContent;
        private GUIContent _noPropertyDrawerWarningGUIContent;

        private const string TITLE_FORMAT = "List ({0})";
        private const string NO_PROPERTY_WARNING_FORMAT = "No PropertyDrawer for type [{0}]";

        // Property Names
        private const string LIST_PROPERTY_NAME = "_list";
        private const string ON_ITEM_ADDED_PROPERTY_NAME = "_onItemAddedEvent";
        private const string ON_ITEM_REMOVED_PROPERTY_NAME = "_onItemRemovedEvent";

        private void OnEnable()
        {
            _titleGUIContent = new GUIContent(string.Format(TITLE_FORMAT, Target.Type));
            _noPropertyDrawerWarningGUIContent = new GUIContent(string.Format(NO_PROPERTY_WARNING_FORMAT, Target.Type));

            _reorderableList = new ReorderableList(
                serializedObject,
                CollectionItemsProperty,
                ELEMENT_DRAGGABLE,
                LIST_DISPLAY_HEADER,
                LIST_DISPLAY_ADD_BUTTON,
                LIST_DISPLAY_REMOVE_BUTTON)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = GetHeight,
            };

            if (target is IStackTraceObject stackTraceTarget)
            {
                _stackTrace = new StackTrace(stackTraceTarget, startCollapsed: true);
                _stackTrace.OnRepaint.AddListener(Repaint);
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawEventFields();

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            _reorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (_stackTrace != null)
            {
                if (!SOArchitecturePreferences.IsDebugEnabled)
                    EditorGUILayout.HelpBox("Debug mode disabled\nStack traces will not be filed on collection changes!", MessageType.Warning);

                _stackTrace.Draw();
            }
        }
        private void DrawEventFields()
        {
            SerializedProperty addedProp = serializedObject.FindProperty(ON_ITEM_ADDED_PROPERTY_NAME);
            SerializedProperty removedProp = serializedObject.FindProperty(ON_ITEM_REMOVED_PROPERTY_NAME);

            if (addedProp != null && removedProp != null)
            {
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(addedProp, new GUIContent("On Item Added"));
                EditorGUILayout.PropertyField(removedProp, new GUIContent("On Item Removed"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, _titleGUIContent);
        }
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = SOArchitecture_EditorUtility.GetReorderableListElementFieldRect(rect);
            SerializedProperty property = CollectionItemsProperty.GetArrayElementAtIndex(index);

            EditorGUI.BeginDisabledGroup(DISABLE_ELEMENTS);

            GenericPropertyDrawer.DrawPropertyDrawer(rect, property, Target.Type);

            EditorGUI.EndDisabledGroup();
        }
        private float GetHeight(int index)
        {
            SerializedProperty property = CollectionItemsProperty.GetArrayElementAtIndex(index);

            return GenericPropertyDrawer.GetHeight(property, Target.Type) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}