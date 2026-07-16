using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace TowerFall.Editor;

public class patch_ActorData : ActorData 
{
    public extern static void orig_Init();
    public Dictionary<string, string> Attributes;
    public Dictionary<string, string[]> AttributeSchemas;
    public static Dictionary<string, patch_ActorData> Data;
    public static List<patch_ActorData> ActorDatas;

    public Func<Actor, Vector2, bool> Validate;


    public static void Init() 
    {
        if (Data != null)
        {
            return;
        }

        ActorDatas = [];

        orig_Init();
        PatchVanillaActor();

        AddActorDataModded(new() 
        {
            Name = "Dummy",
            Title = "DUMMY",
            Subtexture = TFGame.Atlas["dummy/dummy"],
            TextureSizeRect = new Vector2(12, 20),
            Width = 12,
            Height = 20,
            AllowScreenWrap = true,
            Origin = new Vector2(6, -10),
            Attributes = new() 
            {
                { "Facing", "Left" }
            },
            AttributeSchemas = new() 
            {
                { "Facing", ["Left", "Right"]}
            }
        });

        AddActorDataModded(new() 
        {
            Name = "Orb",
            Title = "ORB ALT",
            Subtexture = TFGame.EditorAtlas["actors/orb"],
            Width = 14,
            Height = 14,
            AllowScreenWrap = true,
            Origin = Vector2.Zero,
            Renderer = (actor, at, alpha) => 
            {
                Draw.TextureCentered(TFGame.Atlas["details/orbHolder"], at + new Vector2(0, 12), Color.White * alpha);
                Draw.TextureCentered(actor.Data.Subtexture, at, Color.White * alpha);
            }
        });

        AddActorDataModded(new() 
        {
            Name = "FloorMiasma",
            Title = "FLOOR MIASMA",
            Subtexture = TFGame.Atlas["quest/floorMiasma"],
            Width = 10,
            Height = 10,
            AllowScreenWrap = true,
            TextureSizeRect = new Vector2(10, 10),
            ResizeableX = true,
            Attributes = new() {
                { "Group", "0" }
            },
            Renderer = (actor, at, alpha) => 
            {
                Subtexture subtexture = TFGame.Atlas["quest/floorMiasma"];
                Vector2 pos = at + new Vector2(-actor.Width / 2, -actor.Height / 2);
                Draw.Texture(subtexture, new Rectangle(0, 0, 10, 10), pos, Color.White * alpha);
                Draw.Texture(subtexture, new Rectangle(20, 0, 10, 10), pos + Vector2.UnitX * (actor.Width - 10), Color.White * alpha);

                for (int i = 0; i < actor.Width / 10 - 2; i++)
                {
                    Draw.Texture(subtexture, new Rectangle(10, 0, 10, 10), pos + Vector2.UnitX * (10 + i * 10), Color.White * alpha);
                }
            }
        });

        AddActorDataModded(new() 
        {
            Name = "Cobwebs",
            Title = "COBWEB",
            Subtexture = TFGame.Atlas["details/cobwebs"],
            TextureSizeRect = new Vector2(20, 20),
            Width = 20,
            Height = 20,
            AllowScreenWrap = true,
            Origin = new Vector2(0, 0),
            Validate = (actor, at) => 
            {
                bool west = actor.Level.SolidsLayer.Check(at + new Vector2(-11f, 0f));
                bool east = actor.Level.SolidsLayer.Check(at + new Vector2(11f, 0f));
                bool north = actor.Level.SolidsLayer.Check(at + new Vector2(0f, -11f));
                bool south = actor.Level.SolidsLayer.Check(at + new Vector2(0f, 11f));

                if (north && west) { return true; }
                if (north && east) { return true; }
                if (south && west) { return true; }
                if (south && east) { return true; }

                return false;
            },
            Renderer = (actor, at, alpha) => 
            {
                bool west = actor.Level.SolidsLayer.Check(at + new Vector2(-11f, 0f));
                bool east = actor.Level.SolidsLayer.Check(at + new Vector2(11f, 0f));
                bool north = actor.Level.SolidsLayer.Check(at + new Vector2(0f, -11f));
                bool south = actor.Level.SolidsLayer.Check(at + new Vector2(0f, 11f));

                SpriteEffects spriteEffects;
                Subtexture subtexture = TFGame.Atlas["details/cobwebs"];
                Rectangle rectangle = new Rectangle(
                    subtexture.X, subtexture.Y, 20, 20
                );

                int invalidY = rectangle.Y + 40;

                Vector2 newAt = at;

                if (north && west)
                {
                    spriteEffects = SpriteEffects.None;
                }
                else if (north && east)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }

                else if (south && west)
                {
                    rectangle.Y += 20;
                    spriteEffects = SpriteEffects.None;
                }
                else 
                {
                    if (!south || !east)
                    {
                        rectangle.Y += 40;
                        spriteEffects = SpriteEffects.None;
                    }
                    else 
                    {
                        rectangle.Y += 20;
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                }

                newAt.X -= 10;
                newAt.Y -= 10;

                Draw.SpriteBatch.Draw(
                    subtexture.Texture2D, 
                    new Rectangle((int)newAt.X, (int)newAt.Y, 20, 20),
                    rectangle,
                    invalidY == rectangle.Y ? Color.Red : Color.White * alpha,
                    0f,
                    Vector2.One,
                    spriteEffects,
                    1f
                );
            }
        });

        AddActorDataModded(new() 
        {
            Name = "BGCrystal",
            Title = "CRYSTAL",
            Subtexture = TFGame.EditorAtlas["actors/MoonstoneCrystal"],
            Width = 10,
            Height = 10,
            AllowScreenWrap = true,
            Origin = new Vector2(0f, -5f)
        });

        AddActorDataModded(new() 
        {
            Name = "RainDrops",
            Title = "RAINDROPS",
            Subtexture = TFGame.Atlas["details/rainDrops"],
            Width = 10,
            Height = 10,
            AllowScreenWrap = true,
            TextureSizeRect = new Vector2(10, 10),
            ResizeableX = true,
            Origin = new Vector2(0, -5),
            Renderer = (actor, at, alpha) => 
            {
                Subtexture subtexture = TFGame.Atlas["details/rainDrops"];
                Vector2 pos = at + new Vector2(-actor.Width / 2, -actor.Height / 2);
                Draw.Texture(subtexture, new Rectangle(0, 0, 10, 10), pos, Color.White * alpha);
                Draw.Texture(subtexture, new Rectangle(20, 0, 10, 10), pos + Vector2.UnitX * (actor.Width - 10), Color.White * alpha);

                for (int i = 0; i < actor.Width / 10 - 2; i++)
                {
                    Draw.Texture(subtexture, new Rectangle(10, 0, 10, 10), pos + Vector2.UnitX * (10 + i * 10), Color.White * alpha);
                }
            }
        });

        AddActorDataModded(new() 
        {
            Name = "GhostShipWindow",
            Title = "SHIP WINDOW",
            Subtexture = TFGame.Atlas["details/shipWindow"],
            Width = 20,
            Height = 20,
            AllowScreenWrap = true,
            TextureSizeRect = new Vector2(20, 20),
            DarkWorldDLC = true
        });

        AddActorDataModded(new() 
        {
            Name = "SnowClump",
            Title = "SNOW CLUMP",
            Subtexture = TFGame.Atlas["details/snowDeposit"],
            Width = 10,
            Height = 10,
            TextureSizeRect = new Vector2(10, 10),
            AllowScreenWrap = true,
            Origin = new Vector2(-5, 5)
        });

        AddActorDataModded(new() 
        {
            Name = "BGMushroom",
            Title = "MUSHROOM",
            Subtexture = TFGame.Atlas["details/wallMushroom"],
            Width = 10,
            Height = 10,
            TextureSizeRect = new Vector2(10, 10),
            AllowScreenWrap = true,
            Origin = new Vector2(0, -5)
        });

        AddActorDataModded(new() 
        {
            Name = "BGBigMushroom",
            Title = "BIG MUSHROOM",
            Subtexture = TFGame.Atlas["details/bigMushroom"],
            Width = 10,
            Height = 20,
            AllowScreenWrap = true,
            Origin = new Vector2(0, 10)
        });

        AddActorDataModded(new() 
        {
            Name = "KingIntro",
            Title = "KING'S THRONE",
            Subtexture = TFGame.Atlas["throneRoom"],
            Width = 20,
            Height = 35,
            TextureSizeRect = new Vector2(20, 35),
            AllowScreenWrap = true,
            Origin = new Vector2(0, 5)
        });

        AddActorDataModded(new() 
        {
            Name = "PrismBlock",
            Title = "PRISM BLOCK",
            Subtexture = TFGame.BossAtlas["cataclysm/block2x3"],
            Width = 20,
            Height = 30,
            AllowScreenWrap = true,
            TextureSizeRect = new Vector2(20, 20),
            HasNodes = true,
            ResizeableX = true,
            ResizeableY = true,
            NodeCount = 1,
            Validate = (actor, at) => 
            {
                return (actor.Width == 40 && actor.Height == 10) ||
                    (actor.Width == 20 && actor.Height == 30) ||
                    (actor.Width == 30 && actor.Height == 10);
            },
            Renderer = (actor, at, alpha) => 
            {
                Subtexture subtexture;
                if (actor.Width == 40 && actor.Height == 10)
                {
                    subtexture = TFGame.BossAtlas["cataclysm/block4x1"];
                }
                else if (actor.Width == 20 && actor.Height == 30)
                {
                    subtexture = TFGame.BossAtlas["cataclysm/block2x3"];
                }
                else if (actor.Width == 30 && actor.Height == 10)
                {
                    subtexture = TFGame.BossAtlas["cataclysm/block3x1"];
                }
                else 
                {
                    Draw.Rect(at.X - actor.Width * 0.5f, at.Y - actor.Height * 0.5f, actor.Width, actor.Height, Color.Red * alpha);
                    return;
                }

                Draw.TextureCentered(subtexture, at, Color.White * alpha);
            }
        });
    }

