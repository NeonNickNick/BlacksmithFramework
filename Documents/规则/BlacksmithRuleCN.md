# 《打铁》核心规则手册 v1.3

> 核对最新实现、技能数值或职业细节，请以 `Project/Blacksmith/BlacksmithCore` 下实际代码为准。Mod 示例职业以 `Project/Blacksmith/ModExamples/` 下实际代码为准。

---

## 1. 游戏基础逻辑

- **模式：** 双人同步回合制博弈
- **动作经济：** 每回合双方必须且只能声明 1 个技能
- **胜负：** 初始 10 HP / 10 Max HP。HP ≤ 0 死亡，同回合双死为平局

### 回合结算流程

1. **声明与支付：** 双方亮出技能，立刻扣除资源/生命成本
2. **效果及部署：** 结算非攻击性效果——获取资源、回复生命、建立装甲（永久）、部署伤减（单回合）
3. **交锋与伤害：** 对冲（按优先级 1:1 抵消）→ 突防（剩余攻击力穿透伤减 → 装甲 → HP）→ 结算打断/吸血/纹章
4. **结束：** 清理单回合 Buff，判定生死

---

## 2. 资源与数值

- **基础资源：** Iron（最小单位 0.5）、Time、Space
- **内置专属资源：** Magic（术士）、gIron（金铁/炼金术）、Marks（战矛纹章状态）
- **Mod 专属资源：** Cross（圣书）、Dream（幻书）、Spirit（梦魇）、Fire/Water/Wood/Earth（炼药锅）、Bolt（弩）、Jade（武僧）
- **HP 作为成本：** 鲜血印记、术士、幻书、武僧均可消耗 HP

### 伤害与防御优先级

- **伤害：** Real(0) > Magical(128) > Physical(256)
- **伤减（单回合）：** PercentageReduction(24) < CommonReduction(16) < ThornReduction(8) < RealReduction(0)
- **装甲（永久）：** CommonArmor(128) < RealArmor(64) < StoneShell(32)
- **特殊防御：** PhysicalImmunity(-16) / MagicalImmunity(-16) 完全免疫对应类型；GreyHP（亵渎）提供可成长的真实伤减；PermanentRealReduction（免罪）提供永续真实伤减

ThornReduction 吸收物理攻击时反弹 50%（向上取整）为 Magical 伤害。PercentageReduction 以百分比减免伤害（baseline 为分母基准，如 baseline:100 时每点防御 = 1% 减伤）。

---

## 3. 技能与职业数据

### 3.1 基础公共技能

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 打铁 | 0 | 1 Iron |
| 刺 | 0.5 Iron | 1 ATK（鲜血印记不可用） |
| 钻 | 1.5 Iron | 3 ATK（鲜血印记不可用） |
| 切 | 2.5 Iron | 5 ATK（鲜血印记不可用） |
| 盾(n) | 0 + 0.5n Iron | 2 + n DEF |
| 刺盾(n) | 1 + 0.5n Iron | 4 + n CDEF |
| 恢复(n) | n Iron | 回复 2n HP |
| 时间 | 3 Iron | 1 Time |
| 空间 | 3 Iron | 1 Space |
| 撕裂 | 1 Space | 8 ATK |
| 反射 | 2 Space | 本回合所有即时攻击和指向敌方的效果推迟一回合生效 |

### 3.2 职业购买

购买职业视为本回合动作。全局限购一次，绑定后不可更改，其他主职业选项永久失效。

| 职业 | 成本 | 效果 |
|:---|:---|:---|
| 钢炮 | 4 Iron | 获得钢炮特性 +3 DEF |
| 驱动器 | 3 Iron | 获得驱动器特性 |
| 术士(法杖) | 1 Iron | 获得术士特性 |
| 鲜血印记 | 7 Iron | +4 HP, +4 Max HP，获得鲜血印记特性。禁止使用刺/钻/切 |
| 战矛 | 3 Iron | 获得战矛特性 |
| 圣书 | 2 Iron | 获得圣书特性（Mod） |
| 幻书 | 2.5 Iron | 获得幻书特性（Mod） |
| 炼药锅 | 3 Iron | 获得炼药锅特性（Mod） |
| 弩 | 2 Iron | 获得弩特性（Mod） |
| 武僧 | 3 Iron | 获得武僧特性（Mod） |

### 3.3 职业专属技能

#### 钢炮

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 炮击 | 1 Iron | 4 ATK |
| 二连击 | 2 Iron | 8 ATK |
| 三连击 | 3 Iron | 11 ATK；结算后返还 0.5 Iron；下次钢炮攻击 +1 伤（消耗后失效，可叠加） |
| 炮管 | 0 | 1 ATK + 2 DEF |
| 穿甲弹 | 1 Iron | 2 ATK。造成伤害则打断对手本回合"打铁"或"积魔"。对非真实防御时伤害临时 ×3 穿透，离开后 ÷3（向上取整）还原 |

