using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑 FireSequenceSO，并通过独立 SerializedObject 编辑其引用的 FirePhaseSO。
/// FireSequenceSO.phases 存放的是资产引用，不是内嵌序列化对象。
/// </summary>
public class FirePatternEditorWindow : EditorWindow
{
    [SerializeField] private FireSequenceSO editSequence;
    [SerializeField] private int selectedPhaseIndex;
    private SerializedObject sequenceSO;
    private SerializedProperty phasesProp;

    private FirePhaseSO editPhase;
    private SerializedObject phaseSO;
    [SerializeField] private Transform previewOrigin;
    [SerializeField] private Vector2 scrollPosition;

    [MenuItem("Tools/Fire Pattern Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<FirePatternEditorWindow>("Fire Pattern Editor");
        window.minSize = new Vector2(380f, 560f);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if (editSequence != null)
        {
            BindSequence();
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ClearPhaseBinding();
    }

    private void OnGUI()
    {
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            scrollPosition = scrollView.scrollPosition;
            DrawWindowContent();
        }
    }

    private void DrawWindowContent()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("弹幕序列编辑器", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        FireSequenceSO selectedSequence = (FireSequenceSO)EditorGUILayout.ObjectField(
            "Fire Sequence", editSequence, typeof(FireSequenceSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            editSequence = selectedSequence;
            BindSequence();
            RestartGuiLayout();
        }

        if (editSequence == null)
        {
            EditorGUILayout.HelpBox(
                "请选择 FireSequenceSO。Sequence 只负责排列 FirePhaseSO 资产。",
                MessageType.Info);
            return;
        }

        if (sequenceSO == null)
        {
            BindSequence();
            RestartGuiLayout();
        }

        if (phasesProp == null)
        {
            EditorGUILayout.HelpBox(
                "FireSequenceSO 中找不到 phases 字段。",
                MessageType.Error);
            return;
        }

        sequenceSO.Update();
        DrawSequenceSettings();
        DrawPhaseList();

        if (phaseSO != null && editPhase != null)
        {
            phaseSO.Update();
            DrawPhaseEditor();
            if (phaseSO.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(editPhase);
                SceneView.RepaintAll();
            }
        }

        DrawPreviewSettings();

        if (sequenceSO.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(editSequence);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("保存相关资产", GUILayout.Height(28f)))
        {
            SaveAssets();
        }
    }

    private void DrawSequenceSettings()
    {
        DrawSection("Sequence 设置");
        EditorGUILayout.PropertyField(sequenceSO.FindProperty("loop"), new GUIContent("循环"));
        EditorGUILayout.PropertyField(sequenceSO.FindProperty("loopCount"), new GUIContent("循环次数"));
    }

    private void DrawPhaseList()
    {
        DrawSection("Phase 引用列表");

        int phaseCount = phasesProp.arraySize;
        if (phaseCount == 0)
        {
            EditorGUILayout.HelpBox("Sequence 中还没有 Phase 引用。", MessageType.Warning);
        }
        else
        {
            selectedPhaseIndex = Mathf.Clamp(selectedPhaseIndex, 0, phaseCount - 1);
            int newIndex = EditorGUILayout.IntSlider(
                "当前 Phase", selectedPhaseIndex, 0, phaseCount - 1);
            if (newIndex != selectedPhaseIndex)
            {
                selectedPhaseIndex = newIndex;
                BindSelectedPhase();
                RestartGuiLayout();
            }

            SerializedProperty selectedElement = phasesProp.GetArrayElementAtIndex(selectedPhaseIndex);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(selectedElement, new GUIContent("Phase 资产"));
            if (EditorGUI.EndChangeCheck())
            {
                sequenceSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(editSequence);
                BindSelectedPhase();
                RestartGuiLayout();
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("添加引用槽"))
            {
                int newIndex = phasesProp.arraySize;
                phasesProp.InsertArrayElementAtIndex(newIndex);
                phasesProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = null;
                selectedPhaseIndex = newIndex;
                sequenceSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(editSequence);
                BindSelectedPhase();
                RestartGuiLayout();
            }

            using (new EditorGUI.DisabledScope(phaseCount == 0))
            {
                if (GUILayout.Button("删除当前引用"))
                {
                    SerializedProperty element = phasesProp.GetArrayElementAtIndex(selectedPhaseIndex);
                    element.objectReferenceValue = null;
                    phasesProp.DeleteArrayElementAtIndex(selectedPhaseIndex);
                    selectedPhaseIndex = Mathf.Clamp(
                        selectedPhaseIndex, 0, Mathf.Max(0, phasesProp.arraySize - 1));
                    sequenceSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(editSequence);
                    BindSelectedPhase();
                    RestartGuiLayout();
                }
            }
        }

        if (phaseCount > 1)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(selectedPhaseIndex <= 0))
                {
                    if (GUILayout.Button("上移"))
                    {
                        phasesProp.MoveArrayElement(selectedPhaseIndex, selectedPhaseIndex - 1);
                        selectedPhaseIndex--;
                        sequenceSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(editSequence);
                        BindSelectedPhase();
                        RestartGuiLayout();
                    }
                }

                using (new EditorGUI.DisabledScope(selectedPhaseIndex >= phaseCount - 1))
                {
                    if (GUILayout.Button("下移"))
                    {
                        phasesProp.MoveArrayElement(selectedPhaseIndex, selectedPhaseIndex + 1);
                        selectedPhaseIndex++;
                        sequenceSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(editSequence);
                        BindSelectedPhase();
                        RestartGuiLayout();
                    }
                }
            }
        }

