## v6.18.0（historical fork notes）

> 本仓库为 [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 的本地增强型分叉，跟随上游 `master-v2`。
> 与上游 v6.17.0 → v6.18.0-beta.1 的三方合并已于 2026-09-16 以 merge `6d32144858` 合入 staging，Fork 私有功能保持兼容。
> 详细工作流与差异说明见 [`README.md`](./README.md)、[`AGENTS.md`](./AGENTS.md) 和 [`WORKFLOW.md`](./WORKFLOW.md)。

### Fork 增量（随 merge `6d32144858` 保留）

- 多账号自动轮换日常任务（`feat/account_rotation`），肉鸽与生息演算延后到最后（`feat/defer-rogue`）
- 公招加急门槛（`feat/expedite-threshold`）：`expedite_min_level` Fork 协议字段，与上游 `expedite` / `expedite_times` 并存
- 公招加急按目标槽位点击（`fix/recruit-expedite-slot-target`），避免多槽位串位
- 干员 / 仓库识别数据按账号分桶（`feat/account-scoped-recognition-data`），切号即切桶
- 「生成诊断报告」重构（`fix/diagnostic-export-refactor`）：分卷导出 + 系统信息收集
- 会客室线索快捷置入失败回退 legacy 循环（`fix/reception-clue-restore`）：修复上游 issue #16165
- 账号轮换 UI 整理与启动竞态保护（`fix/account-rotation-supersede-switcher`、`fix/account-cycle-start-race`）
- 仓库识别特别纪念 tab 修复（`fix/depot-special-memorial-tab`）：与上游 `da5ccfe4ed` 在 v6.18 自然收敛
- 切号异步丢失保护（`fix/account-switch-retry`），增量本地构建与 NetBeauty2 文档补齐
- 本地构建脚本强化、`tools/local-install-staging.bat` 与便携 MSVC（C:\msvc-portable）兼容路径

### v6.18 上游新功能（master-v2 v6.17.0 → v6.18.0-beta.1，随 merge 接入）

- 运行时重构：`RunningState` → `RunControlState` + `RunOwner`，所有 Idle/Inited/Stopping 公开属性收敛到 `Run.*`
- 三方服务设置页（Yituliu OpenAPI Token / Penguin.id 迁移）
- 基建副手换班（`InfrastAssistantChangeTask`）与 MaterialSynthesis 自动合成
- Roguelike 黑流树海适配（v6.17.0 已含，beta.1 跟进）
- GPU 驱动信息展示、`Achievement` 成就等次要功能
- 切主题（`SwitchTheme`）自动切换


> 本仓库为 [`MaaAssistantArknights/MaaAssistantArknights`](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 的本地增强型分叉，跟随上游 `master-v2`。
> 与上游 v6.17.0 的同步已在 `a92a8a9124` 完成，Fork 私有功能保持兼容。
> 详细工作流与差异说明见 [`README.md`](./README.md)、[`AGENTS.md`](./AGENTS.md) 和 [`WORKFLOW.md`](./WORKFLOW.md)。

### Fork 增量（持续累积）

- 多账号自动轮换日常任务（`feat/account_rotation`），肉鸽与生息演算延后到最后（`feat/defer-rogue`）
- 公招加急门槛（`feat/expedite-threshold`）：`expedite_min_level` Fork 协议字段，与上游 `expedite` / `expedite_times` 并存
- 公招加急按目标槽位点击（`fix/recruit-expedite-slot-target`），避免多槽位串位
- 干员 / 仓库识别数据按账号分桶（`feat/account-scoped-recognition-data`），切号即切桶
- 「生成诊断报告」重构（`fix/diagnostic-export-refactor`）：分卷导出 + 系统信息收集
- 会客室线索快捷置入失败回退 legacy 循环（`fix/reception-clue-restore`）：修复上游 issue #16165
- 账号轮换 UI 整理与启动竞态保护（`fix/account-rotation-supersede-switcher`、`fix/account-cycle-start-race`）
- 账号切号异步丢失保护（`fix/account-switch-retry`），增量本地构建与 NetBeauty2 文档补齐
- 本地构建脚本强化、`tools/local-install-staging.bat` 与 VS 2022 BuildTools 兼容路径

## v6.19.0

### Highlights

#### 新增 ｢干员培养｣ 任务（测试功能）

为干员设定精英化、技能等级与专精目标后，任务将自动逐步完成培养流程，材料即缺即合自动合成。该功能为测试功能，可能存在较多未发现的 bug，遇到问题欢迎通过 ｢设置 - 问题反馈｣ 反馈。

#### 从指定任务处继续运行

任务列表新增 ｢从此处运行｣ 右键菜单项，可从指定任务处开始执行任务队列，跳过其之前的任务，任务失败后无需从头重跑；指定任务未启用时从其后首个已启用的任务开始。

#### 库存保持导入当前活动预设与按达成率排序

库存保持预设新增 ｢当前活动｣ ，一键导入当前开放活动的关卡掉落为目标；新增 ｢按库存达成率排序执行｣ 选项，开启后每次运行把理智优先投给缺口比例最大的计划。

#### 界面显示干员头像与物品图片

干员识别、自动战斗作业预览、干员培养与肉鸽开局设置等界面新增干员头像与职业图标显示，干员识别卡新增 ｢头像展示｣ 样式选项；理智作战与库存保持的指定材料下拉、牛杂 ｢活动商店｣ 的黑名单多选框同步支持显示物品图片。

#### 自动战斗一图流数据预检

自动战斗可使用一图流的干员练度数据预检作业编队，提前校验编队中干员的练度是否满足作业要求；编队结果按干员实际数据匹配优化，需在设置中配置一图流 Token。

<details>
<summary><b>English</b></summary>

#### New Task: Operator Progression (Testing)

Set elite promotion, skill level, and mastery targets for an operator, and the task completes the training flow step by step with on-demand material synthesis. This feature is still in testing and may contain undiscovered bugs; feedback is welcome via Settings - Issue Report.

#### Resume the Queue from a Selected Task

A "Run from Here" option is added to the task list context menu, letting you start the queue from any task and skip the ones before it, so a failed run no longer has to restart from scratch. If the selected task is disabled, the queue starts from the first enabled task after it.

#### Current Event Presets and Progress-Based Ordering in Depot Maintain

The Depot Maintain presets gain a "Current Event" option that imports the drops of the currently open event as targets in one click. The new "Execute in inventory progress order" option makes each run spend sanity on the plan with the largest shortfall first.

#### Operator Avatars and Item Images in the UI

Operator avatars and class icons are now shown in the operator recognition view, copilot formation preview, operator progression, and roguelike start-up settings; the recognition card gains an "Avatar View" style option. The material dropdowns in Combat and Depot Maintain, as well as the event shop blacklist checkboxes, now display item images as well.

#### Copilot Formation Pre-check with Yituliu Data

The copilot feature can pre-check the formation against operator data from Yituliu, verifying in advance whether the operators in a formation meet the operation's requirements; formation assignment is optimized with real operator data. A Yituliu token needs to be configured in Settings.

</details>

----

以下是详细内容：

<details open>
<summary><b>v6.19.0 (2026-10-11)</b></summary>

### 新增 | New

* 新增 ｢干员培养｣ 任务：为干员设定精英化、技能等级与专精目标后自动逐步完成培养流程，材料即缺即合自动合成，训练室专精自动选择协助者；支持导入多语言干员名与拖拽调整培养顺序，任务运行中锁定参数编辑 ([#18137](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18137) [#18329](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18329) [#18413](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18413) [#18425](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18425) [#18443](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18443)) @Lancarus @youzibigg @HX3N @Constrat @status102 @Manicsteiner @momomochi987
* 任务列表支持右键 ｢从此处运行｣ ，从指定任务处开始执行任务队列，跳过其之前的任务；指定任务未启用时从其后首个已启用的任务开始 ([#18317](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18317)) @H2O-MERO
* 库存保持预设新增 ｢当前活动｣ ，一键导入当前开放活动的关卡掉落为目标；新增 ｢按库存达成率排序执行｣ 选项，开启后每次运行把理智优先投给缺口比例最大的计划 @ABA2396
* 自动战斗新增一图流数据预检：作业编队按一图流干员练度数据校验可行性，编队结果改用最小费用流匹配优化；设置中新增一图流 Token 配置与获取链接 ([#17414](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/17414)) @yali-hzy @ABA2396
* 基建换班新增周计划，换班方案支持按星期几勾选启用，当日未勾选时跳过基建任务 ([#18440](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18440)) @H2O-MERO
* 定时设置新增 ｢定时启动前尝试唤醒计算机并启动 MAA｣ ，通过 Windows 任务计划程序在每个启用的定时项前 5 分钟唤醒计算机并拉起 MAA（部分设备仅支持从睡眠唤醒，不支持从休眠唤醒） ([#18505](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18505)) @ABA2396
* 干员识别、自动战斗作业预览、作业集预览、干员培养与肉鸽开局设置等界面新增干员头像与职业图标显示，干员识别卡新增 ｢头像展示｣ 样式选项，干员选择框与指定材料下拉在收起状态显示已选头像或物品图标 ([#18352](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18352) [#18536](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18536)) @Lancarus @youzibigg @ABA2396
* 干员名、关卡名等 OCR 纠错规则拆分至独立热修复资源，此类修复随资源更新即时分发，无需等待完整版本更新 ([#18406](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18406)) @ABA2396
* 牛杂 ｢活动商店｣ 新增商品黑名单，可按商品名称或物品 ID 排除自动购买，选项列表显示材料图标 ([#18270](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18270)) @H2O-MERO @ABA2396
* 新增界面主题 ｢幽脉｣ ｢剧目｣ ([#18002](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18002)) @SherkeyXD
* 完成后动作新增 ｢锁屏｣ （Windows），与休眠、关机、睡眠选项互斥 ([#18412](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18412)) @H2O-MERO
* 连接设置新增 ｢MuMu 模拟器（Windows ARM）｣ 连接预设，支持自动检测并测试 ([#18380](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18380)) @jisanjin41
* PC 端启动设置新增 PC 客户端配置：可设置客户端路径与启动等待时间，支持启动 MAA 后自动开启客户端、连接失败时尝试启动客户端并重连 ([#18026](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18026)) @moranfanhua
* PC 端窗口绑定新增 AnchoredTouch 鼠标输入方式（全后台、不抢占鼠标，需 Win10 1809+，被遮挡时窗口短暂闪烁） ([#18341](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18341)) @wkrs15
* PC 端连接设置新增 ｢最小化游戏窗口｣ 选项（连接设置 - Win32 附加选项），修复任务运行过程中无法最小化游戏窗口的问题 ([#18419](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18419)) @H2O-MERO

### 改进 | Improved

* 外部通知设置重组：新增独立 ｢通知设置｣ 区块（日志有效时间、日志更新停滞检测），发送条件扩展为任务完成、任务出错、定时执行前、日志输出停滞、新增日志条数与日志内容匹配（白名单正则），支持附带历史日志并按数量、有效时间与黑名单过滤，解决 Bark/Discord 因日志过长推送失败的问题 ([#18445](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18445) [#18459](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18459)) @H2O-MERO @ABA2396
* 黑流树海肉鸽商店按策略选择购买与出售列表，完善商店交互流程 ([#18501](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18501) [#18506](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18506)) @ZiyinLin
* 优化黑流树海肉鸽事件选择，按事件策略规则选择选项并在界面日志中显示理由，调整默认选择，固定选项改用编号 ([#18339](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18339) [#18455](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18455)) @ZiyinLin @H2O-MERO
* 黑流树海肉鸽新增地图拓扑模板识别，节点预览揭示节点实际身份后立即重新规划路线 ([#18368](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18368)) @ZiyinLin
* 重组设置页区块，性能配置迁移为全局设置，不再跟随配置档案切换 ([#18551](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18551)) @ABA2396 @H2O-MERO
* 任务日志输出各任务用时 ([#18433](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18433)) @H2O-MERO
* 任务栏进度改为按 Core 任务链计数 ([#18338](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18338)) @ABA2396
* 设置指引新增添加任务引导：主界面任务按钮与添加任务菜单显示红点，任务设置步骤需先添加任务才能进入下一步 @ABA2396
* 调整会客室与控制中枢的基建入驻优先级 ([#18449](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18449) [#18450](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18450)) @Reverse0xCC
* 基建生产状态识别兼容活动主题（换肤）设施 ([#18495](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18495)) @panda361
* 库存保持列表启用虚拟化、像素滚动与惯性滚动，操作按钮区固定显示 @ABA2396
* 库存保持增加预检语义提示与识别后缓存翻转警告，理智作战与库存保持的指定材料未选择时显示圆环占位图标 @ABA2396
* 库存保持各预设的默认目标量按常见需求量调整 ([#18574](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18574)) @Daydreamer114
* 自动战斗 ｢使用编队｣ 选项状态会被记忆 ([#18523](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18523)) @H2O-MERO
* 大幅优化彩虹文字动画的资源占用，并优化显示效果 ([#18432](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18432)) @H2O-MERO
* 内核初始化完成前开始任务时弹出警告 ([#18436](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18436)) @H2O-MERO
* 任务链失败时统一保存现场截图至 debug/interface ([#18383](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18383)) @jinghero
* Python 集成接口更新时清理新版本已移除的旧资源文件 ([#18520](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18520)) @djx-trans-n
* 主窗口最小化或启动早期产生的应用内提示改为暂存，窗口恢复后统一显示 @ABA2396
* YostarKR add the Reconstruct UI theme @HX3N
* PC 端任务运行中允许点击 ｢窗口恢复｣ ([#18278](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18278)) @H2O-MERO
* PC 端跳过掉落上报的提示改为指明具体三方平台，避免重复输出相同内容 ([#18311](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18311)) @H2O-MERO
* PC 端截图测试结束后恢复游戏窗口位置 ([#18427](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18427)) @H2O-MERO
* Rename the roguelike start-up item option from "Thought" to "Plan" in the English UI @Constrat

### 修复 | Fix

* 各任务开始阶段遇到掉线弹窗时立即停止任务队列并取消后续任务，不再等待重试超限逐个报任务出错 ([#18575](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18575)) @XXLC @ABA2396
* 适配新版登录页账号管理与设置页用户中心入口位置，账号管理识别区域同时兼容新旧登录页布局 ([#18524](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18524) [#18538](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18538)) @Aliothmoon
* 修复战斗结束时等级提升界面点击卡住后无法继续结算的问题 ([#18361](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18361)) @youzibigg
* 修复作业集模式错误地向作业追加 SkillDaemon 动作的问题 @status102
* 修复 SSS 保全派驻部分策略作业战斗等待期间不推进的问题 @ABA2396
* 补齐六个肉鸽主题中升变阿米娅术师与近卫形态的职业配置，修复使用对应形态时的配置缺失异常 ([#18414](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18414)) @ZiyinLin
* 修复肉鸽开局干员手动输入缺少校验的问题 ([#18379](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18379)) @youzibigg
* 修复黑流树海肉鸽 ｢失与得｣ ｢先行一步｣ 等事件卡住或报任务出错的问题 ([#18339](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18339)) @ZiyinLin
* 修复黑流树海肉鸽首次进层时楼层名识别失败导致地图重建缺少楼层记录的问题，现在优先在缩放后的画面上识别，持续失败时报错停止 ([#18390](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18390)) @ZiyinLin
* 修复黑流树海肉鸽前往预览未按步行与加工品分别读取行动力消耗的问题 ([#18539](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18539)) @ZiyinLin
* 修复黑流树海肉鸽事件 OCR 识别与 ｢出发前往｣ 按钮模板匹配失败的问题 ([#18516](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18516) [#18529](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18529)) @ZiyinLin
* 修正黑流树海肉鸽 ｢秘境行商｣ 事件名称的错别字（必境→秘境） ([#18420](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18420)) @youzibigg
* 修复界园肉鸽事件选项无限滑动的问题 ([#18477](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18477)) @ZiyinLin @Constrat
* 修复界园肉鸽铜币交换完成后偶发识别失败导致任务异常的问题 ([#18423](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18423)) @Manicsteiner
* 修复基建常规换班将本轮已安排的干员重复分配到其他设施的问题 ([#18346](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18346)) @youzibigg
* 修复基建常规模式宿舍入驻的问题 ([#18447](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18447)) @Reverse0xCC
* 修复基建换班评分误将 ｢泰拉的方舟｣ （人力办公室技能）按加工站技能评分的问题 @Lancarus
* 修正基建训练室 ｢专精完成｣ 的识别模板与识别区域 @Lancarus
* 修复芯片助剂补购返回流程异常并调整商店滑动起点 ([#18498](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18498)) @youzibigg
* 修复生息演算 RA4 关卡名括号误识别、完成识别在部分分辨率下失败与完成等待的问题 @Saratoga-Official
* 修复物品模板无效时仓库识别继续运行的问题 ([#18521](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18521)) @Arthur031221 @wzacolemak
* 修复自动战斗切换作业页签时循环执行开关状态不同步的问题 ([#18567](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18567)) @yali-hzy @status102
* 修复自定义基建计划包含未适配的设施或产物时报错失败的问题，现改为跳过该计划 @ABA2396
* 修复任务出错跳过完成后动作时日志重复输出出错任务名的问题 @status102
* 修复主窗口标题栏文本滚动启用后高度未恢复为单行的问题 ([#18422](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18422)) @H2O-MERO
* 修复 Python 集成接口截图返回尺寸与实际图像不一致的问题 ([#18434](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18434)) @Arthur031221
* 我是小猪（修正识别替换规则中半角括号的正则转义） @Saratoga-Official
* 繁中服修复烏啾的识别 ([#18519](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18519)) @vonnoq
* 修复 PC 端任务结束后游戏静音未正常恢复的问题 ([#18411](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18411)) @H2O-MERO
* 修复 PC 端窗口恢复位置错误的问题，以及所有任务全部跳过时窗口停留在屏幕外的问题 ([#18430](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18430)) @H2O-MERO
* 修复 PC 端牛牛监控预览与任务截图相互干扰的问题，预览与任务截图改用独立缓存与采集流程 ([#18462](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18462)) @H2O-MERO
* YostarJP add the StageEncounterOptionUnknown template for the Sami roguelike ([#18277](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18277)) @LinYanxi0
* YostarJP add the missing NoStone template ([#18583](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18583)) @chanshengbinying
* YostarKR add ocrReplace for SpecialAccessActivities @HX3N
* YostarKR adjust the notification dot roi of AutoRaisePotential @HX3N

### 文档 | Docs

* 基建排班协议文档完善，新增排班表扩充协议说明 ([#18511](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18511)) @Zirun-wang
* 集成文档补充活动商店兑换黑名单参数 @ABA2396
* 集成协议文档补充自动提升潜能任务（AutoRaisePotential）的参数说明 @ABA2396
* winget 安装命令改为指定包 ID 与 winget 源，修正部分网络环境下 msstore 源证书校验失败导致安装中止的问题 ([#18513](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18513)) @djx-trans-n
* Android 实体设备文档新增 MAA 安卓版使用说明 ([#18503](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18503)) @Aliothmoon
* 修复多语言文档中 26 处失效的链接与页内锚点 ([#18549](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18549) [#18573](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18573)) @chanshengbinying
* 更新 README 界面截图至干员识别头像显示的新版界面 @ABA2396

### 其他 | Other

* 新增 --skip-core-init 启动参数，跳过内核加载用于界面预览 ([#18492](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18492)) @ABA2396
* MaaWpfGui 开启 TreatWarningsAsErrors 并清理 StyleCop 抑制 ([#18340](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18340)) @ABA2396
* 修复资源同步工具：基建模板按房间映射过滤、Recycle 设施校验与头像误判重写导致的资源更新失败 @ABA2396
* 资源同步脚本支持 hotfix 变更自动 bump version.json 与单跳同步 @ABA2396
* ImageCropper 新增无头 CLI 并改造为线坐标语义与两轴窄带网格定位，MaskRangeTool 支持无头调参，移除被取代的 ImageCoordinate 工具 @ABA2396
* 统一 XAML 格式化并约定 XAML Styler 工具链 @ABA2396
* 更新开发规范：WPF 配置存储不保留原始 Json、MaaCore 不手动拼接业务任务流 @status102
* 统一计划列表注释与绑定属性格式 @ABA2396
* 新增 ItemImageConverter 物品图标转换器 @ABA2396
* 补充与调整界面层代码注释 @ABA2396
* 版权年份更新至 2026 @ABA2396
* CI 升级 pnpm/setup 至 v3 ([#18358](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18358)) @dependabot[bot]
* CI 维护：合并 issue 检查与 AI 分析 workflow、PR 标题检查中英文间空格、调整日志上传失败提醒 @ABA2396 @Saratoga-Official
* 更新仓库内 AI 开发技能（活动导航、模板制作、README 截图演示） @ABA2396
* 统一界面文案、代码注释与文档的角引号格式为半角 @ABA2396

</details>

## v6.20.1

### Highlights

#### 基建换班支持训练室与回收站

新增基建新设施 ｢回收站｣ 的换班支持，常规模式按技能评分选择进驻干员；自定义基建排班可配置回收站与训练室，训练室分别指定训练位与协助者的候选干员；候选干员或编组不可用时房间保持原样，任务日志附原因提示。

#### 黑流树海 DLC 初步适配

难度选择新增 N16，分队选择新增团结阵线分队与踏足遍历分队两个新分队。

#### 黑流树海完整刷等级模式

黑流树海刷等级新增目标结局选项（现开放一结局），按所选结局推进；快速飞三层为独立勾选项（需满天赋机械师），勾选后临时以特勤分队、稳扎稳打与自己的机械师开局。

<details>
<summary><b>English</b></summary>

#### Infrast Shift Support for the Training Room and the Recycling Station

Add shift support for the new facility "Recycling Station", where operators are selected by skill score in Normal Mode; the Custom Base Mode schedule can now configure the Recycling Station and the Training Room, specifying candidates for the training slot and the assistant separately; when candidates or groups are unavailable, the room is kept unchanged, with the reason shown in the task log.

#### Preliminary DLC Support for BlackFlow

N16 difficulty is added to the difficulty list, and two new squads, 团结阵线分队 and 踏足遍历分队, are added to the squad list.

#### Full Level-Farming Mode for BlackFlow

A target ending can now be selected for BlackFlow level farming (the first ending for now), and the run pushes toward it; quick flight to Floor 3 remains available as a standalone checkbox (requires the maxed Mechanic talent), temporarily overriding the squad, roles and starting operator when checked.

</details>

----

以下是详细内容：

<details open>
<summary><b>v6.20.1 (2026-10-20)</b></summary>

### 新增 | New

* 黑流树海加入 DLC1 部分新加工品 ([#18758](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18758)) @ZiyinLin

### 改进 | Improved

* 黑流树海刷开局模式在准备局第一层商店自动加购种荚等物品 ([#18755](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18755)) @bhdtrrr
* YostarJP add 龍門 to OrundumActivities, add JieGarden StageEncounter leave confirm 720p template and update the signin event popup texts @Manicsteiner

### 修复 | Fix

* 修复更新日志窗口在宽度变化触发重排时崩溃的问题 ([#18772](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18772)) @crazysmile-PhD
* 修复黑流树海编队后重新进入战斗时点击位置错误的问题 ([#18731](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18731)) @youzibigg
* 修复黑流树海 ｢险路尽头｣ 选项错配的问题 ([#18757](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18757)) @ZiyinLin
* 修复黑流树海 ｢居民据点｣ 节点识别失败的问题 ([#18754](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18754)) @ZiyinLin
* 修复黑流树海 ｢误入奇境｣ 节点缺少路由处理的问题 @ZiyinLin
* 黑流树海扣款后等待源石锭数字滚动结束再读取余额，关闭加工品面板后等待界面加载完成 @ZiyinLin
* 放宽黑流树海零件箱超载标识的模板匹配阈值 @ZiyinLin
* 放宽界园放弃探索图标的颜色饱和度匹配范围，修复识别失败的问题 ([#18766](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18766)) @youzibigg
* 修复调试环境下程序退出时因 .old 目录不存在而报错的问题 @status102
* YostarJP update the Annihilation entrance template ([#18752](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18752)) @Rememorio
* YostarKR keep line breaks in the operator OCR replacement @HX3N
* YostarKR update the Awards templates @HX3N

### 文档 | Docs

* 补齐五语言协议文档与使用手册的近期行为变更描述，覆盖回调协议、集成接口、CLI 配置、基建排班与黑流树海资源清单 @ABA2396

### 其他 | Other

* CI 升级 apple-actions/download-provisioning-profiles 从 6 到 7 ([#18712](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18712)) @dependabot[bot]
* 任务队列界面的 UserControl 与 ViewModel 文件按 ｢XxxTaskUserControl｣ 命名规范统一重命名 @status102

</details>

<details>
<summary><b>v6.20.0 (2026-10-18)</b></summary>

### 新增 | New

* 基建自定义排班新增训练室与回收站设施支持，训练室可分别配置训练位与协助者的候选干员（训练位需已精二且仍有可专精的技能，协助者不限）；候选干员或编组不可用时房间保持原样，任务日志以设施卡片附原因提示；房间数组留空时整设施保持原样，未覆盖到的房间不换班并提示；候选选人排除正在工作中的干员，宿舍在干员心情回满后才让位；一键收获识别回收站 ｢材料产出｣ ([#18632](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18632)) @ABA2396
* 基建常规换班支持回收站设施，按技能评分选择进驻干员 ([#18601](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18601)) @Lancarus @youzibigg @ABA2396
* 黑流树海初步适配肉鸽 DLC，难度选择新增 N16，分队选择新增团结阵线分队与踏足遍历分队两个新分队；险路尽头出现第四个选项时仍选择说服同伴 ([#18697](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18697)) @ZiyinLin
* 黑流树海新增刷开局模式，先打准备局取得指定襁褓动物并抵达第三层，目标局开局命中期望奖励后不做选择、退出并保留这一局；第一只与第二只襁褓动物可分别指定（羽蛇至多一只），期望奖励与羽蛇奖励任意一项命中即停，奖励识别失败时停止任务并提示；原刷襁褓动物策略并入其中 ([#18696](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18696)) @ZiyinLin
* 黑流树海刷等级新增目标结局选项（现开放一结局），不勾选快速飞三层时按所选结局推进；快速飞三层改为独立勾选项（需满天赋机械师），勾选时开局设置不能修改，任务临时以特勤分队、稳扎稳打与自己的机械师开局；狭路相逢暂时按普通事件处理 ([#18681](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18681)) @ZiyinLin @ABA2396

### 改进 | Improved

* 完善黑流树海 ｢秘境行商｣ 交易流程，第一层进入行商时只培育补货不出售，第二层起出售闲置自然物并优先回购增值物品，交易异常时停止交易并离店 ([#18667](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18667)) @ZiyinLin
* 调整黑流树海路线偏好权重，第二层偏向 ｢不期而遇｣ 节点，带线人角标的不期而遇额外加分 ([#18664](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18664)) @ZiyinLin
* 优化黑流树海阶段目标的界面上报，按访问次数计分的路线偏好不再刷屏，界面按已完成、已错过或无法完成着色显示；节点外语名称与既有肉鸽主题译名统一 ([#18663](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18663)) @ZiyinLin
* 黑流树海任务日志不再附带路线决策原因与规则说明 ([#18680](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18680)) @ZiyinLin
* 材料合成与回收站换班选人时排除已进驻其他生产设施与训练室的干员 ([#18654](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18654)) @youzibigg @ABA2396
* 会客室入驻优先级新增克莱门莎 ([#18675](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18675)) @Reverse0xCC
* 优化基建设施设置页布局，默认基建选择合并为组合控件 @ABA2396
* 干员培养在训练室被占用且计划仅剩专精目标时跳过干员查找 @status102
* 多作业模式启用 ｢使用一图流数据辅助编队｣ 时，任务日志在预匹配各作业前显示其关卡与文件名 ([#18691](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18691)) @yali-hzy
* YostarJP update OCR replacements, add Zelgrad to OrundumActivities and add Reconstruct theme navigation templates ([#18669](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18669)) @Manicsteiner
* YostarKR add EP17-related OCR entries and the OrundumActivities notice board entrance @HX3N
* YostarKR recognize the unexecuted production plan popup when switching facilities @HX3N
* YostarKR recognize the quick formation role filter All button also displayed as ALL, with OCR misread correction @HX3N
* YostarEN add Zelgrad to OrundumActivities @Constrat
* YostarEN add missing GALLUS to GALLUS² battle name OCR replacement @Constrat
* YostarKR match the SpecialAccessActivitiesConfirm button with the full text "수령하기" @HX3N

### 修复 | Fix

* 修复基建换班切换设施时被 ｢尚未执行新的制造计划，确认离开设施｣ 弹窗卡住、后续任务全部失败的问题；源石碎片补货改为等待更改确认完成，换产物失败时同样处理该弹窗 ([#18655](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18655)) @djx-trans-n @ABA2396
* 修复水月肉鸽结算后卡在数据回溯界面的问题 ([#18656](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18656)) @chanshengbinying
* 修复黑流树海选择支援页奖励识别范围未覆盖整排奖励卡片、边缘奖励漏识别的问题 ([#18682](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18682)) @ZiyinLin
* 修复萨卡兹肉鸽设置开局干员后，点刺投资不直接放弃招募、仍按指定干员选招募券的问题 ([#18684](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18684)) @ZiyinLin
* 修复快速编队职业筛选 ｢ALL｣ 按钮被 OCR 误读为 ｢ALI｣ ｢AL1｣ ｢AL[｣ 等导致筛选失效的问题 ([#18619](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18619) [#18625](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18625)) @youzibigg @Aliothmoon
* 放宽加载提示的 OCR 匹配以容错误读，修复高延迟下等待提前结束打断流程的问题（国服） ([#18650](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18650)) @djx-trans-n
* 修正肉鸽招募配置中 ｢骋风｣ ｢艾丝黛尔｣ 等干员名错别字，修复对应干员招募优先级不生效的问题 ([#18677](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18677)) @chanshengbinying
* 修复肉鸽第二、三位开局干员头像显示异常的问题 @ABA2396
* 修复可搜索下拉框在输入法组字期间误过滤并意外选中干员的问题，影响肉鸽开局干员、关卡选择、干员培养与库存保持的输入框 ([#18631](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18631)) @Aliothmoon
* 修复干员培养任务页面计划列表显示不全的问题 @status102
* 修复刷理智战斗在代理倍率与战斗次数上限组合下提前结束时，错误触发战斗次数提示日志的问题 @status102
* 修复自动提升潜能信物不足提示的识别，兼容提示换行显示 ([#18594](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18594)) @Craun718 @HX3N
* YostarJP fix in-battle name recognition for Pramanix when the name is misread by OCR ([#18683](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18683)) @Radekyspec

### 文档 | Docs

* 明确黑流树海购物配置的适用范围与校验规则，命名买卖表与价格字段仅对黑流树海生效 ([#18670](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18670)) @ZiyinLin
* 协议文档新增黑流树海刷开局回调 BlackFlowStartRun 的字段说明，补充集成接口模式 30003 及相关参数说明 ([#18696](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18696)) @ZiyinLin
* 补齐协议文档 en-us/ja-jp/ko-kr 缺失的段落与字段说明，涉及回调协议、集成文档 Fight 字段与关卡清单、任务流程协议特殊任务类型一节 @ABA2396
* 修复文档中 33 处失效的站内锚点链接（目标小节更名 10 处、Unicode 规范化差异 23 处） ([#18620](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18620) [#18629](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18629)) @chanshengbinying
* 修复集成文档中列表后提示容器未正确闭合的显示问题，五语言同步 @ABA2396

### 其他 | Other

* CI 对 PR 标题自动补齐中英文间空格 ([#18657](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18657)) @satgo1546
* 修复 clang-formatter 忽略目录时误伤同前缀目录的问题 ([#18613](https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/18613)) @FanWu-ai
* 统一调试截图与录像导出文件的落盘位置到用户目录 @ABA2396
* GenericCombinedData 与 LocalizedObservableList 支持条目级悬浮提示并随语言热切换刷新，黑流树海刷开局的动物与奖励选项列表迁移至该组件 @ABA2396

</details>
