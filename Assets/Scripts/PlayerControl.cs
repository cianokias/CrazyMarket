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

    //box pushing
    float rayDistance = 1.0f;  // Raycast distance
    LayerMask wallLayer;       // wall layer
    LayerMask boxLayer;        // box layer

    //other
    bool canBeHurt = true;
    float speedForOneBlock = 0.2f;

    //components
    Animator anim;
    SpriteRenderer sr;

    private void Awake()
    {
        wallLayer = LayerMask.GetMask("Wall");
        boxLayer = LayerMask.GetMask("Box");
        destination = transform.position;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        destination = transform.position;
        canMove = true;
        StartCoroutine(cannotBeHurt(2f));
    }

    void Update()
    {
        //move to destination
        transform.position = Vector2.MoveTowards(transform.position, destination, (1 / speedForOneBlock) * Time.deltaTime);

        //check if at destination
        if ((Vector2)transform.position == destination) { isMoving = false; }

        if (Input.GetAxis("Horizontal") == 0f && Input.GetAxis("Vertical") == 0f) { anim.SetBool("isWalking", false); }
        
        //update sortingorder
        sr.sortingOrder = (14 - (int)transform.position.y) * 2 + 1;
        
        //move logic
        if (canMove && !isMoving && !Game.Control.recovering && !Game.Control.gamePaused)
        {
            moveDirection = Vector2.zero;

            //user input
            if (Input.GetAxis("Horizontal") >= 0.2f)
            {
                moveDirection = Vector2.right;
                anim.SetInteger("faceDirection", 1);
            }
            else if (Input.GetAxis("Horizontal") <= -0.2f)
            {
                moveDirection = Vector2.left;
                anim.SetInteger("faceDirection", 3);
            }
            else if (Input.GetAxis("Vertical") >= 0.2f)
            {
                moveDirection = Vector2.up;
                anim.SetInteger("faceDirection", 2);
            }
            else if (Input.GetAxis("Vertical") <= -0.2f)
            {
                moveDirection = Vector2.down;
                anim.SetInteger("faceDirection", 0);
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
                    anim.SetBool("isWalking", true);
                    StartCoroutine("moving");
                }
            }
        }

        //push the box/////////////////////////////////////////////////////////////////////////////////////
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
        yield return new WaitForSeconds(speedForOneBlock); //deafult 0.2f = time used to move 1 step
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
        Vector3 pos = collision.transform.position;

        //checkout
        if (collision.gameObject.tag == "Checkout")
        {
            int tempItem = Game.Control.item;
            if (tempItem > 0)
            {
                Game.Control.updateItem(-tempItem);
                Game.Control.updateScore(tempItem * 100);

                StartCoroutine(anotherOHT("Score +" + tempItem * 100, pos, 1.5f));
                StartCoroutine(anotherOHT("Item -" + tempItem, pos, 1f));

                MusicPlayer.player.playAudio("checkout");

                //Game.Control.timer += 0.3f * tempItem * (1 + 0.07f * (tempItem - 1));
                //Game.Control.timer += 0.3f * tempItem * 1.7f;
            }
        }

        if (collision.gameObject.tag == "Pickup")
        {
            Destroy(collision.gameObject);

            Game.Control.updateItem(1);
            Game.Control.displayOHT("Item +1", pos);

            MusicPlayer.player.playAudio("collect");
        }

        if (collision.gameObject.tag == "PowerUp")
        {
            Destroy(collision.gameObject);

            Game.Control.updateScore(200);
            Game.Control.displayOHT("Score +200", pos);
            StartCoroutine(anotherOHT("Speed Boost!", pos, 0.5f));

            StartCoroutine(cannotBeHurt(8f));
            StartCoroutine(speedBoost(8f));

            MusicPlayer.player.playAudio("powerUp");
        }

        if (collision.gameObject.tag == "PowerUpHP")
        {
            Destroy(collision.gameObject);

            Game.Control.updateScore(200);
            Game.Control.displayOHT("Score +200", pos);
            Game.Control.updateHealth(1);
            StartCoroutine(anotherOHT("HP +1", pos, 0.5f));

            MusicPlayer.player.playAudio("powerUp");
        }

        if (collision.gameObject.tag == "NPC")
        {
            if (canBeHurt)
            {
                StartCoroutine(cannotBeHurt(0.5f));
                Game.Control.updateHealth(-1);
                if (Game.Control.health <= 0)
                {
                    Game.Control.resetPlayer();
                }
            }
        }
    }

    IEnumerator anotherOHT(string textToDisplay, Vector3 targetPosition, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        Game.Control.displayOHT(textToDisplay, targetPosition);
    }

    IEnumerator cannotBeHurt(float time)
    {
        canBeHurt = false;
        sr.color = Color.yellow;
        yield return new WaitForSeconds(time);
        canBeHurt = true;
        sr.color = Color.white;
    }

    IEnumerator speedBoost(float time)
    {
        speedForOneBlock = 0.15f;
        yield return new WaitForSeconds(time);
        speedForOneBlock = 0.2f;
    }

}
