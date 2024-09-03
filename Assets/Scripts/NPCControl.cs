using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Android;

public class NPCContol : MonoBehaviour
{
    public float speed = 1.5f;
    public int score = 0;
    public float aggresive;
    public Randomer rnd;
    List<Vector3> astarPath;
    //Movement vars
    Vector2 destination = new Vector2(0, 0);
    
    //Vector2 faceDirection = new Vector2(0, 0);

    public bool canMove = true;
    public Vector2 moveDirection = new Vector2(0, 0);
    Animator anim;

    public bool isMoving=false;
    bool blocked = false;

    float rayDistance = 1.0f;  // Raycast distance
    LayerMask wallLayer;       // wall layer
    LayerMask boxLayer;        // box layer

    int[,] _mapinfo = null;
    int[,] MapInfo
    {
        get
        {
            if (_mapinfo is null)
            {
                if (Game.Control is null)
                {
                    _mapinfo= GameObject.Find("NPC_SceneControl").GetComponent<NPC_SceneControl>().MapInfo;
                }
                else
                {
                    _mapinfo= Game.Control.mapInfo;
                }
            }
                return _mapinfo;
        }
    }

    GameObject _playerGO;
    GameObject PlayerGO
    {
        get
        {
            if (_playerGO == null)
            {
                if(Game.Control is not null)
                {
                    _playerGO=Game.Control.player; 
                }
                else
                {
                    _playerGO = GameObject.Find("Player");
                }
            }
            return _playerGO;

        }
    }

    GameObject _obj;
    GameObject Obj
    {
        get
        {
            if(_obj == null)
            {
                if( Game.Control is not null)
                {
                    _obj = Game.Control.objs;
                }
                else
                {
                    _obj = GameObject.Find("Obj");
                }
            }
            return _obj;
        }
    }

    SpriteRenderer sr;

    NPCState curState = null;
    List<NPCState> states = new List<NPCState>() {new State_Stay(),new State_Chase(),new State_Spread(),new State_Surround(),new State_Wander() };
    public NPCStateType curType;
    void Start()
    {
        if (states.Count<=0)
        {
            Debug.LogError($"{transform.name} has no state, and will not work!");
            return;
        }

        foreach (var state in states)
        {
            state.Init(this);
        }
        rnd = new Randomer();
        curState = states[0];
        ChangeState(curState,NPCStateType.Stay);
        
        speed = speed +( Random.value-0.5f)*1.5f;
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");
        astarPath= new List<Vector3>();
        sr = GetComponent<SpriteRenderer>();
        anim=GetComponent<Animator>();
    }

    void Update()
    {
        anim.SetBool("isMoving",isMoving);
        anim.SetInteger("direction",VectorToDir(moveDirection));

        //update sortingorder
        sr.sortingOrder = (14 - (int)transform.position.y) * 2 + 1;

        curType = curState.type;
        curState.Refresh();

        for (int i = 0; i < astarPath.Count - 1; i++)
        {
            Debug.DrawLine(astarPath[i], astarPath[i + 1], Color.red);
        }
        /*
        for (int i = 0; i < astarPath.Count-1; i++)
        {
            Debug.DrawLine(astarPath[i], astarPath[i + 1],Color.red);
        }

        if (!isMoving)
        {
            NPCThought();
            sr.sortingOrder = (14 - (int)transform.position.y) * 2 + 1;
        }

        if (canMove)
        {
            if (Vector2.Distance(transform.position, destination) < 0.05f)//检测是否到达终点
            {
                transform.position = destination;
                isMoving = false;
                return;
            }
            isMoving = true;
            transform.position= Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        }
        */
        /*        if (health<=0)
                {
                    Die();
                }*/

    }

    int VectorToDir(Vector2 vec)
    {
        int dir=0;
        vec= vec.normalized;
        List<Vector2>directions=new List<Vector2>() { Vector2.down,Vector2.right,Vector2.up,Vector2.left};
        float dis = 999;
        for (int i = 0; i < directions.Count; i++) 
        {
            if (Vector2.Distance(directions[i], vec) < dis)
            {
                dis = Vector2.Distance(directions[i], vec);
                dir= i;
            }
         }
        return dir;
    }

    IEnumerator stoping()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("End Stopping");
        isMoving = false;
    }

    IEnumerator moving()
    {
        yield return new WaitForSeconds(0.2f); //0.2f = time used to move 1 step
        canMove = true;
    }

    //prevent player stuck in the wall
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isMoving)
        {
            destination = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pickup")
        {
            Destroy(collision.gameObject);
            score += 100;
            Debug.Log("Score: " + score);
        }
    }

