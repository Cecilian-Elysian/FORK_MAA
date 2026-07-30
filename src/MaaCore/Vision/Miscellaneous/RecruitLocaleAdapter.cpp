#include "RecruitLocaleAdapter.h"

#include <algorithm>
#include <fstream>

#include "Utils/Logger.hpp"

namespace asst
{
RecruitLocaleAdapter::Server RecruitLocaleAdapter::parse_server(const std::string& server_name)
{
    if (server_name == "CN" || server_name == "Official") return Server::CN;
    if (server_name == "Bilibili" || server_name == "BL") return Server::Bilibili;
    if (server_name == "YoStarEN" || server_name == "EN") return Server::YoStarEN;
    if (server_name == "YoStarJP" || server_name == "JP") return Server::YoStarJP;
    if (server_name == "YoStarKR" || server_name == "KR") return Server::YoStarKR;
    if (server_name == "txwy" || server_name == "TW") return Server::txwy;
    Log.warn(__FUNCTION__, "未知 server 名:", server_name, "降级为 CN");
    return Server::CN;
}

std::filesystem::path RecruitLocaleAdapter::roster_path(Server srv)
{
    switch (srv) {
    case Server::CN:
    case Server::Bilibili:
        return std::filesystem::path("resource/recruit/oper_roster_zh-cn.json");
    case Server::YoStarEN:
        return std::filesystem::path("resource/recruit/oper_roster_en-us.json");
    case Server::YoStarJP:
        return std::filesystem::path("resource/recruit/oper_roster_ja-jp.json");
    case Server::YoStarKR:
        return std::filesystem::path("resource/recruit/oper_roster_ko-kr.json");
    case Server::txwy:
        return std::filesystem::path("resource/recruit/oper_roster_zh-tw.json");
    }
    return std::filesystem::path("resource/recruit/oper_roster_zh-cn.json");
}

std::string RecruitLocaleAdapter::normalize_name(const std::string& raw)
{
    std::string out;
    out.reserve(raw.size());
    for (char c : raw) {
        if (c == ' ' || c == '\t' || c == '\n' || c == '\r') continue;
        // 半角 → 全角（仅限常用标点 + 数字）
        if (c >= '!' && c <= '~') {
            // 跳过；干员名通常是汉字/日韩字符，OCR 错位时直接保留
        }
        out.push_back(c);
    }
    return out;
}

std::vector<std::string>
RecruitLocaleAdapter::build_required_set(Server srv, const std::filesystem::path& resource_root)
{
    std::vector<std::string> out;

    std::filesystem::path global_dir;
    switch (srv) {
    case Server::CN:
        global_dir = resource_root / "resource";
        break;
    case Server::Bilibili:
        global_dir = resource_root / "resource";
        break;
    case Server::YoStarEN:
        global_dir = resource_root / "resource" / "global" / "YoStarEN" / "resource";
        break;
    case Server::YoStarJP:
        global_dir = resource_root / "resource" / "global" / "YoStarJP" / "resource";
        break;
    case Server::YoStarKR:
        global_dir = resource_root / "resource" / "global" / "YoStarKR" / "resource";
        break;
    case Server::txwy:
        global_dir = resource_root / "resource" / "global" / "txwy" / "resource";
        break;
    }

    auto recruit_json = global_dir / "recruitment.json";
    std::ifstream f(recruit_json);
    if (!f) {
        Log.warn(__FUNCTION__, "roster 文件不存在:", recruit_json.string());
        return out;
    }

    json::value j;
    try {
        f >> j;
    }
    catch (const std::exception& e) {
        Log.error(__FUNCTION__, "roster JSON 解析失败:", e.what());
        return out;
    }

    if (!j.contains("operators") || !j["operators"].is_array()) return out;

    for (const auto& oper : j["operators"].as_array()) {
        if (oper.contains("name") && oper["name"].is_string()) {
            out.push_back(oper["name"].as_string());
        }
    }
    Log.info(__FUNCTION__, "roster 加载完成:", out.size(), "干员, server=", static_cast<int>(srv));
    return out;
}
} // namespace asst