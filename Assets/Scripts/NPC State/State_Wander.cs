using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Wander : NPCState
{
    float wanderTime;
    float stateCount;
    float wanderFrequency;
    float wanderCount;
    float wanderSpeed;

    Vector2 nextPos;
    public override void Init(NPCContol n, object args = null)
    {
        base.Init(n, args);
        type=NPCStateType.Wander;   
    }
    public override void OnEnterState(NPCStateType lastState, object args = null)
    {
        base.OnEnterState(lastState, args);
        wanderTime=GetWanderTime();
        stateCount = 0;
        wanderFrequency=GetWanderFrequency();
        wanderCount=0;
        wanderSpeed=GetWanderSpeed();
        npc.aggresive = 0;
        
    }
    public override void OnLeaveState()
    {
        base.OnLeaveState();
        npc.isMoving = false;
    }
    public override void Refresh()
    {
        base.Refresh();
        stateCount+=Time.deltaTime;
        if (stateCount >= wanderCount)
        {
            ThinkNextStep();
            return;
        }

        if(Vector2.Distance(nextPos, npc.transform.position) < 0.5f)
        {
            npc.transform.position = nextPos;

            //TODO:Add Wait Time;

            List<Vector2> direction = new List<Vector2>() { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
            int i = npc.rnd.nextInt(0, 4);
            for (int j = 0; j < 4; j++)
            {
                int x = (int)direction[i].x, y = (int)direction[i].y;
                if (Game.Control.mapInfo[(int)(npc.transform.position.x+0.5f)+x, (int)(npc.transform.position.y + 0.5f) + y] < 100)
                {
                    break;
                }
                i = (i + 1) % 4;
            }
            nextPos=new Vector2((int)(npc.transform.position.x + 0.5f) + direction[i].x, (int)(npc.transform.position.y + 0.5f) + direction[i].y);
        }
        else
        {
            if (Game.Control.mapInfo[(int)nextPos.x, (int)nextPos.y] > 100)
            {
                //TODO:Add Path find
                return;
            }
            npc.isMoving = true;
            npc.moveDirection=nextPos-(Vector2)npc.transform.position;
            npc.transform.position = Vector2.MoveTowards(npc.transform.position, nextPos, wanderSpeed * Time.deltaTime);
        }

    }
    void ThinkNextStep()
    {
        float choice = npc.rnd.nextFloat();

        if (choice >= (Game.Control.HazardLevel + 1) / 2)//Select the Safe way
        {
            if (choice <= Game.Control.HazardLevel / 2)
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
    float GetWanderTime()
    {
        return 6 - Game.Control.HazardLevel * 3;
    }
    float GetWanderFrequency()
    {
        return 0.5f-0.3f*Game.Control.HazardLevel;
    }
    float GetWanderSpeed()
    {
        return Game.Control.HazardLevel * 0.3f + 1.1f;
    }

}
