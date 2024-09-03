using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LootLocker.Requests;
using TMPro;

public class StartSceneControl : MonoBehaviour
{
    public TMP_Text tipsText;
    public TMP_InputField nameInputField;
    public GameObject nameInputGO;

    public GameObject blackscreen;
    public GameObject startscreen1;
    public GameObject tips2;

    bool isLogin = false;
    bool triggerOnce = true;
    bool readyToStart = false;

    void Start()
    {
        if (!LootLockerSDKManager.CheckInitialized())
            StartCoroutine("LoginRoutine");
        else
            isLogin = true;
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
        isLogin = true;
    }

    void Update()
    {
        if (isLogin && triggerOnce)
        {
            triggerOnce = false;
            tipsText.text = " ";
            nameInputGO.SetActive(true);
            blackscreen.GetComponent<Animator>().SetTrigger("blackout");
        }

        if (readyToStart && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(1);
        }

    }

    public void SetPlayerName()
    {
        tipsText.text = " ";
        nameInputGO.SetActive(false);
        tips2.SetActive(true);
        StartCoroutine("setName");
    }

    IEnumerator setName()
    {
        bool done = false;
        LootLockerSDKManager.SetPlayerName(nameInputField.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Player Name Set!");
                done = true;
            }
            else
            {
                Debug.Log("Failed to set player name!");
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
        yield return new WaitForSeconds(1);
        //tipsText.text = "PRESS SPACE TO START";
        tips2.SetActive(false);
        startscreen1.GetComponent<Animator>().SetTrigger("start");
        yield return new WaitForSeconds(1);
        readyToStart = true;
    }
}
