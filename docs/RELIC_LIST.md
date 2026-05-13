# Frontier — 유물 목록

기준일: 저장소 기준 스냅샷. 데이터는 `sts2-frontier/localization/kor|eng/relics.json`, 유물 구현 `FrontierCode/Relics/*.cs`, 풀 정의 `FrontierCode/Character/ShumitRelicPool.cs`, 캐릭터 풀 연결 `FrontierCode/Character/ShumitCharacter.cs`를 따름.

## 요약

| 구분 | 개수 |
|------|------|
| 한·영 로컬라이즈가 있는 유물 (`*.title` 키 개수) | **12** |
| `ShumitRelicPool.GenerateAllRelics()`에 등록 — 일반 전투 보상 풀 | **6** |
| `EventRelicPool` — 이벤트·변환·시작 전용 | **6** |

### 풀별 분류

- **ShumitRelicPool** (캐릭터 전용 풀, `ShumitCharacter.RelicPool`이 가리키는 풀): 일반 전투 보상에서 등장하는 슈미트 전용 유물.
- **EventRelicPool** (본체 공용 이벤트/특수 풀): 일반 보상 외 경로(이벤트 보상, 시작 유물, «오로바스의 손길» 변환 등)로만 획득.

---

## 통계 (코드 기준, 12개)

### 희귀도 분포

| `RelicRarity` | 개수 | 해당 유물 |
|---------------|------|-----------|
| Starter | 2 | 판단의 눈, 신의 눈 |
| Common | 1 | 내열 가죽 앞치마 |
| Uncommon | 2 | 끝없는 노력, 타지않는 육체 |
| Rare | 3 | 헤파이스토스의 피, 걸작 박물관, 무한히 불타는 화로 |
| Shop | 1 | 오래된 모루 |
| Event | 3 | 융합자의 망치, 융합자의 집게, 융합자의 모루 |

### 풀별 분류 (8 + 4)

| 풀 | 개수 | 해당 유물 |
|----|------|-----------|
| **ShumitRelicPool** | 6 | 내열 가죽 앞치마, 헤파이스토스의 피, 끝없는 노력, 타지않는 육체, 걸작 박물관, 무한히 불타는 화로 |
| **EventRelicPool** | 6 | 판단의 눈, 신의 눈, 오래된 모루, 융합자의 망치, 융합자의 집게, 융합자의 모루 |

### 슈미트 Run 전용 (`IsAllowed`로 잠금)

`FrontierShumitRelicGate.IsShumitRun`(슈미트 캐릭터가 Run에 있는지)으로 다른 캐릭터 Run에서 등장을 막은 유물:

| 유물 | 위치 |
|------|------|
| 끝없는 노력 | `EndlessLaborRelic` |
| 타지않는 육체 | `UnburnableBodyRelic` |
| 걸작 박물관 | `MasterpieceMuseumRelic` |
| 무한히 불타는 화로 | `EternallyBurningFurnaceRelic` |
| 신의 눈 | `DivineEyeRelic` (`IsAllowed => false` — 변환 전용) |

> **참고**: `HeatproofApronRelic`/`HephaestusBloodRelic`은 `IsAllowed` 미오버라이드 — `ShumitRelicPool`에 속하므로 슈미트가 아닌 Run에서는 풀 자체가 사용되지 않아 자연스럽게 등장하지 않음. 단, 모드 호환성이 문제되면 추후 `IsAllowed`로 명시 잠금이 필요할 수 있음.

---

## 전체 표 (한글명 가나다순)

