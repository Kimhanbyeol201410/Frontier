# Frontier

`Slay the Spire 2`에 새로운 캐릭터 **슈미트(Shumit)** 를 추가하는 모드입니다.

---

## 소개

> *이세계로 끌려온 개척자, 돌아갈 길을 벼려내는 대장장이.*

슈미트는 열기를 다루며 카드를 반복 강화해 전투를 풀어가는 캐릭터입니다.  
강화 제한을 확장하는 **재련**, 특정 강화 구간에서 카드를 변화시키는 **걸작**을 중심으로 성장합니다.

- **시작 체력**: 72
- **시작 덱**: 타격 ×4, 수비 ×4, 단조, 유냉
- **시작 유물**: 부서진 대장간

---

## 핵심 메커니즘

### 열기 (Heat)
열기는 슈미트의 핵심 리소스입니다.  
일정 수치 이상이 되면 화상/신체 화상과 연계되어 전투 리스크와 보상을 동시에 만듭니다.

### 신체 화상 (Body Burn)
카드 사용 시 누적 수치만큼 피해를 받는 디버프입니다.  
턴 종료 시 감소하며, 열기 운용이 과열될수록 관리가 중요해집니다.

### 재련 (Reforge)
카드의 추가 강화 한도를 늘립니다.  
강화 중심 빌드에서 핵심 성장 축으로 작동합니다.

### 걸작 (Masterpiece)
특정 카드가 걸작 강화치에 도달하면 다른 카드로 변환됩니다.  
현재는 `모루의 잔향 -> 모루의 기억` 변환이 구현되어 있습니다.

---

## 설치 방법

1. `Slay the Spire 2` 로컬 폴더를 열고, `mods/` 폴더가 없으면 생성합니다.
2. **(필수)** [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases) 를 다운로드하여 `mods/` 폴더에 압축 해제합니다.
3. `Frontier` 모드를 `mods/` 폴더에 넣습니다.
4. 게임 실행 후 모드 목록에서 **Frontier**를 활성화합니다.

```text
Slay the Spire 2/
└── mods/
    ├── BaseLib/      ← 필수 선행 모드
    └── Frontier/     ← Frontier 모드
```

---

## 지원 언어

- 한국어 (Korean)
- 영어 (English)

---

## 개발 현황

현재 버전은 카드/유물/시스템 골격이 우선 구현된 상태입니다.  
향후 카드별 세부 효과, 열기 상호작용, 인챈트/조건부 효과, 밸런스 조정이 순차 적용될 예정입니다.

---

## 모드 정보

- Mod ID: `sts2-frontier`
- 게임 버전: 기본 공개 버전
- 이름: `Frontier`
- 의존성: `BaseLib`
- 게임플레이 영향: `true`

---

## 개발 가이드 (워크스페이스 규칙)

새 카드·유물·로캘·PCK를 손볼 때 아래를 함께 본다.

| 구분 | 경로 |
|------|------|
| 세션 공통 핸드북 | `.cursor/rules/sts2-mod-authoring-handbook.mdc` |
| 로캘·PCK (`STS2-The-First-Blade-Master-Soldoros-MOD` 과 동일 원칙) | `.cursor/rules/loc-key-coverage.mdc` |
| Frontier 전용 규약 | `.cursor/rules/frontier-character-mod.RULE.md` |
| 카드 효과 메모(구현 기준) | `Frontier/docs/card-effects.md` |
| 코드 레퍼런스 모드 | `STS2-The-First-Blade-Master-Soldoros-MOD/` |

**로캘:** Soldoros와 같이 **`sts2-frontier/localization/eng`·`kor` JSON**을 진실의 원천으로 두고, 같은 변경에서 양쪽 언어 키를 맞춘다. 릴리즈는 `has_pck: true` 로 PCK를 함께 배포하는 것을 전제로 한다.

**빌드:** 리포 루트 또는 `Frontier/` 에서 `dotnet build Frontier/Frontier.csproj` 로 확인한다.

