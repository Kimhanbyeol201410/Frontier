# Frontier — 카드 목록

기준일: 저장소 기준 스냅샷. 데이터는 `sts2-frontier/localization/kor|eng/cards.json`, 카드 구현 `FrontierCode/Cards/*.cs`, 풀 정의 `FrontierCode/Character/ShumitCardPool.cs`, 풀 필터 `FrontierCode/MainFile.cs`의 `FrontierRules`를 따름.

**각 카드가 무엇을 하는지**는 아래 **[카드별 기능 요약](#카드별-기능-요약)** 표를 보면 됩니다. **비용·타입·방어·AOE 등 집계**는 **[통계 (코드 기준, 85장)](#통계-코드-기준-85장)** 을 참고하세요. 수치·조건은 게임 내 카드 설명(로컬 JSON)과 동적 변수가 기준이며, 코드와 로컬이 어긋난 경우는 코드·키워드(`Exhaust` 등)가 우선입니다.

## 요약

| 구분 | 개수 |
|------|------|
| 한·영 로컬라이즈가 있는 카드 (`*.title` 키 개수) | **85** |
| `ShumitCardPool.GenerateAllCards()`에 등록 | **79** |
| 풀에 없고 전용 경로로만 등장하는 카드 | **6** (`기민함/메아리/숙련/예리함 제련`, `불사르지 않는 몸`, `모루의 기억`) |
| 풀에 있으나 변환·이벤트로만 등장하도록 `FilterThroughEpochs`에서 제외되는 카드 | **4** (`고대의 단조`, `무수히 많은 기억`, `광란의 열기`, `모루의 기억`) |
| 코드에만 있고 로컬 없음(개발용 스켈레톤) | `SkeletonCard`, `SkeletonUncommonCard`, `SkeletonRareCard` — 아래 표에서 제외 |

### 보상·상점 등에서의 동작 메모

- `ShumitCardPool.FilterThroughEpochs`로 **보상/상점 풀에서 제외**되는 엔트리:
  - `FrontierRules.HiddenFromShumitCardPoolEntries`: 시작 덱 4종(`타격`, `수비`, `단조`, `유냉`), 인챈트 4종(`기민함/메아리/숙련/예리함 제련`), `불사르지 않는 몸`
  - 변환 전용: `모루의 기억` (걸작 «모루의 잔향 +5»), `광란의 열기` (걸작 «멈출 수 없는 열기 +5»), `고대의 단조` (유물 «고대의 이빨»), `무수히 많은 기억` (유물 «먼지 쌓인 책»)
- **백과사전(Card Library)** 에서는 `FrontierCardLibraryUnlockPatch`로 위 카드들이 자물쇠 없이 표시됨. 아직 게임에서 본 적 없는 카드는 "?" 흐릿(NotSeen) 상태가 됨.
- **풀 미등록 카드** 6종은 전투 종료 인챈트, 걸작 변환 등으로만 덱에 들어갈 수 있음.
- **토큰 카드(`TokenCardBase`)**: `철검`/`철방패`/`철갑옷` — `showInCardLibrary: false` 라 백과사전에 안 보임. `MaxUpgradeLevel == 0`인 토큰은 강화 대상이 아님(슈미트 재련 상한 패치와 무관).

---

## 통계 (코드 기준, 85장)

아래는 `FrontierCode/Cards`의 구체 카드 클래스 생성자 `base(에너지, CardType, …)`를 기준으로 한 집계입니다. **업그레이드로 비용이 바뀌는 카드**(예: 명인의 긍지, 재사용의 미학)는 **기본(0강) 표시 비용**만 반영합니다. 스켈레톤·추상 베이스(`Skeleton*`, `ShumitCard`, `TokenCardBase`)는 제외합니다.

### 타입 분포

| `CardType` | 장수 | 비고 |
|------------|------|------|
| Attack | **30** | 단일/다중 타격·피해 중심 |
| Skill | **36** | 방어·드로·소멸·열기 조절·토큰 생성 등 |
| Power | **18** | 지속 효과·전투 종료 인챈트 트리거·토큰 파워 |
| Status | **1** | 냉온화상 |

### 표시 에너지 비용 분포

| 비용 | 장수 |
|------|------|
| **-2** | 1 |
| **0** | 10 |
| **1** | 49 |
| **2** | 22 |
| **3** | 2 |
| **4** | 1 |
| **합계** | **85** |

- **-2코**: 냉온화상(사용 불가·상태 카드).
- **0코 10장**: 고대의 단조, 목숨을 걸어, 차가운 손짓, 두드리기, 임시 담금질, 집게질, 지쳐 쓰러질 때까지, 열에너지 전환, 환기, 제조.
- **1코 49장**: 가장 많음. 일반적인 공격/방어/스킬 다수.
- **2코 22장**: 강력한 공격·파워 카드. `절대영도`/`불태우는 일격`/`목숨을 걸어`/`멈출 수 없는 열기` 등.
- **3코 2장**: 단층 파괴, 증기 배출.
- **4코 1장**: 신의 형상 (강화 시 3코로 감소).

### 고코스트 (표시 **3+** 에너지, 3장)

| 한글명 | 타입 | 비용 | 비고 |
|--------|------|------|------|
| 단층 파괴 | Attack | 3 | 전체 공격(`AllEnemies`) |
| 증기 배출 | Power | 3 | 열기 감소 시마다 전체 피해. **재련** 상한 |
| 신의 형상 | Power | 4 | 내 턴 시작 시 손패 전체 강화(강화 가능한 카드만). **강화 시 비용 −1 (3)** |

### 2코스트 (22장, "중간 비용")

가나다순: 가열로 정비, 걸작을 위한 망치질, 광란의 열기, 금속 액화, 대장장이의 가호, 리버스 엔지니어링, 메아리 제련, 멈출 수 없는 열기, 모루의 기억, 모루의 잔향, 무수히 많은 기억, 배기 시스템, 불꽃 튀기기, 불사르지 않는 몸, 불순물 압착, 불태우는 일격, 뜨거운 노력, 절대영도, 정련, 화염 강타, 화염의 갑옷, 화염의 심장

### X 비용 (추가 에너지, `HasEnergyCostX`)

- **지쳐 쓰러질 때 까지** — 기본 0코 + X.
- **열에너지 전환** — 기본 0코 + X.

### 조건부 비용

- **가열**: 플레이 시점에 열기가 0이면 **이번 사용 비용 0**(기본 생성자 비용은 1의 Attack).

### 강화 시 비용 변경 카드

- **재사용의 미학**: 1강(재련 1)에서 코스트 **1 → 0**. 2강에서 «소멸» 대신 «덱 영구 제거»로 변경(키워드도 제거).
- **명인의 긍지**: 강화 시 비용 감소(코드 주석 기준).
- **신의 형상**: 강화 시 비용 −1(4 → 3).
- **화염의 심장**: 강화 시 비용 −1(2 → 1).
- **리버스 엔지니어링**: 강화 시 비용 −1(2 → 1).

### 전체 공격(카드 타겟이 `AllEnemies`, 8장)

모루의 기억, 모루의 잔향, 무수히 많은 기억(범위는 코드 확인 필요), 광란의 열기, 단층 파괴, 멈출 수 없는 열기, 불꽃 튀기기, 불순물 압착, 화염 파동

### 플레이 시 **방어도(Block)** 직접 부여 (`CreatureCmd.GainBlock`)

**14장**: 절대영도(제거한 열기만큼), 수비, 차가운 심장, 달궈진 방패, 단단한 갑옷, 불사르지 않는 몸, 불순물 제거, 유냉, 임시 담금질, 재료 수급, 수냉, 가열로 정비, 냉각, 화염의 갑옷(플레이 시 기본 방어도 + 이후 화상 연동은 파워 쪽).

### 판금(Plating)만 부여 — 방어도와 별개

- **철갑옷** (토큰 스킬)

### 방어와 연관되나 "플레이 직후 블록 숫자"가 아닌 것

- **명인의 긍지** 파워: 이후 **카드 강화 시** 방어도.
- **화염의 갑옷** 파워: **화상 생성 시** 추가 방어도.

### 적의 방어도를 깎거나 제거하는 공격

- **파쇄의 망치질**: 피해 후 대상에 [취약]·[약화] 부여 (Uncommon).
- **단층 파괴**: 방어가 있었던 적에게 피해 2배 후 남은 방어 제거.

### 디버프 부여 공격

- **틈새**: 피해 + [약화] (기본 1, 강화 시 +1).
- **내려찍기**: 피해 + [취약] (기본 1, 강화 시 +1).
- **화염 강타**: 피해 + [취약] (기본 1, 강화 시 +1) + 열기.
- **열정! 패기!**: 피해 2번 + [취약] (기본 2, 강화 시 +1).
- **파쇄의 망치질**: 피해 + [취약]/[약화] (기본 1, 강화 시 +1).
- **모루의 기억**: [취약]·[약화] 부여 후 다타격.

---

## 전체 표 (한글명 가나다순)

| 한글명 | 영문명 | 엔트리 ID | 타입 | 비용 | 희귀도 | 풀 등록 |
|--------|--------|-----------|------|------|--------|---------|
| 가열 | Heating | `FRONTIER-HEATING_CARD` | Attack | 1 | Uncommon | O |
| 가열로 정비 | Furnace Maintenance | `FRONTIER-FURNACE_MAINTENANCE_CARD` | Skill | 2 | Common | O |
| 가열된 망치 | Heated Hammer | `FRONTIER-HEATED_HAMMER_CARD` | Attack | 1 | Common | O |
| 걸작을 위한 망치질 | Masterpiece Hammer | `FRONTIER-MASTERPIECE_HAMMER_CARD` | Attack | 2 | Uncommon | O |
| 고대의 단조 | Ancient Forging | `FRONTIER-ANCIENT_FORGING_CARD` | Attack | 0 | Ancient | O (보상·변환 제외, «고대의 이빨» 전용) |
| 광란의 열기 | Frenzied Heat | `FRONTIER-FRENZIED_HEAT_CARD` | Attack | 2 | Rare | O (걸작 «멈출 수 없는 열기 +5» 전용) |
| 금속 액화 | Metal Liquefaction | `FRONTIER-METAL_LIQUEFACTION_CARD` | Skill | 2 | Uncommon | O |
| 기민함 제련 | Agile Smelting | `FRONTIER-AGILE_SMELTING_CARD` | Power | 1 | Uncommon | **풀 없음** |
| 내려찍기 | Hammer Down | `FRONTIER-HAMMER_DOWN_CARD` | Attack | 1 | Common | O |
| 냉각 | Thermal Cooling | `FRONTIER-THERMAL_COOLING_CARD` | Skill | 1 | Rare | O |
| 냉각 시스템 | Cooling System | `FRONTIER-COOLING_SYSTEM_CARD` | Power | 1 | Uncommon | O |
| 냉각 타격 | Cooling Strike | `FRONTIER-COOLING_STRIKE_CARD` | Attack | 1 | Common | O |
| 냉온화상 | Cold Burn Status | `FRONTIER-COLD_BURN_STATUS_CARD` | Status | -2 | Event | O |
| 녹이기 | Melting | `FRONTIER-MELTING_CARD` | Skill | 1 | Uncommon | O |
| 다가가는 공포 | Approaching Dread | `FRONTIER-APPROACHING_DREAD_CARD` | Attack | 1 | Uncommon | O |
| 단단한 갑옷 | Sturdy Armor | `FRONTIER-STURDY_ARMOR_CARD` | Skill | 1 | Common | O |
| 단조 | Forging | `FRONTIER-FORGING_CARD` | Attack | 1 | Basic | O (보상 풀 제외) |
| 단층 파괴 | Fault Break | `FRONTIER-FAULT_BREAK_CARD` | Attack | 3 | Rare | O |
| 달궈진 방패 | Heated Shield | `FRONTIER-HEATED_SHIELD_CARD` | Skill | 1 | Common | O |
| 대장간 | Forge | `FRONTIER-FORGE_CARD` | Skill | 1 | Rare | O |
| 대장장이의 가호 | Blacksmith's Blessing | `FRONTIER-BLACKSMITHS_BLESSING_CARD` | Skill | 2 | Rare | O |
| 두 번 두드리기 | Double Tap | `FRONTIER-DOUBLE_TAP_CARD` | Attack | 1 | Common | O |
| 두드리기 | Tap | `FRONTIER-TAP_CARD` | Attack | 0 | Common | O |
| 뜨거운 노력 | Hot Effort | `FRONTIER-FEARLESS_OF_FLAME_CARD` | Power | 2 | Uncommon | O |
| 뜨거워진 대장간 | Heated Forge | `FRONTIER-HEATED_FORGE_CARD` | Power | 1 | Uncommon | O |
| 리버스 엔지니어링 | Reverse Engineering | `FRONTIER-REVERSE_ENGINEERING_CARD` | Skill | 2 | Rare | O |
| 머리 식히기 | Cool Head | `FRONTIER-COOL_HEAD_CARD` | Skill | 1 | Common | O |
| 멈출 수 없는 열기 | Unstoppable Heat | `FRONTIER-UNSTOPPABLE_HEAT_CARD` | Attack | 2 | Uncommon | O |
| 메아리 제련 | Echo Smelting | `FRONTIER-ECHO_SMELTING_CARD` | Power | 2 | Rare | **풀 없음** |
| 명인의 긍지 | Master Pride | `FRONTIER-MASTER_PRIDE_CARD` | Power | 1 | Uncommon | O |
| 모루의 기억 | Anvil Memory | `FRONTIER-ANVIL_MEMORY_CARD` | Attack | 2 | Rare | **풀 없음** (걸작 «모루의 잔향 +5» 전용) |
| 모루의 잔향 | Anvil Echo | `FRONTIER-ANVIL_ECHO_CARD` | Attack | 2 | Uncommon | O |
| 무수히 많은 기억 | Countless Memories | `FRONTIER-COUNTLESS_MEMORIES_CARD` | Power | 2 | Ancient | O (보상·변환 제외, «먼지 쌓인 책» 전용) |
| 목숨을 걸어 | Bet Your Life | `FRONTIER-BET_YOUR_LIFE_CARD` | Power | 0 | Rare | O |
| 배기 시스템 | Exhaust System | `FRONTIER-EXHAUST_SYSTEM_CARD` | Power | 2 | Uncommon | O |
| 불꽃 튀기기 | Spark Burst | `FRONTIER-SPARK_BURST_CARD` | Attack | 2 | Uncommon | O |
| 불사르지 않는 몸 | Unburning Body | `FRONTIER-UNBURNING_BODY_CARD` | Skill | 2 | Common | **풀 없음** |
| 불순물 압착 | Impurity Compression | `FRONTIER-IMPURITY_COMPRESSION_CARD` | Attack | 2 | Rare | O |
| 불순물 제거 | Impurity Removal | `FRONTIER-IMPURITY_REMOVAL_CARD` | Skill | 1 | Common | O |
| 불태우는 일격 | Burning Strike | `FRONTIER-BURNING_STRIKE_CARD` | Attack | 2 | Rare | O |
| 설계의 완성 | Design Completion | `FRONTIER-DESIGN_COMPLETION_CARD` | Skill | 1 | Uncommon | O |
| 수냉 | Water Cooling | `FRONTIER-WATER_COOLING_CARD` | Skill | 1 | Common | O |
| 수비 | Defend | `FRONTIER-DEFEND_SHUMIT_CARD` | Skill | 1 | Basic | O (보상 풀 제외) |
| 숙련 제련 | Mastery Smelting | `FRONTIER-MASTERY_SMELTING_CARD` | Power | 1 | Rare | **풀 없음** |
| 신의 형상 | Divine Form | `FRONTIER-DIVINE_FORM_CARD` | Power | 4 | Rare | O |
| 연료 최대로 | Full Fuel | `FRONTIER-FULL_FUEL_CARD` | Skill | 1 | Common | O |
| 연마실 | Grinding Room | `FRONTIER-GRINDING_ROOM_CARD` | Skill | 1 | Uncommon | O |
| 열 교환 | Heat Exchange | `FRONTIER-HEAT_EXCHANGE_CARD` | Skill | 1 | Uncommon | O |
| 열기 순환 | Heat Cycle | `FRONTIER-HEAT_CYCLE_CARD` | Skill | 1 | Common | O |
| 열에너지 전환 | Thermal Energy Conversion | `FRONTIER-THERMAL_ENERGY_CONVERSION_CARD` | Skill | 0+X | Rare | O |
| 열정! 패기! | Passion! Pluck! | `FRONTIER-PASSION_VERVE_CARD` | Attack | 1 | Uncommon | O |
| 예리한 제련 | Sharp Smelting | `FRONTIER-SHARP_SMELTING_CARD` | Power | 1 | Uncommon | **풀 없음** |
| 용광로 | Blast Furnace | `FRONTIER-BLAST_FURNACE_CARD` | Skill | 1 | Rare | O |
| 유냉 | Oil Cooling | `FRONTIER-OIL_COOLING_CARD` | Skill | 1 | Basic | O (보상 풀 제외) |
| 융해 | Melt | `FRONTIER-MELT_CARD` | Skill | 1 | Uncommon | O |
| 이게 아니야! | Not This One! | `FRONTIER-NOT_THIS_ONE_CARD` | Attack | 1 | Rare | O |
| 임시 담금질 | Temporary Quenching | `FRONTIER-TEMPORARY_QUENCHING_CARD` | Skill | 0 | Common | O |
| 재료 수급 | Material Gather | `FRONTIER-MATERIAL_GATHER_CARD` | Skill | 1 | Common | O |
| 재사용의 미학 | Reuse Aesthetics | `FRONTIER-REUSE_AESTHETICS_CARD` | Skill | 1 | Uncommon | O |
| 절대영도 | Absolute Zero | `FRONTIER-ABSOLUTE_ZERO_CARD` | Skill | 2 | Uncommon | O |
| 정련 | Refining | `FRONTIER-REFINING_CARD` | Attack | 2 | Rare | O |
| 제련 설계 | Smelting Design | `FRONTIER-SMELTING_DESIGN_CARD` | Skill | 1 | Common | O |
| 제련 타격 | Smelting Strike | `FRONTIER-SMELTING_STRIKE_CARD` | Attack | 1 | Common | O |
| 제련소 | Smelter | `FRONTIER-SMELTER_CARD` | Skill | 1 | Uncommon | O |
| 제조 | Manufacture | `FRONTIER-MANUFACTURE_CARD` | Skill | 0 | Uncommon | O |
| 증기 배출 | Steam Release | `FRONTIER-STEAM_RELEASE_CARD` | Power | 3 | Uncommon | O |
| 지쳐 쓰러질 때 까지 | Until Exhaustion | `FRONTIER-UNTIL_EXHAUSTION_CARD` | Attack | 0+X | Rare | O |
| 집게질 | Tongs | `FRONTIER-TONGS_CARD` | Skill | 0 | Uncommon | O |
| 차가운 손짓 | Cold Gesture | `FRONTIER-COLD_GESTURE_CARD` | Attack | 0 | Common | O |
| 차가운 심장 | Cold Heart | `FRONTIER-COLD_HEART_CARD` | Skill | 1 | Uncommon | O |
| 철갑옷 | Iron Armor | `FRONTIER-IRON_ARMOR_CARD` | Power | 1 | Event | O (토큰, 라이브러리 비표시) |
| 철검 | Iron Sword | `FRONTIER-IRON_SWORD_CARD` | Power | 1 | Event | O (토큰, 라이브러리 비표시) |
| 철방패 | Iron Shield | `FRONTIER-IRON_SHIELD_CARD` | Power | 1 | Event | O (토큰, 라이브러리 비표시) |
| 타격 | Strike | `FRONTIER-STRIKE_SHUMIT_CARD` | Attack | 1 | Basic | O (보상 풀 제외) |
| 틈새 | Crevice | `FRONTIER-CREVICE_CARD` | Attack | 1 | Common | O |
| 파쇄의 망치질 | Crushing Hammer | `FRONTIER-CRUSHING_HAMMER_CARD` | Attack | 1 | Uncommon | O |
| 풀무질 | Bellows | `FRONTIER-BELLOWS_CARD` | Skill | 1 | Common | O |
| 화력발전기 | Fire Power Plant | `FRONTIER-FIRE_POWER_PLANT_CARD` | Skill | 1 | Uncommon | O |
| 화염 강타 | Flame Smash | `FRONTIER-FLAME_SMASH_CARD` | Attack | 2 | Common | O |
| 화염 타격 | Flame Strike | `FRONTIER-FLAME_STRIKE_CARD` | Attack | 1 | Common | O |
| 화염 파동 | Flame Pulse | `FRONTIER-FLAME_PULSE_CARD` | Attack | 1 | Common | O |
| 화염의 갑옷 | Flame Armor | `FRONTIER-FLAME_ARMOR_CARD` | Power | 2 | Uncommon | O |
| 화염의 심장 | Heart of Flame | `FRONTIER-HEART_OF_FLAME_CARD` | Power | 2 | Uncommon | O |
| 환기 | Ventilation | `FRONTIER-VENTILATION_CARD` | Skill | 0 | Common | O |
| 접쇠 | Folded Steel | `FRONTIER-FOLDED_STEEL_CARD` | Skill | 1 | Uncommon | O |

---

## 카드별 기능 요약

게임 내 문구는 `sts2-frontier/localization/kor/cards.json`의 `description`과 동일한 의미이며, 아래는 동적 수치(`{…}`)는 생략하고 한 줄로 요약한 것입니다.

| 한글명 | 기능 요약 |
|--------|-----------|
| 가열 | 피해. 열기 0이면 비용 0·추가 드로우·열기 획득. |
| 가열로 정비 | 방어도, 열기. 이번 턴 화상이 소멸했으면 에너지 보너스. 소멸. |
| 가열된 망치 | 피해 + 열기 10당 추가 피해. |
| 걸작을 위한 망치질 | 피해 + 열기. |
| 고대의 단조 | 피해. 손에 있는 모든 카드 강화. (유물 «고대의 이빨» 변환 전용) |
| 광란의 열기 | 모든 적에게 피해 2회 + 열기 20당 추가 피해, 열기 40 획득. **재련** 5. (걸작 «멈출 수 없는 열기 +5» 변환 전용) |
| 금속 액화 | 강화된 공격 1장 선택 → 당일 비용 0. 강화 공격 사용 시마다 열기. 뽑을 더미에 화상 섞기. |
| 기민함 제련 | 전투 종료 후 덱의 일정 장수에 **기민함 인챈트** 부여. |
| 내려찍기 | 피해 + [취약] 1 부여(강화 시 2). |
| 냉각 | [열화](손패 강화 1단계 제거) 후 방어도·열기 감소. |
| 냉각 시스템 | 파워: 매 턴 시작 시 열기 감소·방어도 획득. |
| 냉각 타격 | 피해 + 열기 감소. |
| 냉온화상 | 사용 불가. 내 턴 종료 시 손에 있으면 체력 손실. |
| 녹이기 | 손패 1장 소멸, 카드 2장 뽑기(강화 시 3장). |
| 다가가는 공포 | 피해. 이번 턴 열기 구간당 힘 획득, 턴 종료 시 해당 힘 상실. |
| 단단한 갑옷 | 방어도. |
| 단조 | 피해. 손패 카드 일정 장 강화. (시작 카드, 보상 풀 제외) |
| 단층 파괴 | 전체 피해. 대상이 방어를 가졌으면 피해 2배 후 남은 방어 제거. |
| 달궈진 방패 | 방어도 + 열기 20당 추가 방어도. **재련** 5. |
| 대장간 | 손패 무작위 강화 + 열기 획득. |
| 대장장이의 가호 | 열기 구간당 힘·민첩 1씩 획득 후 열기 2배. |
| 두 번 두드리기 | 피해를 여러 히트로, 열기 획득. |
| 두드리기 | 피해 + 열기. |
| 뜨거운 노력 | 사용 시 **즉시 [힘] 2 획득** + 파워: 매 턴 시작마다 열기 10 획득. |
| 뜨거워진 대장간 | 파워(Instanced): 매 턴 종료 시 현재 열기 비례 체력 회복. |
| 리버스 엔지니어링 | 손패 1장 소멸 → 그 카드 표시 비용만큼 에너지, 피해 수치만큼 이번 턴 힘, 방어 수치만큼 이번 턴 민첩, 이번 턴 열기 증감 반전. **강화 시 비용 1**. |
| 머리 식히기 | 방어도, 열기 감소. 다음 내 턴 시작 시 카드 1장 드로우 + 열기 감소. |
| 멈출 수 없는 열기 | 전체 피해 + 열기 구간당 피해 증가. **걸작** (변환 시 «광란의 열기»). |
| 메아리 제련 | 전투 종료 후 덱 일부에 **메아리 인챈트** 부여. |
| 명인의 긍지 | 파워(카운터): 카드 강화할 때마다 방어도. **강화 시 비용 감소**. |
| 모루의 기억 | 모든 적에게 [취약]·[약화] 부여 후 다타격. **재련** 5. (걸작 «모루의 잔향 +5» 변환 전용) |
| 모루의 잔향 | 모든 적에게 피해를 여러 번. **걸작** (변환 시 «모루의 기억»). |
| 무수히 많은 기억 | 카드 강화 누적 횟수만큼, 이후 X턴 동안 매 턴 시작 시 에너지 및 추가 드로우. (유물 «먼지 쌓인 책» 변환 전용) |
| 목숨을 걸어 | 파워(카운터): 신체 화상 더 이상 감소 X. 덱 모든 카드 강화. 신체 화상 획득. 스킬 사용 시 힘, 공격 사용 시 민첩. |
| 배기 시스템 | 파워: 턴 시작 시 열기가 임계 이상이면 열기 10 감소 후 카드 1장 드로우. |
| 불꽃 튀기기 | 전체 피해 + 열기. **재련** 10. |
| 불사르지 않는 몸 | 방어도, 버린 카드 더미에 [화상] 1장. (현재 풀 미등록·미구현 표시) |
| 불순물 압착 | 모든 위치(뽑을/손/버린)의 [화상] 전부 소멸, 소멸 수만큼 보너스 피해로 전체 공격, 열기. |
| 불순물 제거 | 방어도. 버림/손패의 [화상] 일정 장 소멸. |
| 불태우는 일격 | 피해 + 열기 획득. **재련** 무제한 강화. |
| 설계의 완성 | 뽑을 더미에서 카드 선택 → 강화 → 손패로. 소멸. |
| 수냉 | 방어도, 열기 감소, 손패 무작위 1장 강화. |
| 수비 | 방어도. (시작 카드, 보상 풀 제외) |
| 숙련 제련 | 전투 종료 후 덱 일부에 **숙련 인챈트** 부여. |
| 신의 형상 | 파워(카운터): 즉시 손패 모든 카드 강화 + 매 턴 시작 시 손패 모든 카드 강화. **강화 시 비용 −1**. |
| 연료 최대로 | 방어도, 현재 열기 2배, 다음 턴 시작 시 에너지. |
| 연마실 | 사용 시 [활력] 획득. |
| 열 교환 | 방어도. 손패 1장을 강화. 열기 감소. |
| 열기 순환 | 방어도, 드로우. 열기 70 미만이면 열기 획득, 이상이면 감소. |
| 열에너지 전환 | X 비용. 보유 열기 구간당 에너지 획득 → 모든 열기 제거 → 카드 X장 드로우. 소멸. |
| 열정! 패기! | 피해 2회 + [취약] 2 부여(강화 시 +1). |
| 예리한 제련 | 전투 종료 후 덱 일부에 **예리함 인챈트** 부여. |
| 용광로 | 에너지 1 + 열기 획득. |
| 유냉 | 방어도, 열기 감소, 손패 무작위 1장 강화. (시작 카드, 보상 풀 제외) |
| 융해 | 손패 1장 소멸, 열기 30. |
| 이게 아니야! | 손패 1장 소멸, 피해, 에너지. |
| 임시 담금질 | 방어도, 열기 감소. |
| 재료 수급 | 방어도 + 드로우(강화 시 2장). |
| 재사용의 미학 | 소멸 더미에서 1장 손패로. **0강 소멸 / 1강 비용 0 + 소멸 / 2강 비용 0 + 영구 제거**. |
| 절대영도 | 열기를 모두 제거하고, 제거한 만큼 방어도. |
| 정련 | 피해. 처치 시 이 카드 **영구 강화**. **재련** 무제한. |
| 제련 설계 | 카드 드로우 후 뽑은 카드 강화. |
| 제련 타격 | 피해, 손패 무작위 강화, 열기. |
| 제련소 | 손패 1장 강화 + 열기 감소. |
| 제조 | [철검]/[철갑옷]/[철방패] 중 무작위 1장 손에 생성. |
| 증기 배출 | 파워: 열기가 줄어들 때마다 전체 피해(기본 4, 강화당 +3). **재련** 상한. |
| 지쳐 쓰러질 때 까지 | X 비용. X회 무작위 적 피해, 손패 무작위 강화, 열기, 턴 종료. |
| 집게질 | 손패 1장 강화 + 열기. (0코, Uncommon) |
| 차가운 손짓 | 피해, 방어도, 열기 감소. |
| 차가운 심장 | 방어도, 열기 감소, 카드 1장 드로우. |
| 철갑옷 | [판금] 획득. (토큰, 백과사전 비표시) |
| 철검 | [힘] 획득. (토큰, 백과사전 비표시) |
| 철방패 | [민첩] 획득. (토큰, 백과사전 비표시) |
| 타격 | 피해. (시작 카드, 보상 풀 제외) |
| 틈새 | 피해 5(강화 시 +2) + [약화] 1 부여(강화 시 +1). |
| 파쇄의 망치질 | 피해 + [취약]·[약화] 1씩 부여(강화 시 +1). |
| 풀무질 | 열기 획득, 카드 1장 드로우. **재련** 무제한 강화. |
| 화력발전기 | 손패 1장 소멸. 소멸한 카드가 [화상]이면 에너지. |
| 화염 강타 | 피해 11(강화 시 +4) + [취약] 1 부여(강화 시 +1) + 열기 15. (2코 Common) |
| 화염 타격 | 피해 7(강화 시 +값) + 열기 10. |
| 화염 파동 | 모든 적에게 피해 8(강화 시 +값) + 열기 10. |
| 화염의 갑옷 | 방어도. [화상]이 생성될 때마다 추가 방어도. |
| 화염의 심장 | 파워(카운터): 내 [화상] 카드가 더미에 들어올 때마다 [gold]에너지[/gold] 1 획득. **강화 시 비용 −1**. |
| 환기 | 이번 턴 카드 사용마다 열기 잃기 + 방어도 3 획득. (턴 종료 시 제거) |
| 접쇠 | 다음 강화 카드 1장 일정 횟수 재사용. 열기 임계 이상이면 +. 재사용 1회마다 [열기] +15. |

---

## 희귀도 요약

| 희귀도 | 장수 | 해당 카드(한글명) |
|--------|------|---------------------|
| Basic | 4 | 타격, 수비, 단조, 유냉 |
| Common | 25 | 차가운 손짓, 머리 식히기, 냉각 타격, 틈새, 두 번 두드리기, 화염 파동, 화염 강타, 화염 타격, 연료 최대로, 가열로 정비, 내려찍기, 열기 순환, 가열된 망치, 달궈진 방패, 불순물 제거, 재료 수급, 제련 설계, 제련 타격, 단단한 갑옷, 두드리기, 임시 담금질, 환기, 수냉, 풀무질, 불사르지 않는 몸 |
| Uncommon | 32 | 절대영도, 기민함 제련, 모루의 잔향, 다가가는 공포, 차가운 심장, 냉각 시스템, 파쇄의 망치질, 설계의 완성, 배기 시스템, 뜨거운 노력, 화력발전기, 화염의 갑옷, 접쇠, 화염의 심장, 열 교환, 열정! 패기!, 뜨거워진 대장간, 가열, 걸작을 위한 망치질, 명인의 긍지, 융해, 녹이기, 금속 액화, 재사용의 미학, 예리한 제련, 제련소, 불꽃 튀기기, 증기 배출, 집게질, 멈출 수 없는 열기, 제조, 연마실 |
| Rare | 18 | 모루의 기억, 목숨을 걸어, 용광로, 대장장이의 가호, 불태우는 일격, 냉각, 메아리 제련, 단층 파괴, 대장간, 광란의 열기, 숙련 제련, 신의 형상, 정련, 리버스 엔지니어링, 이게 아니야!, 열에너지 전환, 불순물 압착, 지쳐 쓰러질 때 까지 |
| Ancient | 2 | 고대의 단조, 무수히 많은 기억 |
| Event | 4 | 냉온화상, 철갑옷, 철검, 철방패 |

---

## 구현 상태 메모 (로컬 설명 기준)

- **화염의 심장**: `ShumitHeartOfFlameEnergyPower`(`PowerStackType.Counter`) — 내 소유 화상이 `oldPileType == None`으로 더미에 들어올 때마다 `GainEnergy(1)` (전투당 카드 인스턴스당 첫 진입 1회).
- **리버스 엔지니어링**: 소멸 대상의 `EnergyCost.GetAmountToSpend()`·동적 변수 `Damage`/`Block` 기준 수치 부여; `ShumitReverseEngineeringInvertHeatPower` + `FrontierHeatPowerApplyAmountInvertPatch`로 이번 턴 `HeatPower` 적용량 부호 반전.
- **신의 형상**: `ShumitDivineFormPower`(`Counter`) — `AfterPlayerTurnStart`에서 손패 `IsUpgradable` 카드만 모아 `CardCmd.Upgrade` 일괄 호출.
- **머리 식히기**: `CoolHeadCard` — 플레이 시 `FrontierHeatUtil.ReduceHeat`; `ShumitCoolHeadNextTurnPower`(`Amount` = 다음 턴 열기 감소량)로 다음 내 턴 시작 시 `Draw(1)` 후 `ReduceHeat`.
- **환기**: `VentilationCard` — `ShumitVentilationThisTurnPower`(`IsInstanced`); `AfterCardPlayed`에서 `ReduceHeat`, 플레이어 턴 종료에 `PowerCmd.Remove`.
- **냉각**: `ThermalCoolingCard` — 손패 `CurrentUpgradeLevel > 0` 카드 선택 후 `FrontierThermalDegradationUtil`이 `CardCmd.Downgrade`(리플렉션)로 열화 1회; 이어서 `GainBlock`, `ReduceHeat`.
- **연료 최대로**: `FullFuelCard` — 현재 열만큼 `ApplyHeat`(2배), `ShumitFuelMaxNextTurnEnergyPower`, 소멸 X.
- **뜨거운 노력**: `FearlessOfFlameCard` — 사용 시 `StrengthPower` 2를 1회 즉시 부여, 이어서 `ShumitFearlessFlamePower`로 매 턴 열기.
- **뜨거워진 대장간**: `ShumitHeatedForgePower`(`IsInstanced = true`) — 카드 인스턴스마다 독립 적용.
- **목숨을 걸어**: `ShumitBetYourLifePower`(`Counter`), 본인 카드 자체의 [신체 화상] 트리거 면제(`BodyBurnPower.AfterCardPlayed` 가드).
- **재사용의 미학**: 1강에서 `EnergyCost.UpgradeBy(-1)`, 2강에서 `RemoveKeyword(Exhaust)` + `RemoveOnPlay = 1` → 플레이 시 `CardPileCmd.RemoveFromCombat` + `RemoveFromDeck`.

---

## 엔트리 ID 빠른 목록 (정렬)

```
FRONTIER-ABSOLUTE_ZERO_CARD
FRONTIER-AGILE_SMELTING_CARD
FRONTIER-ANCIENT_FORGING_CARD
FRONTIER-ANVIL_ECHO_CARD
FRONTIER-ANVIL_MEMORY_CARD
FRONTIER-APPROACHING_DREAD_CARD
FRONTIER-BELLOWS_CARD
FRONTIER-BET_YOUR_LIFE_CARD
FRONTIER-BLAST_FURNACE_CARD
FRONTIER-BLACKSMITHS_BLESSING_CARD
FRONTIER-BURNING_STRIKE_CARD
FRONTIER-COLD_BURN_STATUS_CARD
FRONTIER-COLD_GESTURE_CARD
FRONTIER-COLD_HEART_CARD
FRONTIER-COOL_HEAD_CARD
FRONTIER-COOLING_STRIKE_CARD
FRONTIER-COOLING_SYSTEM_CARD
FRONTIER-COUNTLESS_MEMORIES_CARD
FRONTIER-CREVICE_CARD
FRONTIER-CRUSHING_HAMMER_CARD
FRONTIER-DEFEND_SHUMIT_CARD
FRONTIER-DESIGN_COMPLETION_CARD
FRONTIER-DIVINE_FORM_CARD
FRONTIER-DOUBLE_TAP_CARD
FRONTIER-ECHO_SMELTING_CARD
FRONTIER-EXHAUST_SYSTEM_CARD
FRONTIER-FAULT_BREAK_CARD
FRONTIER-FEARLESS_OF_FLAME_CARD
FRONTIER-FIRE_POWER_PLANT_CARD
FRONTIER-FLAME_ARMOR_CARD
FRONTIER-FLAME_PULSE_CARD
FRONTIER-FLAME_SMASH_CARD
FRONTIER-FLAME_STRIKE_CARD
FRONTIER-FOLDED_STEEL_CARD
FRONTIER-FORGE_CARD
FRONTIER-FORGING_CARD
FRONTIER-FRENZIED_HEAT_CARD
FRONTIER-FULL_FUEL_CARD
FRONTIER-FURNACE_MAINTENANCE_CARD
FRONTIER-GRINDING_ROOM_CARD
FRONTIER-HAMMER_DOWN_CARD
FRONTIER-HEART_OF_FLAME_CARD
FRONTIER-HEAT_CYCLE_CARD
FRONTIER-HEAT_EXCHANGE_CARD
FRONTIER-HEATED_FORGE_CARD
FRONTIER-HEATED_HAMMER_CARD
FRONTIER-HEATED_SHIELD_CARD
FRONTIER-HEATING_CARD
FRONTIER-IMPURITY_COMPRESSION_CARD
FRONTIER-IMPURITY_REMOVAL_CARD
FRONTIER-IRON_ARMOR_CARD
FRONTIER-IRON_SHIELD_CARD
FRONTIER-IRON_SWORD_CARD
FRONTIER-MANUFACTURE_CARD
FRONTIER-MASTERPIECE_HAMMER_CARD
FRONTIER-MASTER_PRIDE_CARD
FRONTIER-MASTERY_SMELTING_CARD
FRONTIER-MATERIAL_GATHER_CARD
FRONTIER-MELT_CARD
FRONTIER-MELTING_CARD
FRONTIER-METAL_LIQUEFACTION_CARD
FRONTIER-NOT_THIS_ONE_CARD
FRONTIER-OIL_COOLING_CARD
FRONTIER-PASSION_VERVE_CARD
FRONTIER-REFINING_CARD
FRONTIER-REUSE_AESTHETICS_CARD
FRONTIER-REVERSE_ENGINEERING_CARD
FRONTIER-SHARP_SMELTING_CARD
FRONTIER-SMELTER_CARD
FRONTIER-SMELTING_DESIGN_CARD
FRONTIER-SMELTING_STRIKE_CARD
FRONTIER-SPARK_BURST_CARD
FRONTIER-STEAM_RELEASE_CARD
FRONTIER-STRIKE_SHUMIT_CARD
FRONTIER-STURDY_ARMOR_CARD
FRONTIER-TAP_CARD
FRONTIER-TEMPORARY_QUENCHING_CARD
FRONTIER-THERMAL_COOLING_CARD
FRONTIER-THERMAL_ENERGY_CONVERSION_CARD
FRONTIER-TONGS_CARD
FRONTIER-UNBURNING_BODY_CARD
FRONTIER-UNSTOPPABLE_HEAT_CARD
FRONTIER-UNTIL_EXHAUSTION_CARD
FRONTIER-VENTILATION_CARD
FRONTIER-WATER_COOLING_CARD
```
