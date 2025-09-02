using UnityEngine;
using NeonShift.Meta;

namespace NeonShift.Core
{
    public class TugOfWaste : MonoBehaviour
    {
        private int _you, _rival;
        private float _timer;
        private enum State { Idle, Holding, Applied, Done }
        private State _state;

        public void SetScores(int you, int rival) { _you = you; _rival = rival; }

        public void ResetForRound()
        { _timer = 0f; _state = State.Idle; }

        public void TickPressure(float dt)
        {
            if (_state == State.Done) return;
            int diff = Mathf.Abs(_you - _rival);
            if (diff >= 15)
            {
                if (_state == State.Idle)
                { _state = State.Holding; _timer = 0f; AnalyticsBridge.Log("pressure_start", ("diff", diff)); }
                else if (_state == State.Holding)
                {
                    _timer += dt;
                    if (_timer >= 5f)
                    { _state = State.Applied; AnalyticsBridge.Log("pressure_complete", ("diff", diff), ("applied", -10)); }
                }
            }
            else
            {
                if (_state == State.Holding)
                { _state = State.Idle; AnalyticsBridge.Log("pressure_break", ("diff", diff)); }
            }
        }

        public bool ShouldApplyPenaltyOnce()
        {
            if (_state == State.Applied)
            { _state = State.Done; return true; }
            return false;
        }
    }
}
