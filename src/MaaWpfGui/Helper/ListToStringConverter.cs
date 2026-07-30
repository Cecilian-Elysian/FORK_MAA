// <copyright file="ListToStringConverter.cs" company="MaaAssistantArknights">
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
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace MaaWpfGui.Helper;

/// <summary>
/// 把 IEnumerable 用分隔符连接成字符串（feat/recruit-history-tab 用）
/// </summary>
public class ListToStringConverter : IValueConverter
{
    public static ListToStringConverter Default { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable enumerable) return string.Empty;
        var sep = parameter as string ?? ", ";
        var parts = new System.Collections.Generic.List<string>();
        foreach (var item in enumerable)
        {
            if (item != null) parts.Add(item.ToString() ?? string.Empty);
        }
        return string.Join(sep, parts);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}