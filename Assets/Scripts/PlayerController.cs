using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //start() variables
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;

    //FSM
    private enum State { idle, running, jumping, falling, hurt}
    private State state = State.idle;
    

    //
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 4f;
    [SerializeField] private float jumpforce = 9f;
    [SerializeField] private float hurtForce = 3f;
    [SerializeField] private AudioSource footstep;
    [SerializeField] private AudioSource cherrySound;
    [SerializeField] private AudioSource jump;
    [SerializeField] private AudioSource enemyDeath;

    //Collactable
    public TextMeshProUGUI cherryText;
    private int cherries = 0;

    //Health
    public TextMeshProUGUI healthAmount;
    private int health = 5;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        healthAmount.text = health.ToString();
    }

    private void Update()
    {
        if(state != State.hurt)
        {
            Movement();
        } 
        AnimationState();
        anim.SetInteger("state", (int)state); //set animation based on enum
    }

    private void Movement()
    {
        float hDirection = Input.GetAxis("Horizontal"); //make variable from input manager

        //move to left
        if (hDirection < 0) 
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            transform.localScale = new Vector2(-1, 1);
        }

        //move to right
        else if (hDirection > 0) 
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);
        }

        //jump
        //GetButtonDown will happen once when you hit the key
        //IsTouchingLayers for player only jump if touch the ground layers
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground)) 
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpforce);
        state = State.jumping;
        jump.Play();
    }

    private void AnimationState()
    {
        if (state == State.jumping)
        {
            if (rb.velocity.y < .1f)
            {
                state = State.falling; //if player is jumping then he is falling
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle; //if player touch the ground back to idle
            }
        }
        else if (state == State.hurt)
        {
            if (Mathf.Abs(rb.velocity.x) < .1f)
            {
                state = State.idle;
            }
        }

        else if(Mathf.Abs(rb.velocity.x) > 2f)
        {
            state = State.running; //moving
        }
        else
        {
            state = State.idle; //switch to idle
        }
    }

    //Collectable
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable")
        {
            cherrySound.Play();
            Destroy(collision.gameObject);
            cherries += 1;
            cherryText.text = cherries.ToString();
        }
    }

    //Enemy
    private void OnCollisionEnter2D(Collision2D other)
    {
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        if(other.gameObject.tag == "Enemy")
        {
            if (state == State.falling)
            {
                enemyDeath.Play();
                enemy.JumpedOn();
                Jump();
            }
            else
            {
                state = State.hurt;
                HandleHealth();
                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    //enemy is to my right therefore i should be damaged
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    //enemy is to my left therefore i should be damaged
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
        }
    }

    private void HandleHealth()
    {
        health -= 1;
        healthAmount.text = health.ToString();
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Footstep()
    {
        footstep.Play();
    }


}
