using System;
using System.Collections.Generic;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_Background : Background
{
    private Level level;
    private List<Background.BGElement> elements;
    public patch_Background(Level level, XmlElement xml) : base(level, xml)
    {
    }


    [MonoModReplace]
    private void ReadLayers(XmlElement xml) 
    {
        var lookup = false;
        foreach (var obj in xml) 
        {
            if (obj is XmlElement xmlElement) 
            {
                var name = xmlElement.Name;
                switch (name) 
                {
                case "Overtime":
                    if (this.level != null && this.level.Session.IsInOvertime)
                    {
                        this.ReadLayers(xmlElement);
                        lookup = false;
                        break;
                    }
                    lookup = true;
                    break;
                case "Else":
                    if (lookup) 
                    {
                        ReadLayers(xmlElement);
                        lookup = false;
                        break;
                    }
                    break;
                case "GhostShipLayer":
                    elements.Add(new Background.GhostShipLayer(level, xmlElement));
                    break;
                case "FadeMoveLayer":
                    elements.Add(new Background.FadeMoveLayer(level, xmlElement));
                    break;
                case "WavyLayer":
                    elements.Add(new Background.WavyLayer(level, xmlElement));
                    break;
                case "Random":
                    if (Calc.Random.Chance(xmlElement.AttrFloat("chance", 0.5f))) 
                    {
                        ReadLayers(xmlElement);
                        lookup = false;
                        break;
                    }
                    lookup = true;
                    break;
                case "AscensionLightningLayer":
                    elements.Add(new Background.AscensionLightningLayer(level, xmlElement));
                    break;
                case "LightningFlash":
                    if (!SaveData.Instance.Options.RemoveScreenFlashEffects) 
                    {
                        elements.Add(new Background.LightningFlashLayer(level, xmlElement));
                        break;
                    }
                    break;
                case "FadeLayer":
                    elements.Add(new Background.FadeLayer(level, xmlElement));
                    break;
                case "ScrollLayer":
                    elements.Add(new Background.ScrollLayer(level, xmlElement));
                    break;
                case "NodeLayer":
                    elements.Add(new Background.NodeLayer(level, xmlElement));
                    break;
                case "OceanLayer":
                    elements.Add(new Background.OceanLayer(level, xmlElement));
                    break;
                case "AnimatedLayer":
                    elements.Add(new Background.AnimatedLayer(level, xmlElement));
                    break;
                case "Backdrop":
                    elements.Add(new Background.Backdrop(level, xmlElement));
                    break;
                case "VortexLayer":
                    elements.Add(new Background.VortexLayer(level, xmlElement));
                    break;
                case "FlightMoonLayer":
                    elements.Add(new Background.FlightMoonLayer(level, xmlElement));
                    break;
                case "SacredGroundMoonLayer":
                    elements.Add(new Background.SacredGroundMoonLayer(level, xmlElement));
                    break;

                /* Customs */
                case "CustomBackdrop":
                    elements.Add(new patch_Background.CustomBackdrop(level, xmlElement));
                    break;
                case "CustomAnimatedLayer":
                    elements.Add(new patch_Background.CustomAnimatedLayer(level, xmlElement));
                    break;    
                case "CustomFadeLayer":
                    elements.Add(new patch_Background.CustomFadeLayer(level, xmlElement));
                    break;
                case "CustomFadeMoveLayer":
                    elements.Add(new patch_Background.CustomFadeMoveLayer(level, xmlElement));
                    break;
                case "CustomNodeLayer":
                    elements.Add(new patch_Background.CustomNodeLayer(level, xmlElement));
                    break;
                case "CustomOceanLayer":
                    elements.Add(new patch_Background.CustomOceanLayer(level, xmlElement));
                    break;
                case "CustomScrollLayer":
                    elements.Add(new patch_Background.CustomScrollLayer(level, xmlElement));
                    break;
                case "CustomWavyLayer":
                    elements.Add(new patch_Background.CustomWavyLayer(level, xmlElement));
                    break;
                }
            }
        }
    }

    public class CustomBackdrop : Background.BGElement
    {
        public Image Image;
        public CustomBackdrop(Level level, XmlElement xml) : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have an atlas!");
                return;
            }
            this.Image = new Image(storage.Atlas[xml.InnerText], null);
            this.Image.Position = xml.Position(Vector2.Zero);
            this.Image.Color = Color.White * xml.AttrFloat("opacity", 1f);
        }

        public override void Render()
        {
            Image.Render();
        }

        public override void Update()
        {
        }
    }

    public class CustomAnimatedLayer : Background.BGElement
    {
        private Sprite<int> sprite;
        public CustomAnimatedLayer(Level level, XmlElement xml) : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have a spriteData!");
                return;
            }
            this.sprite = storage.SpriteData.GetSpriteInt(xml.InnerText);
            this.sprite.Play(0, false);
            this.sprite.Position = xml.Position(Vector2.Zero);
            this.sprite.Color = Color.White * xml.AttrFloat("opacity");
        }

        public override void Render()
        {
            sprite.Render();
        }

        public override void Update()
        {
            sprite.Update();
        }
    }

    public class CustomFadeLayer : Background.BGElement
    {
        public Sprite<int> Sprite;
        public float Base;
        public float Amplitude;
        public Vector2 Position;
        public Vector2 Range;
        private SineWave sine;


        public CustomFadeLayer(Level level, XmlElement xml) : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have a spriteData!");
                return;
            }
            this.Base = xml.AttrFloat("opacity", 0.5f);
            this.Amplitude = xml.AttrFloat("opacityRange", 0.25f);
            this.Position = xml.Position(Vector2.Zero);
            this.Range = new Vector2(xml.AttrFloat("xRange"), xml.AttrFloat("yRange"));
            this.Sprite = storage.SpriteData.GetSpriteInt(xml.InnerText);
            this.Sprite.Position = this.Position;
            this.Sprite.Play(0, false);
            this.Sprite.RandomizeFrame();
            this.Sprite.OnAnimationComplete = this.UpdatePosition;
            this.sine = new SineWave(xml.AttrFloat("fadeSpeed", 0.02f) * 6.2831855f);
            this.sine.Randomize();
            this.UpdatePosition(null);
        }

        private void UpdatePosition(Sprite<int> s = null)
        {
            this.Sprite.Position = Calc.Random.Range(this.Position - this.Range, this.Range * 2f);
            this.Sprite.Rate = Calc.Random.Range(0.8f, 0.4f);
        }

        public override void Update()
        {
            this.Sprite.Update();
            this.sine.Update();
            this.Sprite.Color = Color.White * (this.Base + this.Amplitude * this.sine.Value);
        }

        public override void Render()
        {
            this.Sprite.Render();
        }        
    }

    public class CustomFadeMoveLayer : Background.BGElement
    {
        public Sprite<int> Sprite;
        public Vector2 Position;
        public Vector2 Range;
        public float MaxAlpha;
        public float Time;
        public float RateRange;
        private float counterAdd;
        private float counter;
        private bool appearing;


        public CustomFadeMoveLayer(Level level, XmlElement xml)
            : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have a spriteData!");
                return;
            }
            this.Position = xml.Position(Vector2.Zero);
            this.Range = new Vector2(xml.AttrFloat("xRange"), xml.AttrFloat("yRange"));
            this.MaxAlpha = xml.AttrFloat("maxAlpha", 1f);
            this.Time = xml.AttrFloat("time", 1f) * 60f;
            this.RateRange = xml.AttrFloat("rateRange", 0.4f);
            this.Sprite = storage.SpriteData.GetSpriteInt(xml.InnerText);
            this.Sprite.Position = this.Position;
            this.Sprite.Play(0, false);
            this.appearing = true;
            this.UpdatePosition();
        }

        private void UpdatePosition()
        {
            this.Sprite.Position = Calc.Random.Range(this.Position - this.Range, this.Range * 2f);
            this.Sprite.Rate = Calc.Random.Range(1f - this.RateRange / 2f, this.RateRange);
            this.counterAdd = 1f / this.Time;
            this.counterAdd *= Calc.Random.Range(0.9f, 1.1f);
        }

        public override void Update()
        {
            if (this.appearing)
            {
                this.counter = Math.Min(this.counter + this.counterAdd * Engine.TimeMult, 1f);
                if (this.counter >= 1f)
                {
                    this.appearing = false;
                }
            }
            else
            {
                this.counter = Math.Max(this.counter - this.counterAdd * Engine.TimeMult, 0f);
                if (this.counter <= 0f)
                {
                    this.appearing = true;
                    this.UpdatePosition();
                }
            }
            this.Sprite.Update();
        }

        public override void Render()
        {
            this.Sprite.Color = Color.White * Ease.CubeOut(this.counter) * this.MaxAlpha;
            this.Sprite.Render();
        }
    }

    public class CustomNodeLayer : Background.BGElement
    {
        private Sprite<int> sprite;
        private List<CustomNodeLayer.Node> nodes;

        private bool endWarp;
        private int currentNode;
        private Vector2 targetPosition;
        private float currentSpeed;
        private Counter delayCounter;

        public CustomNodeLayer(Level level, XmlElement xml)
            : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have a spriteData!");
                return;
            }
            this.endWarp = xml.AttrBool("endWarp", true);
            this.sprite = storage.SpriteData.GetSpriteInt(xml.Attr("sprite"));
            this.sprite.Play(0, false);
            this.sprite.Color = Color.White * xml.AttrFloat("opacity", 1f);
            this.delayCounter = new Counter();
            this.nodes = new List<CustomNodeLayer.Node>();
            foreach (object obj in xml)
            {
                if (obj is XmlElement && (obj as XmlElement).Name == "Node")
                {
                    XmlElement xmlElement = obj as XmlElement;
                    CustomNodeLayer.Node node = new CustomNodeLayer.Node
                    {
                        Position = xmlElement.Position(),
                        PositionRange = new Vector2(xmlElement.AttrFloat("xRange", 0f), xmlElement.AttrFloat("yRange")),
                        Speed = xmlElement.AttrFloat("speed", 1f),
                        SpeedRange = xmlElement.AttrFloat("speedRange", 0f),
                        AnimID = xmlElement.AttrInt("animID", 0),
                        Delay = xmlElement.AttrInt("delay", 0),
                        DelayRange = xmlElement.AttrInt("delayRange", 0)
                    };
                    this.nodes.Add(node);
                }
            }
            this.sprite.Position = Calc.Random.Range(this.nodes[0].Position - this.nodes[0].PositionRange / 2f, this.nodes[0].PositionRange);
            this.currentNode = -1;
            this.AdvanceNode();
        }

        private void AdvanceNode()
        {
            this.currentNode++;
            this.currentNode %= this.nodes.Count;
            int num = (this.currentNode + 1) % this.nodes.Count;
            if (num == 0 && this.endWarp)
            {
                this.currentNode = 0;
                num = 1;
                this.sprite.Position = Calc.Random.Range(this.nodes[0].Position - this.nodes[0].PositionRange / 2f, this.nodes[0].PositionRange);
            }
            this.targetPosition = Calc.Random.Range(this.nodes[num].Position, this.nodes[num].PositionRange);
            this.currentSpeed = Calc.Random.Range(this.nodes[this.currentNode].Speed, this.nodes[this.currentNode].SpeedRange);
            this.delayCounter.Set(Calc.Random.Range(this.nodes[this.currentNode].Delay, this.nodes[this.currentNode].DelayRange));
            this.sprite.Play(this.nodes[this.currentNode].AnimID, false);
        }

        public override void Update()
        {
            this.sprite.Update();
            if (this.delayCounter)
            {
                this.delayCounter.Update();
                return;
            }
            this.sprite.Position = this.sprite.Position.Approach(this.targetPosition, this.currentSpeed * Engine.TimeMult);
            if (this.sprite.Position == this.targetPosition)
            {
                this.AdvanceNode();
            }
        }

        public override void Render()
        {
            this.sprite.Render();
        }

        private struct Node
        {
            public Vector2 Position;
            public Vector2 PositionRange;
            public float Speed;
            public float SpeedRange;
            public int AnimID;
            public int Delay;
            public int DelayRange;
        }
    }

    public class CustomScrollLayer : Background.BGElement
    {
        public Image Image;
        public Vector2 Speed;
        public Vector2 WrapSize;
        public bool TileX;
        public bool TileY;

        public CustomScrollLayer(Level level, XmlElement xml) : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have an atlas!");
                return;
            }
            this.TileX = true;
            this.TileY = true;
            this.Image = new Image(storage.Atlas[xml.InnerText], null);
            this.Speed = new Vector2(xml.AttrFloat("speedX", 0f), xml.AttrFloat("speedY", 0f));
            this.Image.Position = xml.Position(Vector2.Zero);
            this.Image.Color = Color.White * xml.AttrFloat("opacity", 1f);
            this.Image.Visible = false;
            this.TileX = xml.AttrBool("tileX", true);
            this.TileY = xml.AttrBool("tileY", true);
            this.WrapSize = new Vector2(Math.Max(320f, this.Image.Width), Math.Max(240f, this.Image.Height));
        }

        public override void Update()
        {
            this.Image.Position += this.Speed * Engine.TimeMult;
            if (this.Image.X < 0f)
            {
                this.Image.X += this.WrapSize.X;
            }
            else if (this.Image.X >= this.WrapSize.X)
            {
                this.Image.X -= this.WrapSize.X;
            }
            if (this.Image.Y < 0f)
            {
                this.Image.Y += this.WrapSize.Y;
                return;
            }
            if (this.Image.Y >= this.WrapSize.Y)
            {
                this.Image.Y -= this.WrapSize.Y;
            }
        }

        public override void Render()
        {
            Vector2 position = this.Image.Position;
            this.Image.Position = this.Image.Position.Floor();
            this.Image.Render();
            if (this.Image.X != 0f && this.TileX)
            {
                this.Image.X -= this.WrapSize.X;
                this.Image.Render();
                this.Image.X += this.WrapSize.X;
            }
            if (this.Image.Y != 0f && this.TileY)
            {
                this.Image.Y -= this.WrapSize.Y;
                this.Image.Render();
                this.Image.Y += this.WrapSize.Y;
            }
            if (this.Image.X != 0f && this.Image.Y != 0f && this.TileX && this.TileY)
            {
                this.Image.Position -= this.WrapSize;
                this.Image.Render();
            }
            this.Image.Position = position;
        }
    }

    public class CustomWavyLayer : Background.BGElement
    {
        private Subtexture subtexture;
        private Color color;
        private Vector2 position;
        private float counter;
        private float add;
        private float amplitude;
        private int sliceSize;
        private float sliceAdd;
        private bool horizontal;

        public CustomWavyLayer(Level level, XmlElement xml)
            : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have an atlas!");
                return;
            }

            this.subtexture = storage.Atlas[xml.InnerText];
            this.color = Color.White * xml.AttrFloat("opacity", 1f);
            this.position = xml.Position(Vector2.Zero);
            this.add = 6.2831855f / (float)xml.AttrInt("waveFrames", 60);
            this.amplitude = xml.AttrFloat("amplitude", 2f);
            this.sliceSize = xml.AttrInt("sliceSize", 10);
            this.sliceAdd = 6.2831855f * xml.AttrFloat("sliceAdd", 0.125f);
            this.horizontal = xml.AttrBool("horizontal", false);
        }

        public override void Update()
        {
            this.counter = (this.counter + this.add * Engine.TimeMult) % 6.2831855f;
        }

        public override void Render()
        {
            if (this.horizontal)
            {
                Draw.SineTextureH(this.subtexture, this.position, Vector2.Zero, Vector2.One, 0f, this.color, SpriteEffects.None, this.counter, this.amplitude, this.sliceSize, this.sliceAdd);
                return;
            }
            Draw.SineTextureV(this.subtexture, this.position, Vector2.Zero, Vector2.One, 0f, this.color, SpriteEffects.None, this.counter, this.amplitude, this.sliceSize, this.sliceAdd);
        }
    }

    public class CustomOceanLayer : Background.BGElement
    {
        private Sprite<int> sprite;
        private Counter playCounter;
        private int timeMin;
        private int timeAdd;

        public CustomOceanLayer(Level level, XmlElement xml)
            : base(level)
        {
            var id = (level.Session.MatchSettings.LevelSystem.Theme as patch_TowerTheme).ThemeID;
            var storage = patch_GameData.CustomBGAtlas[id];
            if (storage.Atlas == null) 
            {
                Logger.Error($"[Background] {level.Session.MatchSettings.LevelSystem.Theme.Name} Storage does not have a spriteData!");
                return;
            }

            this.playCounter = new Counter();
            this.sprite = storage.SpriteData.GetSpriteInt(xml.InnerText);
            this.sprite.Position = xml.Position(Vector2.Zero);
            this.sprite.Color = Color.White * xml.AttrFloat("opacity");
            this.sprite.OnAnimationComplete = new Action<Sprite<int>>(this.OnAnimationComplete);
            this.timeMin = xml.AttrInt("timeMin", 120);
            this.timeAdd = xml.AttrInt("timeAdd", 660);
            this.OnAnimationComplete(null);
        }

        public override void Update()
        {
            if (this.playCounter)
            {
                this.playCounter.Update();
                if (!this.playCounter)
                {
                    Sounds.env_wave.Play(160f, 1f);
                    this.sprite.Play(0, true);
                    return;
                }
            }
            else
            {
                this.sprite.Update();
            }
        }

        public override void Render()
        {
            this.sprite.Render();
        }

        private void OnAnimationComplete(Sprite<int> sprite)
        {
            this.playCounter.Set(Calc.Random.Range(this.timeMin, this.timeAdd));
        }
    }

}
