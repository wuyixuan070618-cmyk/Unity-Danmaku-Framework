# Unity Danmaku Framework

一套基于 Unity 的可扩展弹幕运动与发射框架，用于通过 ScriptableObject 配置弹道、发射阶段和触发式形态切换。

> 一套基于 Unity 的可扩展弹幕框架。

## 当前状态

- 在团结引擎 1.9.2 / Editor 2022.3.62t10 中开发并完成 Demo Play Mode 验证。
- 官方 Unity Editor 兼容性尚未验证。
- 当前版本适合学习、原型制作和个人 STG 项目，不承诺生产环境稳定性。

## 功能

- 基于对象池的子弹与发射器生成，避免频繁 `Instantiate` / `Destroy`。
- 数据与运行时状态分离：`BulletDefinitionSO`、`MovementConfigSO` 与 `BulletSpawnContext`。
- Linear、Sine、Polar、Bezier、Sniper、Laser 六类运动模式。
- `FireSequenceSO` / `FirePhaseSO` 驱动的分阶段发射。
- Circle、Line 等发射形状，以及固定方向、自机狙和相对父对象瞄准。
- 基于 `ITriggerCondition` + `ITriggerAction` 的可扩展触发系统。
- `FormReplacer` 在触发后以新定义替换原子弹，同时继承位置、方向、Owner、Target 和 Parent。
- Scene Gizmo 预览与弹幕配置编辑器。

## 目录结构

Assets/Danmaku-FrameWork/

├─ Runtime/

│  ├─ BulletMovement/

│  ├─ BulletMovementConfig/

│  ├─ FirePattern/

│  ├─ Pooling/

│  ├─ Spawning/

│  └─ Trigger/

├─ Editor/

├─ Demo/

└─ ScriptableObject/


## 快速开始

1. 打开 `Assets/Scenes/SampleScene`。
2. 确认场景中的 `ObjectPool`、`BulletSpawner` 和 Demo 启动对象引用完整。
3. 在 `FirePhaseSO` 中选择子弹 Prefab、`BulletDefinitionSO`、发射形状与间隔。
4. 将一个或多个 Phase 加入 `FireSequenceSO`。
5. 将 Sequence 分配给 `BulletSummoner`，进入 Play Mode。

核心调用链：

BulletSummoner
  -> BulletSpawner
    -> ObjectPool.Get(beforeActivate)
      -> BulletMovementBase.Init(definition, context)


对象会在激活前完成运动数据注入，从而避免使用对象池时出现首帧默认方向或视觉闪现。

## Trigger / FormReplacer

1. 在子弹 Prefab 上挂载 `BulletTrigger`。
2. 为 `conditionSource` 指定一个实现 `ITriggerCondition` 的组件，例如 `TimeCondition`。
3. 挂载实现 `ITriggerAction` 的行为，例如 `FormReplacer`。
4. 为替换行为配置下一形态的 Prefab 与 `BulletDefinitionSO`。

触发器在每次从对象池取出时重置运行时状态，在回池时清理事件订阅。Demo 已验证单次触发、再次复用，以及替换前后上下文继承。

## 已知限制

- `FirePhaseSO.duration` 当前按实际经过时间切换 Phase，而不是保证固定 Volley 数量。启动长帧可能使时间型 Phase 少发一轮。
- `FireSequenceSO` 的循环配置仍需进一步完善。
- 首次显示某些材质或 Shader 时可能出现 Editor 启动卡顿，后续计划增加视觉预热方案。
- 尚未在官方 Unity Editor 和独立 Player Build 中完成兼容性验证。

## 验证状态

- Runtime 与 Editor 脚本命令行编译通过。
- 六类 Movement 已完成 Demo 运行验证。
- Trigger 与 FormReplacer 已完成 Play Mode 生命周期测试。
- 已从 GitHub 全新克隆，并完成从零导入、编译与 Play Mode 运行验证。

## License

本项目使用 [MIT License](LICENSE)。
