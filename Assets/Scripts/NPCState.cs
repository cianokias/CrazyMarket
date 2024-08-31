using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCState
{
    protected NPCContol npc;
    public NPCStateType type;
    /// <summary>
    /// First initialize the state
    /// should be used only once
    /// </summary>
    /// <param name="args"></param>
    public virtual void Init(NPCContol n,object args=null)
    {
        npc = n;
    }

    /// <summary>
    /// run when entering this state
    /// </summary>
    /// <param name="lastState"></param>
    /// <param name="args"></param>
    public virtual void OnEnterState(NPCStateType lastState, object args = null)
    {

    }

    /// <summary>
    /// Run when leave the state
    /// </summary>
    public virtual void OnLeaveState()
    {

    }
    /// <summary>
    /// Run every frame when this is npc's current state
    /// </summary>
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