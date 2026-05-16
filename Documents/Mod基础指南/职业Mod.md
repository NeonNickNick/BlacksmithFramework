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
    int Param { get; }        // 前端 -p 标志
    string StringParam { get; }  // 前端 -s 标志
}
public interface ISudoOperations
{
    GameInstance DeepCopy(int preRounds = 0);
    bool IsPlayer(Community community);
    IReadOnlySet<string> ProfessionSkillNames { get; }
    IReadOnlySet<string> EquipmentSkillNames { get; }
}
```

常用：`sc.Self`（`Community`）、`sc.Self.Focus`（`Body`）、`sc.Param`、`sc.StringParam`、`sc.SudoOperations`。

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

## [IsProfessionSkill] 与 [IsEquipmentSkill]

标记技能分类，启动时 `ProfessionRegistry` 扫描注册到全局集合（见[项目架构 - ProfessionRegistry](../项目架构.md#professionregistry)），通过 `ISudoOperations` 暴露：

```csharp
[IsProfessionSkill]  // 标记为职业技能——通常是转职入口
private IDSLSourceFile HolyBook(ISkillContext sc) { ... }

[IsEquipmentSkill]   // 标记为装备技能
private IDSLSourceFile StarRifle(ISkillContext sc) { ... }
```

- 新增职业的转职入口 → `[IsProfessionSkill]`
- 装备提供的技能 → `[IsEquipmentSkill]`
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
| `WriteEffect(...)` | 效果 |
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
public class CommonModifier : ProfessionModifier
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

## 被动技能

重写 `MainProfession` 的 `PassiveSkill(ISkillContext sc)` 方法。

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
