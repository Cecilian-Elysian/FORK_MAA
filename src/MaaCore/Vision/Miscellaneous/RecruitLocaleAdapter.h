#pragma once

#include <filesystem>
#include <string>
#include <vector>

namespace asst
{
// 五语服适配（CN / EN / JP / KR / TW）
//
// 资源路径：resource/global/<Server>/resource/recruitment.json
// 每个 server 用同一份 tags，但 name 字段按 server 本地化。
//
// RecruitResultImageAnalyzer 的 L1 阶段会用 build_required_set() 构造 OCR required 列表。
class RecruitLocaleAdapter final
{
public:
    enum class Server
    {
        CN, // 官服
        Bilibili, // B 服
        YoStarEN,
        YoStarJP,
        YoStarKR,
        txwy, // 繁中
    };

    // 服名 → Server 枚举（不识别则返回 CN）
    static Server parse_server(const std::string& server_name);

    // 服名 → resource/recruit/oper_roster_<locale>.json 路径（用于 pHash 库等）
    static std::filesystem::path roster_path(Server srv);

    // 把 OCR 输出归一化（去空格、半角→全角、数字与字母统一）
    static std::string normalize_name(const std::string& raw);

    // 加载指定服的 roster，返回所有干员名（用于 OCR required）
    static std::vector<std::string> build_required_set(Server srv, const std::filesystem::path& resource_root);
};
} // namespace asst