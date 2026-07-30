// feat/recruit-result-display (L3) — WPF 端经验库读取服务
//
// 路径：config/recruit_experience.json
// 与 C++ 端 RecruitExperience 共享 JSON 格式
// 用途：WPF 启动时加载，用于公招历史面板的「上次招到 X」展示

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaaWpfGui.Services
{
    public class ExperienceEntry
    {
        [JsonPropertyName("tags_hash")]
        public string TagsHash { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("operator")]
        public string Operator { get; set; } = string.Empty;

        [JsonPropertyName("operator_id")]
        public string OperatorId { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; } = string.Empty;

        [JsonPropertyName("last_seen")]
        public long LastSeen { get; set; }
    }

    public class RecruitExperienceService
    {
        public static RecruitExperienceService Instance { get; } = new();

        private readonly string _storagePath;
        private readonly List<ExperienceEntry> _entries = new();
        private readonly object _lock = new();

        private RecruitExperienceService()
        {
            _storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MaaAssistantArknights", "config", "recruit_experience.json");
        }

        public IReadOnlyList<ExperienceEntry> Entries
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
                    var json = File.ReadAllText(_storagePath);
                    var data = JsonSerializer.Deserialize<ExperienceFile>(json);
                    if (data?.Experiences != null) _entries.AddRange(data.Experiences);
                }
                catch { /* ignore */ }
            }
        }

        private class ExperienceFile
        {
            [JsonPropertyName("experiences")]
            public List<ExperienceEntry>? Experiences { get; set; }
        }
    }
}