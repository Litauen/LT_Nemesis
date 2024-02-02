using LT.Logger;
using SandBox.Tournaments.MissionLogics;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace LT_Nemesis
{

    public class AgentData
    {
        Hero? _hero = null;
        //float _distanceToPlayer = 0;
        public AgentData(Agent agent) 
        {
            if (!agent.IsHero) return;
            
            _hero = (agent.Character as CharacterObject)?.HeroObject;
            
        }
    }


    internal class NemesisMissionLogic : MissionLogic
    {
        readonly bool _debug = false;

        public static NemesisMissionLogic? Instance { get; set; }

        public MissionMode MissionMode;

        public List<Agent> MissionEnemyLordAgents;
        public List<Agent> MissionCompanionAgents;

        int _generalShoutMin = 15;  // minimal time when general shout is generated, default 15
        int _generalShoutMax = 45;  // maximal time when general shout is generated, default 45


        int maxDamageToRegisterHit = 5;
        int maxDamageToRegisterHorseHit = 10; //10

        // for fast lookup of agent data
        private Dictionary<Agent, AgentData> MissionEnemyLordAgentsDict;


        private TournamentBehavior? _tournamentBehavior;
        private TournamentMatch? _currentMatch;


        public NemesisMissionLogic()
        {

            Instance = this;

            this.MissionMode = MissionMode.Battle;

            this.MissionEnemyLordAgents = new List<Agent>();
            this.MissionCompanionAgents = new List<Agent>();

            this.MissionEnemyLordAgentsDict = new Dictionary<Agent, AgentData>();

            this._tournamentBehavior = null;
            this._currentMatch = null;

            if (_debug)
            {
                _generalShoutMin = 8;  // minimal time when general shout is generated, default 15
                _generalShoutMax = 8;  // maximal time when general shout is generated, default 45
            }
        }

        public override void OnDeploymentFinished()
        {
            base.OnDeploymentFinished();
            //LTLogger.IMRed("OnDeploymentFinished..");

            GetAllNemesisFromBattle();
            GetAllMissionCompanions();

        }


        public override void EarlyStart()
        {
            this._tournamentBehavior = Mission.Current.GetMissionBehavior<TournamentBehavior>();
            this._currentMatch = null;

            if (this._tournamentBehavior != null)
            {
                this.MissionMode = MissionMode.Tournament;
            }
        }

        public override void OnMissionTick(float dt)
        {

            if (this.MissionMode == MissionMode.Battle)
            {
                if (this.MissionEnemyLordAgents.Count > 0)
                {
                    this.NemesisGeneralShoutAction();
                }
            }
            else
            {
                if (this._tournamentBehavior != null)
                {
                    if (this._currentMatch != this._tournamentBehavior.CurrentMatch)
                    {
                        TournamentMatch currentMatch = this._tournamentBehavior.CurrentMatch;
                        if (currentMatch != null && currentMatch.IsPlayerParticipating())
                        {
                            if (Agent.Main != null && Agent.Main.IsActive())
                            {
                                this._currentMatch = this._tournamentBehavior.CurrentMatch;
                                this.OnTournamentRoundBegin(this._tournamentBehavior.NextRound == null);
                            }
                        }
                    }
                }
            }

        }


        public void OnTournamentRoundBegin(bool isFinalRound)
        {
            //LTLogger.IMRed("OnTournamentRoundBegin");

            MissionEnemyLordAgents.Clear();
            MissionEnemyLordAgentsDict.Clear();

            GetAllNemesisFromTournament();
        }

        // get all enemies in the Tournament
        public void GetAllNemesisFromTournament()
        {

            if (this._currentMatch == null) return;

            List<TournamentParticipant> tournamentParticipants = new();

            // get all enemy participants
            foreach (TournamentTeam tournamentTeam in this._currentMatch.Teams)
            {
                if (!tournamentTeam.IsPlayerTeam)
                {
                    foreach (TournamentParticipant tournamentParticipant in tournamentTeam.Participants)
                    {
                        tournamentParticipants.Add(tournamentParticipant);
                    }
                }
            }

            // convert participants to the agents and add them to the lists
            foreach (Agent agent in Mission.Agents)
            {
                if (agent.IsHero)
                {
                    TournamentParticipant participant = this._currentMatch.GetParticipant(agent.Origin.UniqueSeed);
                    if (participant != null)
                    {
                        if (tournamentParticipants.Contains(participant))
                        {
                            MissionEnemyLordAgents.Add(agent);
                            AgentData ad = new(agent);
                            MissionEnemyLordAgentsDict.Add(agent, ad);
                            //LTLogger.IMRed("added agent");
                        }
                    }
                }
            }

            if (_debug) LTLogger.IMRed("Enemy Heroes found: " + MissionEnemyLordAgents.Count.ToString());
        }


        // format list of Nemesis in the Mission
        public void GetAllNemesisFromBattle()
        {

            foreach (Agent agent in Mission.Current.PlayerEnemyTeam.ActiveAgents)
            {

                if (agent.IsHuman && agent.Character != null && agent.Character.IsHero) //&& agent != Agent.Main
                {

                    // get hero out of agent
                    Hero? hero = NemesisHelpers.GetAgentHero(agent);
                    if (hero == null) continue;

                    int relation = CharacterRelationManager.GetHeroRelation(Hero.MainHero, hero);

                    int rndStart = _generalShoutMin;
                    int rndEnd = _generalShoutMax;
                    if (_debug)
                    {
                        rndStart = 8;
                        rndEnd = 9;
                        //LTLogger.IMGreen("Hero: " + agent.Name + " rel: " + relation);
                    }

                    agent.AddComponent(new NemesisAgentComponent(agent, new RandomTimer(base.Mission.CurrentTime, rndStart, rndEnd)));
                    MissionEnemyLordAgents.Add(agent);

                    AgentData ad = new(agent);

                    MissionEnemyLordAgentsDict.Add(agent, ad);

                }

            }
            if (_debug) LTLogger.IMRed("Enemy Heroes found: " + MissionEnemyLordAgents.Count.ToString());
        }


        //Hero? GetAgentHero(Agent agent)
        //{
        //    Hero? hero = (agent.Character as CharacterObject)?.HeroObject;
        //    if (hero == null) return null; 
        //    return hero;
        //}




        // generates general shouts on random time periods
        private void NemesisGeneralShoutAction()
        {
            foreach (Agent agent in MissionEnemyLordAgents)
            {
                NemesisAgentComponent component = agent.GetComponent<NemesisAgentComponent>();
                if (component == null) continue;
                if (component.CheckTimer() && agent != null && agent.IsActive() && Agent.Main != null && Agent.Main.IsActive())
                {

                    float distance = Agent.Main.Position.Distance(agent.Position);
                   
                    if (distance < 30f)
                    {

                        if (AgentCanSeeThePlayer(agent, distance)) 
                        {
                            if (_debug) LTLogger.IMTAGreen(agent.Name + " activated. Distance: " + distance.ToString());
                            NemesisMissionView.Instance?.ActivateNemesis(agent, 1);
                        } else
                        {
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

            if (Agent.Main == null) goto end;

            int receivedDamage = blow.InflictedDamage;
            int takeAction = 0;

            Agent? shoutingHeroAgent = null;

            Random random = new Random();

            if (affectedAgent == Agent.Main)
            {
                
                WeaponComponentData attackerWeaponComponentData = attackerWeapon.CurrentUsageItem;
                // attackerWeaponComponentData null when hit by a horse
                if (_debug && attackerWeaponComponentData != null) LTLogger.IMRed("Received damage: " + receivedDamage + " [" + attackerWeaponComponentData.WeaponClass.ToString() + "]");


                if (receivedDamage > maxDamageToRegisterHit)  // player hit
                {
                    takeAction = 11;
                } else if (receivedDamage == 0)  // player blocked
                {
                    takeAction = 12; 
                }

                //affectorAgent.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
            }

            if (affectedAgent == Agent.Main.MountAgent)
            {
                if (_debug) LTLogger.IMRed("Horse received damage: " + receivedDamage);

                if (receivedDamage > maxDamageToRegisterHorseHit)  // player's horse-hit
                {
                    takeAction = 13;
                }

            }


            if (affectorAgent == Agent.Main)    // affected by player
            {

                if (affectedAgent.IsHero && MissionEnemyLordAgentsDict.ContainsKey(affectedAgent))   // is it enemy lord?
                {

                    shoutingHeroAgent = affectedAgent;

                    if (receivedDamage > maxDamageToRegisterHit)
                    {
                        takeAction = 21; // enemy-hit
                    }
                    else if (receivedDamage == 0)  // enemy-blocked
                    {
                        takeAction = 22;
                    }
                    // is it nearby? should we care? arrow from afar still he can curse, no need to check distance
                } 
                else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.State == AgentState.Active && MissionEnemyLordAgentsDict.ContainsKey(affectedAgent.RiderAgent))     // if horse, check if belongs to the enemy hero
                {
                    if (receivedDamage > maxDamageToRegisterHorseHit)
                    {
                        shoutingHeroAgent = affectedAgent.RiderAgent;
                        takeAction = 23; // enemy-horse-hit

                        if (_debug) LTLogger.IMRed("Enemy horse received damage: " + receivedDamage);
                    }
                }
                
            }

            if (takeAction == 0) goto end;

            // sometimes do not react
            int chance = 100;
            switch (takeAction)
            {
                case 2:
                    chance = 80;
                    break;
                case 11:
                    chance = 70;
                    break;
                case 12:
                    chance = 70;
                    break;
                case 13:
                    chance = 70;
                    break;
                case 21:
                    chance = 70;
                    break;
                case 22:
                    chance = 70;
                    break;
                case 23:
                    chance = 70;
                    break;
                default:
                    chance = 100;
                    break;
            }

            if (MissionMode == MissionMode.Tournament) chance /= 2;  // less talking in the tournaments

            if (_debug) chance = 100;   // FOR TESTING

            int r = random.Next(100)+1;
            if (r > chance)
            {
                if (_debug) LTLogger.IMRed("Low chance for scream :( " + r + " chance: " + chance);
                goto end;
            }


            // 11-12-13 if player hit/blocked/horse-hit - use enemy if he made a hit, orherwise find nearby enemies, select closest ones that can see the player, select random, generate a shout
            if (takeAction > 1 && takeAction < 30)
            {
                // maybe affectorAgent is enemy - use it, if not - search for random nearby enemies
                if (affectorAgent != null && MissionEnemyLordAgentsDict.ContainsKey(affectorAgent))
                {
                    shoutingHeroAgent = affectorAgent;
                } else
                {
                    // enemy lord did not hit the player or his horse
                    //find nearby enemies, limited distance

                    if (Agent.Main != null) {

                        float nearbyLordDistance = 20f;  // how far do we consider 'nearby'

                        MBList<Agent> nearbyEnemyAgents = Mission.GetNearbyEnemyAgents(Agent.Main.Position.AsVec2, nearbyLordDistance, Mission.PlayerTeam, new MBList<Agent>());

                        if (_debug) LTLogger.IMRed("Enemies around: " + nearbyEnemyAgents.Count.ToString());

                        List<Agent> nearbyLords = new();

                        foreach (Agent agent in nearbyEnemyAgents)
                        {
                            if (MissionEnemyLordAgentsDict.ContainsKey(agent))
                            {
                                if (AgentCanSeeThePlayer(agent, nearbyLordDistance)) 
                                { 
                                    nearbyLords.Add(agent);
                                    if (_debug) LTLogger.IMRed(" Nearby Enemy Lord: " + agent.Name);
                                }
                            }
                        }

                        if (nearbyLords.Count == 1)
                        {
                            shoutingHeroAgent = nearbyLords[0];
                        }
                        else if (nearbyLords.Count > 0)
                        {
                            random = new Random();
                            int randomIndex = random.Next(0, nearbyLords.Count);
                            shoutingHeroAgent = nearbyLords[randomIndex];
                        }
                    }

                }


                if (shoutingHeroAgent != null)
                {

                    if (_debug) LTLogger.IMRed("shoutingHeroAgent: " + shoutingHeroAgent.IsHero.ToString());

                    NemesisMissionView.Instance?.ActivateNemesis(shoutingHeroAgent, takeAction);

                    // reset timer to change how often the general msg will be played
                    NemesisAgentComponent component = shoutingHeroAgent.GetComponent<NemesisAgentComponent>();
                    if (component != null) component.ChangeTimerDuration((float)_generalShoutMin, (float)_generalShoutMax);  
                } else
                {
                    if (_debug) LTLogger.IMRed("shoutingHeroAgent == null");
                }

                if (_debug) LTLogger.IMRed("OnHitAction: " + takeAction + "   dmg: " + receivedDamage);
            }


        end:
            {
                base.OnAgentHit(affectedAgent, affectorAgent, attackerWeapon, blow, attackCollisionData);
            }
        }



        bool AgentCanSeeThePlayer(Agent agent, float distance)
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




        // cleanup
        public override void OnClearScene()
        {
            this.MissionEnemyLordAgents.Clear();
            this.MissionCompanionAgents.Clear();

            this.MissionEnemyLordAgentsDict.Clear();
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {

            // remove enemy from the list and his component
            NemesisAgentComponent component = affectedAgent.GetComponent<NemesisAgentComponent>();
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

            // remove enemy from the dictionary
            if (MissionEnemyLordAgentsDict.ContainsKey(affectedAgent))
            {
                MissionEnemyLordAgentsDict.Remove(affectedAgent);
            }


            // remove companion from the list
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