#### 驱动器
**被动 - 时之盾：** 每回合自动产出 1 + 2n RDEF（n = 当前 Time 数，上限 5 RDEF）。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 空间冲击(n) | n Space | 12n ATK |
| 空时变幻 | 1 Space | 1 Time + 3 RDEF；下次驱动器空间冲击 +1 伤（消耗后失效，可叠加） |
| 时空变幻 | 1 Time | 1 Space + 3 RDEF；下次驱动器空间冲击 +1 伤（消耗后失效，可叠加） |
| 空间屏障(n) | n Iron | -0.5n² + 5.5n RDEF (n ≤ 5) |

#### 术士

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 积魔 | 1 Iron | 1 Magic |
| 魔法(n) | n Magic | 每回合 2n MATK，悬挂于攻击预备区，持续 3 回合 |
| 禁言 | 0 | 对手本回合时空获取无效 |
| 献祭 | 1 HP + 1 Max HP | 7 RDEF + 2 Iron |

**二级装备：**

| 装备 | 成本 | 效果 |
|:---|:---|:---|
| 炼金术 | 2 Iron | 解锁"点铁成金" |
| 点铁成金 | 1 Iron | 5 gIron |

#### 鲜血印记

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 血刃 | 4 HP | 6 ATK，造成伤害 75% 吸血 |
| 嗜血 | 2 HP | 下次攻击伤害 +50%（向上取整） |
| 恢复 | 0 | 回复 2 HP。前提：本回合结算前未受攻击伤害 |
| 血之盾 | 1 HP | DEF = 当前 HP × 40%（向上取整） |
| 狂怒 | 1 HP | 5 ATK（仅 HP ≤ 5 可用），造成伤害 150% 吸血 |

#### 战矛
**特性 - 纹章系统：** 技能造成伤害后获得特定纹章，下次攻击消耗并附加效果。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 天击 | 1 Iron | 3 ATK + 打断。命中获【火纹】：下次攻击首段 +2 |
| 龙牙 | 1 Iron | 3 ATK + 3 DEF。命中获【冰纹】：下次攻击获 +2 AMR |
| 霸碎 | 1 Iron | 3 ATK + 两倍穿甲。命中获【光纹】：下次攻击获 +2 HP |
| 连突 | 1 Iron | 2+2+1 ATK（三段同回合）。命中获【暗纹】：下次攻击附两回合 1 RATK |
| 伏龙翔天 | 4 Iron | 10 MATK |
| 蓄力 | 4 Iron | 下次伏龙翔天 +4 MATK。<br>减耗：下回合伏龙翔天/蓄力成本 0 Iron。<br>自动反击：本回合受攻击则视为蓄力完成，立刻触发 0 成本伏龙翔天。<br>最多连续蓄力两次 |

#### Monk
| 技能 | 成本 | 效果 |
|:---|:---|:---|
| Jade | 1 Iron | 1 Jade |
| GhostStep | 1 Jade + 1HP| 创造一个同等生命值的分身。Monk受到的直接性普通和魔法伤害将按照创造顺序从前到后逐次转移50%（向上取整，总和不变）至每个分身。最多同时存在两个分身。<br>注：当有两个分身时，逐次指第一个转移50%，第二个转移25% |
| GoldenBellCover | 1 Jade | 直接性普通和魔法伤害(100-60*n)%伤减一回合，n为已使用过此技能的次数。<br>注：n大于2时变为易伤 |
| MazeFist | 1 分身 | 消耗生命值最高的分身，6ATK，若击中甲，则下回合闪避所有直接伤害 |
| Mist | 1 iron | 给予生命值最低的分身3HP恢复*3回合，3回合内伤害逐次转移比例提升至90% <br>注：每三回合最多使用一次|
| Disillusionment | 2 Jade | 6ATK + 4MATK|
| 被动 | 0 | 每主动或被动失去一个分身，下回合追加一次伤害减半的Disillusionment |
| 分身 | 0 | 每回合给Monk施加一次GoldenBellCover，Monk和每个分身独立计数<br>注：GhostStep创建分身当回合伤害转移即生效，但分身下回合才开始释放此技能|

#### 预言者
| 技能 | 成本 | 效果 |
|:---|:---|:---|
| Crystal | 1.5 Iron | 1 Crystal |
| CrystalBall | 2 Crystal | 1 CrystalBall, 最多持有1个 |
| Foretold | 1 iron | 闪避所有直接伤害 |
| GreatestCaution | 1 Crystal | 下次闪避时，如果闪避成功，下回合反伤4RATK |
| Revelation | 1 Iron | 下次闪避时，如果闪避成功，额外获得一个Crystal |
| Assertion | 0 | 闪避所有直接伤害，但如果对方这回合没有攻击，失去2HP |
| CrystalWall | 1 CrystalBall | 7RDEF * 2回合，100%物理反伤（反为魔法伤害），且如果该RDEF受到攻击则返还1Crystal |
| Refraction | 要求持有CrystalBall | 50%伤减 |
| Ultimatum | 1 CrystalBall + 2 Crystal | 宣告一个正整数n，要求n > 5，n回合后追加12RATK |
| 被动 | 0 | 每次闪避成功后立即获得一个Crystal |

