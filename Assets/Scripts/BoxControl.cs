using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoxControl : MonoBehaviour
{
    [Header("Setting")]
    public bool isSpecialBox = false;
    public GameObject pickup;
    public GameObject powerup;

    [Header("DontCare")]
    public bool isPushed = false;
    bool triggered = false;
    bool opened = false;

    public Vector2 moveDirection = new Vector2 (0, 0);
    Vector2 destination;

    LayerMask wallLayer;
    LayerMask boxLayer;

    void Start()
    {
        destination = transform.position;
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");

        //special box change sprite
        //todo
        if (isSpecialBox)
        {
            GetComponent<SpriteRenderer>().color = Color.gray;
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

        if (isSpecialBox)
        {
            Instantiate(powerup, transform.position, powerup.transform.rotation);
        }
        else 
        {
            Instantiate(pickup, transform.position, pickup.transform.rotation);
        }
        
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPushed && collision.gameObject.tag == "NPC")
        {
            // TODO: NPC HP--
            collision.GetComponent<NPCContol>().health--;
        }
    }
}
