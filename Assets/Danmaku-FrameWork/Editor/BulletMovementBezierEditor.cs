/*
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(BulletMovementBezier))]
public class BulletMovementBezierEditor : Editor
{
    SerializedProperty controlPointsProp;
    SerializedProperty useLocalProp;
    SerializedProperty durationProp;
    SerializedProperty easingProp;
    SerializedProperty parentProp;
    SerializedProperty continueStraightProp;
    SerializedProperty straightSpeedProp;
    SerializedProperty straightDurationProp;

    // Pattern asset 编辑支持
    BulletPatternSO editPatternAsset;
    SerializedObject patternSO;
    SerializedProperty patternControlPointsProp;

    void OnEnable()
    {
        controlPointsProp = serializedObject.FindProperty("controlPoints");
        useLocalProp = serializedObject.FindProperty("useLocal");
        durationProp = serializedObject.FindProperty("duration");
        easingProp = serializedObject.FindProperty("easing");
        parentProp = serializedObject.FindProperty("parentTrans");
        continueStraightProp = serializedObject.FindProperty("continueStraight");
        straightSpeedProp = serializedObject.FindProperty("straightSpeed");
        straightDurationProp = serializedObject.FindProperty("straightDuration");
        RefreshPatternSOIfNeeded();
    }

    void RefreshPatternSOIfNeeded()
    {
        if (editPatternAsset != null)
        {
            patternSO = new SerializedObject(editPatternAsset);
            patternControlPointsProp = patternSO.FindProperty("controlPoints");
        }
        else
        {
            patternSO = null;
            patternControlPointsProp = null;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(useLocalProp);
        EditorGUILayout.PropertyField(durationProp);
        EditorGUILayout.PropertyField(easingProp);
        EditorGUILayout.PropertyField(continueStraightProp);
        EditorGUILayout.PropertyField(straightSpeedProp);
        EditorGUILayout.PropertyField(straightDurationProp);

        bool editingPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(serializedObject.targetObject);
        parentProp.objectReferenceValue =
            EditorGUILayout.ObjectField(new GUIContent("Parent Trans", "父 Transform（场景对象或 prefab 内部对象）"),
                                       parentProp.objectReferenceValue,
                                       typeof(Transform),
                                       allowSceneObjects: !editingPrefabAsset);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Control Points (Instance)", EditorStyles.boldLabel);

        // 实例 controlPoints 编辑
        for (int i = 0; i < controlPointsProp.arraySize; i++)
        {
            SerializedProperty elem = controlPointsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(elem, new GUIContent($"Point {i}"));
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                controlPointsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Point"))
        {
            controlPointsProp.arraySize++;
            controlPointsProp.GetArrayElementAtIndex(controlPointsProp.arraySize - 1).vector3Value = Vector3.zero;
        }
        if (GUILayout.Button("Clear"))
        {
            controlPointsProp.arraySize = 0;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("PatternSO Asset Editing", EditorStyles.boldLabel);

        // 选择要编辑的 PatternSO asset
        EditorGUI.BeginChangeCheck();
        editPatternAsset = (BulletPatternSO)EditorGUILayout.ObjectField("Edit Pattern Asset", editPatternAsset, typeof(BulletPatternSO), false);
        if (EditorGUI.EndChangeCheck()) RefreshPatternSOIfNeeded();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Pattern -> Instance"))
        {
            if (editPatternAsset != null)
            {
                Undo.RecordObject(target, "Copy Pattern To Instance");
                var bez = (BulletMovementBezier)target;
                bez.controlPoints = editPatternAsset.controlPoints != null ? (Vector3[])editPatternAsset.controlPoints.Clone() : new Vector3[0];
                EditorUtility.SetDirty(bez);
                serializedObject.Update();
                controlPointsProp = serializedObject.FindProperty("controlPoints");
            }
            else Debug.LogWarning("No pattern asset selected to copy from.");
        }
        if (GUILayout.Button("Apply Instance -> Pattern"))
        {
            if (editPatternAsset != null)
            {
                var bez = (BulletMovementBezier)target;
                Undo.RecordObject(editPatternAsset, "Apply Instance To Pattern");
                editPatternAsset.controlPoints = bez.controlPoints != null ? (Vector3[])bez.controlPoints.Clone() : new Vector3[0];
                EditorUtility.SetDirty(editPatternAsset);
                AssetDatabase.SaveAssets();
                RefreshPatternSOIfNeeded();
            }
            else Debug.LogWarning("No pattern asset selected to apply to.");
        }
        EditorGUILayout.EndHorizontal();

        // 编辑 asset 的 controlPoints（若选中 asset）
        if (patternSO != null)
        {
            patternSO.Update();
            EditorGUILayout.LabelField($"Editing Asset: {editPatternAsset.name}", EditorStyles.helpBox);

            for (int i = 0; i < patternControlPointsProp.arraySize; i++)
            {
                SerializedProperty elem = patternControlPointsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(elem, new GUIContent($"A{i}"));
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    patternControlPointsProp.DeleteArrayElementAtIndex(i);
                    patternSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(editPatternAsset);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Point to Asset"))
            {
                patternControlPointsProp.arraySize++;
                patternControlPointsProp.GetArrayElementAtIndex(patternControlPointsProp.arraySize - 1).vector3Value = Vector3.zero;
            }
            if (GUILayout.Button("Clear Asset Points"))
            {
                patternControlPointsProp.arraySize = 0;
            }
            EditorGUILayout.EndHorizontal();

            patternSO.ApplyModifiedProperties();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Scene 视图拖拽：若正在编辑 asset 则作用于 asset，否则作用于实例
    void OnSceneGUI()
    {
        serializedObject.Update();
        var bez = target as BulletMovementBezier;
        if (bez == null) return;

        // origin 用于把局部点转换为世界坐标（useLocal 语义）
        Vector3 origin = bez.useLocal ? bez.transform.position : Vector3.zero;

        Handles.color = Color.cyan;
        if (patternSO != null && patternControlPointsProp != null)
        {
            patternSO.Update();
            var pts = GetPointsFromSerializedArray(patternControlPointsProp);
            DrawBezierCurve(pts, editPatternAsset.useLocal ? origin : Vector3.zero, editPatternAsset.useLocal);
            for (int i = 0; i < patternControlPointsProp.arraySize; i++)
            {
                SerializedProperty elem = patternControlPointsProp.GetArrayElementAtIndex(i);
                Vector3 localPos = elem.vector3Value;
                Vector3 worldPos = editPatternAsset.useLocal ? (origin + localPos) : localPos;

                Handles.Label(worldPos + Vector3.up * 0.1f, $"A{i}");
                EditorGUI.BeginChangeCheck();
                Vector3 newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(editPatternAsset, "Move Pattern Control Point");
                    Vector3 newLocal = editPatternAsset.useLocal ? (newWorld - origin) : newWorld;
                    elem.vector3Value = newLocal;
                    patternSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(editPatternAsset);
                }

                Handles.DotHandleCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.05f, EventType.Repaint);
                if (i > 0)
                {
                    SerializedProperty prev = patternControlPointsProp.GetArrayElementAtIndex(i - 1);
                    Vector3 prevWorld = editPatternAsset.useLocal ? (origin + prev.vector3Value) : prev.vector3Value;
                    Handles.DrawLine(prevWorld, worldPos);
                }
            }
            patternSO.ApplyModifiedProperties();
        }
        else if (controlPointsProp != null)
        {
            var pts = GetPointsFromSerializedArray(controlPointsProp);
            DrawBezierCurve(pts, bez.useLocal ? origin : Vector3.zero, bez.useLocal);

            for (int i = 0; i < controlPointsProp.arraySize; i++)
            {
                SerializedProperty elem = controlPointsProp.GetArrayElementAtIndex(i);
                Vector3 localPos = elem.vector3Value;
                Vector3 worldPos = bez.useLocal ? (origin + localPos) : localPos;

                Handles.Label(worldPos + Vector3.up * 0.1f, $"P{i}");
                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(bez, "Move Bezier Control Point");
                    Vector3 newLocal = bez.useLocal ? (newWorldPos - origin) : newWorldPos;
                    elem.vector3Value = newLocal;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(bez);
                }

                Handles.DotHandleCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.05f, EventType.Repaint);
                if (i > 0)
                {
                    SerializedProperty prev = controlPointsProp.GetArrayElementAtIndex(i - 1);
                    Vector3 prevWorld = bez.useLocal ? origin + prev.vector3Value : prev.vector3Value;
                    Handles.DrawLine(prevWorld, worldPos);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    // 从 SerializedProperty array 提取点数组（安全处理）
    List<Vector3> GetPointsFromSerializedArray(SerializedProperty arr)
    {
        var list = new List<Vector3>();
        if (arr == null) return list;
        for (int i = 0; i < arr.arraySize; i++)
        {
            var elem = arr.GetArrayElementAtIndex(i);
            list.Add(elem.vector3Value);
        }
        return list;
    }

    // 在 Scene 视图绘制贝塞尔曲线（通用 n 次），通过采样连线近似
    void DrawBezierCurve(List<Vector3> pts, Vector3 origin, bool useLocal)
    {
        if (pts == null || pts.Count == 0) return;
        int steps = 64;
        Vector3 prev = useLocal ? origin + pts[0] : pts[0];
        Handles.color = Color.green;
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pLocal = EvaluateBezier(pts, t);
            Vector3 p = useLocal ? origin + pLocal : pLocal;
            Handles.DrawLine(prev, p);
            prev = p;
        }
    }

    // de Casteljau 实现（不分配 List 版本）
    static Vector3 EvaluateBezier(List<Vector3> pts, float t)
    {
        int n = pts != null ? pts.Count : 0;
        if (n == 0) return Vector3.zero;
        if (n == 1) return pts[0];
        // 使用临时数组栈分配（短期、编辑器中可接受）
        Vector3[] tmp = new Vector3[n];
        for (int i = 0; i < n; i++) tmp[i] = pts[i];
        for (int k = 1; k < n; k++)
            for (int i = 0; i < n - k; i++)
                tmp[i] = Vector3.Lerp(tmp[i], tmp[i + 1], t);
        return tmp[0];
    }
}
*/