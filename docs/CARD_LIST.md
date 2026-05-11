# Frontier — 카드 목록

기준일: 저장소 기준 스냅샷. 데이터는 `sts2-frontier/localization/kor|eng/cards.json`, 카드 구현 `FrontierCode/Cards/*.cs`, 풀 정의 `FrontierCode/Character/ShumitCardPool.cs`, 풀 필터 `FrontierCode/MainFile.cs`의 `FrontierRules`를 따름.

**각 카드가 무엇을 하는지**는 아래 **[카드별 기능 요약](#카드별-기능-요약)** 표를 보면 됩니다. **비용·타입·방어·AOE 등 집계**는 **[통계 (코드 기준, 85장)](#통계-코드-기준-85장)** 을 참고하세요. 수치·조건은 게임 내 카드 설명(로컬 JSON)과 동적 변수가 기준이며, 코드와 로컬이 어긋난 경우는 코드·키워드(`Exhaust` 등)가 우선입니다.

## 요약

| 구분 | 개수 |
|------|------|
| 한·영 로컬라이즈가 있는 카드 (`*.title` 키 개수) | **85** |
| `ShumitCardPool.GenerateAllCards()`에 등록 | **79** |
| 풀에 없고 전용 경로로만 등장하는 카드 | **6** (`기민함/메아리/숙련/예리함 제련`, `불사르지 않는 몸`, `모루의 기억`) |
| 코드에만 있고 로컬 없음(개발용 스켈레톤) | `SkeletonCard`, `SkeletonUncommonCard`, `SkeletonRareCard` — 아래 표에서 제외 |

### 보상·상점 등에서의 동작 메모

- `ShumitCardPool.FilterThroughEpochs`로 **보상/상점 풀에서 제외**되는 엔트리: 시작 덱·인챈트 전용 등 (`FrontierRules.HiddenFromShumitCardPoolEntries`) + `모루의 기억` (걸작 변환 전용).
- **풀 미등록 카드** 6종은 전투 종료 인챈트, 걸작 변환 등으로만 덱에 들어갈 수 있음.
- `대장간` / `위대한 대장간` / `용광로` / `연마실` / `제련소` / `냉온화상` 등은 **Event** 또는 토큰 성격이 강하고, 일부는 카드 도서관에 안 보일 수 있음 (`showInCardLibrary: false` 등). **대장간·위대한 대장간·용광로·연마실·제련소**는 `MaxUpgradeLevel == 0`으로 **카드 강화 대상이 아님**(슈미트 재련 상한 패치와 무관하게 토큰 자체는 강화 불가).

---

## 통계 (코드 기준, 85장)

아래는 `FrontierCode/Cards`의 구체 카드 클래스 생성자 `base(에너지, CardType, …)`를 기준으로 한 집계입니다. **업그레이드로 비용이 바뀌는 카드**(예: 명인의 긍지)는 **기본(0강) 표시 비용**만 반영합니다. 스켈레톤·추상 베이스(`Skeleton*`, `ShumitCard`, `TokenCardBase` 자체)는 제외합니다.

### 타입 분포

| `CardType` | 장수 | 비고 |
|------------|------|------|
| Attack | **28** | 단일/다중 타격·피해 중심 |
| Skill | **40** | 방어·드로·소멸·열기 조절·토큰 생성 등 |
| Power | **16** | 지속 효과·전투 종료 인챈트 트리거 |
| Status | **1** | 냉온화상 |

### 표시 에너지 비용 분포

| 비용 | 장수 |
|------|------|
| **-2** | 1 |
| **0** | 13 |
| **1** | 44 |
| **2** | 20 |
| **3** | 6 |
| **합계** | **85** |

- **-2코**: 냉온화상(사용 불가·상태 카드).
- **0코 13장**: 목숨을 걸어, 용광로, 차가운 손짓, 대장간, 위대한 대장간, 연마실, 제련소, 두드리기, 임시 담금질, 집게질, 지쳐 쓰러질 때까지, 열에너지 전환, 환기 — 대부분 토큰·파워·X/특수 비용 구조와 겹침.
- **1코**: 44장(가장 많음). 전체 표에서 `희귀도`·`타입`이 Skill/Attack/Common 등으로 고르게 분포.

### 고코스트 (표시 **3** 에너지, 6장)

| 한글명 | 타입 | 비고 |
|--------|------|------|
| 단층 파괴 | Attack | 전체 공격(`AllEnemies`) |
| 배기 시스템 | Power | 열기 임계 시 감소·드로우 |
| 신의 형상 | Power | 내 턴 시작 시 손패 전체 강화(강화 가능한 카드만). **강화 시 비용 2** |
| 화염의 심장 | Power | 파워: 내 소유 화상이 전투 중 더미에 처음 들어올 때마다 에너지 1. **강화 시 비용 2** |
| 명인의 긍지 | Power | 강화 시 방어도 파워; **강화 시 비용 감소** 있음(코드 주석) |
| 증기 배출 | Power | 열기 감소 시마다 전체 피해. **재련** 상한 |

### 2코스트 (21장, “중간 비용”)

가나다순: 가열로 정비, 걸작을 위한 망치질, 금속 액화, 대장간 시설의 도면, 대장간의 도면, 단조, 뜨거운 노력, 리버스 엔지니어링, 메아리 제련, 멈출 수 없는 열기, 모루의 기억, 모루의 잔향, 불꽃 튀기기, 불사르지 않는 몸, 불순물 압착, 대장장이의 가호, 신의 형상(강화 시), 정련, 절대영도, 화염 강타, 화염의 갑옷

### X 비용 (추가 에너지, `HasEnergyCostX`)

- **지쳐 쓰러질 때 까지** — 기본 0코 + X.
- **열에너지 전환** — 기본 0코 + X.

### 조건부 비용

- **가열**: 플레이 시점에 열기가 0이면 **이번 사용 비용 0**(기본 생성자 비용은 1의 Attack).

### 전체 공격(카드 타겟이 `AllEnemies`, 7장)

멈출 수 없는 열기, 모루의 기억, 모루의 잔향, 불꽃 튀기기, 불순물 압착, 단층 파괴, 화염 파동

### 플레이 시 **방어도(Block)** 직접 부여 (`CreatureCmd.GainBlock`)

**14장**: 절대영도(제거한 열기만큼), 수비, 차가운 심장, 달궈진 방패, 단단한 갑옷, 불사르지 않는 몸, 불순물 제거, 유냉, 임시 담금질, 재료 수급, 수냉, 가열로 정비, 냉각, 화염의 갑옷(플레이 시 기본 방어도 + 이후 화상 연동은 파워 쪽)

- **유냉**은 `GainsBlock` 플래그는 없으나 동일하게 `GainBlock` 호출.

### 판금(Plating)만 부여 — 방어도와 별개

- **철갑옷** (토큰 스킬)

### 방어와 연관되나 “플레이 직후 블록 숫자”가 아닌 것

- **명인의 긍지** 파워: 이후 **카드 강화 시** 방어도.
- **화염의 갑옷** 파워: **화상 생성 시** 추가 방어도.

### 적의 방어도를 깎거나 제거하는 공격

- **파쇄의 망치질**: 피해 후 대상 방어 전부 제거.
- **단층 파괴**: 방어가 있었던 적에게 피해 2배 후 남은 방어 제거.

---

## 전체 표 (한글명 가나다순)

| 한글명 | 영문명 | 엔트리 ID | 타입 | 희귀도 | 풀 등록 |
|--------|--------|-----------|------|--------|---------|
| 절대영도 | Absolute Zero | `FRONTIER-ABSOLUTE_ZERO_CARD` | Skill | Uncommon | O |
| 기민함 제련 | Agile Smelting | `FRONTIER-AGILE_SMELTING_CARD` | Power | Uncommon | **풀 없음** |
| 모루의 잔향 | Anvil Echo | `FRONTIER-ANVIL_ECHO_CARD` | Attack | Uncommon | O |
| 모루의 기억 | Anvil Memory | `FRONTIER-ANVIL_MEMORY_CARD` | Attack | Rare | **풀 없음** (걸작 전용) |
| 다가가는 공포 | Approaching Dread | `FRONTIER-APPROACHING_DREAD_CARD` | Attack | Uncommon | O |
| 풀무질 | Bellows | `FRONTIER-BELLOWS_CARD` | Skill | Common | O |
| 목숨을 걸어 | Bet Your Life | `FRONTIER-BET_YOUR_LIFE_CARD` | Power | Rare | O |
| 용광로 | Blast Furnace | `FRONTIER-BLAST_FURNACE_CARD` | Skill | Event | O |
| 대장장이의 가호 | Blacksmith's Blessing | `FRONTIER-BLACKSMITHS_BLESSING_CARD` | Skill | Rare | O |
| 불태우는 일격 | Burning Strike | `FRONTIER-BURNING_STRIKE_CARD` | Attack | Rare | O |
| 냉온화상 | Cold Burn Status | `FRONTIER-COLD_BURN_STATUS_CARD` | Status | Event | O |
| 차가운 손짓 | Cold Gesture | `FRONTIER-COLD_GESTURE_CARD` | Attack | Common | O |
| 차가운 심장 | Cold Heart | `FRONTIER-COLD_HEART_CARD` | Skill | Uncommon | O |
| 머리 식히기 | Cool Head | `FRONTIER-COOL_HEAD_CARD` | Skill | Common | O |
| 냉각 타격 | Cooling Strike | `FRONTIER-COOLING_STRIKE_CARD` | Attack | Common | O |
| 냉각 시스템 | Cooling System | `FRONTIER-COOLING_SYSTEM_CARD` | Power | Uncommon | O |
| 냉각 | Thermal Cooling | `FRONTIER-THERMAL_COOLING_CARD` | Skill | Rare | O |
| 틈새 | Crevice | `FRONTIER-CREVICE_CARD` | Attack | Common | O |
| 파쇄의 망치질 | Crushing Hammer | `FRONTIER-CRUSHING_HAMMER_CARD` | Attack | Uncommon | O |
| 수비 | Defend | `FRONTIER-DEFEND_SHUMIT_CARD` | Skill | Basic | O (보상 풀 제외) |
| 설계의 완성 | Design Completion | `FRONTIER-DESIGN_COMPLETION_CARD` | Skill | Uncommon | O |
| 두 번 두드리기 | Double Tap | `FRONTIER-DOUBLE_TAP_CARD` | Attack | Common | O |
| 메아리 제련 | Echo Smelting | `FRONTIER-ECHO_SMELTING_CARD` | Power | Rare | **풀 없음** |
| 배기 시스템 | Exhaust System | `FRONTIER-EXHAUST_SYSTEM_CARD` | Power | Uncommon | O |
| 단층 파괴 | Fault Break | `FRONTIER-FAULT_BREAK_CARD` | Attack | Rare | O |
| 뜨거운 노력 | Hot Effort | `FRONTIER-FEARLESS_OF_FLAME_CARD` | Power | Uncommon | O |
| 화력발전기 | Fire Power Plant | `FRONTIER-FIRE_POWER_PLANT_CARD` | Skill | Uncommon | O |
| 화염의 갑옷 | Flame Armor | `FRONTIER-FLAME_ARMOR_CARD` | Power | Uncommon | O |
| 화염 파동 | Flame Pulse | `FRONTIER-FLAME_PULSE_CARD` | Attack | Common | O |
| 화염 강타 | Flame Smash | `FRONTIER-FLAME_SMASH_CARD` | Attack | Common | O |
| 화염 타격 | Flame Strike | `FRONTIER-FLAME_STRIKE_CARD` | Attack | Common | O |
| 접쇠 | Folded Steel | `FRONTIER-FOLDED_STEEL_CARD` | Skill | Uncommon | O |
| 대장간의 도면 | Forge Blueprint | `FRONTIER-FORGE_BLUEPRINT_CARD` | Power | Rare | O |
| 대장간 | Forge | `FRONTIER-FORGE_CARD` | Skill | Event | O |
| 대장간 시설의 도면 | Forge Facility Blueprint | `FRONTIER-FORGE_FACILITY_BLUEPRINT_CARD` | Power | Uncommon | O |
| 단조 | Forging | `FRONTIER-FORGING_CARD` | Attack | Basic | O (보상 풀 제외) |
| 연료 최대로 | Full Fuel | `FRONTIER-FULL_FUEL_CARD` | Skill | Common | O |
| 가열로 정비 | Furnace Maintenance | `FRONTIER-FURNACE_MAINTENANCE_CARD` | Skill | Common | O |
| 위대한 대장간 | Great Forge | `FRONTIER-GREAT_FORGE_CARD` | Skill | Event | O |
| 연마실 | Grinding Room | `FRONTIER-GRINDING_ROOM_CARD` | Skill | Event | O |
| 내려찍기 | Hammer Down | `FRONTIER-HAMMER_DOWN_CARD` | Attack | Common | O |
| 화염의 심장 | Heart of Flame | `FRONTIER-HEART_OF_FLAME_CARD` | Power | Uncommon | O |
| 환기 | Ventilation | `FRONTIER-VENTILATION_CARD` | Skill | Common | O |
| 열기 순환 | Heat Cycle | `FRONTIER-HEAT_CYCLE_CARD` | Skill | Common | O |
| 열 교환 | Heat Exchange | `FRONTIER-HEAT_EXCHANGE_CARD` | Attack | Uncommon | O |
| 열정! 패기! | Passion! Pluck! | `FRONTIER-PASSION_VERVE_CARD` | Attack | Uncommon | O |
| 뜨거워진 대장간 | Heated Forge | `FRONTIER-HEATED_FORGE_CARD` | Power | Uncommon | O |
| 가열된 망치 | Heated Hammer | `FRONTIER-HEATED_HAMMER_CARD` | Attack | Common | O |
| 달궈진 방패 | Heated Shield | `FRONTIER-HEATED_SHIELD_CARD` | Skill | Common | O |
| 가열 | Heating | `FRONTIER-HEATING_CARD` | Attack | Uncommon | O |
| 불순물 압착 | Impurity Compression | `FRONTIER-IMPURITY_COMPRESSION_CARD` | Attack | Rare | O |
| 불순물 제거 | Impurity Removal | `FRONTIER-IMPURITY_REMOVAL_CARD` | Skill | Common | O |
| 철갑옷 | Iron Armor | `FRONTIER-IRON_ARMOR_CARD` | Skill | Common | O |
| 철방패 | Iron Shield | `FRONTIER-IRON_SHIELD_CARD` | Skill | Common | O |
| 철검 | Iron Sword | `FRONTIER-IRON_SWORD_CARD` | Skill | Common | O |
| 제조 | Manufacture | `FRONTIER-MANUFACTURE_CARD` | Skill | Uncommon | O |
| 걸작을 위한 망치질 | Masterpiece Hammer | `FRONTIER-MASTERPIECE_HAMMER_CARD` | Attack | Uncommon | O |
| 명인의 긍지 | Master Pride | `FRONTIER-MASTER_PRIDE_CARD` | Power | Uncommon | O |
| 숙련 제련 | Mastery Smelting | `FRONTIER-MASTERY_SMELTING_CARD` | Power | Rare | **풀 없음** |
| 재료 수급 | Material Gather | `FRONTIER-MATERIAL_GATHER_CARD` | Skill | Common | O |
| 융해 | Melt | `FRONTIER-MELT_CARD` | Skill | Uncommon | O |
| 녹이기 | Melting | `FRONTIER-MELTING_CARD` | Skill | Uncommon | O |
| 금속 액화 | Metal Liquefaction | `FRONTIER-METAL_LIQUEFACTION_CARD` | Skill | Uncommon | O |
| 리버스 엔지니어링 | Reverse Engineering | `FRONTIER-REVERSE_ENGINEERING_CARD` | Skill | Rare | O |
| 이게 아니야! | Not This One! | `FRONTIER-NOT_THIS_ONE_CARD` | Attack | Rare | O |
| 유냉 | Oil Cooling | `FRONTIER-OIL_COOLING_CARD` | Skill | Basic | O (보상 풀 제외) |
| 정련 | Refining | `FRONTIER-REFINING_CARD` | Attack | Rare | O |
| 재사용의 미학 | Reuse Aesthetics | `FRONTIER-REUSE_AESTHETICS_CARD` | Skill | Uncommon | O |
| 예리한 제련 | Sharp Smelting | `FRONTIER-SHARP_SMELTING_CARD` | Power | Uncommon | **풀 없음** |
| 제련소 | Smelter | `FRONTIER-SMELTER_CARD` | Skill | Event | O |
| 제련 설계 | Smelting Design | `FRONTIER-SMELTING_DESIGN_CARD` | Skill | Common | O |
| 제련 타격 | Smelting Strike | `FRONTIER-SMELTING_STRIKE_CARD` | Attack | Common | O |
| 불꽃 튀기기 | Spark Burst | `FRONTIER-SPARK_BURST_CARD` | Attack | Uncommon | O |
| 신의 형상 | Divine Form | `FRONTIER-DIVINE_FORM_CARD` | Power | Rare | O |
| 증기 배출 | Steam Release | `FRONTIER-STEAM_RELEASE_CARD` | Power | Uncommon | O |
| 타격 | Strike | `FRONTIER-STRIKE_SHUMIT_CARD` | Attack | Basic | O (보상 풀 제외) |
| 단단한 갑옷 | Sturdy Armor | `FRONTIER-STURDY_ARMOR_CARD` | Skill | Common | O |
| 두드리기 | Tap | `FRONTIER-TAP_CARD` | Attack | Common | O |
| 임시 담금질 | Temporary Quenching | `FRONTIER-TEMPORARY_QUENCHING_CARD` | Skill | Common | O |
| 열에너지 전환 | Thermal Energy Conversion | `FRONTIER-THERMAL_ENERGY_CONVERSION_CARD` | Skill | Rare | O |
| 집게질 | Tongs | `FRONTIER-TONGS_CARD` | Skill | Common | O |
| 불사르지 않는 몸 | Unburning Body | `FRONTIER-UNBURNING_BODY_CARD` | Skill | Common | **풀 없음** |
| 멈출 수 없는 열기 | Unstoppable Heat | `FRONTIER-UNSTOPPABLE_HEAT_CARD` | Attack | Uncommon | O |
| 지쳐 쓰러질 때 까지 | Until Exhaustion | `FRONTIER-UNTIL_EXHAUSTION_CARD` | Attack | Rare | O |
| 수냉 | Water Cooling | `FRONTIER-WATER_COOLING_CARD` | Skill | Common | O |

---

## 카드별 기능 요약

게임 내 문구는 `sts2-frontier/localization/kor/cards.json`의 `description`과 동일한 의미이며, 아래는 동적 수치(`{…}`)는 생략하고 한 줄로 요약한 것입니다.

| 한글명 | 기능 요약 |
|--------|-----------|
| 절대영도 | 열기를 모두 제거하고, 제거한 만큼 방어도를 얻음. 소멸. |
| 기민함 제련 | 전투 종료 후 덱의 일정 장수에 **기민함 인챈트** 부여. |
| 모루의 잔향 | 모든 적에게 피해를 여러 번 가함. 강화 +5 시 **모루의 기억**으로 변환. |
| 모루의 기억 | 모든 적에게 피해를 여러 번 가함. 강화할 때마다 피해 증가. 모루의 잔향 걸작(+5)으로만 획득. |
| 다가가는 공포 | 피해. 이번 턴 열기 구간당 힘 획득, 턴 종료 시 해당 힘 상실. |
| 풀무질 | 열기 획득, 카드 1장 드로우. **재련**(무제한 강화 상한). |
| 목숨을 걸어 | 전투 중 파워: 열기·신체 화상이 더 이상 감소하지 않음. 덱·손패·버림·소멸 전 카드 강화. 사용 시 열기 200 획득. 스킬 사용 시 힘, 공격 사용 시 민첩 획득. |
| 용광로 | 매 턴 손패에 있으면 열기 획득, 손패 카드 일정 장 **소멸**(보존 토큰). |
| 대장장이의 가호 | 열기 구간당 힘·민첩 1씩 획득 후 열기 2배. |
| 불태우는 일격 | 피해 + 열기 획득. **재련** 무제한 강화. |
| 냉온화상 | 사용 불가. 내 턴 종료 시 손패에 있으면 체력 손실. |
| 차가운 손짓 | 피해, 열기 감소. |
| 차가운 심장 | 방어도, 열기 감소, 카드 1장 드로우. |
| 머리 식히기 | 즉시 열기 감소, 다음 내 턴 시작 시 카드 1장 드로우·열기 감소(파워). |
| 냉각 타격 | 피해, 열기 감소. |
| 냉각 시스템 | 파워: 매 턴 종료 시 열기 감소. |
| 냉각 | 열화(손패 강화 1단계 제거) 후 방어도·열기 감소. |
| 틈새 | 피해 + 약화 부여(기본 1, 강화 시 2). |
| 파쇄의 망치질 | 피해, 대상 방어도 전부 제거, 열기 획득. |
| 수비 | 방어도. |
| 설계의 완성 | 뽑을 더미에서 카드 선택 → 강화 → 손패로. 소멸. |
| 두 번 두드리기 | 피해를 여러 히트로, 열기 획득. |
| 메아리 제련 | 전투 종료 후 덱 일부에 **메아리 인챈트** 부여. |
| 배기 시스템 | 파워: 턴 시작 시 열기가 임계 이상이면 열기 10 감소 후 카드 1장 드로우. |
| 단층 파괴 | 전체 피해. 대상이 방어를 가졌으면 피해 2배 후 남은 방어 제거. |
| 뜨거운 노력 | 파워: 턴 시작마다 열기·힘 획득. |
| 화력발전기 | 손패 1장 소멸. 화상이면 에너지 보상. |
| 화염의 갑옷 | 방어도. 화상 생성 시마다 추가 방어도. |
| 화염 파동 | 전체 피해 + 열기. |
| 화염 강타 | 피해 + 열기. |
| 화염 타격 | 피해 + 열기. |
| 접쇠 | 열기 임계 이상일 때만 사용. 이번 턴 강화된 카드 일정 개수까지 효과 추가 발동. |
| 대장간의 도면 | 언제든 사용. 누적 강화 5회 도달 시(또는 이미 5회 이상이면 즉시) **대장간** 또는 **용광로** 선택해 손패에 추가. |
| 대장간 | 매 턴 손패에 있으면 열기 획득, 손패 무작위 1장을 전투 중 여러 번 강화(보존 토큰). |
| 대장간 시설의 도면 | 언제든 사용. 누적 강화 5회 도달 시(또는 이미 5회 이상이면 즉시) **연마실** 또는 **제련소** 선택해 손패에 추가. |
| 단조 | 피해 5(+열기 10당 1), 열기 획득, 손패 카드 1장을 전투 동안 강화. 강화 시 열기 10당 피해 +2 · 손패 카드 2장 선택 강화. |
| 연료 최대로 | 열기 2배, 다음 턴 시작 시 에너지(파워), 소멸. |
| 가열로 정비 | 방어도, 열기. 이번 턴 화상이 소멸했으면 에너지 보너스. 소멸. |
| 위대한 대장간 | 매 턴 손패에 있으면 손패 무작위 1장 강화(대장간·위대한 대장간 제외). |
| 연마실 | 매 턴 손패에 있으면 드로우·힘·민첩, 당일 에너지 획득 감소. |
| 내려찍기 | 피해 + 취약 부여. |
| 화염의 심장 | 파워: 내 소유 [화상](Burn)이 전투 중 더미에 처음 들어올 때마다 에너지 1(화염의 갑옷과 동일한 `PileType.None` 진입 조건). 강화 시 비용 −1. |
| 열기 순환 | 카드 드로우. 열기 70 미만이면 열기 획득, 이상이면 열기 감소. |
| 열 교환 | 피해. 손패 1장을 뽑을 더미 맨 위로 옮기고 강화, 열기 감소. |
| 열정! 패기! | 피해 + 취약 부여(기본 2, 강화 시 3). |
| 뜨거워진 대장간 | 파워: 매 턴 종료 시 열기 획득. |
| 가열된 망치 | 피해 + 열기 10당 추가 피해. |
| 달궈진 방패 | 방어도 + 열기. |
| 가열 | 피해. 열기 0이면 비용 0·추가 드로우·열기 획득. |
| 불순물 압착 | 뽑을 더미의 화상 전부 소멸, 소멸 수만큼 보너스 피해로 전체 공격, 열기. |
| 불순물 제거 | 방어도. 버림/손패에서 화상 일정 장 소멸. |
| 철갑옷 | 판금 획득. |
| 철방패 | 민첩 획득. |
| 철검 | 힘 획득. |
| 제조 | 손패에 대장간이 있을 때만. 철검·철갑옷·철방패 중 무작위 생성. |
| 걸작을 위한 망치질 | 피해 + 열기. |
| 명인의 긍지 | 파워: 카드 강화할 때마다 방어도. |
| 숙련 제련 | 전투 종료 후 덱 일부에 **숙련 인챈트** 부여. |
| 재료 수급 | 방어도 + 드로우(강화 시 2장). |
| 융해 | 손패 1장 소멸, 열기 20. 손패에 대장간 또는 용광로 필요. 이 카드도 소멸. |
| 녹이기 | 손패 1장 소멸, 카드 2장 뽑기(강화 시 3장). 손패에 대장간 또는 용광로 필요. |
| 금속 액화 | 강화된 공격 1장 선택 → 당일 비용 0. 강화 공격 사용 시마다 열기. 뽑을 더미에 화상 섞기. |
| 리버스 엔지니어링 | 손패 1장 소멸 → 그 카드 표시 비용만큼 에너지, 피해 수치만큼 이번 턴 힘, 방어 수치만큼 이번 턴 민첩, 이번 턴 열기 증감 반전(`PowerCmd.Apply<HeatPower>` 부호 반전 패치). **강화 시 비용 1**. |
| 이게 아니야! | 손패 1장 소멸, 피해, 에너지. |
| 유냉 | 방어도, 열기 감소. |
| 환기 | 이번 턴 카드 사용마다 열기 감소(파워, 턴 종료 제거). |
| 정련 | 피해. 처치 시 이 카드 **영구 강화**. **재련** 무제한. |
| 재사용의 미학 | 소멸 더미에서 1장 손패로. 소멸. |
| 예리한 제련 | 전투 종료 후 덱 일부에 **예리함 인챈트** 부여. |
| 제련소 | 매 턴 손패에 있으면 열기 15 감소 후 손패 1장 전투 중 강화. |
| 제련 설계 | 드로우. 이번 턴 다음 공격에 열기 부여 효과. |
| 제련 타격 | 피해, 손패 무작위 강화, 열기. |
| 불꽃 튀기기 | 전체 피해 + 열기. **재련** 상한 표기. |
| 신의 형상 | 파워: 내 턴 시작마다 손패의 강화 가능한 모든 카드 1회 강화. **강화 시 비용 −1**. |
| 증기 배출 | 파워: 열기가 줄어들 때마다 전체 피해(기본 4, 강화당 +3). **재련** 상한. |
| 타격 | 피해. |
| 단단한 갑옷 | 방어도. |
| 두드리기 | 피해 + 열기. |
| 임시 담금질 | 방어도, 열기 감소. 소멸. |
| 열에너지 전환 | X 비용. 보유 열기 구간당 에너지 획득 → 모든 열기 제거 → 카드 X장 드로우. 소멸. |
| 집게질 | 손패 1장 강화 + 열기. |
| 불사르지 않는 몸 | 방어도 11(강화 시 14), 버림 더미에 화상 1장. |
| 멈출 수 없는 열기 | 전체 피해 + 열기 구간당 피해 증가. |
| 지쳐 쓰러질 때 까지 | X회 무작위 적 피해, 손패 무작위 강화, 열기, 턴 종료. |
| 수냉 | 방어도, 열기 감소. |

---

## 희귀도 요약

| 희귀도 | 해당 카드(한글명) |
|--------|---------------------|
| Basic | 타격, 수비, 단조, 유냉 |
| Event | 냉온화상, 대장간, 위대한 대장간, 용광로, 연마실, 제련소 |
| Common | 위 표에서 `Common` |
| Uncommon | 위 표에서 `Uncommon` |
| Rare | 위 표에서 `Rare` |

---

## 구현 상태 메모 (로컬 설명 기준)

- **화염의 심장**: `ShumitHeartOfFlameEnergyPower` — 내 소유 화상이 `oldPileType == None`으로 더미에 들어올 때마다 `GainEnergy(1)` (전투당 카드 인스턴스당 첫 진입 1회).
- **리버스 엔지니어링**: 소멸 대상의 `EnergyCost.GetAmountToSpend()`·동적 변수 `Damage`/`Block` 기준 수치 부여; `ShumitReverseEngineeringInvertHeatPower` + `FrontierHeatPowerApplyAmountInvertPatch`로 이번 턴 `HeatPower` 적용량 부호 반전(다른 코드 경로의 열기 변화는 게임 API에 따라 다를 수 있음).
- **신의 형상**: `ShumitDivineFormPower` — `AfterPlayerTurnStart`에서 손패 `IsUpgradable` 카드만 모아 `CardCmd.Upgrade` 일괄 호출(대장간 토큰 등 `MaxUpgradeLevel == 0`은 제외).
- **머리 식히기**: `CoolHeadCard` — 플레이 시 `FrontierHeatUtil.ReduceHeat`; `ShumitCoolHeadNextTurnPower`(`Amount` = 다음 턴 열기 감소량)로 다음 내 턴 시작 시 `CardPileCmd.Draw(1)` 후 `ReduceHeat(Amount)`·파워 제거.
- **환기**: `VentilationCard` — `ShumitVentilationThisTurnPower`(`IsInstanced`, `Amount` = 카드당 열기 감소량); `AfterCardPlayed`에서 `ReduceHeat`, 플레이어 턴 종료에 `PowerCmd.Remove`.
- **냉각**: `ThermalCoolingCard` — 손패 `CurrentUpgradeLevel > 0` 카드 선택 후 `FrontierThermalDegradationUtil`이 `CardCmd.Downgrade`(리플렉션)로 열화 1회; 이어서 `GainBlock`, `ReduceHeat`.
- **연료 최대로**: `FullFuelCard` — 현재 열만큼 `ApplyHeat`(2배), `ShumitFuelMaxNextTurnEnergyPower`(`Amount` = 다음 턴 에너지), `CardCmd.Exhaust`.

---

## 엔트리 ID 빠른 목록 (정렬)

```
FRONTIER-ABSOLUTE_ZERO_CARD
FRONTIER-AGILE_SMELTING_CARD
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
FRONTIER-FORGE_BLUEPRINT_CARD
FRONTIER-FORGE_CARD
FRONTIER-FORGE_FACILITY_BLUEPRINT_CARD
FRONTIER-FORGING_CARD
FRONTIER-FULL_FUEL_CARD
FRONTIER-FURNACE_MAINTENANCE_CARD
FRONTIER-GREAT_FORGE_CARD
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
FRONTIER-REVERSE_ENGINEERING_CARD
FRONTIER-REUSE_AESTHETICS_CARD
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
