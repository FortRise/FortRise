using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_LevelLoaderXML : LevelLoaderXML
{
    public Level Level
    {
        [MonoModIgnore]
        get
        {
            return null;
        }
        [MonoModIgnore]
        private set
        {
        }
    }

    public bool Finished
    {
        [MonoModIgnore]
        get
        {
            return false;
        }
        [MonoModIgnore]
        private set
        {
        }
    }



    private Coroutine loader;
    private bool errorShown;
    private int randomSeed;


    public patch_LevelLoaderXML(Session session) : base(session)
    {
    }

    public extern void orig_ctor(Session session);

    [MonoModConstructor]
    public void ctor(Session session)
    {
        orig_ctor(session);
        if (XML == null)
        {
            SetLayer(-1, new Layer());
            loader = null;
            Sounds.ui_click.Play(160f, 1f);
            session.MatchSettings.LevelSystem.Dispose();
        }
    }

    [MonoModLinkTo("Monocle.Scene", "System.Void Update()")]
    [MonoModIgnore]
    public void base_Update() { base.Update(); }

    [MonoModLinkTo("Monocle.Scene", "System.Void Render()")]
    [MonoModIgnore]
    public void base_Render() { base.Render(); }


    [MonoModReplace]
    public override void Update()
    {
        if (loader == null && !errorShown)
        {
            errorShown = true;
            this.ShowError("Missing Level");
        }
        if (errorShown)
        {
            MenuInput.Update();
            base_Update();
            return;
        }
        loader.Update();
    }

    public extern void orig_Render();

    public override void Render()
    {
        if (errorShown)
        {
            base_Render();
            return;
        }
        orig_Render();
    }

    [MonoModReplace]
    private IEnumerator Load()
    {
        TowerTheme theme = Session.MatchSettings.LevelSystem.Theme;
        if (Session.MatchSettings.Mode == Modes.Trials)
        {
            Music.Play(theme.Music);
        }
        else if (Session.MatchSettings.Mode == Modes.LevelTest)
        {
            Music.Play(theme.Music);
        }

        Level = new Level(Session, XML)
        {
            LoadSeed = randomSeed
        };

        Calc.Random = new Random(Level.LoadSeed);

        Level.Background = Session.MatchSettings.LevelSystem.GetBackground(Level);
        Level.Foreground = Session.MatchSettings.LevelSystem.GetForeground(Level);

        Session.LevelLoadStart(Level);

        bool[,] solidsBitData = Calc.GetBitData(XML["Solids"].InnerText, 32, 24);
        bool[,] bgBitData = Calc.GetBitData(XML["BG"].InnerText, 32, 24);
        int[,] overwriteData = Calc.ReadCSVIntGrid(XML["BGTiles"].InnerText, 32, 24);

        if (Session.MatchSettings.LevelSystem.Procedural)
        {
            bool[,] cloneSolids = (bool[,])solidsBitData.Clone();
            XmlNodeList elementsByTagName = XML["Entities"].GetElementsByTagName("RandomBlock");
            List<Rectangle> list = new List<Rectangle>();
            foreach (object obj in elementsByTagName)
            {
                XmlElement xmlElement = (XmlElement)obj;
                Vector2 vector = xmlElement.Position();
                Rectangle rectangle = new Rectangle(
                    (int)(vector.X / 10f),
                    (int)(vector.Y / 10f),
                    xmlElement.AttrInt("width") / 10,
                    xmlElement.AttrInt("height") / 10
                );
                list.Add(rectangle);
            }
            Calc.PushRandom(Session.MatchSettings.RandomLevelSeed);

            LevelRandomItems levelRandomItems = new LevelRandomItems();
            solidsBitData = LevelRandomGeometry.GenerateData(list, cloneSolids);
            levelRandomItems.AddItems(solidsBitData, XML["Entities"], Level.Session.MatchSettings.TeamMode);
            LevelRandomTreasure.AddRandomTreasure(XML, levelRandomItems.BaseSolids, levelRandomItems.MovingPlatforms, Level.Session.MatchSettings.TeamMode);
            bgBitData = LevelRandomBGTiles.GenerateBitData(solidsBitData);
            overwriteData = LevelRandomBGDetails.GenerateTileData(levelRandomItems.BaseSolids, bgBitData, solidsBitData, XML["Entities"]);

            Calc.PopRandom();
        }

        Level.Add(Level.Tiles = new LevelTiles(XML, solidsBitData));
        Level.Add(Level.BGTiles = new LevelBGTiles(XML, bgBitData, solidsBitData, overwriteData));

        foreach (object obj2 in XML["Entities"])
        {
            XmlElement e = (XmlElement)obj2;
            Level.LoadEntity(e);
        }

        if (XML["Entities"].GetElementsByTagName("BlueSwitchBlock").Count > 0 || XML["Entities"].GetElementsByTagName("RedSwitchBlock").Count > 0)
        {
            Level.Add(new SwitchBlockControl(Session));
        }

        if (XML.AttrBool("CanUnlockMoonstone", false) && SaveData.Instance.Unlocks.ShouldShowMoonBreakSequence)
        {
            Level.Add(new MoonBreakSequence());
        }

        if (XML.AttrBool("CanUnlockPurple", false) && SaveData.Instance.Unlocks.ShouldOpenPurpleArcherPortal)
        {
            Level.Add(new PurpleArcherUnlockSequence());
        }
        else if (Session.MatchSettings.Variants.DarkPortals)
        {
            Level.Add(new DarkPortalsVariantSequence());
        }

        Level.UpdateEntityLists();
        Session.OnLevelLoadFinish();

        if (StartLevelOnFinish)
        {
            Engine.Instance.Scene = Level;
        }

        Finished = true;
        yield break;
    }

}