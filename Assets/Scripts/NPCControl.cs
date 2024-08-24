using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Android;

public class NPCContol : MonoBehaviour
{

    List<Vector3> astarPath;
    //Movement vars
    Vector2 destination = new Vector2(0, 0);
    Vector2 moveDirection = new Vector2(0, 0);
    Vector2 faceDirection = new Vector2(0, 0);
    bool canMove = true;
    bool isMoving=false;

    float rayDistance = 1.0f;  // Raycast distance
    LayerMask wallLayer;       // wall layer
    LayerMask boxLayer;        // box layer

    int score = 0;
    int health = 1;
    void Start()
    {
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");
        astarPath= new List<Vector3>();
        
    }

    void Update()
    {

        for (int i = 0; i < astarPath.Count-1; i++)
        {
            Debug.DrawLine(astarPath[i], astarPath[i + 1],Color.red);
        }

        if (!isMoving)
        {
            NPCThought();
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
            transform.position= Vector2.MoveTowards(transform.position, destination, NPC_SceneControl.NPC_Scene.NPC_Speed * Time.deltaTime);

        }

        /*
         //move to destination
        transform.position = Vector2.MoveTowards(transform.position, destination, 5 * Time.deltaTime);
        //check if at destination
        if ((Vector2)transform.position == destination) { isMoving = false; }

        //move logic
        if (canMove && !isMoving)
        {
            moveDirection = Vector2.zero;

            
            if (Vector2.Distance( destination-(Vector2)transform.position,Vector2.right)<0.05)
            {
                moveDirection = Vector2.right;
            }
            else if (Vector2.Distance(destination - (Vector2)transform.position, Vector2.left) < 0.05)
            {
                moveDirection = Vector2.left;
            }
            else if (Vector2.Distance(destination - (Vector2)transform.position, Vector2.up) < 0.05)
            {
                moveDirection = Vector2.up;
            }
            else if (Vector2.Distance(destination - (Vector2)transform.position, Vector2.down) < 0.05)
            {
                moveDirection = Vector2.down;
            }

            //if there is a input
            if (moveDirection != Vector2.zero)
            {
                faceDirection = moveDirection; //for push box checking and future animation

                // Raycast to check the wall
                RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, rayDistance, wallLayer | boxLayer);

                if (hit.collider == null)
                {
                    // no wall, can move
                    destination += moveDirection;
                    canMove = false;
                    isMoving = true;
                    StartCoroutine("moving");
                }
            }
        }
        */
        /*
        //push the box/////////////////////////////////////////////////////////////////////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, faceDirection, rayDistance, boxLayer);

            if (hit.collider != null)
            {
                BoxControl hitObjectControl = hit.collider.gameObject.GetComponent<BoxControl>();
                hitObjectControl.isPushed = true;
                hitObjectControl.moveDirection = faceDirection;
            }
        }
        */
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
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

    private void NPCThought()//敌人思考去哪里
    {
        if (NPC_SceneControl.NPC_Scene.player.activeSelf)
        {
            Vector2Int playerPos = new Vector2Int((int)(NPC_SceneControl.NPC_Scene.player.transform.position.x+0.5), (int)(NPC_SceneControl.NPC_Scene.player.transform.position.y + 0.5));
            destination = AStarPathFind(playerPos);
            return;
        }

        float nearPickupDis = 19260817;
        Vector2Int pickupPos = new Vector2Int();
        
        for (int i = 0; i < NPC_SceneControl.NPC_Scene.obj_trans.childCount; i++) 
        {
            if (NPC_SceneControl.NPC_Scene.obj_trans.GetChild(i).tag == "Pickup")
            {
                if (Vector2.Distance(NPC_SceneControl.NPC_Scene.obj_trans.GetChild(i).position, transform.position) < nearPickupDis)
                {
                    nearPickupDis = Vector2.Distance(NPC_SceneControl.NPC_Scene.obj_trans.GetChild(i).position, transform.position);
                    pickupPos =new Vector2Int((int)(NPC_SceneControl.NPC_Scene.obj_trans.GetChild(i).position.x+0.5), (int)(NPC_SceneControl.NPC_Scene.obj_trans.GetChild(i).position.y + 0.5)) ;
                }
            }
        }

        if (nearPickupDis < 19260817)//证明找到了pickup
        {
            destination = AStarPathFind(pickupPos);
            return;
        }

        int d = Random.Range(0, 4);
        List<Vector2Int> direction = new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        destination = new Vector2Int((int)(transform.position.x+0.5), (int)(transform.position.y + 0.5)) + direction[d];
    }

    private class Node
    {
        public int f,g,h;
        
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
    private Vector2 AStarPathFind(Vector2Int targetPos)
    {
        Node targetNode = new Node(targetPos.x,targetPos.y);
        targetNode.parent = null;

        List<Node>openList = new List<Node>();  
        List<Node> closeList = new List<Node>();
        //Start Searching
        Node startNode = new Node((int)(transform.position.x + 0.5), (int)(transform.position.y + 0.5));
        openList.Add(new Node(startNode.pos.x, startNode.pos.y));
        List<Vector2Int> direction=new List<Vector2Int>() { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        for(int i = 0;i<4;i++)//Add pos around curPos
        {
            int x = startNode.pos.x + direction[i].x,y= startNode.pos.y+direction[i].y;
            if (x == 0 || x == 16 || y == 0 || y == 16 )
            {
                continue;
            }
            else if (NPC_SceneControl.NPC_Scene.MapInfo[x,y]>0)
            {
                continue;
            }
            Node node= new Node(x, y, startNode);
            node.g = 1;
            node.f = node.g + Node.HValue(node,targetNode);
            openList.Add(node);
        }
        closeList.Add(startNode);
        openList.Remove(startNode);

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
                Node nextNode=new Node(x, y, node);
                nextNode.g = node.f+1;

                if (x == 0 || x == 16 || y == 0 || y == 16)//检测是否到达边界
                {
                    continue;
                }
                else if (NPC_SceneControl.NPC_Scene.MapInfo[x, y] > 0)
                {
                    continue;
                }

                if (nextNode.pos == targetNode.pos)//检测是否找到终点
                {
                    targetNode.parent = node;
                    findTargetFlag = true;
                    break;
                }

                bool flag = false;
                for (int j = 0; j < closeList.Count; j++)//检测相邻节点是否在close中
                {
                    if (closeList[j].pos == nextNode.pos)
                    {
                        flag = true;
                        break;
                    }
                }
                if(flag)continue;
                flag = false;

                for (int j = 0; j < openList.Count; j++)//检测是否有相邻节点在open中
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
            Debug.LogError($"{transform.name}未能找到通路！");
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

        Debug.Log(log);
        return replayNode.pos;
    }
}
