using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class State_Stay : NPCState
{
    float StopTime;
    float countTime;
    public override void Init(NPCContol n, object args = null)
    {
        base.Init(n, args);
        type = NPCStateType.Stay;
    }
    
    /// <param name="args">在这个状态下会等待多久(秒为单位)</param>
    public override void OnEnterState(NPCStateType lastState, object args = null)
    {
        base.OnEnterState(lastState, args);
        npc.isMoving= false;

        //StopTime = (float)args;
        float hazardLevel = 0;
        if (Game.Control is not null)
            hazardLevel = Game.Control.HazardLevel;
        else //For the use of Dev only
        {
            hazardLevel = GameObject.Find("NPC_SceneControl").GetComponent<NPC_SceneControl>().DangerRate;
        }
        StopTime=GetWaitTime(hazardLevel);
        countTime = 0;
        Debug.Log($"{npc.transform.name}进入等待模式，等待{StopTime}秒");
    }

    public override void OnLeaveState()
    {
        base.OnLeaveState();

    }

    public override void Refresh()
    {
        base.Refresh();
        countTime += Time.deltaTime;
        if (countTime > StopTime) 
        {
            ThinkNextStep();
        }
    }

    void ThinkNextStep()
    {
        float hazardLevel = 0;    
        if (Game.Control is not null)
            hazardLevel = Game.Control.HazardLevel;
        else //For the use of Dev only
        {
            hazardLevel = GameObject.Find("NPC_SceneControl").GetComponent<NPC_SceneControl>().DangerRate;
        }

        Randomer rnd = new Randomer();
        float choice=rnd.nextFloat();        

        if(choice < hazardLevel)//Select the Safe way
        {
            if (choice <= hazardLevel / 2)
            {
                npc.ChangeState(this, NPCStateType.Wander);
            }
            else
            {
                npc.ChangeState(this, NPCStateType.Spread);
            }
        }
        else//Select the dangerous way
        {
            if(choice<=npc.aggresive) 
            {
                npc.ChangeState(this, NPCStateType.Chase);
            }
            else
            {
                npc.ChangeState(this, NPCStateType.Surround);
            }
        }

    }

    float GetWaitTime(float dangerRate)
    {
        return 1 - dangerRate>0? 1-dangerRate:0;
    }
}
