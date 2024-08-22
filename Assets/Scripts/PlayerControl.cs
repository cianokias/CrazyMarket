using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    //Movement vars
    Vector2 destination = new Vector2(0,0);
    Vector2 moveDirection = new Vector2(0,0);
    Vector2 faceDirection = new Vector2(0, 0);
    bool canMove = true;
    bool isMoving = false;

    float rayDistance = 1.0f;  // Raycast distance
    LayerMask wallLayer;       // wall layer
    LayerMask boxLayer;        // box layer

    int score = 0;

    void Start()
    {
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");
        destination = transform.position;
    }

    private void OnEnable()
    {
        destination = transform.position;
    }

    void Update()
    {
        //move to destination
        transform.position = Vector2.MoveTowards(transform.position, destination, 5 * Time.deltaTime);

        //check if at destination
        if ((Vector2)transform.position == destination) { isMoving = false; }

        //move logic
        if (canMove && !isMoving && !Game.Control.recovering)
        {
            moveDirection = Vector2.zero;

            //user input
            if (Input.GetAxis("Horizontal") >= 0.2f)
            {
                moveDirection = Vector2.right;
            }
            else if (Input.GetAxis("Horizontal") <= -0.2f)
            {
                moveDirection = Vector2.left;
            }
            else if (Input.GetAxis("Vertical") >= 0.2f)
            {
                moveDirection = Vector2.up;
            }
            else if (Input.GetAxis("Vertical") <= -0.2f)
            {
                moveDirection = Vector2.down;
            }

            //if there is a input
            if (moveDirection != Vector2.zero)
            {
                faceDirection = moveDirection; //for push box checking and future animation

                // Raycast to check the wall
                RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, rayDistance, wallLayer | boxLayer);

                if (hit.collider == null)
                {
                    // no wall, can move
                    destination += moveDirection;
                    canMove = false;
                    isMoving = true;
                    StartCoroutine("moving");
                }
            }
        }

        //push the box/////////////////////////////////////////////////////////////////////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, faceDirection, rayDistance, boxLayer);

            if (hit.collider != null)
            {
                BoxControl hitObjectControl = hit.collider.gameObject.GetComponent<BoxControl>();
                hitObjectControl.isPushed = true;
                hitObjectControl.moveDirection = faceDirection;
            }
        }

    }

    IEnumerator moving()
    {
        yield return new WaitForSeconds(0.2f); //0.2f = time used to move 1 step
        canMove = true;
    }

    //prevent player stuck in the wall
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isMoving)
        {
            destination = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pickup")
        {
            Destroy(collision.gameObject);
            score += 100;
            Debug.Log("Score: " + score);
        }

        if (collision.gameObject.tag == "PowerUp")
        {
            Destroy(collision.gameObject);
            score += 100;
            Debug.Log("Score: " + score + " , and a power up!");
        }

        if (collision.gameObject.tag == "NPC")
        {
            Game.Control.health -= 1;
            if (Game.Control.health == 0)
            {
                Game.Control.resetPlayer();
            }
        }
    }

}
