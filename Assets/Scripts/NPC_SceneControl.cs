using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_SceneControl : MonoBehaviour
{
    GameObject player;
    public GameObject box;
    public Transform obj_trans;
    public int box_Num = 5;

    List <GameObject> NPCs;
    public int NPC_Num = 3;
    public float NPC_Speed=2;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        

    }

    // Update is called once per frame
    void Update()
    {
    }


    public void GenerateBox()
    {
        //Clear Objs(Box and item?) in scene 
        int ori_num = obj_trans.childCount;
        for (int i = 0; i < ori_num; i++)
        {
            Destroy(obj_trans.GetChild(0));
        }

        for (int i = 0; i < box_Num; i++) 
        {
            GameObject new_box= GameObject.Instantiate(box,obj_trans);

        }
    }

}
