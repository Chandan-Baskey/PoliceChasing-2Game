using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    public float moveSpeed;
    public float rotateSpeed;
    
    float rot;

    public static Player Instance;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float rot=0;

        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    if(mousePos.x < 0)
        //    {
        //        rot = 1;
        //    }
        //    else if(mousePos.x > 0)
        //    {
        //      rot = -1;
        //    }
        //    //float rotAmout = rotateSpeed * rot;
        //    //transform.Rotate(0, 0, rotAmout);

        //    //transform.Rotate(0, 0,rot)
        //}

        if (Keyboard.current.aKey.isPressed)
        {
            //Debug.Log("A key is pressed left");
            rot = 1;
        }

        else if (Keyboard.current.dKey.isPressed)
        {
            // Debug.Log("D key is pressed right");
            rot = -1;
        }

        float rotAmout = rotateSpeed * rot;
        transform.Rotate(0, 0, rotAmout);

    }

    private void FixedUpdate()
    {
        //// Apply rotation through the physics engine to avoid jitter
        //rb.MoveRotation(rb.rotation + rot * Time.fixedDeltaTime);

        // Always move forward relative to the player's facing direction
        rb.linearVelocity = transform.up * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cash"))
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PoliceBlock") ||
            collision.gameObject.CompareTag("Police"))
        {
            // Reset timeScale in case the game was paused before reloading
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}