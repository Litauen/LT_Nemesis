using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace LT_Nemesis
{
    public class NemesisAgentComponent : AgentComponent
    {

        public NemesisAgentComponent(Agent agent, RandomTimer timer) : base(agent)
        {
            this._timer = timer;
        }

        public bool CheckTimer()
        {
            return this._timer.Check(Mission.Current.CurrentTime);
        }

        public void ChangeTimerDuration(float min, float max)
        {
            this._timer.ChangeDuration(min, max);
        }

        private readonly RandomTimer _timer;
    }
}
