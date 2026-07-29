// <copyright file="IssueReportUserControlModel.cs" company="MaaAssistantArknights">
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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows;
using HandyControl.Controls;
using HandyControl.Data;
using JetBrains.Annotations;
using MaaWpfGui.Constants;
using MaaWpfGui.Helper;
using MaaWpfGui.Main;
using MaaWpfGui.Models;
using Microsoft.Win32;
using Serilog;
using Stylet;

namespace MaaWpfGui.ViewModels.UserControl.Settings;

/// <summary>
/// 问题反馈
/// </summary>
public class IssueReportUserControlModel : PropertyChangedBase
{
    static IssueReportUserControlModel()
    {
        Instance = new();
    }

    public static IssueReportUserControlModel Instance { get; }

    // ===== Diagnostic Report Properties (used by GenerateSupportPayload) =====

    private int _diagnosticDateRange = 7;

    public int DiagnosticDateRange
    {
        get => _diagnosticDateRange;
        set => SetAndNotify(ref _diagnosticDateRange, value);
    }

    private List<DateRangeOption>? _dateRangeOptions;

    public List<DateRangeOption> DateRangeOptions => _dateRangeOptions ??= InitDateRangeOptions();

    private static List<DateRangeOption> InitDateRangeOptions()
    {
        return
        [
            new(1, LocalizationHelper.GetString("DiagnosticLast1Day")),
            new(3, LocalizationHelper.GetStringFormat("DiagnosticLastNDays", 3)),
            new(7, LocalizationHelper.GetStringFormat("DiagnosticLastNDays", 7)),
            new(14, LocalizationHelper.GetStringFormat("DiagnosticLastNDays", 14)),
            new(30, LocalizationHelper.GetStringFormat("DiagnosticLastNDays", 30)),
        ];
    }

    public record DateRangeOption(int Value, string Display);

    private bool _includeConfig = true;

    public bool IncludeConfig
    {
        get => _includeConfig;
        set => SetAndNotify(ref _includeConfig, value);
    }

    private bool _includeCache;

    public bool IncludeCache
    {
        get => _includeCache;
        set => SetAndNotify(ref _includeCache, value);
    }

    private bool _includeCustomResource = true;

    public bool IncludeCustomResource
    {
        get => _includeCustomResource;
        set => SetAndNotify(ref _includeCustomResource, value);
    }

    // ===== End Diagnostic Export Properties =====

    public void OpenDebugFolder()
    {
        try
        {
            if (!Directory.Exists(PathsHelper.DebugDir))
            {
                Directory.CreateDirectory(PathsHelper.DebugDir);
            }

            Process.Start("explorer.exe", PathsHelper.DebugDir);
            AchievementTrackerHelper.Instance.Unlock(AchievementIds.BackstageExplorer);
        }
        catch (Exception ex)
        {
            ToastNotification.ShowDirect($"Failed to open debug folder\n{ex.Message}");
            Log.Error(ex, "Failed to open debug folder");
        }
    }

    public void OpenReportsFolder()
    {
        try
        {
            if (!Directory.Exists(PathsHelper.ReportsDir))
            {
                Directory.CreateDirectory(PathsHelper.ReportsDir);
            }

            Process.Start("explorer.exe", PathsHelper.ReportsDir);
        }
        catch (Exception ex)
        {
            ToastNotification.ShowDirect($"Failed to open reports folder\n{ex.Message}");
            Log.Error(ex, "Failed to open reports folder");
        }
    }

    /// <summary>
    /// 清空图片缓存 仅删除 cache 目录和 debug 目录中的图片文件，保留文件夹结构
    /// </summary>
    public static void ClearImageCache()
    {
        var result = MessageBoxHelper.Show(
            LocalizationHelper.GetString("ClearImageCacheTip"),
            LocalizationHelper.GetString("Warning"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Error,
            no: LocalizationHelper.GetString("Confirm"),
            yes: LocalizationHelper.GetString("Cancel"));
        if (result == MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png",
            };

            int deletedCount = 0;

            if (Directory.Exists(PathsHelper.CacheDir))
            {
                deletedCount += DeleteImageFiles(PathsHelper.CacheDir, imageExtensions);
            }

            if (Directory.Exists(PathsHelper.DebugDir))
            {
                deletedCount += DeleteImageFiles(PathsHelper.DebugDir, imageExtensions);
            }

            if (deletedCount > 0)
            {
                ShowGrowl(LocalizationHelper.GetString("ClearImageCacheSuccessful"));
            }
            else
            {
                ShowGrowl(LocalizationHelper.GetString("ClearImageCacheAlreadyEmpty"));
            }
        }
        catch (Exception ex)
        {
            ShowGrowl($"{LocalizationHelper.GetString("ClearImageCacheException")}\n{ex.Message}");
            Log.Error(ex, "Failed to clear image cache");
        }
    }

