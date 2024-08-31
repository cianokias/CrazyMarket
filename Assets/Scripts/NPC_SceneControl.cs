using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_SceneControl : MonoBehaviour
{
    public static NPC_SceneControl NPC_Scene;
    public GameObject player;
    public GameObject box;
    public Transform obj_trans;
    public int box_Num = 5;

    List <GameObject> NPCs;
    public int NPC_Num = 3;
    public float NPC_Speed=2;

    public int[,] MapInfo;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        MapInfo=new int[17,17];
        RefreshMapInfo();
        NPC_Scene = this;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateBox();
        }
        RefreshMapInfo();
    }


    public void GenerateBox()
    {
        //Clear Objs(Box and item?) in scene 
        int ori_num = obj_trans.childCount;
        for (int i = 0; i < ori_num; i++)
        {
            Destroy(obj_trans.GetChild(i).gameObject);
        }

        //Generate New box
        for (int i = 0; i < box_Num; i++) 
        {
            GameObject new_box= GameObject.Instantiate(box,obj_trans);
            new_box.transform.position = new Vector2((int)(Random.value*13+1.5),(int)(Random.value*13+1.5));

        }
    }
    public void RefreshMapInfo()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j <16; j++)
            {
                if (i == 0 || i == 15 || j == 0 || j == 15)
                {
                    MapInfo[i, j] = 19260817;
                    continue;
                }
                MapInfo[i, j] = 0;
            }
        }

        for (int i = 0;i < obj_trans.childCount; i++)
        {
            var item= obj_trans.GetChild(i);
            int x=(int)item.position.x,y=(int)item.position.y;
            if (item.tag == "Box")
            {
                MapInfo[x, y] = 114;
            }
        }
        var npcList = GameObject.Find("NPCList");
        for (int i = 0; i < npcList.transform.childCount; i++)
        {
            int x=(int)(npcList.transform.GetChild(i).position.x+0.5f);
            int y = (int)(npcList.transform.GetChild(i).position.y + 0.5f);
            MapInfo[x, y] = 8;
        }
    }
}
