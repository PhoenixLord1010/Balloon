using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    enum State {Idle, Walk, Skid, Jump, Pump, Dying};
    enum Form {None, Balloon1, Balloon2, Chute, Rocket};

    private State state = State.Idle;
    private Form form = Form.None;
    
    private Vector2 moveVector = new Vector2(0, 0);
    private float speed, accel, decel, gravity, grav, jump, flap;
    private int isRight = 1;
    private bool isGrounded = false;
    private int pumpCt = 0;
    
    private SpriteRenderer rend;
    private Animator anim;
    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private RaycastHit2D[] hit;

    private float scale = 0.005f;

	// Use this for initialization
	void Start ()
    {
        rend = this.GetComponent<SpriteRenderer>();
        anim = this.GetComponent<Animator>();
        coll = this.GetComponent<BoxCollider2D>();
        rb = this.GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
		switch(form)
        {
            case Form.Balloon1:
                speed = 7.5f * scale;
                accel = 0.6f * scale;
                decel = 0.6f * scale;
                gravity = -10 * scale;
                grav = 0.35f * scale;
                jump = 6 * scale;
                flap = 4 * scale;
                break;
            case Form.Balloon2:
                speed = 7 * scale;
                accel = 0.4f * scale;
                decel = 0.4f * scale;
                gravity = -10 * scale;
                grav = 0.25f * scale;
                jump = 7 * scale;
                flap = 5 * scale;
                break;
            case Form.Chute:
                speed = 5 * scale;
                accel = 0.2f * scale;
                decel = 0.2f * scale;
                gravity = -10 * scale;
                grav = 0.1f * scale;
                jump = 0.5f * scale;
                flap = 0 * scale;
                break;
            case Form.Rocket:
                speed = 5 * scale;
                accel = 0.3f * scale;
                decel = 0.4f * scale;
                gravity = -10 * scale;
                grav = 2.4f * scale;
                jump = 18 * scale;
                flap = 3 * scale;
                break;
            case Form.None:
            default:
                speed = 8 * scale;
                accel = 0.7f * scale;
                decel = 0.6f * scale;
                gravity = -12 * scale;
                grav = 1.75f * scale;
                jump = 22 * scale;
                flap = 0 * scale;
                break;
        }

        //Collisions
        if (castUp() && moveVector.y > 0)
        {
            moveVector.y = 0;
        }

        if (castDown() && moveVector.y <= 0)
        {
            moveVector.y = 0;
            isGrounded = true;
            transform.position = new Vector2(transform.position.x, hit[0].transform.position.y + hit[0].collider.bounds.size.y + coll.bounds.extents.y);
        }

        if (castLeft() && moveVector.x < 0)
        {
            moveVector.x = 0;
        }

        if (castRight() && moveVector.x > 0)
        {
            moveVector.x = 0;
        }

        if (state != State.Pump)
        {
            if (form == Form.None || form == Form.Chute || form == Form.Rocket)
            {
                if (Input.GetKey("d") && !castRight())
                {
                    if (moveVector.x < speed) moveVector.x += accel;
                    else if (moveVector.x > speed) moveVector.x = speed;

                    if (moveVector.x > 0)
                    {
                        if (isGrounded && state != State.Walk)
                        {
                            state = State.Walk;
                            anim.SetInteger("State", (int)state);
                            anim.SetTrigger("Walk");
                        }
                        if (isRight == 0)
                        {
                            isRight = 1;
                            anim.SetInteger("isRight", isRight);
                        }
                    }
                    else
                    {
                        if (isGrounded && state != State.Skid)
                        {
                            state = State.Skid;
                            anim.SetInteger("State", (int)state);
                        }
                    }
                }
                else if (Input.GetKey("a") && !castLeft())
                {
                    if (moveVector.x > -speed) moveVector.x -= accel;
                    else if (moveVector.x < -speed) moveVector.x = -speed;

                    if (moveVector.x < 0)
                    {
                        if (isGrounded && state != State.Walk)
                        {
                            state = State.Walk;
                            anim.SetInteger("State", (int)state);
                            anim.SetTrigger("Walk");
                        }
                        if (isRight == 1)
                        {
                            isRight = 0;
                            anim.SetInteger("isRight", isRight);
                        }
                    }
                    else
                    {
                        if (isGrounded && state != State.Skid)
                        {
                            state = State.Skid;
                            anim.SetInteger("State", (int)state);
                        }
                    }
                }
                else
                {
                    if (Mathf.Abs(moveVector.x) < decel)
                    {
                        moveVector.x = 0;
                        if (isGrounded && state != State.Idle)
                        {
                            state = State.Idle;
                            anim.SetInteger("State", (int)state);
                        }
                    }
                    else if (moveVector.x > 0) moveVector.x -= decel;
                    else moveVector.x += decel;
                }
            }
            else
            {
                if (Input.GetKey("d"))
                {
                    isRight = 1;
                    anim.SetInteger("isRight", isRight);
                }
                else if (Input.GetKey("a"))
                {
                    isRight = 0;
                    anim.SetInteger("isRight", isRight);
                }
            }
        }

        //Falling
        if (!isGrounded)
        {
            if (moveVector.y > gravity) moveVector.y -= grav;
            else if (moveVector.y < gravity) moveVector.y = gravity;           

            if (state != State.Jump)
            {
                state = State.Jump;
                anim.SetInteger("State", (int)state);
            }
        }

        transform.Translate(moveVector);

        isGrounded = false;   
    }

    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && state != State.Pump)
        {
            if (castDown())
            {
                moveVector.y = jump;
                state = State.Jump;
                anim.SetInteger("State", (int)state);
                anim.ResetTrigger("Flap");
            }   
            else
            {
                if(form == Form.Balloon1 || form == Form.Balloon2)
                {
                    moveVector.y = flap;
                }
                anim.SetTrigger("Flap");
            } 
        }

        //(Take out / Put away) pump
        if (Input.GetKeyDown("p") && castDown() && form != Form.Balloon2 && form != Form.Rocket)
        {
            if (state != State.Pump)
            {
                state = State.Pump;
                moveVector.x = 0;
                anim.ResetTrigger("Pump");
            }
            else
            {
                state = State.Idle;
                pumpCt = 0;
            }

            anim.SetInteger("State", (int)state);
        }

        //Pump
        if (state == State.Pump && Input.GetKeyDown("s"))
        {
            anim.SetTrigger("Pump");
            pumpCt++;
        }
    }

    bool castUp()
    {
        hit = Physics2D.BoxCastAll(transform.position, new Vector2(coll.bounds.size.x - 0.1f, 0.01f), 0, Vector2.up, coll.bounds.extents.y - 0.01f, 1);
        if (hit.Length > 0)
            return true;
        else
            return false;
    }

    bool castDown()
    {
        hit = Physics2D.BoxCastAll(transform.position, new Vector2(coll.bounds.size.x - 0.02f, 0.01f), 0, Vector2.down, coll.bounds.extents.y - 0.01f, 1);
        if(hit.Length > 0)
            return true;
        else
            return false;
    }

    bool castLeft()
    {
        hit = Physics2D.BoxCastAll(transform.position, new Vector2(0.01f, coll.bounds.extents.y), 0, Vector2.left, coll.bounds.extents.x - 0.01f, 1);
        if (hit.Length > 0)
            return true;
        else
            return false;
    }

    bool castRight()
    {
        hit = Physics2D.BoxCastAll(transform.position, new Vector2(0.01f, coll.bounds.extents.y), 0, Vector2.right, coll.bounds.extents.x - 0.01f, 1);
        if (hit.Length > 0)
            return true;
        else
            return false;
    }
}
