#pragma warning disable CS0626
#pragma warning disable CS0108

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public delegate Enemy EnemyLoader(Vector2 position, Facing facing);

public class patch_QuestSpawnPortal : QuestSpawnPortal
{
    private Queue<string> toSpawn;
    public static Dictionary<string, EnemyLoader> Loader = new();

    private bool autoDisappear;
    private Facing lastFacing;
    private Wiggler spawnWiggler;
    private Counter addCounter;

    public patch_QuestSpawnPortal(Vector2 position, Vector2[] nodes) : base(position, nodes)
    {
    }


    [MonoModIgnore]
    private extern bool Disappear();

    private ArrowTypes GetArrowTypes(string name) 
    {
        ArrowTypes types = ArrowTypes.Normal;
        if (name.Contains("Bomb"))
            types = ArrowTypes.Bomb;
        else if (name.Contains("SuperBomb"))
            types = ArrowTypes.SuperBomb;
        else if (name.Contains("Bramble"))
            types = ArrowTypes.Bramble;
        else if (name.Contains("Drill"))
            types = ArrowTypes.Drill;
        else if (name.Contains("Trigger"))
            types = ArrowTypes.Trigger;
        else if (name.Contains("Toy"))
            types = ArrowTypes.Toy;
        else if (name.Contains("Feather"))
            types = ArrowTypes.Feather;
        else if (name.Contains("Laser"))
            types = ArrowTypes.Laser;
        else if (name.Contains("Prism"))
            types = ArrowTypes.Prism;
        return types;
    }

    [MonoModReplace]
    private void FinishSpawn(Sprite<int> sprite) 
    {
        if (sprite.CurrentAnimID != 1 || sprite.CurrentFrame != 25 || toSpawn.Count == 0) 
            return;
        
        Facing facing;
        if (X == 160f) 
        {
            facing = lastFacing;
            lastFacing = lastFacing switch
            {
                Facing.Left => Facing.Right,
                _ => Facing.Left
            };
        }
        else 
        {
            facing = ((X < 160f)) ? Facing.Right : Facing.Left;
        }

        var name = toSpawn.Dequeue();
        if (Loader.TryGetValue(name, out EnemyLoader loader)) 
        {
            Level.Add(loader?.Invoke(Position + new Vector2(0f, 2f), facing));
        }
        else if (name.Contains("Skeleton") || name.Contains("Jester")) 
        {
            ArrowTypes arrows = ArrowTypes.Normal;
            bool hasShields = false;
            bool hasWings = false;
            bool canMimic = false;
            bool jester = false;
            bool boss = false;

            if (name.EndsWith("S"))
                hasShields = true;

            if (name.Contains("Wing"))
                hasWings = true;

            if (name.Contains("Mimic"))
                canMimic = true;

            if (name.Contains("Boss"))
                boss = true;
            
            if (name.Contains("Jester"))
                jester = true;
            
            arrows = GetArrowTypes(name);
            Level.Add(new Skeleton(Position + new Vector2(0f, 2f), facing, arrows, hasShields, hasWings, canMimic, jester, boss));
        }
        else

        switch (name) 
        {
        case "Elemental":
            Level.Add(new Ghost(Position, facing, Nodes, Ghost.GhostTypes.Fire));
            break;
        case "GreenElemental":
            Level.Add(new Ghost(Position, facing, Nodes, Ghost.GhostTypes.GreenFire));
            break;
        case "Ghost":
            Level.Add(new Ghost(Position, facing, Nodes, Ghost.GhostTypes.Blue));
            break;
        case "GreenGhost":
            Level.Add(new Ghost(Position, facing, Nodes, Ghost.GhostTypes.Green));
            break;

        case "EvilCrystal":
            Level.Add(new EvilCrystal(Position, facing, EvilCrystal.CrystalColors.Red, Nodes));
            break;
        case "BlueCrystal":
            Level.Add(new EvilCrystal(Position, facing, EvilCrystal.CrystalColors.Blue, Nodes));
            break;
        case "PrismCrystal":
            Level.Add(new EvilCrystal(Position, facing, EvilCrystal.CrystalColors.Pink, Nodes));
            break;
        case "BoltCrystal":
            Level.Add(new EvilCrystal(Position, facing, EvilCrystal.CrystalColors.Green, Nodes));
            break;

        case "Bat":
            Level.Add(new Bat(Position, facing, Bat.BatType.Eye));
            break;
        case "BombBat":
            Level.Add(new Bat(Position, facing, Bat.BatType.Bomb));
            break;
        case "SuperBombBat":
            Level.Add(new Bat(Position, facing, Bat.BatType.SuperBomb));
            break;
        case "Crow":
            Level.Add(new Bat(Position, facing, Bat.BatType.Bird));
            break;

        case "Slime":
            Level.Add(new Slime(Position + new Vector2(0, 5f), facing, Slime.SlimeColors.Green));
            break;
        case "RedSlime":
            Level.Add(new Slime(Position + new Vector2(0, 5f), facing, Slime.SlimeColors.Red));
            break;
        case "BlueSlime":
            Level.Add(new Slime(Position + new Vector2(0, 5f), facing, Slime.SlimeColors.Blue));
            break;

        case "Cultist":
            Level.Add(new Cultist(Position + new Vector2(0, 8), facing, Cultist.CultistTypes.Normal));
            break;
        case "ScytheCultist":
            Level.Add(new Cultist(Position + new Vector2(0, 8), facing, Cultist.CultistTypes.Scythe));
            break;
        case "BossCultist":
            Level.Add(new Cultist(Position + new Vector2(0, 8), facing, Cultist.CultistTypes.Boss));
            break;
        
        case "Birdman":
            Level.Add(new Birdman(Position, facing, false));
            break;
        case "DarkBirdman":
            Level.Add(new Birdman(Position, facing, true));
            break;

        case "Exploder":
            Level.Add(new Exploder(Position, facing, Nodes));
            break;

        case "Mole":
            Level.Add(new Mole(Position, facing));
            break;

        case "FlamingSkull":
            Level.Add(new FlamingSkull(Position, facing));
            break;

        case "TechnoMage":
            Level.Add(new TechnoMage(Position, facing));
            break;

        case "Worm":
            Level.Add(new Worm(Position + new Vector2(0f, 5f)));
            break;
        
        default:
            throw new Exception($"Entity Name: {name} is not a valid entity");
        }

        addCounter.Set(2);
        if (toSpawn.Count > 0) 
        {
            sprite.Play(1, true);
        }
        else 
        {
            sprite.Play(0, false);
            if (autoDisappear)
                Disappear();
        }
        Sounds.sfx_portalSpawn.Play(X, 1f);
        Level.ParticlesFG.Emit(Particles.EnPortalSpawn, 16, Position, new Vector2(6f, 8f));
        spawnWiggler.Start();
    }
}