/*    private void Die()
    {
        Game.Control.updateScore(200); // TODO: kill more gain more
        Destroy(gameObject);
    }*/

    private void NPCThought()//敌人思考去哪里
    {
        List<Vector2Int> direction = new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        if (blocked)//check if is not blocked anymore
        {
            for (int i = 0; i < 4; i++) 
            {
                var checkpos = new Vector2Int((int)(transform.position.x + 0.5), (int)(transform.position.y + 0.5)) + direction[i];
                if (MapInfo[checkpos.x, checkpos.y] == 0)
                {
                    blocked = false;
                    break;
                }
            }
        }

        if (blocked)//if blocked, abandon all thoughts and become Kazi Sama
            return;

        //if player alive, chase player
        if (PlayerGO.activeSelf)
        {
            Vector2Int playerPos = new Vector2Int((int)(PlayerGO.transform.position.x+0.5), (int)(PlayerGO.transform.position.y + 0.5));
            destination = AStarPathFind(playerPos);
            return;
        }

        //if no player, find nearest pickup
        float nearPickupDis = 19260817;
        Vector2Int pickupPos = new Vector2Int();
        
        for (int i = 0; i < Obj.transform.childCount; i++) 
        {
            if (Obj.transform.GetChild(i).tag == "Pickup")
            {
                if (Vector2.Distance(Obj.transform.GetChild(i).position, transform.position) < nearPickupDis)
                {
                    nearPickupDis = Vector2.Distance(Obj.transform.GetChild(i).position, transform.position);
                    pickupPos =new Vector2Int((int)(Obj.transform.GetChild(i).position.x+0.5), (int)(Obj.transform.GetChild(i).position.y + 0.5)) ;
                }
            }
        }

        if (nearPickupDis < 19260817)//证明找到了pickup
        {
            destination = AStarPathFind(pickupPos);
            return;
        }

        //if nothing to chase, randomly move around
        int d = Random.Range(0, 4);
        
        destination = new Vector2Int((int)(transform.position.x+0.5), (int)(transform.position.y + 0.5)) + direction[d];
        int loop = 0;
        while (MapInfo[(int)destination.x, (int)destination.y] >0 && loop < 6)
        {
            loop++;
            d = (d + 1) % 4;
            destination = new Vector2Int((int)(transform.position.x + 0.5), (int)(transform.position.y + 0.5)) + direction[d];
        }
        if (loop >= 6)
        {
            destination = transform.position;
            blocked = false;
        }
    }

    public void ChangeState(NPCState nowState, NPCStateType nextStateType, object args = null)
    {
        nowState.OnLeaveState();
        foreach(var state in states)
        {
            if (state.type == nextStateType)
            {
                state.OnEnterState(nextStateType,args);
                curState= state;
                return;
            }
        }
        Debug.LogError($"{transform.name}未找到状态{nextStateType}");
    }

    private class Node
    {
        public int f,g;
        
        public Vector2Int pos;
        public Node parent;
        
        public Node(int x,int y)
        {
            pos = new Vector2Int(x,y);
        }
        public Node(int x, int y, Node parent)
        {
            pos = new Vector2Int(x, y);
            this.parent = parent;
        }
        public static int HValue(Node n1,Node n2)
        {
            return Mathf.Abs((n1.pos - n2.pos).x)+Mathf.Abs((n1.pos - n2.pos).y);
        }
    }
    public Vector2 AStarPathFind(Vector2 inputPos)
    {
        Vector2Int targetPos = new Vector2Int((int)(inputPos.x+0.5f), (int)(inputPos.y + 0.5f));
        Node targetNode = new Node(targetPos.x,targetPos.y);
        targetNode.parent = null;

        List<Node>openList = new List<Node>();  
        List<Node> closeList = new List<Node>();
        //Start Searching
        Node startNode = new Node((int)(transform.position.x + 0.5), (int)(transform.position.y + 0.5));
        openList.Add(new Node(startNode.pos.x, startNode.pos.y));
        List<Vector2Int> direction=new List<Vector2Int>() { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        //Path Sorting
        for (int findTime = 0; openList.Count > 0; findTime++)
        {
            bool findTargetFlag = false;
            Node node=openList[0];
            for (int i = 0; i < openList.Count; i++)//选择f最小的节点
            {
                if (openList[i].f < node.f)
                {
                    node = openList[i];
                }
            }

            for (int i = 0; i < 4; i++)
            {
                int x = node.pos.x + direction[i].x, y = node.pos.y + direction[i].y;

                if (x <= 0 || x > Game.Control.mapWidth || y <= 0 || y >Game.Control.mapHeight)//检测是否到达边界
                {
                    continue;
                }
                else if (MapInfo[x, y] > 100)//检测是否有障碍物
                {
                    //By removing this line, NPC will regard box as movable.
                    //continue;
                }

                Node nextNode=new Node(x, y, node);
                nextNode.g = node.f+MapInfo[x,y]+1;                

                if (nextNode.pos == targetNode.pos)//检测是否找到终点
                {
                    targetNode.parent = node;
                    findTargetFlag = true;
                    break;
                }

                bool flag = false;
                for (int j = 0; j < closeList.Count; j++)//检测该节点是否在close中
                {
                    if (closeList[j].pos == nextNode.pos)
                    {
                        flag = true;
                        break;
                    }
                }
                if(flag)continue;

                flag = false;
                for (int j = 0; j < openList.Count; j++)//检测该节点是否已存在于open中
                {
                    if (openList[j].pos == nextNode.pos)
                    {
                        flag= true;
                        if (nextNode.g < openList[j].g)
                        {
                            openList[j].f = nextNode.g + Node.HValue(openList[j],targetNode);
                            openList[j].g = nextNode.g;
                            openList[j].parent = node;
                        }
                    }
                }
                if (flag)continue;

                nextNode.f=nextNode.g+Node.HValue(nextNode,targetNode);
                openList.Add(nextNode);
            }

            if (findTargetFlag)
            {
                break;
            }

            openList.Remove(node);
            closeList.Add(node);
        }

        if(targetNode.parent is null)
        {
            Debug.LogError($"{transform.name}未能找到到达{targetNode.pos}的通路！");
            return startNode.pos;
        }

        var replayNode = targetNode;
        string log = $"{transform.name}的寻路结果如下：";
        astarPath.Clear();
        astarPath.Add(new Vector3(replayNode.pos.x, replayNode.pos.y));
        while (replayNode.parent.pos != startNode.pos)
        {
            astarPath.Add(new Vector3(replayNode.pos.x, replayNode.pos.y));
            log += $"{replayNode.parent.pos}->";
            replayNode = replayNode.parent;
        }
        astarPath.Add(new Vector3(replayNode.pos.x, replayNode.pos.y));
        astarPath.Add(new Vector3(startNode.pos.x, startNode.pos.y));
        return replayNode.pos;
    }
}
