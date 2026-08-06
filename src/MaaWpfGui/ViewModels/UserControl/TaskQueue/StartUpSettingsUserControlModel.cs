// <copyright file="StartUpSettingsUserControlModel.cs" company="MaaAssistantArknights">
// Part of the MaaWpfGui project, maintained by the MaaAssistantArknights team (Maa Team)
// Copyright (C) 2021-2025 MaaAssistantArknights Contributors
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

public class StartUpSettingsUserControlModel : TaskSettingsViewModel, StartUpSettingsUserControlModel.ISerialize
{
    static StartUpSettingsUserControlModel()
    {
        Instance = new();
        Instances.AsstProxy.AsstSubTaskMsgEvent += Instance.ProcSubTaskMsg;
    }

    public static StartUpSettingsUserControlModel Instance { get; }

    /// <summary>
    /// fix/account-cycle-config-source: 直达配置源, 不依赖 TaskSettingVisibilityInfo.CurrentIndex.
    /// 替代 <see cref="GetTaskConfig{T}"/> 在焦点离开 StartUp 任务时返回默认空实例导致的潜伏 bug
    /// (切走基建/集成战略再切回一键长草时轮换列表被清空, RebuildCycleSteps 生成 0 步骤, 静默跳过).
    /// </summary>
    private static StartUpTask? CycleConfig =>
        ConfigFactory.CurrentConfig.TaskQueue.OfType<StartUpTask>().FirstOrDefault();

    #region Account Switch (Single)

    public string AccountName
    {
        get => CycleConfig?.AccountName ?? string.Empty;
        set
        {
            if (CycleConfig == null)
            {
                return;
            }

            CycleConfig.AccountName = value.Trim();
            NotifyOfPropertyChange();
        }
    }

    public bool AccountSwitchEnabled
    {
        get => CycleConfig?.AccountSwitchEnabled ?? false;
        set
        {
            if (CycleConfig == null)
            {
                return;
            }

            CycleConfig.AccountSwitchEnabled = value;
            NotifyOfPropertyChange();
        }
    }

    [UsedImplicitly]
    public async void AccountSwitchManualRun()
    {
        if (AccountCycleEnabled)
        {
            var next = GetCurrentCycleAccount();
            if (next == null)
            {
                Instances.TaskQueueViewModel.AddLog(LocalizationHelper.GetString("AccountCycleAllDone"), UiLogColor.Info);
                return;
            }

            var task = new StartUpTask() { AccountSwitchEnabled = true, AccountName = next };
            await Instances.TaskQueueViewModel.LinkStartWithTasks([task]);
            return;
        }

        if (TaskSettingVisibilityInfo.CurrentTask is not StartUpTask startUp)
        {
            Instances.TaskQueueViewModel.AddLog("Current task is not StartUpTask", UiLogColor.Error);
            return;
        }
var singleTask = new StartUpTask() { AccountSwitchEnabled = true, AccountName = startUp.AccountName };
        await Instances.TaskQueueViewModel.LinkStartWithTasks([singleTask]);
    }

    #endregion

    #region Account Cycle

    private readonly ObservableCollection<AccountCycleItem> _accountCycleItems = [];

    /// <summary>
    /// feat/account-cycle-refactor: 委托给 <see cref="AccountCycleOrchestrator.Instance"/> 管理
    /// 步骤列表 / 完成集合 / IsCycling 等轮换状态. 本类仅负责 UI 项列表 (<c>_accountCycleItems</c>)
    /// 与勾选/编辑交互.
    /// </summary>
    private static AccountCycleOrchestrator Orchestrator => AccountCycleOrchestrator.Instance;

