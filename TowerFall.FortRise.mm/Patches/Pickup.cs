using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_Pickup : Pickup
{
    [MonoModIgnore]
    public Pickups PickupType { get; private set; }
    public patch_Pickup(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
    }

    [MonoModReplace]
    public static Pickup CreatePickup(Vector2 position, Vector2 targetPosition, Pickups type, int playerIndex)
    {
        Pickup pickup;
        pickup = type switch 
        {
            Pickups.Arrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Normal),
            Pickups.BombArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Bomb),
            Pickups.SuperBombArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.SuperBomb),
            Pickups.LaserArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Laser),
            Pickups.BrambleArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Bramble),
            Pickups.DrillArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Drill),
            Pickups.BoltArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Bolt),
            Pickups.FeatherArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Feather),
            Pickups.TriggerArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Trigger),
            Pickups.PrismArrows => new ArrowTypePickup(position, targetPosition, ArrowTypes.Prism),
            Pickups.Shield => new ShieldPickup(position, targetPosition),
            Pickups.Wings => new WingsPickup(position, targetPosition),
            Pickups.SpeedBoots => new SpeedBootsPickup(position, targetPosition),
            Pickups.Mirror => new MirrorPickup(position, targetPosition),
            Pickups.DarkOrb => new OrbPickup(position, targetPosition, OrbPickup.OrbTypes.Dark),
            Pickups.TimeOrb => new OrbPickup(position, targetPosition, OrbPickup.OrbTypes.Time),
            Pickups.ChaosOrb => new OrbPickup(position, targetPosition, OrbPickup.OrbTypes.Chaos),
            Pickups.LavaOrb=> new OrbPickup(position, targetPosition, OrbPickup.OrbTypes.Lava),
            Pickups.SpaceOrb => new OrbPickup(position, targetPosition, OrbPickup.OrbTypes.Space),
            Pickups.Bomb => new BombPickup(position, targetPosition, playerIndex),
            _ => GetCustomPickup(position, targetPosition, playerIndex, type)
        };
        (pickup as patch_Pickup).PickupType = type;
        return pickup;
    }

    private static Pickup GetCustomPickup(Vector2 position, Vector2 target, int playerIndex, Pickups type) 
    {
        if (type <= Pickups.Gem)
            return null;
        if (!FortRise.RiseCore.PickupLoader.TryGetValue(type, out var loader)) 
        {
            Logger.Error("Pickup type cannot be found!");
            return null;
        }
        return loader?.Invoke(position, target, playerIndex);
    }
}