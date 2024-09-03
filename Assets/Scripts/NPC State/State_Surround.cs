using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class State_Surround : NPCState
{
    float speed;
    int surroundRadius;
    float surroundTime;
    float counter;
    float aggresiveIndex = 0.8f;
    Vector2 SurroundPos;
    Vector2 nextPos;

    public override void Init(NPCContol n, object args = null)
    {
        base.Init(n, args);
        type=NPCStateType.Surround;
      
    }
    public override void OnEnterState(NPCStateType lastState, object args = null)
    {
        base.OnEnterState(lastState, args);
        Debug.Log($"{npc.transform.name} changes to {type} state");
        speed = GetSurroundSpeed();
        surroundRadius = GetRadius();
        surroundTime =
        counter = 0;
        npc.aggresive += aggresiveIndex * (1 - Game.Control.HazardLevel > 0 ? (1 - Game.Control.HazardLevel) : 0);
        Randomer rnd = new Randomer();
        int y=rnd.nextInt(-surroundRadius,surroundRadius+1);
        int x = rnd.nextInt(-(surroundRadius-Mathf.Abs(y)), (surroundRadius - Mathf.Abs(y)+1));
        int oriID = XYToDid(surroundRadius, x, y);
        while ((x + (int)(Game.Control.player.transform.position.x + 0.5f) <= 0 || (y + (int)(Game.Control.player.transform.position.y + 0.5f)) <= 0) || (x + (int)(Game.Control.player.transform.position.x + 0.5f) > Game.Control.mapWidth || (y + (int)(Game.Control.player.transform.position.y + 0.5f)) > Game.Control.mapHeight)//if out of border
        || (Game.Control.mapInfo[x + (int)(Game.Control.player.transform.position.x + 0.5f), y + (int)(Game.Control.player.transform.position.y + 0.5f)] > 100))//if position blocked
        {
            int id=XYToDid(surroundRadius,x,y);
            if (id == oriID - 1) 
            {
                Debug.Log($"{npc.transform.name} not found space in surround state");
                break;
            }
            id=(id+1)%((2*surroundRadius*surroundRadius)+2*surroundRadius+1);
            var pos = DidToXY(surroundRadius, id);
            x=(int)(pos.x); y=(int)(pos.y);
        }
        SurroundPos=new Vector2(x,y);
        nextPos = npc.AStarPathFind(SurroundPos+(Vector2)Game.Control.player.transform.position);

    }
    public override void OnLeaveState()
    {
        base.OnLeaveState();
    }
    public override void Refresh()
    {
        base.Refresh();
        counter += Time.deltaTime;
        if (counter >= surroundTime)
        {
            ThinkNextStep();
        }

        if (Vector2.Distance(Game.Control.player.transform.position, npc.transform.position) < surroundRadius / 2)
        {
            Vector2 toVec=Game.Control.player.transform.position- npc.transform.position;
            toVec += Game.Control.player.GetComponent<PlayerControl>().moveDirection;
            toVec = -toVec.normalized;
            List<Vector2> direction=new List<Vector2>() {Vector2.down,Vector2.up,Vector2.left,Vector2.right };
            float angle = 999;
            Vector2 nextDir = new Vector2();
            foreach (var vec in direction)
            {
                if (Vector2.Angle(toVec, vec) < angle)
                {
                    angle=Vector2.Angle(toVec, vec);
                    nextDir = vec;
                }
            }
            nextPos= Vector2Int.FloorToInt(npc.transform.position)+nextDir;
        }
        

        if (Vector2.Distance(npc.transform.position, nextPos) < 0.05f)//����Ƿ񵽴��յ�
        {
            npc.transform.position = nextPos;
            nextPos = npc.AStarPathFind(SurroundPos + (Vector2)Game.Control.player.transform.position);
        }
        else
        {
            if (Game.Control.mapInfo[(int)nextPos.x, (int)nextPos.y] > 100)
                return;
            npc.transform.position = Vector2.MoveTowards(npc.transform.position, nextPos, speed * Time.deltaTime);
        }

    }

    void ThinkNextStep()
    {

        Randomer rnd = new Randomer();
        float choice = rnd.nextFloat();

        if (choice < Game.Control.HazardLevel)//Select the Safe way
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
            if (choice <= npc.aggresive)
            {
                npc.ChangeState(this, NPCStateType.Chase);
            }
            else
            {
                npc.ChangeState(this, NPCStateType.Surround);
            }
        }
    }

    int GetRadius()
    {
        if (Game.Control.HazardLevel < 0.6f)
        {
            return 3;
        }
        else if (Game.Control.HazardLevel<0.8f)
        {
            return 2;
        }
        else
        {
            return 1;
        }

    }
    float GetSurroundSpeed()
    {
        return Game.Control.HazardLevel * 0.4f + 1.2f;
    }
    float GetSurroundTime()
    {
        return Game.Control.HazardLevel * 4f + 3f;
    }


    Vector2 DidToXY(int radius, int id)
    {
        int count = -1;
        Vector2 xy = new Vector2();
        if (id <= (radius+1) * (radius+1)-1)
        {
            for (int y = radius; y >= 0; y--)
            {
                for (int x = -(radius - y); x <= (radius - y); x++)
                {
                    count++;
                    if (count == id)
                    {
                        xy.x = x;
                        xy.y = y;
                        return xy;
                    }
                }
            }
        }
        else
        {
            count = (radius + 1) * (radius + 1)-1;
            for (int y = -1; y >= -radius; y--)
            {
                for (int x = -(radius + y); x <= (radius + y); x++)
                {
                    count++;
                    if (count == id)
                    {
                        xy.x = x;
                        xy.y = y;
                        return xy;
                    }
                }
            }
        }
        Debug.LogError($"{id} not found in diamond area{radius}!");
        return xy;
    }
    int XYToDid(int radius, int x, int y)
    {
        int id = 0;
        if (y >= 0)
        {
            id += (radius - y) * (radius - y) > 0 ? (radius - y) * (radius - y) : 0;
            id += radius - y + x ;
        }
        else
        {
            id = (radius + 1) * (radius + 1);
            id += radius * radius - (radius + y + 1) * (radius + y + 1);
            id += radius + y + x ;
        }

        return id;
    }
}