# Frontier 모드 — 카드 효과 정리

본 문서는 `Frontier/FrontierCode` 기준 **실제 C# 구현**을 우선으로 정리합니다.  
`sts2-frontier/localization/kor/cards.json`의 설명문과 다를 수 있으며, 그 경우 본문에 **불일치**로 표시합니다.  
갱신일: 저장소 기준 스냅샷.

---

## 0. 규칙·문서와의 정합

워크스페이스 규칙에 맞춰 작업할 때 아래를 교차 확인한다.

- **필수 핸드북:** `.cursor/rules/sts2-mod-authoring-handbook.mdc` — Soldoros 레퍼런스, `unpack/docs/*`, 본 문서 링크.
- **로캘:** `.cursor/rules/loc-key-coverage.mdc` — **`sts2-frontier/localization/{eng,kor}`** 를 Soldoros와 동일하게 원천으로 두고, 카드·유물·캐릭터·파워 등 해당 테이블 JSON을 동시에 갱신한다. **카드** 문구는 주로 `cards.json`(및 eng/kor 키 일치)이다.
- **Frontier 전용:** `.cursor/rules/frontier-character-mod.RULE.md` — ID·폴더·manifest.
- **카드 시스템 분석:** `unpack/docs/cards-system-analysis.md` — 신규 카드 훅·풀 등록.

---

## 1. 공통 시스템 (카드 외)

### 1.1 열기 (HeatPower)

| 조건 | 효과 |
|------|------|
| 플레이어 턴 종료 시 열기 ≥ 70 | 버린 카드 더미에 **화상(Burn)** 카드 1장 추가 |
| 플레이어 턴 종료 시 열기 ≥ 200 | 열기 100당 **신체 화상(BodyBurnPower)** 1 부여 |

정의: `MainFile.cs` (`HeatPower`).

### 1.2 신체 화상 (BodyBurnPower)

- 카드를 **사용할 때마다** 신체 화상 수치만큼 피해.
- 플레이어 턴 종료 시 **1 감소**.

정의: `MainFile.cs` (`BodyBurnPower`).

### 1.3 슈미트 전용 강화 상한 (Harmony)

`FrontierRules.IsShumit`인 카드에 대해 `MaxUpgradeLevel`을 조정합니다.

- **재련(Reforge)**: 카드 클래스별로 최대 강화 단계에 **가산 보너스** (일반 슈미트 카드는 별도 보너스 없음 → 사실상 상한 `999`에 가깝게).
- **걸작(Masterpiece)**: 지정 카드는 보너스만큼 최대 강화가 늘어나고, 특정 단계에서 **다른 카드로 변환**.

| 카드 클래스 | 재련 보너스 | 걸작 보너스 | 비고 |
|-------------|------------|-------------|------|
| `BellowsCard` | +1 | — | |
| `AnvilMemoryCard` | +1 | — | `ShumitCardPool.FilterThroughEpochs`로 **일반 획득 제외**; `AnvilEcho` 걸작 변환만 |
| `BurningStrikeCard` | +1 | — | |
| `SparkBurstCard` | +5 | — | |
| `SteamReleaseCard` | +10 | — | |
| `AnvilEchoCard` | — | +10 | 강화 후 **+10 도달 시** `AnvilMemoryCard`로 변환 |

정의: `MainFile.cs` (`FrontierRules`, `FrontierUpgradeCapPatch`, `FrontierMasterpieceTransformPatch`).

### 1.4 부서진 대장간 (BrokenForgeRelic) — 대장간 카드와 연동

`ForgeCard`는 **유물 로직**으로 효과가 붙습니다 (카드 `OnPlay` 없음).

| 시점 | 효과 |
|------|------|
| 전투 시작 | `ForgeCard`를 **설정된 매수**(기본 1)만큼 생성해 뽑을 더미에 섞어 넣음 |
| 드로우 | 손에 `ForgeCard`가 있으면 드로우 수 **−DrawPenalty**(기본 1, 최소 0) |
| 플레이어 턴 시작 | 손의 `ForgeCard`들 중 **최대 `UpgradesPerTurn` 값**만큼, 손에서 카드 선택해 강화 (`ForgeCard` 제외) |

동적 변수: `DrawPenalty`, `ForgeCount`(기본 각 1).

---

## 2. 구현됨 — 플레이 시 효과가 코드에 있음

