using Godot;
using Frontier.Resources;

namespace Frontier.Nodes;

/// <summary>
/// 슈미트 캐릭터 선택 배경. PCK에 임포트된 <c>.ctex</c> 없이도 raw PNG를 <see cref="FrontierResPngTexture"/> 로 읽는다.
/// </summary>
public partial class NShumitCharacterSelectBg : Control
{
    public const string BgTexturePath = "res://sts2-frontier/images/charui/bg.png";

    public override void _Ready()
    {
        TextureRect? background = GetNodeOrNull<TextureRect>("Background");
        if (background == null)
        {
            return;
        }

        Texture2D? texture = LoadBgTexture();
        if (texture == null)
        {
            GD.PushError($"[Frontier] Failed to load character select background: {BgTexturePath}");
            return;
        }

        background.Texture = texture;
    }

    internal static Texture2D? LoadBgTexture()
    {
        if (ResourceLoader.Exists(BgTexturePath))
        {
            Texture2D? imported = ResourceLoader.Load<Texture2D>(BgTexturePath);
            if (imported != null)
            {
                return imported;
            }
        }

        return FrontierResPngTexture.TryLoadTexture2DFromRes(BgTexturePath);
    }
}