    private static void PatchVanillaActor()
    {
        if (Data is null)
        {
            return;
        }

        if (Data.TryGetValue("Spawner", out var spawner))
        {
            spawner.HasNode = true;
            spawner.Attributes = [];
            spawner.Attributes.Add("name", "---");
        }

        var treasureChest = Data["TreasureChest"];
        treasureChest.Attributes = [];
        treasureChest.Attributes.Add("Type", "Normal");
        treasureChest.Attributes.Add("Mode", "Normal");
        treasureChest.Attributes.Add("Treasure", "Arrows");
        treasureChest.AttributeSchemas = [];

        treasureChest.AttributeSchemas.Add("Type", ["Normal", "AutoOpen", "Special", "Bottomless"]);
        treasureChest.AttributeSchemas.Add("Mode", ["Normal", "Enemies", "Touch", "Torches", "Event", "Time"]);
    }

    private static ActorData AddActorDataModded(ActorDataConstruct construct)
    {
        Subtexture subtexture = construct.Subtexture;
        if (construct.TextureSizeRect is {} rect)
        {
            var x = construct.Subtexture.X;
            var y = construct.Subtexture.Y;

            var tex = construct.Subtexture.Texture;
            subtexture = new Subtexture(tex, x, y, (int)rect.X, (int)rect.Y);
        }

        patch_ActorData actorData = new patch_ActorData
        {
            Name = construct.Name,
            Title = construct.Title,
            Subtexture = subtexture,
            Origin = construct.Origin,
            DefaultWidth = construct.Width,
            DefaultHeight = construct.Height,
            AllowScreenWrap = construct.AllowScreenWrap,
            HasNode = construct.HasNodes,
            ResizeableX = construct.ResizeableX,
            ResizeableY = construct.ResizeableY,
            MinWidth = construct.MinWidth,
            MaxWidth = construct.MaxWidth,
            MinHeight = construct.MinHeight,
            MaxHeight = construct.MaxHeight,
            Weight = construct.Weight,
            DarkWorldDLC = construct.DarkWorldDLC,
            Renderer = construct.Renderer,
            Validate = construct.Validate,
            Attributes = construct.Attributes,
            AttributeSchemas = construct.AttributeSchemas
        };

        Data.Add(construct.Name, actorData);
        ActorDatas.Add(actorData);
        return actorData;
    }