#### 圣书

> 源码：`Project/Blacksmith/ModExamples/HolyBookMod/HolyBook.cs`

专属资源：**Cross**（圣痕）。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 圣痕 | 0.5 Iron | 1 Cross |
| 祈祷 | 0 | 3 DEF |
| 约柜 | 2 Cross | 8 ATK + 1 50%伤减 |
| 亵渎 | 1 Cross | 2 RATK + (2+n/3) GreyHP。n 为已使用亵渎次数（向上取整） |
| 重生 | 1 Cross | 3 HP恢复 × 3回合 + 25%伤减 × 3回合（每回合独立部署） |
| 免罪 | 1 Cross | 1 永续真实伤减（PermanentRealReduction，跨回合不消失） |

**防御机制：**
- **GreyHP**：可变值的真实伤减，亵渎使用次数越多越厚
- **PermanentRealReduction**：与其他伤减不同，不会在回合结束时清除

#### 幻书

> 源码：`Project/Blacksmith/ModExamples/PhantomBookMod/PhantomBook.cs`

专属资源：**Dream**（梦境）。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 幻想曲 | 0.5 Iron | 1 Dream |
| 联想 | 2 Dream | 复制对手一个可用技能并施放（消耗由自己支付）。不可复制转职/装备技能/联想自身 |
| 致幻 | 2 Dream | 对手本回合及之后所有攻击延迟 +1 回合（无限持续预处理钩子） |
| 梦醒时分 | 最高 2 Dream | 将自身状态回滚到 3 回合前的沙盒快照（含 HP、资源、防御） |
| 幻觉 | 1 Dream | 回复 5 HP |
| 梦魇（装备） | 1 Dream + 5 HP | 物免 + 6 梦魇装甲。装甲破裂时恢复梦魇入口技能并移除子职业包 |

**二级装备/子职业 - 梦魇：**

> 源码：`Project/Blacksmith/ModExamples/PhantomBookMod/Nightmare.cs`

专属资源：**Spirit**（魂）。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 梦潜 | 1 Dream | 2 RATK + 5 DEF + 回复 1 HP + 1 Spirit + 魔法免疫 |
| 具现 | 1 Dream + 2 Spirit | 4 ATK + 4 RATK + 4 DEF |
| 缠身 | 1 Spirit | 2 MATK（本回合）+ 2 MATK（下回合）+ 1 MATK（第三回合） |
| 通灵 | 1 Spirit | 回复 3 HP + 2 DEF + 魔法免疫 |

#### 炼药锅

> 源码：`Project/Blacksmith/ModExamples/CauldronMod/Cauldron.cs`

专属资源：**Fire**（火）、**Water**（水）、**Wood**（木）、**Earth**（土）。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 火 | 1 Iron | 1 Fire |
| 水 | 1 Iron | 1 Water |
| 木 | 1 Iron | 1 Wood |
| 土 | 1 Iron | 1 Earth |
| 爆炸(n) | n Fire | 4n MATK |
| 冰刃(n) | n Water | 5n ATK |
| 再生 | 1 Wood | 1→2→3 递增 HP 恢复 × 3回合 |
| 岩壳 | 1 Earth | 一次性完全吸收物理攻击 |
| 燃命 | 1 Fire + 1 Wood | 25%伤减 × 4回合；真实伤害减半 × 4回合；1回合后全部攻击翻倍 × 3回合；3回合后自毁（扣 114514 MHP） |
| 火雨 | 1 Fire + 1 Earth | 8 ATK + 2 RATK + 1 RATK（同回合三段） |
| 元素之甲（装备） | 1 Earth + 1 Water | 物免 + 8 元素装甲。禁用所有其他技能包，切换为元素之甲模式 |

**二级装备/子职业 - 元素之甲：**

> 源码：`Project/Blacksmith/ModExamples/CauldronMod/ElementalArmor.cs`

进入元素之甲模式后所有原有技能被禁用，仅可使用以下技能。装甲破裂后恢复原有技能包。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 锤 | 0 | 6 ATK |
| 守护 | 0 | 8 DEF |

#### 弩

> 源码：`Project/Blacksmith/ModExamples/CrossBowMod/CrossBow.cs`

专属资源：**Bolt**（弩箭）。

| 技能 | 成本 | 效果 |
|:---|:---|:---|
| 制箭 | 1 Iron | 3 Bolt |
| 箭雨(n) | n Bolt | n ATK |
| 瞄准 | 0 | 下次暴击强化：物理伤害归零，真实伤害从 1 提升至 2 |
| 暴击 | 1 Bolt | 未瞄准：1 ATK + 1 RATK。瞄准后：0 ATK + 2 RATK。瞄准状态消费后复位 |
| 格挡(n) | 0.5n Bolt | (4.5n - 0.5n²) DEF（n=1~4，盾牌公式抛物线）。Iron 不足不可用 |
| 标记弹 | 1 Bolt + 1 Iron | 1 ATK。下回合对手所有即时攻击在命中身体时伤害翻倍 |