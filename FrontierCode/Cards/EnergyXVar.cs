using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TextEffects;

namespace Frontier.Cards;

/// <summary>X 코스트 카드 설명용. 손패 등에서 소모 에너지를 숫자로, 그 외에는 글자 X.</summary>
public sealed class EnergyXVar : DynamicVar
{
    public const string DefaultName = "X";

    private bool _previewResolved;

    public EnergyXVar(string name = DefaultName)
        : base(name, 0m)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        _previewResolved = false;
        if (!card.EnergyCost.CostsX)
        {
            PreviewValue = BaseValue;
            return;
        }

        if (runGlobalHooks && card.Owner?.PlayerCombatState != null)
        {
            PreviewValue = card.EnergyCost.GetAmountToSpend();
            _previewResolved = true;
            return;
        }

        PreviewValue = BaseValue;
    }

    /// <summary><c>{X:diff()}</c> 전용 — 코어 <see cref="DynamicVar.ToHighlightedString"/>는 non-virtual이라 패치에서 호출한다.</summary>
    public string FormatForDiffHighlight(bool inverse)
    {
        if (!_previewResolved)
        {
            return "X";
        }

        int value = (int)PreviewValue;
        int value2 = (int)EnchantedValue;
        return StsTextUtilities.HighlightChangeText(
            baseComparison: WasJustUpgraded ? 1 : value.CompareTo(value2),
            text: value.ToString(CultureInfo.InvariantCulture));
    }
}
