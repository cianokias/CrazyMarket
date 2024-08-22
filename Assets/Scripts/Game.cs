using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Control;

    [Header("Player Status")]
    public GameObject player;
    public int health = 4;
    public bool secondLife = false;
    public bool recovering = false;

    private void Awake()
    {
        Control = this;
    }

    public void resetPlayer()
    {
        if (secondLife) { gameOver(); return;}
        StartCoroutine("anotherChance");
    }

    IEnumerator anotherChance()
    {
        recovering = true;
        player.SetActive(false);
        health = 4;
        secondLife = true;
        yield return new WaitForSeconds(1.0f);
        player.transform.position = new Vector2(0,0);
        yield return new WaitForSeconds(2.0f);
        player.SetActive(true);
        recovering = false;
    }

    //test only
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            health -=1;
            if (health == 0)
            {
                resetPlayer();
            }
        }
    }

    void gameOver()
    {
        //todo
    }

}
