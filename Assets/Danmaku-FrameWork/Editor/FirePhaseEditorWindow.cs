using UnityEditor;
using UnityEngine;

/// <summary>
/// FirePhaseSO 独立浮窗编辑器 — 不依赖选中资产，在独立窗口中编辑弹幕阶段。
/// 使用方式：菜单栏 Tools → Fire Phase Editor
/// </summary>
public class FirePhaseEditorWindow : EditorWindow
{
    private FirePhaseSO editPhase;
    private SerializedObject phaseSO;

    private SerializedProperty durationProp;
    private SerializedProperty shapeProp;
    private SerializedProperty bulletCountProp;
    private SerializedProperty angleSpreadProp;
    private SerializedProperty offsetAngleProp;
    private SerializedProperty aimTypeProp;
    private SerializedProperty bulletPrefabProp;
    private SerializedProperty bulletDefinitionProp;
    private SerializedProperty fireIntervalProp;
    private SerializedProperty rotationSpeedProp;
    private SerializedProperty randomizeAngleProp;
    private SerializedProperty randomRangeProp;

    private Transform previewOrigin;
    private Vector2 scrollPos;

    [MenuItem("Tools/Fire Phase Editor")]
    static void OpenWindow()
    {
        var w = GetWindow<FirePhaseEditorWindow>("Fire Phase");
        w.minSize = new Vector2(340, 500);
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        BindPhase();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
        {
            scrollPos = scrollView.scrollPosition;
            DrawWindowContent();
        }
    }

    void DrawWindowContent()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Fire Phase Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUI.BeginChangeCheck();
        editPhase = (FirePhaseSO)EditorGUILayout.ObjectField(
            "Edit Phase", editPhase, typeof(FirePhaseSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            BindPhase();
            Repaint();
            SceneView.RepaintAll();
            GUIUtility.ExitGUI();
        }

        if (editPhase == null)
        {
            EditorGUILayout.HelpBox(
                "Drag a FirePhaseSO asset here, or Create -> STG -> Fire Phase.",
                MessageType.Info);
            return;
        }

        if (phaseSO == null)
        {
            BindPhase();
            return;
        }

        phaseSO.Update();

        // Time
        DrawHeader("Duration");
        EditorGUILayout.PropertyField(durationProp, new GUIContent("Duration (0=infinite)"));
        if (durationProp.floatValue < 0f) durationProp.floatValue = 0f;
        EditorGUILayout.Space(4);

        // Shape
        DrawHeader("Shape");
        EditorGUILayout.PropertyField(shapeProp, new GUIContent("Shape"));
        FireShape shape = (FireShape)shapeProp.enumValueIndex;

        string countLabel = shape == FireShape.Fan ? "Bullet Count (odd recommended)" : "Bullet Count";
        EditorGUILayout.PropertyField(bulletCountProp, new GUIContent(countLabel));

        if (shape == FireShape.Fan)
        {
            EditorGUILayout.PropertyField(angleSpreadProp, new GUIContent("Angle Spread"));
        }
        EditorGUILayout.PropertyField(offsetAngleProp, new GUIContent("Offset Angle"));
        EditorGUILayout.Space(4);

        // Aim
        DrawHeader("Aim Mode");
        EditorGUILayout.PropertyField(aimTypeProp, new GUIContent("Aim Type"));
        EditorGUILayout.Space(4);

        // Bullet
        DrawHeader("Bullet");
        EditorGUILayout.PropertyField(bulletPrefabProp, new GUIContent("Prefab"));
        EditorGUILayout.PropertyField(bulletDefinitionProp, new GUIContent("Bullet Definition"));
        DrawBulletValidation();
        EditorGUILayout.Space(4);

        // Rhythm
        DrawHeader("Rhythm");
        EditorGUILayout.PropertyField(fireIntervalProp, new GUIContent("Fire Interval (s)"));
        EditorGUILayout.PropertyField(rotationSpeedProp, new GUIContent("Rotation Speed (deg/s)"));
        EditorGUILayout.PropertyField(randomizeAngleProp, new GUIContent("Randomize Angle"));
        if (randomizeAngleProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(randomRangeProp, new GUIContent("Range (+/- deg)"));
            EditorGUI.indentLevel--;
        }

        // Scene Preview
        EditorGUILayout.Space(8);
        DrawHeader("Scene Preview");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Use Selected", GUILayout.Height(22)))
            previewOrigin = Selection.activeTransform;
        if (GUILayout.Button("Clear Origin", GUILayout.Height(22)))
            previewOrigin = null;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Origin", previewOrigin != null ? previewOrigin.name : "(world origin)");

        // Save
        EditorGUILayout.Space(8);
        if (GUILayout.Button("Save Asset", GUILayout.Height(28)))
        {
            phaseSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(editPhase);
            AssetDatabase.SaveAssets();
        }

        phaseSO.ApplyModifiedProperties();
        if (GUI.changed)
            EditorUtility.SetDirty(editPhase);
    }

