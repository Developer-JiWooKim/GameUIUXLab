# 주문받아라!!

손님 주문대로 디저트를 쟁반에 담는 세로형 캐주얼 게임. Unity 6 (6000.5.8f1, URP), UGUI + TextMeshPro, Input System.

- **포트폴리오 페이지**: https://developer-jiwookim.github.io/GameUIUXLab/
- **플레이 빌드 (Windows)**: [Releases](https://github.com/Developer-JiWooKim/GameUIUXLab/releases/latest)

> 이 저장소에는 **직접 작성한 C# 스크립트와 설계 문서만** 남겨 두었습니다.
> 씬·프리팹·에셋·프로젝트 설정은 포함하지 않으므로 Unity 프로젝트로 열 수 없습니다.

## 스크립트 구조

원래 프로젝트의 `Assets/MyAssets/PORTFOLIO_Assets/Scripts/`를 루트로 옮겼습니다. 네임스페이스는 원래 경로 기준(`Assets.MyAssets.PORTFOLIO_Assets.Scripts.*`)입니다.

```
Scripts/
├── Core/       게임 진행(GamePlayController), 주문 생성, 디저트·등급 테이블
│   └── Rules/  주문 판정·점수·제한 시간 규칙 (UnityEngine 비의존 순수 C#)
├── Screens/    화면 상태(Title/Play/Pause/Confirm/Result)와 스택 기반 화면 전환, 입력 라우팅, 포커스 표시
└── Views/      HUD, 주문 카드, 진열대, 쟁반, 토스트, 결과 화면, 해상도 레터박스
```

### 먼저 보면 좋은 파일

| 주제 | 파일 |
| :--- | :--- |
| 화면 전환 (상태 스택, 팝업 위 입력 잠금) | [`ScreenFlowController.cs`](Scripts/Screens/ScreenFlowController.cs), [`UIStateBase.cs`](Scripts/Screens/UIStateBase.cs) |
| 선택 오브젝트와 무관한 입력 처리 (Cancel 등) | [`UIInputRouter.cs`](Scripts/Screens/UIInputRouter.cs) |
| 주문 판정 규칙 | [`OrderSession.cs`](Scripts/Core/Rules/OrderSession.cs), [`PickResult.cs`](Scripts/Core/Rules/PickResult.cs) |
| 게임 진행 이벤트와 HUD 구독 | [`GamePlayController.cs`](Scripts/Core/GamePlayController.cs), [`HudController.cs`](Scripts/Views/HudController.cs) |
| 주문 카드·쟁반 아이콘 재사용 | [`DessertIconPool.cs`](Scripts/Views/DessertIconPool.cs), [`OrderCardView.cs`](Scripts/Views/OrderCardView.cs) |
| 해상도 대응 | [`AspectRatioLetterbox.cs`](Scripts/Views/AspectRatioLetterbox.cs) |

## 설계 문서

| 문서 | 내용 |
| :--- | :--- |
| [UIUX_ARCHITECTURE.md](UIUX_ARCHITECTURE.md) | 화면 목록, 화면 전환 흐름, 기준 해상도와 Canvas Scaler, HUD-데이터 연결, 입력, UI Prefab, 피드백 설계 |
| [GUI_GUIDE.md](GUI_GUIDE.md) | 최소 버튼 크기, 글자 크기와 대비, 정보 우선순위, 지원 입력 방식, 원안에서 바꾼 항목 |
| [CHECKLIST.md](CHECKLIST.md) | 기대 / 실제 / 원인 / 수정 / 재확인 형식의 UI 점검 기록 |

문서에 나오는 `Assets/...` 경로는 원래 Unity 프로젝트 기준입니다.

## 사용한 외부 에셋 (저장소 미포함)

- [Sweet Cakes Icon Pack](https://assetstore.unity.com/packages/2d/gui/icons/sweet-cakes-icon-pack-88182) (Unity Asset Store) — Asset Store Standard EULA
- [Free Casual GUI](https://uncogames.itch.io/free-casual-gui) © Unco Games Studio — 무료 사용 가능, 원본 재배포 금지
- [케리스 케듀체 (KERISKEDU_R)](https://copyright.keris.or.kr/wft/fntDwnldView?fntGrpId=GFT202512150000000000002) © KERIS, 제작 투게더그룹 — SIL Open Font License 1.1
- Baloo © Ek Type — SIL Open Font License 1.1 (Free Casual GUI 동봉)

빌드·스크린샷·플레이 영상에 보이는 UI 그래픽·아이콘의 저작권은 위 제작자에게 있습니다.
