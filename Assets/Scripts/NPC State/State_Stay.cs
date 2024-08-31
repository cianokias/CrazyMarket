using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Stay : NPCState
{
    public override void Init(NPCContol n, object args = null)
    {
        base.Init(n, args);
        type = NPCStateType.Stay;
    }

    public override void OnEnterState(NPCStateType lastState, object args = null)
    {
        base.OnEnterState(lastState, args);
    }

    public override void OnLeaveState()
    {
        base.OnLeaveState();

    }

    public override void Refresh()
    {
        base.Refresh();
    }
}