    public bool AccountCycleEnabled
    {
        get => CycleConfig?.AccountCycleEnabled ?? true;
        set
        {
            if (CycleConfig == null)
            {
                return;
            }

            CycleConfig.AccountCycleEnabled = value;
            if (!value)
            {
                ResetCycle();
            }

            NotifyOfPropertyChange();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 是否将肉鸽 (Roguelike) 与生息演算 (Reclamation) 任务延后到所有账号的基础任务完成后执行。
    /// 仅在 <see cref="AccountCycleEnabled"/> 为 true 时生效。
    /// </summary>
    public bool LateStageRogueAndReclamation
    {
        get => CycleConfig?.LateStageRogueAndReclamation ?? false;
        set
        {
            if (CycleConfig == null)
            {
                return;
            }

            CycleConfig.LateStageRogueAndReclamation = value;
            NotifyOfPropertyChange();
        }
    }

    public bool ShowEditSection
    {
        get => _showEditSection;
        set
        {
            SetAndNotify(ref _showEditSection, value);
            if (!value)
            {
                AccountCycleMode = 0;
            }
        }
    }

    private bool _showEditSection;

    public int AccountCycleMode
    {
        get => _accountCycleMode;
        set
        {
            SetAndNotify(ref _accountCycleMode, value);
            NotifyOfPropertyChange(nameof(ShowAddMode));
            NotifyOfPropertyChange(nameof(ShowDeleteMode));
        }
    }

    private int _accountCycleMode;

    public bool ShowAddMode => AccountCycleMode == 1;

    public bool ShowDeleteMode => AccountCycleMode == 2;

    public ObservableCollection<AccountCycleItem> AccountCycleItems => _accountCycleItems;

    public void InitAccountCycleItems()
    {
        SyncAccountNamesToItems();
    }

    /// <summary>
    /// feat/account-cycle-refactor: 委托到 Orchestrator.
    /// </summary>
    public bool IsCycling
    {
        get => Orchestrator.IsCycling;
        set => Orchestrator.IsCycling = value;
    }

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public int CurrentStepCount => Orchestrator.CurrentStepCount;

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public int CurrentStepIndex => Orchestrator.CurrentStepIndex;

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public AccountCycleStep? CurrentStep => Orchestrator.CurrentStep;

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public AccountCycleStep? GetPreviousStep() => Orchestrator.GetPreviousStep();

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public int CurrentPhase => Orchestrator.CurrentPhase;

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public void RebuildCycleSteps() => Orchestrator.RebuildCycleSteps(
        _accountCycleItems
            .Where(x => x.IsSelected && !string.IsNullOrEmpty(x.AccountName))
            .OrderBy(x => x.Index)
            .Select(x => x.AccountName),
        LateStageRogueAndReclamation
            && ConfigFactory.CurrentConfig.TaskQueue.Any(t =>
                IsTaskEnable(t) &&
                (t.TaskType == TaskType.Roguelike || t.TaskType == TaskType.Reclamation)));

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public void AdvanceStepIndex() => Orchestrator.AdvanceStepIndex();

    public void SyncAccountNamesToItems()
    {
        var config = CycleConfig;
        if (config == null)
        {
            // fix/account-cycle-config-source: 无 StartUp 任务时清空轮换列表 (避免 UI 残留)
            _accountCycleItems.Clear();
            NotifyOfPropertyChange(nameof(AccountCycleItems));
            return;
        }

        // fix/trim-account-name: 迁移清理历史脏数据 (账号名尾随空格/制表符/换行)
        // 清理后对首个受影响账号打 INFO 日志便于用户感知, 后续步骤不再感知
        // (MaaCore set_account 也已 Trim, 此处是配置层根治, 让 UI 也立即显示干净账号名)
        bool trimmedFirstAccount = false;
        for (int i = 0; i < config.AccountNames.Count; i++)
        {
            var original = config.AccountNames[i];
            var trimmed = original?.Trim();
            if (trimmed != original)
            {
                if (!trimmedFirstAccount && !string.IsNullOrEmpty(trimmed))
                {
                    Instances.TaskQueueViewModel.AddLog(
                        $"[fix/trim-account-name] AccountNames[{i}] 已去除首尾空白: \"{original}\" → \"{trimmed}\"",
                        UiLogColor.Info);
                    trimmedFirstAccount = true;
                }
                config.AccountNames[i] = trimmed ?? string.Empty;
            }
        }

        if (!string.IsNullOrEmpty(config.AccountName))
        {
            var originalAcctName = config.AccountName;
            var trimmedAcctName = originalAcctName.Trim();
            if (trimmedAcctName != originalAcctName)
            {
                Instances.TaskQueueViewModel.AddLog(
                    $"[fix/trim-account-name] AccountName 已去除首尾空白: \"{originalAcctName}\" → \"{trimmedAcctName}\"",
                    UiLogColor.Info);
                config.AccountName = trimmedAcctName;
            }
        }

        // 从单账号切换复制账号名到轮换列表第一项
        if (config.AccountNames.Count > 0 && string.IsNullOrEmpty(config.AccountNames[0]) && !string.IsNullOrEmpty(config.AccountName))
        {
            config.AccountNames[0] = config.AccountName;
        }

        if (config.AccountNames.Count == 0 && !string.IsNullOrEmpty(config.AccountName))
        {
            config.AccountNames.Add(config.AccountName);
            config.AccountNames.Add(string.Empty);
        }

        var existingSelections = _accountCycleItems
            .Where(x => !string.IsNullOrEmpty(x.AccountName))
            .ToDictionary(x => x.AccountName, x => x.IsSelected);

        _accountCycleItems.Clear();

        // fix/account-cycle-fault-tolerance (C4): 重名校验, 保留首次出现, 后续同名取消勾选并提示
        var seenNames = new HashSet<string>(System.StringComparer.Ordinal);
        int duplicateCount = 0;
        for (int i = 0; i < config.AccountNames.Count; i++)
        {
            var name = config.AccountNames[i];
            bool isDuplicate = !string.IsNullOrEmpty(name) && !seenNames.Add(name);
            if (isDuplicate)
            {
                duplicateCount++;
            }

            var item = new AccountCycleItem
            {
                DisplayName = LocalizationHelper.GetString("AccountCycleNewAccountDefaultName") + (i + 1),
                AccountName = name,
                IsSelected = !isDuplicate && (existingSelections.TryGetValue(name, out var selected) ? selected : true),
                IsCompleted = Orchestrator.IsAccountCompleted(name),
                Index = i,
            };
            item.PropertyChanged += OnAccountCycleItemPropertyChanged;
            _accountCycleItems.Add(item);
        }

        if (duplicateCount > 0)
        {
            Instances.TaskQueueViewModel.AddLog($"[Cycle] Warning: {duplicateCount} duplicate account name(s) detected, duplicates have been deselected.", UiLogColor.Warning);
        }

        NotifyOfPropertyChange(nameof(AccountCycleItems));
    }

    private void OnAccountCycleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AccountCycleItem item || e.PropertyName != nameof(AccountCycleItem.AccountName))
        {
            return;
        }

        var config = CycleConfig;
        if (config != null && item.Index < config.AccountNames.Count)
        {
            config.AccountNames[item.Index] = item.AccountName;
            NotifyOfPropertyChange(nameof(AccountCycleItems));
        }
    }

