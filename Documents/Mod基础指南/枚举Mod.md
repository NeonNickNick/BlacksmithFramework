# 枚举Mod
[返回](./引言.md)

介绍如何扩展"可扩展枚举"系统：新建可扩展枚举类型、为现有枚举添加成员、调整优先级。

## 适用范围

项目核心类型使用继承自 `BlacksmithEnum<T>` 的可扩展枚举：`ResourceType`、`AttackType`、`DefenseType`、`EffectType`、`EffectTargetType`、`JudgeStage`。机制原理见[项目架构](../项目架构.md#可扩展枚举-clapenumt-tmemberattribute)。

## 总体流程

1. 创建 `.NET 8` 类库，引用 `BlacksmithCore`
2. 编写枚举类或枚举扩展类
3. `Blacksmith.cmd` 发布，DLL 放入子目录，`mod.json` 声明路径

## 创建新的可扩展枚举

```csharp
public class Names : BlacksmithEnum<Names>
{
    [IsBlacksmithEnumMember(0)]
    public CEValue Alice() => GetCEValue();

    [IsBlacksmithEnumMember(1)]
    public CEValue Bob() => GetCEValue();
}
```

- 返回类型必须是 `CEValue`，方法为 `public` 无参实例方法
- `[IsBlacksmithEnumMember(priority)]` 值越小排越前面
- `==` 比较唯一 ID，类型正确则相同成员相等

## 修改现有枚举

```csharp
[IsBlacksmithEnumModifier]
public static class NamesExtension
{
    [IsBlacksmithEnumMember(-1)]
    public static Names.CEValue Carol(this Names names) => Names.GetCEValue();

    [IsBlacksmithEnumMember(3)]
    public static Names.CEValue Dave(this Names names) => Names.GetCEValue();
}
```

- `public static` 方法，第一个参数是 `this EnumType`
- 返回 `CEValue`，方法名即成员名
- 方法名与已有成员同名 → 覆盖优先级；否则追加

## 资源类型特殊规则

扩展 `ResourceType` 时，金资源命名必须用 `Gold_普通资源名`（如 `Cross` + `Gold_Cross`），否则不会配对共享模板。

## 真实示例

```csharp
[IsBlacksmithEnumModifier]
public static class ResourceExtension
{
    [IsBlacksmithEnumMember(0)]
    public static ResourceType.CEValue Cross(this ResourceType resourceType)
        => ResourceType.GetCEValue();
}
```

参考：`Project/Blacksmith/ModExamples/HolyBookMod/EnumExtension.cs`

## 注意事项

1. 多个 Mod 修改同一成员 → 结果取决于 DLL 加载顺序
2. 同名枚举类型 → 抛异常
3. `priority` 只决定排序，不保证跨 Mod 兼容性
4. 启动后 `CloseFactory()` 关闭枚举工厂（见[启动流程](../项目架构.md#3-启动流程)），运行动态创建无效