    /// <summary>
    /// 删除指定目录及其子目录中的所有图片文件。
    /// </summary>
    private static int DeleteImageFiles(string dir, HashSet<string> imageExtensions)
    {
        int deletedCount = 0;
        foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(file);
            if (imageExtensions.Contains(extension))
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, $"Failed to delete image file: {file}");
                }
            }
        }

        return deletedCount;
    }

    /// <summary>
    /// 生成诊断报告 — 合并原「生成日志压缩包」+「导出诊断包」：SaveFileDialog 选位置 + 日志按日期范围过滤 + diagnostic.json 系统信息 + 可选配置/缓存/自定义资源
    /// </summary>
    public void GenerateSupportPayload()
    {
        try
        {
            const int PartSize = 20 * 1024 * 1024; // 20 MB

            if (!Directory.Exists(PathsHelper.ReportsDir))
            {
                Directory.CreateDirectory(PathsHelper.ReportsDir);
            }

            string reportNameBase = $"report_{DateTimeOffset.Now:MM-dd_HH-mm-ss}";
            string tempPath = Path.Combine(PathsHelper.ReportsDir, $"maa-report-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempPath);

            // 弹保存对话框选保存位置
            var saveDialog = new SaveFileDialog
            {
                Title = LocalizationHelper.GetString("GenerateDiagnosticReportSelectLocation"),
                Filter = "ZIP files (*.zip)|*.zip",
                FileName = $"{reportNameBase}.zip",
                InitialDirectory = PathsHelper.ReportsDir,
                OverwritePrompt = true,
                AddExtension = true,
                DefaultExt = ".zip",
            };
            if (saveDialog.ShowDialog() != true)
            {
                Directory.Delete(tempPath, recursive: true);
                return;
            }

            string userChosenDir = Path.GetDirectoryName(saveDialog.FileName) ?? PathsHelper.ReportsDir;

            // 收集系统信息并写入 diagnostic.json
            var toDate = DateTime.Now.Date;
            var fromDate = toDate.AddDays(-_diagnosticDateRange);
            var diagInfo = DiagnosticInfo.Collect(fromDate, toDate);
            string diagJson = JsonSerializer.Serialize(diagInfo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(tempPath, "diagnostic.json"), diagJson);

            // 复制 debug/（rotated logs 由 LastWriteTime 在后续 part02+ 阶段过滤）
            CopyDirectoryIfExists(PathsHelper.DebugDir, Path.Combine(tempPath, "debug"),
                f => !Path.GetFileName(f).StartsWith("report", StringComparison.OrdinalIgnoreCase));

            // 可选目录
            if (_includeCustomResource)
            {
                CopyDirectoryIfExists(PathsHelper.ResourceDir, Path.Combine(tempPath, "resource"),
                    f => Path.GetFileName(f).Contains("_custom.", StringComparison.OrdinalIgnoreCase));
            }

            if (_includeConfig)
            {
                CopyDirectoryIfExists(PathsHelper.ConfigDir, Path.Combine(tempPath, "config"));
            }

            if (_includeCache)
            {
                CopyDirectoryIfExists(PathsHelper.CacheDir, Path.Combine(tempPath, "cache"));
            }

            // 分卷输出目录紧贴用户选定的 zip 路径所在目录
            string partsFolder = Path.Combine(userChosenDir, $"{reportNameBase}_parts");
            if (!Directory.Exists(partsFolder))
            {
                Directory.CreateDirectory(partsFolder);
            }

            // ====== part01：tempPath 根文件 + config + resource + cache + debug 根目录文件 ======
            List<string> part01Files = [];

            part01Files.AddRange(Directory.EnumerateFiles(tempPath, "*", SearchOption.TopDirectoryOnly));

            string[] categories = ["config", "resource", "cache"];
            foreach (string category in categories)
            {
                string categoryPath = Path.Combine(tempPath, category);
                if (Directory.Exists(categoryPath))
                {
                    part01Files.AddRange(Directory.EnumerateFiles(categoryPath, "*", SearchOption.AllDirectories));
                }
            }

            string debugPath = Path.Combine(tempPath, "debug");
            if (Directory.Exists(debugPath))
            {
                var debugRootFiles = Directory.EnumerateFiles(debugPath, "*", SearchOption.TopDirectoryOnly).ToList();
                part01Files.AddRange(debugRootFiles);
            }

            if (part01Files.Count > 0)
            {
                string part01Path = Path.Combine(partsFolder, $"{reportNameBase}_part01.zip");
                using var fs = new FileStream(part01Path, FileMode.Create);
                using var archive = new ZipArchive(fs, ZipArchiveMode.Create);
                foreach (var file in part01Files)
                {
                    string entryName = Path.GetRelativePath(tempPath, file).Replace("\\", "/");
                    var entry = archive.CreateEntry(entryName, CompressionLevel.SmallestSize);

                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(file);
                    fileStream.CopyTo(entryStream);
                }
            }

            // ====== part02+：debug 子目录文件按日期范围 + PartSize 分卷 ======
            var cutoffTime = DateTime.Now.AddDays(-_diagnosticDateRange);
            var debugSubFiles = Directory.EnumerateFiles(debugPath, "*", SearchOption.AllDirectories)
                .Where(f => Path.GetDirectoryName(f) != debugPath)
                .Where(f => new FileInfo(f).LastWriteTime >= cutoffTime)
                .ToList();

            int partNumber = 2;
            while (debugSubFiles.Count > 0)
            {
                string partFileName = $"{reportNameBase}_part{partNumber:D2}.zip";
                string partPath = Path.Combine(partsFolder, partFileName);

                using (var fs = new FileStream(partPath, FileMode.Create))
                {
                    using var archive = new ZipArchive(fs, ZipArchiveMode.Create);
                    long currentSize = 0;
                    List<string> processedFiles = [];

                    foreach (var file in debugSubFiles.ToList())
                    {
                        var fileInfo = new FileInfo(file);

                        if (currentSize + fileInfo.Length > PartSize && currentSize > 0)
                        {
                            break;
                        }

                        string entryName = Path.GetRelativePath(tempPath, file).Replace("\\", "/");
                        var entry = archive.CreateEntry(entryName, CompressionLevel.SmallestSize);

                        using (var entryStream = entry.Open())
                        {
                            using var fileStream = File.OpenRead(file);
                            fileStream.CopyTo(entryStream);
                        }

                        currentSize += fileInfo.Length;
                        processedFiles.Add(file);
                    }

                    debugSubFiles.RemoveAll(f => processedFiles.Contains(f));
                }

                partNumber++;
            }

            // ====== 生成完整压缩包（用户选定路径） ======
            string fullZipPath = saveDialog.FileName;
            ZipFile.CreateFromDirectory(tempPath, fullZipPath, CompressionLevel.SmallestSize, includeBaseDirectory: false);

            // 清理临时目录
            Directory.Delete(tempPath, recursive: true);

            ShowGrowl($"{LocalizationHelper.GetString("GenerateSupportPayloadSuccessful")}\n{fullZipPath}");
            OpenReportsFolder();
        }
        catch (Exception ex)
        {
            ShowGrowl($"{LocalizationHelper.GetString("GenerateSupportPayloadException")}\n{ex.Message}");
            Log.Error(ex, "Failed to create support payload");
        }
    }

    /*
    /// <summary>
    /// 删除目录下的所有文件和子目录，排除指定的文件名。
    /// </summary>
    private static void DeleteDirectoryContentsExcept(string dir, IEnumerable<string> excludeFileNames)
    {
        var excludeSet = excludeFileNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            if (excludeSet.Contains(Path.GetFileName(file)))
            {
                continue;
            }

            File.Delete(file);
        }

        foreach (var subDir in Directory.EnumerateDirectories(dir))
        {
            Directory.Delete(subDir, recursive: true);
        }
    }
    */

    /// <summary>
    /// 从 sourceDir 复制文件到 targetDir，支持过滤。
    /// </summary>
    private static void CopyDirectoryIfExists(string? sourceDir, string targetDir, Func<string, bool>? filter = null)
    {
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            if (filter != null && !filter(file))
            {
                continue;
            }

            string relative = Path.GetRelativePath(sourceDir, file);
            string dest = Path.Combine(targetDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            try
            {
                File.Copy(file, dest, true);
            }
            catch (IOException)
            {
                // 某些文件可能被占用，忽略复制失败
            }
            catch (UnauthorizedAccessException)
            {
                // 也忽略权限问题
            }
        }
    }

    // UI 绑定的方法
    [UsedImplicitly]
    public void SetAcknowledgedNightlyWarning()
    {
        // 其实不应该放这里，但懒得写一个新的方法，就塞到这里了
        AchievementTrackerHelper.Instance.Unlock(AchievementIds.ProblemFeedback);
        VersionUpdateSettingsUserControlModel.Instance.HasAcknowledgedNightlyWarning = true;
    }

    private static void ShowGrowl(string message)
    {
        var growlInfo = new GrowlInfo {
            IsCustom = true,
            Message = message,
            IconKey = "HangoverGeometry",
            IconBrushKey = "PallasBrush",
        };
        Growl.Info(growlInfo);
    }
}
