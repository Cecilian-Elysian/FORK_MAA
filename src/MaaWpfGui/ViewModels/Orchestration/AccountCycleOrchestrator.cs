// <copyright file="AccountCycleOrchestrator.cs" company="MaaAssistantArknights">
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
using System.Linq;
using MaaWpfGui.Helper;
using MaaWpfGui.Models;
using Stylet;

namespace MaaWpfGui.ViewModels.Orchestration;

/// <summary>
/// feat/account-cycle-refactor: 账号轮换调度状态机 (单一职责).
/// 从 <see cref="StartUpSettingsUserControlModel"/> 抽出:
///   - 扁平步骤列表 <c>_cycleSteps</c> / <c>_currentStepIndex</c>
///   - 完成集合 <c>_completedAccounts</c>
///   - RebuildCycleSteps / AdvanceStepIndex / MarkAccountCompleted / ResetCycle / ClearCompletedAccounts
///   - 当前步骤访问器 CurrentStep / GetPreviousStep / CurrentStepCount / CurrentStepIndex / CurrentPhase
/// <see cref="TaskQueueViewModel.AdvanceAccountCycle"/> 调用 RebuildCycleSteps 重建步骤,
/// 然后读 CurrentStep / GetPreviousStep 推进; LinkStart 触发 ResetCycle 清空状态.
/// </summary>
public class AccountCycleOrchestrator : PropertyChangedBase
{
    static AccountCycleOrchestrator()
    {
        Instance = new();
    }

    public static AccountCycleOrchestrator Instance { get; }

    private readonly List<AccountCycleStep> _cycleSteps = [];
    private int _currentStepIndex = -1;
    private readonly HashSet<string> _completedAccounts = [];

    private bool _isCycling;

    public bool IsCycling
    {
        get => _isCycling;
        set => SetAndNotify(ref _isCycling, value);
    }

    /// <summary>
    /// 当前步骤总数 (<c>RebuildCycleSteps</c> 后有效).
    /// </summary>
    public int CurrentStepCount => _cycleSteps.Count;

    /// <summary>
    /// 当前正在执行或即将执行的步骤的索引. -1 表示尚未开始.
    /// </summary>
    public int CurrentStepIndex => _currentStepIndex;

    /// <summary>
    /// 当前正在执行或即将执行的步骤. null 表示尚未开始.
    /// </summary>
    public AccountCycleStep? CurrentStep =>
        _currentStepIndex >= 0 && _currentStepIndex < _cycleSteps.Count
            ? _cycleSteps[_currentStepIndex]
            : null;

    /// <summary>
    /// 上一个步骤, 用于判断是否需要跨账号切号. 无返回 null.
    /// </summary>
    public AccountCycleStep? GetPreviousStep() =>
        _currentStepIndex - 1 >= 0 ? _cycleSteps[_currentStepIndex - 1] : null;

    /// <summary>
    /// 当前步骤的 Phase (1 = 基础任务, 2 = 收尾任务). 未启动时返回 1.
    /// </summary>
    public int CurrentPhase => CurrentStep?.Phase ?? 1;

    /// <summary>
    /// 根据当前勾选账号与 <paramref name="hasLateStageTasks"/> 重新构建扁平步骤列表.
    /// Phase 1 = 所有账号的基础任务 (1 个 step/账号);
    /// Phase 2 = 当开关开启且至少存在 1 个肉鸽/生息任务时, 每个账号各加 1 个 step.
    /// <paramref name="hasLateStageTasks"/> 由调用方预先判断 (避免 Orchestrator 引用 ConfigFactory/TaskType).
    /// </summary>
    public void RebuildCycleSteps(IEnumerable<string> selectedAccounts, bool hasLateStageTasks)
    {
        _cycleSteps.Clear();

        // Phase 1: 每个账号各 1 个 step
        foreach (var acc in selectedAccounts)
        {
            _cycleSteps.Add(new AccountCycleStep(acc, 1));
        }

        // Phase 2: 仅当开关开启 + 至少存在 1 个肉鸽/生息任务时, 每个账号各加 1 个 step
        if (hasLateStageTasks)
        {
            foreach (var acc in selectedAccounts)
            {
                _cycleSteps.Add(new AccountCycleStep(acc, 2));
            }
        }

        _currentStepIndex = _cycleSteps.Count > 0 ? 0 : -1;
    }

    /// <summary>
    /// 将步骤索引推进 1, 准备下一轮任务的提交.
    /// </summary>
    public void AdvanceStepIndex()
    {
        if (_currentStepIndex >= 0)
        {
            _currentStepIndex++;
        }
    }

    /// <summary>
    /// 标记指定账号为已完成.
    /// </summary>
    public void MarkAccountCompleted(string accountName)
    {
        if (string.IsNullOrEmpty(accountName))
        {
            return;
        }

        _completedAccounts.Add(accountName);
    }

    /// <summary>
    /// 检查指定账号是否已完成.
    /// </summary>
    public bool IsAccountCompleted(string accountName)
    {
        return !string.IsNullOrEmpty(accountName) && _completedAccounts.Contains(accountName);
    }

    /// <summary>
    /// 重置轮换状态 (IsCycling=false + 步骤清空 + 完成集合清空 + 索引重置).
    /// </summary>
    public void ResetCycle()
    {
        _isCycling = false;
        _cycleSteps.Clear();
        _currentStepIndex = -1;
        ClearCompletedAccounts();
    }

    /// <summary>
    /// 清空完成集合, 不重置 IsCycling 与步骤.
    /// </summary>
    public void ClearCompletedAccounts()
    {
        _completedAccounts.Clear();
    }
}