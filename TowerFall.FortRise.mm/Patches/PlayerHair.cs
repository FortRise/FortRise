using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_PlayerHair : PlayerHair
{
    public Color Color;
    public Color OutlineColor;
    public Vector2 Offset;
    public Vector2 DuckingOffset;

#nullable enable
    public Player? FollowPlayer => Follow as Player;
    public Facing Facing
    {
        get
        {
            if (Follow is Player player)
            {
                return player.Facing;
            }

            if (Follow is Enemy enemy)
            {
                return enemy.Facing;
            }

            return Facing.Right;
        }
    }
    public int CharacterIndex
    {
        get
        {
            if (FollowPlayer is not null)
            {
                return FollowPlayer.CharacterIndex;
            }

            if (Follow is Enemy enemy)
            {
                return enemy.ColorIndex;
            }

            return -1;
        }
    }
#nullable disable

    private int links;
    private Vector2[] offsets;
    private float scale;
    private Subtexture[] images;

    public patch_PlayerHair(Entity follow, Vector2 position, float scale) : base(follow, position, scale)
    {
    }

    [Prefix("System.Void .ctor(Monocle.Entity,Microsoft.Xna.Framework.Vector2,System.Single)")]
    public void ctor_Prefix()
    {
        Color = Color.White;
        OutlineColor = Color.Black;
    }

    [MonoModReplace]
    public override void Render()
    {
        Vector2 finalDuckingOffset = Vector2.Zero;

        if (Follow is Player player)
        {
            if (player.State == Player.PlayerStates.Ducking)
            {
                finalDuckingOffset = DuckingOffset;
            }
        }

        for (int i = 0; i < links; i++)
        {
            Vector2 offseted = new Vector2(
                Position.X + ((finalDuckingOffset.X + Offset.X) * (int)Facing),
                Position.Y + finalDuckingOffset.Y + Offset.Y
            );
            Vector2 position = Follow.Position + offseted + offsets[i];

            float rotation = (i == 0) ? 0f : Calc.Angle(offsets[i], offsets[i - 1]);

            Draw.TextureCentered(images[i], position, Color * Alpha * Alpha, scale, rotation);
        }
    }

    [MonoModReplace]
    public void RenderBlack()
    {
        Vector2 finalDuckingOffset = Vector2.Zero;

        if (Follow is Player player)
        {
            if (player.State == Player.PlayerStates.Ducking)
            {
                finalDuckingOffset = DuckingOffset;
            }
        }

        for (int i = 0; i < links; i++)
        {
            Vector2 offseted = new Vector2(
                Position.X + ((finalDuckingOffset.X + Offset.X) * (int)Facing),
                Position.Y + finalDuckingOffset.Y + Offset.Y
            );
            Vector2 position = Follow.Position + offseted + offsets[i];

            float rotation = (i == 0) ? 0f : Calc.Angle(offsets[i], offsets[i - 1]);

            Draw.TextureCentered(images[i], position, Color.Black * Alpha * Alpha, scale, rotation);
        }
    }

    [MonoModReplace]
    public void RenderOutline()
    {
        Vector2 finalDuckingOffset = Vector2.Zero;

        if (Follow is Player player)
        {
            if (player.State == Player.PlayerStates.Ducking)
            {
                finalDuckingOffset = DuckingOffset;
            }
        }

        for (int i = 0; i < links; i++)
        {
            Vector2 offseted = new Vector2(
                Position.X + ((finalDuckingOffset.X + Offset.X) * (int)Facing),
                Position.Y + finalDuckingOffset.Y + Offset.Y
            );
            Vector2 position = Follow.Position + offseted + offsets[i];

            float rotation = (i == 0) ? 0f : Calc.Angle(offsets[i], offsets[i - 1]);
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    if (j != 0 || k != 0)
                    {
                        Draw.TextureCentered(images[i], position+ new Vector2(j, k), OutlineColor, scale, rotation);
                    }
                }
            }
        }
    }
}