# SkillMetadata 技能元数据体系

技能元数据属性体系，目标是为职业技能提供运行时可查询的类型信息，让引擎、AI 和动态规则能够获知"双方声明了什么类型的技能"。

**已从此前的 SkillClassification 规划重命名为 SkillMetadata**，核心变化：
- 所有分类属性统一实现 `ISkillMetadata` 接口，可被统一扫描和查询
- 不再散落多个命名空间，集中在 `Attributes.SkillMetadata`
- 通过 `GameMetadata.SkillMetadataDict` 提供完整只读查询入口
- 每回合的 `GameMetadata.UpdateCurrentSkill()` 更新当前双方技能元数据快照

## 当前已实现

### 命名空间

`BlacksmithCore.Infra.Attributes.SkillMetadata`（属性定义）
`BlacksmithCore.Infra.Attributes.SkillMetadata.Core`（`ISkillMetadata` 接口）

### 接口

```csharp
// Attributes/SkillMetadata/Core/ISkillMetadata.cs
public interface ISkillMetadata
{
}
```

所有技能分类属性（`IsAttack`、`IsDefense`、`IsResource`、`IsRecovery`、`IsProfessionSkill`、`IsEquipmentSkill`）均实现此接口，使得 `ProfessionRegistry` 可以通过 `info.GetCustomAttribute(type)` 统一收集。

### 已有属性

| 属性 | 说明 | 状态 |
|---|---|---|
| `[IsAttack]` | 标记为攻击技能 | 已实现 `ISkillMetadata` |
| `[IsDefense]` | 标记为防御技能 | 已实现 `ISkillMetadata` |
| `[IsResource]` | 标记为资源技能 | 已实现 `ISkillMetadata` |
| `[IsRecovery]` | 标记为恢复技能 | 已实现 `ISkillMetadata` |
| `[IsProfessionSkill]` | 标记为转职入口技能 | 已实现 `ISkillMetadata`，`ProfessionRegistry` 扫描 |
| `[IsEquipmentSkill]` | 标记为装备技能 | 已实现 `ISkillMetadata`，`ProfessionRegistry` 扫描 |

所有属性定义为标记特性（无参数），使用 `[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]`。

### 辅助命名空间

`BlacksmithCore.Infra.Attributes.SkillMarkOnly` 包含纯标记属性（不实现 `ISkillMetadata`）：
- `[IsExperimental]` — 标记实验性技能
- `[IsHighCost]` — 标记高消耗技能

### 收集与存储

启动时 `ModLoader` 扫描所有 `ISkillMetadata` 实现类型，然后对每个 `SkillPackageBase` 调用 `ProfessionRegistry.CollectSkillMetadata(package, skillMetadatas)`：

```csharp
public static void CollectSkillMetadata(SkillPackageBase package, List<ISkillMetadata> skillClassifications)
{
    // 遍历 package 的所有方法
    // 对每个有效技能方法，检查每个 ISkillMetadata 类型对应的 Attribute
    // 存在则加入 SkillMetadataDict[skillName]
}
```

`SkillMetadataDict` 是 `Dictionary<string, HashSet<ISkillMetadata>>`——按技能名（小写）索引，值是该技能标注的所有元数据属性集合。

### 查询入口：GameMetadata

`GameMetadata`（实现 `IGameMetadata`）是只读元数据查询的统一入口：

- **`MainProfessionSkillNames`**：延迟初始化的 `IReadOnlySet<string>`，从 `SkillMetadataDict` 中找出所有标注 `[IsProfessionSkill]` 的技能
- **`EquipmentSkillNames`**：延迟初始化的 `IReadOnlySet<string>`，从 `SkillMetadataDict` 中找出所有标注 `[IsEquipmentSkill]` 的技能
- **`SkillMetadataDict`**：`IReadOnlyDictionary<string, IReadOnlySet<ISkillMetadata>>`——完整的技能名到元数据集合映射
- **`CurrentPlayerSkillMetadata` / `CurrentEnemySkillMetadata`**：每回合 `Declare` 时通过 `UpdateCurrentSkill(playerSkill, enemySkill)` 更新为双方当前声明技能的元数据快照

`ISkillContext.GameMetadata` 和 `ISudoOperations.GameMetadata` 提供对 `GameMetadata` 的访问。

## 规划方向

### 核心目标（已部分完成）

在每回合技能声明阶段，框架已可获知双方声明技能的分类信息。下一步是让这些信息被实际消费。

### 1. 动态规则基于对手技能类型触发（基础架构就绪）

```csharp
// 示例：对手使用攻击技能时，自动获得真实伤减
new ModifierCallback((player, enemy) =>
{
    if (sc.GameMetadata.CurrentEnemySkillMetadata.Classifications
        .Any(c => c is IsAttack))
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
2. ~~`ISkillMetadata` 接口 + `CollectSkillMetadata`~~（已完成）
3. ~~`GameMetadata.SkillMetadataDict` 查询入口~~（已完成）
4. ~~每回合 `UpdateCurrentSkill()` 更新元数据快照~~（已完成）
5. **进行中**：在动态规则和 AI 策略中消费 `GameMetadata.CurrentPlayerSkillMetadata` / `CurrentEnemySkillMetadata`
6. 集成到前端技能面板

## 参考

- 属性源码：`Project/Blacksmith/BlacksmithCore/Infra/Attributes/SkillMetadata/`
- ISkillMetadata 接口：`Project/Blacksmith/BlacksmithCore/Infra/Attributes/SkillMetadata/Core/ISkillMetadata.cs`
- ProfessionRegistry：`Project/Blacksmith/BlacksmithCore/Infra/Profession/ProfessionRegistry.cs`
- GameMetadata：`Project/Blacksmith/BlacksmithCore/Driver/GameMetadata.cs`
- IGameMetadata：`Project/Blacksmith/BlacksmithCore/Infra/Profession/ISkillContext.cs`
- 项目架构：`Documents/项目架构.md`
