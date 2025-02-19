using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_ArrowTypePickup : ArrowTypePickup
{
    [MonoModPublic]
    public GraphicsComponent graphic;
    public string Name;
    public Color Color;
    public Color ColorB;
    private ArrowTypes arrowType;
    public patch_ArrowTypePickup(Vector2 position, Vector2 targetPosition, ArrowTypes type) : base(position, targetPosition, type)
    {
    }

    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.Pickup", "System.Void .ctor(Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Vector2)")]
    public void base_ctor(Vector2 position, Vector2 targetPosition) {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Vector2 position, Vector2 targetPosition, ArrowTypes type)
    {
        base_ctor(position, targetPosition);
        arrowType = type;
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Tag(GameTags.PlayerCollectible);

        Color = Calc.HexToColor("F7EAC3");
        ColorB = Calc.HexToColor("FFFFFF");

        switch (arrowType)
        {
        case ArrowTypes.Normal:
            Name = "+2";

            graphic = new Image(TFGame.Atlas["pickups/arrowPickup"], null);
            graphic.CenterOrigin();
            Add(this.graphic);
            break;
        case ArrowTypes.Bomb:
        {
            Name = "BOMB";
            Color = Calc.HexToColor("F8B800");
            ColorB = Calc.HexToColor("F7D883");
            Sprite<int> sprite = new Sprite<int>(TFGame.Atlas["pickups/bombArrows"], 12, 12, 0);
            sprite.Add(0, 0.3f, new int[] { 0, 1 });
            sprite.Play(0, false);
            sprite.CenterOrigin();
            Add(sprite);
            graphic = sprite;
            break;
        }
        case ArrowTypes.SuperBomb:
        {
            Name = "SUPER BOMB";
            Color = Calc.HexToColor("F8B800");
            ColorB = Calc.HexToColor("F7D883");
            Sprite<int> sprite = new Sprite<int>(TFGame.Atlas["pickups/superBombArrows"], 12, 12, 0);
            sprite.Add(0, 0.3f, new int[] { 0, 1 });
            sprite.Play(0, false);
            sprite.CenterOrigin();
            Add(sprite);
            graphic = sprite;
            break;
        }
        case ArrowTypes.Laser:
        {
            Name = "LASER";
            Color = Calc.HexToColor("B8F818");
            ColorB = Calc.HexToColor("D0F76C");
            Sprite<int> sprite = new Sprite<int>(TFGame.Atlas["pickups/laserArrows"], 12, 12, 0);
            sprite.Add(0, 0.3f, new int[] { 0, 1 });
            sprite.Play(0, false);
            sprite.CenterOrigin();
            Add(sprite);
            graphic = sprite;
            break;
        }
        case ArrowTypes.Bramble:
            Name = "BRAMBLE";
            Color = Calc.HexToColor("F87858");
            ColorB = Calc.HexToColor("F7B09E");
            graphic = new Image(TFGame.Atlas["pickups/brambleArrows"], null);
            graphic.CenterOrigin();
            Add(this.graphic);
            break;
        case ArrowTypes.Drill:
            Name = "DRILL";
            Color = Calc.HexToColor("8EE8FF");
            ColorB = Calc.HexToColor("D8F7FF");
            graphic = new Image(TFGame.Atlas["pickups/drillArrows"], null);
            graphic.CenterOrigin();
            Add(this.graphic);
            break;
        case ArrowTypes.Bolt:
        {
            Name = "BOLT";
            Color = Calc.HexToColor("00FF4C");
            ColorB = Calc.HexToColor("00D33B");
            Sprite<int> sprite = new Sprite<int>(TFGame.Atlas["pickups/boltArrows"], 12, 12, 0);
            sprite.Add(0, 0.05f, new int[] { 0, 1, 2 });
            sprite.Play(0, false);
            sprite.CenterOrigin();
            Add(sprite);
            graphic = sprite;
            break;
        }
        case ArrowTypes.Feather:
            Name = "FEATHER";
            Color = Calc.HexToColor("BC70FF");
            ColorB = Calc.HexToColor("D5A5FF");
            graphic = new Image(TFGame.Atlas["pickups/featherArrows"], null);
            graphic.CenterOrigin();
            Add(this.graphic);
            break;
        case ArrowTypes.Trigger:
        {
            Name = "TRIGGER";
            Color = Calc.HexToColor("1BB7EE");
            ColorB = Calc.HexToColor("56D4FF");
            Sprite<int> sprite = TFGame.SpriteData.GetSpriteInt("TriggerArrowsPickup");
            sprite.Play(0, false);
            Add(sprite);
            graphic = sprite;
            break;
        }
        case ArrowTypes.Prism:
        {
            Name = "PRISM";
            Color = Calc.HexToColor("DB4ADB");
            ColorB = Calc.HexToColor("FF52FF");
            Sprite<int> sprite = TFGame.SpriteData.GetSpriteInt("PrismArrowsPickup");
            sprite.Play(0, false);
            Add(sprite);
            graphic = sprite;
            break;
        }
        }
    }

    [MonoModReplace]
    public override void OnPlayerCollide(Player player)
    {
        if (player.CollectArrows([ this.arrowType, this.arrowType ]))
        {
            base.DoCollectStats(player.PlayerIndex);
            RemoveSelf();

            Level.Add<FloatText>(new FloatText(this.Position + new Vector2(0f, -10f), Name.ToUpperInvariant(), Color, ColorB, 1f, 1f, false));
            Level.Add<FloatText>(new FloatText(this.Position + new Vector2(0f, -3f), "ARROWS", Color, ColorB, 1f, 1f, false));
            Level.Add<LightFade>(Cache.Create<LightFade>().Init(this, null));

            PlaySound();
        }
    }

    [MonoModReplace]
    public virtual void PlaySound()
    {
        switch (arrowType)
        {
        case ArrowTypes.Bomb:
            Sounds.pu_bombArrow.Play(X, 1f);
            return;
        case ArrowTypes.SuperBomb:
            Sounds.pu_superBomb.Play(X, 1f);
            return;
        case ArrowTypes.Laser:
            Sounds.pu_laserArrow.Play(X, 1f);
            return;
        case ArrowTypes.Bramble:
            Sounds.pu_brambleArrow.Play(X, 1f);
            return;
        case ArrowTypes.Drill:
            Sounds.pu_drill.Play(X, 1f);
            return;
        case ArrowTypes.Bolt:
            Sounds.pu_boltArrow.Play(X, 1f);
            return;
        case ArrowTypes.Feather:
            Sounds.pu_feather.Play(X, 1f);
            return;
        case ArrowTypes.Trigger:
            Sounds.pu_triggerArrow.Play(X, 1f);
            return;
        case ArrowTypes.Prism:
            Sounds.pu_prismArrow.Play(X, 1f);
            return;
        default:
            Sounds.pu_plus2Arrows.Play(X, 1f);
            return;
        }
    }

    protected void AddGraphic(GraphicsComponent component)
    {
        graphic = component;
        Add(component);
    }
}