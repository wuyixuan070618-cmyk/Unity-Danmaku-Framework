using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class BezierMoveConfigEditorWindow : EditorWindow
{
    BulletDefinitionSO selectedDefinition;
    BezierMoveConfigSO bezierConfig;
    SerializedObject serializedConfig;
    SerializedProperty controlPointsProperty;
    Transform previewOrigin;
    bool showCurve = true;
    int curveSamples = 64;

    [MenuItem("Tools/Bullet/Bezier Path Editor")]
    static void OpenWindow()
    {
        var w = GetWindow<BezierMoveConfigEditorWindow>("Bezier Path Editor");
        w.minSize = new Vector2(300, 200);
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ClearBinding();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bezier Movement Control Points", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        selectedDefinition = (BulletDefinitionSO)EditorGUILayout.ObjectField("Bullet Definition", selectedDefinition, typeof(BulletDefinitionSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            BindDefinition(selectedDefinition);
            RestartGuiLayout();
        }

        if (selectedDefinition == null)
        {
            EditorGUILayout.HelpBox("Select a BulletDefinitionSO that uses a BezierMoveConfigSO.", MessageType.Info);
            return;
        }

        if (bezierConfig == null)
        {
            EditorGUILayout.HelpBox("The selected definition does not use a BezierMoveConfigSO.", MessageType.Warning);
            return;
        }

        if (controlPointsProperty != null && controlPointsProperty.arraySize < 2)
        {
            EditorGUILayout.HelpBox(
                "Bezier movement requires at least two control points.",
                MessageType.Error);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Use Selected As Origin"))
        {
            previewOrigin = Selection.activeTransform;
        }
        if (GUILayout.Button("Clear Origin"))
        {
            previewOrigin = null;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        showCurve = EditorGUILayout.Toggle("Show Curve in Scene", showCurve);
        curveSamples = EditorGUILayout.IntSlider("Curve Samples", curveSamples, 8, 256);

        EditorGUILayout.Space();
        if (controlPointsProperty != null)
        {
            serializedConfig.Update();
            EditorGUILayout.LabelField($"Points: {controlPointsProperty.arraySize}", EditorStyles.miniLabel);

            for (int i = 0; i < controlPointsProperty.arraySize; i++)
            {
            SerializedProperty elem = controlPointsProperty.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            elem.vector3Value = EditorGUILayout.Vector3Field($"P{i}", elem.vector3Value);
            bool deletePoint = GUILayout.Button("X", GUILayout.Width(24));
            EditorGUILayout.EndHorizontal();

            if (deletePoint)
            {
                controlPointsProperty.DeleteArrayElementAtIndex(i);
                ApplyConfigChanges();
                RestartGuiLayout();
            }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Point"))
                {
                    Vector3 newPoint = controlPointsProperty.arraySize > 0
                        ? controlPointsProperty
                            .GetArrayElementAtIndex(controlPointsProperty.arraySize - 1)
                            .vector3Value
                        : Vector3.zero;
                    controlPointsProperty.arraySize++;
                    controlPointsProperty
                        .GetArrayElementAtIndex(controlPointsProperty.arraySize - 1)
                        .vector3Value = newPoint;
                    ApplyConfigChanges();
                    RestartGuiLayout();
                }
                if (GUILayout.Button("Clear"))
                {
                    controlPointsProperty.arraySize = 0;
                    ApplyConfigChanges();
                    RestartGuiLayout();
                }
                if (GUILayout.Button("Save Asset"))
                {
                    ApplyConfigChanges();
                    AssetDatabase.SaveAssets();
                }
            }

            if (serializedConfig.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(bezierConfig);
                SceneView.RepaintAll();
            }
        }
    }

    void BindDefinition(BulletDefinitionSO definition)
    {
        ClearBinding();
        selectedDefinition = definition;
        if(selectedDefinition==null)
            return;
        bezierConfig=selectedDefinition.movementConfig as BezierMoveConfigSO;
        if(bezierConfig==null)
            return;
        serializedConfig = new SerializedObject(bezierConfig);
        controlPointsProperty = serializedConfig.FindProperty("controlPoints");
    }

    void ClearBinding()
    {
        serializedConfig = null;
        controlPointsProperty = null;
        bezierConfig=null;
    }

    void ApplyConfigChanges()
    {
        serializedConfig.ApplyModifiedProperties();
        EditorUtility.SetDirty(bezierConfig);
        SceneView.RepaintAll();
    }

    void RestartGuiLayout()
    {
        Repaint();
        SceneView.RepaintAll();
        GUIUtility.ExitGUI();
    }

    void OnSceneGUI(SceneView sv)
    {
        if (selectedDefinition == null || serializedConfig == null || bezierConfig == null || controlPointsProperty == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        // Local control points are offset from the selected preview origin.
        Vector3 origin = Vector3.zero;
        if (previewOrigin != null) origin = previewOrigin.position;
        else origin = Vector3.zero;

        // Read the latest serialized control points before drawing the scene handles.
        serializedConfig.Update();
        var pts = new List<Vector3>();
        for (int i = 0; i < controlPointsProperty.arraySize; i++)
            pts.Add(controlPointsProperty.GetArrayElementAtIndex(i).vector3Value);

        // Draw the sampled Bezier curve.
        if (showCurve && pts.Count > 0)
        {
            Handles.color = Color.green;
            Vector3 prev =  origin + EvaluateBezierList(pts, 0f);
            for (int i = 1; i <= curveSamples; i++)
            {
                float t = i / (float)curveSamples;
                Vector3 pLocal = EvaluateBezierList(pts, t);
                Vector3 p =  origin + pLocal ;
                Handles.DrawLine(prev, p);
                prev = p;
            }
        }

        // Draw draggable handles for every control point.
        Handles.color = Color.cyan;
        for (int i = 0; i < pts.Count; i++)
        {
            Vector3 local = pts[i];
            Vector3 world = origin + local;

            Handles.Label(world + Vector3.up * 0.12f, $"P{i}");
            EditorGUI.BeginChangeCheck();
            Vector3 newWorld = Handles.PositionHandle(world, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezierConfig, "Move Bezier Control Point");
                // Control points are always stored as offsets from the spawn origin.
                Vector3 newLocal = newWorld - origin;
                controlPointsProperty.GetArrayElementAtIndex(i).vector3Value = newLocal;
                ApplyConfigChanges();
            }

            Handles.DotHandleCap(0, world, Quaternion.identity, HandleUtility.GetHandleSize(world) * 0.05f, EventType.Repaint);

            // draw lines between control points
            if (i > 0)
            {
                Vector3 prevLocal = controlPointsProperty.GetArrayElementAtIndex(i - 1).vector3Value;
                Vector3 prevWorld =  origin + prevLocal;
                Handles.DrawLine(prevWorld, world);
            }
        }
    }

    // de Casteljau for List<Vector3>
    static Vector3 EvaluateBezierList(List<Vector3> pts, float t)
    {
        int n = pts != null ? pts.Count : 0;
        if (n == 0) return Vector3.zero;
        if (n == 1) return pts[0];
        Vector3[] tmp = new Vector3[n];
        for (int i = 0; i < n; i++) tmp[i] = pts[i];
        for (int k = 1; k < n; k++)
            for (int i = 0; i < n - k; i++)
                tmp[i] = Vector3.Lerp(tmp[i], tmp[i + 1], t);
        return tmp[0];
    }
}
