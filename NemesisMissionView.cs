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
using TaleWorlds.MountAndBlade.View;

namespace LT_Nemesis
{
    [DefaultView]
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

        //bool gotMissionScreen = false;

        public NemesisMissionView()
        {
            Instance = this;    // useless here after 1.2.9
            this._dataSource = null;
            if (_debug) LTLogger.IMGreen("NemesisMissionView INIT");
        }


        public override void OnBehaviorInitialize()
        {

            base.OnBehaviorInitialize();

            if (Mission != null && (Mission.Mode is MissionMode.Battle or MissionMode.Deployment or MissionMode.StartUp)) // or MissionMode.Stealth))
            {
                DelayedScreenInit(100);
            }
        }



        private async void DelayedScreenInit(int duration)
        {
            await Task.Delay(duration);

            if (base.MissionScreen == null)
            {
                LTLogger.IMRed("DelayedScreenInit: We DON'T have MissionScreen!");
                return;
            }

            CreateScreen();

            _camera = base.MissionScreen.CombatCamera;

        }

        void CreateScreen()
        {
            if (_debug) LTLogger.IMRed("CreateScreen");

            if (Mission == null) return;

            _dataSource = new NemesisMissionVM(Mission);
            _layer = new GauntletLayer(100);
            _movie = _layer.LoadMovie("NemesisMissionHUD", _dataSource);
            base.MissionScreen.AddLayer(_layer);
            _camera = base.MissionScreen.CombatCamera;

            if (_debug) LTLogger.IMRed("CreateScreen DONE");

            Instance = this;

        }

        public override void OnMissionScreenTick(float dt)
        {


            base.OnMissionScreenTick(dt);

            //if (!gotMissionScreen)
            //{
            //    if (base.MissionScreen == null)
            //    {
            //        LTLogger.IMGrey("OnMissionScreenTick: No MissionScreen...");
            //        return;
            //    }
            //    else
            //    {
            //        LTLogger.IMRed("OnMissionScreenTick: We have MissionScreen!");
            //        gotMissionScreen = true;
            //    }
            //}

            if (Agent.Main == null) return;

            //if (_debug)
            //{
            //    if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            //    {
            //        RemoveScreen();
            //        CreateScreen();
            //        LTLogger.IMBlue("Screen recreated.");

            //        NemesisTextManager.Instance.Initialize();
            //    }
            //}

            // only in the battle, no in the tournament
            ////if (NemesisMissionLogic.Instance != null && NemesisMissionLogic.Instance.MissionMode == MissionMode.Battle)
            if (Mission != null && (Mission.Mode == MissionMode.Battle))
            {
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
                    if (_dataSource != null)
                    {
                        _dataSource.IsVisible = false;
                        _dataSource.IsVisibleImage = false;
                    }
                }

            }


