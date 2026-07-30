#include "OcrEngineFallback.h"

#include "Utils/Logger.hpp"

namespace asst
{
OcrEngineFallback& OcrEngineFallback::get_instance()
{
    static OcrEngineFallback inst;
    return inst;
}

bool OcrEngineFallback::record_failure()
{
    m_consecutive_failure += 1;
    if (m_consecutive_failure >= kFailureThreshold) {
        Engine next = m_current;
        if (m_current == Engine::PaddleOCR) next = Engine::FastDeploy;
        else if (m_current == Engine::FastDeploy) next = Engine::OnnxRuntime;
        else next = Engine::PaddleOCR;

        if (next != m_current) {
            Log.warn(__FUNCTION__, "OCR 引擎回退:", engine_name(m_current), "→", engine_name(next));
            m_current = next;
            m_consecutive_failure = 0;
            return true;
        }
    }
    return false;
}

const char* OcrEngineFallback::engine_name(Engine e) const
{
    switch (e) {
    case Engine::PaddleOCR: return "PaddleOCR";
    case Engine::FastDeploy: return "FastDeploy";
    case Engine::OnnxRuntime: return "OnnxRuntime";
    }
    return "Unknown";
}
} // namespace asst