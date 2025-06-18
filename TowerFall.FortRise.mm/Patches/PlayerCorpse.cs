using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_PlayerCorpse : PlayerCorpse
{
    private float drawOpacity;
    public int PlayerIndex
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        private set;
    }

    public int KillerIndex
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        private set;
    }

    public Facing Facing
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        private set;
    }

    public ArrowCushion ArrowCushion
    {
        [MonoModIgnore]
        get;
        [MonoModIgnore]
        private set;
    }

    private FireControl fire;
    private Counter dodgeTooLateCounter;
    private Counter squishedCounter;
    private Sprite<string> sprite;
    private Sprite<int> flashSprite;
    private Action<Platform> onCollideH;
    private Action<Platform> onCollideV;

    public patch_PlayerCorpse(EnemyCorpses enemyCorpse, Vector2 position, Facing facing, int killerIndex) : base(enemyCorpse, position, facing, killerIndex)
    {
    }

    [MonoModLinkTo("TowerFall.Actor", "System.Void .ctor(Microsoft.Xna.Framework.Vector2)")]
    public void base_ctor(Vector2 position) { }

    [MonoModConstructor]
    [MonoModReplace]
    private void ctor(string corpseSpriteID, Allegiance teamColor, Vector2 position, Facing facing, int playerIndex, int killerIndex)
    {
        drawOpacity = 1f;
        base_ctor(position);
        PlayerIndex = playerIndex;
        KillerIndex = killerIndex;
        Allegiance = Allegiance.Neutral;

        Facing = facing;
        Collider = new WrapHitbox(8f, 8f, -4f, 0f);
        Tag([GameTags.Corpse, GameTags.ExplosionCollider]);

        ScreenWrap = true;
        Depth = -50;
        Add(fire = new FireControl(true, Vector2.UnitY * 4f, new Vector2(4f, 6f), 60));
        dodgeTooLateCounter = new Counter(10);
        squishedCounter = new Counter();
        Add(ArrowCushion = new ArrowCushion(true, true));

        ref var corpseSprite = ref CollectionsMarshal.GetValueRefOrNullRef(TFGame.CorpseSpriteData.GetSprites(), corpseSpriteID);
        XmlElement xml;
        if (Unsafe.IsNullRef(ref corpseSprite))
        {
            xml = TFGame.SpriteData.GetXML(corpseSpriteID);
            sprite = TFGame.SpriteData.GetSpriteString(corpseSpriteID);
        }
        else
        {
            xml = corpseSprite;
            sprite = TFGame.CorpseSpriteData.GetSpriteString(corpseSpriteID);
        }

        if (teamColor == Allegiance.Red && xml.HasChild("RedTeam"))
        {
            sprite.SwapSubtexture(TFGame.Atlas[xml.ChildText("RedTeam")]);
        }
        else if (teamColor == Allegiance.Blue && xml.HasChild("BlueTeam"))
        {
            sprite.SwapSubtexture(TFGame.Atlas[xml.ChildText("BlueTeam")]);
        }

        sprite.Y = 8f;
        sprite.Play("ground");
        sprite.FlipX = Facing == Facing.Left;
        Add(sprite);
        if (xml.HasChild("Flash"))
        {
            flashSprite = new Sprite<int>(TFGame.Atlas[xml.ChildText("Flash")], (int)sprite.Width, (int)sprite.Height);
            flashSprite.Visible = false;
            Add(flashSprite);
        }
        onCollideH = CollideH;
        onCollideV = CollideV;
    }

    [MonoModIgnore]
    private extern void CollideH(Platform platform);

    [MonoModIgnore]
    private extern void CollideV(Platform platform);
}