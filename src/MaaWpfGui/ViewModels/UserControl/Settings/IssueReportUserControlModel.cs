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
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>
    /// 单个分卷 zip 未压缩字节上限 = GitHub Issue 附件 20MB 上限。压缩后实际 zip 体积会显著小于此值。
    /// </summary>
    private const long MaxPartSizeBytes = 20L * 1024 * 1024;

    // ===== Diagnostic Report Properties (used by GenerateSupportPayload) =====

    private int _diagnosticDateRange = 7;

    public int DiagnosticDateRange
    {
        get => _diagnosticDateRange;
        set => SetAndNotify(ref _diagnosticDateRange, value);
    }

    private Lazy<List<DateRangeOption>> _dateRangeOptions = new(InitDateRangeOptions);

    public List<DateRangeOption> DateRangeOptions => _dateRangeOptions.Value;

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

    private bool _isBusy;

    /// <summary>
    /// 「生成诊断报告」执行期间为 true，绑定按钮 IsEnabled 防止重复点击。
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetAndNotify(ref _isBusy, value))
            {
                NotifyOfPropertyChange(nameof(IsNotBusy));
            }
        }
    }

    public bool IsNotBusy => !_isBusy;

    private string _busyStatusText = string.Empty;

    /// <summary>
    /// 异步执行时显示给用户的进度文字（如"正在复制 debug 日志..."），由后台线程通过 Dispatcher 投回 UI 线程更新。
    /// </summary>
    public string BusyStatusText
    {
        get => _busyStatusText;
        set => SetAndNotify(ref _busyStatusText, value);
    }

    // ===== End Diagnostic Report Properties =====

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
    /// 清空图片缓存 仅删除 cache 目录和 debug 目录中的图片文件，保留文件夹结构。
    /// 注：下方 MessageBoxHelper.Show 调用 HandyControl 自定义按钮文案机制 — <c>yes:</c> 参数对应 MessageBoxResult.Yes 按钮文案、<c>no:</c> 对应 No 按钮文案。
    /// 当前 yes=Cancel / no=Confirm 是有意为之（用户习惯「确认删除」放主按钮位即 No 位），改回 yes=Confirm/no=Cancel 会导致用户点击主按钮反而放弃删除，引入回归风险。
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
    /// 生成诊断报告 — WPF action 入口。串行编排：准备上下文 → 写 diagnostic.json → 复制目录（带失败收集）→ 统一按大小分卷 → 生成完整 zip → 清理临时目录。
    /// 全程 <c>Task.Run</c> 包 IO，避免 UI 线程卡顿；通过 <see cref="ReportBusyStatus"/> 向 UI 线程回传进度文字。
    /// </summary>
    public async void GenerateSupportPayload()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        BusyStatusText = string.Empty;
        ExportContext? ctx = null;
        try
        {
            ctx = TryPrepareExportContext();
            if (ctx == null)
            {
                return;
            }

            await Task.Run(() => ExecuteExportPipeline(ctx));

            ShowGrowlSuccess(ctx);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create support payload");
            ShowGrowlError(ex);
        }
        finally
        {
            if (ctx != null)
            {
                SafeDelete(ctx.TempPath);
            }

            BusyStatusText = string.Empty;
            IsBusy = false;
        }
    }

    /// <summary>
    /// 后台线程执行的导出流水线。调用方需在 UI 线程先获取 <see cref="ExportContext"/>，本方法不再触发对话框。
    /// </summary>
    private void ExecuteExportPipeline(ExportContext ctx)
    {
        var toDate = DateTime.Now.Date;
        var fromDate = toDate.AddDays(-_diagnosticDateRange);

        ReportBusyStatus(LocalizationHelper.GetStringFormat("DiagnosticBusyStatus", LocalizationHelper.GetString("DiagnosticBusyCopyingDebug")));
        var copyResults = CopyAll(ctx, fromDate);

        ReportBusyStatus(LocalizationHelper.GetStringFormat("DiagnosticBusyStatus", LocalizationHelper.GetString("DiagnosticBusyWritingJson")));
        WriteDiagnosticJson(ctx, fromDate, toDate);

        ReportBusyStatus(LocalizationHelper.GetStringFormat("DiagnosticBusyStatus", LocalizationHelper.GetString("DiagnosticBusySplitting")));
        SplitIntoParts(ctx, fromDate);

        ReportBusyStatus(LocalizationHelper.GetStringFormat("DiagnosticBusyStatus", LocalizationHelper.GetString("DiagnosticBusyZippingFull")));
        ZipFile.CreateFromDirectory(ctx.TempPath, ctx.FullZipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

        if (copyResults.FailedFiles.Count > 0)
        {
            Log.Warning("Diagnostic report copied {Copied} files; skipped {Skipped} due to IO/permission errors: {Files}",
                copyResults.CopiedCount, copyResults.FailedFiles.Count, string.Join(", ", copyResults.FailedFiles));
        }
    }

    /// <summary>
    /// 弹保存对话框选保存位置 + 创建 tempPath。返回 null 表示用户取消或异常。
    /// </summary>
    private ExportContext? TryPrepareExportContext()
    {
        if (!Directory.Exists(PathsHelper.ReportsDir))
        {
            Directory.CreateDirectory(PathsHelper.ReportsDir);
        }

        string reportNameBase = $"report_{DateTimeOffset.Now:MM-dd_HH-mm-ss}";
        string tempPath = Path.Combine(PathsHelper.ReportsDir, $"maa-report-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);

        string? fullZipPath;
        try
        {
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
                return null;
            }

            fullZipPath = saveDialog.FileName;
        }
        catch
        {
            // SaveFileDialog 异常时也要清理已创建的 tempPath
            SafeDelete(tempPath);
            throw;
        }

        string userChosenDir = Path.GetDirectoryName(fullZipPath) ?? PathsHelper.ReportsDir;
        string partsFolder = Path.Combine(userChosenDir, $"{reportNameBase}_parts");

        return new ExportContext(
            FromDate: DateTime.Now.Date.AddDays(-_diagnosticDateRange),
            ToDate: DateTime.Now.Date,
            ReportNameBase: reportNameBase,
            TempPath: tempPath,
            FullZipPath: fullZipPath,
            PartsFolder: partsFolder);
    }

    private void WriteDiagnosticJson(ExportContext ctx, DateTime fromDate, DateTime toDate)
    {
        var info = DiagnosticInfo.Collect(fromDate, toDate);
        string json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(ctx.TempPath, "diagnostic.json"), json);
    }

    /// <summary>
    /// 顺序复制 debug（含日期过滤）→ 自定义资源 → 配置 → 缓存。debug 子目录文件按 <paramref name="fromDate"/> 截日过滤（LastWriteTime &lt; fromDate 跳过）；
    /// debug 根目录文件（gui.log / asst.log / gui.bak.log 等主日志）始终包含。
    /// </summary>
    private CopyResult CopyAll(ExportContext ctx, DateTime fromDate)
    {
        var result = new CopyResult();

        CopyDirectoryWithLogging(
            PathsHelper.DebugDir,
            Path.Combine(ctx.TempPath, "debug"),
            sourceRoot: PathsHelper.DebugDir,
            fromDate: fromDate,
            filter: f => !Path.GetFileName(f).StartsWith("report", StringComparison.OrdinalIgnoreCase),
            result: result);

        if (_includeCustomResource)
        {
            CopyDirectoryWithLogging(
                PathsHelper.ResourceDir,
                Path.Combine(ctx.TempPath, "resource"),
                sourceRoot: PathsHelper.ResourceDir,
                fromDate: null,
                filter: f => Path.GetFileName(f).Contains("_custom.", StringComparison.OrdinalIgnoreCase),
                result: result);
        }

        if (_includeConfig)
        {
            CopyDirectoryWithLogging(
                PathsHelper.ConfigDir,
                Path.Combine(ctx.TempPath, "config"),
                sourceRoot: PathsHelper.ConfigDir,
                fromDate: null,
                filter: null,
                result: result);
        }

        if (_includeCache)
        {
            CopyDirectoryWithLogging(
                PathsHelper.CacheDir,
                Path.Combine(ctx.TempPath, "cache"),
                sourceRoot: PathsHelper.CacheDir,
                fromDate: null,
                filter: null,
                result: result);
        }

        return result;
    }

    /// <summary>
    /// 按未压缩字节大小统一分卷 — 不再区分 part01（debug 根文件）与 part02+（debug 子目录）。
    /// 先收集所有 tempPath 下文件 → 按文件名排序（保证 part 内容稳定可复现）→ 累加大小超过 <see cref="MaxPartSizeBytes"/> 时关闭当前 zip 开启下一个。
    /// 分卷元数据回填到 <see cref="DiagnosticInfo.Parts"/> 并重新写入 diagnostic.json，最终进入完整 zip。
    /// </summary>
    private void SplitIntoParts(ExportContext ctx, DateTime fromDate)
    {
        if (!Directory.Exists(ctx.PartsFolder))
        {
            Directory.CreateDirectory(ctx.PartsFolder);
        }

        var allFiles = Directory.EnumerateFiles(ctx.TempPath, "*", SearchOption.AllDirectories)
            .Select(f => new FileInfo(f))
            .OrderBy(fi => fi.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var partMetas = new List<PartInfo>();
        var currentFiles = new List<FileInfo>();
        long currentSize = 0;
        int partNumber = 1;

        void FlushCurrent()
        {
            if (currentFiles.Count == 0)
            {
                return;
            }

            string partFileName = $"{ctx.ReportNameBase}_part{partNumber:D2}.zip";
            string partPath = Path.Combine(ctx.PartsFolder, partFileName);
            using (var fs = new FileStream(partPath, FileMode.Create))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (var file in currentFiles)
                {
                    string entryName = Path.GetRelativePath(ctx.TempPath, file.FullName).Replace("\\", "/");
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(file.FullName);
                    fileStream.CopyTo(entryStream);
                }
            }

            partMetas.Add(new PartInfo
            {
                FileName = partFileName,
                UncompressedSizeBytes = currentSize,
                FileCount = currentFiles.Count,
            });

            currentFiles.Clear();
            currentSize = 0;
            partNumber++;
        }

        foreach (var fi in allFiles)
        {
            // 跳过 diagnostic.json — 它包含 Parts 字段，分卷生成期间还是空的；最终由 CreateFromDirectory 直接打包完整 tempPath
            if (string.Equals(fi.Name, "diagnostic.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            long size = fi.Length;

            // 当前文件单个就超阈值 — 直接开新卷装它（无法拆分单文件）
            if (size > MaxPartSizeBytes && currentFiles.Count == 0)
            {
                currentFiles.Add(fi);
                FlushCurrent();
                continue;
            }

            if (currentSize + size > MaxPartSizeBytes && currentFiles.Count > 0)
            {
                FlushCurrent();
            }

            currentFiles.Add(fi);
            currentSize += size;
        }

        FlushCurrent();

        // 回填 diagnostic.json — 读已有内容、覆盖 Parts 字段、重写；最终 ZipFile.CreateFromDirectory 会把更新后的 diagnostic.json 打入完整 zip
        string diagJsonPath = Path.Combine(ctx.TempPath, "diagnostic.json");
        if (File.Exists(diagJsonPath))
        {
            var json = File.ReadAllText(diagJsonPath);
            var info = JsonSerializer.Deserialize<DiagnosticInfo>(json) ?? new DiagnosticInfo();
            info.Parts = partMetas;
            File.WriteAllText(diagJsonPath, JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    /// <summary>
    /// 从 sourceDir 复制文件到 targetDir，支持按 <paramref name="fromDate"/> 过滤（仅对子目录文件生效，根目录文件始终包含）。
    /// 文件复制失败（IOException / UnauthorizedAccessException）记录到 <paramref name="result"/>.FailedFiles 而非静默吞错。
    /// </summary>
    private static void CopyDirectoryWithLogging(
        string? sourceDir,
        string targetDir,
        string sourceRoot,
        DateTime? fromDate,
        Func<string, bool>? filter,
        CopyResult result)
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

            // debug 根目录文件始终包含；仅子目录文件按 fromDate 过滤 LastWriteTime
            if (fromDate.HasValue)
            {
                bool isRootFile = string.Equals(
                    Path.GetDirectoryName(file),
                    sourceRoot,
                    StringComparison.OrdinalIgnoreCase);
                if (!isRootFile && File.GetLastWriteTime(file) < fromDate.Value)
                {
                    continue;
                }
            }

            string relative = Path.GetRelativePath(sourceDir, file);
            string dest = Path.Combine(targetDir, relative);
            string? destDir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            try
            {
                File.Copy(file, dest, true);
                result.CopiedCount++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                result.FailedFiles.Add(file);
                Log.Warning(ex, "Failed to copy diagnostic file: {File}", file);
            }
        }
    }

    private void ShowGrowlSuccess(ExportContext ctx)
    {
        string message = LocalizationHelper.GetString("GenerateSupportPayloadSuccessful") + "\n" + ctx.FullZipPath;
        Application.Current.Dispatcher.Invoke(() => ShowGrowl(message));
    }

    private void ShowGrowlError(Exception ex)
    {
        string message = LocalizationHelper.GetString("GenerateSupportPayloadException") + "\n" + ex.Message;
        Application.Current.Dispatcher.Invoke(() => ShowGrowl(message));
    }

    /// <summary>
    /// 把进度文字投回 UI 线程更新（后台线程直接 SetAndNotify 会触发跨线程异常）。
    /// </summary>
    private void ReportBusyStatus(string status)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            BusyStatusText = status;
        }
        else
        {
            dispatcher.Invoke(() => BusyStatusText = status);
        }
    }

    private static void SafeDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to delete temp directory: {Path}", path);
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
        var growlInfo = new GrowlInfo
        {
            IsCustom = true,
            Message = message,
            IconKey = "HangoverGeometry",
            IconBrushKey = "PallasBrush",
        };
        Growl.Info(growlInfo);
    }

    /// <summary>
    /// 导出上下文 — 持有单次「生成诊断报告」所需的全部中间状态，由 <see cref="TryPrepareExportContext"/> 在 UI 线程构造后传给后台流水线。
    /// </summary>
    private sealed record ExportContext(
        DateTime FromDate,
        DateTime ToDate,
        string ReportNameBase,
        string TempPath,
        string FullZipPath,
        string PartsFolder);

    private sealed class CopyResult
    {
        public int CopiedCount { get; set; }

        public List<string> FailedFiles { get; } = new();
    }
}
