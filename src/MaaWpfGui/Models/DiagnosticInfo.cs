// <copyright file="DiagnosticInfo.cs" company="MaaAssistantArknights">
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

using System;
using System.Runtime.InteropServices;
using MaaWpfGui.Helper;
using MaaWpfGui.ViewModels.UI;
using MaaWpfGui.ViewModels.UserControl.Settings;
using MaaWpfGui.WineCompat;

namespace MaaWpfGui.Models;

public class DiagnosticInfo
{
    public string ExportTime { get; set; } = DateTimeOffset.Now.ToString("O");

    public DateFilterInfo DateFilter { get; set; } = new();

    public AppInfo App { get; set; } = new();

    public SysInfo SysInfo { get; set; } = new();

    public GpuInfo Gpu { get; set; } = new();

    public static DiagnosticInfo Collect(DateTime fromDate, DateTime toDate)
    {
        var info = new DiagnosticInfo
        {
            DateFilter = new DateFilterInfo
            {
                From = fromDate.ToString("yyyy-MM-dd"),
                To = toDate.ToString("yyyy-MM-dd"),
            },
            App = CollectAppInfo(),
            SysInfo = CollectSysInfo(),
            Gpu = CollectGpuInfo(),
        };
        return info;
    }

    private static AppInfo CollectAppInfo()
    {
        return new AppInfo
        {
            UiVersion = VersionUpdateSettingsUserControlModel.UiVersion ?? "unknown",
            CoreVersion = VersionUpdateSettingsUserControlModel.CoreVersion ?? "unknown",
            ResourceVersion = SettingsViewModel.VersionUpdateSettings?.ResourceVersion ?? "unknown",
            BuildTime = VersionUpdateSettingsUserControlModel.BuildDateTimeCurrentCultureString ?? "unknown",
        };
    }

    private static SysInfo CollectSysInfo()
    {
        return new SysInfo
        {
            Os = RuntimeInformation.OSDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            Framework = RuntimeInformation.FrameworkDescription,
            IsAdmin = IsAdministrator(),
            IsWine = WineRuntimeInformation.IsRunningUnderWine,
            WineVersion = WineRuntimeInformation.WineVersion,
        };
    }

    private static GpuInfo CollectGpuInfo()
    {
        if (GpuOption.GetCurrent() is GpuOption.EnableOption opt)
        {
            var gpu = opt.GpuInfo;
            if (gpu != null)
            {
                return new GpuInfo
                {
                    Description = gpu.Description,
                    DriverVersion = gpu.DriverVersion,
                    DriverDate = gpu.DriverDate?.ToString("yyyy-MM-dd"),
                };
            }
        }

        return new GpuInfo();
    }

    private static bool IsAdministrator()
    {
        try
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}

public class DateFilterInfo
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public class AppInfo
{
    public string UiVersion { get; set; } = string.Empty;
    public string CoreVersion { get; set; } = string.Empty;
    public string ResourceVersion { get; set; } = string.Empty;
    public string BuildTime { get; set; } = string.Empty;
}

public class SysInfo
{
    public string Os { get; set; } = string.Empty;
    public string OsArchitecture { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsWine { get; set; }
    public string? WineVersion { get; set; }
}

public class GpuInfo
{
    public string? Description { get; set; }
    public string? DriverVersion { get; set; }
    public string? DriverDate { get; set; }
}
