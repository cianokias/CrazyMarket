using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatCodeDetector : MonoBehaviour
{
    KeyCode[] theCode;
    int index;
    bool actived;

    public GameObject cheatTips;

    void Start()
    {
        theCode = new KeyCode[] { KeyCode.UpArrow, KeyCode.UpArrow,
                                  KeyCode.DownArrow, KeyCode.DownArrow,
                                  KeyCode.LeftArrow, KeyCode.RightArrow,
                                  KeyCode.LeftArrow, KeyCode.RightArrow,
                                  KeyCode.B, KeyCode.A };
        index = 0;
    }

    void Update()
    {
        if (!actived && Input.anyKeyDown)
        {
            if (Input.GetKeyDown(theCode[index]))
            {
                index++;
            }
            else
            {
                index = 0;
            }
        }

        if (index == theCode.Length && !actived)
        {
            actived = true;
            MusicPlayer.player.cheatMode = true;
            Debug.Log("CHEAT MODE ON");
            StartCoroutine(showCheatTips());
        }
    }

    IEnumerator showCheatTips()
    {
        cheatTips.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        cheatTips.SetActive(false);
    }
}