            // only in the battle, no in the tournament
            //if (NemesisMissionLogic.Instance != null && NemesisMissionLogic.Instance.MissionMode == MissionMode.Battle)
            if (Mission != null && (Mission.Mode == MissionMode.Battle))
                {

                // update icon position
                if (_dataSource != null && _camera != null && _agent != null)
                {

                    // update agent icon coordinates

                    float a = 0f;
                    float b = 0f;
                    float num = 0f;

                    Vec3 position = _agent.Position;
                    if (_agent.IsActive())
                    {
                        position.z = _agent.GetEyeGlobalPosition().Z;

                        if (_agent.HasMount && _agent.MountAgent != null)
                        {
                            //position.z += _agent.MountAgent.GetEyeGlobalPosition().Z;
                            position.z += 0.8f;
                        }
                        else
                        {
                            position.z += 0.6f;
                        }
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

        }



        public void ActivateNemesis(Agent agent, int actionID = 1)
        {

            if (_debug) LTLogger.IMBlue("ActivateNemesis");

            if (Hero.MainHero == null) return;

            if (_debug && _agent != null) LTLogger.IMBlue("agent != null");

            if (_agent != null || _soundEvent != null) return;     // we already have agent in process

            // get hero data out of agent
            Hero? hero = (agent.Character as CharacterObject)?.HeroObject;
            if (hero == null)
            {
                if (_debug) LTLogger.IMBlue("ActivateNemesis - no hero!");
                return;
            }
            
            _hero = hero;
            _agent = agent;     // mark that this agent will shout now



            if (_debug) LTLogger.IMBlue("ActivateNemesis - we have hero");

            if (hero.CharacterObject != null) _persona = hero.CharacterObject.GetPersona();

            int relation = CharacterRelationManager.GetHeroRelation(Hero.MainHero, hero);

            if (_debug && _persona != null)
            {
                string heroID = hero.StringId;
                LTLogger.IMBlue(hero.Name.ToString() + " " + _persona.Name.ToString() + " relation: " + relation.ToString() + " stringID: " + heroID);
            }



            // shout audio
            // TODO - read actual audio duration
            int voiceDuration = 8000;

            string voiceName = NemesisHelpers.GetVoiceName(agent, hero, _persona, actionID, ref voiceDuration, hero.StringId, _debug);

            // play audio
            int soundIndex = SoundEvent.GetEventIdFromString(voiceName);
            _soundEvent = SoundEvent.CreateEvent(soundIndex, Mission.Scene);
            _soundEvent.SetPosition(agent.Position);
            _soundEvent.Play();

            if (_dataSource == null) LTLogger.IMRed("_dataSource == null");


            //LTLogger.IMRed(Mission.Mode.ToString());

            // visuals (name/face/text)
            // only in the battle, not in the tournament, not anymore, MissionMode in 1.2.x for tournament became 'Battle'
            if (Mission != null && (Mission.Mode == MissionMode.Battle) && _dataSource != null)
            //if (NemesisMissionLogic.Instance != null && NemesisMissionLogic.Instance.MissionMode == MissionMode.Battle && _dataSource != null)
            {

                _dataSource.Color = NemesisHelpers.GetColorByRelation(relation);
                _dataSource.HeroName = _hero.Name.ToString();

                //_dataSource.HeroName = "Veeeeryyyyyyyyy loooong name of a lord";

                // show lord face+text only for general shouts
                if (actionID == 1)
                {
                    _dataSource.IsVisibleImage = true;
                    if (_hero.CharacterObject != null) _dataSource.ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(_hero.CharacterObject)));
                    if (_hero.Clan != null && _hero.Clan.Banner != null) _dataSource.Banner = new ImageIdentifierVM(BannerCode.CreateFrom(_hero.Clan.Banner), true);

                    _dataSource.VoiceLineText = NemesisTextManager.Instance.GetVoiceLineTextByVoiceName(voiceName);
                }
                else
                {
                    _dataSource.IsVisibleImage = false;
                }
            }

            // lord should look at the player when screeming, not sure if it works
            agent.AgentVisuals?.SetLookDirection(Agent.Main.GetEyeGlobalPosition());

            DelayedNemesisActionDeactivation(voiceDuration);
        }


        private async void DelayedNemesisActionDeactivation(int duration)
        {
            if (_debug) LTLogger.IMBlue("DelayedNemesisActionDeactivation: " + duration.ToString());

            await Task.Delay(duration);

            if (_dataSource != null)
            {
                _dataSource.IsVisible = false;
                _dataSource.IsVisibleImage = false;
            }

            //if (_debug) _agent?.AgentVisuals?.SetContourColor(null);

            // we are not stopping playing sound here, letting it finish

            _agent = null;
            _soundEvent = null;

            if (_debug) LTLogger.IMBlue("DelayedNemesisActionDeactivation: _agent = null");

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
            if (Mission != null && (Mission.Mode == MissionMode.Battle))
            {
                affectedAgent?.AgentVisuals?.SetContourColor(null);
            }

            //if (affectedAgent == _agent)
            //{
            //    LTLogger.IMRed("OnAgentRemoved");

            //    if (_dataSource != null)
            //    {
            //        _dataSource.IsVisible = false;
            //        _dataSource.IsVisibleImage = false;
            //    }
            //}

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

            _hero = null;
            _agent = null;
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
