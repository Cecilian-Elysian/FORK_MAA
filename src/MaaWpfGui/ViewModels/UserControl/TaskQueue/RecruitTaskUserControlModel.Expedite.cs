// <copyright file="RecruitTaskUserControlModel.Expedite.cs" company="MaaAssistantArknights">
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
using MaaWpfGui.Configuration.Single.MaaTask;
using MaaWpfGui.Utilities.ValueType;

namespace MaaWpfGui.ViewModels.UserControl.TaskQueue;

/// <summary>
/// feat/expedite-threshold partial class.
/// 加急门槛 UI 交互 (<see cref="ExpediteMode"/> 属性 + <see cref="ExpediteModeList"/> 下拉) 下沉,
/// 主文件仅保留 RefreshLocalization 中的一行 RefreshLocalization() 钩子调用。
/// 减少与 upstream/master-v2 合并时主文件的冲突面 (下游 [HOT] 详见 docs/downstream-changes.md)。
/// </summary>
public partial class RecruitTaskUserControlModel
{
    /// <summary>
    /// Gets or sets 加急招募模式（合并总开关与星级门槛）：
    /// 0 = 不使用加急；1 = 所有星级均加急；4/5/6 = 仅对应星级及以上使用加急
    /// </summary>
    public int ExpediteMode
    {
        get
        {
            var config = GetTaskConfig<RecruitTask>();
            if (config.UseExpedited is false)
            {
                return 0;
            }

            return config.ExpediteMinLevel == 0 ? 1 : config.ExpediteMinLevel;
        }
        set
        {
            var expedited = value != 0;
            bool? newUseExpedited = expedited;
            var newMinLevel = value switch
            {
                0 => 0,
                1 => 0,
                int v => v,
            };

            SetTaskConfig<RecruitTask>(
                t => t.UseExpedited == newUseExpedited && t.ExpediteMinLevel == newMinLevel,
                t =>
                {
                    t.UseExpedited = newUseExpedited;
                    t.ExpediteMinLevel = newMinLevel;
                });
            NotifyOfPropertyChange(nameof(ExpediteMode));
        }
    }

    /// <summary>
    /// Gets 加急招募模式下拉框可选项。
    /// </summary>
    public LocalizedObservableList<int> ExpediteModeList { get; } = new(
        (0, "ExpediteModeDisabled"),
        (1, "ExpediteModeAll"),
        (4, "ExpediteMode4"),
        (5, "ExpediteMode5"),
        (6, "ExpediteMode6"));

    /// <summary>
    /// 上游 v6.20.0 在 TaskQueueViewModel.ResetAllTemporaryVariable（右键半选还原临时变量）中
    /// 新增了对本方法的调用，故在此按上游语义补齐。
    /// 上游实现为 <c>recruit?.UseExpedited ??= false</c>（仅把 null 归一为 false，不改写已显式设定的值）。
    /// fork 用 <see cref="RecruitTask.ExpediteMode"/> 表达加急开关，故此处只归一底层
    /// <c>UseExpedited</c> 的 null 值，保留 fork 独有的 <c>ExpediteMinLevel</c> 门槛设置。
    /// </summary>
    public static void ResetRecruitVariables(RecruitTask? recruit)
    {
        if (recruit is null)
        {
            return;
        }

        if (recruit.UseExpedited is null)
        {
            recruit.UseExpedited = false;
        }
    }
}