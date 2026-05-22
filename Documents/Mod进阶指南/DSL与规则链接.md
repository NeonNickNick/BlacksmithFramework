# DSL与规则链接
[返回](./引言.md)

专门解释 `LinkJudgeRuleDynamic` 方法——技能如何把动态规则挂到判定系统中。机制原理见[判定实现](./判定实现.md)。

## 方法做了什么

```csharp
// SourceFile 内部
_mutationsOnCompile.Add(mutations);
```

`Compile(judger)` 时才真正注册：

```csharp
judger.JudgeRuleManager.AddJudgeRule(_owner, mutations);
```

即：书写阶段记下来 → 编译阶段按施法者专门化 → 加入本回合判定链。

## 典型调用

```csharp
Pen pen = sf => sf
    .UseResource(1, ResourceType.Instance.Iron())
    .LinkJudgeRuleDynamic(
        new List<Mutation>
        {
            new Mutation(
                judgeRule: (player, enemy) =>
                {
                    // 阶段插入的逻辑
                },
                stage: JudgeStage.Instance.OnAttackCanceling(),
                ruleType: RuleType.Modifier,
                modifierOrder: ModifierOrder.Before,
                remainingRounds: 1,
                delayRounds: 0)
        });

return DSL.Create(sc.Self, pen);
```

## Mutation 字段

| 字段 | 说明 |
|---|---|
| `judgeRule` | `Action<Community, Community>`，实际执行逻辑 |
| `stage` | 挂到哪个 `JudgeStage` |
| `ruleType` | `Override` 覆盖或 `Modifier` 修饰 |
| `modifierOrder` | `Before` 或 `After`（核心规则的前/后） |
| `remainingRounds` | 规则持续几回合 |
| `delayRounds` | 延迟几回合开始生效 |

## 规则内可做的事

操作 Resolution 列表、组件，甚至临时编译 DSL：

```csharp
new Mutation(
    (player, enemy) =>
    {
        if (enemy.Focus.Get<TurnContext>().Get<AttackResolution>()
            .Find(a => a.Clock.IsRinging) == null) return;

        DSL.Create(player, sf => sf
            .WriteAttack(10, AttackType.Instance.Magical()))
            .Compile().Execute(player);
    },
    JudgeStage.Instance.OnAttackCanceling(),
    RuleType.Modifier, ModifierOrder.Before)
```

## 常见模式：下回合检查 + 本回合触发

```csharp
new List<Mutation>
{
    new Mutation(/* 触发规则：本阶段立刻生效 */,
        JudgeStage.Instance.OnAttackCanceling(),
        RuleType.Modifier, ModifierOrder.Before),

    new Mutation(/* 清理/重置：下回合开始执行 */,
        JudgeStage.Instance.OnBegin(),
        RuleType.Modifier, ModifierOrder.Before,
        delayRounds: 1)
}
```

## 为什么用 Mutation 而非普通 DSL

有的效果不是"当前技能一放就结算完"——它依赖对手行为、挂在特定阶段、持续到下回合、要插在默认规则前/后。这类逻辑放到 `Mutation` 里比塞进普通 DSL 更自然可控。

## 编写建议

1. `judgeRule` 只写阶段相关逻辑，不要重写整套技能
2. 在规则内制造即时攻击时，调用简短 DSL 即可
3. 规则依赖技能类字段时，控制好重置时机
4. 先写最小可运行版本，再补持续/延迟回合

参考：`Project/Blacksmith/BlacksmithCore/Specific/BuiltInProfessions/Lancer.cs`。完整拆解见[高级技能模式 - 动态规则](../高级技能模式.md#6-动态规则-linkjudgeruledynamic)。
