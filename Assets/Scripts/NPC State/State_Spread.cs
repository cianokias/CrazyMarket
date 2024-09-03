using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class State_Spread : NPCState
{
    List<GameObject> checkouts;
    float stateCount;
    float spreadTime;
    int spreadRadius;
    float spreadSpeed;
    Vector2 checkoutPos;
    Vector2 spreadPos;
    Vector2 nextPos;


    public override void Init(NPCContol n, object args = null)
    {
        base.Init(n, args);
        type = NPCStateType.Spread;
    }
    public override void OnEnterState(NPCStateType lastState, object args = null)
    {
        base.OnEnterState(lastState, args);
        Debug.Log($"{npc.transform.name} change to {type} state");
        checkouts = new List<GameObject>();
        var co=GameObject.FindGameObjectsWithTag("Checkout");
        foreach (var c in co) 
        {
            checkouts.Add(c);
        }
        npc.aggresive = 0;
        stateCount= 0;
        spreadTime= GetSpreadTime();
        spreadRadius= GetSpreadRadius();
        spreadSpeed=GetSpeed();

        float checkoutDis = 999;
        GameObject nearCheckout = null;
        foreach (var c in checkouts) 
        {
            if (Vector2.Distance(c.transform.position, npc.transform.position) < checkoutDis)
            {
                checkoutDis = Vector2.Distance(c.transform.position, npc.transform.position);
                nearCheckout = c;
            }
        }
        checkoutPos=nearCheckout.transform.position;

        Randomer rnd = new Randomer();
        int y = rnd.nextInt(-spreadRadius, spreadRadius + 1);
        int x = rnd.nextInt(-(spreadRadius - Mathf.Abs(y)), (spreadRadius - Mathf.Abs(y) + 1));
        int oriID = XYToDid(spreadRadius, x, y);
        int id = oriID;
        while ((x + (int)(checkoutPos.x + 0.5f) <= 0 || (y + (int)(checkoutPos.y + 0.5f)) <= 0) || (x + (int)(checkoutPos.x + 0.5f) > Game.Control.mapWidth || (y + (int)(checkoutPos.y + 0.5f)) > Game.Control.mapHeight)//if out of border
           || (Game.Control.mapInfo[x + (int)(checkoutPos.x + 0.5f), y + (int)(checkoutPos.y + 0.5f)] > 100))//if position blocked
        {
            if (id == oriID - 1)
            {
                Debug.Log($"{npc.transform.name} not found space in spread state");
                break;
            }
            id = (id + 1) % ((2 * spreadRadius * spreadRadius) + 2 * spreadRadius + 1);
            var pos = DidToXY(spreadRadius, id);
            x = (int)(pos.x); y = (int)(pos.y);
        }
        spreadPos = new Vector2(x, y);
        nextPos = npc.AStarPathFind(spreadPos + checkoutPos);

    }
    public override void OnLeaveState()
    {
        base.OnLeaveState();
    }
    public override void Refresh()
    {
        base.Refresh();

        stateCount += Time.deltaTime;
        if (stateCount >= spreadTime)
        {
            ThinkNextStep();
        }

        if (Vector2.Distance(npc.transform.position, nextPos) < 0.05f)//����Ƿ񵽴��յ�
        {
            npc.transform.position = nextPos;
            if (nextPos == (checkoutPos + spreadPos))//npc has arrived
            {
                Randomer rnd = new Randomer();
                int y = rnd.nextInt(-spreadRadius, spreadRadius + 1);
                int x = rnd.nextInt(-(spreadRadius - Mathf.Abs(y)), (spreadRadius - Mathf.Abs(y) + 1));
                int oriID = XYToDid(spreadRadius, x, y);
                int id = oriID;
                while ((x + (int)(checkoutPos.x + 0.5f) <= 0 || (y + (int)(checkoutPos.y + 0.5f)) <= 0) || (x + (int)(checkoutPos.x + 0.5f)> Game.Control.mapWidth  || (y + (int)(checkoutPos.y + 0.5f)) > Game.Control.mapHeight)//if out of border
                   || (Game.Control.mapInfo[x + (int)(checkoutPos.x + 0.5f), y + (int)(checkoutPos.y + 0.5f)] > 100))//if position blocked
                {
                    if (id == oriID - 1)
                    {
                        Debug.Log($"{npc.transform.name} not found space in spread state");
                        break;
                    }
                    id = (id + 1) % ((2 * spreadRadius * spreadRadius) + 2 * spreadRadius + 1);
                    var pos = DidToXY(spreadRadius, id);
                    x = (int)(pos.x); y = (int)(pos.y);
                }
                spreadPos = new Vector2(x, y);
                nextPos = npc.AStarPathFind(spreadPos + checkoutPos);
            }

            nextPos = npc.AStarPathFind(spreadPos +checkoutPos);
        }
        else
        {
            if (Game.Control.mapInfo[(int)nextPos.x, (int)nextPos.y] > 100)
                return;
            npc.transform.position = Vector2.MoveTowards(npc.transform.position, nextPos, spreadSpeed * Time.deltaTime);
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
    float GetSpreadTime()
    {
        return 7 - Game.Control.HazardLevel * 3;
    }
    int GetSpreadRadius()
    {

        if (Game.Control.HazardLevel < 0.6f)
        {
            return 5;
        }
        else if (Game.Control.HazardLevel < 0.8f)
        {
            return 4;
        }
        else
        {
            return 3;
        }
    }
    float GetSpeed()
    {
        return 1f+Game.Control.HazardLevel * 0.4f;
    }
    Vector2 DidToXY(int radius, int id)
    {
        int count = -1;
        Vector2 xy = new Vector2();
        if (id <= (radius + 1) * (radius + 1) - 1)
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
            count = (radius + 1) * (radius + 1) - 1;
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
            id += radius - y + x;
        }
        else
        {
            id = (radius + 1) * (radius + 1);
            id += radius * radius - (radius + y + 1) * (radius + y + 1);
            id += radius + y + x;
        }

        return id;
    }

}