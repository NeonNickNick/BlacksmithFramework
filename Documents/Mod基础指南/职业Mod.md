# 职业Mod
[返回](./引言.md)

面向希望添加新职业、扩展已有职业技能或编写职业获得方式的开发者。

## 总体流程

1. 创建 `.NET 8` 类库项目，引用 `BlacksmithCore`
2. 编写 `MainProfession` 或 `ProfessionModifier`
3. `Blacksmith.cmd` 发布 → 编译好的 DLL 放入子目录 → `mod.json` 声明路径 → 启动

## 核心类型

### ISkillContext

```csharp
public interface ISkillContext
{
    ISudoOperations SudoOperations { get; }
    string SkillName { get; }
    Community Self { get; }
    int Param { get; }           // 前端 -p 标志
    string StringParam { get; }  // 前端 -s 标志
    IReadOnlyList<(ISkillContext, ISkillContext)> SkillHistory { get; }
    IGameMetadata GameMetadata { get; }
}
public interface ISudoOperations
{
    GameInstance DeepCopy(int preRounds = 0);
    bool IsPlayer(Community community);
    IGameMetadata GameMetadata { get; }
}
public interface IGameMetadata
{
    IReadOnlySet<string> MainProfessionSkillNames { get; }
    IReadOnlySet<string> EquipmentSkillNames { get; }
}
```

常用：`sc.Self`（`Community`）、`sc.Self.Focus`（`Body`）、`sc.Param`、`sc.StringParam`、`sc.SudoOperations`、`sc.GameMetadata`。

### IGameMetadata 元数据入口

新增的只读元数据查询接口，通过 `ISkillContext.GameMetadata` 暴露：

- **`MainProfessionSkillNames`**：所有标注 `[IsProfessionSkill]` 的技能名集合——用于 Association 安全检查中排除转职入口
- **`EquipmentSkillNames`**：所有标注 `[IsEquipmentSkill]` 的技能名集合——用于排除装备技能

底层通过 `GameMetadata.SkillMetadataDict`（`IReadOnlyDictionary<string, IReadOnlySet<ISkillMetadata>>`）提供技能名到所有元数据属性集合的完整映射，引擎可在回合中获知双方声明技能的完整类型信息。

### Community 与 Body