| 한글명 | 영문명 | 엔트리 ID | 희귀도 | 풀 | 슈미트 전용 잠금 |
|--------|--------|-----------|--------|----|-----|
| 걸작 박물관 | Masterpiece Museum | `FRONTIER-MASTERPIECE_MUSEUM_RELIC` | Rare | ShumitRelicPool | O |
| 끝없는 노력 | Endless Labor | `FRONTIER-ENDLESS_LABOR_RELIC` | Uncommon | ShumitRelicPool | O |
| 내열 가죽 앞치마 | Heatproof Apron | `FRONTIER-HEATPROOF_APRON_RELIC` | Common | ShumitRelicPool | (풀에서 격리) |
| 무한히 불타는 화로 | Eternally Burning Furnace | `FRONTIER-ETERNALLY_BURNING_FURNACE_RELIC` | Rare | ShumitRelicPool | O |
| 신의 눈 | Divine Eye | `FRONTIER-DIVINE_EYE_RELIC` | Starter | EventRelicPool | O (`IsAllowed => false`, 변환 전용) |
| 오래된 모루 | Ancient Anvil | `FRONTIER-ANCIENT_ANVIL_RELIC` | Shop | EventRelicPool | — |
| 융합자의 망치 | Fusioner's Hammer | `FRONTIER-FUSIONER_HAMMER_RELIC` | Event | EventRelicPool | — |
| 융합자의 모루 | Fusioner's Anvil | `FRONTIER-FUSIONER_ANVIL_RELIC` | Event | EventRelicPool | — |
| 융합자의 집게 | Fusioner's Tongs | `FRONTIER-FUSIONER_TONGS_RELIC` | Event | EventRelicPool | — |
| 타지않는 육체 | Unburnable Body | `FRONTIER-UNBURNABLE_BODY_RELIC` | Uncommon | ShumitRelicPool | O |
| 판단의 눈 | Broken Forge | `FRONTIER-BROKEN_FORGE_RELIC` | Starter | EventRelicPool | (슈미트 시작 유물) |
| 헤파이스토스의 피 | Hephaestus' Blood | `FRONTIER-HEPHAESTUS_BLOOD_RELIC` | Rare | ShumitRelicPool | (풀에서 격리) |

---

## 유물별 기능 요약

게임 내 문구는 `sts2-frontier/localization/kor/relics.json`의 `description`과 동일한 의미이며, 아래는 한 줄로 요약한 것입니다.

| 한글명 | 기능 요약 |
|--------|-----------|
| 걸작 박물관 | 전투 시작 시 보유한 [걸작] 카드 1장당 [힘] 2, [민첩] 2, [에너지] 2 획득. |
| 끝없는 노력 | 휴식처 [모루] 사용 시 강화할 카드를 2장 더 선택 가능. (`SmithRestSiteOption.SmithCount += 2`) |
| 내열 가죽 앞치마 | [열기]를 얻을 때마다 방어도 2 획득. (`AfterPowerAmountChanged` — `FRONTIER-HEAT_POWER` 증가 트리거) |
| 무한히 불타는 화로 | 전투 시작 시 [열기] 70 즉시 획득. [화상] 카드 드로우 시 그 카드를 [소진]하고 카드 1장 드로우 + [열기] 20 획득. |
| 신의 눈 | 모든 카드 [재련] 보장 **2 → 4** (`FrontierRules.GetReforgeBonus`). 모든 [걸작] 카드의 변환 기준 **−5** (`FrontierRules.GetMasterpieceValue`). 획득 시점에 이미 보유 중인 슈미트 걸작 카드의 `MasterpieceLeft` 표시값도 −5 동기화. |
| 오래된 모루 | 강화된 카드를 사용할 때마다 방어도 3 획득. |
| 융합자의 망치 | 강화된 카드를 사용할 때마다 [활력] 5 획득. |
| 융합자의 모루 | 강화된 카드를 사용할 때마다 방어도 5 획득. |
| 융합자의 집게 | 강화된 카드를 사용할 때마다 카드 1장 드로우. |
| 타지않는 육체 | [신체 화상]을 얻는 [열기] 임계값이 **200 → 300**으로 증가. 실제 임계값 변경은 `HeatPower.AfterTurnEnd` 측에서 처리. |
| 판단의 눈 | 모든 카드에 [재련] **2** 보장. (슈미트 시작 유물 — `ShumitCharacter.StartingRelics`) |
| 헤파이스토스의 피 | [열기] 20마다 [힘] 1 획득. (구간 경계를 넘을 때마다 `StrengthPower` 적용) |

