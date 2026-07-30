# 淇敼鏃ュ織

鏃ュ織瑙勮寖锛氭瘡娆′慨鏀规枃浠跺悗锛屽湪姝よ褰曚慨鏀瑰唴瀹广€?
## 2026-07-27

### tools/local-install-staging.bat 鍒涘缓

`branch` 鍒嗘敮浣跨敤 `install/` 浣滀负鏋勫缓杈撳嚭鐩綍銆傛柊澧?`tools/local-install-staging.bat`锛岀粨鏋勪笌 `local-install.bat` 涓€鑷达紝浠呭皢 6 澶?`install` 璺緞鏀逛负 `install-staging`锛岃緭鍑哄埌鐙珛鐩綍涓?`install/` 骞跺瓨锛屼簰涓嶈鐩栥€俙branch` 涓?`staging` 鍒嗘敮鍚勮嚜鐙珛鏋勫缓锛屾闈㈠垎鍒揩鎹枫€?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `tools/local-install-staging.bat` | 鏂板缓 | 鍩轰簬 `local-install.bat`锛? 澶?`install` 璺緞鏀逛负 `install-staging`锛坈make `--prefix`銆乨otnet `-o`銆乶beauty 琛ヤ竵銆佹竻鐞?`*.h`/`msvc-debug`銆乺obocopy `resource`锛?|
| 2 | `LOG.md` | 淇敼 | 鏈妭 |

**鐢ㄦ硶**:
- `branch` 鍒嗘敮锛歚tools\local-install.bat` 鈫?`install\MAA.exe`
- `staging` 鍒嗘敮锛歚tools\local-install-staging.bat` 鈫?`install-staging\MAA.exe`

### staging 妗岄潰蹇嵎鏂瑰紡

`%USERPROFILE%\Desktop\MAA (staging).lnk`锛氭寚鍚?`install-staging\MAA.exe`锛岃捣濮嬬洰褰?`install-staging\`锛屽浘鏍囧鐢?MAA.exe 鑷韩鍥炬爣銆傞娆℃墽琛屽墠闇€鍦?`staging` 鍒嗘敮涓婂厛杩愯 `tools\local-install-staging.bat` 鐢熸垚鏋勫缓浜х墿锛屽惁鍒欏弻鍑绘姤閿欍€屾壘涓嶅埌鏂囦欢銆嶃€?
| # | 璺緞 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `%USERPROFILE%\Desktop\MAA (staging).lnk` | 鍒涘缓 | PowerShell `WScript.Shell.CreateShortcut()`锛岀洰鏍?`install-staging\MAA.exe`锛岃捣濮嬬洰褰?`install-staging\` |
| 2 | `LOG.md` | 淇敼 | 鏈妭 |

### tools/local-install{,-staging}.bat global.json 鐗堟湰閿佸畾淇

棣栨璺?`tools\local-install-staging.bat` 鏋勫缓 `install-staging/` 鏃讹紝**C++ 绔瀯寤烘垚鍔熶絾 WPF `dotnet restore`/`publish` 鍏ㄥけ璐?*锛岃剼鏈蛋鍒?`:error` 閫€鍑猴紝`install-staging/` 鏈敓鎴愩€傛挒鍒?AGENTS.md 搂4.1 璁板綍鐨?VS 2026 SDK 璺緞 bug 涓庝竴涓箣鍓嶆湭璁板綍鐨勮剼鏈潙鍙犲姞銆?
**鏍瑰洜**锛歚local-install.bat` 绗?11 琛岋紙`local-install-staging.bat` 鍚屼綅缃級鍘熷啓锛?
```bat
> ".\global.json" echo {"sdk":{"version":"10.0.203","rollForward":"disable"}}
```

- `"version":"10.0.203"` 閿佹 10.0.203锛屼絾鏈満鍙 10.0.300 SDK锛坄dotnet --list-sdks`锛?- `"rollForward":"disable"` 绂佺敤 fallback锛岃繛 `latestPatch` / `latestFeature` / `latestMajor` 閮戒笉鍏佽
- MSBuild 鎵句笉鍒?SDK 鈫?`MSB4276` / `鏃犳硶瑙ｆ瀽 SDK"Microsoft.NET.Sdk"` 鎶ラ敊
- 璇?`global.json` 鐢辫剼鏈敞鍏ュ埌浠撳簱鏍圭洰褰曪紝瑕嗙洊鎵€鏈夊瓙鐩綍锛坄src/MaaWpfGui/`锛?- cmake `--build build --parallel` 瑙﹀彂 WPF MSBuild 璇勪及锛堝凡鐭?bug锛夛紝鏁撮摼璺繛閿佸け璐?
**淇**锛氬皢涓や釜 bat 鐨勭 11 琛屾敼涓轰笌 AGENTS.md 搂4.1 鎻忚堪涓€鑷寸殑鍐欐硶锛?
```bat
> ".\global.json" echo {"sdk":{"version":"10.0.100","rollForward":"latestFeature"}}
```

`10.0.100` + `latestFeature` 鍏佽鑷姩鏅嬪崌鍒?10.0.x锛堝寘鍚湰鏈虹殑 10.0.300锛夛紝鍚戝悗鍏煎鎵€鏈?dotnet 10.x 瑁呮満鐗堟湰銆?
**棣栨鎴愬姛鏋勫缓 `install-staging/` 鐨勭粫杩囨柟娉?*锛堝湪鑴氭湰淇鍓嶆墜鍔ㄦ墽琛岋級锛?
```bash
cmake --install build --config RelWithDebInfo --prefix install-staging
dotnet restore src/MaaWpfGui/MaaWpfGui.csproj
dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -r win-x64 -o install-staging /p:DisableBeauty=True
& "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x86\nbeauty2.exe" --usepatch "$PWD\install-staging\." ./externals
# 娓呯悊 *.h / msvc-debug / robocopy resource锛堜笌 bat 涓€鑷达級
```

缁曞紑鑴氭湰娉ㄥ叆鐨?global.json锛岃 dotnet 鑷姩閫夌敤鏈満鏈€楂?SDK銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `tools/local-install.bat:11` | 淇敼 | `global.json` 娉ㄥ叆浠?`{"version":"10.0.203","rollForward":"disable"}` 鏀逛负 `{"version":"10.0.100","rollForward":"latestFeature"}` |
| 2 | `tools/local-install-staging.bat:11` | 淇敼 | 鍚屼笂 |
| 3 | `install-staging/` | 鏋勫缓浜у嚭 | C++ / WPF 鍏ㄩ噺鏋勫缓鎴愬姛锛?775 鏂囦欢涓?`install/resource/` 瀹屽叏涓€鑷达紱`MAA.exe` 339 KB / `MaaCore.dll` 4.0 MB / `MAA.dll` 3.6 MB |
| 4 | `LOG.md` | 淇敼 | 鏈妭 |

## 2026-07-25

### staging 鍒嗘敮寮曞叆 + fix/expedite-threshold 閲嶅懡鍚?
**鑳屾櫙**锛歚fix/expedite-threshold` 绱Н浜?11 涓法鏂瑰悜 commit锛堝惎鍔ㄩ摼 / 鍒囧彿 OCR / 鍔犳€ラ棬妲?/ recruit_now 椤哄簭 / docs锛夛紝宸蹭笉閫傚悎浣滀负鍗曚竴 fix 鍒嗘敮鍛藉悕銆傚紩鍏?`staging` 灞備綔涓?feat / fix 鐨勫悎骞剁洰鏍囦笌 `branch` 涔嬮棿鐨勫緟楠岃瘉鏁村悎鍖猴紝鏀掓壒娴嬭瘯閫氳繃鍚庢檵鍗囪嚦 `branch`銆?
**宸ヤ綔娴佸彉鏇?*锛?
```
master (涓婃父 dev-v2 闀滃儚)
  鈹? (rebase / merge 鍚屾鑺傚涓嶅彉)
  鈻?branch (绋冲畾涓嬫父鍩虹嚎) 鈼勨攢鈹€鈹€鈹€ staging 鏅嬪崌 (--no-ff, 鏀掓壒)
  鈹?                                鈻?  鈹?(feat / fix 鎷夊彇婧?              鈹?(鍚堝苟鐩爣)
  鈻?                                鈹?feat/<name>, fix/<name> 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹?```

- 鎵€鏈?feat / fix 涓€寰嬩粠 `branch` 鎷夊嚭锛屽悎骞跺埌 `staging`
- `staging` 鏀掍竴鎵癸紙寤鸿 3-5 涓級娴嬭瘯閫氳繃鍚庡啀鏅嬪崌鑷?`branch`
- `branch` 鈫?`master` 涓婃父鍚屾鑺傚涓嶅彉
- 褰撳墠 `staging` 棰嗗厛 `branch` 11 commits銆佽惤鍚?2 commits锛坆ranch 涓婄殑 `784d9005f6` + `da157d163d` 涓?staging 涓婄殑 cherry-pick `6011051af2` + `f241b2160b` 鍐呭绛変环銆丼HA 涓嶅悓锛夛紝棣栨鏅嬪崌灏嗘棤娉?FF锛岄渶 `--no-ff`

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | 鏈湴 git 鍒嗘敮 | 閲嶅懡鍚?| `fix/expedite-threshold` 鈫?`staging` |
| 2 | `Github` 杩滅 | 鎺ㄩ€?| 鏂板 `staging` 鍒嗘敮锛涙棫鍚?`fix/expedite-threshold` 鏈帹杩囪繙绔紝鏃犻渶鍒犻櫎 |
| 3 | `AGENTS.md 搂2.2` | 淇敼 | 鏂板 `staging` 琛岋紱`branch` 澶囨敞銆屾湰鍦颁笅娓告暣鍚堛€嶁啋銆岀ǔ瀹氫笅娓稿熀绾裤€?|
| 4 | `AGENTS.md 搂2.3` | 淇敼 | 銆宖eat 鍚堝苟鍒?`branch`銆嶁啋銆宖eat 鍚堝苟鍒?`staging`銆?|
| 5 | `AGENTS.md 搂2.4` | 鏂板 | staging 宸ヤ綔娴侊紙鎷撴墤 / 瑙勫垯 / 褰撳墠寰呴獙璇佸唴瀹癸級 |
| 6 | `AGENTS.md 搂3.2` | 淇敼 | feat 娴佺▼姝ラ 6 鍚堝苟鐩爣 `branch` 鈫?`staging` |
| 7 | `AGENTS.md 搂3.3` | 淇敼 | fix 鍚堝苟鐩爣琛ャ€屼慨 branch 鑷韩鐨?fix 鈫?鍚堝苟鍒?`staging`銆?|
| 8 | `AGENTS.md 搂6` | 淇敼 | 鍒犻櫎 `fix/expedite-threshold` 琛岋紙staging 鏄暱鏈熻鑹诧紝涓嶅睘浜?feat/fix 閫熸煡锛?|
| 9 | `LOG.md` | 淇敼 | 鏈妭 |

**鎺?upstream**锛氫粎鏈?fork 宸ヤ綔娴佽皟鏁达紝涓嶆帹銆?
### fix/expedite-threshold recruit_now 璋冪敤椤哄簭淇

`feat/expedite-threshold`锛坄7df4e94e3f`锛夐噸鏋勬椂鎶?`recruit_now()` 浠?`_run()` 澶栧眰寰幆鎸繘 `recruit_one()`,浣嗘尓鍒颁簡 `confirm()` 涔嬪墠銆傛父鎴?UI 瑙勫垯:銆岀珛鍗虫嫑 / 绔嬪嵆瀹屾垚銆嶆寜閽彧瀛樺湪浜庡叕鎷涗富椤?slot 宸插紑濮?9h 鍊掕鏃?, 璇︽儏椤?confirm 涔嬪墠)鏃犳鎸夐挳銆傚鑷?`RecruitNow` task 鐨?OCR `["绔嬪嵆鎷?]` 鍦?ROI `[0,300,1280,420]` 鍐?4 娆?retry 鍏ㄧ┖, `recruit_now()` 蹇呭け璐? 鍔犳€ュ垽瀹氶€氳繃鍗村疄闄呮湭鍔犳€? slot 濮嬬粓璧?9h 鍊掕鏃躲€?
`install/debug/asst.bak.log` 澶氭棩澶嶇幇(line 158873-158897, 2026-07-25 14:03:46-48):
- 鍔犳€ュ垽瀹氭棩蹇?`Recruit slot level 4 >= expedite threshold 4 , using expedited plan.` 姝ｅ父鎵撳嵃
- OCR 瀹為檯璇嗗埆鏂囨湰涓恒€屽凡鎷涘嫙骞插憳 / 杩滅▼浣?/ 杩戞垬/鍥炲 / 寮€濮嬪埛鏂版爣绛?/ 鎷涘嫙棰勬湡銆嶇瓑璇︽儏椤靛厓绱?- 4 娆?retry 鍚?`SubTaskError`, `Failed to use expedited plan, fall back to normal confirm.`
- 闅忓悗 `check_timer` + `RecruitConfirm` 璧板畬姝ｅ父 9h 纭娴佺▼

淇: 鎶婂姞鎬ュ潡浠?`confirm()` 涔嬪墠鎸埌 `confirm()` 涔嬪悗, 鎭㈠ `3529ab0f05` 鍘熺増鐨勩€屽厛纭鍚姩 9h, 鍐嶄富椤电偣绔嬪嵆瀹屾垚銆嶈涔夈€俙fix/expedite-threshold` 鏃㈡湁鐨?`m_last_confirmed_min_level` 涓ゅ閲嶇疆(line 312-314 / 358-359)淇濇寔涓嶅姩銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:353-365` | 鍒犻櫎 | 绉婚櫎 `confirm()` 涔嬪墠鐨勫姞鎬ュ潡 |
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:389` | 鎻掑叆 | 鍦?`confirm()` 鎴愬姛鍚庛€乣return` 鍓嶆彃鍏ユ柊鍔犳€ュ潡, 鍚€岀珛鍗冲畬鎴愰渶鍦ㄤ富椤点€嶆敞閲?|
| 3 | `LOG.md` | 淇敼 | 鏈妭 |
| 4 | `AGENTS.md 搂6` | 淇敼 | `fix/expedite-threshold` 瑙掕壊鎻忚堪杩藉姞銆岋紱recruit_now 绉诲埌 confirm 涔嬪悗锛堜慨璇︽儏椤垫棤銆岀珛鍗虫嫑銆嶆寜閽鑷村姞鎬ュ繀澶辫触锛夈€?|

**鎺?upstream**: 浠呮湰 fork 淇, 涓嶆帹銆?
### fix/account-switch-retry LoginOther OCR 妯℃澘鍏滃簳 + retry_times 鍒嗘瀽淇

**鍒濈増璇垽**锛坄cd704f8bbc`锛? 灏?`navigate_to_start_page` 鐨?`retry_times` 浠?30 闄嶈嚦 5, 浠ヤ负 30 娆?retry 鍏ㄨ姳鍦?LoginOther OCR銆傚疄娴嬪け璐ワ紙`asst.log` 19:18:57锛? `last matched task: SwitchAccount@StartUpBegin`, 瀵艰埅棣栨灏辫€楀厜 5 娆?retry, `TaskChainError`銆?
**鏍瑰洜淇**: 闃呰 `ProcessTask::find_and_run_task()`锛坄ProcessTask.cpp:336-380`锛夊彂鐜?`cur_retry` 鏄?*灞€閮ㄥ彉閲?*, 姣忔 `run()` 寰幆璋冪敤 `find_and_run_task()` 鏃?*鐙珛浠?0 寮€濮?*銆傞摼璺瘡涓€姝ュ悇鑷韩鏈夊畬鏁寸殑 `m_retry_times` 棰勭畻銆傚鑸姝ワ紙`SwitchAccount@StartUpBegin` 鈫?22 涓?`next` 鍊欓€夛級鏈€鍧忛渶瑕?~13 娆?retry 鎵嶆湁 UI 鍏冪礌鍙瘑鍒? 5 娆¤繙杩滀笉澶熴€?
**姝ｇ‘淇硶**: 淇濈暀 `retry_times=30`锛堝鑸綑閲忎笉鍙橈級, 鍦?`LoginOther.next` 杩藉姞 `AccountManagerPageConfirm`锛坄baseTask: AccountManagerListAccount` + `action: DoNothing`锛夈€侽CR 澶辫触鏃跺悓涓€ retry cycle 鍐呮ā鏉垮尮閰嶅厹搴曞懡涓紙鏃ュ織鍘嗗彶 score 0.93 绋冲畾锛? 涓嶅啀绌鸿€?30 脳 0.6s = 18s retry銆傜敱浜?`action: DoNothing`, 涓嶆敼鍙?UI 鐘舵€? 鍚庣画 `equal_current_account()` / `show_account_list()` 涓嶅彈褰卞搷銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:68` | 鍥為€€ | `set_retry_times(5)` 鈫?`set_retry_times(30)` + 娉ㄩ噴鏇存柊 |
| 2 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:74` | 淇敼 | `last_name` 鐧藉悕鍗曡拷鍔?`"AccountManagerPageConfirm"` |
| 3 | `resource/tasks/tasks.json:808-817` | 淇敼 | `LoginOther.next` 杩藉姞 `"AccountManagerPageConfirm"`; 鏂板 `AccountManagerPageConfirm` task锛坄baseTask: AccountManagerListAccount`, `action: DoNothing`锛?|
| 4 | `LOG.md` | 淇敼 | 鏈妭锛堟浛鎹㈠垵鐗堢殑 retry_times=5 鎻忚堪锛?|
| 5 | `AGENTS.md 搂6` | 淇敼 | `fix/account-switch-retry` 鎻忚堪鏇存柊 |

**棰勬湡鏁堟灉**: 姣忔 `navigate_to_start_page` 鐨?LoginOther 闃舵浠?~18s锛?0 retry 脳 0.6s锛夐檷鑷?~0.1s锛堥涓?cycle 妯℃澘鍛戒腑锛? 姣忔 -18s, 涓よ处鍙?-36s銆傚鑸樁娈典笉鍙楀奖鍝嶏紙retry_times=30 涓嶅彉锛夈€?
**鎺?upstream**: 浠呮湰 fork 淇, 涓嶆帹銆?
## 2026-07-24

### fix/expedite-threshold 璐﹀彿鍒楄〃 OCR 閫傞厤 UI 鏀圭増

