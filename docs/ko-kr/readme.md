---
icon: ic:round-home
index: true
dir:
  order: 0
---

<!-- markdownlint-disable -->

::: center

<!-- markdownlint-restore -->

![MAA Logo =256x256](/images/maa-logo_512x512.png)

# MAA Assistant Arknights · Cecilian-Elysian Fork

![C++](https://img.shields.io/badge/C++-20-%2300599C?logo=cplusplus)  
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blueviolet)  
![license](https://img.shields.io/github/license/Cecilian-Elysian/FORK_MAA) ![upstream](https://img.shields.io/badge/upstream-MaaAssistantArknights%2FMaaAssistantArknights-181717?logo=github)
![stars](https://img.shields.io/github/stars/Cecilian-Elysian/FORK_MAA?style=social) ![commit](https://img.shields.io/github/commit-activity/m/Cecilian-Elysian/FORK_MAA?color=%23ff69b4)
<a href="https://github.com/Cecilian-Elysian/FORK_MAA/commits/feat/upstream-v617-sync"><img src="https://img.shields.io/badge/sync-v6.17.0-blue?logo=git" alt="sync v6.17.0"></a>

MAA는 MAA Assistant Arknights의 약자입니다

본 저장소는 [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights)의 **개인 강화형 로컬 포크**입니다.
다중 계정 로테이션, 업스트림 베이스라인 추적, 로컬 빌드/검증을 중심으로 하며 업스트림을 직접 머지하지 않습니다. Fork 전용 기능은 `feat/<name>` / `fix/<name>` 브랜치에서 점진적으로 발전하고 `staging` → `branch` 플로우로 통합됩니다.

명일방주 어시스턴트

이미지 인식을 기반으로, 한 번의 클릭만으로 그날의 모든 작업을 끝내드립니다!

개발 진행 중입니다 ✿✿ヽ(°▽°)ノ✿

:::

## 본 포크 소개

| 항목 | 설명 |
|------|------|
| 업스트림 | [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) `master-v2` (안정 릴리스 브랜치) |
| 미러 | 로컬 `master`는 `upstream/master-v2`를 가리키며 `tools/update-upstream.ps1`로 동기화 |
| 동기화 | 업스트림 릴리스마다 (예: v6.17.0) `feat/upstream-<version>-sync`에서 베이스라인 충돌을 해결한 뒤 주제별 브랜치를 파생하여 Fork 변경을 구현 |
| Fork 전용 기능 | 계정 로테이션, 계정별 데이터 버킷, 지원 가속 임계값, 응접실 단서 폴백, 진단 보고서, `expedite_min_level` 프로토콜 확장 등 모두 `branch`의 로컬 작업에서 파생 |
| 문서 | 저장소 루트의 [README](../../README.md) + [AGENTS.md](../../AGENTS.md) + [WORKFLOW.md](../../WORKFLOW.md) + [LOG.md](../../LOG.md) |

> `upstream/master-v2`를 `staging` 또는 `branch`로 직접 머지하지 마세요. Fork 흐름 세부사항은 [WORKFLOW.md](../../WORKFLOW.md) §5 (역사 接목) 및 §6 (수동 충돌 해결)을 참조하세요.

## 다운로드 및 설치

[문서](./manual/newbie.md) 를 읽은 후 [공식 웹사이트](https://maa.plus) 또는 [Releases](https://github.com/MaaAssistantArknights/MaaAssistantArknights/releases) 에서 다운로드하세요. [초보자 가이드](./manual/newbie.md) 를 참고하여 설치하세요.

본 포크의 설치 절차는 업스트림과 완전히 동일합니다. `branch`가 업스트림보다 뒤처져 있다면 업스트림의 빌드된 릴리스를 우선 사용하고, 로컬 빌드는 [WORKFLOW.md](../../WORKFLOW.md)에 따라 진행하세요.

## 주요 기능

- 이성을 사용하고, 아이템 드랍을 인식해 자동으로 통계 사이트에 업로드 합니다. [펭귄 물류](https://penguin-stats.io/), [Yituliu](https://ark.yituliu.cn/)
- 자동으로 오퍼레이터 효율을 고려해 기반시설 교대를 하며, 시설 간 조합을 지원합니다. 동시에 사용자 커스텀 교대 기능도 지원합니다. [기반시설 스케줄링 프로토콜](./protocol/base-scheduling-schema.md)
- 자동으로 공개모집 및 즉시 모집을 선택할 수 있으며, 자동으로 통계 사이트에 업로드 합니다. [펭귄 물류](https://penguin-stats.io/result/stage/recruit/recruit), [Yituliu](https://ark.yituliu.cn/survey/maarecruitdata)
- 공개 모집 화면에서 태그 인식을 할 수 있습니다.
- 오퍼레이터 목록을 인식해, 보유중인 오퍼레이터들 및 잠재를 통계화하여 공개모집 태그 인식 시 표시합니다.
- 현재 보유중인 육성 재화를 인식해 다음 사이트로 데이터를 내보낼 수 있습니다. [펭귄 물류 플래너](https://penguin-stats.io/planner), [Arkntools](https://arkntools.app/#/material), [ark-nights](https://ark-nights.com/settings)
- 공개 모집 인터페이스에 수동 식별을 지원하여 높은 등급의 오퍼레이터 공개 모집을 용이하게 합니다.
- 오퍼레이터 목록을 식별하고, 보유 및 미보유 오퍼레이터와 그들의 잠재를 계산하며, 이를 공개 모집 인터페이스에서 표시하는 것을 지원합니다.
- 원클릭으로 친구 방문 및 크레딧 획득/구매를 하며, 임무 보상 수령 등의 모든 일과를 수행합니다.
- 통합 전략에서 자동으로 오퍼레이터 레벨을 인식하며, 오리지늄 각뿔 획득 및 노드 개방과 레벨을 올려줍니다.
- Copliot 파일을 통한 자동 지휘가 가능합니다. [영상 설명](https://www.bilibili.com/video/BV1H841177Fk/)
- C, Python, Java, Rust, Golang, Java HTTP, Rust HTTP 다음과 같은 다양한 환경에서 MAA를 사용할 수 있습니다!

### Fork 전용 강화

- **다중 계정 자동 로테이션**(`feat/account_rotation`): 일상 임무가 구성된 계정 순서로 자동 실행. 마지막까지 로그라이크/생식 연산 연기 (`feat/defer-rogue`)
- **오퍼레이터/창고 인식 데이터를 계정별 버킷화**(`feat/account-scoped-recognition-data`): 계정 전환 시 재고가 섞이지 않음
- **지원 가속 임계값**(`feat/expedite-threshold`): Fork 프로토콜 필드 `expedite_min_level`는 확인된 최소 레어도 ≥ 임계값 (4/5/6)일 때만 긴급 소집권 사용
- **지원 가속 슬롯별 클릭**(`fix/recruit-expedite-slot-target`): 다중 슬롯 동시 실행 시 잘못된 가속 방지
- **진단 보고서 재구성**(`fix/diagnostic-export-refactor`): 크기별 분할 패키징, 시스템 정보 수집, 비동기 실행
- **응접실 단서 빠른 투입 폴백**(`fix/reception-clue-restore`): 업스트림 issue #16165 수정
- **로컬 빌드 체인 강화**: `tools/local-install-staging.bat`, VS 2022 BuildTools 호환 경로, NetBeauty2 후처리

### Fork 전용 기능 핵심 파일

- `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.AccountCycle.cs`: 계정 로테이션 오케스트레이션
- `src/MaaWpfGui/ViewModels/Orchestration/AccountCycleOrchestrator.cs`: 오케스트레이터
- `src/MaaWpfGui/ViewModels/UI/ToolboxViewModel.cs`의 `#region AccountScopedRecognitionData`: 계정별 데이터 버킷
- `src/MaaWpfGui/Models/DiagnosticInfo.cs`: 진단 보고서 모델
- `src/MaaCore/Task/Infrast/InfrastReceptionTask.cpp`의 `proc_clue_vacancy`: 응접실 단서 빠른 투입 폴백
- `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.{h,cpp}`와 `src/MaaCore/Task/Interface/RecruitTask.cpp`: `expedite_min_level` 임계값 판정

말로만 설명하기보다는 사진으로 보여드리겠습니다!

<ImageGrid :imageList="[
  {
    light: 'images/ko-kr/readme/1-light.png',
    dark: 'images/ko-kr/readme/1-dark.png'
  },
  {
    light: 'images/ko-kr/readme/2-light.png',
    dark: 'images/ko-kr/readme/2-dark.png'
  },
  {
    light: 'images/ko-kr/readme/3-light.png',
    dark: 'images/ko-kr/readme/3-dark.png'
  },
  {
    light: 'images/ko-kr/readme/4-light.png',
    dark: 'images/ko-kr/readme/4-dark.png'
  }
]" />

## 사용 방법

### 기능 소개

[사용자 매뉴얼](./manual/)을 참조하세요.

### 해외 서버 지원

현재 글로벌 서버 EN/KR/JP/txwy 서버의 대부분의 기능이 지원됩니다.
그러나, 해외 서버 사용자가 적고 프로젝트 인원이 부족하기 때문에 대부분의 기능이 자세하게 테스트를 거치지 않았습니다.
그러니 직접 사용해서 어떠한지 확인하시기 바랍니다.
만약 버그가 있거나, 특정 기능에 대한 강한 요구가 있다면 [Issues](https://github.com/MaaAssistantArknights/MaaAssistantArknights/issues) 및 [토론](https://github.com/MaaAssistantArknights/MaaAssistantArknights/discussions) 에서 요청하거나 참여해주세요!
현지화는 다음을 참조해주세요. [해외 클라이언트 현지화](./develop/overseas-client-adaptation.md)

### CLI지원

MAA는 명령줄 인터페이스(CLI)를 지원하며, Linux,macOS 및 Window에서 사용할 수 있으며, 자동화 스크립트 작성 및 그래픽 인터페이스가 없는 서버에서 사용할 수 있습니다. [CLI 가이드](./manual/cli/)

## 관련 프로젝트

### 주요 관련 프로젝트

- New Framework: [MaaFramework](https://github.com/MaaXYZ/MaaFramework)
- [전략 파일 저장소](https://prts.plus): Frontend [zoot-plus-frontend](https://github.com/ZOOT-Plus/zoot-plus-frontend)
- [전략 파일 저장소](https://prts.plus): Backend [ZootPlusBackend](https://github.com/ZOOT-Plus/ZootPlusBackend)
- [공식 웹사이트](https://maa.plus): [frontend](https://github.com/MaaAssistantArknights/maa-website)
- Deep Learning: [MaaAI](https://github.com/MaaAssistantArknights/MaaAI)
- 본 포크: [Cecilian-Elysian/FORK_MAA](https://github.com/Cecilian-Elysian/FORK_MAA)

### 다국어 지원 (i18n)

MAA는 중국어(간체)를 기본 언어로 하며, 번역 단어는 중국어(간체)를 기준으로 합니다.

### 개발 참여 및 기여

[개발 가이드](./develop/development.md)와 저장소 루트의 [AGENTS.md](../../AGENTS.md) / [WORKFLOW.md](../../WORKFLOW.md)를 참조하세요. 본 포크 요점:

- 편집 전에 [`docs/downstream-changes.md`](../../docs/downstream-changes.md)에서 과거 Fork 변경 확인
- 새 기능은 독립적인 `feat/<name>` / `fix/<name>` 브랜치에서 작업하여 교차 도메인 변경이 동일 충돌 세트에 섞이지 않도록 함
- Fork 전용 로직은 `src/MaaWpfGui/Models`, `TaskQueueViewModel.AccountCycle.cs`, `ToolboxViewModel.cs`, `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.*` 같은 핫스팟에 집중

### API

- [C 인터페이스](../../include/AsstCaller.h)：[예제](../../src/Cpp/main.cpp)
- [Python 인터페이스](../../src/Python/asst/asst.py)：[예제](../../src/Python/sample.py)
- [Golang 인터페이스](../../src/Golang)：[예제](../../src/Golang/maa/maa.go)
- [Dart 인터페이스](../../src/Dart)
- [Java 인터페이스](../../src/Java/src/main/java/com/iguigui/maaj/easySample/MaaCore.java)：[예제](../../src/Java/src/main/java/com/iguigui/maaj/easySample/MaaJavaSample.java)
- [Java HTTP 인터페이스](../../src/Java/Readme.md)
- [Rust 인터페이스](../../src/Rust/src/maa_sys)：[HTTP 인터페이스](../../src/Rust)
- [TypeScript 인터페이스](https://github.com/MaaAssistantArknights/MaaX/tree/main/packages/main/coreLoader)
- [Woolang 인터페이스](../../src/Woolang/maa.wo)：[예제](../../src/Woolang/demo.wo)
- [통합문서](./protocol/integration.md)
- [콜백 프로토콜](./protocol/callback-schema.md)
- [작업 프로토콜](./protocol/task-schema.md)
- [자동지휘 프로토콜](./protocol/copilot-schema.md)

### 해외 서버 현지화

[해외 클라이언트 현지화](./develop/overseas-client-adaptation.md)를 참조하세요. 대부분은 스크린샷 + 간단한 json 수정만 필요합니다.

### Issue bot

[Issue bot 사용법](./develop/issue-bot-usage.md)

## 감사의 말

### 오픈소스 라이브러리

- 이미지 인식: [opencv](https://github.com/opencv/opencv.git)
- ~~글자 인식: [chineseocr_lite](https://github.com/DayBreak-u/chineseocr_lite.git)~~
- 글자 인식: [PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR)
- 딥러닝 배포 라이브러리: [FastDeploy](https://github.com/PaddlePaddle/FastDeploy)
- ML accelerator: [onnxruntime](https://github.com/microsoft/onnxruntime)
- ~~아이템 드랍 인식: [Penguin Stats recognizer](https://github.com/penguin-statistics/recognizer)~~
- 맵 타일 인식: [Arknights-Tile-Pos](https://github.com/yuanyan3060/Arknights-Tile-Pos)
- C++ JSON 라이브러리: [meojson](https://github.com/MistEO/meojson.git)
- C++ 오퍼레이터 파서: [calculator](https://github.com/kimwalisch/calculator)
- ~~C++ Base64 인코딩/디코딩: [cpp-base64](https://github.com/ReneNyffenegger/cpp-base64)~~
- C++ ZIP 라이브러리: [zlib](https://github.com/madler/zlib)
- C++ Gzip 라이브러리: [gzip-hpp](https://github.com/mapbox/gzip-hpp)
- 안드로이드 터치 이벤트 구현: [Minitouch](https://github.com/DeviceFarmer/minitouch)
- 안드로이드 터치 이벤트 구현: [MaaTouch](https://github.com/MaaAssistantArknights/MaaTouch)
- WPF MVVM 프레임워크: [Stylet](https://github.com/canton7/Stylet)
- WPF 조작 라이브러리: [HandyControl](https://github.com/HandyOrg/HandyControl) -> [HandyControls](https://github.com/ghost1372/HandyControls)
- C# 로그: [Serilog](https://github.com/serilog/serilog)
- C# JSON 라이브러리: [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) & [System.Text.Json](https://github.com/dotnet/runtime)
- ~~다운로더: [aria2](https://github.com/aria2/aria2)~~

### 데이터 소스

- ~~공개모집 데이터: [ArkTools](https://www.bigfun.cn/tools/aktools/hr)~~
- ~~오퍼레이터/기반시설 데이터: [PRTS Arknights Wiki (Chinese)](http://prts.wiki/)~~
- 스테이지 데이터: [Penguin Stats](https://penguin-stats.io/)
- 게임 데이터/리소스: [Arknights Game Resource](https://github.com/yuanyan3060/ArknightsGameResource)
- 게임 데이터: [Arknights Yostar Game Data](https://github.com/ArknightsAssets/ArknightsGamedata)

### 기여자

MAA의 개선을 위한 개발/테스트에 기여해준 모든 친구들에게 감사드립니다! (\*´▽｀)ノノ

[![Contributors](https://contributors-img.web.app/image?repo=Cecilian-Elysian/FORK_MAA&max=105&columns=15)](https://github.com/Cecilian-Elysian/FORK_MAA/graphs/contributors)<br>
[업스트림 기여자](https://github.com/MaaAssistantArknights/MaaAssistantArknights/graphs/contributors)

## 주의사항

- 본 소프트웨어는 [GNU Affero General Public License v3.0 only](https://spdx.org/licenses/AGPL-3.0-only.html) 오픈소스 라이선스를 사용합니다, 또한 추가의 [사용자 계약](https://github.com/MaaAssistantArknights/MaaAssistantArknights/blob/dev-v2/terms-of-service.md)이 포함됩니다.
- 이 프로그램의 로고는 AGPL 3.0 라이센스의 적용 대상이 아닙니다. [耗毛](https://weibo.com/u/3251357314)와 Vie 두 명의 아티스트와 프로그램의 개발자들이 모든 권리를 가집니다. 프로젝트가 AGPL 3.0 라이센스 하에 있다고 하더라도 프로그램의 로고는 동의 없이 사용되어서는 안 되며, 동의 없는 상업적 이용 또한 금지됩니다.
- 이 프로그램은 오픈소스이자 무료이며 학습 및 커뮤니케이션 전용입니다. 이 프로그램을 이용해 장비값이나 시간당 수수료 등을 대가로 서비스를 제공하는 판매자로 인해 발생하는 문제와 결과는 프로그램의 개발자들에게는 책임이 없습니다.

### DirectML 지원 설명

이 소프트웨어는 Microsoft에서 Windows 플랫폼에서 제공하는 독립 구성 요소 [DirectML](https://learn.microsoft.com/en-us/windows/ai/directml/)에 의존하는 GPU 가속을 지원합니다. DirectML은 이 프로젝트의 오픈 소스 부분이 아니며 AGPL 3.0의 적용을 받지 않습니다. 사용자의 편의를 위해 설치 패키지에 수정되지 않은 DirectML.dll 파일을 포함했습니다. GPU 가속이 필요하지 않으면 DLL 파일을 안전하게 삭제해도 소프트웨어의 핵심 기능은 정상적으로 실행됩니다.

## 광고

사용자 그룹: [QQ 그룹](https://api.maa.plus/MaaAssistantArknights/api/qqgroup/index.html)  
Discord 서버: [Discord 링크](https://discord.gg/23DfZ9uA4V)  
사용자 그룹: [Telegram](https://t.me/+Mgc2Zngr-hs3ZjU1)  
[전략 JSON 공유](https://prts.plus)  
Bilibili 라이브 방송: [MrEO 방송](https://live.bilibili.com/2808861) 코딩 방송 & [MAA-Official 방송](https://live.bilibili.com/27548877) 게임/잡담

[명일방주 무관 기술 공유 & 만담 (QQ 그룹)](https://jq.qq.com/?_wv=1027&k=ypbzXcA2): 지옥 같아요!  
[개발자 그룹 (QQ 그룹)](https://jq.qq.com/?_wv=1027&k=JM9oCk3C)

프로그램이 도움이 된다고 생각하시면 Star를 눌러주세요! (페이지 우측 상단의 작은 별) 저희에게 가장 큰 도움이 됩니다!
