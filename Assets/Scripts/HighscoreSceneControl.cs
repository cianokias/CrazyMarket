using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using TMPro;

public class HighScoreSceneControl : MonoBehaviour
{
    public TMP_Text playerNames;
    public TMP_Text playerScores;
    public TMP_Text yourScores;

    public GameObject door;
    public GameObject restartText;

    string leaderboardID = "24260";
    bool wait3sec = false;

    void Start()
    {
        StartCoroutine("GetHighScores");
    }

    IEnumerator GetHighScores()
    {
        if (!LootLockerSDKManager.CheckInitialized())
        {
            StartCoroutine("LoginRoutine");
            yield return new WaitForSeconds(1);
        }

        bool done = false;
        LootLockerSDKManager.GetScoreList(leaderboardID,10, (response) =>
        {
            if (response.success) 
            {
                string tempPlayerNames = "Name\n";
                string tempPlayerScores = "Score\n";

                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    
                    if (i == 0) { tempPlayerNames += "<color=#FFD700><b><size=+10>"; tempPlayerScores += "<color=#FFD700><b><size=+10>"; }
                    if (i == 1) { tempPlayerNames += "<color=#FFFFFF><size=+5>"; tempPlayerScores += "<color=#FFFFFF><size=+5>"; }
                    if (i == 2) { tempPlayerNames += "<color=#CD7F32><size=+3>"; tempPlayerScores += "<color=#CD7F32><size=+3>"; }
                    if (i == 3) { tempPlayerNames += "<color=#666666></b><size=100%>"; tempPlayerScores += "<color=#666666></b><size=100%>"; }

                    if (members[i].player.id.ToString() == PlayerPrefs.GetString("PlayerID"))
                    {
                        tempPlayerNames += "<u>"; tempPlayerScores += "<u>";
                    }

                    if (members[i].player.name != "")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score + "\n</u>";
                    tempPlayerNames += "\n</u>";
                }
                done = true;

                playerNames.text = tempPlayerNames;
                playerScores.text = tempPlayerScores;
                if (MusicPlayer.player.cheatMode)
                {
                    yourScores.text = "Cheat mode active: your score won't count.";
                }
                else if (Game.Control.score <= members[members.Length - 1].score)
                {
                    yourScores.text = "YOUR SCORE: " + Game.Control.score;
                }
                else
                {
                    yourScores.text = " ";
                }
            }
            else
            {
                Debug.Log("Failed to get the leaderboard. " + response.errorData);
                yourScores.text = "Failed to get the leaderboard.";
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
        yield return new WaitForSecondsRealtime(3);
        wait3sec = true;
        restartText.SetActive(true);
    }

    IEnumerator LoginRoutine() //lootlocker login
    {
        bool done = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Player logged in");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                done = true;
            }
            else
            {
                Debug.Log("Player login failed");
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && wait3sec)
        {
            StartCoroutine(loadMainScene());
        }
    }

    IEnumerator loadMainScene()
    {
        door.SetActive(true);
        yield return new WaitForSecondsRealtime(1.2f);
        SceneManager.LoadScene(1);
    }
}
