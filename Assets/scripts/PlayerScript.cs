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
    private bool isRight = true;
    private bool jumping = false;
    private bool uCheck, dCheck, lCheck, rCheck;
    private Rect collision;
    private int pumpCt = 0;

    public Transform balloon;
    private Transform prefab;
    
    private SpriteRenderer rend;
    private Animator anim;
    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private RaycastHit2D[] hit;
    private ContactFilter2D filter;

    private float scale = 0.006f;

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
                decel = 0.5f * scale;
                gravity = -10 * scale;
                grav = 0.35f * scale;
                jump = 6 * scale;
                flap = 4 * scale;
                break;
            case Form.Balloon2:
                speed = 7 * scale;
                accel = 0.4f * scale;
                decel = 0.3f * scale;
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
        checkCollisions();

        if (dCheck && moveVector.y > 0)
        {
            moveVector.y = 0;
        }

        if (uCheck && moveVector.y <= 0)
        {
            moveVector.y = 0;
            transform.position = new Vector2(transform.position.x, collision.height - 0.01f);
        }

        if (rCheck && moveVector.x < 0)
        {
            moveVector.x = 0;
        }

        if (lCheck && moveVector.x > 0)
        {
            moveVector.x = 0;
        }

        if (state != State.Pump)
        {
            if (uCheck || form == Form.None || form == Form.Chute || form == Form.Rocket)
            {
                if (Input.GetKey("d") && !lCheck)
                {
                    if (moveVector.x < speed) moveVector.x += accel;
                    else if (moveVector.x > speed) moveVector.x = speed;

                    if(moveVector.x < 0)
                    {
                        if (uCheck && state != State.Skid)
                        {
                            state = State.Skid;
                            anim.SetInteger("State", (int)state);
                            if (prefab != null) prefab.SendMessage("GetState", (int)state);
                        }
                    }
                }
                else if (Input.GetKey("a") && !rCheck)
                {
                    if (moveVector.x > -speed) moveVector.x -= accel;
                    else if (moveVector.x < -speed) moveVector.x = -speed;

                    if (moveVector.x > 0)
                    {
                        if (uCheck && state != State.Skid)
                        {
                            state = State.Skid;
                            anim.SetInteger("State", (int)state);
                            if (prefab != null) prefab.SendMessage("GetState", (int)state);
                        }
                    }
                }
                else
                {
                    if (Mathf.Abs(moveVector.x) < decel)
                    {
                        moveVector.x = 0;
                        if (uCheck && state != State.Idle)
                        {
                            state = State.Idle;
                            anim.SetInteger("State", (int)state);
                            if (prefab != null) prefab.SendMessage("GetState", (int)state);
                        }
                    }
                    else if (moveVector.x > 0) moveVector.x -= decel;
                    else moveVector.x += decel;
                }

                if (moveVector.x != 0)
                {
                    if (uCheck && state != State.Walk && ((moveVector.x > 0 && !Input.GetKey("a")) || (moveVector.x < 0 && !Input.GetKey("d"))))
                    {
                        state = State.Walk;
                        anim.SetInteger("State", (int)state);
                        anim.SetTrigger("Walk");
                        if (prefab != null) prefab.SendMessage("GetState", (int)state);
                    }
                    if (moveVector.x > 0 && isRight == false)
                    {
                        isRight = true;
                        anim.SetBool("isRight", isRight);
                        if (prefab != null) prefab.SendMessage("IsRight", isRight);
                    }
                    if (moveVector.x < 0 && isRight == true)
                    {
                        isRight = false;
                        anim.SetBool("isRight", isRight);
                        if (prefab != null) prefab.SendMessage("IsRight", isRight);
                    }
                }
            }
            else
            {
                if (Input.GetKey("d"))
                {
                    isRight = true;
                    anim.SetBool("isRight", isRight);
                    if (prefab != null) prefab.SendMessage("IsRight", isRight);
                }
                else if (Input.GetKey("a"))
                {
                    isRight = false;
                    anim.SetBool("isRight", isRight);
                    if (prefab != null) prefab.SendMessage("IsRight", isRight);
                }
            }
        }

        //Jumping
        if(jumping)
        {
            moveVector.y = jump;
            state = State.Jump;
            anim.SetInteger("State", (int)state);
            anim.ResetTrigger("Flap");
            if (prefab != null) prefab.SendMessage("GetState", (int)state);
            jumping = false;
        }

        //Falling
        if (!uCheck)
        {
            if (moveVector.y > gravity) moveVector.y -= grav;
            else if (moveVector.y < gravity) moveVector.y = gravity;           

            if (state != State.Jump)
            {
                state = State.Jump;
                anim.SetInteger("State", (int)state);
                if (prefab != null) prefab.SendMessage("GetState", (int)state);
            }

            if(form == Form.Balloon1 || form == Form.Balloon2)
            {
                moveVector.x = ((int)(moveVector.x / scale) - ((int)(moveVector.x / scale) % 2)) * scale;
            }
        }

        transform.Translate(moveVector);
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && state != State.Pump)
        {
            if (uCheck)                 /*Jump*/
            {
                jumping = true;
            }   
            else                        /*Flap*/
            {
                if(form == Form.Balloon1 || form == Form.Balloon2)
                {
                    moveVector.y = flap;
                    if (Input.GetKey("d") && (int)(moveVector.x / scale) < 9) moveVector.x += 2 * scale;
                    else if (Input.GetKey("a") && (int)(moveVector.x / scale) > -9) moveVector.x -= 2 * scale;
                }
                anim.SetTrigger("Flap");
            } 
        }

        //(Take out / Put away) pump
        if (Input.GetKeyDown("p") && uCheck && form != Form.Balloon2 && form != Form.Rocket)
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
            if (prefab != null) prefab.SendMessage("GetState", (int)state);
        }

        //Pump
        if (state == State.Pump && Input.GetKeyDown("s"))
        {
            anim.SetTrigger("Pump");
            pumpCt++;
            if(pumpCt >= 6)
            {
                if(form == Form.None)
                {
                    prefab = Instantiate(balloon, transform, false);
                    prefab.transform.position = new Vector2(transform.position.x - 0.12f, transform.position.y + 0.4f);
                    form = Form.Balloon1;
                }
            }
        }

        if(Input.GetKeyDown("o") && form != Form.None)
        {
            switch(form)
            {
                case Form.Balloon1:
                case Form.Balloon2:
                    form = Form.None;
                    prefab.parent = null;
                    break;
            }
        }
    }

    void checkCollisions()
    {
        int ct = 0;
        Collider2D[] results = new Collider2D[5];
        EntityScript entity;

        filter.NoFilter();
        filter.useTriggers = true;
        ct = coll.OverlapCollider(filter, results);
        uCheck = false;
        dCheck = false;
        lCheck = false;
        rCheck = false;
        collision.Set(0, 0, 0, 0);

        if (ct > 0)
        {
            for (int i = 0; i < ct; i++)
            {
                entity = results[i].GetComponent<EntityScript>();

                if ((Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x)) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x)) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && moveVector.y <= 0 && entity.uTang)
                {
                    uCheck = true;
                    collision.height = results[i].bounds.max.y;
                }
                else if ((Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x)) && (Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && entity.rTang)
                {
                    rCheck = true;
                    collision.x = results[i].bounds.max.x;
                }
                else if ((Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && entity.lTang)
                {
                    lCheck = true;
                    collision.width = results[i].bounds.min.x;
                }
                else if(entity.dTang)
                {
                    dCheck = true;
                    collision.height = results[i].bounds.min.y;
                }
            }
        }
    }

    /*bool castUp()
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
    }*/
}
