#pragma once

#include <string>

namespace asst
{
// OCR 引擎回退（C5）
//
// 现有 OCR 引擎：PaddleOCR（默认）+ FastDeploy + onnxruntime（备选）。
// RecruitResultImageAnalyzer 的 OCR 失败 ≥3 次时切换备用引擎。
//
// 实际 OCR 引擎管理由 OcrPack 负责（AsstInfrastDef.h），本类仅记录回退状态。
class OcrEngineFallback final
{
public:
    enum class Engine
    {
        PaddleOCR,
        FastDeploy,
        OnnxRuntime,
    };

    static OcrEngineFallback& get_instance();

    // 记录一次失败；返回 true 表示应切换到下一个引擎
    bool record_failure();

    // 当前应使用的引擎
    Engine current() const noexcept { return m_current; }

    // 强制切换（测试用）
    void force_switch(Engine e) { m_current = e; }

    const char* engine_name(Engine e) const;

private:
    OcrEngineFallback() = default;

    int m_consecutive_failure = 0;
    Engine m_current = Engine::PaddleOCR;
    static constexpr int kFailureThreshold = 3;
};
} // namespace asst