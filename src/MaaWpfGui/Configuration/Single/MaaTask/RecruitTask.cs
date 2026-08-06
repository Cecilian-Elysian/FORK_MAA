// <copyright file="RecruitTask.cs" company="MaaAssistantArknights">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MaaWpfGui.Helper;
using MaaWpfGui.ViewModels.UserControl.TaskQueue;
using static MaaWpfGui.Main.AsstProxy;

namespace MaaWpfGui.Configuration.Single.MaaTask;

/// <summary>
/// 公招
/// </summary>
public class RecruitTask : BaseTask, IJsonOnDeserialized
{
    public RecruitTask() => TaskType = TaskType.Recruit;

    /// <summary>
    /// Gets or sets a value indicating whether 是否使用公招加速卷
    /// </summary>
    public bool? UseExpedited { get; set; } = false;

    /// <summary>
    /// Gets or sets 加急招募门槛：仅当组合最低星级 ≥ 此值时使用加急许可
    /// 0 = 所有星级均加急（默认，向后兼容）；4 / 5 / 6 = 对应星级阈值
    /// </summary>
    public int ExpediteMinLevel { get; set; } = 0;

    /// <summary>
    /// Gets or sets 加急招募模式（合并总开关与星级门槛）由 ViewModel 维护：
    /// 0 = 不使用加急；1 = 所有星级均加急；4 / 5 / 6 = 仅对应星级及以上使用加急
    /// </summary>
    [JsonIgnore]
    public int ExpediteMode
    {
        get {
            if (UseExpedited is false) {
                return 0;
            }
            return ExpediteMinLevel == 0 ? 1 : ExpediteMinLevel;
        }
        set {
            bool expedited = value != 0;
            UseExpedited = expedited;
            ExpediteMinLevel = value switch {
                0 => 0,
                1 => 0,
                int v => v,
            };
        }
    }

    /// <summary>
    /// Gets or sets 单轮最大公招次数
    /// </summary>
    public int MaxTimes { get; set; } = 4;

    /// <summary>
    /// Gets or sets 多Tag策略
    /// TODO 改枚举？
    /// </summary>
    public int ExtraTagMode { get; set; }

    /// <summary>
    /// Gets or sets 3星首选Tag
    /// </summary>
    public List<string> Level3PreferTags { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether 是否启用3星Tag倾向。
    /// </summary>
    public bool PreferTagEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets 需要保留并跳过的Tag。
    /// </summary>
    public List<string> PreserveTagList { get; set; } = [RecruitSettingsUserControlModel.LegacyRobotTag];

    /// <summary>
    /// Gets or sets a value indicating whether 是否启用保留指定Tag。
    /// </summary>
    public bool PreserveTagEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 无倾向Tag时是否刷新3星
    /// TODO 重命名?
    /// </summary>
    public bool RefreshLevel3 { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether 无招聘许可仍然刷新
    /// </summary>
    public bool ForceRefresh { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether 自动确认3星
    /// </summary>
    public bool Level3Choose { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether 自动确认4星
    /// </summary>
    public bool Level4Choose { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether 自动确认5星
    /// 招募时间固定为 9:00（540 分钟）。
    /// </summary>
    public bool Level5Choose { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 自动确认6星。
    /// 仅作为配置文件可修改项，界面不可修改，招募时间固定为 9:00（540 分钟）。
    /// </summary>
    public bool Level6Choose { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 3星时间
    /// </summary>
    public int Level3Time { get; set; } = 540;

    /// <summary>
    /// Gets or sets 4星时间
    /// </summary>
    public int Level4Time { get; set; } = 540;

    public void OnDeserialized()
    {
        var normalizedPreferTags = RecruitTagHelper.NormalizeTagList(Level3PreferTags);
        if (!Level3PreferTags.SequenceEqual(normalizedPreferTags, StringComparer.Ordinal))
        {
            Level3PreferTags = normalizedPreferTags;
        }

        var normalizedPreserveTags = RecruitTagHelper.NormalizeTagList(PreserveTagList);
        if (!PreserveTagList.SequenceEqual(normalizedPreserveTags, StringComparer.Ordinal))
        {
            PreserveTagList = normalizedPreserveTags;
        }
    }
}