    private extern static patch_ActorData orig_AddData(string name, string title, Subtexture subtexture, Vector2 origin, int width, int height, bool allowScreenWrap, bool hasNode, bool resizeableX, bool resizeableY, int minWidth, int maxWidth, int minHeight, int maxHeight, int weight, bool darkWorldDLC, Action<Actor, Vector2, float> renderer = null);

    private static patch_ActorData AddData(string name, string title, Subtexture subtexture, Vector2 origin, int width, int height, bool allowScreenWrap, bool hasNode, bool resizeableX, bool resizeableY, int minWidth, int maxWidth, int minHeight, int maxHeight, int weight, bool darkWorldDLC, Action<Actor, Vector2, float> renderer = null) 
    {
        var data = orig_AddData(name, title, subtexture, origin, width, height, allowScreenWrap, hasNode, resizeableX, resizeableY, minWidth, maxWidth, minHeight, maxHeight, weight, darkWorldDLC, renderer);

        ActorDatas.Add(data);
        return data;
    }

    private struct ActorDataConstruct 
    {
        public required string Name;
        public required string Title;
        public required Subtexture Subtexture;
        public Vector2? TextureSizeRect;
        public Vector2 Origin;
        public int Width;
        public int Height;

        public bool AllowScreenWrap;
        public bool HasNodes;
        public bool ResizeableX;
        public bool ResizeableY;
        public int NodeCount = -1;
        public int MinWidth;
        public int MinHeight;
        public int MaxWidth = 320;
        public int MaxHeight = 240;
        public int Weight = 1;
        public bool DarkWorldDLC;
        public Action<Actor, Vector2, float> Renderer;
        public Func<Actor, Vector2, bool> Validate;
        public Dictionary<string, string> Attributes;
        public Dictionary<string, string[]> AttributeSchemas;

        public ActorDataConstruct()
        {
            if (MinWidth == 0)
            {
                MinWidth = Width;
            }

            if (MinHeight == 0)
            {
                MinHeight = Height;
            }
        }
    }
}
