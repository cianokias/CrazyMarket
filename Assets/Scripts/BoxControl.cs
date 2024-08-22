using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoxControl : MonoBehaviour
{
    public bool isPushed = false;
    bool triggered = false;
    bool opened = false;

    public Vector2 moveDirection = new Vector2 (0, 0);
    Vector2 destination;

    public GameObject pickup;

    LayerMask wallLayer;
    LayerMask boxLayer;

    void Start()
    {
        destination = transform.position;
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");
    }

    void Update()
    {
        if (isPushed && !triggered)
        {
            triggered = true;
            destination += moveDirection * 20;
        }

        transform.position = Vector2.MoveTowards(transform.position, destination, 5 * Time.deltaTime);

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
        Instantiate(pickup, transform.position, pickup.transform.rotation);
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }
}
