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
using MaaWpfGui.Configuration.Single.MaaTask;
using MaaWpfGui.Constants;
using MaaWpfGui.Constants.Enums;
using MaaWpfGui.Helper;
using MaaWpfGui.Main;
using MaaWpfGui.Models;
using MaaWpfGui.Models.AsstTasks;
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

    #region Account Switch (Single)

    public string AccountName
    {
        get => GetTaskConfig<StartUpTask>().AccountName;
        set {
            value = value.Trim();
            SetTaskConfig<StartUpTask>(t => t.AccountName == value, t => t.AccountName = value);
        }
    }

    public bool AccountSwitchEnabled
    {
        get => GetTaskConfig<StartUpTask>().AccountSwitchEnabled ?? false;
        set => SetTaskConfig<StartUpTask>(t => t.AccountSwitchEnabled == value, t => t.AccountSwitchEnabled = value);
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
    private readonly HashSet<string> _completedAccounts = [];
    private bool _isCycling;

    public bool AccountCycleEnabled
    {
        get => GetTaskConfig<StartUpTask>().AccountCycleEnabled;
        set
        {
            SetTaskConfig<StartUpTask>(t => t.AccountCycleEnabled == value, t => t.AccountCycleEnabled = value);
            if (!value)
            {
                ResetCycle();
            }

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

    public void SyncAccountNamesToItems()
    {
        var config = GetTaskConfig<StartUpTask>();

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

        _accountCycleItems.Clear();
        for (int i = 0; i < config.AccountNames.Count; i++)
        {
            var item = new AccountCycleItem
            {
                DisplayName = LocalizationHelper.GetString("AccountCycleNewAccountDefaultName") + (i + 1),
                AccountName = config.AccountNames[i],
                IsSelected = true,
                IsCompleted = _completedAccounts.Contains(config.AccountNames[i]),
                Index = i,
            };
            item.PropertyChanged += OnAccountCycleItemPropertyChanged;
            _accountCycleItems.Add(item);
        }

        NotifyOfPropertyChange(nameof(AccountCycleItems));
    }

    private void OnAccountCycleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AccountCycleItem item || e.PropertyName != nameof(AccountCycleItem.AccountName))
        {
            return;
        }

        var config = GetTaskConfig<StartUpTask>();
        if (item.Index < config.AccountNames.Count)
        {
            config.AccountNames[item.Index] = item.AccountName;
            SetTaskConfig<StartUpTask>(_ => false, _ => { });
        }
    }

    [UsedImplicitly]
    public void AddAccountAfter(AccountCycleItem currentItem)
    {
        var config = GetTaskConfig<StartUpTask>();
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
        SetTaskConfig<StartUpTask>(_ => false, _ => { });
    }

    [UsedImplicitly]
    public void RemoveAccount(AccountCycleItem item)
    {
        var config = GetTaskConfig<StartUpTask>();

        if (item.Index < config.AccountNames.Count)
        {
            config.AccountNames.RemoveAt(item.Index);
        }

        _completedAccounts.Remove(item.AccountName);
        item.PropertyChanged -= OnAccountCycleItemPropertyChanged;
        _accountCycleItems.Remove(item);

        RebuildIndexes();
        SetTaskConfig<StartUpTask>(_ => false, _ => { });
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

    public void MarkAccountCompleted(string accountName)
    {
        if (string.IsNullOrEmpty(accountName))
        {
            return;
        }

        _completedAccounts.Add(accountName);
        var item = _accountCycleItems.FirstOrDefault(x => x.AccountName == accountName);
        if (item != null)
        {
            item.IsCompleted = true;
        }
    }

    public void ResetCycle()
    {
        _isCycling = false;
        ClearCompletedAccounts();
    }

    public void ClearCompletedAccounts()
    {
        _completedAccounts.Clear();
        foreach (var item in _accountCycleItems)
        {
            item.IsCompleted = false;
        }
    }

    public bool IsCycling
    {
        get => _isCycling;
        set => SetAndNotify(ref _isCycling, value);
    }

    #endregion

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
            var accountName = !SettingsViewModel.ConnectSettings.UseAttachWindow &&
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
