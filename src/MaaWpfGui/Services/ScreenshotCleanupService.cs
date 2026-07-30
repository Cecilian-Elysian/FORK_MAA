// feat/recruit-result-display (C2) — 截图自动清理服务
//
// 启动时清理 7 天前的 debug/recruit_*.png
// 任务结束时再清理一次（避免长时间运行堆积）

using System;
using System.IO;
using System.Linq;

namespace MaaWpfGui.Services
{
    public class ScreenshotCleanupService
    {
        public static ScreenshotCleanupService Instance { get; } = new();

        private const int MaxAgeDays = 7;
        private readonly string _debugDir;

        private ScreenshotCleanupService()
        {
            _debugDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaaAssistantArknights", "debug");
        }

        public int CleanupOldFiles()
        {
            if (!Directory.Exists(_debugDir)) return 0;

            var cutoff = DateTime.UtcNow.AddDays(-MaxAgeDays);
            var removed = 0;

            foreach (var file in Directory.EnumerateFiles(_debugDir, "recruit_*.png"))
            {
                try
                {
                    var lastWrite = File.GetLastWriteTimeUtc(file);
                    if (lastWrite < cutoff)
                    {
                        File.Delete(file);
                        removed++;
                    }
                }
                catch { /* ignore single file errors */ }
            }
            return removed;
        }
    }
}