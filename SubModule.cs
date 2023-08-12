using HarmonyLib;
using LT.Helpers;
using LT.Logger;
using TaleWorlds.MountAndBlade;


namespace LT_Nemesis
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            Harmony harmony = new Harmony("lt_nemesis");
            harmony.PatchAll();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            NemesisTextManager.Instance = new();
            NemesisTextManager.Instance.Initialize();
            
            LTLogger.IMGrey(LTHelpers.GetModName() + " Loaded");
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            if (mission == null) return;
            base.OnMissionBehaviorInitialize(mission);

            mission.AddMissionBehavior(new NemesisMissionLogic());
            mission.AddMissionBehavior(new NemesisMissionView());

            

        }

    }
}