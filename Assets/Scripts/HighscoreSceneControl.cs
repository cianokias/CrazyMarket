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

    string leaderboardID = "24260";

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

                    if (members[i].player.name != "")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score + "\n";
                    tempPlayerNames += "\n";
                }
                done = true;

                playerNames.text = tempPlayerNames;
                playerScores.text = tempPlayerScores;
            }
            else
            {
                Debug.Log("Failed to get the leaderboard. " + response.errorData);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
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

        yield return new WaitWhile(() => done = false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(1);
        }
    }
}
