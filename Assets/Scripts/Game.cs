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

    private void Awake()
    {
        Control = this;
    }

    private void Start()
    {
        startPoint = player.transform.position;
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

    private void Update()
    {
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
