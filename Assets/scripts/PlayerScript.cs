using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    enum State {Idle, Walk, Skid, Jump, Pump, Dying};
    enum Form {None, Balloon1, Balloon2, Chute, Rocket};

    private State state = State.Idle;
    private Form form = Form.None;
    
    private float speed, accel, decel, gravity, grav, jump, flap;
    private bool isRight = true;
    private bool uCheck, dCheck, lCheck, rCheck;   
    private Rect collision;
    private int pumpCt = 0;
    private bool fly = false;

    public Transform balloon;
    public Transform chute;
    private Transform prefab;
    
    private Animator anim;
    private BoxCollider2D coll;
    private ContactFilter2D filter;
    private EntityScript self, above, below, left, right; 

    private float scale = 0.005f;

	// Use this for initialization
	void Start ()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        self = GetComponent<EntityScript>();
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
                jump = 7 * scale;
                flap = 5 * scale;
                break;
            case Form.Balloon2:
                speed = 7 * scale;
                accel = 0.4f * scale;
                decel = 0.3f * scale;
                gravity = -10 * scale;
                grav = 0.25f * scale;
                jump = 8 * scale;
                flap = 6 * scale;
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
                jump = 30 * scale;
                flap = 0 * scale;
                break;
        }

        //Collisions
        checkCollisions();

        if (dCheck && self.move.y > 0)
        {
            self.move.y = 0;
        }

        if (uCheck && self.move.y <= 0)
        {
            self.move.y = 0;
            transform.position = new Vector2(transform.position.x, collision.height - (coll.offset.y - coll.size.y * 0.5f));
            fly = false;
        }

        if (rCheck && self.move.x < 0)
        {
            self.move.x = 0;
        }

        if (lCheck && self.move.x > 0)
        {
            self.move.x = 0;
        }

        if (state != State.Pump)
        {
            if (uCheck || form == Form.None || form == Form.Chute || form == Form.Rocket)
            {
                if (Input.GetKey("d") && !Input.GetKey("a") && !lCheck)
                {
                    if (self.move.x < speed) self.move.x += accel;
                    else if (self.move.x > speed) self.move.x = speed;

                    if(self.move.x < 0)
                    {
                        if (uCheck && state != State.Skid)
                        {
                            state = State.Skid;
                            anim.SetInteger("State", (int)state);
                            if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
                        }
                    }
                }
                else if (Input.GetKey("a") && !Input.GetKey("d") && !rCheck)
                {
                    if (self.move.x > -speed) self.move.x -= accel;
                    else if (self.move.x < -speed) self.move.x = -speed;

                    if (self.move.x > 0)
                    {
                        if (uCheck && state != State.Skid)
                        {
                            state = State.Skid;
                            anim.SetInteger("State", (int)state);
                            if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
                        }
                    }
                }
                else
                {
                    if (Mathf.Abs(self.move.x) < decel)
                    {
                        self.move.x = 0;
                        if (uCheck && state != State.Idle)
                        {
                            state = State.Idle;
                            anim.SetInteger("State", (int)state);
                            if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
                        }
                    }
                    else if (self.move.x > 0) self.move.x -= decel;
                    else self.move.x += decel;
                }

                if (self.move.x != 0)
                {
                    if (uCheck && state != State.Walk && ((self.move.x > 0 && !Input.GetKey("a")) || (self.move.x < 0 && !Input.GetKey("d"))))
                    {
                        state = State.Walk;
                        anim.SetInteger("State", (int)state);
                        anim.SetTrigger("Walk");
                        if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
                    }
                    if (self.move.x > 0 && isRight == false)
                    {
                        isRight = true;
                        anim.SetBool("isRight", isRight);
                        if (prefab != null && prefab.name == "Rocket") prefab.SendMessage("IsRight", isRight);
                    }
                    if (self.move.x < 0 && isRight == true)
                    {
                        isRight = false;
                        anim.SetBool("isRight", isRight);
                        if (prefab != null && prefab.name == "Rocket") prefab.SendMessage("IsRight", isRight);
                    }
                }
            }
            else
            {
                if (Input.GetKey("d"))
                {
                    isRight = true;
                    anim.SetBool("isRight", isRight);
                    if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("IsRight", isRight);
                }
                else if (Input.GetKey("a"))
                {
                    isRight = false;
                    anim.SetBool("isRight", isRight);
                    if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("IsRight", isRight);
                }
            }
        }

        //Falling
        if (!uCheck)
        {
            if (form != Form.Chute)
            {
                if (self.move.y > gravity) self.move.y -= grav;
                else if (self.move.y < gravity) self.move.y = gravity;

                if (state != State.Jump)
                {
                    state = State.Jump;
                    anim.SetInteger("State", (int)state);
                    if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
                }

                if (form == Form.Balloon1 || form == Form.Balloon2)
                {
                    self.move.x = ((int)(self.move.x / scale) - ((int)(self.move.x / scale) % 2)) * scale;
                }
            }
            else
            {
                if (self.move.y / 1.15f <= -jump) self.move.y /= 1.15f;
                else self.move.y = -jump;
            }
        }
        else
        {
            if(prefab != null && prefab.tag == "Chute")
            {
                DestroyObject(prefab.gameObject);
                if(form == Form.Chute)form = Form.None;
            }
        }

        /*Reconnect with balloons*/
        if(above != null && form == Form.None)
        {
            if(above.name == "Balloon")
            {
                if (above.tag == "Balloon1") form = Form.Balloon1;
                else form = Form.Balloon2;
                prefab = above.transform;
                prefab.parent = transform;
                prefab.transform.position = new Vector2(transform.position.x - 0.12f, transform.position.y + 0.4f);
                self.move.x = ((int)(self.move.x / scale) - ((int)(self.move.x / scale) % 2)) * scale;
                self.move.y = (self.move.y + above.move.y) * 0.5f;
            }
        }

        /*Connect with Rocket*/
        if(((left != null && left.name == "Rocket") || (right != null && right.name == "Rocket")) && form == Form.None)
        {
            form = Form.Rocket;
            if (left != null && left.name == "Rocket") prefab = left.transform;
            else prefab = right.transform;
            prefab.parent = transform;
            prefab.SendMessage("IsRight", isRight);
        }

        /*Fly Rocket*/
        if (Input.GetButton("Jump") && form == Form.Rocket && fly == true)
        {
            if (self.move.y < jump / 2) self.move.y += flap;
            if (prefab != null) prefab.SendMessage("Fly", true);
        }
        else if (prefab != null) prefab.SendMessage("Fly", false);

        transform.Translate(self.move);
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && state != State.Pump)
        {
            if (uCheck)                 /*Jump*/
            {
                self.move.y = jump;
                state = State.Jump;
                anim.SetInteger("State", (int)state);
                anim.ResetTrigger("Flap");
                if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
            }   
            else                        /*Flap*/
            {
                if (form == Form.Balloon1 || form == Form.Balloon2)
                {
                    self.move.y = flap;
                    if (Input.GetKey("d") && (int)(self.move.x / scale) < 9) self.move.x += 2 * scale;
                    else if (Input.GetKey("a") && (int)(self.move.x / scale) > -9) self.move.x -= 2 * scale;
                }
                else if (form == Form.Rocket) fly = true;
                anim.SetTrigger("Flap");
            } 
        }

        //(Take out / Put away) pump
        if (Input.GetKeyDown("p") && uCheck && form != Form.Balloon2 && form != Form.Rocket)
        {
            if (state != State.Pump)
            {
                state = State.Pump;
                self.move.x = 0;
                anim.ResetTrigger("Pump");
            }
            else
            {
                state = State.Idle;
                pumpCt = 0;
            }

            anim.SetInteger("State", (int)state);
            if (prefab != null && prefab.name == "Balloon") prefab.SendMessage("GetState", (int)state);
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
                    prefab.SendMessage("IsRight", isRight);
                    form = Form.Balloon1;
                }
                else
                {
                    form = Form.Balloon2;
                    state = State.Idle;
                    anim.SetInteger("State", (int)state);
                    if (prefab != null)
                    {
                        prefab.SendMessage("GetForm", (int)form);
                        prefab.SendMessage("GetState", (int)state);
                    }
                }
                pumpCt = 0;
            }
        }

        //Detach
        if(Input.GetKeyDown("o") && form != Form.None)
        {
            switch(form)
            {
                case Form.Balloon1:
                case Form.Balloon2:
                    form = Form.None;
                    prefab.SendMessage("GetVector", self.move);
                    prefab.parent = null;
                    prefab = null;
                    break;
                case Form.Chute:
                    form = Form.None;
                    prefab.SendMessage("Rip");
                    break;
                case Form.Rocket:
                    form = Form.None;
                    prefab.parent = null;
                    prefab = null;
                    break;
            }
        }

        /*Pull out parachute*/
        if(Input.GetKeyDown("p") && !uCheck && self.move.y < 0 && form == Form.None)
        {
            prefab = Instantiate(chute, transform, false);
            prefab.position = new Vector2(transform.position.x - 0.03f, transform.position.y + 0.53f);
            form = Form.Chute;
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
        above = null;
        below = null;
        left = null;
        right = null;
        collision.Set(0, 0, 0, 0);
        

        if (ct > 0)
        {
            for (int i = 0; i < ct; i++)
            {
                entity = results[i].GetComponent<EntityScript>();

                if ((Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x)) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x)) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && self.move.y <= entity.move.y)
                {
                    below = entity;
                    if (entity.uTang)
                    {
                        uCheck = true;
                        collision.height = results[i].bounds.max.y;
                    }
                }
                else if ((Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x)) && (Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && self.move.x <= entity.move.x)
                {
                    left = entity;
                    if (entity.rTang)
                    {
                        rCheck = true;
                        collision.x = results[i].bounds.max.x;
                    }
                }
                else if ((Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && self.move.x >= entity.move.x)
                {
                    right = entity;
                    if (entity.lTang)
                    {
                        lCheck = true;
                        collision.width = results[i].bounds.min.x;
                    }
                }
                else if (self.move.y >= entity.move.y)
                {
                    above = entity;
                    if (entity.dTang)
                    {
                        dCheck = true;
                        collision.height = results[i].bounds.min.y;
                    }
                }
            }
        }
    }    
}
