using LT.Logger;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace LT_Nemesis
{
    internal class NemesisMissionLogic : MissionLogic
    {
        readonly bool _debug = false;

        public static NemesisMissionLogic? Instance { get; set; }

        public List<Agent> MissionEnemyLordAgents;
        public List<Agent> MissionCompanionAgents;

        public NemesisMissionLogic()
        {

            Instance = this;

            this.MissionEnemyLordAgents = new List<Agent>();
            this.MissionCompanionAgents = new List<Agent>();

        }

        public override void OnDeploymentFinished()
        {
            base.OnDeploymentFinished();
            //LTLogger.IMBlue("OnDeploymentFinished..");
            GetAllNemesis();
            GetAllMissionCompanions();
        }

        // format list of Nemesis in the Mission
        public void GetAllNemesis()
        {
            int totalHeroes = 0;

            foreach (Agent agent in Mission.Current.PlayerEnemyTeam.ActiveAgents)
            {

                if (agent.IsHuman && agent.Character != null && agent.Character.IsHero) //&& agent != Agent.Main
                {

                    totalHeroes++;

                    // get hero out of agent
                    Hero? hero = (agent.Character as CharacterObject)?.HeroObject;
                    if (hero == null) continue;

                    int relation = CharacterRelationManager.GetHeroRelation(Hero.MainHero, hero);

                    int rndStart = 15;
                    int rndEnd = 40;
                    if (_debug)
                    {
                        rndStart = 6;
                        rndEnd = 8;

                        //LTLogger.IMGreen("Hero: " + agent.Name + " rel: " + relation);
                    }

                    agent.AddComponent(new NemesisAgentComponent(agent, new RandomTimer(base.Mission.CurrentTime, rndStart, rndEnd)));
                    MissionEnemyLordAgents.Add(agent);

                }

            }
            //LTLogger.IMBlue("Heroes found: " + total_heroes);
        }


        public override void OnMissionTick(float dt)
        {
            if (this.MissionEnemyLordAgents.Count > 0)
            {
                this.NemesisAction();
            }
        }

        
        private void NemesisAction()
        {
            foreach (Agent agent in MissionEnemyLordAgents)
            {
                NemesisAgentComponent component = agent.GetComponent<NemesisAgentComponent>();
                if (component == null) continue;
                if (component.CheckTimer() && agent != null && agent.IsActive() && Agent.Main != null && Agent.Main.IsActive())
                {

                    float distance = Agent.Main.Position.Distance(agent.Position);
                   
                    if (distance < 50f)
                    {

                        if (AgentCanSeeThePlayer(agent, distance)) 
                        {
                            if (_debug) LTLogger.IMTAGreen(agent.Name + " activated. Distance: " + distance.ToString());
                            NemesisMissionView.Instance?.ActivateNemesis(agent, 1);
                        } else
                        {
                            // can't see the player
                            if (_debug) LTLogger.IMGrey(agent.Name + " can't see the player, distance: " + distance.ToString());
                        }

                    } else
                    {
                        // too far
                        if (_debug) LTLogger.IMGrey(agent.Name + ": " + distance.ToString());
                    }

                    //component.ChangeTimerDuration(6f, 12f);  // reset timer to change how often the audio will be played

                }
            }
        }


        private bool AgentCanSeeThePlayer(Agent agent, float distance)
        {
            bool debug = false;

            if (Agent.Main == null) return false;

            Vec3 eyeGlobalPosition = agent.GetEyeGlobalPosition();
            //This calculates the normalized vector(v) with a length of 1 and represents the direction from the agent's eye to the player's eye.
            Vec3 v = (Agent.Main.GetEyeGlobalPosition() - eyeGlobalPosition).NormalizedCopy();
            float num = distance; // 100f;
            float num2; // distance to any obstacle
            float num3; // distance to the player for raycasting
            // first raycast checks if there is anything between agent and the player
            if (base.Mission.Scene.RayCastForClosestEntityOrTerrain(eyeGlobalPosition, eyeGlobalPosition + v * (num + 0.01f), out num2, 0.01f, BodyFlags.CameraCollisionRayCastExludeFlags))
            {
                num = num2;
            }

            if (debug) LTLogger.IMGrey("1 RayCast dst: " + num2.ToString());

            // second raycast checks if this is the player
            Agent rayCastAgent = base.Mission.RayCastForClosestAgent(eyeGlobalPosition, eyeGlobalPosition + v * (num + 0.01f), out num3, agent.Index, 0.01f);
            string rcAgentName = rayCastAgent?.Name;

            if (rayCastAgent == Agent.Main)
            {
                if (debug) LTLogger.IMTAGreen("2 RayCast dst: " + num3.ToString() + " agent: " + rcAgentName);
                return true;
            }

            // some kind of exception, nothing in between (num2) but agent raycasting out of bounds
            if (float.IsNaN(num2) && num3 > distance)
            {
                if (debug) LTLogger.IMRed("Kind of ok...");
                return true;
            }

            if (debug) LTLogger.IMGrey("2 RayCast dst: " + num3.ToString() + " agent: " + rcAgentName);
            return false;
        }



        public void GetAllMissionCompanions()
        {
            int totalHeroes = 0;

            foreach (Agent agent in Mission.Current.PlayerTeam.ActiveAgents)
            {

                if (agent.IsHuman && agent.Character != null && agent.Character.IsHero && agent != Agent.Main)
                {

                    totalHeroes++;

                    //// get hero out of agent
                    //Hero? hero = (agent.Character as CharacterObject)?.HeroObject;
                    //if (hero == null) continue;
                    //if (hero!= null) { LTLogger.IMBlue("Name: " + hero.Name + "   StringID: " + hero.StringId);  }

                    MissionCompanionAgents.Add(agent);

                }

            }
            if (_debug) LTLogger.IMBlue("Companions found: " + totalHeroes);
        }




        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {

            if (affectedAgent == Agent.Main)
            {
                int receivedDamage = blow.InflictedDamage;

                WeaponComponentData attackerWeaponComponentData = attackerWeapon.CurrentUsageItem;

                if (_debug) LTLogger.IMRed("Received damage: " + receivedDamage + " [" + attackerWeaponComponentData.WeaponClass.ToString() + "]");

                //affectorAgent.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
            }

            if (Agent.Main != null && affectedAgent == Agent.Main.MountAgent)
            {
                int receivedDamage = blow.InflictedDamage;
                if (_debug) LTLogger.IMRed("Horse received damage: " + receivedDamage);
            }


            base.OnAgentHit(affectedAgent, affectorAgent, attackerWeapon, blow, attackCollisionData);
        }



        // cleanup
        public override void OnClearScene()
        {
            this.MissionEnemyLordAgents.Clear();
            this.MissionCompanionAgents.Clear();
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            VictoryComponent component = affectedAgent.GetComponent<VictoryComponent>();
            if (component != null)
            {
                affectedAgent.RemoveComponent(component);
            }
            for (int i = 0; i < this.MissionEnemyLordAgents.Count; i++)
            {
                if (this.MissionEnemyLordAgents[i] == affectedAgent)
                {
                    this.MissionEnemyLordAgents.RemoveAt(i);
                    return;
                }
            }
            for (int i = 0; i < this.MissionCompanionAgents.Count; i++)
            {
                if (this.MissionCompanionAgents[i] == affectedAgent)
                {
                    this.MissionCompanionAgents.RemoveAt(i);
                    return;
                }
            }
        }

    }
}
