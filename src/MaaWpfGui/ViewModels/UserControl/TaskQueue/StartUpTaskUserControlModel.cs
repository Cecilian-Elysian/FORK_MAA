// <copyright file="StartUpTaskUserControlModel.cs" company="MaaAssistantArknights">
// Part of the MaaWpfGui project, maintained by the MaaAssistantArknights team (Maa Team)
// Copyright (C) 2021-2026 MaaAssistantArknights Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License v3.0 only as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
// </copyright>

#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;
using MaaWpfGui.Configuration.Factory;
using MaaWpfGui.Configuration.Single.MaaTask;
using MaaWpfGui.Constants;
using MaaWpfGui.Constants.Enums;
using MaaWpfGui.Helper;
using MaaWpfGui.Main;
using MaaWpfGui.Models;
using MaaWpfGui.Models.AsstTasks;
using MaaWpfGui.ViewModels.Orchestration;
using MaaWpfGui.ViewModels.UI;
using Stylet;
using static MaaWpfGui.Main.AsstProxy;

namespace MaaWpfGui.ViewModels.UserControl.TaskQueue;

public partial class StartUpTaskUserControlModel : TaskSettingsViewModel, StartUpTaskUserControlModel.ISerialize
{
    static StartUpTaskUserControlModel()
    {
        Instance = new();
        Instances.AsstProxy.AsstSubTaskMsgEvent += Instance.ProcSubTaskMsg;
    }

    public static StartUpTaskUserControlModel Instance { get; }

    /// <summary>
    /// fix/account-cycle-config-source: 直达配置源，不依赖 TaskSettingVisibilityInfo.CurrentIndex。
    /// 替代 <see cref="GetTaskConfig{T}"/> 在焦点离开 StartUp 任务时返回默认空实例导致的作废 bug
    /// （切换基建/集成或略再切回同一长跑耗时，替换列表被清空，RebuildCycleSteps 生成 0 步骤，静默跳过）。
    /// </summary>
    private static StartUpTask? CycleConfig =>
        ConfigFactory.CurrentConfig.TaskQueue.OfType<StartUpTask>().FirstOrDefault();

    // feat/account_rotation + feat/account-cycle-refactor + fix/account-rotation-supersede-switcher:
    // 全部逻辑下沉到 partial class StartUpTaskUserControlModel.AccountCycle.cs,
    // 本文件保留 CycleConfig 直达源 + InitAccountCycleItems() 钩子调用 + RefreshUI/SerializeTask/ProcSubTaskMsg 三个上游原生职责。
    // 减少与 upstream/master-v2 合并时本文件的冲突面 (下游 [HOT] 详见 docs/downstream-changes.md)。

    private static bool IsTaskEnable(BaseTask t) => TaskQueueViewModel.IsTaskEnable(t);

    public void ProcSubTaskMsg(AsstMsg msg, AsstSubTaskMsg? details)
    {
        if (msg == AsstMsg.SubTaskExtraInfo && details?.What == "AccountSwitch")
        {
            Instances.TaskQueueViewModel.AddLog(LocalizationHelper.GetString("AccountSwitch") + $" -->> {details?.Details?["account_name"]}", UiLogColor.Info);
        }
    }

    public override void RefreshUI(BaseTask baseTask)
    {
        if (baseTask is StartUpTask)
        {
            InitAccountCycleItems();
        }
    }

    public override (bool? IsSuccess, IEnumerable<int> TaskId) SerializeTask(BaseTask? baseTask, int? taskId = null) => (this as ISerialize).Serialize(baseTask, taskId);

    private interface ISerialize : ITaskQueueModelSerialize
    {
        (bool? IsSuccess, IEnumerable<int> TaskId) ITaskQueueModelSerialize.Serialize(BaseTask? baseTask, int? taskId)
        {
            if (baseTask is not StartUpTask startUp)
            {
                return (null, []);
            }

            var clientType = SettingsViewModel.GameSettings.ClientType;
            var accountName = !SettingsViewModel.ConnectSettings.IsPCConnectConfig &&
                clientType is ClientType.Official or ClientType.Bilibili or ClientType.Txwy or ClientType.KR &&
                startUp.AccountSwitchEnabled is true
                    ? startUp.AccountName
                    : string.Empty;

            var task = new AsstStartUpTask() {
                ClientType = clientType,
                StartGame = SettingsViewModel.GameSettings.StartGame,
                AccountName = accountName,
            };

            return taskId switch {
                int id when id > 0 => (Instances.AsstProxy.AsstSetTaskParamsEncoded(id, task), [id]),
                null => FromSingle(Instances.AsstProxy.AsstAppendTaskWithEncoding(TaskType.StartUp, task)),
                _ => (null, []),
            };
        }
    }
}
