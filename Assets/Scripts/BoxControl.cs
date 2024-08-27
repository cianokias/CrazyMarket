using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoxControl : MonoBehaviour
{
    [Header("Setting")]
    //public bool isSpecialBox = false;
    public int boxType = 0;
    public GameObject[] pickup;
    public GameObject powerup;
    public Sprite[] boxSprites;

    [Header("DontCare")]
    public bool isPushed = false;
    bool triggered = false;
    bool opened = false;

    public Vector2 moveDirection = new Vector2 (0, 0);
    Vector2 destination;

    LayerMask wallLayer;
    LayerMask boxLayer;

    SpriteRenderer sr;
    int killCount = 0;

    private void OnValidate()
    {
        sr = GetComponent<SpriteRenderer>();
        switch (GetComponent<BoxControl>().boxType)
        {
            case 0:
                sr.sprite = boxSprites[0];
                break;
            case 1:
                sr.sprite = boxSprites[1];
                break;
            case 2:
                sr.sprite = boxSprites[2];
                break;
            case 3:
                sr.sprite = boxSprites[3];
                break;
        }
    }

    void Start()
    {
        destination = transform.position;
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");

        //set box orderInLayer
        sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 14 - (int)transform.position.y;

        //special box change sprite
        /*if (isSpecialBox)
        {
            GetComponent<SpriteRenderer>().color = Color.gray;
        }*/
        switch (boxType)
        {
            case 0:
                sr.sprite = boxSprites[0];
                break;
            case 1:
                sr.sprite = boxSprites[1];
                break;
            case 2:
                sr.sprite = boxSprites[2];
                break;
            case 3:
                sr.sprite = boxSprites[3];
                break;

        }
    }

    void Update()
    {
        if (isPushed && !triggered)
        {
            triggered = true;
            destination += moveDirection * 20;
        }

        transform.position = Vector2.MoveTowards(transform.position, destination, 10 * Time.deltaTime);

        if (isPushed && !opened)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)moveDirection * 0.55f, moveDirection, 0.01f, wallLayer | boxLayer);

            if (hit.collider != null)
            {
                opened = true;
                StartCoroutine("openTheBox");
            }
        }
    }

    IEnumerator openTheBox()
    {
        destination = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().enabled = false;

        /*if (isSpecialBox)
        {
            Instantiate(powerup, transform.position, powerup.transform.rotation);
        }
        else 
        {
            Instantiate(pickup, transform.position, pickup.transform.rotation);
        }*/
        switch (boxType)
        {
            case 0:
                Instantiate(pickup[0], transform.position, pickup[0].transform.rotation);
                break;
            case 1:
                Instantiate(pickup[1], transform.position, pickup[1].transform.rotation);
                break;
            case 2:
                Instantiate(pickup[2], transform.position, pickup[2].transform.rotation);
                break;
            case 3:
                Instantiate(powerup, transform.position, powerup.transform.rotation);
                break;

        }

        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPushed && collision.gameObject.tag == "NPC")
        {
            Destroy(collision.gameObject);
            killCount++;
            Game.Control.updateScore(100 * killCount);
        }
    }
}
