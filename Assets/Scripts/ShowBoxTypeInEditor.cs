using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBoxTypeInEditor : MonoBehaviour
{

    [ExecuteInEditMode]
    public bool refresh;
    BoxControl bc;
    SpriteRenderer sr;

    private void Start()
    {
        bc = GetComponent<BoxControl>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void OnValidate()
    {
        switch (bc.GetComponent<BoxControl>().boxType)
        {
            case 0:
                sr.sprite = bc.boxSprites[0];
                break;
            case 1:
                sr.sprite = bc.boxSprites[1];
                break;
            case 2:
                sr.sprite = bc.boxSprites[2];
                break;
            case 3:
                sr.sprite = bc.boxSprites[3];
                break;

        }
    }

}
