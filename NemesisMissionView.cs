using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Engine;
using LT.Logger;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;
using System;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace LT_Nemesis
{

    public class NemesisMissionView : MissionView
    {

        bool _debug = false;

        public static NemesisMissionView? Instance { get; set; }

        GauntletLayer? _layer;
        IGauntletMovie? _movie;
        NemesisMissionVM? _dataSource;

        Camera? _camera;

        Agent? _agent = null;
        SoundEvent? _soundEvent = null;
        Hero? _hero = null;
        TraitObject? _persona;

        private bool _companionsHighlighted = false;
        private bool _enemyLordsHighlighted = false;

        public NemesisMissionView()
        {

            Instance = this;

            this._dataSource = null;

        }

        public override void OnMissionScreenInitialize()
        {

            base.OnMissionScreenInitialize();

            //LTLogger.IMRed(Mission.Mode.ToString());

            // workaround to hide the icon on mission start
            if (Mission != null && (Mission.Mode is MissionMode.Battle or MissionMode.Stealth or MissionMode.Deployment))
            {
                DelayedScreenInit(100);
            }

            //_dataSource.IsVisible = false;
            //_dataSource.Refresh();
            //LTLogger.IMRed("OnMissionScreenInitialize _dataSource.IsVisible = false");
        }


        private async void DelayedScreenInit(int duration)
        {
            await Task.Delay(duration);

            CreateScreen();

            _camera = base.MissionScreen.CombatCamera;

        }

        void CreateScreen()
        {
            if (Mission == null) return;

            _dataSource = new NemesisMissionVM(Mission);
            _layer = new GauntletLayer(1);
            _movie = _layer.LoadMovie("NemesisMissionHUD", _dataSource);
            MissionScreen.AddLayer(_layer);

            _camera = base.MissionScreen.CombatCamera;
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (_dataSource == null) return;
            if (Agent.Main == null) return;

            if (_debug)
            {
                if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
                {
                    RemoveScreen();
                    CreateScreen();
                    LTLogger.IMRed("Screen recreated.");

                    NemesisTextManager.Instance.Initialize();
                }
            }

            // Companion highlighting
            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.LeftAlt))
            {
                HighlightCompanions(true);
                HighlightEnemyLords(true);
            }
            if (Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.LeftAlt))
            {
                HighlightCompanions(false);
                HighlightEnemyLords(false);
            }



            if (_soundEvent != null) 
            {
                if (_agent != null && _agent.IsActive())
                {
                    // update sound position, because of agent movement
                    _soundEvent.SetPosition(_agent.Position);
                } else
                {
                    // agent disabled - stop the sound
                    if (_soundEvent.IsPlaying()) _soundEvent.Stop();

                    // and hide the icon
                    _dataSource.IsVisible = false;
                    _dataSource.IsVisibleImage = false;
                }

            } 

            // update icon position
            if (_camera != null && _agent != null)
            {
                // update agent icon coordinates

                float a = 0f;
                float b = 0f;
                float num = 0f;

                Vec3 position = _agent.Position;
                position.z = _agent.GetEyeGlobalPosition().Z;

                if (_agent.HasMount && _agent.MountAgent != null)
                {
                    //position.z += _agent.MountAgent.GetEyeGlobalPosition().Z;
                    position.z += 0.8f;
                } else
                {
                    position.z += 0.6f;
                }

                MBWindowManager.WorldToScreen(this._camera, position, ref a, ref b, ref num);

                // IsBehind = (num < 0f);
                if (num > 0f)
                {
                    _dataSource.IsVisible = true;

                    float agentHeight = 0;
                    float xOffset = 30;

                    _dataSource.ScreenPositionX = a - xOffset;
                    _dataSource.ScreenPositionY = b - agentHeight;

                    // update alpha based on distance
                    float distance = Agent.Main.Position.Distance(_agent.Position);
                   

                    if (distance > 100f) distance = 100f;
                    float alpha = (100f - distance) / 100f;
                    _dataSource.AlphaFactor = alpha;

                    int iconSize;

                    // update font size based on distance
                    if (distance < 10f) { _dataSource.FontSize = 20; iconSize = 25; }
                    else if (distance < 15f) { _dataSource.FontSize = 19; iconSize = 24; }
                    else if (distance < 20f) { _dataSource.FontSize = 18; iconSize = 23; }
                    else if (distance < 25f) { _dataSource.FontSize = 17; iconSize = 22; }
                    else if (distance < 30f) { _dataSource.FontSize = 16; iconSize = 21; }
                    else if (distance < 35f) { _dataSource.FontSize = 15; iconSize = 20; }
                    else if (distance < 40f) { _dataSource.FontSize = 14; iconSize = 19; }
                    else if (distance < 50f) { _dataSource.FontSize = 13; iconSize = 18; }
                    else if (distance < 60f) { _dataSource.FontSize = 12; iconSize = 17; }
                    else if (distance < 70f) { _dataSource.FontSize = 11; iconSize = 16; }
                    else if (distance < 80f) { _dataSource.FontSize = 10; iconSize = 15; }
                    else if (distance < 90f) { _dataSource.FontSize = 9; iconSize = 14; }
                    else if (distance < 100f) { _dataSource.FontSize = 8; iconSize = 13; }
                    else { _dataSource.FontSize = 7; iconSize = 12; }


                    _dataSource.Width = (float)iconSize;
                    _dataSource.Height = (float)iconSize;
                  

                    //_dataSource.HeroName = _agent.Name.ToString(); // + " alpha: " + alpha + " distance: " + distance; // + ": " + a.ToString() + " " + b.ToString();


                }
                else
                {
                    _dataSource.IsVisible = false;
                }

            }

        }



        public void ActivateNemesis(Agent agent, int actionID = 1) // actionID - what kind of audio to play - not implemented yet
        {
            if (_dataSource == null || Hero.MainHero == null) return;
            if (_agent != null || _soundEvent != null) return;     // we already have agent in process

            _agent = agent;

            // get hero out of agent
            Hero? hero = (agent.Character as CharacterObject)?.HeroObject;
            if (hero == null) return;
            _hero = hero;

            int relation = CharacterRelationManager.GetHeroRelation(Hero.MainHero, hero);

            if (hero.CharacterObject != null) _persona = hero.CharacterObject.GetPersona();

            if (_debug && _persona != null) LTLogger.IMTAGreen(hero.Name.ToString() + " " + _persona.Name.ToString() + " relation: " + relation.ToString());


            // TODO - read actual audio duration
            int voiceDuration = 5000;

            string voiceName = GetVoiceName(actionID, ref voiceDuration);

            // play audio
            int soundIndex = SoundEvent.GetEventIdFromString(voiceName);
            _soundEvent = SoundEvent.CreateEvent(soundIndex, Mission.Scene);
            _soundEvent.SetPosition(agent.Position);
            _soundEvent.Play();

            //relation = 100;

            _dataSource.Color = GetColorByRelation(relation);

            _dataSource.HeroName = _hero.Name.ToString();

            //_dataSource.HeroName = "Veeeeryyyyyyyyy loooong name of a lord";

            _dataSource.IsVisibleImage = true;
            if (_hero.CharacterObject != null) _dataSource.ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(_hero.CharacterObject)));
            if (_hero.Clan != null && _hero.Clan.Banner != null) _dataSource.Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_hero.Clan.Banner), true);

            _dataSource.VoiceLineText = NemesisTextManager.Instance.GetVoiceLineTextByVoiceName(voiceName);

            if (_debug)
            {
                uint focusedContourColor = new TaleWorlds.Library.Color(1f, 0.84f, 0.35f, 1f).ToUnsignedInteger();
                agent.AgentVisuals?.SetContourColor(focusedContourColor, true);
            }

            // lord should look at the player when screeming, not sure if it works
            agent.AgentVisuals?.SetLookDirection(Agent.Main.GetEyeGlobalPosition());

            DelayedNemesisActionDeactivation(voiceDuration);
        }


        private async void DelayedNemesisActionDeactivation(int duration)
        {
            await Task.Delay(duration);
            if (_dataSource == null) return;
            
            _dataSource.IsVisible = false;
            _dataSource.IsVisibleImage = false;

            if (_debug) _agent?.AgentVisuals?.SetContourColor(null);

            // we are not stopping playing sound here, letting it finish

            _agent = null;
            _soundEvent = null;

        }


        string GetVoiceName(int actionID, ref int duration)
        {
            string voiceName = "f1_test";
            if (_agent == null) return voiceName;

            string gender;
            if (_agent.IsFemale) gender = "f"; else gender = "m";

            int voiceNumber = 1;

            int voiceCountInCategory = 100;

            // select voiceNumber/voice line count based on the game's voice type
            if (_agent.IsFemale) {
                if (_persona == DefaultTraits.PersonaSoftspoken)   voiceNumber = 2;
                else if (_persona == DefaultTraits.PersonaCurt)    voiceNumber = 1;
                else if (_persona == DefaultTraits.PersonaEarnest) voiceNumber = 3;
                else if (_persona == DefaultTraits.PersonaIronic)  voiceNumber = 1;
            }
            else
            {
                if (_persona == DefaultTraits.PersonaSoftspoken)    voiceNumber = 1;
                else if (_persona == DefaultTraits.PersonaCurt)     voiceNumber = 1; // 1
                else if (_persona == DefaultTraits.PersonaEarnest)  voiceNumber = 3;
                else if (_persona == DefaultTraits.PersonaIronic)   voiceNumber = 4;
            }


            // debug
            if (_debug)
            {
                if (_agent.IsFemale)
                {
                    voiceNumber = 4;
                    voiceCountInCategory = 16;
                }
                else
                {
                    voiceNumber = 3;
                    voiceCountInCategory = 100;
                }
            }


            string categoryName = "general";
            if (actionID == 1) categoryName = "general";    // for the future

            Random rand = new Random();

            voiceName = gender + voiceNumber.ToString() + "_" + categoryName + "_" + (rand.Next(voiceCountInCategory) + 1).ToString();

            duration = 5000; // TODO rework

            if (_debug) LTLogger.IMBlue(voiceName);

            return voiceName;
        }





        string GetColorByRelation(int relation)
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



        private void HighlightCompanions(bool show = true)
        {
            if (show == _companionsHighlighted) return; // action already done

            if (NemesisMissionLogic.Instance == null || NemesisMissionLogic.Instance.MissionCompanionAgents == null || NemesisMissionLogic.Instance.MissionCompanionAgents.Count == 0) return;

            foreach (Agent agent in NemesisMissionLogic.Instance.MissionCompanionAgents)
            {
                if (agent.IsActive())
                {
                    if (show)
                    {
                        if (Agent.Main.Position.Distance(agent.Position) < 30f)
                        {
                            //uint focusedContourColor = new TaleWorlds.Library.Color(1f, 1f, 1f, 1f).ToUnsignedInteger();
                            uint focusedContourColor = new TaleWorlds.Library.Color(0f, 1f, 0f, 1f).ToUnsignedInteger();
                            agent.AgentVisuals?.SetContourColor(focusedContourColor, true);
                        }
                    }
                    else
                    {
                        agent?.AgentVisuals?.SetContourColor(null);
                    }
                }
            }
            _companionsHighlighted = show;  // marking the current state
        }

        private void HighlightEnemyLords(bool show = true)
        {
            if (show == _enemyLordsHighlighted) return; // action already done

            if (NemesisMissionLogic.Instance == null || NemesisMissionLogic.Instance.MissionEnemyLordAgents == null || NemesisMissionLogic.Instance.MissionEnemyLordAgents.Count == 0) return;

            foreach (Agent agent in NemesisMissionLogic.Instance.MissionEnemyLordAgents)
            {
                if (agent.IsActive())
                {
                    if (show)
                    {
                        if (Agent.Main.Position.Distance(agent.Position) < 30f)
                        {
                            uint focusedContourColor = new TaleWorlds.Library.Color(1f, 0f, 0f, 1f).ToUnsignedInteger();
                            agent.AgentVisuals?.SetContourColor(focusedContourColor, true);
                        }
                    }
                    else
                    {
                        agent?.AgentVisuals?.SetContourColor(null);
                    }
                }
            }
            _enemyLordsHighlighted = show;  // marking the current state
        }


        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {

            affectedAgent?.AgentVisuals?.SetContourColor(null);

            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
        }

        // cleanup
        public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            base.OnMissionModeChange(oldMissionMode, atStart);
            _dataSource?.OnMissionModeChanged(Mission);
        }


        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            RemoveScreen();
        }

        void RemoveScreen()
        {
            if (_layer != null) MissionScreen.RemoveLayer(_layer);
            _movie = null;
            _layer = null;
            _dataSource = null;
        }







    }
}
