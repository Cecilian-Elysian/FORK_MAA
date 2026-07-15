// <copyright file="AccountCycleStep.cs" company="MaaAssistantArknights">
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
namespace MaaWpfGui.Models;

/// <summary>
/// 单个账号轮换步骤,用于 feat/defer-rogue 跨账号轮转 Phase 调度。
/// Phase 1 = 基础任务(除肉鸽、生息演算外的所有任务);
/// Phase 2 = 收尾任务(仅肉鸽、生息演算)。
/// </summary>
public record AccountCycleStep(string AccountName, int Phase);
