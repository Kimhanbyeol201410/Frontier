using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace Frontier.Relics;

/// <summary>
/// 슈미트 유물 공통: 인벤토리 작은 아이콘은 본체 PNG + (있으면) <c>{엔트리}_outline.png</c> 레이어.
/// Soldoros 모드와 동일하게 <see cref="PackedIconOutlinePath"/> 를 PNG로 두면 <c>NRelic</c> 아웃라인 슬롯에 실린다.
/// </summary>
public abstract class FrontierRelicModel : CustomRelicModel
{
    protected FrontierRelicModel(bool autoAdd = true)
        : base(autoAdd)
    {
    }

    /// <summary>아틀라스 대신 모드 PCK의 <c>res://images/relics/{id}.png</c>.</summary>
    public override string PackedIconPath => RelicBodyPngPath;

    /// <summary>큰 아이콘(검사 화면 등) — 본체와 동일 파일 사용. 필요 시 <c>images/relics/big/</c> 분리 가능.</summary>
    protected override string BigIconPath => RelicBodyPngPath;

    /// <summary>
    /// <c>images/relics/{id}_outline.png</c> 가 있으면 사용. 없으면 본체 PNG를 재사용(윤곽 없이 표시).
    /// </summary>
    protected override string PackedIconOutlinePath => ResolveOutlinePngPath();

    private string RelicBodyPngPath => $"res://images/relics/{IconBaseName}.png";

    private string ResolveOutlinePngPath()
    {
        string outline = $"res://images/relics/{IconBaseName}_outline.png";
        if (ResourceLoader.Exists(outline))
        {
            return outline;
        }

        return RelicBodyPngPath;
    }
}
