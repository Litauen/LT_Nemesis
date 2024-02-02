using HarmonyLib;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace LT_Nemesis
{
    // True Noble Opinion
    [HarmonyPatch(typeof(DefaultDiplomacyModel), "GetHeroesForEffectiveRelation")]
    internal class GetHeroesForEffectiveRelationPrefix
    {
        public static bool Prefix(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
        {
            effectiveHero1 = hero1;
            effectiveHero2 = hero2;
            return false;
        }
    }


    // relation increase on the prisoner release from the Party screen
    [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_player_let_prisoner_go_on_consequence")]
    internal class ApplyByReleasedByChoicePostfix
    {
        public static void Postfix()
        {
            NemesisHelpers.CleanRelationChange(Hero.MainHero, Hero.OneToOneConversationHero, 5, true);
        }
    }

}
