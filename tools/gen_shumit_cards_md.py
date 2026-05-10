# -*- coding: utf-8 -*-
"""Generate 슈미트-카드-현행.md from kor/cards.json + Cards/*.cs"""
import json
import re
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
JSON_PATH = ROOT / "sts2-frontier/localization/kor/cards.json"
CSDIR = ROOT / "FrontierCode/Cards"
OUT = ROOT.parent / "슈미트-카드-현행.md"


def strip_gold(s: str) -> str:
    return re.sub(r"\[gold\]([^\[]*?)\[/gold\]", r"\1", s)


def entry_to_clsname(e: str) -> str:
    mid = e.replace("FRONTIER-", "").replace("_CARD", "")
    parts = mid.split("_")
    return "".join(x.capitalize() for x in parts) + "Card"


def main() -> None:
    data = json.loads(JSON_PATH.read_text(encoding="utf-8"))
    card_info: dict[str, tuple[int, str, str]] = {}
    energy_upgrade: set[str] = set()
    has_x: set[str] = set()

    for p in CSDIR.glob("*Card.cs"):
        if "Skeleton" in p.name:
            continue
        t = p.read_text(encoding="utf-8")
        m = re.search(
            r"public\s+sealed\s+class\s+(\w+)\s*:\s*(?:ShumitCard|TokenCardBase)", t
        )
        if not m:
            continue
        cls = m.group(1)
        mb = re.search(
            r":\s*base\(\s*(-?\d+)\s*,\s*CardType\.(\w+)(?:\s*,\s*CardRarity\.(\w+))?",
            t,
        )
        if mb:
            cost_s, ctype, crare = mb.group(1), mb.group(2), mb.group(3) or "Token"
            card_info[cls] = (int(cost_s), ctype, crare)
        else:
            mb2 = re.search(
                r":\s*base\(\s*(-?\d+)\s*,\s*CardType\.(\w+)\s*,\s*TargetType", t
            )
            if mb2:
                card_info[cls] = (int(mb2.group(1)), mb2.group(2), "Token")
        if re.search(r"EnergyCost\.UpgradeBy\(-1\)", t):
            energy_upgrade.add(cls)
        if re.search(r"HasEnergyCostX\s*=>\s*true", t) or re.search(
            r"protected\s+override\s+bool\s+HasEnergyCostX\s*=>\s*true", t
        ):
            has_x.add(cls)

    titles: dict[str, str] = {}
    descs: dict[str, str] = {}
    for k, v in data.items():
        if k.endswith(".title"):
            titles[k[: -len(".title")]] = v
        elif k.endswith(".description"):
            descs[k[: -len(".description")]] = v

    rare_order = ["Basic", "Common", "Uncommon", "Rare", "Event", "Token"]
    rare_title = {
        "Basic": "## 기본 · 시작 덱 구성",
        "Common": "## 일반",
        "Uncommon": "## 고급",
        "Rare": "## 희귀",
        "Event": "## 이벤트 · 상태",
        "Token": "## 토큰 · 시설 (보존)",
    }
    ctype_order = {"Attack": 0, "Skill": 1, "Power": 2, "Status": 3}

    rows: list[tuple] = []
    for eid in titles:
        cls = entry_to_clsname(eid)
        if cls not in card_info:
            continue
        cost, ctype, rare = card_info[cls]
        title = titles[eid]
        d = strip_gold(descs.get(eid, ""))
        eu = cls in energy_upgrade
        xb = cls in has_x
        rows.append((rare, ctype, cost, title, d, eid, eu, xb))

    lines: list[str] = []
    lines.append("# 슈미트 카드 목록 (현행)")
    lines.append("")
    lines.append(
        "Frontier 모드 기준입니다. 문구는 `sts2-frontier/localization/kor/cards.json`, "
        "비용·희귀도·타입은 `FrontierCode/Cards/*.cs`와 맞추었습니다."
    )
    lines.append("")
    lines.append("## 키워드(기믹)")
    lines.append("")
    lines.append("### 열기")
    lines.append("")
    lines.append(
        "> 열기가 70 이상이면 매 턴 종료 시 버린 카드 더미에 화상을 추가합니다. "
        "열기가 200 이상이면 매 턴 종료 시 신체 화상을 열기 100마다 1씩 얻습니다."
    )
    lines.append("")
    lines.append("### 신체 화상")
    lines.append("")
    lines.append(
        "> 카드 사용 시 신체 화상 수치만큼 피해를 받습니다. 턴 종료 시 1 감소합니다."
    )
    lines.append("")
    lines.append("### 재련")
    lines.append("")
    lines.append(
        "> 강화 횟수가 재련만큼 더 가능합니다. "
        "숫자 없이 재련만 적힌 카드는 강화 상한이 사실상 없습니다."
    )
    lines.append("")
    lines.append("### 걸작")
    lines.append("")
    lines.append(
        "> 걸작 수치만큼 더 강화할 수 있으며, 해당 횟수에 도달하면 카드가 다른 카드로 바뀝니다 "
        "(예: 모루의 잔향 → 모루의 기억)."
    )
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("### 시작 덱 구성 (전투 보상 풀 제외)")
    lines.append("")
    lines.append(
        "- 타격 ×4, 수비 ×4, 단조 ×1, 유냉 ×1 (총 10장). "
        "대장간은 시작 덱에 넣지 않으며, 유물 등으로 뽑을 더미에 추가됩니다."
    )
    lines.append("")

    for rare in rare_order:
        sub = [r for r in rows if r[0] == rare]
        if not sub:
            continue
        lines.append(rare_title[rare])
        lines.append("")
        sub.sort(key=lambda x: (ctype_order.get(x[1], 9), x[3]))
        for _, ctype, cost, title, d, _eid, eu, xb in sub:
            ctype_ko = {
                "Attack": "공격",
                "Skill": "스킬",
                "Power": "파워",
                "Status": "상태",
            }.get(ctype, ctype)
            if xb:
                cost_s = "X"
            else:
                cost_s = str(cost)
            if eu and not xb:
                up = cost - 1
                head = f"#### {title} ({cost_s}→{up}코 / {ctype_ko})"
            else:
                head = f"#### {title} ({cost_s}코 / {ctype_ko})"
            lines.append(head)
            lines.append("")
            for para in d.split("\n"):
                lines.append(f"> {para}")
            lines.append("")

    lines.append("---")
    lines.append("")
    lines.append("## 보상·상점 풀에서 빠지는 카드")
    lines.append("")
    lines.append("- 시작 덱 전용: 타격, 수비, 단조, 유냉")
    lines.append(
        "- 전투 종료 인챈트용(제련 계열): 기민함 제련, 메아리 제련, 숙련 제련, 예리한 제련"
    )
    lines.append("- 기타: 불사르지 않는 몸")
    lines.append(
        "- 별도 규칙: **모루의 기억**은 모루의 잔향이 걸작(+5)에 도달해 변환될 때만 획득합니다."
    )
    lines.append("")

    OUT.write_text("\n".join(lines), encoding="utf-8")
    print("Wrote", OUT, len(lines), "lines")


if __name__ == "__main__":
    main()