        if (phaseCount > 0 && editPhase == null)
        {
            EditorGUILayout.HelpBox(
                "当前槽位为空。请指定一个 FirePhaseSO 资产后再编辑参数。",
                MessageType.Info);
        }
    }

    private void DrawPhaseEditor()
    {
        DrawSection("当前 Phase 参数");

        SerializedProperty duration = phaseSO.FindProperty("duration");
        SerializedProperty shape = phaseSO.FindProperty("shape");
        SerializedProperty bulletCount = phaseSO.FindProperty("bulletCount");
        SerializedProperty angleSpread = phaseSO.FindProperty("angleSpread");
        SerializedProperty offsetAngle = phaseSO.FindProperty("offsetAngle");
        SerializedProperty aimType = phaseSO.FindProperty("aimType");
        SerializedProperty bulletPrefab = phaseSO.FindProperty("bulletPrefab");
        SerializedProperty bulletDefinition = phaseSO.FindProperty("bulletDefinition");
        SerializedProperty fireInterval = phaseSO.FindProperty("fireInterval");
        SerializedProperty rotationSpeed = phaseSO.FindProperty("rotationSpeed");
        SerializedProperty randomizeAngle = phaseSO.FindProperty("randomizeAngle");
        SerializedProperty randomRange = phaseSO.FindProperty("randomRange");

        EditorGUILayout.PropertyField(duration, new GUIContent("持续时间 (0=无限)"));

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(shape, new GUIContent("发射形状"));
        if (EditorGUI.EndChangeCheck())
        {
            ApplyPhaseLayoutChange();
        }

        EditorGUILayout.PropertyField(bulletCount, new GUIContent("子弹数量"));

        FireShape fireShape = (FireShape)shape.enumValueIndex;
        if (fireShape == FireShape.Fan)
        {
            EditorGUILayout.PropertyField(angleSpread, new GUIContent("角度范围"));
        }

        EditorGUILayout.PropertyField(offsetAngle, new GUIContent("方向偏移角"));
        EditorGUILayout.PropertyField(aimType, new GUIContent("瞄准类型"));

        EditorGUILayout.Space(4f);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(bulletPrefab, new GUIContent("子弹预制体"));
        EditorGUILayout.PropertyField(bulletDefinition, new GUIContent("子弹定义"));
        if (EditorGUI.EndChangeCheck())
        {
            ApplyPhaseLayoutChange();
        }

        DrawDefinitionValidation(
            bulletPrefab.objectReferenceValue as GameObject,
            bulletDefinition.objectReferenceValue as BulletDefinitionSO);

        EditorGUILayout.Space(4f);
        EditorGUILayout.PropertyField(fireInterval, new GUIContent("发射间隔"));
        EditorGUILayout.PropertyField(rotationSpeed, new GUIContent("旋转速度"));

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(randomizeAngle, new GUIContent("随机角度"));
        if (EditorGUI.EndChangeCheck())
        {
            ApplyPhaseLayoutChange();
        }

        if (randomizeAngle.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(randomRange, new GUIContent("随机范围"));
            EditorGUI.indentLevel--;
        }
    }

    private void ApplyPhaseLayoutChange()
    {
        phaseSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(editPhase);
        RestartGuiLayout();
    }

    private static void DrawDefinitionValidation(GameObject prefab, BulletDefinitionSO definition)
    {
        if (prefab == null)
        {
            EditorGUILayout.HelpBox("未指定子弹预制体。", MessageType.Warning);
        }

        if (definition == null)
        {
            EditorGUILayout.HelpBox("未指定 BulletDefinitionSO。", MessageType.Warning);
            return;
        }

        if (definition.movementConfig == null)
        {
            EditorGUILayout.HelpBox(
                "BulletDefinitionSO 没有 MovementConfigSO。",
                MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("运动类型", definition.MovementType.ToString());

        if (prefab == null) return;

        BulletMovementBase mover = prefab.GetComponent<BulletMovementBase>();
        if (mover == null)
        {
            EditorGUILayout.HelpBox(
                "子弹预制体没有 BulletMovementBase 派生组件。",
                MessageType.Error);
        }
    }

    private void DrawPreviewSettings()
    {
        DrawSection("Scene 预览");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("使用当前选中对象"))
        {
            previewOrigin = Selection.activeTransform;
        }
        if (GUILayout.Button("使用世界原点"))
        {
            previewOrigin = null;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(
            "预览原点", previewOrigin != null ? previewOrigin.name : "世界原点");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (editPhase == null) return;

        Vector3 origin = previewOrigin != null ? previewOrigin.position : Vector3.zero;
        Vector2 baseDirection = Quaternion.Euler(0f, 0f, editPhase.offsetAngle) * Vector2.right;
        const float previewRadius = 2f;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Color oldColor = Handles.color;

        Handles.color = Color.green;
        Handles.DrawLine(origin, origin + (Vector3)baseDirection * previewRadius, 2f);

        if (editPhase.shape == FireShape.Fan)
        {
            Handles.color = new Color(1f, 0f, 0f, 0.25f);
            DrawHandlesFan(origin, baseDirection, editPhase.angleSpread, previewRadius);
        }
        else if (editPhase.shape == FireShape.Circle)
        {
            Handles.color = new Color(0f, 0.6f, 1f, 0.3f);
            Handles.DrawWireDisc(origin, Vector3.forward, previewRadius);
        }

        int count = Mathf.Max(1, editPhase.bulletCount);
        for (int i = 0; i < count; i++)
        {
            Vector2 direction = CalculateDirection(
                editPhase.shape, baseDirection, i, count, editPhase.angleSpread);
            Vector3 end = origin + (Vector3)direction * previewRadius;
            Handles.color = Color.yellow;
            Handles.DrawLine(origin, end, 1.5f);
            Handles.SphereHandleCap(
                0, end, Quaternion.identity,
                HandleUtility.GetHandleSize(end) * 0.04f,
                EventType.Repaint);
        }

        BulletMovementPreviewUtility.DrawSinePath(
            editPhase.bulletDefinition,
            origin,
            baseDirection);

        Handles.color = oldColor;
    }

    private static Vector2 CalculateDirection(
        FireShape shape, Vector2 baseDirection, int index, int total, float spread)
    {
        switch (shape)
        {
            case FireShape.Fan:
                float half = (total - 1) / 2f;
                float fanAngle = (index - half) * (spread / Mathf.Max(1, total - 1));
                return Quaternion.Euler(0f, 0f, fanAngle) * baseDirection;
            case FireShape.Circle:
                return Quaternion.Euler(0f, 0f, index * 360f / total) * baseDirection;
            default:
                return baseDirection;
        }
    }

    private static void DrawHandlesFan(
        Vector3 origin, Vector2 baseDirection, float spread, float radius)
    {
        const int steps = 24;
        float half = spread / 2f;
        Vector3 previous = origin;
        for (int i = 0; i <= steps; i++)
        {
            float angle = -half + spread * i / steps;
            Vector3 next = origin +
                (Vector3)(Quaternion.Euler(0f, 0f, angle) * baseDirection * radius);
            Handles.DrawLine(origin, next, 1f);
            if (i > 0) Handles.DrawLine(previous, next, 1f);
            previous = next;
        }
    }

    private static void DrawSection(string title)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }

    private void BindSequence()
    {
        sequenceSO = editSequence != null ? new SerializedObject(editSequence) : null;
        phasesProp = sequenceSO?.FindProperty("phases");
        selectedPhaseIndex = 0;
        BindSelectedPhase();
    }

    private void BindSelectedPhase()
    {
        ClearPhaseBinding();
        if (phasesProp == null || phasesProp.arraySize == 0) return;

        selectedPhaseIndex = Mathf.Clamp(selectedPhaseIndex, 0, phasesProp.arraySize - 1);
        SerializedProperty element = phasesProp.GetArrayElementAtIndex(selectedPhaseIndex);
        editPhase = element.objectReferenceValue as FirePhaseSO;
        phaseSO = editPhase != null ? new SerializedObject(editPhase) : null;
        SceneView.RepaintAll();
    }

    private void ClearPhaseBinding()
    {
        editPhase = null;
        phaseSO = null;
    }

    private void SaveAssets()
    {
        sequenceSO?.ApplyModifiedProperties();
        phaseSO?.ApplyModifiedProperties();
        if (editSequence != null) EditorUtility.SetDirty(editSequence);
        if (editPhase != null) EditorUtility.SetDirty(editPhase);
        AssetDatabase.SaveAssets();
    }

    private void RestartGuiLayout()
    {
        Repaint();
        SceneView.RepaintAll();
        GUIUtility.ExitGUI();
    }
}
