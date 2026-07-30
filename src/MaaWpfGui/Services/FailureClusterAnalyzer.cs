// feat/recruit-result-display (B3) — 失败模式聚类分析
//
// 目的：统计哪些 tags 组合最容易识别失败，指导后续优化
// 数据源：RecruitHistoryService.Entries (ocr_status="L3" 视为失败)
// 存储：config/recruit_failure_clusters.json

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaaWpfGui.Services
{
    public class FailureCluster
    {
        [JsonPropertyName("tags_hash")]
        public string TagsHash { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("last_failed")]
        public long LastFailed { get; set; }

        [JsonPropertyName("common_ocr_raw")]
        public string CommonOcrRaw { get; set; } = string.Empty;
    }

    public class FailureClusterAnalyzer
    {
        public static FailureClusterAnalyzer Instance { get; } = new();

        private readonly string _storagePath;
        private readonly Dictionary<string, FailureCluster> _clusters = new();
        private readonly object _lock = new();

        private FailureClusterAnalyzer()
        {
            _storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaaAssistantArknights", "config", "recruit_failure_clusters.json");
        }

        public IReadOnlyList<FailureCluster> TopClusters(int n = 10)
        {
            lock (_lock)
            {
                return _clusters.Values.OrderByDescending(c => c.Count).Take(n).ToList();
            }
        }

        public void Load()
        {
            lock (_lock)
            {
                _clusters.Clear();
                if (!File.Exists(_storagePath)) return;
                try
                {
                    var json = File.ReadAllText(_storagePath);
                    var list = JsonSerializer.Deserialize<List<FailureCluster>>(json) ?? new();
                    foreach (var c in list) _clusters[c.TagsHash] = c;
                }
                catch { /* ignore */ }
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_storagePath)!);
                var json = JsonSerializer.Serialize(_clusters.Values.ToList(),
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_storagePath, json, Encoding.UTF8);
            }
        }

        public void RecordFailure(IEnumerable<string> tags, string ocrRaw)
        {
            var sortedTags = tags.OrderBy(t => t).ToList();
            var hash = ComputeTagsHash(sortedTags);

            lock (_lock)
            {
                if (!_clusters.TryGetValue(hash, out var cluster))
                {
                    cluster = new FailureCluster { TagsHash = hash, Tags = sortedTags };
                    _clusters[hash] = cluster;
                }
                cluster.Count += 1;
                cluster.LastFailed = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                cluster.CommonOcrRaw = ocrRaw; // 简单覆盖；后续可改为多值计数
            }
        }

        private static string ComputeTagsHash(IEnumerable<string> sortedTags)
        {
            var joined = string.Join("|", sortedTags);
            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(joined));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}