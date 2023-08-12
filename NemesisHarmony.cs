using HarmonyLib;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem;

namespace LT_Nemesis
{
    // True Noble Opinion
    [HarmonyPatch(typeof(DefaultDiplomacyModel), "GetHeroesForEffectiveRelation")]
    internal class DiplomaticRelationOverride
    {
        public static bool Prefix(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
        {
            effectiveHero1 = hero1;
            effectiveHero2 = hero2;
            return false;
        }
    }
}