    [UsedImplicitly]
    public void AddAccountAfter(AccountCycleItem currentItem)
    {
        var config = CycleConfig;
        if (config == null)
        {
            return;
        }

        int insertIndex = currentItem?.Index + 1 ?? config.AccountNames.Count;

        config.AccountNames.Insert(insertIndex, string.Empty);

        var newItem = new AccountCycleItem
        {
            DisplayName = LocalizationHelper.GetString("AccountCycleNewAccountDefaultName") + (insertIndex + 1),
            AccountName = string.Empty,
            IsSelected = true,
            IsCompleted = false,
            Index = insertIndex,
        };
        newItem.PropertyChanged += OnAccountCycleItemPropertyChanged;
        _accountCycleItems.Insert(insertIndex, newItem);

        RebuildIndexes();
        NotifyOfPropertyChange(nameof(AccountCycleItems));
    }

    [UsedImplicitly]
    public void RemoveAccount(AccountCycleItem item)
    {
        var config = CycleConfig;
        if (config == null)
        {
            return;
        }

        if (item.Index < config.AccountNames.Count)
        {
            config.AccountNames.RemoveAt(item.Index);
        }

        item.PropertyChanged -= OnAccountCycleItemPropertyChanged;
        _accountCycleItems.Remove(item);

        RebuildIndexes();
        NotifyOfPropertyChange(nameof(AccountCycleItems));
    }

    private void RebuildIndexes()
    {
        for (int i = 0; i < _accountCycleItems.Count; i++)
        {
            _accountCycleItems[i].Index = i;
            _accountCycleItems[i].DisplayName = LocalizationHelper.GetString("AccountCycleNewAccountDefaultName") + (i + 1);
        }
    }

    public string? GetCurrentCycleAccount()
    {
        return _accountCycleItems
            .Where(x => x.IsSelected && !x.IsCompleted && !string.IsNullOrEmpty(x.AccountName))
            .OrderBy(x => x.Index)
            .FirstOrDefault()?.AccountName;
    }

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator, 同时更新 UI 项.</summary>
    public void MarkAccountCompleted(string accountName)
    {
        if (string.IsNullOrEmpty(accountName))
        {
            return;
        }

        Orchestrator.MarkAccountCompleted(accountName);
        var item = _accountCycleItems.FirstOrDefault(x => x.AccountName == accountName);
        if (item != null)
        {
            item.IsCompleted = true;
        }
    }

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator.</summary>
    public void ResetCycle() => Orchestrator.ResetCycle();

    /// <summary>feat/account-cycle-refactor: 委托到 Orchestrator + 重置 UI 项.</summary>
    public void ClearCompletedAccounts()
    {
        Orchestrator.ClearCompletedAccounts();
        foreach (var item in _accountCycleItems)
        {
            item.IsCompleted = false;
        }
    }

    #endregion

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
                clientType is ClientType.Official or ClientType.Bilibili or ClientType.Txwy &&
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
