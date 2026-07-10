// <copyright file="AccountCycleItem.cs" company="MaaAssistantArknights">
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
using Stylet;

namespace MaaWpfGui.Models;

public class AccountCycleItem : PropertyChangedBase
{
    private string _displayName = string.Empty;

    public string DisplayName
    {
        get => _displayName;
        set => SetAndNotify(ref _displayName, value);
    }

    private string _accountName = string.Empty;

    public string AccountName
    {
        get => _accountName;
        set => SetAndNotify(ref _accountName, value);
    }

    private bool _isSelected = true;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndNotify(ref _isSelected, value);
    }

    private bool _isCompleted;

    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetAndNotify(ref _isCompleted, value);
    }

    private int _index;

    public int Index
    {
        get => _index;
        set => SetAndNotify(ref _index, value);
    }
}