| 한글명 (JSON) | 클래스 | 비용 | 타입 | 희귀 | 구현 효과 | 업그레이드 |
|----------------|--------|------|------|------|-----------|------------|
| 타격 | `StrikeShumitCard` | 1 | 공격 | 기본 | 단일 적에게 피해 **6** | 피해 **+3** → 9 |
| 수비 | `DefendShumitCard` | 1 | 스킬 | 기본 | 방어도 **5** | 방어도 **+3** → 8 |
| 대장간 | `ForgeCard` | 1 | 스킬 | 이벤트 | **보존**. 동적 변수 `UpgradesPerTurn` 기본 **1** (플레이 자체 효과 없음 — §1.4 참고) | `UpgradesPerTurn` **+1** |
| 단조 | `ForgingCard` | 2 | 공격 | 일반 | 피해 **8** → 열기 **+10** → 손패에서 **자기 제외·강화 가능**한 카드 1장 강화 | 피해 **+3** (열기·강화 횟수는 동일) |
| 유냉 | `OilCoolingCard` | 1 | 스킬 | 일반 | 방어도 **7** → 현재 열기와 **5** 중 작은 만큼 열기 감소 | 방어도 **+3**, 열기 감소량 **+5** → 최대 10 감소 |
| 내려찍기 | `HammerDownCard` | 1 | 공격 | 일반 | 단일 적에게 피해 **8** | 피해 **+3** |
| 두 번 두드리기 | `DoubleTapCard` | 1 | 공격 | 일반 | 같은 적에게 피해 **3**를 **2회** | 피해 **+1** (히트당) |
| 두드리기 | `TapCard` | 0 | 공격 | 일반 | 단일 적에게 피해 **4** | 피해 **+2** |

### 코드 vs PDF 주석 / JSON 불일치 (알려진 차이)

- **내려찍기**: 소스 주석·일부 기획 문구는 취약 부여를 가정할 수 있으나, **현재 구현은 피해만** 있습니다.
- **두 번 두드리기·두드리기**: 주석에는 열기 증가가 있으나 **구현에는 없음** (피해·다타만 해당).
- **냉각 타격** 등: JSON에는 `{Damage}`·열기 감소 문구가 있으나 **클래스에 `OnPlay` 없음** → §3 참고.

---

## 3. 미구현 / 스켈레톤 — 생성자·키워드만 있거나 `OnPlay` 없음

아래 카드들은 `ShumitCard` 등을 상속하지만 **`OnPlay`(또는 동등 플레이 로직)가 없어**, 전투에서 사용 시 **기본 카드 처리만** 되거나 효과가 없을 수 있습니다.  
일부는 `Retain` 등 키워드만 선언되어 있습니다.

- **토큰 베이스만**: `GreatForgeCard` (`TokenCardBase`, 보존) — 유물 `GreatForgeRelic`이 전투 시작 시 뽑을 더미에 추가. 카드 자체 플레이 로직 없음.
- **상태 카드**: `ColdBurnStatusCard` — 비용 −2, 상태, `TODO` 주석.
- **빈 셸**: `AbsoluteZeroCard` — 주석상 절대영도 효과·소멸 등, **구현 없음**.
- **Retain 등만**: 예) `AnvilMemoryCard`, `SmelterCard`, `HeatingCard`, `GreatForgeCard`, `GrindingRoomCard`, `BlastFurnaceCard` 등 다수.
- **나머지 대부분**: `CoolingStrikeCard`, `FlameStrikeCard`, `FlamePulseCard`, `HeatedShieldCard`, `MaterialGatherCard`, `SturdyArmorCard`, `AnvilEchoCard` … 파일당 생성자+풀 등록 수준.

`sts2-frontier/localization/kor/cards.json`에는 많은 카드에 **“슈미트.pdf 기준으로 순차 반영 중”** 같은 플레이스홀더 설명이 붙어 있으며, 이는 **기획 방향 참고용**으로 보는 것이 맞습니다.

---

## 4. 개발용 스켈레톤

| 클래스 | 비고 |
|--------|------|
| `SkeletonCard`, `SkeletonUncommonCard`, `SkeletonRareCard` | 인라인 `Localization` 플레이스홀더. 실제 풀 카드 아님. |

---

## 5. 유지보수 시 참고 경로

| 내용 | 경로 |
|------|------|
| 카드 클래스 | `Frontier/FrontierCode/Cards/*.cs` |
| 한글 이름·설명 | `Frontier/sts2-frontier/localization/kor/cards.json` |
| 열기·신체 화상·Harmony | `Frontier/FrontierCode/MainFile.cs` |
| 대장간 유물 | `Frontier/FrontierCode/Relics/BrokenForgeRelic.cs` |
| 기타 유물 | `Frontier/FrontierCode/Relics/NewRelics.cs` |

이 문서는 구현 변경 시 **수동으로** 함께 갱신하는 것을 권장합니다.
