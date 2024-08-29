using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCState
{
    protected NPCContol npc;
    public NPCStateType type;

    public virtual void OnEnterState(NPCStateType lastState,object args)
    {

    }

    public virtual void OnLeaveState()
    {

    }

    public virtual void Refresh()
    {

    }

}

public enum NPCStateType
{
    Empty,
    Stay,
    Chase,
    Spread,
}