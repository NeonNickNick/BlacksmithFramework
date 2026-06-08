# DSL与动态注册
[返回](./引言.md)

专门解释 `RegistCallbackOnJudge` 方法——技能如何向判定管线动态注册回调。它向特定 `JudgeStage` 注册 `ICallbackOnJudge`（含 `Action<Community, Community>` 回调 + `ClapRoundClock` 生命周期）的通用入口。机制原理见[判定实现](./判定实现.md)。

## 方法做了什么

```csharp
// SourceFile 内部
_mutationsOnCompile.Add(callbacks);
```

`Compile(judger)` 时才真正注册：

```csharp
judger.JudgeRuleManager.AddJudgeRule(_owner, callbacks);
```

即：书写阶段记下来 → 编译阶段按施法者专门化 → 加入本回合判定链。

## 核心类型

| 类型 | 说明 |
|---|---|
| `ICallbackOnJudge` | 接口，定义 `JudgeRule`（`Action<Community, Community>`）、`Stage`（`JudgeStage`）、`Clock`（`ClapRoundClock`） |
| `ModifierCallback` | 实现 `ICallbackOnJudge`，额外携带 `ModifierOrder`（`Before` / `After`），作为阶段核心规则的修饰器 |
| `OverrideCallback` | 实现 `ICallbackOnJudge`，直接替换阶段的默认核心规则 |

## 典型调用

### ModifierCallback（修饰器）

```csharp
Pen pen = sf => sf
    .UseResource(1, ResourceType.Instance.Iron())
    .RegistCallbackOnJudge(
        new()
        {
            new ModifierCallback(
                judgeRule: (player, enemy) =>
                {
                    // 阶段插入的逻辑
                },
                stage: JudgeStage.Instance.OnAttackCanceling(),
                modifierOrder: ModifierOrder.Before,
                clock: new ClapRoundClock(delayRounds: 0, remainingRounds: 1))
        });

return DSL.Create(sc.Self, pen);
```

### OverrideCallback（覆盖默认规则）

```csharp
.RegistCallbackOnJudge(new()
{
    new OverrideCallback(
        (player, enemy) =>
        {
            // 完全替代该阶段的默认行为
        },
        JudgeStage.Instance.OnEffectSwaping(),
        new ClapRoundClock(remainingRounds: 1))
})
```

### 混合使用

同一 `RegistCallbackOnJudge` 调用中可混合 `ModifierCallback` 和 `OverrideCallback`：

```csharp
.RegistCallbackOnJudge(new()
{
    new ModifierCallback(
        AttackCanceling_Modifier_Before,
        JudgeStage.Instance.OnAttackCanceling(),
        ModifierOrder.Before,
        new ClapRoundClock(remainingRounds: 1)),
    new ModifierCallback(
        (player, enemy) =>
        {
            if (_chargeCount.Value == chargeCountThis && !_wasPassive.Value)
            {
                _chargeCount.Reset();
                _chargeCost.Reset();
            }
            _wasPassive.Reset();
        },
        JudgeStage.Instance.OnBegin(),
        ModifierOrder.Before,
        new ClapRoundClock(delayRounds: 1, remainingRounds: 1))
});
```

## ICallbackOnJudge 字段

| 字段 | 说明 |
|---|---|
| `JudgeRule` | `Action<Community, Community>`，实际执行逻辑（可在注册时被 `AddJudgeRule` 按 source 专门化改写） |
| `Stage` | 挂到哪个 `JudgeStage` |
| `Clock` | `ClapRoundClock`，统一管理 `delayRounds`、`remainingRounds`、`isInfinite`、`forceKill` |

`ModifierCallback` 额外字段：

| 字段 | 说明 |
|---|---|
| `ModifierOrder` | `Before` 或 `After`（核心规则的前/后） |

## ICallbackOnJudge 内可做的事

操作 Resolution 列表、组件，甚至临时编译 DSL：

```csharp
new ModifierCallback(
    (player, enemy) =>
    {
        if (enemy.Focus.Get<TurnContext>().Get<AttackResolution>()
            .Find(a => a.Clock.IsRinging) == null) return;

        DSL.Create(player, sf => sf
            .WriteAttack(10, AttackType.Instance.Magical()))
            .Compile().Execute(player);
    },
    JudgeStage.Instance.OnAttackCanceling(),
    ModifierOrder.Before,
    new ClapRoundClock(remainingRounds: 1))
```

## 常见模式：下回合检查 + 本回合触发

```csharp
new()
{
    new ModifierCallback(/* 触发规则：本阶段立刻生效 */,
        JudgeStage.Instance.OnAttackCanceling(),
        ModifierOrder.Before,
        new ClapRoundClock(remainingRounds: 1)),

    new ModifierCallback(/* 清理/重置：下回合开始执行 */,
        JudgeStage.Instance.OnBegin(),
        ModifierOrder.Before,
        new ClapRoundClock(delayRounds: 1, remainingRounds: 1))
}
```

## 为什么用动态注册

有的效果不是"当前技能一放就结算完"——它依赖对手行为、挂在特定阶段、持续到下回合、要插在默认规则前/后。这类逻辑放到 `ICallbackOnJudge` 里比塞进普通 DSL 更自然可控。

## 编写建议

1. `JudgeRule` 只写阶段相关逻辑，不要重写整套技能
2. 在规则内制造即时攻击时，调用简短 DSL 即可
3. 规则依赖技能类字段时，控制好重置时机
4. 先写最小可运行版本，再通过 `ClapRoundClock` 补持续/延迟回合和 `forceKill` 条件
5. 修饰器用 `ModifierCallback` + `ModifierOrder`，替换默认规则用 `OverrideCallback`

参考：`Project/Blacksmith/BlacksmithCore/Specific/BuiltInProfessions/Lancer.cs`。完整拆解见[高级技能模式 - 动态注册](../高级技能模式.md#6-动态注册-registcallbackonjudge)。
