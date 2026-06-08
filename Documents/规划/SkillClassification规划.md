# SkillClassification 规划

技能分类属性体系，目标是为职业技能提供运行时每回合元数据，让引擎、AI 和动态规则能够获知"双方即将声明什么类型的技能"。

## 当前已实现

### 命名空间

`BlacksmithCore.Infra.Attributes.SkillClassification`

### 已有属性

| 属性 | 说明 | 状态 |
|---|---|---|
| `[IsAttack]` | 标记为攻击技能 | 已定义 |
| `[IsDefense]` | 标记为防御技能 | 已定义 |
| `[IsResource]` | 标记为资源技能 | 已定义 |
| `[IsRecovery]` | 标记为恢复技能 | 已定义 |
| `[IsProfessionSkill]` | 标记为转职入口技能 | 已定义，`ProfessionRegistry` 扫描注册 |
| `[IsEquipmentSkill]` | 标记为装备技能 | 已定义，`ProfessionRegistry` 扫描注册 |

所有属性定义为标记特性（无参数），使用 `[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]`。

### 集成情况

- `ProfessionRegistry.RegistProfessionEquipmentSkillName()` 已从 `Attributes.SkillClassification` 命名空间导入并扫描 `[IsProfessionSkill]` / `[IsEquipmentSkill]`
- 四个类型属性（`IsAttack`、`IsDefense`、`IsResource`、`IsRecovery`）已定义，等待框架集成

### 辅助命名空间

`BlacksmithCore.Infra.Attributes.SkillMarkOnly` 包含纯标记属性：
- `[IsExperimental]` — 标记实验性技能
- `[IsHighCost]` — 标记高消耗技能

## 规划方向

### 核心目标

在每回合技能声明阶段，框架收集双方已声明技能的 SkillClassification 属性，构建当回合的**技能类型元数据视图**。这个视图可供以下场景使用：

### 1. 动态规则基于对手技能类型触发

```csharp
// 示例：对手使用攻击技能时，自动获得真实伤减
new ModifierCallback((player, enemy) =>
{
    if (enemy.Focus.Get<Skill>().CurrentSkillClassifications
        .Contains(typeof(IsAttack)))
    {
        player.Focus.Get<Defense>().Add(new RealReduction { Power = 1 });
    }
},
JudgeStage.Instance.OnBegin(),
ModifierOrder.Before,
new ClapRoundClock(remainingRounds: 1))
```

### 2. AI 决策增强

`GeneralStrategy`（MCTS）和 `BloodSigilStrategy`（启发式）可读取双方技能类型：
- 对手即将使用恢复技能 → 优先使用打断/延迟效果
- 自己即将使用防御技能 → AI 评估减伤收益
- 对手使用资源技能 → 考虑是否竞速打断

### 3. 技能面板增强

前端技能面板可根据分类属性显示类型图标/标签（攻击⚔、防御🛡、资源⛏、恢复💚），帮助玩家快速识别技能类型。

### 4. 可能的扩展属性

根据需求可扩展：
- `[IsBuff]` — 增益技能
- `[IsDebuff]` — 减益技能
- `[IsControl]` — 控制技能
- `[IsSummon]` — 召唤技能
- `[IsMovement]` — 位移技能（延迟/加速）

## 实施步骤

1. ~~属性定义~~（已完成）
2. ~~`ProfessionRegistry` 集成~~（已完成）
3. **进行中**：在 `Skill` 组件或 `GameInstance.Declare` 阶段收集已声明技能的分类信息
4. 提供查询接口（如 `CurrentSkillClassifications` 属性）
5. 集成到 AI 策略
6. 集成到前端技能面板

## 参考

- 属性源码：`Project/Blacksmith/BlacksmithCore/Infra/Attributes/SkillClassification/`
- ProfessionRegistry：`Project/Blacksmith/BlacksmithCore/Infra/Profession/ProfessionRegistry.cs`
- 项目架构：`Documents/项目架构.md`
