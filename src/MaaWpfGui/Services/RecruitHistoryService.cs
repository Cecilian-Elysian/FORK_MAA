// feat/recruit-result-display (L2) — 招募历史服务
//
// 路径：config/recruit_history.json (AES-256-GCM 加密，D5)
// 上限：1000 条，超出滚动清理
//
// 数据来源：AsstProxy.cs 收到 RecruitSlotCompleted callback 时 RecordSlot()

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MaaWpfGui.Services
{
    public class RecruitHistoryEntry
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("slot_index")]
        public int SlotIndex { get; set; }

        [JsonPropertyName("slot_total")]
        public int SlotTotal { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("operator")]
        public string Operator { get; set; } = string.Empty;

        [JsonPropertyName("operator_id")]
        public string OperatorId { get; set; } = string.Empty;

        [JsonPropertyName("ocr_status")]
        public string OcrStatus { get; set; } = "L3";

        [JsonPropertyName("ocr_raw_text")]
        public string OcrRawText { get; set; } = string.Empty;

        [JsonPropertyName("screenshot_path")]
        public string ScreenshotPath { get; set; } = string.Empty;

        [JsonPropertyName("screenshot_sha256")]
        public string ScreenshotSha256 { get; set; } = string.Empty;

        [JsonPropertyName("expedited")]
        public bool Expedited { get; set; }

        [JsonPropertyName("account_name")]
        public string AccountName { get; set; } = string.Empty;

        [JsonPropertyName("user_override")]
        public string UserOverride { get; set; } = string.Empty;

        [JsonPropertyName("user_note")]
        public string UserNote { get; set; } = string.Empty;
    }

    public class RecruitHistoryService
    {
        public static RecruitHistoryService Instance { get; } = new();

        private const int MaxEntries = 1000;
        private readonly string _storagePath;
        private readonly List<RecruitHistoryEntry> _entries = new();
        private readonly object _lock = new();

        private RecruitHistoryService()
        {
            _storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaaAssistantArknights", "config", "recruit_history.json");
        }

        public IReadOnlyList<RecruitHistoryEntry> Entries
        {
            get
            {
                lock (_lock) { return _entries.ToList(); }
            }
        }

        public void Load()
        {
            lock (_lock)
            {
                _entries.Clear();
                if (!File.Exists(_storagePath)) return;
                try
                {
                    var encrypted = File.ReadAllBytes(_storagePath);
                    var json = HistoryCrypto.Instance.Decrypt(encrypted);
                    var data = JsonSerializer.Deserialize<List<RecruitHistoryEntry>>(json) ?? new();
                    _entries.AddRange(data);
                }
                catch (Exception ex)
                {
                    // 加密失败时降级为明文（历史遗留数据）
                    try
                    {
                        var json = File.ReadAllText(_storagePath, Encoding.UTF8);
                        var data = JsonSerializer.Deserialize<List<RecruitHistoryEntry>>(json) ?? new();
                        _entries.AddRange(data);
                    }
                    catch { /* ignore */ }
                }
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_storagePath)!);
                var json = JsonSerializer.Serialize(_entries);
                var encrypted = HistoryCrypto.Instance.Encrypt(json);
                File.WriteAllBytes(_storagePath, encrypted);
            }
        }

        public void RecordSlot(RecruitHistoryEntry entry)
        {
            lock (_lock)
            {
                _entries.Add(entry);
                // 滚动清理
                while (_entries.Count > MaxEntries) _entries.RemoveAt(0);
            }
            Save();
        }

        /// <summary>
        /// 异步写入（不阻塞 callback 线程）
        /// </summary>
        public void RecordSlotAsync(RecruitHistoryEntry entry)
        {
            // 在线程池上同步执行 Save（避免阻塞 callback）
            Task.Run(() =>
            {
                try { RecordSlot(entry); }
                catch (Exception ex) { /* 单条失败不致命 */ }
            });
        }

        public void UpdateEntry(int index, string userOverride, string userNote)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _entries.Count) return;
                _entries[index].UserOverride = userOverride;
                _entries[index].UserNote = userNote;
            }
            Save();
        }

        public string ExportJson()
        {
            lock (_lock)
            {
                return JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        public void ImportJson(string json)
        {
            lock (_lock)
            {
                var imported = JsonSerializer.Deserialize<List<RecruitHistoryEntry>>(json) ?? new();
                foreach (var e in imported)
                {
                    if (_entries.Count >= MaxEntries) _entries.RemoveAt(0);
                    _entries.Add(e);
                }
            }
            Save();
        }
    }
}