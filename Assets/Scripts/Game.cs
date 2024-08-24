using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game Control;

    [Header("Player Status")]
    public GameObject player;
    public int health = 4;
    public int score = 0;
    public bool secondLife = false;
    public bool recovering = false;
    public bool gameOver = false;
    public Vector2 startPoint = Vector2.zero;

    [Header("GameObjects")]
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public TMP_Text timerText;

    int timer = 60;

    [Header("Map Info")]
    public int[,] mapInfo;
    public int mapHeight, mapWidth;//Boundary included
    public GameObject objs;

    private void Awake()
    {
        Control = this;
        //Setting the map size. Should be manually set if need to auto generate map 
        var bound = GameObject.Find("Boundary");
        mapHeight =(int) (bound.transform.Find("H UP").GetChild(0).position.y - bound.transform.Find("H Down").GetChild(0).position.y)+1;
        mapWidth = (int)(bound.transform.Find("V Right").GetChild(0).position.x - bound.transform.Find("V Left").GetChild(0).position.x)+1;
        mapInfo = new int[mapWidth,mapHeight];
        RefreshMapInfo();
    }

    private void Start()
    {
        startPoint = player.transform.position;
        StartCoroutine("StartTimer");
    }

    public void resetPlayer()
    {
        if (secondLife) { gameEnd(); return;}
        StartCoroutine("anotherChance");
    }

    IEnumerator anotherChance()
    {
        recovering = true;
        player.SetActive(false);
        health = 4;
        healthText.text = "HP " + health.ToString();
        scoreText.text = "LAST CHANCE";
        secondLife = true;
        yield return new WaitForSeconds(1.0f);
        player.transform.position = startPoint;
        yield return new WaitForSeconds(2.0f);
        player.SetActive(true);
        recovering = false;
        updateScore(0);
    }

    IEnumerator StartTimer()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(1.0f);
            timer -= 1;
            timerText.text = timer.ToString("00");
            if (timer <= 0) { gameEnd(); }
        }
    }

    private void Update()
    {
        RefreshMapInfo();
        //test only, press F to HP--
        if (Input.GetKeyDown(KeyCode.F))
        {
            health -= 1;
            healthText.text = "HP " + health.ToString();
            if (health == 0)
            {
                resetPlayer();
            }
        }//

        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(0);
            }
        }

    }

    void RefreshMapInfo()
    {
        //Reset MapInfo
        for (int i = 0; i < mapHeight; i++) 
        {
            for (int j = 0; j < mapWidth; j++)
            {
                mapInfo[j, i] = 0;
            }
        }
        //Add the Boundary
        var boundary = GameObject.Find("Boundary").transform;
        for (int i = 0;i < boundary.childCount; i++)
        {
            for(int j = 0; j < boundary.GetChild(i).childCount; j++)
            {
                int x = (int)boundary.GetChild(i).GetChild(j).transform.position.x, y = (int)boundary.GetChild(i).GetChild(j).transform.position.y;
                mapInfo[x, y] = 19260817;
            }
            
        }

        var objs = GameObject.Find("Objects").transform;
        for (int i = 0; i < objs.childCount; i++)
        {
            int x = (int)objs.GetChild(i).transform.position.x, y = (int)objs.GetChild(i).transform.position.y;
            if (objs.GetChild(i).tag == "Box")
            {
                mapInfo[x, y] = 114;
            }
            else if (objs.GetChild(i).tag == "Pickup")
            {
                mapInfo[x, y] = 0;
            }
            
        }

    }

    void gameEnd()
    {
        healthText.text = "GAME OVER";
        gameOver = true;
        Time.timeScale = 0f;
    }

    public void updateScore(int scoreToAdd)
    { 
        score += scoreToAdd;
        scoreText.text = "SCORE " + score.ToString("00000");
    }
}
