using UnityEditor;
using UnityEngine;

/// <summary>
/// FirePhaseSO 的自定义 Inspector — 提供滑条和参数分组，支持选中后在 Scene View 中实时预览。
/// 使用方式：选中任意 FirePhaseSO 资产即可在 Inspector 中看到增强面板。
/// </summary>
[CustomEditor(typeof(FirePhaseSO))]
public class FirePhaseSOEditor : Editor
{
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

    private void OnEnable()
    {
        durationProp       = serializedObject.FindProperty("duration");
        shapeProp          = serializedObject.FindProperty("shape");
        bulletCountProp    = serializedObject.FindProperty("bulletCount");
        angleSpreadProp    = serializedObject.FindProperty("angleSpread");
        offsetAngleProp    = serializedObject.FindProperty("offsetAngle");
        aimTypeProp        = serializedObject.FindProperty("aimType");
        bulletPrefabProp   = serializedObject.FindProperty("bulletPrefab");
        bulletDefinitionProp = serializedObject.FindProperty("bulletDefinition");
        fireIntervalProp   = serializedObject.FindProperty("fireInterval");
        rotationSpeedProp  = serializedObject.FindProperty("rotationSpeed");
        randomizeAngleProp = serializedObject.FindProperty("randomizeAngle");
        randomRangeProp    = serializedObject.FindProperty("randomRange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── 标题 ──
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("🔥 弹幕阶段编辑器", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // ═══ 时间 ═══
        DrawHeader("时间");
        EditorGUILayout.PropertyField(durationProp, new GUIContent("持续时间 (0=无限)"));
        if (durationProp.floatValue < 0f) durationProp.floatValue = 0f;
        EditorGUILayout.Space(4);

        // ═══ 发射形状 ═══
        DrawHeader("发射形状");
        EditorGUILayout.PropertyField(shapeProp, new GUIContent("形状"));

        // Fan 时自动调整 bulletCount
        FireShape shape = (FireShape)shapeProp.enumValueIndex;
        int count = bulletCountProp.intValue;
        GUIContent countLabel = new GUIContent(
            shape == FireShape.Fan ? "子弹条数 (Fan 建议奇数)" : "子弹条数");
        EditorGUILayout.PropertyField(bulletCountProp, countLabel);
        if (count < 1) bulletCountProp.intValue = 1;
        if (count > 720) bulletCountProp.intValue = 720;

        // 夹角（仅 Fan 有歧义）
        if (shape == FireShape.Fan)
        {
            EditorGUILayout.PropertyField(
                angleSpreadProp,
                new GUIContent("扇形总夹角 (angleSpread)"));
        }

        // 基准偏移
        EditorGUILayout.PropertyField(offsetAngleProp, new GUIContent("基准方向偏移角"));
        EditorGUILayout.Space(4);

        // ═══ 瞄准模式 ═══
        DrawHeader("瞄准模式");
        EditorGUILayout.PropertyField(aimTypeProp, new GUIContent("瞄准类型"));

        EditorGUILayout.Space(4);

        // ═══ 子弹 ═══
        DrawHeader("子弹");
        EditorGUILayout.PropertyField(bulletPrefabProp, new GUIContent("子弹预制体"));
        EditorGUILayout.PropertyField(bulletDefinitionProp, new GUIContent("子弹定义"));

        if (bulletPrefabProp.objectReferenceValue == null)
            EditorGUILayout.HelpBox("⚠ 未指定子弹预制体，运行时不会生成子弹。", MessageType.Warning);
        if (bulletDefinitionProp.objectReferenceValue == null)
            EditorGUILayout.HelpBox("⚠ 未指定子弹定义，运行时不会生成子弹。", MessageType.Warning);
        else
        {
            BulletDefinitionSO definition =
                bulletDefinitionProp.objectReferenceValue as BulletDefinitionSO;
            if (definition != null && definition.movementConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "子弹定义没有 MovementConfigSO。",
                    MessageType.Error);
            }
            else if (definition != null)
            {
                EditorGUILayout.LabelField("运动类型", definition.MovementType.ToString());
            }
        }

        GameObject bulletPrefab = bulletPrefabProp.objectReferenceValue as GameObject;
        if (bulletPrefab != null && bulletPrefab.GetComponent<BulletMovementBase>() == null)
        {
            EditorGUILayout.HelpBox(
                "子弹预制体没有 BulletMovementBase 派生组件。",
                MessageType.Error);
        }
        EditorGUILayout.Space(4);

        // ═══ 发射节奏 ═══
        DrawHeader("发射节奏");
        EditorGUILayout.PropertyField(fireIntervalProp, new GUIContent("发射间隔 (秒)"));
        EditorGUILayout.PropertyField(rotationSpeedProp, new GUIContent("整体旋转速度 (度/秒)"));
        EditorGUILayout.PropertyField(randomizeAngleProp, new GUIContent("随机化角度"));
        if (randomizeAngleProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(randomRangeProp, new GUIContent("随机范围 (±度)"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(6);

        // ── 快捷预览按钮 ──
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("选中此资产查看 Gizmos", GUILayout.Height(24)))
        {
            Selection.activeObject = target;
        }
        EditorGUILayout.EndHorizontal();
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("播放模式下参数修改不会影响已发射的子弹。", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private void OnSceneGUI()
    {
        // 当 FirePhaseSO 为选中对象时，在 Scene View 原点绘制预览
        if (Selection.activeObject != target) return;
        if (Event.current.type != EventType.Repaint) return;

        FirePhaseSO phase = (FirePhaseSO)target;
        if (phase.bulletPrefab == null) return;

        Vector3 origin = Vector3.zero;
        Vector2 baseDir = Quaternion.Euler(0f, 0f, phase.offsetAngle) * Vector2.right;
        float previewRadius = 3f;

        Color oldColor = Handles.color;

        // 基准方向
        Handles.color = Color.green;
        Vector3 baseEnd = origin + (Vector3)baseDir * previewRadius;
        Handles.DrawLine(origin, baseEnd, 2f);

        // 扇形范围
        if (phase.shape == FireShape.Fan)
        {
            Handles.color = new Color(1f, 0f, 0f, 0.25f);
            DrawHandleFan(origin, baseDir, phase.angleSpread, previewRadius);
        }

        // 子弹方向
        Handles.color = Color.yellow;
        for (int i = 0; i < phase.bulletCount; i++)
        {
            Vector2 dir = CalcPreviewDirection(phase, baseDir, i);
            Vector3 endPt = origin + (Vector3)dir * previewRadius;
            Handles.DrawLine(origin, endPt, 1.5f);
        }

        BulletMovementPreviewUtility.DrawSinePath(
            phase.bulletDefinition,
            origin,
            baseDir);

        Handles.color = oldColor;
    }

    // ── helpers ──

    private void DrawHeader(string title)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 20f);
        EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);
        Rect lineRect = new Rect(rect.x, rect.y + 18, rect.width, 1);
        EditorGUI.DrawRect(lineRect, new Color(0.4f, 0.4f, 0.4f, 0.5f));
    }

    private Vector2 CalcPreviewDirection(FirePhaseSO phase, Vector2 baseDir, int idx)
    {
        float total = (float)phase.bulletCount;
        float angle = 0f;
        switch (phase.shape)
        {
            case FireShape.Fan:
                float half = (total - 1) / 2f;
                angle = (idx - half) * (phase.angleSpread / Mathf.Max(1, total - 1));
                break;
            case FireShape.Circle:
                angle = idx * 360f / total;
                break;
            case FireShape.Line:
                angle = 0f;
                break;
        }
        Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;


        return dir;
    }

    private void DrawHandleFan(Vector3 origin, Vector2 baseDir, float spread, float radius)
    {
        float half = spread / 2f;
        int steps = 20;
        Vector3 prev = origin;
        for (int i = 0; i <= steps; i++)
        {
            float a = -half + (spread * i / steps);
            Vector3 next = origin + (Vector3)(Quaternion.Euler(0, 0, a) * baseDir * radius);
            Handles.DrawLine(origin, next, 1f);
            if (i > 0) Handles.DrawLine(prev, next, 1f);
            prev = next;
        }
    }
}