---

## 획득 경로 메모

### 시작 유물

- **판단의 눈** (`BrokenForgeRelic`): 슈미트 캐릭터의 시작 유물(`ShumitCharacter.StartingRelics`). 모든 슈미트 카드의 «재련» 기본 보장 2를 제공.
- **신의 눈** (`DivineEyeRelic`): «오로바스의 손길»(`TouchOfOrobas`) 휴식처 행동 등으로 시작 유물을 강화했을 때만 등장. 일반 보상 풀에는 안 나옴(`IsAllowed => false`).

### 변환·이벤트 전용

- **오래된 모루**, **융합자의 망치/집게/모루**: `EventRelicPool`에 속하며 일반 보상에서는 등장하지 않음. 이벤트·상점·특수 보상에서 획득.

### 슈미트 풀 (일반 전투 보상)

`ShumitRelicPool`의 6종은 `Common`/`Uncommon`/`Rare` 슬롯에 따라 일반 전투 보상에서 등장.

- **Common**: 내열 가죽 앞치마
- **Uncommon**: 끝없는 노력, 타지않는 육체
- **Rare**: 헤파이스토스의 피, 걸작 박물관, 무한히 불타는 화로

---

## 구현 상태 메모

- **`ShumitCharacter.RelicPool` 연결**: `ModelDb.RelicPool<ShumitRelicPool>()`. 이전에는 `IroncladRelicPool`을 가리켜서 슈미트 전용 유물이 일반 보상에 등장하지 않던 버그가 있었으나 수정됨.
- **`AfterPowerAmountChanged`**: 슈미트 풀의 다수 유물이 `HeatPower`(`FRONTIER-HEAT_POWER`) 변화 트리거를 사용해 효과 발동. 외부 모드의 동명 파워와 충돌하지 않도록 풀 ID로 정확히 매칭.
- **`MasterpieceMuseumRelic.BeforeCombatStart`**: `player.Deck.Cards`에서 `FrontierRules.GetMasterpieceValue(card) > 0`인 카드 수를 센 뒤 2배만큼 [힘]/[민첩]/[에너지] 적용. 신의 눈 적용 후에도 「걸작 값 > 0」이면 카운트됨.
- **`EternallyBurningFurnaceRelic.AfterCardChangedPiles`**: `oldPileType == Draw && card.Pile.Type == Hand`인 [화상] 카드만 처리(매번 [소진] → [드로우] → [열기] +20).
- **`DivineEyeRelic.AfterObtained`**: 휴식처·이벤트(비전투)에서는 `player.Deck.Cards`만 동기화. 전투 중 획득 시(있다면) 손/뽑을/버린/소진 더미까지 모두 동기화. `ShumitCard.AfterCreated`도 본 유물의 `ApplyMasterpieceReductionIfApplicable`을 호출해 신규 생성 걸작 카드 인스턴스도 −5 보정.

---

## 엔트리 ID 빠른 목록 (정렬)

```
FRONTIER-ANCIENT_ANVIL_RELIC
FRONTIER-BROKEN_FORGE_RELIC
FRONTIER-DIVINE_EYE_RELIC
FRONTIER-ENDLESS_LABOR_RELIC
FRONTIER-ETERNALLY_BURNING_FURNACE_RELIC
FRONTIER-FUSIONER_ANVIL_RELIC
FRONTIER-FUSIONER_HAMMER_RELIC
FRONTIER-FUSIONER_TONGS_RELIC
FRONTIER-HEATPROOF_APRON_RELIC
FRONTIER-HEPHAESTUS_BLOOD_RELIC
FRONTIER-MASTERPIECE_MUSEUM_RELIC
FRONTIER-UNBURNABLE_BODY_RELIC
```
