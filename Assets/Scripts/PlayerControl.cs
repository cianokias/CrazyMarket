using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    //Movement vars
    Vector2 destination = new Vector2(0,0);
    Vector2 direction = new Vector2(0,0);
    bool canMove = true;
    bool isMoving = false;

    float rayDistance = 1.0f;  // Raycast distance
    LayerMask wallLayer;       // wall layer
    LayerMask boxLayer;        // box layer

    void Start()
    {
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");
    }

    void Update()
    {
        //move to destination
        transform.position = Vector2.MoveTowards(transform.position, destination, 5 * Time.deltaTime);

        //check if at destination
        if ((Vector2)transform.position == destination) { isMoving = false; }

        //move logic
        if (canMove && !isMoving)
        {
            direction = Vector2.zero;

            //user input
            if (Input.GetAxis("Horizontal") >= 0.2f)
            {
                direction = Vector2.right;
            }
            else if (Input.GetAxis("Horizontal") <= -0.2f)
            {
                direction = Vector2.left;
            }
            else if (Input.GetAxis("Vertical") >= 0.2f)
            {
                direction = Vector2.up;
            }
            else if (Input.GetAxis("Vertical") <= -0.2f)
            {
                direction = Vector2.down;
            }

            //if there is a input
            if (direction != Vector2.zero)
            {
                // Raycast to check the wall
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, wallLayer);

                if (hit.collider == null)
                {
                    // no wall, can move
                    destination += direction;
                    canMove = false;
                    isMoving = true;
                    StartCoroutine("moving");
                }
            }
        }

        //push the box/////////////////////////////////////////////////////////////////////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //TODO
            Debug.Log("Pressed Push");
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

}