`Body` 通过 `Get<T>()` 访问各组件（详见[项目架构 - 组件一览](../项目架构.md#组件一览)）：

- `Get<Health>().HP` / `.MHP` / `.GainHP()` / `.LoseHP()`
- `Get<Resource>().Check(type, need)` / `.Use()` / `.Gain()` / `.Query()` / `.QueryAll()`
- `Get<Skill>().AddPackage()` / `.AddSkill()` / `.RemoveSkill()`

## 技能配对规则

系统自动收集 `private` 实例方法配对：

- `XxxCheck(ISkillContext)` → `bool`
- `Xxx(ISkillContext)` → `IDSLSourceFile`

技能名转小写。**必须返回 `IDSLSourceFile`，不能返回具体类 `DSL.SourceFile`**——否则编译报错。

## SkillMetadata 技能元数据

命名空间 `BlacksmithCore.Infra.Attributes.SkillMetadata` 中的所有属性实现 `ISkillMetadata` 接口。`ProfessionRegistry.CollectSkillMetadata()` 在启动时扫描所有技能方法收集元数据，存入 `SkillMetadataDict`，通过 `GameMetadata` → `IGameMetadata` 暴露：

```csharp
using BlacksmithCore.Infra.Attributes.SkillMetadata;

[IsProfessionSkill]  // 标记为职业技能——通常是转职入口
private IDSLSourceFile HolyBook(ISkillContext sc) { ... }

[IsEquipmentSkill]   // 标记为装备技能
private IDSLSourceFile StarRifle(ISkillContext sc) { ... }

[IsAttack]           // 标记为攻击技能（提供回合类型信息）
private IDSLSourceFile Slash(ISkillContext sc) { ... }
```

- 新增职业的转职入口 → `[IsProfessionSkill]`
- 装备提供的技能 → `[IsEquipmentSkill]`
- 攻击/防御/资源/恢复技能 → 对应标注 `[IsAttack]` / `[IsDefense]` / `[IsResource]` / `[IsRecovery]`
- 普通技能无需标注

## DSL 基础用法

```csharp
using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
using DSL = DSLforSkillLogic;

private IDSLSourceFile SomeSkill(ISkillContext sc)
{
    Pen pen = sf => sf
        .UseResource(1, ResourceType.Instance.Iron())
        .WriteAttack(3, AttackType.Instance.Physical());

    return DSL.Create(sc.Self, pen);
}
```

常用语句：

| 语句 | 说明 |
|---|---|
| `WriteAttack(power, type, apFactor, delay)` | 攻击 |
| `WriteDefense(power, defense, delay)` | 防御 |
| `WriteResource(power, type, delay)` | 资源 |
| `WriteRecovery(power)` | 回复 HP |
| `WriteEffect(type, target, power, duration, action, delayRounds)` | 效果，`delayRounds` 可选（默认 0） |
| `WriteFree(action, canMove: true)` | 可转移的自由逻辑 |
| `UseResource(need, type)` | 消耗资源（不可转移） |
| `LoseHP(loss)` / `LoseMHP(loss)` | 扣血（不可转移） |
| `WithBloodSuck(percent)` | 攻击吸血 |
| `WithInterupt()` | 攻击打断（移除铁/金铁/魔力资源决议） |

### WriteFree 与 Move()

`WriteFree(action, canMove)` 的 `canMove` 决定该句子在 `Move()` 所有权转移时是否保留：

- `true` → 随 DSL 转移（可复用增益逻辑）
- `false` → 剥离（消耗类动作）

`UseResource`、`LoseHP`、`LoseMHP` 内部是 `WriteFree(..., false)`。

`Move()` 的完整机制和 Association 模式见[高级技能模式 - 联想](../高级技能模式.md#7-联想-deepcopy--swap--move)。

## 示例一：最小主职业

```csharp
public class MyProfession : MainProfession
{
    private bool JokeCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Get<Health>().HP > 5
            && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
    }

    private IDSLSourceFile Joke(ISkillContext sc)
    {
        Pen pen = sf => sf
            .UseResource(1, ResourceType.Instance.Iron())
            .WriteRecovery(1)
            .WriteAttack(3, AttackType.Instance.Physical())
            .WriteAttack(3, AttackType.Instance.Magical())
                .WithBloodSuck(0.5f);

        return DSL.Create(sc.Self, pen);
    }
}
```

## 示例二：Common 修改器提供转职入口

```csharp
[IsProfessionModifier(nameof(Common))]
public partial class CommonModifier : ProfessionModifier
{
    private bool MyProfessionCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2);
    }

    [IsProfessionSkill]
    private IDSLSourceFile MyProfession(ISkillContext sc)
    {
        sc.Self.Focus.Get<Skill>().AddPackage(new MyProfession());

        Pen pen = sf => sf
            .UseResource(2, ResourceType.Instance.Iron())
            .WriteFree(source => Common.ExcludeAllProfessions(source), canMove: false);

        return DSL.Create(sc.Self, pen);
    }
}
```

> **注意**：Modifier 类必须声明为 `partial class`。`Bind()` 方法由 `ModifierBindingGenerator` 源生成器自动实现，**不需要手写**。

## Modifier 访问目标职业私有状态（UnsafeAccessor）

这是此次重构的核心增强。`ModifierBindingGenerator`（位于 `ClapSourceGenerators/SkillRegistration/BlacksmithOnly/`）是一个两阶段 Roslyn 增量源生成器，使 Modifier 能够**零开销直接读写**目标 MainProfession 的私有字段和属性。

### 开发者视角（你只需要做什么）

1. 在 `MainProfession` 中正常声明私有状态变量（`ClapStateVar<T>`、`ClapRoundClock` 等）
2. 将 Modifier 声明为 `partial class`，标注 `[IsProfessionModifier(nameof(Target))]`
3. **直接使用**目标职业的私有字段名——如同它们声明在 Modifier 中一样

```csharp
[IsProfessionModifier(nameof(Common))]
public partial class CommonModifier : ProfessionModifier
{
    // _pending 是 Driver/Cannon 的 private 字段！
    // 源生成器自动生成了对应的 public ref 字段和 Bind() 实现
    private bool WineGlassCheck(ISkillContext sc)
    {
        return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
    }

    [IsProfessionSkill]
    private IDSLSourceFile WineGlass(ISkillContext sc)
    {
        sc.Self.Focus.Get<Skill>().AddPackage(new(new WineGlass()));
        // ...
        return DSL.Create(sc.Self, pen);
    }
}
```

### 源生成器内部机制（自动完成，无需关注）

**Phase 1**：扫描所有 `MainProfession` 子类 → 收集 `private` 引用类型字段/属性 → 自动生成 `[BindingContract]` 特性编译进 DLL（跨项目可见）

**Phase 2**：扫描所有 `ProfessionModifier` 子类 → 定位其 `[IsProfessionModifier]` 指向的目标 MainProfession → 双渠道收集私有成员（源码直接读取 + DLL 特性读取，去重合并）→ 在 Modifier 的 partial 类中生成：
- `[UnsafeAccessor]` extern 方法（.NET 8 特性，零开销访问私有成员）
- 同名的公开字段（引用目标私有字段）或 `Func`/`Action` 委托（封装目标私有属性）
- `Bind(MainProfession package)` 的完整实现——cast 到具体目标类型，将所有私有成员的引用赋值给生成的公开字段

运行时流程：`ProfessionRegistry.AddModOnInit` → `new Modifier()` → `modifier.Bind(targetPackage)`（源生成器实现） → Modifier 中所有目标私有状态字段就绪。

### 使用场景

- 修改器需要检查目标职业的蓄力层数、Buff 状态
- 修改器需要修改目标职业的计数器、阈值
- 修改器需要读取目标职业的跨回合状态来决定是否启用自身技能
- 等价于：任何 Modifier 逻辑需要使用目标 MainProfession 的 `private` 字段时

> `ModifierBindingGenerator` 源码：`Project/Clap/ClapSourceGenerators/SkillRegistration/BlacksmithOnly/ModifierBindingGenerator.cs`
```

## 被动技能

重写 `MainProfession` 的 `PassiveSkillImpl(ISkillContext sc)` 方法（`virtual`）。`PassiveSkill` 已改为 `sealed override`，内部调用 `PassiveSkillImpl` 后自动设置 `IsPassive = true`——框架据此区分被动/主动技能的编译顺序。返回类型与主动技能相同：`IDSLSourceFile`。

```csharp
public override IDSLSourceFile PassiveSkillImpl(ISkillContext sc)
{
    Pen pen = sf => sf.WriteDefense(1, new RealReduction());
    return DSL.Create(sc.Self, pen);
}
```

## 注意事项

1. `AttackType` 法术攻击是 `Magical()`，不是 `Magic()`
2. 通过 `body.Get<Health>()` 操作生命值，不是直接在 Body 上调用
3. 资源和攻击类型通过 `Instance` 获取
4. 技能名全小写，手动 `AddSkill`/`RemoveSkill` 也用全小写
5. 多个 Mod 同名技能 → 后写覆盖；同名职业 → 抛异常
6. `Common` 是真实职业包，修改它是提供转职入口的最常见方式

## 参考

- `Project/Blacksmith/ModExamples/HolyBookMod/`
- `Project/Blacksmith/ModExamples/PhantomBookMod/` — Association 模式（逐行拆解见[高级技能模式 - 联想](../高级技能模式.md#7-联想-deepcopy--swap--move)）
- [高级技能模式](../高级技能模式.md) — 所有技巧性写法的实战案例
