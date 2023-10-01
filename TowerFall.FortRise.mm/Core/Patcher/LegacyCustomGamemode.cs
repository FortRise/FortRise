using System;
using TowerFall;

namespace FortRise;

// This is a legacy gamemode.
// Intended used for backward compatibility with CustomRoundLogic
// DO NOT USE THIS AS AN EXAMPLE
internal class LegacyCustomGamemode : GameMode
{
    private readonly Func<RoundLogic> logic;
    public override RoundLogic RoundLogic => logic();
    public override LevelSystem LevelSystem => new VersusLevelSystem(TowerData as VersusTowerData);

    public LegacyCustomGamemode(
        Session session, LevelData levelData,
        string roundLogicName, RoundLogicInfo info) :base(session, levelData)
    {
        logic = () => {
            var mode = CustomVersusRoundLogic.LookUpModes[roundLogicName];
            var id = CustomVersusRoundLogic.VersusModes[(int)mode];
            if (RiseCore.RoundLogicLoader.TryGetValue(id, out var value)) 
            {
                return value.Invoke((patch_Session)session, true);
            }
            return info.RoundType switch
            {
                RoundLogicType.FFA => new LastManStandingRoundLogic(session),
                RoundLogicType.HeadHunters => new HeadhuntersRoundLogic(session),
                RoundLogicType.TeamDeatchmatch => new TeamDeathmatchRoundLogic(session),
                _ => new LastManStandingRoundLogic(session)
            };
        };
    }

    public override GameModeInfo Initialize(GameModeBuilder builder)
    {
        return builder.Build();
    }
}