**淇涓婁竴涓?commit (`2715162c3d`)** 鐨勫垏鍙烽摼淇銆備笂涓€鐗堝湪鍒囧彿閾惧姞浜?`SwitchAccount@StartToWakeUpOCR` OCR 鍏滃簳浠ュ鐞嗛拱瑙掔櫥褰曞脊绐楀満鏅紝浣?*涓昏矾寰?`AccountManagerOfficial` 鐨?OCR 鏂囨湰浠嶆槸鍗曟枃鏈€岀櫥褰曡褰曘€?*锛屼笌楣拌鐧诲綍璐﹀彿鍒楄〃椤垫敼鐗堝悗鐨勫疄闄?UI 涓嶅尮閰嶁€斺€旂敤鎴峰疄闄呰处鍙峰垪琛ㄦ樉绀虹殑鏄€?*涓婃鐧诲綍 X 鍒嗛挓鍓?*銆嶈€岄潪銆岀櫥褰曡褰曘€嶏紝瀵艰嚧鍒囧彿閾炬渶缁?OCR 妫€鏌ュけ璐ャ€?0 retry 鍏ㄥけ璐ャ€?x restart_game 姝诲惊鐜€?
鐢ㄦ埛鎻愪緵鐨勬埅鍥捐瘉鎹細
- 鎴浘 1锛堜富鑿滃崟锛夛細銆屽紑濮嬪敜閱掋€?銆岃处鍙风鐞嗐€嶆寜閽?- 鎴浘 2锛堥拱瑙掔櫥褰曞脊绐楋級锛氥€?92****6952 (鏈€杩?銆?銆岀櫥褰曘€?- 鎴浘 3锛堣处鍙峰垪琛級锛?*3 涓处鍙疯**锛?  - 192****6952 (鏈€杩?
  - 192****6952 (涓婃鐧诲綍 9鍒嗛挓鍓?
  - 189****0830 (涓婃鐧诲綍 39鍒嗛挓鍓?

**鐢ㄦ埛鎻愮ず鍏抽敭宸紓**锛?- 銆屼笂娆＄櫥褰曘€嶅瓧鏍凤細缁忓父鐧诲綍鐨勮处鍙锋樉绀?- 銆岀櫥褰曡褰曘€嶅瓧鏍凤細闀挎椂闂翠笉鐧婚檰鐨勮处鍙锋樉绀?
**淇**锛歚AccountManagerOfficial.text` 涓?`AccountManagerBili.text` 浠庡崟鏂囨湰 `["鐧诲綍璁板綍"]` 鏀逛负鍙屾枃鏈?`["鐧诲綍璁板綍", "涓婃鐧诲綍"]`锛岃鐩栨柊鏃?UI 涓ょ鏄剧ず妯″紡銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `resource/tasks/tasks.json:813-824` | 淇敼 | `AccountManagerOfficial` 涓?`AccountManagerBili` 鐨?`text` 浠?`["鐧诲綍璁板綍"]` 鈫?`["鐧诲綍璁板綍", "涓婃鐧诲綍"]`锛汥oc 鍚屾鏇存柊 |
| 2 | `AGENTS.md 搂6` | 淇敼 | `fix/expedite-threshold` 瑙掕壊鎻忚堪杩藉姞銆?璐﹀彿鍒楄〃 OCR 閫傞厤 UI 鏀圭増锛堢櫥褰曡褰?/ 涓婃鐧诲綍 鍙屾枃鏈厹搴曪級銆?|
| 3 | `LOG.md` | 淇敼 | 鏈妭 |

**琛屼负鐭╅樀**锛?
| 璐﹀彿浣跨敤棰戠巼 | 鏄剧ず鏂囨湰 | 鏀瑰墠 | 鏀瑰悗 |
|-------------|---------|------|------|
| 缁忓父鐧诲綍 | 銆屼笂娆＄櫥褰?X 鍒嗛挓鍓嶃€?| 鉁?OCR 涓嶅懡涓?鈫?30 retry 澶辫触 | 鉁?鍛戒腑 |
| 闀挎椂闂存湭鐧诲綍 | 銆岀櫥褰曡褰曘€?| 鉁?鍛戒腑锛堜繚鐣欙級 | 鉁?鍛戒腑 |
| 娣峰悎璐﹀彿鍒楄〃 | 鍚屾椂鍑虹幇涓ょ | 閮ㄥ垎鍛戒腑 | 鍏ㄩ儴鍛戒腑 |

**椋庨櫓璇勪及**锛?- 鍙屾枃鏈鍖归厤锛歚fullMatch: true` 淇濈暀 + ROI `[237, 50, 771, 242]` 闄愬畾椤堕儴鏍囬鏍忥紱鍏朵粬椤甸潰鏃犮€岀櫥褰曡褰曘€?銆屼笂娆＄櫥褰?X 鍒嗛挓鍓嶃€嶅瓧鏍?- 涓?upstream 鍋忕锛歶pstream 浠嶅崟鏂囨湰 `["鐧诲綍璁板綍"]`锛涙湰 fork 鍥?UI 鏀圭増閫傞厤锛?*涓嶆帹 upstream**
- ROI 寰皟锛氬綋鍓?ROI 涓庡師鐗堜竴鑷达紱濡傛柊 UI 璐﹀彿浣嶇疆瓒呭嚭 ROI 鍐嶅崟鐙皟鏁?
**棰勬湡鏁堟灉**锛?- 澶氳处鍙峰垏鍙?+ 涓昏彍鍗曪紙鎴浘 1锛夆啋 鐐瑰嚮銆岃处鍙风鐞嗐€嶁啋 璐﹀彿鍒楄〃锛堟埅鍥?3锛夆啋 OCR 鍛戒腑銆屼笂娆＄櫥褰曘€嶁啋 閫夊彿 鈫?銆岀櫥褰曘€嶁啋 home
- 鍏ㄧ▼ 30-40s 鍐呭畬鎴愶紝鏃?20s 绛夊緟

**鎺?upstream**锛氫粎鏈?fork 淇锛屼笉鎺ㄣ€?
### fix/expedite-threshold StartUp::run 鎭㈠鍘熷簭 + 鍒囧彿閾?OCR 鍏滃簳

**淇涓婁竴涓?commit (`3f411e494a`)**銆備笂涓€鐗堝皢 `StartUpTask::run` 閲嶆帓涓?`start_game 鈫?start_up 鈫?account_switch 鈫?start_up`锛屽湪 StartUpTask 灞傛樉寮忚皟鐢?`start_up` 澶勭悊鐧诲綍鍓嶇殑楣拌寮圭獥銆備絾鐢ㄦ埛鍙嶉璇ユ灦鏋勮繃浜庝镜鍏ュ紡鈥斺€斿垏鍙峰簲璇ャ€屽厛杩涘叆璐﹀彿绠＄悊鍐嶅垏璐﹀彿銆嶏紝鐢?`AccountSwitchTask::navigate_to_start_page` 鍐呴儴璐熻矗瀵艰埅鍒?AccountManager锛岃€岄潪鍦?StartUpTask 灞傛媶鍑虹櫥褰曟楠ゃ€?
**鏂版灦鏋勶紙鎭㈠鍒囧彿鍘熷簭 + 鍒囧彿閾捐ˉ OCR 鍏滃簳锛?*锛?
```cpp
// StartUpTask::run 涓绘祦绋嬶紙鎭㈠鍘熷簭锛?start_game 鈫?account_switch 鈫?start_up

// restart_game 寰幆锛堟仮澶嶅師搴忥級
restart_game 鈫?account_switch 鈫?start_up
```

鍒囧彿 `navigate_to_start_page` 鍐呴儴璧?`SwitchAccount@StartUpBegin` 閾撅紝鍘熼摼涓?`SwitchAccount@StartToWakeUp` 浠呮ā鏉垮尮閰嶏紙`tasks.json:789-794`锛夛紝**缂?OCR 鍏滃簳**鈥斺€斿綋娓告垙鍋滃湪楣拌鐧诲綍寮圭獥锛坄StartToWakeUp.png` 涓嶅尮閰嶏級鏃舵棤 fallback锛屽鑷?30 retry 鍏ㄥけ璐ャ€?x restart_game 姝诲惊鐜€?0s 绛夊緟銆?
**淇**锛氬弬鑰?`StartUpThemes` 鐨?`StartToWakeUp` + `StartToWakeUpOCR` 閰嶅妯″紡锛岃鍒囧彿閾句篃鏀寔 OCR 鍏滃簳锛?
```json
"SwitchAccount@StartToWakeUp": {
    "template": "StartToWakeUp.png",
    "action": "DoNothing",
    "next": ["AccountManager", "SwitchAccount@StartToWakeUpOCR"]   // 鏂板 OCR 鍏滃簳
},
"SwitchAccount@StartToWakeUpOCR": {
    "baseTask": "SwitchAccount@StartToWakeUp",
    "algorithm": "OcrDetect",
    "text": ["寮€濮嬪敜閱?, "鐧诲綍", "鐧?, "褰?],
    "fullMatch": true,
    "roi": [373, 145, 535, 430]
}
```

`next` 椤哄簭锛歚AccountManager` 鍦ㄥ墠锛堟ā鏉垮尮閰嶆椂鐩存帴璧帮級锛宍SwitchAccount@StartToWakeUpOCR` 鍦ㄥ悗锛堝厹搴曪級锛涗笌 `StartUpThemes` 涓€鑷淬€?
**4 绫诲満鏅涓虹煩闃?*锛?
| 鍦烘櫙 | 涓婁竴涓?commit (`3f411e494a`) | 鏈?commit |
|------|------------------------------|----------|
| 澶氳处鍙峰垏鍙?+ 楣拌鐧诲綍寮圭獥 | 鏄惧紡 start_up 鐐广€岀櫥褰曘€嶁啋 鍒囧彿 鈫?鍐嶆鐧诲綍 | 鍒囧彿閾?OCR 鍛戒腑銆岀櫥褰曘€嶁啋 鐐瑰嚮 鈫?AccountManager 鈫?鍒囧彿 鈫?鐧诲綍 |
| 澶氳处鍙峰垏鍙?+ 宸插湪涓荤晫闈?| 鏄惧紡 start_up 璧板畬 鈫?鍒囧彿 鈫?鍐嶆鐧诲綍 | 鍒囧彿閾剧洿鎺?Settings 鈫?Account 鈫?鍒囧彿 鈫?鐧诲綍锛堝皯涓€娆?start_up锛?|
| 鍗曡处鍙?+ 楣拌鐧诲綍寮圭獥 | start_up 鍏堢櫥褰?鈫?鍒囧彿锛坉isabled锛夆啋 鍐嶆鐧诲綍 | 鍒囧彿閾?OCR 鍛戒腑 鈫?AccountManager锛坉isabled锛夆啋 鍒囧彿锛坉isabled锛夆啋 鐧诲綍 |
| 鍗曡处鍙?+ 宸插湪涓荤晫闈?| 鏄惧紡 start_up 鈫?鍒囧彿锛坉isabled锛夆啋 鍐嶆鐧诲綍 | 鍒囧彿閾?Settings 鈫?Account锛坉isabled锛夆啋 鍒囧彿锛坉isabled锛夆啋 鐧诲綍 |

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Interface/StartUpTask.cpp:30-81` | 鏀瑰洖鍘熷簭 | `start_game 鈫?account_switch 鈫?start_up`锛況estart 寰幆涔熸敼鍥炲師搴?|
| 2 | `resource/tasks/tasks.json:789-803` | 淇敼 | `SwitchAccount@StartToWakeUp.next` 杩藉姞 `SwitchAccount@StartToWakeUpOCR` 鍏滃簳锛涙柊澧?`SwitchAccount@StartToWakeUpOCR` OCR 浠诲姟 |
| 3 | `AGENTS.md 搂6` | 淇敼 | `fix/expedite-threshold` 瑙掕壊鎻忚堪鏇存柊 |
| 4 | `LOG.md` | 淇敼 | 鏈妭 |

**椋庨櫓璇勪及**锛?- 缂栬瘧椋庨櫓锛?
- 琛屼负鍥炲綊锛氬師搴?= 涓?upstream 鏋舵瀯涓€鑷达紱鍗曡处鍙?/ 澶氳处鍙?/ 宸插湪涓荤晫闈㈠満鏅潎宸茶鐩?- OCR 璇瘑鍒闄╋細`SwitchAccount@StartToWakeUpOCR.fullMatch: true` 闄愬畾璇嶈〃銆屽紑濮嬪敜閱?鐧诲綍/鐧?褰曘€嶏紱`next` 椤哄簭鍏堟ā鏉垮悗 OCR锛屾ā鏉垮尮閰嶆椂鐩存帴璧?AccountManager锛屼笉浼氳Е鍙?OCR
- ROI 寰皟锛氬綋鍓?ROI `[373, 145, 535, 430]` 涓?`StartUpThemes@StartToWakeUpOCR` 涓€鑷达紱濡傞拱瑙掑脊绐椼€岀櫥褰曘€嶆寜閽綅缃秴鍑?ROI锛屽啀鍗曠嫭璋冩暣

**棰勬湡鏁堟灉**锛?- 澶氳处鍙峰垏鍙?+ 楣拌鐧诲綍寮圭獥锛?0-40s锛堝惈 20s 绛夊緟锛夆啋 **20-30s**锛堟秷闄?20s 绛夊緟锛?- 鍗曡处鍙?/ 宸插湪涓荤晫闈㈠満鏅細涓庡師搴忕瓑浠?
**鎺?upstream**锛氫粎鏈?fork 淇锛屼笉鎺ㄣ€?
### fix/expedite-threshold StartUp::run 閲嶆帓锛氬厛鐧诲綍鍐嶅垏鍙?
澶氳处鍙峰垏鍙峰満鏅疄娴嬪彂鐜?20+ 绉掔瓑寰咃細娓告垙鍚姩鍚庡仠鍦ㄩ拱瑙掔櫥褰曞脊绐楋紙HyperGryph server auth popup锛屾樉绀?`192****6952` + 銆岀櫥褰曘€嶆寜閽級锛孧AA 鐨?`AccountSwitchTask::navigate_to_start_page` 绔嬪嵆灏濊瘯鍒囧彿锛屼絾 `SwitchAccount@StartUpBegin` 閾撅紙`tasks.json:729-737`锛夊彧璇嗗埆**娓告垙鍐呰处鍙风鐞嗙晫闈?*锛坄AccountManagerOfficial` / `AccountManagerBili` / `Txwy`锛夛紝涓嶈瘑鍒拱瑙掔櫥褰曞脊绐楋紱30 retry 鍏ㄩ儴澶辫触鍚庤繘鍏?`Login failed, entering game-restart loop` 5x restart_game 姝诲惊鐜€?
瀹炴祴鏃ュ織璇佹嵁锛坄install/debug/asst.log` 13:47:40-13:47:54锛夛細

```
13:47:45  GameStartCheckResourceOCR 鍛戒腑 (5s postDelay)
13:47:51  GameStart.png 鍛戒腑 鈫?click (628, 685)
13:47:54  last matched task: SwitchAccount@GameStart
13:47:54  WRN "Account switch failed after restart, retrying game restart"
```

**鏍瑰洜锛堟灦鏋勯棶棰橈紝闈炴€ц兘闂锛?*锛歚StartUpTask::run` 椤哄簭涓?`start_game 鈫?account_switch 鈫?start_up`锛?*鍋囪娓告垙宸茬櫥褰曞埌涓荤晫闈?*銆傚綋娓告垙鍦ㄩ拱瑙掔櫥褰曞脊绐楁椂锛?- `m_account_switch_task_ptr->run()` 绔嬪嵆澶辫触锛堟壘涓嶅埌 AccountManager 椤甸潰锛?- 鏁翠釜 StartUp 浠诲姟杩涘叆 5x restart_game 寰幆锛屾瘡娆?restart 鍚屾牱鐨勪簨鎯呭啀鍙戠敓

**淇**锛氶噸鎺掍负 `start_game 鈫?start_up 鈫?account_switch 鈫?start_up`锛屽厛纭繚娓告垙鐧诲綍鍒颁富鐣岄潰锛屽啀鍒囧彿锛屽啀鐧诲綍銆俽estart_game 寰幆鍐呬篃鐩稿簲鍏?`m_start_up_task_ptr->run()` 鍚?`m_account_switch_task_ptr->run()`銆?
`cherry-pick 784d9005f6`锛坄AccountManagerOfficial` 娈嬬己 OCR 琛ュ叏锛夎В鍐崇殑鏄€屽垏鍙峰満鏅?30 retry 鎵句笉鍒?AccountManager銆嶇殑涓嶅悓瀛愰棶棰橈紝**娌℃湁鏀瑰彉鏋舵瀯椤哄簭**锛涙湰 commit 鎵嶆槸**鏋舵瀯灞傞潰鐨勪慨澶?*銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Interface/StartUpTask.cpp:30-81` | 淇敼 | `StartUpTask::run` 閲嶆帓锛歚start_game 鈫?start_up 鈫?account_switch 鈫?start_up`锛?x restart_game 寰幆鍐呭悓鏍锋敼涓哄厛鐧诲綍鍐嶅垏鍙?|
| 2 | `AGENTS.md 搂6` | 淇敼 | `fix/expedite-threshold` 瑙掕壊鎻忚堪杩藉姞銆?StartUp::run 閲嶆帓锛堜慨鍒囧彿鍓嶆湭鐧诲綍鐨?20s 绛夊緟锛夈€?|
| 3 | `LOG.md` | 淇敼 | 鏈妭 |

**4 绫诲満鏅涓虹煩闃?*锛?
| 鍦烘櫙 | 鏀瑰墠 | 鏀瑰悗 |
|------|------|------|
| 澶氳处鍙峰垏鍙?+ 楣拌鐧诲綍寮圭獥 | 鍒囧彿澶辫触 鈫?5x restart 鈫?澶辫触锛?0s 绛夊緟锛?| 鑷姩鐧诲綍 鈫?鍒囧彿 鈫?鍐嶆鐧诲綍 鈫?鎴愬姛 |
| 澶氳处鍙峰垏鍙?+ 宸插湪涓荤晫闈?| 鍒囧彿 鈫?鐧诲綍 鈫?鎴愬姛 | fast-path 鐧诲綍 鈫?鍒囧彿 鈫?鐧诲綍 鈫?鎴愬姛锛堝 0.5s fast-path 寮€閿€锛?|
| 鍗曡处鍙?+ 楣拌鐧诲綍寮圭獥 | 鍒囧彿锛坉isabled锛夆啋 鐧诲綍 鈫?鎴愬姛 | 鐧诲綍 鈫?鍒囧彿锛坉isabled锛夆啋 fast-path 鐧诲綍 鈫?鎴愬姛锛堝 0.5s锛?|
| 鍗曡处鍙?+ 宸插湪涓荤晫闈?| 鍒囧彿锛坉isabled锛夆啋 鐧诲綍 鈫?鎴愬姛 | 鐧诲綍锛坒ast-path 璺宠繃锛夆啋 鍒囧彿锛坉isabled锛夆啋 鐧诲綍锛坒ast-path锛夆啋 鎴愬姛锛堝 0.5s锛?|

**椋庨櫓璇勪及**锛?- 缂栬瘧椋庨櫓锛?锛堜粎璋冩暣鎺у埗娴侊級
- 鍗曡处鍙锋祦绋嬶細澶?0.5s fast-path 寮€閿€锛圫tartAtHome 妯℃澘鍛戒腑鍗宠烦杩囷級
- 鍒囧彿娴佺▼锛歠ast-path 鍛戒腑 鈫?璺宠繃 `start_up` 浠诲姟閾?鈫?鐩存帴璧板垏鍙?鈫?0 褰卞搷
- 涓?upstream 鍋忕锛歶pstream 浠?`鍒囧彿浼樺厛浜庣櫥褰昤 鏋舵瀯锛涙湰 fork 鍥犲璐﹀彿鍦烘櫙淇
- 鎺?upstream锛氫粎鏈?fork 淇锛屼笉鎺?
**棰勬湡鏁堟灉**锛?- 澶氳处鍙峰垏鍙峰満鏅細30-40s 鈫?**20-30s**锛堟秷闄?20s 绛夊緟锛?- 鍗曡处鍙峰満鏅細5-20s 鈫?3-15s锛圫tartAtHome fast-path 澶氳蛋涓€娆★紝鍙拷鐣ワ級

**鎵嬪姩楠屾敹**锛?1. 澶氳处鍙峰垏鍙?+ 楣拌鐧诲綍寮圭獥 鈫?鍚姩 MAA 鈫?瑙傚療鑷姩鐐广€岀櫥褰曘€嶁啋 杩涗富鐣岄潰 鈫?鍒囧彿 鈫?鍐嶆鐧诲綍
2. 澶氳处鍙峰垏鍙?+ 宸插湪涓荤晫闈?鈫?鍚姩 MAA 鈫?鐩存帴鍒囧彿
3. 鍗曡处鍙?鈫?鍚姩 MAA 鈫?姝ｅ父鐧诲綍

### fix/expedite-threshold StartUp 鍙岄噸缂撳啿娓呯悊

璋冪爺 `upstream/dev-v2` 涓庢湰浠撳簱瀵规瘮鍚庣‘璁わ細`src/MaaCore/Task/Interface/StartUpTask.cpp:24` 鐨?`.set_task_delay(Config.get_options().task_delay * 2)` 鍙岄噸缂撳啿鍦ㄩ粯璁?`task_delay=0`锛坄GeneralConfig.h:34`锛変笅鏃犱换浣曟晥鏋滐紙`0*2=0`锛夛紝涓斾笌 upstream `dev-v2` 涓€鑷达紙鏃?PR 鎺ㄥ姩璋冩暣锛夈€傛湰娆℃竻鐞嗙函绮规槸銆屽垹鏃犳剰涔変唬鐮併€嶏紝涓嶄慨鏀逛换浣?`postDelay` / `preDelay` / `retry_times` / ROI / OCR 绠楁硶锛岄伒寰€岀ǔ瀹氫紭鍏堛€嶅師鍒欙紝follow upstream 鍩虹嚎銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Interface/StartUpTask.cpp:24` | 淇敼 | `.set_task_delay(Config.get_options().task_delay * 2)` 鈫?`.set_task_delay(Config.get_options().task_delay)`锛屽垹 `* 2` |
| 2 | `AGENTS.md 搂6` | 淇敼 | `fix/expedite-threshold` 瑙掕壊鎻忚堪杩藉姞 `;StartUp 鍙岄噸缂撳啿娓呯悊` |
| 3 | `LOG.md` | 淇敼 | 鏈妭 |

**琛屼负鍙樺寲**锛氶粯璁?`task_delay=0` 鏃舵柊鏃у畬鍏ㄧ瓑浠凤紙`0*2==0`锛夛紱浠呭綋鐢ㄦ埛鍦?WPF 鎶?`task_delay` 璋冨埌 >0 鏃讹紝StartUp 闃舵涓嶅啀姣旀棩甯镐换鍔″绛変竴鍊嶏紝鏇寸鍚堢洿瑙夈€?
**棰勬湡鏁堟灉**锛氭棤浠讳綍鍙娴嬬殑杩愯鏃跺樊寮傦紱浠呮竻鐞嗕竴琛屾棤鎰忎箟浠ｇ爜 + 鏂囨。鍚屾銆?
**椋庨櫓璇勪及**锛?- 缂栬瘧椋庨櫓锛?锛堝垹 4 瀛楃锛?- 杩愯鏃跺洖褰掞細0锛堥粯璁?task_delay=0 绛変环锛?- 宸叉湁鐢ㄦ埛閰嶇疆锛氫粎褰卞搷鎵嬪姩璋冮珮 task_delay 鐨勭敤鎴凤紝琛屼负鏇寸洿瑙?- 鍥為€€鎴愭湰锛歚git revert` 鍗?commit 鍗宠繕鍘?
**鏈仛椤?*锛堟槑纭帓闄わ紝閬靛惊 upstream 鍩虹嚎锛夛細
- `GameStartCheckResourceOCR.postDelay: 5000` / `GameStartUpdateOCR.postDelay: 5000` / `LoginOther.preDelay: 3000` 绛?tasks.json 寤惰繜
- `set_retry_times(50)` / `set_retry_times(30)` 鍒囧彿閲嶈瘯涓婇檺
- `MaxRestartAttempts=5` 閲嶅惎寰幆
- ROI 缂╃獎 / OCR 绠楁硶璋冩暣

### fix/account-official-recognize cherry-pick 鍚屾鍒?fix/expedite-threshold

`fix/expedite-threshold`锛圚EAD `301f90897a`锛宐ranch point = `9d8d021610`锛塨ranch point 鏃╀簬 `branch` 涓婁粖澶?12:46 鐨勫畼鏂规湇璐﹀彿鍒囨崲璇嗗埆琛ュ叏 `784d9005f6`锛屽鑷磋鍒嗘敮閮ㄧ讲鐨?`install/MaaCore.dll` 浠嶅甫 `AccountManagerOfficial` 娈嬬己瀹氫箟 bug 鈥斺€?瀹樻湇 + 璐﹀彿杞崲鍦烘櫙涓?`ProcessTask` 30 娆?retry 鍏ㄥけ璐ワ紝杩?`Login failed, entering game-restart loop` 鍗″湪鐧诲綍椤点€傚疄娴嬬幆澧冿紙MAA 涓荤晫闈㈡棩蹇?12:19: `StartToWakeUp.png` 鍛戒腑銆佺櫥褰曢〉 OCR 涓嶈瘑鍒級纭澶嶇幇銆?
upstream `MaaAssistantArknights/MaaAssistantArknights` `dev-v2` 浠嶅甫鍚屾 bug锛屾棤瀵瑰簲 PR锛涙湰 fork `branch` 棰嗗厛 upstream銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `resource/tasks/tasks.json:805-810` | cherry-pick from `784d9005f6` | `AccountManagerOfficial` 鐢?`{"roi":[570,165,140,80]}` 琛ュ叏涓?`{"Doc":"...","algorithm":"OcrDetect","text":["鐧诲綍璁板綍"],"roi":[237,50,771,242]}`锛堜笌 `AccountManagerBili` 瀵归綈锛?|
| 2 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:68-83` | cherry-pick from `784d9005f6` | `navigate_to_start_page()` 鍔?`Log.info(... last matched task ...)` 璇婃柇鏃ュ織锛? 涓?`else if` 鍚堝苟涓哄崟 `if (... \|\| ... \|\| ... \|\| ...)` |
| 3 | `LOG.md` | cherry-pick from `784d9005f6` | 鍚屾 `### fix/account-official-recognize 鍚姩` 涓?`### fix/account-official-recognize 瀹炴柦瀹屾垚` 涓よ妭锛涘啿绐佽В鍐筹細淇濈暀鏈垎鏀?`fix/expedite-threshold` 涓よ妭锛岃拷鍔犳柊涓よ妭锛堟棤鍐呭涓㈠け锛?|
| 4 | `AGENTS.md 搂7.5` | cherry-pick from `da157d163d` | 鏂板 `fix/account-official-recognize` 鐢熷懡鍛ㄦ湡璁板綍灏忚妭 |
| 5 | `AGENTS.md 搂6` | 涓嶅彉 | `fix/expedite-threshold` 浠嶄负杩涜涓垎鏀紙鏈悎鍏?branch锛夛紝寰呮湰鍒嗘敮鍚堝叆 `branch` 鏃跺啀娓呴櫎 |
| 6 | `LOG.md` | 淇敼 | 鏈妭锛坈herry-pick 鍚屾浜嬩欢璇存槑锛?|

**Commit 閾?*锛?
| SHA | 鏉ユ簮 | 鏍囬 |
|-----|------|------|
| `6011051af2` | cherry-pick from `784d9005f6` | `fix(startup): 瀹樻柟鏈嶈处鍙峰垏鎹㈢晫闈㈣瘑鍒ˉ鍏?+ 鍒囧彿璇婃柇鏃ュ織` |
| `f241b2160b` | cherry-pick from `da157d163d` | `docs: 鐧昏 fix/account-official-recognize 鍒嗘敮鐢熷懡鍛ㄦ湡` |

**浣滅敤鍩熷０鏄?*锛?- 涓?`搂7.5 fix/account-official-recognize` 鍚屽悕涓嶅悓鍒嗘敮鐢熷懡鍛ㄦ湡鈥斺€旀湰鑺傝褰曠殑鏄€宍branch` 涓婃父淇 cherry-pick 鍒?`fix/expedite-threshold`銆嶇殑鍚屾浜嬩欢锛屼笉閲嶅鐧昏涓婃父鐢熷懡鍛ㄦ湡銆?- 搂6 `fix/expedite-threshold` 浠嶆爣杩涜涓紱寰呯敤鎴峰悗缁喅瀹?FF / `--no-ff` 鍚堝叆 `branch` 鏃朵竴骞舵竻闄ゃ€?
## 2026-07-15

### 鍒嗘敮宸ヤ綔鏂囨。绾︽潫璋冩暣

LOG.md / AGENTS.md 浠?`.gitignore` 绉婚櫎锛屾敼涓烘墍鏈夊垎鏀窡韪紱feat*.md / fix*.md 淇濈暀 gitignore锛屾案涓嶆彁浜ゃ€?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `.gitignore:515-516` | 鍒犻櫎 | 绉婚櫎 `LOG.md`銆乣AGENTS.md` 蹇界暐瑙勫垯 |
| 2 | `.gitignore:514` | 淇敼 | 娉ㄩ噴鏀逛负 `# Feature/fix working documents (local only, never committed)` |
| 3 | `LOG.md` | 淇敼 | 鏈妭 |

### feat/expedite-threshold 鍚姩

鍏嫑鍔犳€?`expedite`)鍘熸湰鍙湁"寮€/鍏?涓ょ鐘舵€侊紝涓庢槦绾у垽鏂畬鍏ㄨ劚閽┿€傛柊澧?*鍔犳€ラ棬妲?*鏈哄埗:鍙粎鍦ㄦ湰娆＄‘璁ょ殑鎷涘嫙缁勫悎鏈€浣庢槦绾?鈮?4/5/6 鏃舵墠浣跨敤鍔犳€ヨ鍙紝3鈽?涓嶆氮璐瑰姞鎬ヨ鍙€?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `feat/expedite-threshold` | 鏂板缓鍒嗘敮 | 浠?`branch` 鎷夊嚭 |
| 2 | `feat_expedite-threshold.md` | 鏂板缓 | 宸ヤ綔鏂囨。(宸?.gitignore) |

### feat/expedite-threshold 瀹炴柦瀹屾垚

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h:28, 173, 186` | 淇敼 | +1 setter 澹版槑 `set_expedite_min_level`;+2 鎴愬憳 `m_expedite_min_level`(榛樿 0)銆乣m_last_confirmed_min_level`(榛樿 0) |
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:147-152` | 淇敼 | +1 setter 瀹炵幇 `set_expedite_min_level` |
| 3 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:228-303` | 淇敼 | `_run` 涓诲惊鐜Щ闄?`try_use_expedited` 灞€閮ㄥ彉閲?鍔犳€ュ垽瀹氭敼涓?*姣忔杩涘叆鍓嶉噸鏂版眰鍊?* `m_use_expedited && m_last_confirmed_min_level >= m_expedite_min_level`;鍔犳€ユ垚鍔熷悗绔嬪嵆閲嶇疆 `m_last_confirmed_min_level = 0` 闃叉闄堟棫鐘舵€佽澶嶇敤;鍔犳€ュけ璐ユ椂鏄惧紡閫€鍑轰互閬垮厤闃堝€?0 鏃舵寰幆 |
| 4 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:340-347` | 淇敼 | `recruit_one` 寮€澶撮噸缃?`m_last_confirmed_min_level = 0`,浠呭綋 `recruit_calc_task` 璧板埌 success / nothing_to_select 璺緞鏃舵墠浼氳閲嶆柊璧嬪€?|
| 5 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:740-743, 770-772` | 淇敼 | `recruit_calc_task` 鍦?`nothing_to_select` 涓?`success` 涓ゆ潯杩斿洖璺緞鍓嶈祴鍊?`m_last_confirmed_min_level = final_combination.min_level` |
| 6 | `src/MaaCore/Task/Interface/RecruitTask.cpp:54, 87` | 淇敼 | +1 鍙傛暟瑙ｆ瀽 `expedite_min_level`(榛樿 0);閾惧紡璋冪敤閫忎紶缁?AutoRecruitTask |
| 7 | `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs:38-42` | 淇敼 | +1 瀛楁 `ExpediteMinLevel`(榛樿 0) |
| 8 | `src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs:69-72, 161` | 淇敼 | +1 瀛楁 `ExpediteMinLevel`;`Serialize` 濮嬬粓鍐欏叆 `expedite_min_level` 鍒?params |
| 9 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs:142-174, 329` | 淇敼 | +3 VM 鎴愬憳:`ExpediteMinLevelEnabled`(甯冨皵,setter 鎺у埗 0/4 鍒囨崲)銆乣ExpediteMinLevel`(int,setter 鐧藉悕鍗?0/4/5/6)銆乣ExpediteMinLevelOptions`(4/5/6 涓夋。 ComboBox 閫夐」);Serialize 闃舵鍐欏叆 `ExpediteMinLevel` |
| 10 | `src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml:162-182` | 淇敼 | 楂樼骇璁剧疆鍖烘湯灏捐拷鍔?CheckBox + ComboBox;鏁磋 Visibility 缁戝畾鍒?`UseExpeditedWithNull == true` |
| 11 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:1474-1479` | 淇敼 | +5 string key:`ExpediteMinLevelLabel` / `ExpediteMinLevelTip` / `ExpediteMinLevel_4Plus` / `ExpediteMinLevel_5Plus` / `ExpediteMinLevel_6Plus` |
| 12 | `docs/zh-cn/protocol/integration.md:263-276` | 淇敼 | +1 瀛楁璇存槑 `expedite_min_level`,鍚?0/4/5/6 璇箟 |
| 13 | `feat_expedite-threshold.md` | 淇敼 | 杩藉姞绔犺妭 涓?鍥?浜?璁板綍瀹炴柦缁撴灉涓庤俯鍧?|
| 14 | `LOG.md` | 淇敼 | 鏈妭 |

**缂栬瘧/閮ㄧ讲缁撴灉**: (寰呰ˉ鍏?

**鍏煎鎬ф牳鏌?*:
- 鏃?API 鐢ㄦ埛涓嶄紶 `expedite_min_level` 鈫?C++ 绔粯璁?0 = 涓嶉檺 鈫?鍏ㄥ姞鎬?琛屼负涓嶅彉
- 鏃?GUI 鐢ㄦ埛閰嶇疆鏂囦欢涓棤璇ュ瓧娈?鈫?JSON 鍙嶅簭鍒楀寲榛樿 0 + CheckBox 鏈嬀閫?鈫?鍏ㄥ姞鎬?琛屼负涓嶅彉
- 鏂扮敤鎴烽娆″惎鍔?鈫?CheckBox 鏈嬀閫?榛樿 0) 鈫?鍏ㄥ姞鎬?绛夊悓鏃ц涓?
**寰呮墜鍔ㄩ獙璇?(闇€妯℃嫙鍣ㄧ幆澧?**:
1. 鍑嗗 4鈽?缁勫悎 鈫?閫?Tag 鈫?纭 鈫?绔嬪嵆瀹屾垚 鈫?寰幆鍒颁笅涓€鏍?2. 鍑嗗 5鈽?缁勫悎 鈫?閫?Tag 鈫?纭 鈫?绔嬪嵆瀹屾垚 鈫?寰幆鍒颁笅涓€鏍?3. 鍑嗗 3鈽?缁勫悎 鈫?閫?Tag 鈫?纭 鈫?绛夊緟 9 灏忔椂(涓嶅姞鎬?
4. 鍏虫帀"鑷姩鍔犳€? 鈫?鎵€鏈夋爮浣嶈蛋鑷劧鍊掕鏃?5. 涓嬫媺妗嗘敼 "5鈽?" 鈫?4鈽?鏍忎綅璧拌嚜鐒跺€掕鏃?5鈽? 绔嬪嵆瀹屾垚
6. 涓嬫媺妗嗘敼 "6鈽?" 鈫?4鈽?5鈽?閮借蛋鑷劧鍊掕鏃?6鈽?绔嬪嵆瀹屾垚

## 2026-07-15

### feat/defer-rogue 鍚姩

鍚敤璐﹀彿杞崲鏃舵妸鑲夐附 (Roguelike) 涓庣敓鎭紨绠?(Reclamation) 寤跺悗鍒版墍鏈夎处鍙峰熀纭€浠诲姟瀹屾垚鍚庢墽琛屻€傛墽琛岄『搴? A-1 鈫?B-1 鈫?A-2 鈫?B-2 (璺ㄨ处鍙疯疆杞?Phase)銆傞粯璁ゅ叧闂互淇濇寔鍚戝悗鍏煎銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `feat/defer-rogue` | 鏂板缓鍒嗘敮 | 浠?`branch` 鎷夊嚭,鏈湴宸ヤ綔鍒嗘敮 |
| 2 | `feat_defer-rogue.md` | 鏂板缓 | 宸ヤ綔鏂囨。(宸?.gitignore) |

### feat/defer-rogue 瀹炴柦瀹屾垚

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs:36-44` | 淇敼 | 鏂板 `LateStageRogueAndReclamation : bool = false`,榛樿鍏抽棴浠ヤ繚鎸佸悜鍚庡吋瀹?|
| 2 | `src/MaaWpfGui/Models/AccountCycleStep.cs` | 鏂板缓 | `record AccountCycleStep(string AccountName, int Phase)`,姝ラ鎵佸钩鍒楄〃鐨勮浇浣?|
| 3 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs:95-176` | 淇敼 | (a) `LateStageRogueAndReclamation` VM 灞炴€?鐓ф惉 `AccountSwitchEnabled`);(b) 鏂板 `#region Late Stage` 鍚?`_cycleSteps` / `_currentStepIndex` / `RebuildCycleSteps` / `AdvanceStepIndex` / `CurrentStep` / `GetPreviousStep` / `CurrentPhase`;(c) `ResetCycle` 鍚屾娓呯┖姝ラ鍒楄〃 |
| 4 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1813-1851` | 淇敼 | `LinkStart` 鏀逛负 `RebuildCycleSteps` + 鍙?`CurrentStep` 鍐冲畾棣栦釜璐﹀彿 |
| 5 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1939-1982` | 淇敼 | `LinkStartWithTasks` foreach 鏂板 Phase 杩囨护(`IsInCurrentPhase` 鐢?`lateStageOn` 闂搁棬,LateStage 鍏抽棴鏃?no-op) |
| 6 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2172-2355` | 淇敼 | `AdvanceAccountCycle` 鍏ㄩ噺閲嶅啓:鎵佸钩姝ラ鎺ㄨ繘 + `needStartupSwitch` 鏄惧紡鍒囧彿 + 绌烘楠ら€掑綊璺宠繃 + `MarkAccountCompleted` 鎸?LateStage 鐘舵€佸樊寮傚寲瑙﹀彂 |
| 7 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2359-2367` | 淇敼 | 鏂板闈欐€佸姪鎵?`IsInCurrentPhase(TaskType, int phase)` |
| 8 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml:134-149` | 淇敼 | AccountCycle 瀛愰潰鏉挎湯灏炬柊澧?CheckBox + TooltipBlock(闀?Wrap + MaxWidth CalcBinding 闃叉尋鍘? |
| 9 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:696-697` | 淇敼 | +2 string key:`LateStageRogueAndReclamation` / `LateStageRogueAndReclamationTip` |
| 10 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml:696-697` | 淇敼 | 鍚屼笂(绻佷綋) |
| 11 | `src/MaaWpfGui/Res/Localizations/en-us.xaml:696-697` | 淇敼 | 鍚屼笂(鑻辨枃) |
| 12 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml:696-697` | 淇敼 | 鍚屼笂(鏃ユ枃) |
| 13 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml:696-697` | 淇敼 | 鍚屼笂(闊╂枃) |
| 14 | `install/MAA.dll` | 鏇存柊 | `dotnet publish` 閮ㄧ讲鏃堕棿 2026-07-15 11:48 |
| 15 | `install/MaaCore.dll`, `install/MaaUtils.dll`, `install/MAA.Updater.exe` | 鏇存柊 | `cmake --install` 閮ㄧ讲 |
| 16 | `feat_defer-rogue.md` | 淇敼 | 杩藉姞绔犺妭 涓?鍥?浜?璁板綍瀹炴柦缁撴灉涓庤俯鍧?|
| 17 | `LOG.md` | 淇敼 | 鏈妭 |

**缂栬瘧/閮ㄧ讲缁撴灉**: `dotnet build -c Release` 0 error, 3 warning (SA1503, 涓庡師鐗堝悓娆惧啓娉?;`cmake --build` + `cmake --install` + `dotnet publish` 鍏ㄩ儴閫氳繃;鏈€缁?`install/MAA.dll` 鏃堕棿鎴充负浠婃棩銆?
**韪╁潙**:
- cmake 瑙﹀彂 WPF MSBuild 璇勪及鏃舵挒鍒?LOG.md 璁板綍鐨?VS2026 SDK 璺緞 bug,缁曠敤 standalone `dotnet publish`
- Phase 2 涓嶅寘鍚?StartUp 浠诲姟,璺ㄨ处鍙峰垏鍙烽渶鍦?AdvanceAccountCycle 椤堕儴鏄惧紡琛?`StartGame=false StartUp`,閬垮厤 Phase 2 姝ラ鍗℃鍦ㄩ敊璇处鍙?
**寰呮墜鍔ㄩ獙璇?(闇€妯℃嫙鍣ㄧ幆澧?**:
1. 2 璐﹀彿 + 寮€鍏?ON + 鍏ㄥ嬀 鈫?鏃ュ織搴旇 `[Cycle] Account=A, Phase=1` 鍚庡垏鍙?鈫?Account=B, Phase=1 鈫?鍒囧彿 鈫?A, Phase=2 鈫?B, Phase=2 鈫?鍏ㄩ儴瀹屾垚
2. 2 璐﹀彿 + 寮€鍏?ON + 涓嶅嬀鑲夐附鐢熸伅 鈫?Phase 2 鑷姩璺宠繃
3. 2 璐﹀彿 + 寮€鍏?OFF 鈫?鏃х増琛屼负
4. 1 璐﹀彿 + 寮€鍏?ON 鈫?A-1 鈫?A-2 (鍚岃处鍙蜂笉鍒囧彿)
5. 1 璐﹀彿 + 寮€鍏?OFF 鈫?鍗曡处鍙锋棫琛屼负

## 2026-07-14

### fix/account_rotation/3 鈥?鐗堟湰涓嶄竴鑷磋鎶ヤ慨澶?
| # | 鏂囦欢/鎿嶄綔 | 璇存槑 |
|---|----------|------|
| 1 | cmake 缂撳瓨閲嶇疆 | `cmake -DMAA_HASH_VERSION=DEBUG_VERSION`锛屾竻闄や笂娆?release 鑴氭湰閬楃暀鐨?`v6.14.0-fork.20260714` |
| 2 | `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs:117` | `uiVersion` 杩藉姞 `.TrimStart('v', 'V')` |
| 3 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1882` | 鍚屼笂 |
| 4 | `feat/account_rotation` | FF 鍚堝苟 `fix/account_rotation/3` |
| 5 | `branch` | 鍚堝苟 `feat/account_rotation`锛屽悓姝ヤ慨澶?|
| 6 | `Github/branch`, `Github/feat/account_rotation` | 鎺ㄩ€佽嚦杩滅▼ |

### 绉婚櫎鍙戝竷鎵撳寘鏂囦欢 + 娓呯悊 install 鐩綍

浠庝粨搴撳拰 `install/` 涓Щ闄や笉鍐嶉渶瑕佺殑鍙戝竷鎵撳寘鐩稿叧鏂囦欢銆傚悗缁湰鍦拌繍琛岀洿鎺ヤ粠 `build/bin/RelWithDebInfo/` 鍚姩銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `VERSION` | git rm | 浠?release 鑴氭湰璇诲彇锛屼笉鍐嶉渶瑕?|
| 2 | `.github/workflows/release-fork.yml` | git rm | fork 鐨?GitHub CI锛屾湰鍦拌繍琛屼笉闇€瑕?|
| 3 | `tools/release-zip.ps1` | git rm | 鍙戝竷鎵撳寘鑴氭湰锛屼笉鍐嶉渶瑕?|
| 4 | `tools/release-zip.bat` | git rm | 鍙戝竷鎵撳寘鑴氭湰锛屼笉鍐嶉渶瑕?|
| 5 | `tools/DependencySetup_渚濊禆搴撳畨瑁?bat` | git rm | 缁堢鐢ㄦ埛渚濊禆瀹夎鑴氭湰锛屼笉鍐嶉渶瑕?|
| 6 | `install/DependencySetup_渚濊禆搴撳畨瑁?bat` | 鍒犻櫎 | install 鐩綍鍓湰鍚屾娓呯悊 |
| 7 | `install/filelist.txt` | 鍒犻櫎 | 鎵撳寘鏍￠獙娓呭崟锛岃繍琛屾椂鏃犵敤 |
| 8 | `install/Python/` | 鍒犻櫎 | Python 缁戝畾锛學PF GUI 涓嶉渶瑕?|
| 9 | `install/.gitignore` | 鍒犻櫎 | 浜х墿鐩綍涓笉搴旀湁 git 蹇界暐瑙勫垯 |
| 10 | `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs` | 淇敼 | 鐗堟湰姣旇緝鏃?`uiVersion` 涔?`TrimStart('v', 'V')`锛屼慨澶?UI 鍜?Core 鐗堟湰鍙蜂竴鑷翠粛寮硅鍛婄殑 bug |
| 11 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | 鍚屼竴澶勭増鏈瘮杈冿紝琛ヤ笂 `uiVersion.TrimStart` |

### 瀹為檯璺戦€?release-zip + 4 涓?bug 淇

鎸変笂涓€鑺傛柟妗堢涓€娆¤繍琛?`tools\release-zip.bat` 璺戦€氾紝浜у嚭 `installer/MAA-v6.14.0-fork.20260714-win-x64.zip`锛?50.5 MB锛?297 entries锛夈€傝繃绋嬩腑韪╁埌 4 涓湰鏈虹幆澧冮棶棰橈紝鍧囧凡淇骞跺叆鑴氭湰鎴?.gitignore銆?
| # | 闂 | 淇 | 褰卞搷 |
|---|------|------|------|
| 1 | `cmake --build --preset windows-publish-x64` 瑙﹀彂 cmake MSBuild 璇勪及 MaaWpfGui.csproj 鏃舵姤 `Microsoft.NET.Sdk` 鎵句笉鍒帮細VS 2026 瑁呭湪 `E:\visual stduio community 2026\VIUAL\`锛堟嫾鍐欓敊锛夛紝鍏?`MSBuild\Sdks\Microsoft.NET.Sdk\Sdk` 鐩綍缂哄け銆?| 鏀圭敤 `cmake --build build --target MaaCore` 鍗曠洰鏍?C++ 鏋勫缓锛沇PF 鏀硅蛋 standalone `dotnet publish`锛堢敤 `C:\Program Files\dotnet` 鐨?SDK锛屼笌 cmake 甯︾殑 MSBuild 瑙ｈ€︼級銆傝剼鏈敼涓€澶勶紙2 琛?cmake 鍛戒护锛夈€?| 璺宠繃 cmake 鐨?WPF 璇勪及锛岃蛋鍜?`local-install.bat` 涓€鑷寸殑"鍏?cmake 瑁?C++锛屽啀 dotnet publish WPF"鍙岃建娴佺▼銆?|
| 2 | 涓存椂 `global.json` 寮哄埗 `10.0.203` + `rollForward:disable`锛屼絾鏈満 `dotnet --list-sdks` 鍙 `10.0.300`锛孲DK 瑙ｆ瀽澶辫触銆?| 鏀逛负 `10.0.100` + `rollForward:latestFeature`锛氳姹?.NET 10 鐗规€у甫鍐咃紙10.0.x.x锛夛紝浣嗕笉閿佸皬鐗堟湰銆?| 浠讳綍瑁?10.0.x SDK 鐨勬満鍣ㄩ兘鑳借窇锛涙湰鏈?10.0.300 鑷姩鍚敤銆?|
| 3 | `install\.git\` 鏄釜鐪熷疄 git 瀛愮洰褰曪紙鍚?hooks/info/objects/refs锛夛紝鍘?staging 婕忔帓锛屽鑷村畠琚墦杩?zip锛堢害 50 涓棤鍏虫枃浠讹級銆?| robocopy `/XD` 鍒楄〃杩藉姞 `.git`銆?| zip 涓嶅啀鎼哄甫鎰忓鐩綍銆?|
| 4 | `.gitignore` 绗?510 琛?`DependencySetup_渚濊禆搴撳畨瑁?bat` 鏃?`install/` 鍓嶇紑锛岃浼?`tools/DependencySetup_渚濊禆搴撳畨瑁?bat`锛屽鑷存簮鏂囦欢闀挎湡鏃犳硶鍏ヤ粨銆?| 瑙勫垯鏀逛负 `install/DependencySetup_*.bat`锛屽彧灞忚斀 install/ 鍓湰銆?| tools/ 婧愬彲鍏ヤ粨锛宐uild 鍙鐜般€?|

闄勫姞淇锛氬彂鐜?`tools\DependencySetup_渚濊禆搴撳畨瑁?bat` 鍦?git 璺熻釜涓疄闄呭凡涓嶅瓨鍦紙浠?`install/` 閲屾湁鍓湰锛夛紝浠?`install/` 杩樺師鍥?`tools/`锛岃剼鏈殑 `Copy-Item` 鎵嶆湁婧愩€?
鏈€缁?8 姝ュ叏杩囨棩蹇楋細`<install>/installer-build.log`锛?362 琛岋紝2.6 MB锛夈€?
**zip 缁撴瀯 vs `D:\MAA\MAA-v5.21.2-win-x64` 瀵圭収**锛?
- 鍏卞悓锛?6 涓《灞?DLL/EXE 鏂囦欢锛坄MAA.exe` / `MAA.dll` / `MaaCore.dll` / `MaaUtils.dll` / 4 涓?ControlUnit / DirectML / fastdeploy_ppocr_maa / onnxruntime_maa / opencv_world4_maa / `MAA.Updater.exe` 绛夛級+ 4 涓《灞傜洰褰曪紙`externals/`銆乣Python/`銆乣Res/`銆乣resource/`锛? `DependencySetup_渚濊禆搴撳畨瑁?bat`
- 宸紓锛堟鍚戯級锛氭垜浠澶栨湁 `libloader.dll`锛坙ibloader.dll 鍚姩閽╁瓙锛夈€乣res_updater.exe`锛堣祫婧愯嚜鏇存柊鍣級鈥?鏈粨搴撳巻鍙插姞鍏?- 宸紓锛堣礋鍚戯級锛氬畼鏂规湁 `hostfxr.dll` / `hostpolicy.dll`锛坉otnet 鍚姩鍣級銆乣filelist.txt`锛圕I 浜х墿娓呭崟锛夆€?鏈満鐢?SelfContained 鍙戝竷妯″紡涓嶉渶瑕侊紝`filelist.txt` 鍦?.gitignore 宸叉帓
- 鐢ㄦ埛鏁版嵁鐩綍锛坄cache/` / `config/` / `data/` / `debug/`锛夛細鎴戜滑**姝ｇ‘鎺掗櫎** 鉁擄紙瀹樻柟涓嶇煡涓轰綍淇濈暀鍦?release 瑙ｅ帇鍚庣殑鐩綍閲岋紝鍙兘鏄彂甯冨墠琚鐑繃锛?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `tools/release-zip.ps1` | 淇敼 | 姝ラ 2 鏀瑰崟鐩爣 cmake build锛涙楠?6 鏀?global.json 涓?`10.0.100 + latestFeature`锛涙楠?8 staging `/XD` 鍔?`.git`锛涜剼鏈敞閲婅鏄庢敼鍔ㄥ師鍥?|
| 2 | `tools/DependencySetup_渚濊禆搴撳畨瑁?bat` | 杩樺師 | 浠?`install/` 鍓湰鎷峰洖 `tools/`锛屾仮澶?`Copy-Item` 婧愶紱骞跺叆 git |
| 3 | `.gitignore` | 淇敼 | (a) 杩藉姞 `/installer-build.log`锛?.6 MB 涓存椂鏃ュ織涓嶅叆浠擄級锛?b) 淇绗?510 琛岃鍒?`DependencySetup_渚濊禆搴撳畨瑁?bat` 璇激 `tools/`锛屾敼涓?`install/DependencySetup_*.bat`锛堝彧灞忚斀 install/ 鍓湰锛?|
| 4 | `LOG.md` | 淇敼 | 鏈妭璁板綍棣栨璺戦€氫笌 4 涓?bug 淇 |



### 涓€閿彂甯冨寘鑴氭湰锛坕nstaller/锛?
鏂板 `tools/release-zip.{bat,ps1}`锛屽鐓т笂娓?`.github/workflows/ci.yml` 鐨?`Build for Windows` 浣滀笟瀹炵幇鏈湴绛変环娴佺▼锛氫粠 `VERSION` 璇荤増鏈彿 鈫?`cmake --preset windows-publish-x64 -DMAA_HASH_VERSION=<v>` 鈫?鏋勫缓 `MAA.Updater` 鈫?`cmake --install` 鈫?鍚屾 `resource/` 鈫?涓存椂鏀?csproj 4 涓?Version 瀛楁鍚?`dotnet publish` 鈫?鍓?`*.pdb` `*.h` `*.bak` `msvc-debug/` `MAAComponent-DebugSymbol-*.zip` 鈫?鎷?`DependencySetup_渚濊禆搴撳畨瑁?bat` 鈫?鐢?`System.IO.Compression.ZipFile` 鍘嬬缉鍒?`installer/MAA-<v>-win-x64.zip`锛宻taging 鐩綍鎺掗櫎 `cache/` `config/` `data/` `debug/`锛堢敤鎴锋暟鎹笉鍏ュ寘锛夈€?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `VERSION` | 鏂板缓 | 鍐呭 `v6.14.0-fork.20260714`锛屼綔涓?`MAA_HASH_VERSION` 鍜?zip 鏂囦欢鍚嶅崟涓€鏉ユ簮 |
| 2 | `tools/release-zip.bat` | 鏂板缓 | bat 澶栧３锛岃皟 ps1 鍚?`pause`锛涘け璐ユ椂 `errorlevel` 閫忎紶 |
| 3 | `tools/release-zip.ps1` | 鏂板缓 | 鏍稿績 PowerShell 鑴氭湰锛垀180 琛岋級锛宍-Version` / `-SkipBuild` / `-KeepInstallerDir` 涓変釜寮€鍏?|
| 4 | `.gitignore` | 淇敼 | 鏈熬杩藉姞 `/installer/`锛堜骇鐗╀笉鍏ヤ粨锛夛紝涓?`install/` 涓€鑷翠笉姹℃煋 git |
| 5 | `LOG.md` | 淇敼 | 鏈妭 |

**鍏抽敭璁捐**锛?
- **try/finally 淇濇姢浠撳簱鐘舵€?*锛歝sproj 澶囦唤鍒?`.bak` 鍚庢敼 4 涓?Version 瀛楁锛屾棤璁?dotnet publish 鎴愬姛鎴栧け璐ラ兘杩樺師锛沗global.json` 鍚屾牱澶勭悊锛堟寜 AGENTS.md 绾﹀畾鍐?10.0.203锛夆啋 浠撳簱**姘歌繙涓嶅彉鑴?*
- **涓嶈仈缃戞媺 MaaFramework**锛氬鐢?install/ 鐜版湁鐨?`MaaAdbControlUnit.dll` / `MaaWin32ControlUnit.dll`锛岀己鍒欐姤閿欙紙鐢ㄦ埛鍦?install/ 閲屽凡鏈夎繖涓や釜鏂囦欢锛?- **staging 鐩綍涓存椂鏋勯€?*锛氶伩鍏嶆妸 `cache/config/data/debug` 杩?4 涓敤鎴锋暟鎹洰褰曟墦杩?zip锛涚敤 `robocopy /MIR /XD` 瀹炵幇
- **`-SkipBuild` 寮€鍏?*锛氬彧鎯抽噸鏂版墦 zip 涓嶉噸缂栬瘧鏃跺彲鐢?- **涓嶈鍓?`externals/`**锛氫笌涓婃父 CI 琛屼负涓€鑷达紝闆堕闄?- **涓嶇敓鎴?DebugSymbol 鍓寘**锛氱敤鎴风‘璁や笉闇€瑕?- **鑴氭湰鍛藉悕 `release-zip.*` 鑰岄潪 `build-release-zip.*`**锛氭牴 `.gitignore` 绗?6 琛屾湁 `build-*` 瑙勫垯锛圕Make 鏋勫缓浜х墿锛夛紝鍘熷懡鍚嶄細琚悶鎺夛紱鏀瑰悕鍓?git check-ignore 楠岃瘉纭

**鐗堟湰鍙锋牸寮?*锛歚vX.Y.Z-fork.YYYYMMDD`锛堝 `v6.14.0-fork.20260714`锛夈€係emVer 瑙ｆ瀽涓?prerelease锛孲emVer 涓ユ牸鏀寔锛涗笂娓告柊鐗堟湰锛堝 `v6.14.1`锛夋寜 SemVer 鎺掑簭鑳芥纭Е鍙戞洿鏂版彁绀恒€?
**浣跨敤鏂瑰紡**锛?```bat
:: 鏍囧噯鐢ㄦ硶锛堝弻鍑绘垨鍛戒护琛岋級
tools\release-zip.bat

:: 璺宠繃缂栬瘧銆佸彧閲嶆柊鎵撳寘锛坕nstall/ 宸叉槸鏈€鏂版椂锛?powershell -File tools\release-zip.ps1 -SkipBuild

:: 鎸囧畾鐗堟湰锛堣烦杩?VERSION 鏂囦欢锛?powershell -File tools\release-zip.ps1 -Version v6.14.0-fork.20260715
```

**涓庝紶缁?local-install.bat 鐨勫尯鍒?*锛?
| 椤?| `local-install.bat` | `release-zip.{bat,ps1}` |
|----|---------------------|-------------------------|
| 鐢ㄩ€?| 鏈湴寮€鍙戣嚜鐢?| **鎵撳寘缁欏叾浠栦汉** |
| 浜х墿 | `install/` 鐩存帴鍙窇 | `installer/MAA-vX.X.X-fork.YYYYMMDD-win-x64.zip` |
| `csproj` 鏀?| 涓嶆敼 | 涓存椂鏀?4 涓?Version 瀛楁鍐嶈繕鍘?|
| `*.pdb` `*.h` | 淇濈暀 | 鍓?|
| NetBeauty | 鍚敤 | 鍚敤 |
| 璧勬簮 | 鍚屾鍒?`install/` | 鍚屾鍒?`install/` 鐒跺悗鎺掗櫎鐢ㄦ埛鏁版嵁鍚庢墦 zip |

### 宸ヤ綔鍖烘竻鐞?+ 宸ュ叿鑴氭湰褰掓。 + 瀛愭ā鍧楀垵濮嬪寲

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `true` | 鍒犻櫎 | 0 瀛楄妭绌烘枃浠讹紝璇骇鐢燂紝娓呯悊 |
| 2 | `add_maa_to_nahimic_whitelist.ps1` | 绉诲姩 | 浠庢牴鐩綍绉诲叆 `tools/`锛岃剼鏈敤閫旓細灏?MAA.exe 娣诲姞鍒?Nahimic DLL 娉ㄥ叆鐧藉悕鍗曪紝缁曡繃 Nahimic 鎷︽埅 |
| 3 | `disable_nahimic.ps1` | 绉诲姩 | 浠庢牴鐩綍绉诲叆 `tools/`锛岃剼鏈敤閫旓細鍋滄骞剁鐢?NahimicService 寮€鏈鸿嚜鍚紝褰诲簳闃绘 DLL 娉ㄥ叆 |
| 4 | `src/MaaUtils` | 瀛愭ā鍧楀垵濮嬪寲 | 寮曠敤涓婃父 `MaaXYZ/MaaUtils`锛圚EAD `0c2556cfc`锛夛紝鎻愪氦鑷?feat/fix 绱㈠紩 |
| 5 | `3rdparty/EmulatorExtras` | 瀛愭ā鍧楀垵濮嬪寲 | 寮曠敤涓婃父 `MaaXYZ/EmulatorExtras`锛圚EAD `54d3a3ad4`锛夛紝鎻愪氦鑷?feat/fix 绱㈠紩 |

### feat/account_rotation 鍒嗘敮鏀跺熬

灏?`fix/account_rotation/1` + `fix/account_rotation/2` 鍚堝苟鑷?`feat/account_rotation`锛屽舰鎴愬崟涓€涓诲垎鏀€?
## 2026-07-13

### 璐﹀彿杞崲锛氬仠姝㈠崱姝?+ 鍒囨崲鎶ラ敊鍚庢棤娉曟仮澶?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | `SetStopped` 涓?`IsCycling` 鐭矾鍒嗘敮澧炲姞"鏄惁琚己鍒跺仠姝?鍒ゆ柇锛歚runStopScript && _runningState.GetStopping()` 鏃惰惤绌?`IsCycling` 璧板畬鏁撮噸缃祦绋?娓?`Stopping` 鏍囧織;姝ｅ父杞崲鎺ㄨ繘璺緞淇濇寔涓嶅彉(鐩存帴 return)銆備慨澶嶇偣鍋滄鎸夐挳鍚?UI 姘歌繙鍗″湪"姝ｅ湪鍋滄"涓旀寜閽笉鍙敤鐨勯棶棰樸€?|
| 2 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | `AdvanceAccountCycle` 涓や釜澶辫触鍒嗘敮(`count == 0` 鏃犱换鍔¤闄勫姞銆乣AsstStart()` 澶辫触)鏀逛负璋冪敤 `SetStopped(runStopScript: false)`,缁熶竴閲嶇疆 `Stopping/Idle/IsCycling`銆備慨澶?鍒囨崲绗簩涓处鍙蜂换鍔″嚭閿?鍚庣姸鎬佸崱浣忋€佹寜閽彉鐏般€佹爣棰樹笉鎭㈠鐨勯棶棰樸€?|

## 2026-07-11

### 婕忔礊淇涓庨厤缃皟鏁?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | `LinkStart` 琛ヤ笂 `AccountSwitchEnabled = true`锛沗TryStartNextCycleAccount` 澶勭悊 `cfg` 涓?null 鐨勮竟鐣屾儏鍐碉紱鍖呰９ try-catch 闃叉 `async void` 闈欓粯鍚炲紓甯革紱閫氳繃 `Execute.OnUIThreadAsync` 纭繚 UI 绾跨▼鎵ц |
| 2 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | 淇敼 | `GetCurrentCycleAccount` 绠€鍖栵細鍘绘帀 `_currentCycleIndex` 鐘舵€佽窡韪紝鏀逛负鐩存帴鍙栫涓€涓鍚堟潯浠剁殑璐﹀彿锛涘幓鎺?`ResetCycleIndex` 鏂规硶 |
| 3 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml` | 淇敼 | 娣诲姞/鍒犻櫎鎸夐挳鍥炬爣缁熶竴瀛楀彿鍜屽眳涓?|
| 4 | `.gitignore` | 淇敼 | 杩藉姞杩愯鏃剁紦瀛樺拷鐣ヨ鍒欙紱杩藉姞 `.crush/` / `.claude/` / `.cursor/` 瑙勫垯锛涜拷鍔?`LOG.md` / `AGENTS.md` 蹇界暐 |
| 5 | `src/MaaWpfGui/Main/AsstProxy.cs` | 淇敼 | `AllTasksCompleted` 鍥炶皟涓ˉ涓婅疆鎹㈡帹杩涢€昏緫锛氭甯稿畬鎴愭椂璋冪敤 `MarkAccountCompleted` + `GetCurrentCycleAccount` + `LinkStart`锛屽苟 `break` 璺宠繃鏍囧噯瀹屾垚鏃ュ織锛岄槻姝㈡柊涓€杞惎鍔ㄥ悗浠嶆墦鍑?鎵€鏈変换鍔″畬鎴? |

### install 鐩綍閲嶆瀯

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `install/debug/oper` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 2 | `install/debug/drops` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 3 | `install/debug/other` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 4 | `install/debug/infrast` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 5 | `install/debug/interface` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 6 | `install/cache/avatars` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 7 | `install/filelist.txt` | 淇敼 | 閲嶆柊鐢熸垚锛屼笌瀹為檯鏂囦欢鍚屾 |
| 8 | `install/MAA.dll`, `install/MAA.pdb`, `install/MAA.exe`, `install/MAA.deps.json`, `install/MAA.runtimeconfig.json` | 鏇存柊 | 缂栬瘧 MaaWpfGui 鍚庨儴缃诧紱淇 `AsstProxy.cs` 涓?`StartUpTask` 闈欐€佸睘鎬х敤瀹炰緥璁块棶鐨勭紪璇戦敊璇紱淇 `TaskQueueViewModel.cs` 缂╄繘鍜屽浣欑┖琛?|
| 9 | `install/MaaCore.dll`, `install/MaaUtils.dll` | 鏇存柊 | 瀹屾暣 CMake 鏋勫缓鍚庨儴缃?锛圧elWithDebInfo锛墊
| 10 | `src/MaaWpfGui/MaaWpfGui.csproj` | 淇敼 | `SelfContained` 鏀逛负 `false`锛岀鐢?NetBeauty2 鎵撳寘锛堜笉鍏煎 .NET 10.0.300锛?|
| 11 | `install/msvc-debug/` | 鍒犻櫎 | CMake 瀹夎浜х敓鐨?debug 绗﹀彿鐩綍锛岄潪蹇呴渶 |
| 12 | `install/filelist.txt` | 鏇存柊 | 閲嶆柊鐢熸垚 |
| 13 | `src/MaaCore/Assistant.cpp` | 淇敼 | `AllTasksCompleted` 鍚庣珛鍗宠 `m_thread_idle=true`锛屼慨澶嶇浜岃疆 `AsstStart` 鍥犵珵鎬佽繑鍥?false 鐨?bug |
| 14 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | 鏂板 `AdvanceAccountCycle()` 鏂规硶鏇夸唬 `SetStopped` 鍋氳疆鎹㈡帹杩涳紱`SetStopped` 鍓ョ杞崲閫昏緫锛屽彧澶勭悊鍋滄 |
| 15 | `src/MaaWpfGui/Main/AsstProxy.cs` | 淇敼 | `AllTasksCompleted` 鍥炶皟璋?`AdvanceAccountCycle` 鏇夸唬 `SetStopped` |
| 16 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | 淇敼 | `SyncAccountNamesToItems` 淇濈暀宸叉湁椤?`IsSelected` 鐘舵€侊紝鐢ㄦ埛鍙嚜鐢卞嬀閫夊弬涓庤疆鎹㈢殑璐﹀彿 |

### install 鐩綍鏍囧噯鍖?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `install/MAA.runtimeconfig.json` | 淇敼 | 娣诲姞 `additionalProbingPaths: ["./externals"]` 鍜?`STARTUP_HOOKS: libloader` |
| 2 | `install/MAA.deps.json` | 鏇挎崲 | 鏇挎崲涓哄弬鑰冪増锛圢etBeauty2 姝ｇ‘淇ˉ鐨勭増鏈級 |
| 3 | `install/externals/` | 閲嶅缓 | 灏?279 涓墭绠?DLL 绉诲叆 `externals/`锛涘尯鍩熷崼鏄熺▼搴忛泦绉昏嚦 `externals/locales/{lang}/` |
| 4 | `install/` 鏍圭洰褰?| 娓呯悊 | 浠呬繚鐣?11 涓牳蹇?DLL + 2 涓?EXE + 閰嶇疆鏂囦欢 |
| 5 | `install/filelist.txt` | 鏂板 | 浠庡弬鑰冪増澶嶅埗 |
| 6 | `install/MAA.dll` | 鏇存柊 | 澶氭閲嶅缓閮ㄧ讲 |
| 7 | 绌虹洰褰曟竻鐞?| 鍒犻櫎 | 鍒犻櫎 10 涓┖鐩綍锛堝瓙妯″潡鍗犱綅绛夛級 |
| 8 | 鍐椾綑 DLL 娓呯悊 | 鍒犻櫎 | 鍒犻櫎鏍圭洰褰?33 涓?.NET runtime DLL + install 鐩綍 245 涓?|

### Skills 杩佺Щ

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `.crush/skills/` | 鏂板缓 | 浠?`.claude/skills/` 鍜?`.cursor/skills/` 杩佸叆 5 涓?skill |
| 2 | `.claude/` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|
| 3 | `.cursor/` | 鍒犻櫎 | 绌虹洰褰曟竻鐞?|

### branch 鎺ュ彈 feat/account_rotation 鍚堝苟

灏?`feat/account_rotation` 閫氳繃 Fast-forward 鏂瑰紡鍚堝叆 `branch`锛屽畬鎴愯鍔熻兘鐨勬寮忓彂甯冩祦绋嬨€?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|-----------|------|------|
| 1 | `branch` | FF 鍚堝苟 | 鎺ユ敹 `feat/account_rotation` 鍏ㄩ儴 7 涓柊鎻愪氦锛孒EAD 鐢?`c8c8e75be5` 鈫?`23b1bf3167`銆俙merge-base` 绛変簬 `branch` 鏃?HEAD锛屾棤鍒嗗弶銆佹棤鍚堝苟鎻愪氦銆佹棤鍐茬獊 |
| 2 | `Github/branch` | 鎺ㄩ€?| 杩滅▼ `branch` 鍚屾鍒?`23b1bf3167`锛坄git push Github branch`锛墊
| 3 | `Github/feat/account_rotation` | 鍒犻櫎 | feat 鍔熻兘姝ｅ紡鍚堝叆 `branch`锛岃繙绋?feat 鍒嗘敮娓呯悊锛坄git push Github --delete feat/account_rotation`锛夈€傛湰鍦?`feat/account_rotation` 淇濈暀浠ュ鍥炴函涓庡鐓?|
| 4 | `feat_account_rotation.md` | 淇敼 | 杩藉姞 `## 鍗佷竴銆乫eat鈫抌ranch 鍚堝苟瀹屾垚` 娌夋穩娈佃惤锛屾洿鏂?`## 涔濄€佸緟鍔瀈 瀹屾垚椤?|
| 5 | `LOG.md` | 淇敼 | 鏂板鏈妭锛岃褰曞悎骞朵簨浠?|

鍚堝苟鍓嶅悗 `branch` 瀵规瘮锛?
```
c8c8e75be5  Initial commit: MAA fork base with account rotation feature
   鈫?+7 commits
23b1bf3167  fix: 鍋滄鍗℃ + 鍒囨崲鎶ラ敊鍚庢棤娉曟仮澶? chore: 褰掓。 Nahimic 鑴氭湰鍒?tools/; build: 鍒濆鍖?MaaUtils/EmulatorExtras 瀛愭ā鍧?```

鍏抽敭鍐崇瓥锛?
- **FF 鑰岄潪 --no-ff**锛歚branch` 鏄?`feat/account_rotation` 鐨勪弗鏍肩鍏堬紝鏃犱换浣曞垎鍙夈€俙--no-ff` 浼氫骇鐢熸棤淇℃伅澧為噺鐨勫啑浣?merge commit锛屼笌 AGENTS.md 鎺ㄥ磭鐨?PR 绠€娲佸巻鍙?鐩告倴
- **鏈湴淇濈暀 feat 鍒嗘敮**锛歠eat 鍔熻兘铏藉凡鍚堝苟鍒?`branch`锛屼絾淇濈暀鏈湴 `feat/account_rotation` 鎸囬拡渚夸簬鍥炴函锛堝瀵规瘮 feat 琛屼负宸紓銆乧herry-pick 淇绛夛級銆俙fix/account_rotation/1` 鍜?`fix/account_rotation/2` 淇濈暀鍚屾牱鐞嗙敱
- **杩滅▼鍒犻櫎 feat**锛氳繙绔?feat 鍒嗘敮宸叉棤瀛樺湪浠峰€硷紙鍔熻兘鍦?`branch` 涓級锛屾竻鐞嗗彲鍑忓皯杩滅▼鍒嗘敮鍒楄〃鍣煶

### 杩滅▼ feat/account_rotation 鎭㈠锛堝喅绛栦慨姝ｏ級

浜嬪悗婢勬竻锛歚fix/account_rotation/{1,2}` 涓?`feat/account_rotation` 鏄粠灞炲叧绯伙紝杩滅▼ fix/* 涓嶅簲瀛ょ珛瀛樺湪銆備慨姝ｅ厛鍓?鍒犻櫎杩滅▼ feat"鐨勫喅绛栵紝灏嗚繙绋?feat 鎭㈠銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|-----------|------|------|
| 1 | `Github/feat/account_rotation` | 鎭㈠ | `git push Github feat/account_rotation`锛屽湪杩滅閲嶅缓 `refs/heads/feat/account_rotation` 鈫?`23b1bf3167` |
| 2 | `feat/account_rotation` | 璁剧疆 upstream | `git branch --set-upstream-to=Github/feat/account_rotation feat/account_rotation`锛屽悗缁?push/pull 鏃犻渶鎸囧畾杩滅 |
| 3 | `Github/fix/account_rotation/{1,2}` | 涓嶅姩 | 鎸夌敤鎴风‘璁や繚鐣欎袱涓繙绋?fix 鍒嗘敮锛屼笉鍒犻櫎 |
| 4 | `feat_account_rotation.md` | 淇敼 | 杩藉姞 `## 鍗佷簩銆佽繙绋?feat 鎭㈠璁板綍`锛岃褰曞喅绛栦慨姝ｄ笌淇鍚庡垎鏀眰绾?|

淇鍚庤繙绋嬪垎鏀粨鏋勶細

```
branch                          (23b1bf3167, 鐢熶骇灏辩华)
鈹斺攢 feat/account_rotation        (23b1bf3167, 浠庡睘, 鎭㈠)
   鈹溾攢 fix/account_rotation/1    (f3413f24f5, 浠庡睘, 钀藉悗 feat 1 commit)
   鈹斺攢 fix/account_rotation/2    (23b1bf3167, 浠庡睘, 涓?feat 鍚屼綅)
```

### 宸ヤ綔鏂囨。鍛藉悕瑙勮寖鍖?
鎸?AGENTS.md 鍒嗘敮宸ヤ綔娴佺害瀹氾紝feat/fix 宸ヤ綔鏂囨。鍚嶅簲瀵归綈鍒嗘敮鍚嶃€備腑鏂囨枃浠跺悕鍦ㄨ法骞冲彴 / 缁堢鍦烘櫙涓嬪瓨鍦ㄧ紪鐮佸吋瀹归棶棰橈紝缁熶竴鏀逛负鑻辨枃鍛藉悕銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `feat_璐﹀彿杞崲.md` | 鍒犻櫎 | 涓枃鏂囦欢鍚嶏紱鍐呭宸叉媶鍒嗭細涓婂崐娈碉紙绔犺妭 涓€~鍗佷竴 鍚堝苟鏃ュ織锛夊凡瀛樺湪浜?`feat_account_rotation.md`銆佹棤閲嶅锛涙湯灏剧嫭绔嬭璁℃枃妗ｏ紙鏍囬"璐﹀彿杞崲鍔熻兘璁捐涓庡疄鐜?锛?1 绔?~9000 瀛楋級琚悎鍏?`feat_account_rotation.md` 浣滀负鏂扮珷鑺傚崄浜?|
| 2 | `feat_account_rotation.md` | 淇敼 | 鏂板 `## 鍗佷簩銆佽处鍙疯疆鎹㈠姛鑳借璁℃枃妗锛氳鐩栧姛鑳芥灦鏋勩€佹暟鎹ā鍨嬨€佺紪杈戞ā寮忋€佽疆鎹㈢敓鍛藉懆鏈熴€乁I 甯冨眬涓庝氦浜掋€佹湰鍦板寲瀛楃涓层€佹墜鍔ㄥ垏鎹€丆++ 鍚庣娴佺▼銆侀粯璁ら厤缃€佹枃浠舵竻鍗曘€佺害鏉熶笌娉ㄦ剰浜嬮」銆傚師 `## 鍗佷簩銆佽繙绋?feat 鎭㈠璁板綍` 閲嶇紪鍙蜂负 `## 鍗佷笁銆乣锛堝惈 `### 12.x` 鈫?`### 13.x`锛?|
| 3 | `fix_璐﹀彿杞崲.md` | 閲嶅懡鍚?| 鈫?`fix_account_rotation_1.md`锛屽榻愬垎鏀悕 `fix/account_rotation/1`锛岃瘉鏄庢槸 `feat/account_rotation` 鍒嗘敮鐨勭涓€娆′慨澶?|

**鏂囦欢鏁寸悊鍓嶅悗**锛?
```
鏁寸悊鍓?                                   鏁寸悊鍚?feat_account_rotation.md  (16050 B)       feat_account_rotation.md  (31223 B, 绔犺妭 11 鈫?13)
feat_璐﹀彿杞崲.md          (14834 B)  鈫? fix_account_rotation_1.md  (2154 B)
fix_璐﹀彿杞崲.md           ( 2154 B)
```

**涓轰粈涔堟柊澧炵殑鏄珷鑺傚崄浜岃€岄潪鍏朵粬浣嶇疆**锛?
- 璁捐鏂囨。鏄?feat 鍒嗘敮鐨?*鏍稿績浜や粯鐗?*锛圲I 鏋舵瀯 / 鏁版嵁娴?/ 绾︽潫锛夛紝鐞嗗簲浣滀负闀挎湡鐭ヨ瘑娌夋穩锛屼綅缃簲闈犲墠
- 鍘熺珷鑺傚崄浜岋紙杩滅▼ feat 鎭㈠璁板綍锛夋槸**鎿嶄綔娴佹按**锛屾椂鏁堟€у己銆侀暱鏈熷弬鑰冧环鍊间綆锛岄噸缂栧彿涓哄崄涓変笉褰卞搷闃呰
- 绔犺妭鍗佷竴锛坒eat鈫抌ranch 鍚堝苟瀹屾垚锛夋槸鍒嗘敮鐢熷懡鍛ㄦ湡璁板綍锛屼綔涓烘敹瀹樼珷鑺備繚鎸佷笉鍔?
**涓轰粈涔?fix 鏂囦欢鐢?`fix_account_rotation_1.md`锛堜笅鍒掔嚎锛夎€岄潪鐩綍褰㈠紡**锛?
- Windows 鏂囦欢鍚嶄笉鏀寔 `/`锛屾棤娉曚娇鐢?`fix_account_rotation/1.md` 璺緞鍒嗕袱娈?- 涓嬪垝绾?`_` 涓?AGENTS.md "feat_*.md / fix_*.md" 鐜版湁绾﹀畾涓€鑷达紙濡?`feat_璐﹀彿杞崲.md`锛?- 闃挎媺浼暟瀛楀悗缂€ `_1` 闅愬惈搴忓垪璇箟锛屾湭鏉?`fix/account_rotation/2` 瀵瑰簲 `fix_account_rotation_2.md`锛屼竴涓€瀵瑰簲

## 2026-07-10

### 璐﹀彿杞崲婕忔礊淇

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | `SetStopped` 鏂板杞崲閫昏緫锛氬畬鎴愪换鍔″悗璋冪敤 `MarkAccountCompleted` 鏍囪褰撳墠璐﹀彿瀹屾垚锛岃嫢杩樻湁鏈畬鎴愯处鍙峰垯鑷姩瑙﹀彂 `LinkStart` 缁х画涓嬩竴璐﹀彿 |

## 2026-07-10

### 璐﹀彿杞崲鍔熻兘瀹炵幇

瀹炵幇浜嗗畬鏁寸殑璐﹀彿杞崲鍔熻兘锛屽厑璁哥敤鎴烽厤缃涓处鍙凤紝MAA 鍦ㄦ瘡娆″畬鏁翠换鍔￠槦鍒楁墽琛屽畬姣曞悗鑷姩鍒囨崲鍒板垪琛ㄤ腑鐨勪笅涓€涓湭瀹屾垚璐﹀彿锛屽苟閲嶆柊鍚姩浠诲姟闃熷垪锛岀洿鍒版墍鏈夎处鍙峰叏閮ㄥ畬鎴愪竴杞€?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs` | 淇敼 | 娣诲姞 `AccountCycleEnabled` (bool, 榛樿 true) 鍜?`AccountNames` (List\<string\>, 榛樿 ["", ""]) |
| 2 | `src/MaaWpfGui/Models/AccountCycleItem.cs` | 鏂板缓 | 杞崲璐﹀彿鏁版嵁妯″瀷锛圖isplayName / AccountName / IsSelected / IsCompleted / Index锛?|
| 3 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml` | 淇敼 | 娣诲姞 7 涓?AccountCycle 鏈湴鍖?key |
| 4 | `src/MaaWpfGui/Res/Localizations/en-us.xaml` | 淇敼 | 鍚屼笂锛堣嫳鏂囷級 |
| 5 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml` | 淇敼 | 鍚屼笂锛堟棩鏂囷級 |
| 6 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml` | 淇敼 | 鍚屼笂锛堥煩鏂囷級 |
| 7 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml` | 淇敼 | 鍚屼笂锛堢箒浣撲腑鏂囷級 |
| 8 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | 淇敼 | 娣诲姞杞崲 CRUD銆丟etCurrentCycleAccount銆丮arkAccountCompleted銆丼yncAccountNamesToItems 绛夋柟娉?|
| 9 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 淇敼 | LinkStart 鍔犲叆杞崲鍒ゅ畾锛孲etStopped 鍚庤皟鐢?TryStartNextCycleAccount 鑷姩鎺ㄨ繘 |
| 10 | `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs` | 淇敼 | 鐗堟湰姣旇緝蹇界暐 `v` 鍓嶇紑 |
| 11 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml` | 淇敼 | 娣诲姞杞崲 CheckBox銆佽处鍙峰垪琛?ItemsControl銆佺紪杈戞ā寮?ComboBox銆両sCompleted 钃濊壊楂樹寒 |
| 12 | `src/MaaWpfGui/MaaWpfGui.csproj` | 淇敼 | 鐗堟湰鍙蜂粠 0.0.1 鏀逛负 6.14.0 |
| 13 | `install/config/gui.new.json` | 淇敼 | StartUpTask 娣诲姞 AccountCycleEnabled / AccountNames 榛樿瀛楁 |
| 14 | `install/config/gui.new.json.bak` | 淇敼 | 鍚屼笂锛堝浠芥枃浠跺悓姝ワ級 |

### fix/defer-rogue/1 鍚姩

鍩轰簬 `feat/defer-rogue` (commit `31b84f44a3`) 鐨?code review,鍙戠幇 3 涓棶棰?

| ID | 涓ラ噸搴?| 涓婚 |
|----|--------|------|
| A1 | CRITICAL | 姝ラ鑰楀敖鏃?鏈€鍚庝竴涓处鍙蜂笉浼氳 `MarkAccountCompleted` |
| A7 | WARNING | Cycle 杩愯涓?`LateStageRogueAndReclamation` CheckBox 浠嶅彲鐐瑰嚮 |
| A8 | WARNING | `LinkStart` 鏃?re-entrancy guard,杩愯涓啀娆＄偣鍑讳細閲嶇疆 _cycleSteps/_currentStepIndex |

鎸?AGENTS.md 鏂扮害瀹?`fix/*` 蹇呴』浠庡搴?`feat/*` 鎷夊嚭),浠?`feat/defer-rogue` 鎷夊嚭 `fix/defer-rogue/1`銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `fix/defer-rogue/1` | 鏂板缓鍒嗘敮 | 浠?`feat/defer-rogue` 鎷夊嚭 |
| 2 | `fix_defer-rogue_1.md` | 鏂板缓 | 宸ヤ綔鏂囨。(宸?.gitignore) |
| 3 | `AGENTS.md` | 淇敼 | 鎶?淇鍒嗘敮鎸傚湪瀵瑰簲 feat 涓?鍐欏叆姝ｅ紡绾﹀畾:鏇存柊鍒嗘敮鍛藉悕琛?+ 澧炲姞"淇鍒嗘敮蹇呴』鎸傚湪瀵瑰簲 feat 涔嬩笅"灏忚妭 + 鏇存柊宸ヤ綔娴佺ず鎰忓浘 |

### fix/defer-rogue/1 瀹炴柦瀹屾垚

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2195-2200` | 淇敼 | `AdvanceAccountCycle` 鏂囨。娉ㄩ噴杩藉姞 fix/defer-rogue/1 娈佃惤 |
| 2 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2208-2222` | 淇敼 | **A1**: 鎶?`prevStep = GetPreviousStep()` 绉诲埌 `nextStep == null` 鏃╅€€鍒嗘敮**涔嬪墠**;鏃╅€€鍒嗘敮閲屽厛璋冪敤 `MarkPreviousStepCompleted(prevStep)` 鍐?`return` |
| 3 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2235-2236` | 淇敼 | **A1**: 鏅€氭帹杩涜矾寰勭Щ闄ゅ師 inline 鍧?鏀逛负璋冪敤 `MarkPreviousStepCompleted(prevStep)` |
| 4 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2382-2400` | 淇敼 | **A1**: 鏂板绉佹湁鏂规硶 `MarkPreviousStepCompleted(AccountCycleStep?)`,璇箟涓庡師 inline 鍧椾竴鑷?`leftPhase2 \|\| lateStageOff`) |
| 5 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1813-1826` | 淇敼 | **A8**: `LinkStart` 椤堕儴鍔?`if (startUpConfig.IsCycling) { Release; return; }` guard,闃叉 Stop 鍚庡啀娆＄偣鍑?/ 瀹氭椂鍣?/ 蹇嵎閿湪 cycle 涓噸缃繘搴?|
| 6 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml:140` | 淇敼 | **A7**: `LateStageRogueAndReclamation` CheckBox 鍔?`IsEnabled="{c:Binding '!IsCycling'}"`,Cycle 杩愯涓伆鏄?|
| 7 | `LOG.md` | 淇敼 | 鏈妭 |

**缂栬瘧缁撴灉**:
```
dotnet build src/MaaWpfGui/MaaWpfGui.csproj -c Release -p:Platform=x64
0 涓敊璇? 6 涓?warning (3 涓?SA1503 鏉ヨ嚜鍘?feat/defer-rogue 浠ｇ爜 2279/2329/2333 琛?涓庢湰娆′慨澶嶆棤鍏?
```

**鍏煎鎬ф牳鏌?*:
- A1 淇浠呮敼鍙?last step 璺緞鐨勬爣璁版椂鏈?涓嶅奖鍝嶄腑闂存楠?- A7 浠?UI 灞傜鐢?VM 琛屼负涓嶅彉
- A8 浠呭湪 `IsCycling == true` 鏃舵棭閫€,涓嶈繘鍏?LinkStart 涓绘祦绋?涓嶄細鏀瑰彉宸叉湁琛屼负;鐢ㄦ埛浣撴劅涓?杞崲杩愯鏃跺啀鐐瑰紑濮嬫寜閽病鍙嶅簲"(绗﹀悎棰勬湡)
- AGENTS.md 浠呮枃妗ｆ敼鍔?鏃犱唬鐮佸奖鍝?
**閮ㄧ讲楠岃瘉**: (寰呰ˉ鍏?闇€鍦?`install/` 鍚姩 MAA 璧颁竴閬?2 璐﹀彿 cycle,纭鏈€鍚庝竴涓处鍙?IsCompleted 鍙樿摑)
**閮ㄧ讲楠岃瘉**: (寰呰ˉ鍏?闇€鍦?`install/` 鍚姩 MAA 璧颁竴閬?2 璐﹀彿 cycle,纭鏈€鍚庝竴涓处鍙?IsCompleted 鍙樿摑)

### fix/account_rotation/淇敼娆℃暟 鈥?娴嬭瘯鍙戠幇鐨?6 椤?cycle 琛屼负寮傚父淇

浠?`feat/defer-rogue` 鎷夊嚭 `fix/account_rotation/淇敼娆℃暟`銆傝 fix 鍚屾椂淇 `feat/defer-rogue` 涓?`feat/account_rotation` 浜や簰浜х敓鐨勭己闄凤紝鍚堝苟鐩爣涓?`feat/defer-rogue`锛堝湪渚濊禆閾句腑浣嶄簬涓嬫父锛夈€?
**娴嬭瘯鍙戠幇鐨勯棶棰?*:
- A 璐﹀彿 Phase 1 鏄剧ず涓や釜"鏇存柊鏁版嵁"锛堝疄涓?OperBox+Depot 瀛愪换鍔″叡鐢?UserDataUpdate 鍚嶇О锛?- B 璐﹀彿 Phase 1 鏄剧ず甯︽嫭鍙风殑"(骞插憳璇嗗埆)/(浠撳簱璇嗗埆)"锛圱askItemViewModel taskId 鏌ユ壘澶辫触 鈫?绱㈠紩涓嶄竴鑷达級
- 鑲夐附璺戦敊闃舵锛堝彲鑳芥槸涓婂彿鐘舵€佹畫鐣欙級
- UserDataUpdate 鐨?`IsTriggerDue` 璺ㄨ处鍙疯烦杩?鈫?瀛愪换鍔′涪澶?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/UserDataUpdateSettingsUserControlModel.cs:91-97` | 淇敼 | **#1**: cycle 涓?(`GetAccountSwitchEnabled()`) 璺宠繃 `IsTriggerDue` 妫€鏌?淇濊瘉姣忎釜璐﹀彿鐨?OperBox/Depot 瀛愪换鍔￠兘琚拷鍔?|
| 2 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2155-2192` | 淇敼 | **#6**: `SetStopped` 灏?cycling 妫€鏌ョЩ鍒?idle 妫€鏌ヤ箣鍓?褰?`IsCycling=true && Idle=true`锛圠inkStartWithTasks 鏃╅€€璺緞锛夋椂娓呯悊 cycling 鐘舵€?璁╂甯稿仠姝㈡帴绠?闃叉杞崲姘镐箙鍗′綇 |
| 3 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2232-2234` | 淇敼 | **#5**: `AdvanceAccountCycle` 鍏ュ彛鍔?`_logger.Information` 鏃ュ織璁板綍 stepIdx/prev/next 淇℃伅 |
| 4 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs:326` | 淇敼 | **#5**: 鏂板 `CurrentStepIndex` 鍏紑灞炴€ф敮鎸佹棩蹇?|
| 5 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2360-2362` | 淇敼 | **#5**: AdvanceAccountCycle 寰幆鍚庤拷鍔?`_logger.Information` 璁板綍 phase/switch/count/ret |
| 6 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2337-2339` | 淇敼 | **#5**: AdvanceAccountCycle 涓?Append task 鏃惰褰?`[CycleAdv] Append task #Idx` 鏃ュ織 |
| 7 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1983-1985` | 淇敼 | **#5**: LinkStartWithTasks 涓?Append task 鏃惰褰?`[LinkStart] Append task #Idx` 鏃ュ織 |
| 8 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2287-2358` | 淇敼 | **#2/#3**: AdvanceAccountCycle 鐨?Phase 浠诲姟寰幆鐢?foreach + `IndexOf` 鏀逛负 **for 寰幆** (`int index = i`),娑堥櫎閲嶅椤?椤哄簭鍙樻洿鏃剁殑绱㈠紩閿欒;鍚屾椂淇濇寔鍘熸湁 Phase 杩囨护/StartUp 璺宠繃/`SetTaskIds` 閫昏緫涓嶅彉 |
| 9 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2256` | 淇敼 | **#4**: AdvanceAccountCycle 鍒濆鏃ュ織杩藉姞 `idx={CurrentStepIndex}/{CurrentStepCount}` 鏄剧ず姝ラ浣嶇疆 |
| 10 | `AGENTS.md` | 淇敼 | 鏂板璺ㄥ涓?feat 鐨?fix 鍒嗘敮鍛藉悕绾︽潫:鍚堝苟鐩爣閫変緷璧栭摼鏈€涓嬫父鐨?feat,PR 璇存槑鍒楀嚭鎵€鏈夋秹鍙?feat |
| 11 | `LOG.md` | 淇敼 | 鏈妭 |

**缂栬瘧缁撴灉**: `dotnet build -c Release` 0 error, 0 warning

**鍏煎鎬ф牳鏌?*:
- #1 浠呭湪 cycle 杩愯鏃惰烦杩?`IsTriggerDue`,闈?cycle 璺緞琛屼负涓嶅彉
- #6 浠呭湪 `IsCycling=true` 鏃舵寜鏂伴『搴忓懡涓?闈?cycle 璺緞瀹屽叏绛変环
- #2/#3 for 寰幆涓?foreach 琛屼负鍦ㄦ棤閲嶅椤规椂瀹屽叏鐩稿悓;鍘?foreach + IndexOf 鍦ㄦ湁閲嶅椤规椂浼氳繑鍥為涓尮閰嶇储寮曞鑷撮敊璇?UI 鏄剧ず,for 寰幆淇姝ら棶棰?- AGENTS.md 浠呮枃妗ｆ敼鍔?鏃犱唬鐮佸奖鍝?
## 2026-07-16

### 宸插畬缁撳姛鑳藉垎鏀湰鍦板垹闄?
灏嗗凡鍚堝叆 `branch` 鐨勫姛鑳藉垎鏀粠鏈湴鍒犻櫎锛岃繙绔繚鐣欍€?
| # | 鍒嗘敮 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `feat/account_rotation` | `git branch -d` | 宸插悎鍏?branch锛屾湰鍦板垹闄?|
| 2 | `feat/defer-rogue` | `git branch -d` | 宸插悎鍏?branch锛屾湰鍦板垹闄?|
| 3 | `AGENTS.md` | 淇敼 | 杩藉姞銆屽垎鏀敓鍛藉懆鏈熻褰曘€嶇珷鑺?|
| 4 | `LOG.md` | 淇敼 | 鏈妭 |

### AGENTS.md 鍏ㄩ噺閲嶅啓

宸ヤ綔鍖轰腑 `AGENTS.md` 闀挎湡澶勪簬 modified 鐘舵€侊紙浠?24 琛岋紝鍙繚鐣欏垎鏀敓鍛藉懆鏈熻褰曪級锛岀己澶卞懡鍚嶇害瀹?/ 妯℃澘 / 宸ヤ綔娴?/ 鏋勫缓 / 浠ｇ爜椋庢牸 / 杩涜涓垎鏀€熸煡绛夊叧閿珷鑺傘€傛寜鐢ㄦ埛璇锋眰**鍏ㄩ噺閲嶅啓**涓?9 绔犺妭 ~318 琛岀粨鏋勶細椤圭洰姒傝堪 / 浠撳簱鎷撴墤 / 宸ヤ綔娴佷笌鏂囨。瑙勮寖 / feat路fix 宸ヤ綔绗旇妯℃澘 / 鏋勫缓路閮ㄧ讲路鍙戝竷 / 浠ｇ爜椋庢牸涓庤川閲?/ 杩涜涓垎鏀€熸煡 / 鍒嗘敮鐢熷懡鍛ㄦ湡璁板綍 / 鍏抽敭鍙傝€冮摼鎺ャ€傛ā鏉跨珷鑺傛仮澶嶈嚜 HEAD 涓婁竴涓増鏈紙`2b6517fa7a`锛夌殑鍐呭骞舵墿鍏呫€屽疄鏂藉悗杩藉姞銆嶆钀姐€?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `AGENTS.md.local.bak` | 鏂板缓 鈫?鍒犻櫎 | 澶囦唤鏃?24 琛屽伐浣滃尯鐗堟湰锛涙柊鐗堟湰鍐欏叆骞惰嚜妫€鍚庡垹闄わ紙閬垮厤姹℃煋 git 宸ヤ綔鍖猴紝鏂板唴瀹瑰凡鍐欏叆姝ｅ紡鏂囦欢锛?|
| 2 | `AGENTS.md:1-389` | 鍏ㄩ噺閲嶅啓 | 9 绔犺妭缁撴瀯锛浡? 妯℃澘鍚€屽疄鏂借褰?/ 韪╁潙 / 寰呮墜鍔ㄩ獙璇併€嶃€孋ode Review 璁 / 鍏煎鎬ф牳鏌?/ 缂栬瘧閮ㄧ讲缁撴灉銆嶆墿灞曟 |
| 3 | `LOG.md` | 淇敼 | 鏈妭 |

**绔犺妭杩炵画鎬ц嚜妫€**锛?
- 搂1 椤圭洰姒傝堪锛埪?.1 瀹氫綅 / 搂1.2 鎶€鏈爤 / 搂1.3 骞冲彴涓庤鍙瘉锛?- 搂2 浠撳簱鎷撴墤涓庡垎鏀ā鍨嬶紙搂2.1 杩滅▼ / 搂2.2 鏈湴鍒嗘敮 / 搂2.3 宸插畬缁?feat 鍒嗘敮澶勭悊绾﹀畾锛?- 搂3 宸ヤ綔娴佷笌鏂囨。瑙勮寖锛埪?.1 宸ヤ綔绗旇鍛藉悕涓庝繚鐣?/ 搂3.2 璺熻釜鏂囨。绛栫暐 / 搂3.3 鍚姩鏂?feat 鏍囧噯娴佺▼ / 搂3.4 fix 鍒嗘敮鍛藉悕涓庡悎骞剁洰鏍?/ 搂3.5 鏈湴鍖栵級
- 搂4 feat / fix 宸ヤ綔绗旇妯℃澘锛埪?.1 feat 妯℃澘 / 搂4.2 fix 妯℃澘锛?- 搂5 鏋勫缓銆侀儴缃蹭笌鍙戝竷锛埪?.1 C++ 绔?/ 搂5.2 WPF 绔?/ 搂5.3 鏈湴杩愯 / 搂5.4 鎵撳寘鍙戝竷 / 搂5.5 瀛愭ā鍧?/ 搂5.6 杈呭姪宸ュ叿鑴氭湰锛?- 搂6 浠ｇ爜椋庢牸涓庤川閲忥紙搂6.1 C++ / 搂6.2 C# / 搂6.3 鎻愪氦鍓嶆鏌ワ級
- 搂7 杩涜涓垎鏀€熸煡锛坒eat/expedite-threshold 涓€琛岄€熸煡琛級
- 搂8 鍒嗘敮鐢熷懡鍛ㄦ湡璁板綍锛埪?.1 feat/account_rotation / 搂8.2 feat/defer-rogue / 搂8.3 feat/expedite-threshold 鍗犱綅锛?- 搂9 鍏抽敭鍙傝€冮摼鎺ワ紙搂9.1 涓婃父鏂囨。 / 搂9.2 鏈粨闆嗘垚绀轰緥 / 搂9.3 涓婃父鍏宠仈椤圭洰锛?
**鍏煎鎬ф牳鏌?*锛?
- 涓?HEAD 涓婁竴涓?AGENTS.md 鐗堟湰锛坄2b6517fa7a`锛夌浉姣旓細搂4 妯℃澘绔犺妭鎵╁睍 8 涓€屽疄鏂藉悗杩藉姞銆嶆钀斤紙鍘熺増浠?5 涓級锛屼究浜庤褰?commit / 鍏煎鎬ф牳鏌?/ 缂栬瘧鏃ュ織
- 搂7銆岃繘琛屼腑鍒嗘敮閫熸煡銆嶄负鏂板绔犺妭锛屼笌 搂8銆屽垎鏀敓鍛藉懆鏈熻褰曘€嶅舰鎴愩€岃繘琛屼腑 / 宸插畬缁撱€嶅鐓?- 搂5 / 搂6 / 搂9 鍏ㄩ儴涓烘湰鐗堟湰鏂板鍐呭
- 鏃㈡湁 搂8 涓や釜宸插畬缁?feat 鏉＄洰锛坒eat/account_rotation / feat/defer-rogue锛夊師鏍蜂繚鐣?
**鏈彁浜ゅ師鍥?*锛氱敤鎴锋湭鏄庣‘瑕佹眰 commit锛屾寜 AGENTS.md銆屾彁浜ょ瓥鐣ャ€嶇害瀹氫繚鎸佸伐浣滃尯鐘舵€併€?
### AGENTS.md 鍘婚櫎 feat / fix 宸ヤ綔绗旇锛堢浜屾閲嶅啓锛?
鎸夌敤鎴疯繘涓€姝ユ寚浠わ細**涓嶈 feat 鎴?fix 宸ヤ綔绗旇**锛屼粎鍦?AGENTS.md 涓褰曞垎鏀瓨鍦ㄦ€у嵆鍙€?灏嗕笂涓€鐗?9 绔犺妭缁撴瀯缂╁噺涓?8 绔犺妭锛岀Щ闄や互涓嬪唴瀹癸細

- 搂3.1銆宖eat / fix 宸ヤ綔绗旇銆嶏紙鍛藉悕绾﹀畾 / 鍚勫垎鏀繚鐣欒鍒?/ 鎻愪氦绛栫暐鏁存鍒犻櫎锛?- 搂4銆宖eat / fix 宸ヤ綔绗旇妯℃澘銆嶏紙feat 妯℃澘 + fix 妯℃澘涓や釜浠ｇ爜鍧楁暣娈靛垹闄わ級
- 搂3.2銆岃窡韪枃妗ｇ瓥鐣ャ€嶄腑 `feat_<name>.md` / `fix_<name>[_<n>].md` 琛岋紙鍦ㄣ€屾槸鍚﹁窡韪€嶈〃涓級
- 搂3.2銆屽惎鍔ㄦ柊 feat 鏍囧噯娴佺▼銆嶅師姝ラ 3/4锛堝垱寤轰笌缁存姢宸ヤ綔绗旇锛夊垹闄わ紝姝ラ缂栧彿鍘嬬缉
- 搂7銆岃繘琛屼腑鍒嗘敮閫熸煡銆嶈〃澶村垹闄ゃ€屽伐浣滄枃妗ｃ€嶅垪锛堟棤宸ヤ綔绗旇鍚庢鍒楁棤鎰忎箟锛?
鏂板锛氬紑澶翠竴娈垫憳瑕佹槑纭€屾湰浠撳簱涓嶄娇鐢?feat / fix 宸ヤ綔绗旇銆嶏紝搂3.1 鍔?blockquote 寮鸿皟锛屾墍鏈夊彉鏇撮€氳繃 commit message 涓?LOG.md 璺熻釜銆?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `AGENTS.md:1-269` | 浜屾閲嶅啓 | 9 绔犺妭 鈫?8 绔犺妭锛浡? 鐢?5 灏忚妭鍘嬬缉涓?4 灏忚妭锛浡?-搂9 閲嶆柊缂栧彿涓?搂4-搂8 |
| 2 | `LOG.md` | 淇敼 | 鏈妭杩藉姞銆孉GENTS.md 鍘婚櫎 feat / fix 宸ヤ綔绗旇銆嶈褰曪紱鍚屾椂淇鍓嶈妭銆孉GENTS.md 鍏ㄩ噺閲嶅啓銆嶄腑鐨勬弿杩?|

**鏂扮粨鏋勮嚜妫€**锛?
- 搂1 椤圭洰姒傝堪锛埪?.1 瀹氫綅 / 搂1.2 鎶€鏈爤 / 搂1.3 骞冲彴涓庤鍙瘉锛?- 搂2 浠撳簱鎷撴墤涓庡垎鏀ā鍨嬶紙搂2.1 杩滅▼ / 搂2.2 鏈湴鍒嗘敮 / 搂2.3 宸插畬缁?feat 鍒嗘敮澶勭悊绾﹀畾锛?- 搂3 宸ヤ綔娴佷笌鏂囨。瑙勮寖锛埪?.1 璺熻釜鏂囨。绛栫暐 / 搂3.2 鍚姩鏂?feat 鏍囧噯娴佺▼ / 搂3.3 fix 鍒嗘敮鍛藉悕涓庡悎骞剁洰鏍?/ 搂3.4 鏈湴鍖栵級
- 搂4 鏋勫缓銆侀儴缃蹭笌鍙戝竷锛埪?.1 C++ 绔?/ 搂4.2 WPF 绔?/ 搂4.3 鏈湴杩愯 / 搂4.4 鎵撳寘鍙戝竷 / 搂4.5 瀛愭ā鍧?/ 搂4.6 杈呭姪宸ュ叿鑴氭湰锛?- 搂5 浠ｇ爜椋庢牸涓庤川閲忥紙搂5.1 C++ / 搂5.2 C# / 搂5.3 鎻愪氦鍓嶆鏌ワ級
- 搂6 杩涜涓垎鏀€熸煡锛坒eat/expedite-threshold 涓€琛岄€熸煡琛紝宸插幓闄ゅ伐浣滄枃妗ｅ垪锛?- 搂7 鍒嗘敮鐢熷懡鍛ㄦ湡璁板綍锛埪?.1 feat/account_rotation / 搂7.2 feat/defer-rogue / 搂7.3 feat/expedite-threshold 鍗犱綅锛?- 搂8 鍏抽敭鍙傝€冮摼鎺ワ紙搂8.1 涓婃父鏂囨。 / 搂8.2 鏈粨闆嗘垚绀轰緥 / 搂8.3 涓婃父鍏宠仈椤圭洰锛?
**鍏煎鎬ф牳鏌?*锛?
- 鍒犻櫎銆宖eat_<name>.md / fix_<name>[_<n>].md 鍛藉悕绾﹀畾銆嶃€屾ā鏉跨珷鑺傘€嶆剰鍛崇潃鍚庣画鑻ユ湁浜烘兂寤哄伐浣滅瑪璁板皢鏃犺剼鎵嬫灦鍙敤鈥斺€旀寜鐢ㄦ埛瑕佹眰鐩存帴鐮嶆帀锛屼笉鍐嶄繚鐣?- 鏃?commit `2b6517fa7a` / `5fcff1e27f` 寮曞叆鐨勬ā鏉垮交搴曚粠鏂囨。绉婚櫎锛屼絾 git 鍘嗗彶涓彲鍥炴函
- `.gitignore` 涓繚鐣欑殑 `feat*.md` / `fix*.md` 瑙勫垯鍙樹负銆屾浘浣跨敤杩囥€嶇殑姝讳唬鐮侊紱鎸夌敤鎴枫€屼笉瑕?feat 鎴?fix 宸ヤ綔绗旇銆嶇殑鑼冨洿澶栵紝鏈Е鍔?`.gitignore`
- 搂7銆屽垎鏀敓鍛藉懆鏈熻褰曘€嶅師鏍蜂繚鐣欎笁涓潯鐩紙account_rotation / defer-rogue / expedite-threshold 鍗犱綅锛?- 搂6銆岃繘琛屼腑鍒嗘敮閫熸煡銆嶄繚鐣欎竴琛岃〃锛屽幓鎺夈€屽伐浣滄枃妗ｃ€嶅垪

### AGENTS.md 椋庢牸瀵归綈鍘熼」鐩紙绗笁娆￠噸鍐欙級

鎸夌敤鎴峰己璋冿細銆岃寮鸿皟鍘熼」鐩鏍煎拰涔﹀啓涔犳儻锛岃繖鐐瑰緢閲嶈銆嶃€?瀵归綈鐩爣涓?`LOG.md`锛堜粨搴撲綔鑰?levellmy 鏃㈡湁鐨勬潯鐩鏍硷級涓庝笂娓?`docs/zh-cn/develop/*.md`锛坴uepress 绔欑偣璇枡锛夛紝瑕佺偣锛?
| 缁村害 | 璋冩暣鍓?| 璋冩暣鍚?|
|------|--------|--------|
| 娈佃惤 vs 琛ㄦ牸 | 澶ч噺 prose 娈佃惤鎻忚堪 | 浼樺厛 markdown 琛ㄦ牸锛岃鍐?`**key**锛歷alue` 浠呬綔鑴氭敞 |
| 绔犺妭娣卞害 | 3 绾э紙搂1.1.1锛?| 2 绾э紙搂1.1锛夛紝鍑忓皯宓屽 |
| 鏂囦欢寮曠敤 | 瀹屾暣璺緞鏁ｈ惤娈典腑 | `path:line` 闆嗕腑鏀惧湪琛ㄦ牸銆屾枃浠?/ 鎿嶄綔銆嶅垪 |
| emoji / vuepress 瀹瑰櫒 | 鍋舵湁 `> [!note]` 璇曟帰 | 鍏ㄩ儴绉婚櫎锛屾棤 emoji銆佹棤 `::: tip` |
| 鍏抽敭鏈 | 涓嫳娣锋潅锛屾棤鏄庣‘淇濈暀瑙勫垯 | 鏄庣‘銆宐ranch / feat / fix / FF / PR / 瀛愭ā鍧?/ cmake / dotnet銆嶇瓑淇濈暀鑻辨枃 |
| 绔犺妭澶ф | 鏁存銆屽惎鍔ㄦ柊 feat 娴佺▼銆嶇敤 prose 涓叉帴 | 鏀逛负姝ラ琛紙`\| # \| 姝ラ \| 浜х墿 \|`锛夛紝涓?LOG.md 琛ㄦ牸鍒椾竴鑷?|
| 绔犺妭灏忕粨 | 鏈熬鏁ｆ枃鎬荤粨 | 鍒犻櫎锛岀洿鎺ヤ互琛ㄦ牸鏀舵潫 |

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `AGENTS.md:1-242` | 涓夋閲嶅啓 | 8 绔犺妭 + 20 浜岀骇灏忚妭锛?8 寮犺〃鏍硷紱琛屾暟 212 鈫?224锛?12 琛岋紝浣?prose 琛屾暟浠?~70 闄嶈嚦 ~30锛岃〃鏍艰鏁颁粠 ~70 澧炶嚦 ~140锛夛紱琛ㄦ牸瀵嗗害锛坄\|` 璧峰琛岋級浠?71 澧炶嚦 128 |
| 2 | `LOG.md` | 淇敼 | 鏈妭杩藉姞銆孉GENTS.md 椋庢牸瀵归綈鍘熼」鐩€嶈褰?|

**绔犺妭杩炵画鎬ц嚜妫€**锛?
- 搂1 椤圭洰姒傝堪锛埪?.1 涓€鍙ヨ瘽 / 搂1.2 鎶€鏈爤 / 搂1.3 骞冲彴涓庤鍙級
- 搂2 浠撳簱鎷撴墤涓庡垎鏀ā鍨嬶紙搂2.1 杩滅▼ / 搂2.2 鏈湴鍒嗘敮 / 搂2.3 宸插畬缁?feat 澶勭悊锛?- 搂3 宸ヤ綔娴佷笌鏂囨。瑙勮寖锛埪?.1 璺熻釜鏂囨。绛栫暐 / 搂3.2 鍚姩鏂?feat 鏍囧噯娴佺▼ / 搂3.3 fix 鍒嗘敮鍛藉悕涓庡悎骞剁洰鏍?/ 搂3.4 鏈湴鍖栵級
- 搂4 鏋勫缓銆侀儴缃蹭笌鍙戝竷锛埪?.1 鏋勫缓鍛戒护 / 搂4.2 瀛愭ā鍧?/ 搂4.3 鎵撳寘鍙戝竷 / 搂4.4 杈呭姪鑴氭湰锛?- 搂5 浠ｇ爜椋庢牸涓庤川閲?- 搂6 杩涜涓垎鏀€熸煡
- 搂7 鍒嗘敮鐢熷懡鍛ㄦ湡璁板綍锛埪?.1 account_rotation / 搂7.2 defer-rogue / 搂7.3 expedite-threshold 鍗犱綅锛?- 搂8 鍏抽敭鍙傝€冮摼鎺ワ紙搂8.1 涓婃父鏂囨。 / 搂8.2 鏈粨闆嗘垚绀轰緥 / 搂8.3 涓婃父鍏宠仈椤圭洰锛?
**鍏煎鎬ф牳鏌?*锛?
- 绔犺妭缂栧彿涓庝笂涓€鐗堝畬鍏ㄤ竴鑷达紙8 绔犺妭锛屼粎 搂1.1 / 搂1.3 鏍囬鏂囨浠庛€屽畾浣?/ 骞冲彴涓庤鍙瘉銆嶅井璋冧负銆屼竴鍙ヨ瘽 / 骞冲彴涓庤鍙€嶏級锛屾棤寮曠敤澶辨晥
- 浜岀骇灏忚妭鏁颁粠 21 鍑忚嚦 20锛堝悎骞?搂5 涓変釜 prose 娈佃惤涓哄崟涓€琛ㄦ牸锛?- 鎵€鏈夈€屾枃浠惰矾寰?+ 琛屽彿銆嶆弿杩板凡杩佺Щ鑷宠〃鏍煎垪锛宲rose 涓笉鍐嶅嚭鐜?- 搂3.2銆屽惎鍔ㄦ柊 feat銆嶇敱 7 姝?prose 鏀逛负 7 琛屾楠よ〃锛屼笌 LOG.md 琛ㄦ牸鍒椾竴鑷?- 搂4.1銆屾瀯寤哄懡浠ゃ€嶇敱 prose bullet 鏀逛负鍗曡〃锛屼笌 LOG.md 鎿嶄綔鍒椾竴鑷?
**鏈彁浜ゅ師鍥?*锛氱敤鎴锋湭鏄庣‘瑕佹眰 commit銆?
## 2026-07-23

### 鍒犻櫎鏃犵敤鍒嗘敮

| # | 鎿嶄綔 | 璇存槑 |
|---|------|------|
| 1 | 鍒犻櫎鍒嗘敮 `feat/expedite-threshold` | 鏈湴宸插垹闄わ紝杩滅淇濈暀锛圚EAD `5fcff1e27f`锛屾湭鍚堝叆锛墊
| 2 | 鍒犻櫎鍒嗘敮 `feat/idea` | 鏈湴宸插垹闄わ紝杩滅淇濈暀锛圚EAD `dc2212d54b`锛屾棤鐙珛 commit锛墊
| 3 | 鍒犻櫎鍒嗘敮 `fix/account_rotation/5` | 鏈湴宸插垹闄わ紝杩滅淇濈暀锛圚EAD `83a5dc36c1`锛墊
| 4 | 鏇存柊 AGENTS.md 搂6 / 搂7 | `feat/expedite-threshold` 绉诲叆鐢熷懡鍛ㄦ湡璁板綍锛涙柊澧?`feat/idea` 璁板綍锛沗fix/account_rotation/5` 璁板叆 搂7.1 瀛愪慨澶嶅垎鏀?|

### feat/expedite-threshold 鍚姩

| # | 鏂囦欢璺緞 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h` | 鏂板 setter `set_expedite_min_level` | 鍔犳€ラ棬妲涙帴鍙?|
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h` | 鏂板鎴愬憳 `m_expedite_min_level` / `m_last_confirmed_min_level` | 闂ㄦ鍊间笌鏈€杩戠‘璁ゆ槦绾?|
| 3 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | 鏂板 `set_expedite_min_level` 瀹炵幇 | setter |
| 4 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | `_run()` 绉婚櫎鏃х殑 `try_use_expedited` 鍧?| 鏀圭敱 `recruit_one()` 鍐呴€愭Ы鍒ゅ畾 |
| 5 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | `recruit_calc_task()` 鍐欏叆 `m_last_confirmed_min_level` | 鍔犳€ュ喅绛栦緷鎹?|
| 6 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | `recruit_one()` 鍔犳€ュ垎鏀?| 4鈽? 鏃?`recruit_now()` 鏇夸唬 `confirm()` |
| 7 | `src/MaaCore/Task/Interface/RecruitTask.cpp` | 瑙ｆ瀽 `expedite_min_level` 鍙傛暟 | 鏂板弬鏁伴€忎紶 |
| 8 | `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs` | +`ExpediteMinLevel` 灞炴€?| 閰嶇疆妯″瀷 |
| 9 | `src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs` | +`ExpediteMinLevel` 灞炴€?+ 搴忓垪鍖?| DTO |
| 10 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs` | +`ExpediteMinLevelList` / `UseExpeditedMinLevel` / `UseExpeditedMinLevelVisible` | ViewModel |
| 11 | `src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml` | +闂ㄦ涓嬫媺妗?| UI |
| 12 | `src/MaaWpfGui/Res/Localizations/{zh-cn,en-us,ja-jp,ko-kr,zh-tw}.xaml` | +`ExpediteMinLevel*` 6 涓?key | 浜旇鏈湴鍖?|

### feat/expedite-threshold 鍚堝苟鍏?branch

鍦?`feat/expedite-threshold` 涓婅ˉ榻愭湭鏆傚瓨鐨?UI/鏈湴鍖栭€傞厤缁嗚妭鍚庯紝FF 鍚堝苟鍒?`branch`锛屾湰鍦板垹闄?feat 鍒嗘敮銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `branch` | FF 鍚堝苟 | `feat/expedite-threshold` 3 涓柊 commit锛坄7df4e94e3f`锛坒eat 鍒濆锛? `9c02b9e41a`锛堟殏瀛樺墠缃彉鏇达級+ `cbec3d1fb0`锛堝畬鍠勪簲璇湰鍦板寲涓?ViewModel 閫傞厤锛夛級锛孒EAD `dc2212d54b` 鈫?`cbec3d1fb0` |
| 2 | `feat/expedite-threshold` | `git branch -d` | 宸插悎鍏?branch锛屾湰鍦板垹闄?|
| 3 | `AGENTS.md` | 淇敼 | 搂6 娓呯┖锛堟棤杩涜涓垎鏀級锛浡?.3 鏇存柊涓哄凡鍚堝叆鐘舵€侊紱搂7 寮€澶磋ˉ鍏?2026-07-23 鍒犻櫎鏃ユ湡 |
| 4 | `LOG.md` | 淇敼 | 鏈妭 |

### fix/expedite-threshold 鍚姩

`feat/expedite-threshold`锛坄7df4e94e3f`锛夐噸鏋勬椂閬楀け浜?`3529ab0f05` 鍘熺増鍦?`recruit_one()` 鍏ュ彛涓庡姞鎬ユ垚鍔熷悗涓ゅ `m_last_confirmed_min_level = 0;` 閲嶇疆锛屽鑷翠笅涓€妲戒綅璇诲埌涓婁竴妲戒綅闄堟棫鏄熺骇浠嶆弧瓒?`m_last_confirmed_min_level >= m_expedite_min_level` 鑰岃鍔犳€ャ€俙fix/expedite-threshold` 浠?`branch` 鎷夊嚭锛岀洰鏍囦慨澶?`branch` 鑷韩銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `fix/expedite-threshold` | 鏂板缓鍒嗘敮 | 浠?`branch` 鎷夊嚭锛孒EAD 鍚?`9d8d021610` |
| 2 | `src/MaaCore/Task/Interface/RecruitTask.cpp:55-57` | 涓存椂璇婃柇鏃ュ織 | `[fix/expedite-threshold/diag] Recruit params: expedite=..., expedite_min_level=...`锛岀敤浜庡畾浣?WPF鈫扟SON鈫扖++ 閾捐矾鏄惁姝ｇ‘閫忎紶 |

### fix/expedite-threshold 瀹炴柦瀹屾垚

璇婃柇闃舵纭 `install/debug/maa.log` 涓?`expedite_min_level=4` 宸叉纭粠 WPF 搴忓垪鍖栧眰浼犲叆 C++锛岀湡鍑堕攣瀹氫负 C++ 鍐崇瓥閫昏緫缂洪噸缃紝鏃犻渶 WPF 閾捐矾淇銆傝繕鍘熻瘖鏂棩蹇楀悗瀹炴柦 2 涓?commit锛堝厛涓嶅悎骞跺叆 `branch`锛夈€?
| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:312-314` | 淇敼 | `recruit_one()` 鍏ュ彛澶勮ˉ鍥?`m_last_confirmed_min_level = 0;`锛屾潨缁濅笂涓€妲戒綅闄堟棫鍊兼薄鏌撴湰妲戒綅鍔犳€ュ喅绛?|
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:358-359` | 淇敼 | 鍔犳€ユ垚鍔燂紙`recruit_now()` 鎴愬姛锛夊悗琛ュ洖 `m_last_confirmed_min_level = 0;`锛岄槻姝笅涓€妲戒綅璇垽 |
| 3 | `src/MaaCore/Task/Interface/RecruitTask.cpp:55-57` | 鍥炴粴 | 璇婃柇鏃ュ織 `git checkout --` 杩樺師锛屼笉鍏ュ簱 |
| 4 | `docs/zh-cn/protocol/integration.md:266-270` | 淇敼 | `::: field name="expedite_min_level"` 瀛楁鍧楋紝`0 = 涓嶉檺`锛宍4/5/6 = 浠呭搴旀槦绾у強浠ヤ笂鍔犳€ |
| 5 | `docs/zh-tw/protocol/integration.md:266-270` | 淇敼 | 浜旇鍚屾锛氱箒涓?|
| 6 | `docs/en-us/protocol/integration.md:266-270` | 淇敼 | 浜旇鍚屾锛氳嫳鏂?|
| 7 | `docs/ja-jp/protocol/integration.md:266-270` | 淇敼 | 浜旇鍚屾锛氭棩鏂?|
| 8 | `docs/ko-kr/protocol/integration.md:255-259` | 淇敼 | 浜旇鍚屾锛氶煩鏂?|
| 9 | `AGENTS.md 搂6` | 淇敼 | 鐧昏 `fix/expedite-threshold` 涓鸿繘琛屼腑鍒嗘敮 |
| 10 | `LOG.md` | 淇敼 | 鏈妭 |

**Commit 閾?*锛?
| SHA | 鏍囬 |
|-----|------|
| `d73f61adc1` | `fix(expedite-threshold): 琛ュ洖 m_last_confirmed_min_level 閲嶇疆` |
| `20cd79d4ca` | `docs: 琛?expedite_min_level 瀛楁璇存槑` |

**缂栬瘧/閮ㄧ讲缁撴灉**锛?
| 闃舵 | 鍛戒护 | 缁撴灉 |
|------|------|------|
| 鏋勫缓 | `cmake --build build --target MaaCore` | OK锛堜粎 RelWithDebInfo 榛樿 config锛汳aaCore.dll 閲嶆柊鐢熸垚锛?|
| 瀹夎 | `cmake --install build` | OK锛圡aaCore.dll 閮ㄧ讲鑷?`install/`锛沗MAA.Updater.exe` 缂哄け涓庢湰 fix 鏃犲叧锛屽崟鐩爣鏋勫缓鏈Е鍙婂叾缂栬瘧锛?|

**鍏煎鎬ф牳鏌?*锛?
- 鏃?API 鐢ㄦ埛涓嶄紶 `expedite_min_level` 鈫?C++ 绔粯璁?0 = 涓嶉檺 鈫?鍏ㄥ姞鎬ワ紝琛屼负涓嶅彉
- 鏃?GUI 鐢ㄦ埛閰嶇疆鏂囦欢涓棤璇ュ瓧娈?鈫?JSON 鍙嶅簭鍒楀寲榛樿 0 + CheckBox 鏈嬀閫?鈫?鍏ㄥ姞鎬ワ紝琛屼负涓嶅彉
- 閲嶇疆浠呭湪 `recruit_one` 鍏ュ彛涓庡姞鎬ユ垚鍔熷悗瑙﹀彂锛屽鏈姞鎬ヨ矾寰勬棤鍓綔鐢紱`recruit_calc_task` 鐨?`m_last_confirmed_min_level = final_combination.min_level;` 鍐欏叆鏃舵満涓嶅彉锛坙ine 562锛?- docs 浜旇瀛楁鍧楀榻?`expedite_times` 娈佃惤鐨勬牱寮忥紙`<br>` 缁銆乣榛樿 0` 鏀舵潫锛?
**寰呮墜鍔ㄩ獙璇侊紙闇€妯℃嫙鍣ㄧ幆澧冿級**锛?
| # | 鍦烘櫙 | 鏈熸湜 |
|---|------|------|
| 1 | 闂ㄦ 4锛屽噯澶?4鈽?+ 3鈽?妲戒綅 | 4鈽?绔嬪嵆瀹屾垚锛?鈽?璧?9h 鍊掕鏃?|
| 2 | 闂ㄦ 5锛屽噯澶?5鈽?+ 3鈽?妲戒綅 | 5鈽?绔嬪嵆瀹屾垚锛?鈽?璧?9h 鍊掕鏃?|
| 3 | 闂ㄦ 6锛屽噯澶?6鈽?+ 5鈽?妲戒綅 | 6鈽?绔嬪嵆瀹屾垚锛?鈽?璧?9h 鍊掕鏃?|
| 4 | 闂ㄦ 0锛堜笉闄愶級锛屼换涓€缁勫悎 | 鍏ㄩ儴鍔犳€ワ紝琛屼负绛夊悓鏃х増 |
| 5 | 鍏虫帀銆屼娇鐢ㄥ姞鎬ヨ鍙€?| 鎵€鏈夋Ы浣嶈蛋鑷劧鍊掕鏃?|

**鍚堝叆绛栫暐**锛氭殏涓嶅悎骞讹紱鐢辩敤鎴峰悗缁喅瀹?FF锛坈ommit 閾剧嚎鎬э級鎴?`--no-ff`锛堜繚鐣?fix 鎷撴墤锛夈€俙fix/expedite-threshold` 淇 `branch` 鑷韩锛屾寜 `搂3.3` 鍚堝苟鐩爣 = `branch`銆?
## 2026-07-24

### fix/account-official-recognize 鍚姩

銆屽紑濮嬪敜閱掋€嶄换鍔″湪 **瀹樻湇锛圤fficial锛? 璐﹀彿杞崲** 鍦烘櫙涓嬪崱姝汇€俙install/debug/asst.log` 瀹炴祴锛歁AA 鎴愬姛璇嗗埆 `StartToWakeUp.png`锛坰core 0.926锛変笌 `AccountManager.png`锛坰core 0.904锛夛紝浣?`LoginOther` 涔嬪悗 `AccountManagerOfficial` 涓?`AccountManagerBili` 30 娆?retry 鍏ㄥけ璐ワ紝`AccountSwitchTask::navigate_to_start_page()` 璧颁笉鍒颁换涓€鍚堟硶杩斿洖璺緞锛屾渶缁?`Login failed, entering game-restart loop`銆?
鏍瑰洜锛坄resource/tasks/tasks.json:805-807`锛夛細

```json
"AccountManagerOfficial": {
    "roi": [570, 165, 140, 80]    鈫?鍙湁 roi锛屾棤 template/algorithm/text
}
```

瀵规瘮 `AccountManagerBili`锛堢 808-813 琛岋級宸叉湁 `algorithm: "OcrDetect"` + `text: ["鐧诲綍璁板綍"]`锛屽畼鏈嶅畾涔夋畫缂恒€備慨澶嶆柟妗?A+C锛氳ˉ鍏ㄥ畼鏈?OCR 璇嗗埆 + `AccountSwitchTask::navigate_to_start_page()` 鍔犺瘖鏂棩蹇椼€備粎鍦ㄦ湰鍒嗘敮 `fix/account-official-recognize` 淇锛屼笉鎺ㄤ笂娓搞€?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `fix/account-official-recognize` | 鏂板缓鍒嗘敮 | 浠?`branch` 鎷夊嚭 |
| 2 | `resource/tasks/tasks.json:805-807` | 寰呬慨鏀?| `AccountManagerOfficial` 琛?OcrDetect 璇嗗埆銆岀櫥褰曡褰曘€?|
| 3 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:65-84` | 寰呬慨鏀?| `navigate_to_start_page()` 鍔犺瘖鏂棩蹇?|

### fix/account-official-recognize 瀹炴柦瀹屾垚

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `resource/tasks/tasks.json:805-810` | 淇敼 | `AccountManagerOfficial` 鐢?`{"roi":[570,165,140,80]}` 琛ュ叏涓?`{"Doc":"瀹樻柟鏈嶈处鍙峰垏鎹㈢晫闈㈣瘑鍒紝涓?B 鏈嶇粺涓€ OCR銆岀櫥褰曡褰曘€?,"algorithm":"OcrDetect","text":["鐧诲綍璁板綍"],"roi":[237,50,771,242]}`锛堜笌 B 鏈?`AccountManagerBili` 瀵归綈锛?|
| 2 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:71` | 淇敼 | `navigate_to_start_page()` 鍦?`get_last_task_name()` 涔嬪悗杩藉姞 `Log.info(__FUNCTION__, "last matched task:", last_name);`锛屼究浜庡悗缁瘑鍒け璐ユ椂瀹氫綅 |
| 3 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:71-77` | 淇敼 | 4 涓?`else if` 鍚堝苟涓哄崟 `if (... || ... \|\| ... \|\| ...)`锛屽噺灏戝垎鏀祵濂?|
| 4 | `install/MaaCore.dll` | 閮ㄧ讲 | Release 缂栬瘧浜х墿锛屾椂闂存埑 2026/7/24 12:45:38锛屽瓧鑺傜壒寰佷覆 `last matched task:` 鍛戒腑锛坥ffset 3384944 / size 4188160锛?|
| 5 | `install/resource/tasks/tasks.json` | 閮ㄧ讲 | 鍚屾婧愮 SHA256锛坄39972BD09F9EDEA0E7B7D71F8E084071126521E333F9DEDD3296115BC6F5C027`锛夛紝177232 瀛楄妭锛屽瓧鑺傜壒寰佷覆 `瀹樻柟鏈嶈处鍙峰垏鎹 鍛戒腑锛坥ffset 25825锛?|
| 6 | `LOG.md` | 淇敼 | 鏈妭 |

**缂栬瘧缁撴灉**: `cmake --build build --target MaaCore -j 4 --config Release` 鎴愬姛锛屼粎閬楃暀鏍囧噯 `LNK4098` 榛樿搴撹鍛婏紙涓庝笂娓镐竴鑷达級銆俙cmake --install build --config Release` 鎴愬姛锛坄MaaUpdater` 鎶ラ敊涓?AGENTS.md 搂4.1 宸茬煡 VS 2026 SDK 璺緞 bug锛屼笉褰卞搷 C++ 绔儴缃诧級銆?
**閮ㄧ讲浜х墿楠岃瘉**:
- `install/MaaCore.dll` 2026/7/24 12:45:38锛?188160 瀛楄妭锛?- `install/resource/tasks/tasks.json` 2026/7/24 12:45:11锛?77232 瀛楄妭锛?- 婧愮 `resource/tasks/tasks.json` 鈫?瀹夎绔?SHA256 瀹屽叏涓€鑷?
**棰勬湡鏁堟灉**:
1. 瀹樻湇璐﹀彿鍒囨崲鏃讹紝`LoginOther` 鈫?璇嗗埆 `鐧诲綍璁板綍` OCR 鈫?鍛戒腑 `AccountManagerOfficial` 鈫?杩斿洖 `true` 鈫?杩涘叆 `AccountSwitchTask::select_account()` 閫夋嫨鐩爣璐﹀彿
2. 鍗充究 OCR 浠嶆湭鍛戒腑锛宍Log.info("last matched task:", last_name)` 杈撳嚭瀹為檯鏈€鍚庡尮閰嶈妭鐐癸紝渚夸簬鎺掗敊
3. 鍗曡处鍙峰満鏅紙`AccountName` 涓虹┖鏃?`m_account_switch_task_ptr` 浠嶈 disable锛夎涓轰笉鍙?
**寰呮墜鍔ㄩ獙璇侊紙闇€妯℃嫙鍣ㄧ幆澧冿級**:
1. 鍗曡处鍙?+ StartGame=true 璺戝畬鏁存棩甯?鈫?StartUp 瀹屾垚杩涘叆棣栭〉
2. 鍙岃处鍙疯疆鎹紙192鈫?89锛夆啋 楠岃瘉 `AccountManagerOfficial` 鍛戒腑锛屾帶鍒跺彴/鏃ュ織鏄剧ず `last matched task: AccountManagerOfficial`
3. B 鏈嶅洖褰掓祴璇?鈫?涓嶇牬鍧?B 鏈嶅師鏈?`鐧诲綍璁板綍` OCR 琛屼负
4. 鍒囧彿涓€斿紓甯?鈫?鐪?`last matched task:` 杈撳嚭鏄惁浠嶆湁璇婃柇淇℃伅

**鏈帹閫佷笂娓?*: 浠呮湰浠撳簱 `branch` 淇锛屼笉鍚?upstream 鎻?PR銆?
## 2026-07-25

### fix/account_rotation/6 鍚姩

浼氬瀹ゃ€屽～鍏呯嚎绱㈢┖浣嶃€?`use_clue` 鈫?`proc_clue_vacancy`) 鍦ㄥ畼鏈?B 鏈嶅潎瀛樺湪 6 涓牴鍥狅紙鍙?7 鏈?26 鏃ヤ細璇濆垎鏋愶級锛屼笂娓?Issue #16166 宸?closed as "not planned"銆傛湰 fix 鍦?`branch` 涓婄嫭绔嬩慨澶嶏紝涓嶆帹 upstream銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `fix/reception-clue-vacancy` | 鏂板缓鍒嗘敮 | 浠?`branch` 鎷夊嚭 |
| 2 | `LOG.md` | 淇敼 | 鏈妭 |

### fix/reception-clue-vacancy 瀹炴柦瀹屾垚


### fix/reception-clue-vacancy 鍚堝叆 staging

鎸?AGENTS.md 搂3.3 淇 branch 鑷韩鐨?fix 鈫?鍚堝苟鍒?`staging` 娴佺▼锛屼互 `--no-ff` 鍚堝苟锛宑ommit `ad725916b4`锛坒ix 浠ｇ爜锛? 鏈?merge commit銆?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `staging` | `--no-ff` 鍚堝苟 | `fix/reception-clue-vacancy` 1 commit锛坄ad725916b4` 浠ｇ爜 + LOG.md锛夛紝HEAD 鍗冲皢鏇存柊 |
| 2 | `LOG.md` | 淇敼 | 鏈妭 |
| 3 | `fix/reception-clue-vacancy` 鏈湴鍒嗘敮 | 淇濈暀 | 鎸?搂2.3 娴佺▼闇€鍏堟檵鍗?`branch` 鍚庢墠鑳?`git branch -d`锛涘洜 搂2.4銆屼笉鍏佽闅忎究鍚屾鑷?branch銆嶇害鏉熸湭鍔?`branch`锛屾殏淇濈暀 |

**鍐茬獊瑙ｅ喅**: LOG.md 1 澶?conflict marker锛屾寜浠ヤ笅绛栫暐瑙ｅ喅锛?- 淇濈暀 staging 绔?`fix/account_rotation/6` / `fix/account-switch-template-missing` / `fix/account-switch-retry 鍚堝叆 staging` / `fix/account_rotation/6 鍚堝叆 staging` / `fix/account-switch-template-missing 鍚堝叆 staging` / `chore/account-cycle-status-sync` 鍚姩 / `fix/recruit-now-text-aliases` 鍚姩 + 瀹炴柦瀹屾垚 / `fix/post-battle-sanity-display` 鍚姩 + 瀹炴柦瀹屾垚 + 鍚堝叆 staging 鍏?8 鑺傚巻鍙插唴瀹?- 鍦ㄦ湯灏?append fix 鍒嗘敮鐙湁鐨?`fix/reception-clue-vacancy` 鍚姩 + 瀹炴柦瀹屾垚 + 鍚堝叆 staging 涓夎妭
- 鍒犻櫎 `=======` / `>>>>>>> fix/reception-clue-vacancy` marker锛屼繚鐣欏弻鏂圭珷鑺?## 2026-07-28

## 2026-07-28

### feat/downstream-changes 鍚姩

涓烘敮鎸併€屾敼浠讳綍鏂囦欢鍓嶅厛鏌ヤ笅娓告敼鍔ㄦ枃浠舵竻鍗曘€嶆祦绋嬶紝鏂板涓€涓嚜鍔ㄤ粠 LOG.md 琛ㄦ牸鎻愬彇鎵€鏈夎鏀硅繃鏂囦欢銆佹寜鐩綍鍒嗙粍鐢熸垚 markdown 鏂囨。鐨勫伐鍏?+ 棣栨浜х墿銆?
鑴氭湰鍙瑙ｆ瀽 LOG.md 鐨?4 鍒椼€宍#` `鏂囦欢/瀵硅薄` `鎿嶄綔` `璇存槑`銆嶈〃鏍硷紝鎻愬彇鍒?2 鍙嶅紩鍙峰寘瑁圭殑璺緞锛屽幓琛屽彿鍚庣紑銆佽繃婊?`install*/`/鍒嗘敮鍚?`LOG.md`/`AGENTS.md` 绛夐潪婧愮爜鏉＄洰锛屾寜椤跺眰鐩綍鍒嗙粍銆佺粺璁℃瘡涓枃浠惰鏀硅繃鍑犳锛堝悓鏂囦欢澶氭鏀瑰姩鏍囨敞涓恒€岄珮鏁忔劅銆嶏級锛岃緭鍑哄埌 `docs/downstream-changes.md`銆?
**鍚堝叆鐩爣**: `branch`锛堜笉鍦?staging 鏀掓壒锛岀洿鎺ヤ粠 branch 鎷夊嚭锛屼粎鏀瑰伐鍏蜂笌鏂囨。锛屾棤 C++ 浠ｇ爜鏀瑰姩锛屼笉闇€鍗曟祴锛夈€?
| # | 鏂囦欢/瀵硅薄 | 鎿嶄綔 | 璇存槑 |
|---|----------|------|------|
| 1 | `feat/downstream-changes` | 鏂板缓鍒嗘敮 | 浠?`branch` 鎷夊嚭锛坄git switch -c feat/downstream-changes branch`锛?|
| 2 | `LOG.md` | 淇敼 | 鏈妭锛堝惎鍔ㄨ褰曪級 |

### feat/downstream-changes 瀹炴柦瀹屾垚

| # | 鏂囦欢 | 鎿嶄綔 | 璇存槑 |
|---|------|------|------|
| 1 | `tools/gen-downstream-changes.py` | 鏂板缓 | 瑙ｆ瀽 LOG.md 4 鍒楄〃鏍硷紙`# / 鏂囦欢(瀵硅薄) / 鎿嶄綔 / 璇存槑`锛夛紝鎻愬彇鍒?2 鍙嶅紩鍙疯矾寰勶細鍘昏鍙峰悗缂€锛坄path:123-456` 鈫?`path`锛夈€乥race-aware 閫楀彿鍒囧垎锛堜繚鐣?`{zh-cn,en-us}` 鍐呴€楀彿锛夈€乻hell brace 灞曞紑锛坄{a,b,c}.xaml` 鈫?澶氫釜鏂囦欢锛夈€佽繃婊?`install*/`/`build/`/`debug/`/`config/`/`cache/`/`data/`/`reports/` 绛夐潪婧愮爜浜х墿锛涙寜椤跺眰鐩綍鍒嗙粍锛岃鏀?鈮?3 娆℃爣 `[HOT]`锛屽惁鍒?`[TGT]`锛涜緭鍑?markdown 琛ㄦ牸銆傛敮鎸?`--log` / `--out` / `--dry-run` 鍙傛暟 |
| 2 | `docs/downstream-changes.md` | 鏂板缓 | 棣栨杩愯浜х墿锛?6 涓敮涓€婧愭枃浠讹紝瑕嗙洊 220 琛?LOG.md 琛ㄦ牸銆備粨搴撴牴锛坄.gitignore`/`VERSION`锛? `.github/` + `docs/` + `resource/`锛坄tasks.json` [HOT]锛? `src/` 26 鏂囦欢锛坄TaskQueueViewModel.cs` [HOT] 23 娆★級+ `tools/` 5 鏂囦欢 |
| 3 | `AGENTS.md 搂3.2` | 淇敼 | 鍚姩鏂?feat 娴佺▼鍔犳楠?1.5銆屾煡 `docs/downstream-changes.md`銆嶄笌姝ラ 8銆屽悎鍏ュ悗閲嶈窇鑴氭湰鍒锋柊娓呭崟銆? 鏈熬鏂板璇存槑娈碉紙鑴氭湰鏁版嵁鏉ユ簮 / 缁存姢鏂瑰紡锛?|
| 4 | `LOG.md` | 淇敼 | 鏈妭锛堝疄鏂藉畬鎴愯褰曪級 |

**杩愯楠岃瘉**锛歚py tools/gen-downstream-changes.py --dry-run` 杈撳嚭 36 鏂囦欢娓呭崟锛堝惈 12 涓?`[HOT]` 楂樻晱鎰燂級锛沗py tools/gen-downstream-changes.py` 鐢熸垚 `docs/downstream-changes.md` 332 琛屻€?
**鍏抽敭璁捐**锛?- 鍏煎 LOG.md 瀹為檯瀛樺湪鐨勫绉嶈〃鏍兼€浉锛歚path:123` 鍗曡鍙?/ `path:123-456` 琛屽彿鑼冨洿 / `path:68-83, 173, 186` 鍗曟枃浠跺浣嶇疆锛堢敤 brace-aware 閫楀彿鍒囧垎锛? `{zh-cn,en-us,ja-jp,ko-kr,zh-tw}.xaml` shell brace expansion 灞曞紑涓?5 涓嫭绔嬭拷韪枃浠?- 鎿嶄綔鍒楃敤 `.+?` 鑰岄潪 `\S+`锛屽吋瀹?`cherry-pick from xxx`銆乣+ExpediteMinLevel* 6 涓?key`銆乣git rm` 绛夎嚜鐢辨枃鏈?- 鏍圭骇 dotfile锛坄.gitignore`锛変笌瑁告枃浠讹紙`VERSION`锛夊綊鍒般€屼粨搴撴牴銆嶅垎缁勶紝涓嶅姞 `/` 鍚庣紑
- emoji 鏀?ASCII 鏍囪锛坄[OK]`/`[TGT]`/`[HOT]`锛夛紝閬垮厤 PowerShell GBK 鎺у埗鍙扮紪鐮侀棶棰橈紙GBK 鎶?`UnicodeEncodeError` on `馃敶`锛?
**棰勬湡鏁堟灉**锛?1. 寮€鏂?feat 鏃剁涓€姝ユ煡 `docs/downstream-changes.md`锛岀湅鍒扮洰鏍囨枃浠舵槸 `[HOT]` 澶氭敼鍔ㄧ殑锛堝 `TaskQueueViewModel.cs` 23 娆°€乣AutoRecruitTask.cpp` 10 娆★級锛屾敼鍓嶅厛璇诲搴?LOG.md 娈佃惤纭鏄惁鍐茬獊
2. 鍚堝叆 feat 鍚庤窇涓€娆¤剼鏈埛鏂版竻鍗曪紝鏈€鏂版敼鍔ㄨ嚜鍔ㄥ綊绫?3. 鏃犺繍琛屼緷璧栵細绾?Python 3 stdlib锛屾棤闇€ numpy/Cairo 绛夛紱涓庝笂娓歌剼鏈?`tools/TaskSorter/` 鍚屾瀯锛坄py` 鍚姩锛?
**鍚庣画**锛氬緟鎵嬪姩楠岃瘉椤圭洰浠呬负銆屾枃妗ｅ彲璇绘€с€嶁€斺€旇嫢 `[OK]`/`[TGT]`/`[HOT]` ASCII 鏍囪瑙夊緱涓嶅鐩磋锛屼笅涓?feat 鍙敼鐢ㄧ函鏂囧瓧锛坄OK`/`MOD`/`HOT`锛夈€傛湰娆′繚鎸佺畝娲佷笉寮曞叆 emoji銆?
## 2026-07-29

### feat/diagnostic-export 启动

在 IssueReport 页面新增「导出诊断包」功能，支持按日期范围选择性导出日志 + `diagnostic.json` 系统信息 + 可选配置文件/缓存/自定义资源。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `feat/diagnostic-export` | 新建分支 | 从 `branch` 拉出 |
| 2 | `LOG.md` | 修改 | 本节（启动记录） |

### feat/diagnostic-export 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/Models/DiagnosticInfo.cs` | 新建 | 系统信息数据模型 + `Collect()` 静态收集方法：OS/.NET 版本/架构、GPU、管理员、Wine、MAA 版本（UI/Core/Resource） |
| 2 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:42-70` | 修改 | 新增诊断导出属性：`DiagnosticDateRange`(默认 7 天)、`IncludeConfig`/`IncludeCache`/`IncludeCustomResource` 三个 CheckBox、`DateRangeOption` record 与 `DateRangeOptions` 懒加载列表 |
| 3 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:292-399` | 修改 | 新增 `ExportDiagnosticPackage()` 方法：收集系统信息 → diagnostic.json → 逐日志文件按日期范围过滤行 → 可选目录复制 → zip 打包 → growl + 打开 reports 目录 |
| 4 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:401-443` | 修改 | 新增 `CopyFilteredLog()` 辅助方法：正则 `^\[\d{4}-\d{2}-\d{2}` 逐行解析日志时间戳，仅保留日期范围内行；非时间戳行（异常栈）自动保留 |
| 5 | `src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml:97-165` | 修改 | IssueReport 页面新增诊断导出区域：日期范围 ComboBox + 3 个 CheckBox + 导出按钮 |
| 6 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:1457-1473` | 修改 | +13 个中文 localization key（`ExportDiagnosticPackage*` / `DiagnosticDateRange` / `DiagnosticInclude*` / `DiagnosticLast*`） |
| 7 | `src/MaaWpfGui/Res/Localizations/en-us.xaml:1456-1472` | 修改 | +13 个英文 localization key |
| 8 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml:1457-1473` | 修改 | +13 个繁中 localization key |
| 9 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml:1457-1473` | 修改 | +13 个日文 localization key |
| 10 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml:1458-1474` | 修改 | +13 个韩文 localization key |
| 11 | `LOG.md` | 修改 | 本节（实施完成记录） |

**编译/部署结果**: `dotnet build -c Release` 0 error, 50 warning（全为 StyleCop 规则与 `#nullable` 上下文内 `string?` 注释，与 AGENTS.md §5 一致，不阻断）；C++ 端未改动，无需 cmake。

**关键设计**:
- 日志过滤使用 `StreamReader.ReadLine()` 逐行流式读取，避免 64MB+ 大日志（asst.log）加载到内存
- 非时间戳行（C++ 异常栈回溯、Serilog 多行异常）自动保留，附在最后一条有时间戳的日志后
- `DiagnosticInfo.Collect()` 通过 `RuntimeInformation` / `WineRuntimeInformation` / `GpuOption.GetCurrent()` 收集系统信息，不引入新外部依赖
- `DateRangeOptions` 使用懒加载（`Lazy<T>` 模式），避免 `LocalizationHelper` 在静态初始化时未就绪
- 重用已有 `CopyDirectoryIfExists()` 私有方法复制可选目录
- 日志始终包含 crash.log 和 dumps/ 目录（即使超出日期范围，因通常很小且对排错关键）

### feat/diagnostic-export 合入 staging

按 AGENTS.md §2.4 流程合并到 `staging`，以 `--no-ff` 合并。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `staging` | `--no-ff` 合并 | `feat/diagnostic-export` 1 commit 合入 |
| 2 | `LOG.md` | 修改 | 本节（合入记录 + 冲突解决） |

## 2026-07-29

### fix/diagnostic-export-path 启动

按用户反馈，诊断包导出目前固定输出到 `reports/diagnostic_*.zip`，无法指定保存位置。加 `SaveFileDialog` 让用户在导出前选路径 + 文件名，取消则放弃导出。从 `staging` 拉出（按 §3.3，修 feat/diagnostic-export 自身的 fix，挂 staging）。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `fix/diagnostic-export-path` | 新建分支 | 从 `staging` 拉出 |
| 2 | `LOG.md` | 修改 | 本节（启动记录） |

### fix/diagnostic-export-path 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:14-34` | 修改 | 新增 `using Microsoft.Win32;`（按字母顺序排在 `MaaWpfGui.Models` 之后、`Serilog` 之前，避免 SA1208/SA1210） |
| 2 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:414-431` | 修改 | `ExportDiagnosticPackage()` 顶部加 SaveFileDialog：Title 用本地化键 `ExportDiagnosticPackageSelectLocation`、Filter=`ZIP files (*.zip)\|*.zip`、默认文件名=`{reportName}.zip`、初始目录=`PathsHelper.ReportsDir`、开启 OverwritePrompt + AddExtension + DefaultExt |
| 3 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:418` | 修改 | 把 `tempPath` 创建移到 SaveDialog 之后（取消导出时不创建无用临时目录） |
| 4 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:434` | 修改 | `if (saveDialog.ShowDialog() != true) return;` — 取消安全退出，不弹 growl |
| 5 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:497` | 修改 | `zipPath = saveDialog.FileName` 替代硬编码 `Path.Combine(ReportsDir, ...)` |
| 6 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:1462` | 修改 | +1 key `ExportDiagnosticPackageSelectLocation` = "选择诊断包保存位置" |
| 7 | `src/MaaWpfGui/Res/Localizations/en-us.xaml:1461` | 修改 | +1 key = "Select diagnostic package save location" |
| 8 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml:1462` | 修改 | +1 key = "選擇診斷包儲存位置" |
| 9 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml:1462` | 修改 | +1 key = "診断パッケージの保存場所を選択" |
| 10 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml:1463` | 修改 | +1 key = "진단 패키지 저장 위치 선택" |
| 11 | `LOG.md` | 修改 | 本节（实施完成记录） |

**编译/部署结果**: `dotnet build -c Release` 0 error, 58 warning（前次 50 + 8 重复 StyleCop 报告，无新增 SA1208/SA1210/SA15xx 源自本次改动）；C++ 端未改动，无需 cmake。

**关键设计**:
- SaveFileDialog 的 `Filter` 故意不本地化（`"ZIP files (*.zip)|*.zip"`），避免 `LocalizationHelper.GetString()` 增加 xaml key 数；Windows 文件对话框标准文本
- `tempPath` 创建移到 SaveDialog 之后，避免用户在取消时白创建空目录
- 取消导出静默 `return` — 不弹任何 growl（避免误以为是失败）
- `InitialDirectory = PathsHelper.ReportsDir` 保留向后兼容默认值；用户在对话框里切换到任意目录（包括桌面）即可
- `OpenReportsFolder()` 仍打开 `reports/` — 用户已选目标路径，保留打开默认目录不碍事；可后续按反馈去掉

**后续**:
- 待手动验证：在 IssueReport 页面点「导出诊断包」 → 弹出「选择诊断包保存位置」对话框 → 默认文件名 `diagnostic_MM-dd_HH-mm-ss.zip` → 选桌面保存 → 文件成功落桌面并 growl 提示
- 取消对话框不弹 growl，临时目录（若已创建）会被 `try/finally` 风格未覆盖；目前 `tempPath` 在取消路径根本不创建，安全

### feat/diagnostic-export 与 fix/diagnostic-export-path 撤销 / 合并

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `feat/diagnostic-export` 合入 staging 的 commit | 后续合并 | 保留历史，但功能被合并 |
| 2 | `fix/diagnostic-export-path` 合入 staging 的 commit | 后续合并 | 保留历史，但功能被合并 |

### fix/diagnostic-export-merge 启动

`feat/diagnostic-export` + `fix/diagnostic-export-path` 已合入 staging 但发现两个问题：
1. UI 突兀 — 「导出诊断包」独立区域用 Border 分隔 + 居中按钮 + ComboBox/CheckBox 风格，与上方「生成日志压缩包/打开日志文件夹/清空图片缓存」一排的简洁风格不统一
2. 功能重叠 — 两个按钮都导日志，用户分不清用哪个

按用户决策，把「导出诊断包」能力全部吸收到现有「生成日志压缩包」按钮里（重命名为「生成诊断报告」），UI 整合到上方 StackPanel 内，删除独立的"导出诊断包"区域。从 `staging` 拉出。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `fix/diagnostic-export-merge` | 新建分支 | 从 `staging` 拉出 |
| 2 | `AGENTS.md §4.5` | 修改 | 部署目录职责约束（独立 commit `1008219408`） |
| 3 | `LOG.md` | 修改 | 本节（启动记录） |

### fix/diagnostic-export-merge 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:14-33` | 修改 | 删除 `using System.Text.RegularExpressions;`（不再需要行级日志过滤） |
| 2 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:48` | 修改 | 注释 `// ===== Diagnostic Export Properties =====` → `// ===== Diagnostic Report Properties (used by GenerateSupportPayload) =====` |
| 3 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:93` | 修改 | `_includeCustomResource` 默认值 `false` → `true`（保留原 GenerateSupportPayload 行为：始终包含自定义资源） |
| 4 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:217-352` | 修改 | `GenerateSupportPayload()` 重写：顶部加 SaveFileDialog 选保存位置（默认目录 `PathsHelper.ReportsDir`，默认文件名 `report_{MM-dd_HH-mm-ss}.zip`），生成 `diagnostic.json` 系统信息，原 config/resource/cache 复制改为按 `_includeConfig`/`_includeCache`/`_includeCustomResource` 条件复制，原 hardcoded 3 天 `threeDaysAgo` 改为 `_diagnosticDateRange`，完整 zip 输出路径改为 `saveDialog.FileName`，分卷输出目录改为 `userChosenDir/{name}_parts/` 紧贴用户选定位置 |
| 5 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:265` | 修改 | part01 增加 `Directory.EnumerateFiles(tempPath, "*", SearchOption.TopDirectoryOnly)` 包含 `diagnostic.json` 在分卷中 |
| 6 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs` | 删除 | `ExportDiagnosticPackage()` 方法（约 100 行） |
| 7 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs` | 删除 | `CopyFilteredLog()` 行级日志过滤方法（约 50 行） |
| 8 | `src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml:72-118` | 修改 | 右侧 StackPanel 内插入新 UI：日期范围 Grid（ComboBox + TextBlock）+ 3 CheckBox（配置文件/缓存/自定义资源），按钮文案 `GenerateSupportPayload` → `GenerateDiagnosticReport` |
| 9 | `src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml:97-159` | 删除 | 独立"导出诊断包" StackPanel + Border（约 60 行） |
| 10 | `src/MaaWpfGui/Res/Localizations/{zh-cn,en-us,zh-tw,ja-jp,ko-kr}.xaml` | 修改 | 删除 7 个失效 key：`ExportDiagnosticPackage` / `ExportDiagnosticPackageButton` / `ExportDiagnosticPackageSuccessful` / `ExportDiagnosticPackageException` / `ExportDiagnosticPackageSelectLocation` / `DiagnosticIncludeGuiLog` / `DiagnosticIncludeCoreLog` |
| 11 | `src/MaaWpfGui/Res/Localizations/{zh-cn,en-us,zh-tw,ja-jp,ko-kr}.xaml` | 修改 | 新增 2 个 key：`GenerateDiagnosticReport`（按钮文案）/ `GenerateDiagnosticReportSelectLocation`（保存对话框标题） |
| 12 | `src/MaaWpfGui/Res/Localizations/{zh-cn,en-us,zh-tw,ja-jp,ko-kr}.xaml` | 修改 | 节注释 `<!-- DiagnosticExport -->` → `<!-- DiagnosticReport -->` |
| 13 | `src/MaaWpfGui/Models/DiagnosticInfo.cs` | 保留 | 仍被合并后的 `GenerateSupportPayload()` 调用（生成 `diagnostic.json`） |
| 14 | `LOG.md` | 修改 | 本节（实施完成记录） |

**编译/部署结果**: `dotnet build -c Release` 0 error, 48 warning（均为 StyleCop 规则 + `#nullable` 注释，与 AGENTS.md §5 一致不阻断；新增 2 处 SA1512「注释后空行」与已有模式相同）；C++ 端未改动，无需 cmake。

**关键设计**:
- **合并而非新增**：原 `GenerateSupportPayload()` 已有 `CopyDirectoryIfExists` + 20MB 分卷 + 多 zip 拼接的能力，新功能只需在顶部加 `SaveFileDialog` + `diagnostic.json` + 把 hardcoded 3 天换为 `_diagnosticDateRange` + 把 always-on 的 config/cache/custom 改为条件复制。`CopyFilteredLog` 的行级过滤删除（被文件级 `LastWriteTime` 过滤替代，由 part02+ 阶段实现）
- **保留向后兼容**：`_includeCustomResource` 默认 `true`（保留原行为），`_includeConfig` 默认 `true`（保留），`_includeCache` 默认 `false`（新增能力，默认关避免大 zip）
- **输出路径对齐**：分卷目录 `userChosenDir/{name}_parts/` 与用户选定的 zip 同目录，避免「zip 在桌面，parts 在 install/」混乱
- **按钮文案「生成诊断报告」**：兼顾「报告」语义（log）+「诊断」语义（system info），一眼看清用途
- **GUI/核心日志始终包含**：`DiagnosticIncludeGuiLog`/`DiagnosticIncludeCoreLog` 这两个 key 删除（原 UI 显示成 disabled CheckBox 是冗余的——它本就是 always-on）。`gui.log`/`asst.log` 仍由 `CopyDirectoryIfExists(PathsHelper.DebugDir, ...)` 全量复制
- **AGENTS.md §4.5** 同步上线，避免后续 staging 改动误写 `install/`

**后续**:
- 待手动验证：IssueReport 页面「生成诊断报告」按钮 → 弹保存对话框 → 选桌面 → 文件落桌面并打开 reports 文件夹
- 切换日期范围（1/3/7/14/30 天）观察 `diagnostic.json` 中 `fromDate`/`toDate` 与分卷内 debug 子文件是否同步变化
- 取消勾选「包含缓存」观察 zip 大小变化

## 2026-07-30

### feat/auto-recruit-3star-to-4star 启动

3 星 Tag 组合里若能开 4★ 干员（如「费用回复 + 先锋干员」出桃金娘/讯使/凛冬/贾维），自动升级为 4★ 处理路径（3:50 计时 + 联动 `ExpediteMinLevel` 加急）。当前 `final_combination.min_level==3 && max_level>=4` 时仍按 3★ 走，9:00 倒计时且不加急，浪费 4★ 机会。新增 UI 选项 `AutoUpgrade3StarWith4StarToLevel4`（默认 `true`），与现有 `UseLevel3PreferTags` / `RefreshLevel3` / `ChooseLevel3` / `ExpediteMinLevel` 全兼容，沿用 4★ 路径的 `SelectExtraTagsMode` 选 tag。
| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `feat/auto-recruit-3star-to-4star` | 新建分支 | 从 `branch` 拉出，本地工作分支（HEAD `d1770eb310`） |
| 2 | `LOG.md` | 修改 | 本节 |

### feat/auto-recruit-3star-to-4star 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h:29, 184` | 新增 setter `set_auto_upgrade_3star_with_4star` + 成员 `m_auto_upgrade_3star_with_4star = true` | 升级开关默认开启 |
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:153-158` | 新增 setter 实现 | 链式调用 |
| 3 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:540-554` | 新增「4★ 潜力检测」循环 | `min_level==3 && max_level>=4` 时把 min_level/avg_level 重算到 ≥4★ 子集；与 519-535 行「3★ 视角修正」对称 |
| 4 | `src/MaaCore/Task/Interface/RecruitTask.cpp:55, 88` | 解析 `auto_upgrade_3star_with_4star` 参数（默认 `true`）+ 链式调用 `.set_auto_upgrade_3star_with_4star(...)` | 接口层透传 |
| 5 | `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs:43-49` | 新增 `AutoUpgrade3StarWith4Star` 字段（默认 `true`） | 配置模型 |
| 6 | `src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs:70-76, 164` | 新增 DTO 字段 + `Serialize()` 写入 `auto_upgrade_3star_with_4star` | JSON 序列化 |
| 7 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs:83-91, 333` | 新增 VM 属性 `AutoUpgrade3StarWith4Star` + `SerializeTask()` 写入 | 双向绑定 |
| 8 | `src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml:142-150` | 新增 `StackPanel` 包裹 `CheckBox` + `TooltipBlock`，位于「3星 Tag 时的 Tag 倾向」区域下方 | UI 控件 |
| 9 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:1353-1354` | 新增 `AutoUpgrade3StarWith4Star` / `AutoUpgrade3StarWith4StarTip` 字符串 | 简体中文 |
| 10 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml:1354-1355` | 同上（繁体） | 繁体中文 |
| 11 | `src/MaaWpfGui/Res/Localizations/en-us.xaml:1352-1353` | 同上（英文） | English |
| 12 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml:1353-1354` | 同上（日文） | 日本語 |
| 13 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml:1354-1355` | 同上（韩文） | 한국어 |
| 14 | `docs/zh-cn/protocol/integration.md:266-269` | 新增 `auto_upgrade_3star_with_4star` 字段说明段 | 协议文档 |
| 15 | `docs/zh-tw/protocol/integration.md:266-269` | 同上（繁体） | 同上 |
| 16 | `docs/en-us/protocol/integration.md:266-269` | 同上（英文） | 同上 |
| 17 | `docs/ja-jp/protocol/integration.md:266-269` | 同上（日文） | 同上 |
| 18 | `docs/ko-kr/protocol/integration.md:255-258` | 同上（韩文） | 同上 |
| 19 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:354-359` | pre-commit clang-format 自动重排加急 Log.info 多行 | 风格对齐 |
| 20 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:72-73` | pre-commit clang-format 合并 if 链多行 | 风格对齐（历史欠债，关联 fix/account-official-recognize） |
| 21 | `LOG.md` | 修改 | 本节 |

**编译结果**: `cmake --build build --target MaaCore -j 4` 成功，仅遗留标准 `LNK4098` 默认库警告（与上游一致）。`MaaCore.dll` 时间戳更新。

**pre-commit 检查**: clang-format / markdownlint / prettier (Config Files + Documentation) / ruff-format / oxipng 全部 Passed。

**部署验证**: 本节期间在 feat 分支，按 AGENTS.md §4.5 仅执行单目标编译，**不**部署到 `install/` 或 `install-staging/`。待合并到 `staging` 后再由 `tools/local-install-staging.bat` 部署到 `install-staging/` 实测。

**预期效果**:
1. slot tags = `[费用回复, 先锋干员]` 时，`m_auto_upgrade_3star_with_4star=true` 默认开启 → 触发升级 → `final_combination.min_level=4` → 走 4★ 路径：`recruitment_time["4"]` 计时（默认 540/3:50）+ `get_select_tags` 4★ 分支；若 `ExpediteMinLevel≥4` 则 `recruit_now()` 加急
2. slot tags = `[输出, 治疗]`（无 4★ 组合）→ 不触发升级 → 走原 3★ 路径
3. slot 含 `高级资深干员` / `资深干员` → `has_special_tag=true` 优先路径不受影响
4. slot 含 `支援机械` → `has_skip_tag=true` 保留跳过路径不受影响
5. 用户 `AutoUpgrade3StarWith4Star=false` → 行为完全等价于改动前（回归基线）
6. 升级后 `UseLevel3PreferTags=true` 时，`Level3PreferTags` 字段对 3★ 路径仍生效；升级路径走 4★ 的 `SelectExtraTagsMode`，与「4★ 干员选择」体验一致

**待手动验证（需模拟器环境）**:
1. 准备 4★ slot（如「费用回复+先锋干员」）→ 验证走 4★ 路径，计时 3:50
2. 准备 5★ slot（含「资深干员」）→ 验证 special_tag 路径不受影响
3. 准备纯 3★ slot → 验证原 3★ 路径不受影响
4. `AutoUpgrade3StarWith4Star=false` → 验证所有场景回归
5. `UseLevel3PreferTags=true` + 含 4★ slot → 验证 `Level3PreferTags` 对 3★ 仍生效，4★ 路径用 `SelectExtraTagsMode`
6. 加急门槛联动：`ExpediteMode=4` + 升级 slot → 验证 `recruit_now()` 触发立即招

### feat/auto-recruit-3star-to-4star 合入 staging

按 AGENTS.md §3.2 步骤 6，从 `feat/auto-recruit-3star-to-4star` `--no-ff` 合并到 `staging`（首次跨分支合并 staging 领先 branch 部分 commits，需 `--no-ff` 保留合并历史）。
| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `staging` | `--no-ff` 合并 | 接收 `feat/auto-recruit-3star-to-4star` 6 个新 commit（`980603e11b` 启动 + `a41ce91369` C++ + `a7fbff0244` 接口 + `bc7898d483` WPF + `166ad9b5ae` UI/i18n + `3169ad1d64` docs + `bb3e2799eb` clang-format） |
| 2 | `feat/auto-recruit-3star-to-4star` | `git branch -d`（暂缓） | 待 staging 实测通过、晋升 branch 后再处理；保留本地指针便于回溯 |
| 3 | `AGENTS.md §6` | 修改 | 加进行中分支条目 |
| 4 | `LOG.md` | 修改 | 本节 |
| 5 | `docs/downstream-changes.md` | `py tools/gen-downstream-changes.py` | 自动刷新清单（36 → 45 文件，[HOT] 阈值更新） |

### feat/auto-recruit-3star-to-4star 部署到 install-staging

按 AGENTS.md §4.5，本节期间在 `staging` 分支，使用 `tools/local-install-staging.bat` 部署到 `install-staging/`。
| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `install-staging/MaaCore.dll` | 部署 | 2026/7/30 12:41, 4193792 字节（RelWithDebInfo 自 `build/bin/RelWithDebInfo/MaaCore.dll`），含 4★ 潜力检测改动 |
| 2 | `install-staging/MAA.exe` | 部署 | 2026/7/30 12:44, 339456 字节 |
| 3 | `install-staging/MAA.dll` | 部署 | 2026/7/30 12:44, 3742208 字节（含 `AutoUpgrade3StarWith4Star` 字段 + 5 语新 string key） |
| 4 | `LOG.md` | 修改 | 本节 |

**部署过程**:
1. `cmake --preset windows-publish-x64` （首次配置）
2. `cmake --build build --target MaaCore -j 4` （单目标编译，绕开 WPF MSBuild 评估的 VS 2026 SDK 路径 bug）
3. `tools/local-install-staging.bat` 触发 `cmake --install build --config RelWithDebInfo --prefix install-staging` + `dotnet restore` + `dotnet publish`
4. dotnet publish 阶段命中 AGENTS.md §4.1 已知 bug（VS 2026 SDK 路径 `E:\visual stduio community 2026\VIUAL\MSBuild\Sdks\Microsoft.NET.Sdk\Sdk` 拼写错误），绕用 `C:\Program Files\dotnet\dotnet.exe` 独立 SDK 手动 publish
5. `cmake --install build --config RelWithDebInfo --prefix install-staging` 二次执行确保 resource 同步
6. `install-staging/MaaCore.dll` 与 `MAA.dll` 时间戳更新至 2026/7/30，含本次 feat 全部改动

**编译验证**:
- C++: `cmake --build build --target MaaCore -j 4` PASS, 0 error, 仅 LNK4098 警告（与上游一致）
- WPF: `dotnet publish -c Release -r win-x64 -o install-staging` PASS, 0 error, 含若干 StyleCop SA1402/SA1512/SA1518 警告（历史欠债，与本次 feat 无关）
- pre-commit hooks: clang-format / markdownlint / prettier (Config Files + Documentation) / ruff-format / oxipng 全部 Passed

**待手动验证（需模拟器环境）**:
1. 准备 4★ slot（如「费用回复+先锋干员」）→ 验证走 4★ 路径，计时 3:50
2. 准备 5★ slot（含「资深干员」）→ 验证 special_tag 路径不受影响
3. 准备纯 3★ slot → 验证原 3★ 路径不受影响
4. `AutoUpgrade3StarWith4Star=false` → 验证所有场景回归
5. `UseLevel3PreferTags=true` + 含 4★ slot → 验证 `Level3PreferTags` 对 3★ 仍生效，4★ 路径用 `SelectExtraTagsMode`
6. 加急门槛联动：`ExpediteMode=4` + 升级 slot → 验证 `recruit_now()` 触发立即招
7. 冲突解决回归：本次 merge 时与 staging 的 `expedite_min_level` 字段说明、`fix/expedite-threshold recruit_now 顺序修复`（加急分支位置在 confirm 之后）冲突已解，需验证升级后加急路径仍正常