    void DrawHeader(string title)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 18f);
        EditorGUI.LabelField(r, title, EditorStyles.boldLabel);
    }

    void OnSceneGUI(SceneView sv)
    {
        if (editPhase == null || phaseSO == null) return;
        if (editPhase.bulletPrefab == null) return;

        phaseSO.Update();

        Vector3 origin = previewOrigin != null ? previewOrigin.position : Vector3.zero;
        FireShape shape = (FireShape)shapeProp.enumValueIndex;
        int bulletCount = bulletCountProp.intValue;
        float angleSpread = angleSpreadProp.floatValue;
        float offsetAngle = offsetAngleProp.floatValue;
        float previewRadius = 2f;
        Vector2 baseDir = Quaternion.Euler(0, 0, offsetAngle) * Vector2.right;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        Handles.color = Color.green;
        Vector3 baseEnd = origin + (Vector3)baseDir * previewRadius;
        Handles.DrawLine(origin, baseEnd, 2f);

        if (shape == FireShape.Fan)
        {
            Handles.color = new Color(1f, 0f, 0f, 0.2f);
            float half = angleSpread / 2f;
            int steps = 24;
            Vector3 prev = origin;
            for (int i = 0; i <= steps; i++)
            {
                float a = -half + angleSpread * i / steps;
                Vector3 next = origin + (Vector3)(Quaternion.Euler(0, 0, a) * baseDir * previewRadius);
                Handles.DrawLine(origin, next, 1f);
                if (i > 0) Handles.DrawLine(prev, next, 1f);
                prev = next;
            }
        }

        if (shape == FireShape.Circle)
        {
            Handles.color = new Color(0f, 0.6f, 1f, 0.15f);
            Handles.DrawWireDisc(origin, Vector3.forward, previewRadius);
        }

        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 dir = CalcDir(shape, baseDir, i, bulletCount, angleSpread);
            Vector3 endPt = origin + (Vector3)dir * previewRadius;

            Handles.color = Color.yellow;
            Handles.DrawLine(origin, endPt, 1.5f);


            Handles.SphereHandleCap(0, endPt, Quaternion.identity,
                HandleUtility.GetHandleSize(endPt) * 0.04f, EventType.Repaint);
        }

        BulletMovementPreviewUtility.DrawSinePath(
            editPhase.bulletDefinition,
            origin,
            baseDir);
    }

    Vector2 CalcDir(FireShape shape, Vector2 baseDir, int idx, int total, float spread)
    {
        switch (shape)
        {
            case FireShape.Fan:
                float half = (total - 1) / 2f;
                float fa = (idx - half) * (spread / Mathf.Max(1, total - 1));
            return Quaternion.Euler(0, 0, fa) * baseDir;
            case FireShape.Circle:
            return Quaternion.Euler(0, 0, idx * 360f / total) * baseDir;
            case FireShape.Line:
            return baseDir;
            default:
        return baseDir;
        }
    }


    void BindPhase()
    {
        if (editPhase != null)
        {
            phaseSO = new SerializedObject(editPhase);
            durationProp = phaseSO.FindProperty("duration");
            shapeProp = phaseSO.FindProperty("shape");
            bulletCountProp = phaseSO.FindProperty("bulletCount");
            angleSpreadProp = phaseSO.FindProperty("angleSpread");
            offsetAngleProp = phaseSO.FindProperty("offsetAngle");
            aimTypeProp = phaseSO.FindProperty("aimType");
            bulletPrefabProp = phaseSO.FindProperty("bulletPrefab");
            bulletDefinitionProp = phaseSO.FindProperty("bulletDefinition");
            fireIntervalProp = phaseSO.FindProperty("fireInterval");
            rotationSpeedProp = phaseSO.FindProperty("rotationSpeed");
            randomizeAngleProp = phaseSO.FindProperty("randomizeAngle");
            randomRangeProp = phaseSO.FindProperty("randomRange");
        }
        else
        {
            phaseSO = null;
        }
    }

    void DrawBulletValidation()
    {
        GameObject prefab = bulletPrefabProp.objectReferenceValue as GameObject;
        BulletDefinitionSO definition =
            bulletDefinitionProp.objectReferenceValue as BulletDefinitionSO;

        if (prefab == null)
        {
            EditorGUILayout.HelpBox("Bullet prefab is missing.", MessageType.Warning);
        }
        else if (prefab.GetComponent<BulletMovementBase>() == null)
        {
            EditorGUILayout.HelpBox(
                "The prefab has no BulletMovementBase component.",
                MessageType.Error);
        }

        if (definition == null)
        {
            EditorGUILayout.HelpBox("Bullet definition is missing.", MessageType.Warning);
        }
        else if (definition.movementConfig == null)
        {
            EditorGUILayout.HelpBox(
                "The bullet definition has no MovementConfigSO.",
                MessageType.Error);
        }
        else
        {
            EditorGUILayout.LabelField("Movement Type", definition.MovementType.ToString());
        }
    }
}
