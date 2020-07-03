using UnityEditor;
using UnityEngine;

namespace LuviKunG.Editor
{
    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    public class TransformEditor : UnityEditor.Editor
    {
        private const float FIELD_WIDTH = 212.0f;
        private const float POSITION_MAX = 100000.0f;
#if UNITY_2019_3
        private const float BUTTON_WIDTH = 20.0f;
#else
        private const float BUTTON_WIDTH = 12.0f;
#endif

        private const string positionWarningText = "Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.";

        private static readonly GUIContent positionGUIContent = new GUIContent("Position", "The local position of this Game Object relative to the parent.");
        private static readonly GUIContent rotationGUIContent = new GUIContent("Rotation", "The local rotation of this Game Object relative to the parent.");
        private static readonly GUIContent scaleGUIContent = new GUIContent("Scale", "The local scaling of this Game Object relative to the parent.");

        private SerializedProperty positionProperty;
        private SerializedProperty rotationProperty;
        private SerializedProperty scaleProperty;
        private GUILayoutOption[] guiLayoutOptionResetButton = new[] { GUILayout.Width(BUTTON_WIDTH) };

        public override void OnInspectorGUI()
        {
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - FIELD_WIDTH; // align field to right of inspector
            }
            serializedObject.Update();
            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                _ = EditorGUILayout.PropertyField(positionProperty, positionGUIContent);
#if UNITY_2019_4
                GUILayout.Space(4.0f);
#endif
                if (ResetButton())
                    positionProperty.vector3Value = Vector3.zero;
            }
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                RotationPropertyField(rotationGUIContent);
#if UNITY_2019_4
                GUILayout.Space(4.0f);
#endif
                if (ResetButton())
                    rotationProperty.quaternionValue = Quaternion.identity;
            }
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                _ = EditorGUILayout.PropertyField(scaleProperty, scaleGUIContent);
#if UNITY_2019_4
                GUILayout.Space(4.0f);
#endif
                if (ResetButton())
                    scaleProperty.vector3Value = Vector3.one;
            }
            if (!ValidatePosition(((Transform)target).position))
                EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            positionProperty = serializedObject.FindProperty("m_LocalPosition");
            rotationProperty = serializedObject.FindProperty("m_LocalRotation");
            scaleProperty = serializedObject.FindProperty("m_LocalScale");
        }

        private bool ValidatePosition(Vector3 position)
        {
            return Mathf.Abs(position.x) <= POSITION_MAX
                && Mathf.Abs(position.y) <= POSITION_MAX
                && Mathf.Abs(position.z) <= POSITION_MAX;
        }

        private void RotationPropertyField(GUIContent content)
        {
            Transform transform = (Transform)targets[0];
            Quaternion localRotation = transform.localRotation;
            foreach (Object t in targets)
            {
                if (!SameRotation(localRotation, ((Transform)t).localRotation))
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                Vector3 eulerAngles = EditorGUILayout.Vector3Field(content, TransformUtils.GetInspectorRotation(transform));
                if (changeScope.changed)
                {
                    Undo.RecordObjects(targets, "Rotation Changed");
                    foreach (Object obj in targets)
                    {
                        Transform t = (Transform)obj;
                        TransformUtils.SetInspectorRotation(t, eulerAngles);
                    }
                    rotationProperty.serializedObject.SetIsDifferentCacheDirty();
                }
            }
            EditorGUI.showMixedValue = false;
        }

        private bool SameRotation(Quaternion rot1, Quaternion rot2)
        {
            if (rot1.x != rot2.x)
                return false;
            if (rot1.y != rot2.y)
                return false;
            if (rot1.z != rot2.z)
                return false;
            if (rot1.w != rot2.w)
                return false;
            return true;
        }

        private bool ResetButton()
        {
            var rect = GUILayoutUtility.GetRect(BUTTON_WIDTH, EditorGUIUtility.singleLineHeight, guiLayoutOptionResetButton);
            rect.x -= 2.0f;
            rect.y += 2.0f;
            return GUI.Button(rect, GUIContent.none, GUI.skin.GetStyle("OL Minus"));
        }
    }
}