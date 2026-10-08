using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;


[MonoModPatch("TowerFall.ArchivesAwardsPage")]
public class ArchivesAwardsPage : TowerFall.ArchivesAwardsPage
{
    private string[] titles;
    private string[] numbers;
    private string[] totals;
    private Color[] colors;
    private Sprite<int>[] sprites;
    private Subtexture locked;
    private float maxScrollY;
    private float masterAlpha;
    private SineWave arrowSine;

    [MonoModLinkTo("TowerFall.ArchivesPage", "System.Void .ctor(System.String)")]
    [MonoModIgnore]
    public void base_ctor(string title) {}   


    [MonoModConstructor]
    [MonoModReplace]
    public void ctor()
    {
        masterAlpha = 1f;
        base_ctor("AWARDS");
        titles = new string[VersusAwards.Awards.Length];
        numbers = new string[VersusAwards.Awards.Length];
        totals = new string[VersusAwards.Awards.Length];
        colors = new Color[VersusAwards.Awards.Length];
        sprites = new Sprite<int>[VersusAwards.Awards.Length];
        locked = TFGame.MenuAtlas["awards/locked"];
        Calc.PushRandom(0);

        for (int i = 0; i < totals.Length; i++)
        {
            ulong awardPoint = 0;
            ulong totalPoint = 0;
            int highestArcherPoints = -1;

            for (int j = 0; j < SaveData.Instance.Stats.Awards[i].Length; j++)
            {
                ulong totalAward = SaveData.Instance.Stats.Awards[i][j];
                if (highestArcherPoints == -1 || totalAward > totalPoint)
                {
                    totalPoint = totalAward;
                    highestArcherPoints = j;
                }
                else if (totalAward == totalPoint)
                {
                    highestArcherPoints = Calc.Random.Choose(new int[] { highestArcherPoints, j });
                }
                awardPoint += totalAward;
            }

            titles[i] = VersusAwards.Awards[i].Name.ToUpperInvariant();
            numbers[i] = (i + 1).ToString() + ".";
            totals[i] = awardPoint.ToString();
            colors[i] = ArcherData.Archers[highestArcherPoints].ColorA;
            sprites[i] = VersusAwards.Awards[i].GetSprite(awardPoint == 0UL);
        }

        maxScrollY = VersusAwards.Awards.Length * 21.6f;
        Add(arrowSine = new SineWave(120));
        Calc.PopRandom();
    }
}
    