# Blacksmith Framework

Blacksmith 是一个围绕《打铁》规则构建的可扩展对战框架。Xio 是基于同一基础设施（ClapInfra）构建的第二个拍手游戏。使用 `Blacksmith.cmd`（或 `Xio.cmd`）发布后，配置 Mod 目录并启动，即可在 `http://localhost:5000` 本地 Web 对战。

## 项目结构

| 项目 | 说明 |
|---|---|
| `Clap/ClapInfra` | 共享基础设施库。提供可扩展枚举框架 `ClapEnum<T>`、技能包反射配对机制 `ClapSkillPackage`、技能管理 `ClapSkill`、通用实体组件模板 `ClapBody`、决议缓冲区模型 `ClapTurnContext`、判定管线骨架 `ClapJudger`/`ClapJudgeRuleManager`/`ClapIntent`、DSL 编译契约 `IClapDSLSourceFile`、程序集扫描 `DllLoader`、枚举注册 `EnumRegistry`。不依赖任何游戏项目。 |
| `Clap/ClapSourceGenerators` | Roslyn 增量源生成器。编译时为 `ClapSkillPackage` 子类预计算技能注册元数据。基于 ClapInfra 的新游戏必须在 Core 项目中引用它。 |
| `BlacksmithCore` | Blacksmith 核心引擎。包含领域模型、技能 DSL、判定引擎、动态规则、AI 策略、Mod 加载器。基于 `ClapInfra`。 |
| `BlacksmithClient` | Blacksmith 运行入口。ASP.NET Core 本地站点，托管 `wwwroot` 静态前端，暴露 `/api/*` 最小 API。 |
| `ModExamples` | Blacksmith 示例 Mod 源码。演示扩展枚举 + 新职业 + Common 修改器 + 自定义防御的组合写法。 |
| `XioCore` | Xio 核心引擎。包含领域模型、技能 DSL、判定引擎、Mod 加载器。基于 `ClapInfra`。 |
| `XioClient` | Xio 运行入口。与 BlacksmithClient 架构相同的 ASP.NET Core 本地站点。 |

源代码位于 `Project/` 目录下，解决方案文件为 `Project/Project.sln`。所有项目目标框架为 `net8.0`。

## 运行方式

```powershell
# 发布纯净 Blacksmith（生成独立可执行文件 + .blacksmith 配置目录）
.\BlacksmithPure.cmd

# 或发布后附加Mod示例（包含圣书）
.\BlacksmithWithMods.cmd

# 运行 Blacksmith（发布后）
.\Blacksmith\BlacksmithClient.exe

# 发布 Xio
.\Xio.cmd

# 运行 Xio（发布后）
.\Xio\XioClient.exe
```

`.cmd` 脚本执行 `dotnet publish` 并将输出写入 `Blacksmith/`（或 `Xio/`），同时自动创建 `.blacksmith/mod.json`（或 `.Xio/mod.json`）配置文件。如无添加mod需求无需关心这些配置。

## 对战模式

| 模式 | 说明 |
|---|---|
| **Manual** | 双方技能均由前端手动输入，适合调试规则。 |
| **BloodSigil** | 使用 `BloodSigilStrategy`，基于规则的启发式 AI。 |
| **General** | 使用 `GeneralStrategy`，基于 MCTS 搜索的通用 AI。可通过 `data.json` 读取评分参数。 |

## 内置职业

当前核心库内置的主职业包：

- **Common** — 通用技能。
- **Cannon** — 钢炮。高物理伤害，穿甲弹可打断对手并穿透非真实防御。
- **Driver** — 驱动器。被动每回合获得真实伤减，依赖时空资源转换和爆发攻击。
- **Warlock** — 术士。魔法职业，可制造多回合延迟攻击、禁言对手时空获取，有炼金子职业。
- **BloodSigil** — 鲜血印记。以生命值为代价换取高伤害与吸血，转职时 +3 MHP/+3 HP 并移除部分基础攻击。
- **Lancer** — 战矛。纹章系统职业，命中可附加火/冰/光/暗四种纹章效果，蓄力后爆发魔法伤害。

`ModExamples/HolyBook.cs` 提供了圣书职业的部分示例实现。

## 文档导航

- [Blacksmith规则说明](./Documents/规则/BlacksmithRuleCN.md)
- [Xio规则说明](./Documents/规则/XioRuleCN.md)
- [项目架构](./Documents/项目架构.md)
- [Mod 基础指南](./Documents/Mod基础指南/引言.md)
- [Mod 进阶指南](./Documents/Mod进阶指南/引言.md)

## 扩展机制概览

Blacksmith 的扩展体系是"启动期装配型 Mod 架构"：

1. `Blacksmith.cmd` 发布时自动创建 `.blacksmith/mod.json`（Xio 对应 `.Xio/mod.json`）
2. 用户将 Mod DLL 放入子目录，在 `mod.json` 中声明路径
3. 程序启动时 `ModLoader` 读取 `mod.json`，通过 ClapInfra 的 `DllLoader` 扫描指定目录下的全部 `.dll`
4. 反射发现 `BlacksmithEnum<T>` 子类、`[IsBlacksmithEnumModifier]` 静态类、`MainProfession` / `ProfessionModifier` 子类
5. 注册扩展枚举成员、职业名和职业修改器
6. 调用 `ClapEnum.CloseFactory()` 关闭枚举工厂

之后进入运行期，所有装配结果直接可用。不支持运行中热插拔。

核心扩展点：
- **可扩展枚举**：`ResourceType`、`DefenseType`、`AttackType`、`EffectType`、`EffectTargetType`、`DynamicJudgeRuleName`、`JudgeStage`
- **技能包**：`MainProfession`（主职业）和 `ProfessionModifier`（挂到已有职业上的技能补丁）
- **防御类型**：继承 `DefenseBase` 实现新的防御行为
- **AI 策略**：实现 `IAIStrategy` 接口

## 关于 Xio

Xio 是与 Blacksmith 并行构建的第二个拍手游戏，共享 ClapInfra 基础设施。它的存在验证了 ClapInfra 的"机制不内容"设计——通过继承 ClapInfra 的泛型抽象并注入自己的组件、枚举和规则，Xio 仅用约 30 个源文件就完成了完整的游戏引擎。两个游戏的 DSL、判定管线、职业系统和枚举框架都建立在同一套 ClapInfra 基类之上，但具体实现完全独立。
