# 🥊 Punch King Clicker

완전한 기능을 갖춘 Unity 6 모바일 클릭커 게임

[![Unity Version](https://img.shields.io/badge/Unity-6.0+-blue.svg)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-iOS%20%7C%20Android-green.svg)](#)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](#)

## 📱 게임 소개

펀치킹은 샌드백을 터치하여 파괴하고, 골드를 모아 업그레이드하며 무한대로 성장하는 클릭커 게임입니다.

### 주요 기능

- ✨ **무한 성장 시스템**: BigNumber 클래스로 무한대의 숫자 처리
- 🎯 **다양한 업그레이드**: 펀치력, 자동클릭, 크리티컬 등
- 🔥 **4가지 스킬**: 원투펀치, 박치기, 콤보러쉬, 골드러쉬
- 👊 **보스 시스템**: 10스테이지마다 등장하는 강력한 보스
- 💎 **프레스티지**: 100스테이지 이상에서 영구 보너스 획득
- 👨‍🏫 **코치 시스템**: 5명의 코치가 고유 보너스 제공
- 💾 **자동 저장/로드**: 바이너리 직렬화로 안전한 데이터 저장
- 💰 **오프라인 수익**: 게임을 종료해도 최대 8시간 동안 골드 획득
- 📊 **실시간 통계**: DPS, 골드/초 표시

## 🏗️ 프로젝트 구조

```
PunchKingClicker/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # 핵심 시스템
│   │   │   ├── GameManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   └── MobileOptimization.cs
│   │   ├── Data/              # 데이터 유틸리티
│   │   │   └── BigNumber.cs
│   │   ├── Player/            # 플레이어 관련
│   │   ├── Combat/            # 전투 시스템
│   │   ├── Enemies/           # 적 AI
│   │   │   ├── Sandbag.cs
│   │   │   └── Boss.cs
│   │   ├── Upgrades/          # 업그레이드 시스템
│   │   │   ├── UpgradeData.cs (ScriptableObject)
│   │   │   ├── UpgradeManager.cs
│   │   │   └── CoachSystem.cs
│   │   ├── Skills/            # 스킬 시스템
│   │   │   └── SkillManager.cs
│   │   ├── Prestige/          # 프레스티지
│   │   │   └── PrestigeManager.cs
│   │   ├── UI/                # UI 시스템
│   │   │   ├── UIManager.cs
│   │   │   └── DamagePopup.cs
│   │   ├── Effects/           # 비주얼 이펙트
│   │   │   └── CameraShake.cs
│   │   └── Currency/          # 화폐 관리
│   ├── Prefabs/
│   ├── Materials/
│   ├── Sprites/
│   ├── Animations/
│   ├── Audio/
│   └── Resources/
└── README.md
```

## 🚀 시작하기

### 필수 요구사항

- **Unity 6.0 이상**
- **TextMeshPro** (Unity 패키지)
- **Universal Render Pipeline (URP)** (선택사항, 권장)

### 설치 방법

1. **Unity Hub에서 프로젝트 열기**
   ```
   Unity Hub > Projects > Add > 이 폴더 선택
   ```

2. **TextMeshPro 임포트**
   - Window > TextMeshPro > Import TMP Essential Resources

3. **씬 설정**
   - `Assets/Scenes` 폴더에 Main Scene 생성
   - 빈 GameObject 생성 후 다음 컴포넌트 추가:
     - GameManager
     - SaveManager
     - UpgradeManager
     - SkillManager
     - PrestigeManager
     - CoachSystem
     - UIManager
     - AudioManager
     - MobileOptimization
     - CameraShake

4. **UI 설정**
   - Canvas 생성 (Canvas Scaler: Scale With Screen Size)
   - UIManager 스크립트의 public 필드에 UI 요소 연결
   - Safe Area Panel 설정 (노치 대응)

5. **프리팹 생성**
   - Sandbag Prefab: `Assets/Prefabs/Sandbag.prefab`
   - Boss Prefab: `Assets/Prefabs/Boss.prefab`
   - Damage Popup: `Assets/Prefabs/DamagePopup.prefab`

6. **ScriptableObject 생성**
   - Assets > Create > PunchKing > Upgrade Data
   - 업그레이드 항목들 생성 (펀치력, 자동클릭 등)
   - UpgradeManager의 `allUpgrades` 리스트에 추가

## 🎮 핵심 시스템 설명

### 1. BigNumber 시스템

큰 숫자를 처리하기 위한 과학적 표기법 구현:

```csharp
BigNumber gold = new BigNumber(1000000);
gold.ToKoreanString();  // "100만"

BigNumber damage = new BigNumber(5.5e15);
damage.ToKoreanString();  // "5.5경"
```

**지원 단위**: 만, 억, 조, 경, 해, 자, 양, 구, 간, 정, 재, 극

### 2. 업그레이드 시스템

ScriptableObject 기반으로 확장 가능:

```csharp
// 에디터에서 생성:
// Assets > Create > PunchKing > Upgrade Data

UpgradeData punchPowerUpgrade = ...;
punchPowerUpgrade.type = UpgradeType.PunchPower;
punchPowerUpgrade.baseCost = 10;
punchPowerUpgrade.costMultiplier = 1.15f;  // 15% 증가
```

### 3. 스킬 시스템

4가지 스킬:

| 스킬 | 효과 | 쿨다운 |
|------|------|--------|
| 원투펀치 | 즉시 2회 타격 | 5초 |
| 박치기 | 5배 데미지 단일 타격 | 10초 |
| 콤보러쉬 | 10초간 데미지 2배 | 30초 |
| 골드러쉬 | 10초간 골드 3배 | 60초 |

### 4. 프레스티지 시스템

- **조건**: 스테이지 100 이상
- **보상**: `floor(currentStage / 10)` 프레스티지 화폐
- **보너스**: 모든 데미지 × (1 + 프레스티지화폐 × 0.1)
- **유지**: 코치, 스킨, 프리미엄 구매 항목

### 5. 코치 시스템

5명의 코치가 고유 보너스 제공:

| 코치 | 보너스 | 잠금 해제 비용 |
|------|--------|----------------|
| 이소용 | 데미지 +50% | 1,000 골드 |
| 김트레이너 | 골드 +30% | 5,000 골드 |
| 박챔피언 | 크리티컬 +15% | 10,000 골드 |
| 최마스터 | 모든 보너스 +20% | 50,000 골드 |
| 전설의 권왕 | 모든 보너스 +100% | 1,000,000 골드 |

## 📱 모바일 빌드 가이드

### iOS 빌드

1. **Build Settings 설정**
   - File > Build Settings
   - Platform: iOS
   - Switch Platform

2. **Player Settings**
   ```
   Company Name: [회사명]
   Product Name: Punch King
   Bundle Identifier: com.yourstudio.punchking
   Version: 1.0.0
   Minimum iOS Version: 12.0
   Target SDK: Device SDK
   Architecture: ARM64
   ```

3. **Xcode 프로젝트 생성**
   - Build 버튼 클릭
   - Export to Xcode

4. **App Store 제출**
   - Xcode에서 Archive
   - App Store Connect에 업로드

### Android 빌드

1. **Build Settings 설정**
   - Platform: Android
   - Texture Compression: ASTC

2. **Player Settings**
   ```
   Package Name: com.yourstudio.punchking
   Version: 1.0.0
   Minimum API Level: API 23 (Android 6.0)
   Target API Level: Automatic (highest installed)
   Scripting Backend: IL2CPP
   Target Architectures: ARM64 (ARMv7 선택 해제)
   ```

3. **Keystore 생성**
   - Publishing Settings
   - Create a new keystore
   - 안전하게 보관!

4. **AAB 빌드**
   - Build App Bundle (Google Play)
   - Google Play Console에 업로드

## 🎨 커스터마이징 가이드

### 새 업그레이드 추가

1. **ScriptableObject 생성**
   ```
   Assets > Create > PunchKing > Upgrade Data
   ```

2. **데이터 입력**
   - Upgrade Name: "새 업그레이드"
   - Type: PunchPower / AutoClicker / CriticalChance 등
   - Base Cost: 초기 비용
   - Cost Multiplier: 레벨당 비용 증가율

3. **UpgradeManager에 등록**
   - UpgradeManager 오브젝트 선택
   - All Upgrades 리스트에 추가

### 새 코치 추가

`CoachSystem.cs`의 `InitializeCoaches()` 메서드 수정:

```csharp
allCoaches.Add(new Coach
{
    id = 5,
    coachName = "새 코치",
    description = "특별한 보너스",
    unlockCost = new BigNumber(100000),
    damageBonus = 0.3f,
    goldBonus = 0.2f,
    critBonus = 0.1f,
    isUnlocked = false
});
```

## ⚙️ Unity 6 최적화 기능

### GPU Resident Drawer

```csharp
#if UNITY_6_0_OR_NEWER
    QualitySettings.enableGPUResidentDrawer = true;
    QualitySettings.smallMeshScreenPercentage = 0.15f;
#endif
```

### 기기별 렌더 스케일 조정

- iPhone 12/13/14: 0.85
- iPhone 11/X: 0.9
- iPad: 0.8
- Android (RAM 기반):
  - < 2GB: 0.65
  - 2~4GB: 0.75
  - 4~6GB: 0.85
  - 6GB+: 0.95

### 60fps 타겟

```csharp
Application.targetFrameRate = 60;
QualitySettings.vSyncCount = 0;
```

## 🐛 디버그 기능

에디터에서 사용 가능한 디버그 메뉴:

- **GameManager**: Add 1000 Gold, Skip 10 Stages
- **UpgradeManager**: Reset All Upgrades, Max All Upgrades
- **PrestigeManager**: Force Prestige, Add 10 Prestige Currency
- **SkillManager**: Reset All Cooldowns
- **CoachSystem**: Unlock All Coaches

## 📊 성능 목표

- **타겟 FPS**: 60fps
- **메모리 사용량**: < 200MB (iOS), < 300MB (Android)
- **배터리 소모**: 낮음 (배터리 절약 모드 지원)
- **앱 크기**: < 100MB

## 🔧 확장 아이디어

### 구현 가능한 추가 기능

1. **미니게임**
   - 줄넘기 (타이밍 게임)
   - 풍선 피하기 (드래그 게임)
   - 일일 보너스

2. **수익화**
   - Unity IAP 연동
   - Unity Ads (보상형 광고)
   - 프리미엄 패키지

3. **소셜 기능**
   - 리더보드 (Unity Gaming Services)
   - 친구 초대 보상
   - 길드 시스템

4. **콘텐츠 확장**
   - 스킨 시스템
   - 펫 시스템
   - 이벤트 스테이지

## 📝 라이선스

이 프로젝트는 교육 목적으로 제작되었습니다.

## 👨‍💻 개발자

Unity 6 Punch King Clicker
Made with ❤️ using Unity 6

## 🙏 감사의 말

- Unity Technologies - Unity 6 Engine
- TextMeshPro - 고품질 텍스트 렌더링

---

**즐거운 개발 되세요!** 🥊✨
