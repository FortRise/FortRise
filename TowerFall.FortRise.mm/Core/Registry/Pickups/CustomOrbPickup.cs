using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;


public abstract class CustomOrbPickup : Pickup
{
    public Image Border;
    public Sprite<int> Sprite;


    public CustomOrbPickup(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
        Tag(GameTags.PlayerCollectible);

        Border = new Image(TFGame.Atlas["pickups/orbBorder"]);
        Border.CenterOrigin();
        Add(Border);
    }

    public abstract CustomOrbInfo CreateInfo();

    public override void Update()
    {
        base.Update();
        if (this.Collidable)
        {
            this.Sprite.Scale.X = 1f + 0.05f * this.sine.ValueOverTwo;
            this.Sprite.Scale.Y = 1f + 0.05f * this.sine.Value;
        }
        this.Sprite.Position = base.DrawOffset;
        this.Border.Scale = this.Sprite.Scale;
        this.Border.Position = this.Sprite.Position;
        if (base.Level.OnInterval(5))
        {
            this.Border.Visible = !this.Border.Visible;
        }
    }

    public override void Render()
    {
        base.DrawGlow();
        if (this.Border.Visible)
        {
            this.Border.DrawOutline(1);
        }
        else
        {
            this.Sprite.DrawOutline(1);
        }
        base.Render();
    }

    public abstract void Collect(Player player);

    public sealed override void OnPlayerCollide(Player player)
    {
        Level.Add<LightFade>(Cache.Create<LightFade>().Init(this, null));
        DoCollectStats(player.PlayerIndex);
        Collect(player);
        RemoveSelf();
    }

    public override void TweenUpdate(float t)
    {
        base.TweenUpdate(t);
        this.Border.Scale = (this.Sprite.Scale = Vector2.One * t);
    }
}

public struct CustomOrbInfo 
{
    public Hitbox Hitbox;
    public Color Color;
    public Sprite<int> Sprite;

    public CustomOrbInfo(Hitbox hitbox, Color lightColor, Sprite<int> sprite) 
    {
        Hitbox = hitbox ?? new Hitbox(16f, 16f, -8f, -8f);
        Color = lightColor;
        Sprite = sprite ?? TFGame.SpriteData.GetSpriteInt("DarkOrb");
    }
}