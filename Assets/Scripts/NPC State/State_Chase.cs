using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class State_Chase : NPCState
{
    float speed;
    float chaseTime;
    float counter;
    float aggresiveIndex = 1;//use to measure 
    Vector2 TargetPos;


    public override void Init(NPCContol n, object args = null)
    {
        base.Init(n, args);
        type=NPCStateType.Chase;
    }
    public override void OnEnterState(NPCStateType lastState, object args = null)
    {
        base.OnEnterState(lastState, args);
        Debug.Log($"{npc.transform.name} enter chase state");
        speed=GetChaseSpeed();
        chaseTime=GetChaseTime();
        counter = 0;
        npc.aggresive += aggresiveIndex * (1 - Game.Control.HazardLevel>0? (1-Game.Control.HazardLevel):0);
        TargetPos = npc.AStarPathFind(Game.Control.player.transform.position);
    }
    public override void OnLeaveState()
    {
        base.OnLeaveState();
        npc.isMoving = false;
    }
    public override void Refresh()
    {
        base.Refresh();

        if (!npc.canMove)
            return;

        counter += Time.deltaTime;
        if (counter >= chaseTime)
        {
            ThinkNextStep();
            return;
        }

        if (Vector2.Distance(npc.transform.position, TargetPos) < 0.05f)//检测是否到达终点
        {
            npc.transform.position = TargetPos;
            TargetPos = npc.AStarPathFind(Game.Control.player.transform.position);
        }
        else
        {
            if (Game.Control.mapInfo[(int)TargetPos.x, (int)TargetPos.y] > 100)
            {
                //TODO: Add path find?
                return;
            }
                
            npc.isMoving=true;
            npc.moveDirection=TargetPos-(Vector2)npc.transform.position;
            npc.transform.position = Vector2.MoveTowards(npc.transform.position, TargetPos, speed * Time.deltaTime);
        }
        

    }

    float GetChaseTime()
    {
        return Game.Control.HazardLevel*4+3;
    }
    float GetChaseSpeed()
    {
        return Game.Control.HazardLevel * 0.5f + 1.1f;
    }

    void ThinkNextStep()
    {
        float choice = npc.rnd.nextFloat();

        if (choice >= (Game.Control.HazardLevel + 1) / 2)//Select the Safe way
        {
            if (choice <= Game.Control.HazardLevel/ 2)
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
            if (choice >= npc.aggresive)
            {
                npc.ChangeState(this, NPCStateType.Chase);
            }
            else
            {
                npc.ChangeState(this, NPCStateType.Surround);
            }
        }

    }
}
