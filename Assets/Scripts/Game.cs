using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using LootLocker.Requests;

public class Game : MonoBehaviour
{
    public static Game Control;

    [Header("Player Status")]
    public GameObject player;
    public int health = 4;
    public int score = 0;
    public int item = 0;
    public bool secondLife = false;
    public bool recovering = false;
    public bool gameOver = false;
    public bool gamePaused = true;
    public Vector2 startPoint = Vector2.zero;

    [SerializeField]
    private float _hazardLevel=0.5f;
    public float HazardLevel // normally should be within [0,1], it will be a hard time when it rises more than 1
    {
        get
        {
            _hazardLevel = item / 5f + score / 3000f;
            return _hazardLevel;
        }
    }
    
    [Header("GameObjects")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public GameObject timerTextGO;
    public TMP_Text itemText;
    public Transform NPCList;
    public GameObject NPC;
    public GameObject door;
    public GameObject[] HP_icon;
    public GameObject cheatBackwall;

    [Header("MapInfo")]
    public int mapHeight;
    public int mapWidth;//Boundary included
    public GameObject objs;
    int initObjNum;
    public int[,] mapInfo;

    [Header("Timer")]
    public float timer = 20;
    int npcTimer = 10;

    [Header("OverHeadText")]
    public GameObject ohtPrefeb;
    public GameObject canvas;

    [Header("LootLockerLeaderboard")]
    public string leaderboardID = "24260";
    bool uploadScore = false;
    bool wait3sec = false;

    private void Awake()
    {
        Control = this;

        //Setting the map size. Should be manually set if need to auto generate map 
        var bound = GameObject.Find("Boundary");
        mapHeight =(int) (bound.transform.Find("H UP").GetChild(0).position.y - bound.transform.Find("H Down").GetChild(0).position.y)-1;
        mapWidth = (int)(bound.transform.Find("V Right").GetChild(0).position.x - bound.transform.Find("V Left").GetChild(0).position.x)-1;
        mapInfo = new int[mapWidth+2,mapHeight+2];
        RefreshMapInfo();
    }

    private void Start()
    {
        startPoint = player.transform.position;
        StartCoroutine("StartTimer");
        if (MusicPlayer.player.cheatMode) { cheatBackwall.SetActive(true); }
    }

    public void resetPlayer()
    {
        if (secondLife) { gameEnd(); return;}
        StartCoroutine("anotherChance");
    }

    IEnumerator anotherChance()
    {
        recovering = true;

        displayOHT("Item -" + (item >= 3 ? 3 : item), player.transform.position);
        updateItem(item >= 3 ? -3 : -item);

        player.SetActive(false);
        timerText.text = "<align=\"center\">LAST\nTRY";
        secondLife = true;
        yield return new WaitForSeconds(1.0f);
        player.transform.position = startPoint;
        yield return new WaitForSeconds(2.0f);
        health = 4;
        updateHealth(0);
        player.SetActive(true);
        recovering = false;
        updateScore(0);
    }

    IEnumerator StartTimer() 
    {
        yield return new WaitForSeconds(1f);
        door.GetComponent<Animator>().SetTrigger("open");
        yield return new WaitForSeconds(2f);
        gamePaused = false;

        //init npc
        Instantiate(NPC, new Vector3(15, 1), NPC.transform.rotation, NPCList).name+=Time.time.ToString("0.0");
        yield return new WaitForSeconds(0.25f);
        Instantiate(NPC, new Vector3(1, 13), NPC.transform.rotation, NPCList).name += Time.time.ToString("0.0");

        while (!gameOver)//NPC generate timer
        {
            yield return new WaitForSeconds(1.0f);
            npcTimer -= 1;
            if (npcTimer <= 0)
            {
                npcTimer = 10;
                Instantiate(NPC,
                            new Vector3(player.transform.position.x >= 8 ? 1 : 15,
                                        player.transform.position.y >= 7 ? 1 : 13),
                            NPC.transform.rotation,
                            NPCList).name += Time.time.ToString("0.0");
            }
        }
    }

    private void Update()
    {
        RefreshMapInfo();

        if (!gameOver)
        {
            timer -=  Time.deltaTime;
            if (!recovering && !gamePaused) timerText.text = timer.ToString("00.000");

            if (timer < 11) timerTextGO.GetComponent<Animator>().SetTrigger("countdown");
                else timerTextGO.GetComponent<Animator>().SetTrigger("normal");

            if (timer <= 0) { timer = 0; gameEnd(); }
        }

        if (gameOver && uploadScore && wait3sec)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(2);
            }
        }

    }

    public void RefreshMapInfo()
    {
        //Reset MapInfo
        for (int i = 0; i <= mapHeight; i++) 
        {
            for (int j = 0; j <= mapWidth; j++)
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
        //Add the Objs
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
        //Add the npcs
        var npcs = GameObject.Find("NPC List").transform;
        for (int i = 0; i < npcs.childCount; i++)
        {
            int x = (int)(npcs.GetChild(i).transform.position.x+0.5f), y = (int)(npcs.GetChild(i).transform.position.y+0.5f);
            mapInfo[(int)x, (int)y] = 10;
        }
    }

    void gameEnd()
    {
        if (timer == 0) timerText.text = "<align=\"center\">TIME'S\nUP"; else timerText.text = "<align=\"center\">GAME\nOVER";
        timerTextGO.GetComponent<Animator>().SetTrigger("end");
        gameOver = true;
        Time.timeScale = 0f;
        StartCoroutine("SubmitScoreRoutine");
    }

    IEnumerator SubmitScoreRoutine() // lootlocker submit score
    {
        if (MusicPlayer.player.cheatMode)
        {
            yield return new WaitForSecondsRealtime(3);
            wait3sec = true;
            uploadScore = true;
            timerText.text = "<align=\"center\">PRESS\nSPACE";
            yield break;
        }

        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, score, leaderboardID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Score uploaded");
                uploadScore = true;
            }
            else
            {
                Debug.Log("Failed to upload scores. " + response.errorData);
                uploadScore = true;
            }
        });

        yield return new WaitWhile(() => uploadScore == false);
        yield return new WaitForSecondsRealtime(3);
        wait3sec = true;
        timerText.text = "<align=\"center\">PRESS\nSPACE";
    }

    public void updateScore(int scoreToAdd)
    { 
        score += scoreToAdd;
        scoreText.text = score.ToString("00000");
    }

    public int updateHealth(int healthToAdd)
    {
        int addedHealth = 0;

        if (health + healthToAdd <= 4) 
        {
            health += healthToAdd; 
            addedHealth = healthToAdd;
        }
        else
        {
            addedHealth = 4 - health;
            health = 4;
        }
        
        for (int i=0; i < 4; i++)
        {
            HP_icon[i].SetActive(i < health);
        }

        return addedHealth;
    }

    public void updateItem(int itemToAdd)
    {
        item += itemToAdd;
        itemText.text = item.ToString("00");
    }

    public void displayOHT(string textToDisplay, Vector3 targetPosition)
    {
        //Vector3 position = Camera.main.WorldToScreenPoint(targetPosition);
        OverHeadText newOHT = Instantiate(ohtPrefeb, canvas.transform).GetComponent<OverHeadText>();
        newOHT.tmpText.text = textToDisplay;
        newOHT.screenPosition = targetPosition;
    }
}
