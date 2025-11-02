# Punch King Clicker - 프로젝트 셋업 가이드

## 📋 목차
1. [프로젝트 구조](#프로젝트-구조)
2. [ScriptableObject 데이터 생성](#scriptableobject-데이터-생성)
3. [Scene 구성](#scene-구성)
4. [매니저 설정](#매니저-설정)
5. [빌드 설정](#빌드-설정)

---

## 프로젝트 구조

```
PunchKingClicker/
├── Assets/
│   ├── Data/                      # ScriptableObject 데이터 (자동 생성됨)
│   │   ├── Upgrades/             # 업그레이드 데이터 (8개)
│   │   ├── Achievements/         # 업적 데이터 (60개)
│   │   └── Shop/                 # 상점 아이템 (20개)
│   ├── Scenes/
│   │   └── MainGame.unity        # 메인 게임 씬
│   ├── Prefabs/                  # UI 및 게임 오브젝트 Prefab
│   ├── Scripts/
│   │   ├── Core/                 # 핵심 게임 시스템
│   │   ├── UI/                   # UI 스크립트
│   │   ├── Data/                 # 데이터 클래스
│   │   ├── Upgrades/             # 업그레이드 시스템
│   │   ├── Achievements/         # 업적 시스템
│   │   ├── Shop/                 # 상점 시스템
│   │   ├── Leaderboard/          # 리더보드 시스템
│   │   ├── Ads/                  # 광고 시스템
│   │   ├── Visual/               # 비주얼 이펙트
│   │   ├── DailyReward/          # 일일 보상
│   │   ├── MiniGames/            # 미니게임
│   │   ├── Quest/                # 퀘스트 시스템
│   │   └── Editor/               # Editor 도구
│   ├── Sprites/                  # 게임 스프라이트
│   ├── Audio/                    # 사운드/음악
│   └── Fonts/                    # 폰트 (TextMeshPro)
```

---

## ScriptableObject 데이터 생성

### ⚡ 빠른 시작 (1분 완료)

Unity Editor에서:

```
1. 상단 메뉴 > PunchKing > Generate All Data 클릭
2. 완료 다이얼로그 확인
3. Assets/Data 폴더 확인
```

생성되는 데이터:
- ✅ **업그레이드**: 8개
- ✅ **업적**: 60개 (12타입 × 5티어)
- ✅ **상점 아이템**: 20개

---

### 📊 생성되는 데이터 상세

#### 1. 업그레이드 데이터 (8개)

| 파일명 | 이름 | 베이스 비용 | 비용 배율 | 효과 |
|--------|------|------------|----------|------|
| `PunchPower.asset` | 펀치 파워 | 100 | 1.15 | 클릭 데미지 +2 |
| `AutoClicker.asset` | 자동 클릭 | 500 | 1.2 | 초당 클릭 +1 |
| `CriticalChance.asset` | 크리티컬 확률 | 1,000 | 1.25 | 크리티컬 +1% |
| `CriticalDamage.asset` | 크리티컬 데미지 | 2,000 | 1.3 | 크리티컬 배율 +0.5 |
| `GoldMultiplier.asset` | 골드 배율 | 1,500 | 1.25 | 골드 획득 +10% |
| `OfflineEarnings.asset` | 오프라인 수익 | 5,000 | 1.35 | 오프라인 수익 +5% |
| `ClickDamage.asset` | 클릭 데미지 | 800 | 1.18 | 클릭 데미지 +1.5 |
| `GoldPerClick.asset` | 클릭당 골드 | 3,000 | 1.28 | 클릭당 골드 +5 |

#### 2. 업적 데이터 (60개 = 12타입 × 5티어)

**12가지 업적 타입**:
1. `TotalClicks` - 총 클릭 수 (100 → 1,000 → 10,000 → 100,000 → 1,000,000)
2. `TotalGold` - 총 골드 획득 (1만 → 100만 → 1억 → 100억 → 1조)
3. `ReachStage` - 스테이지 도달 (10 → 50 → 100 → 250 → 500)
4. `DefeatBosses` - 보스 처치 (5 → 25 → 50 → 100 → 250)
5. `PrestigeCount` - 프레스티지 횟수 (1 → 5 → 10 → 25 → 50)
6. `UpgradeLevel` - 업그레이드 레벨 (50 → 200 → 500 → 1,000 → 2,500)
7. `UseSkills` - 스킬 사용 (10 → 50 → 200 → 500 → 1,000)
8. `CriticalHits` - 크리티컬 타격 (100 → 500 → 2,000 → 5,000 → 10,000)
9. `UnlockCoaches` - 코치 해금 (1 → 2 → 3 → 4 → 5)
10. `PlayTime` - 플레이 시간 (1h → 5h → 10h → 24h → 48h)
11. `DailyLogins` - 연속 로그인 (3 → 7 → 14 → 30 → 100일)
12. `CompleteQuests` - 퀘스트 완료 (10 → 50 → 100 → 250 → 500)

**각 티어별 보상**:
- 티어 1-2: 골드 보상
- 티어 3-4: 프레스티지 화폐 또는 영구 배율
- 티어 5: 대량 프레스티지 화폐

#### 3. 상점 아이템 데이터 (20개)

**골드 패키지 (5개)**:
- `gold_small` - 100만 골드 ($0.99)
- `gold_medium` - 1,000만 골드 ($2.99)
- `gold_large` - 1억 골드 ($4.99)
- `gold_mega` - 10억 골드 + 2시간 골드 2배 ($9.99)
- `gold_ultra` - 100억 골드 + 6시간 골드 3배 ($19.99) ⭐ Best Value

**프레스티지 화폐 (3개)**:
- `prestige_small` - 100 프레스티지 ($1.99)
- `prestige_medium` - 500 프레스티지 ($4.99)
- `prestige_large` - 1,500 프레스티지 ($9.99) ⭐ Best Value

**특별 패키지 (2개)**:
- `starter_pack` - 스타터 팩 ($4.99, 1회 구매 제한)
- `mega_bundle` - 메가 번들 ($14.99) ⭐ Best Value

**영구 업그레이드 (4개)**:
- `permanent_auto_speed` - 영구 자동클릭 +50% ($9.99, 1회)
- `permanent_offline` - 영구 오프라인 수익 +100% ($9.99, 1회)
- `permanent_crit` - 영구 크리티컬 확률 +5% ($12.99, 1회)
- `remove_ads` - 광고 제거 ($2.99, 1회)

**시한부 부스트 (3개)**:
- `damage_boost_24h` - 24시간 데미지 3배 ($3.99)
- `gold_boost_24h` - 24시간 골드 3배 ($3.99)
- `combo_boost_48h` - 48시간 데미지/골드 4배 ($9.99) ⭐ Best Value

**제한 시간 특가 (2개)**:
- `limited_weekend` - 주말 특가 (70% 할인, 3일 제한)
- `black_friday` - 블랙프라이데이 (80% 할인, 7일 제한, 1회) ⭐ Best Value

---

### 🔧 개별 데이터 생성

필요시 개별 카테고리만 생성 가능:

```
PunchKing > Generate Upgrades Only
PunchKing > Generate Achievements Only
PunchKing > Generate Shop Items Only
```

---

### ✅ 데이터 검증

생성 후 확인사항:

1. **폴더 확인**:
   - `Assets/Data/Upgrades/` - 8개 파일
   - `Assets/Data/Achievements/` - 60개 파일
   - `Assets/Data/Shop/` - 20개 파일

2. **파일 확인**:
   - 각 파일 클릭 → Inspector에서 데이터 확인
   - 모든 필드가 올바르게 채워져 있는지 확인

3. **자동 검증** (옵션):
   ```
   PunchKing > Verify Generated Data
   ```

---

## Scene 구성

### 1. 새 씬 생성

1. `File > New Scene`
2. `MainGame`으로 저장
3. `Scenes/MainGame.unity`

### 2. 게임 매니저 오브젝트 생성

**Hierarchy 구조**:

```
MainGame (Scene)
├── === MANAGERS ===
├── GameManager (빈 오브젝트)
│   ├── GameManager.cs
│   ├── SaveManager.cs
│   └── AudioManager.cs
├── UpgradeManager (빈 오브젝트)
│   └── UpgradeManager.cs
├── SkillManager (빈 오브젝트)
│   └── SkillManager.cs
├── PrestigeManager (빈 오브젝트)
│   └── PrestigeManager.cs
├── CoachSystem (빈 오브젝트)
│   └── CoachSystem.cs
├── AchievementManager (빈 오브젝트)
│   └── AchievementManager.cs
├── DailyRewardSystem (빈 오브젝트)
│   └── DailyRewardSystem.cs
├── QuestSystem (빈 오브젝트)
│   └── QuestSystem.cs
├── MiniGameManager (빈 오브젝트)
│   └── MiniGameManager.cs
├── ShopManager (빈 오브젝트)
│   └── ShopManager.cs
├── LeaderboardManager (빈 오브젝트)
│   └── LeaderboardManager.cs
├── AdManager (빈 오브젝트)
│   └── AdManager.cs
├── VisualEffectsManager (빈 오브젝트)
│   └── VisualEffectsManager.cs
├── FloatingTextPool (빈 오브젝트)
│   └── FloatingTextPool.cs
├── MobileOptimization (빈 오브젝트)
│   └── MobileOptimization.cs
│
├── === CAMERA ===
├── Main Camera
│   └── CameraShake.cs
│
├── === GAME OBJECTS ===
├── Sandbag (Sprite)
│   └── Sandbag.cs
├── Boss (Sprite, 비활성화)
│   └── Boss.cs
│
└── === UI ===
    └── Canvas
        ├── MainPanel
        ├── UpgradePanel
        ├── SkillPanel
        ├── PrestigePanel
        ├── CoachPanel
        ├── ShopPanel
        ├── AchievementPanel
        ├── LeaderboardPanel
        ├── SettingsPanel
        └── DailyRewardPanel
```

### 3. UpgradeManager 설정

**Inspector 설정**:

1. `UpgradeManager` 오브젝트 선택
2. `Upgrades` 리스트 크기: **8**
3. 각 슬롯에 생성된 업그레이드 할당:
   ```
   Element 0: PunchPower
   Element 1: AutoClicker
   Element 2: CriticalChance
   Element 3: CriticalDamage
   Element 4: GoldMultiplier
   Element 5: OfflineEarnings
   Element 6: ClickDamage
   Element 7: GoldPerClick
   ```

### 4. AchievementManager 설정

**Inspector 설정**:

1. `AchievementManager` 오브젝트 선택
2. `All Achievements` 리스트 크기: **60**
3. `Assets/Data/Achievements/` 폴더 전체 선택 → 드래그 앤 드롭

### 5. ShopManager 설정

**Inspector 설정**:

1. `ShopManager` 오브젝트 선택
2. `Shop Items` 리스트 크기: **20**
3. `Assets/Data/Shop/` 폴더 전체 선택 → 드래그 앤 드롭
4. **Unity IAP 설정**:
   - Android Game ID: `1234567` (테스트)
   - iOS Game ID: `7654321` (테스트)
   - Test Mode: `✓` (체크)

---

## 매니저 설정

### GameManager 설정

```
Starting Gold: 0
Starting Damage: 1
Starting Stage: 1
Critical Chance: 0.05 (5%)
Critical Multiplier: 2.0
```

### SkillManager 설정

```
Skills 리스트 크기: 4
(스킬은 코드에서 자동 생성됨)
```

### PrestigeManager 설정

```
Prestige Currency: 0
Times Prestiged: 0
Min Stage For Prestige: 10
```

---

## 빌드 설정

### Android 빌드

1. `File > Build Settings`
2. Platform: `Android` 선택 → `Switch Platform`
3. `Player Settings`:
   ```
   Company Name: YourCompany
   Product Name: Punch King Clicker
   Package Name: com.yourcompany.punchking
   Version: 1.0.0
   Minimum API Level: Android 7.0 (API 24)
   Target API Level: API 33

   Other Settings:
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64 ✓
   - Internet Access: Required
   ```

4. `Build`

### iOS 빌드

1. `File > Build Settings`
2. Platform: `iOS` 선택 → `Switch Platform`
3. `Player Settings`:
   ```
   Bundle Identifier: com.yourcompany.punchking
   Version: 1.0.0
   Target minimum iOS Version: 12.0

   Other Settings:
   - Architecture: ARM64
   ```

4. `Build`

---

## 🎯 다음 단계

데이터 생성 완료 후:

1. ✅ **테스트 플레이**: Play 버튼으로 게임 실행
2. ✅ **디버그 확인**: Console에서 에러 확인
3. ✅ **데이터 검증**: 업그레이드/업적/상점 동작 확인
4. ✅ **빌드 테스트**: Android/iOS 빌드

---

## 📞 문제 해결

### Q: "Generate All Data" 메뉴가 안 보여요
**A**: `Assets/Scripts/Editor/DataGenerator.cs` 파일이 있는지 확인

### Q: 데이터가 생성되지 않아요
**A**: Console 창에서 에러 메시지 확인. `Assets/Data/` 폴더가 없으면 자동 생성됨

### Q: 업적이 60개가 아니라 더 적게 생성돼요
**A**: 정상입니다. 각 업적 타입당 5개 티어 = 12 × 5 = 60개

### Q: Inspector에서 데이터가 비어있어요
**A**:
1. 파일 재선택
2. Unity 재시작
3. `Assets > Reimport All`

---

## 🚀 최적화 팁

### 모바일 최적화

```csharp
// MobileOptimization.cs가 자동으로 처리:
- GPU Resident Drawer (Unity 6)
- Safe Area 자동 감지
- 디바이스별 렌더 스케일 조정
- 60 FPS 타겟
```

### 메모리 최적화

```csharp
// 오브젝트 풀링 자동 적용:
- DamagePopup: 20개 풀
- ParticleEffects: 20개 풀
- FloatingText: 30개 풀
```

---

**완료! 이제 게임을 플레이할 준비가 되었습니다!** 🎮
