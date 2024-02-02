using LT.Logger;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.CampaignSystem.Actions.ChangeRelationAction;

namespace LT_Nemesis
{
    internal class NemesisHelpers
    {
        public static Hero? GetAgentHero(Agent agent)
        {
            Hero? hero = (agent.Character as CharacterObject)?.HeroObject;
            if (hero == null) return null;
            return hero;
        }



        // used for 5 voice pitch variants
        public static int GenerateHashNumber(string input)
        {
            int hash = Math.Abs(input.GetHashCode()); // Get the hash code of the input string
            int range = 5; // Number of possible values (0, 1, 2, 3, 4) 
            return hash % range; // Map the hash code to the desired range
        }


        public static string GetColorByRelation(int relation)
        {
            string color = "#FFFFFFFF";

            if (relation < -80) color = "#C93421FF";
            else if (relation < -60) color = "#D47052FF";
            else if (relation < -40) color = "#E3A38BFF";
            else if (relation < -20) color = "#F3D2C2FF";
            else if (relation < 0) color = "#F9F0EBFF";
            else if (relation == 0) color = "#FFFFFFFF";
            else if (relation < 20) color = "#EDF4ECFF";
            else if (relation < 40) color = "#BED9BBFF";
            else if (relation < 60) color = "#93C090FF";
            else if (relation < 80) color = "#72B074FF";
            else if (relation <= 100) color = "#419550FF";

            return color;
        }


        public static string GetVoiceName(Agent agent, Hero hero, TraitObject? persona, int actionID, ref int duration, string heroID, bool debug)
        {
            string voiceName = "f1_test";
            if (agent == null) return voiceName;
            if ((hero == null || hero.Culture == null)) return voiceName;

            string gender;
            if (agent.IsFemale) gender = "f"; else gender = "m";

            int voiceNumber = 1;

            int voiceCountInCategory = 100;

            int pitchMod = NemesisHelpers.GenerateHashNumber(heroID);

            // select voiceNumber/voice line count based on the game's voice type
            if (agent.IsFemale)
            {
                if (persona == DefaultTraits.PersonaSoftspoken) voiceNumber = 4;
                else if (persona == DefaultTraits.PersonaCurt) voiceNumber = 1;
                else if (persona == DefaultTraits.PersonaEarnest) voiceNumber = 4;
                else if (persona == DefaultTraits.PersonaIronic) voiceNumber = 3;

                
                if (hero.Culture.StringId == "sturgia")
                {
                    // voice with slavic accent for sturgian females
                    voiceNumber = 2;
                } 
                else if (hero.Culture.StringId == "vlandia")
                {
                    // voice with french accent for vlandian females
                    voiceNumber = 5;
                }


            }
            else
            {
                if (persona == DefaultTraits.PersonaSoftspoken) voiceNumber = 2;
                else if (persona == DefaultTraits.PersonaCurt) voiceNumber = 1;
                else if (persona == DefaultTraits.PersonaEarnest) voiceNumber = 4;
                else if (persona == DefaultTraits.PersonaIronic) voiceNumber = 4;

                // voice with scottish accent for battanian males
                if (hero.Culture.StringId == "battania")
                {
                    voiceNumber = 3;
                }
                else if (hero.Culture.StringId == "vlandia")
                {
                    // voice with french accent for vlandian males
                    voiceNumber = 5;
                }
            }

            // debug
            if (debug)
            {
                if (agent.IsFemale)
                {
                    voiceNumber = 5;
                    voiceCountInCategory = 100;
                    pitchMod = 1;
                    //gender = "m";
                }
                else
                {
                    voiceNumber = 5;
                    voiceCountInCategory = 100;
                    pitchMod = 1;
                }
            }


            duration = 6500;  // general shouts
            string categoryName = "general";

            if (actionID == 1) categoryName = "general";
            if (actionID == 2)
            {
                categoryName = "haha";
                duration = 3000;

                //if (debug)
                //{
                //    gender = "m";
                //    voiceNumber = 1;
                //}
            }
            if (actionID > 10)
            {
                categoryName = "onhit";
                duration = 4000;

                //if (debug)
                //{
                //    gender = "f";
                //    voiceNumber = 2;
                //}

            }

            Random rand = new Random();

            if (actionID == 1)
            {
                voiceName = gender + voiceNumber.ToString() + "_" + categoryName + "_" + (rand.Next(voiceCountInCategory) + 1).ToString();
            }
            else if (actionID > 1)
            {
                voiceName = gender + voiceNumber.ToString() + "_" + categoryName + "_" + actionID.ToString();
            }

            if (pitchMod > 0)
            {
                voiceName += "-p" + pitchMod.ToString();
            }

            //pitchMod = GenerateHashNumber("lord_2_dsdfsdf");
            if (debug) LTLogger.IMGreen(voiceName + " pitchMod: " + pitchMod);

            return voiceName;
        }


        // relation change without relation increase factor
        public static void CleanRelationChange(Hero originalHero, Hero originalGainedRelationWith, int relationChange, bool showQuickNotification)
        {

            //if (relationChange > 0)
            //{
            //    relationChange = MBRandom.RoundRandomized(Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(originalHero, originalGainedRelationWith, relationChange));
            //}

            if (relationChange != 0)
            {
                Campaign.Current.Models.DiplomacyModel.GetHeroesForEffectiveRelation(originalHero, originalGainedRelationWith, out var effectiveHero, out var effectiveHero2);
                int value = CharacterRelationManager.GetHeroRelation(effectiveHero, effectiveHero2) + relationChange;
                value = MBMath.ClampInt(value, -100, 100);
                effectiveHero.SetPersonalRelation(effectiveHero2, value);
                CampaignEventDispatcher.Instance.OnHeroRelationChanged(effectiveHero, effectiveHero2, relationChange, showQuickNotification, ChangeRelationDetail.Default, originalHero, originalGainedRelationWith);
            }
        }


    }

}
