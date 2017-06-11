using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    private bool uCheck, dCheck, lCheck, rCheck;
    private Rect collision;

    private Animator anim;
    private BoxCollider2D coll;
    private ContactFilter2D filter;
    private EntityScript self, above, below, left, right;

    private float scale = 0.005f;

    // Use this for initialization
    void Start ()
    {
        name = "Balloon";
        anim = this.GetComponent<Animator>();
        coll = this.GetComponent<BoxCollider2D>();
        self = this.GetComponent<EntityScript>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(transform.parent == null)
        {
            checkCollisions();

            if(lCheck)
            {
                self.move.x = -Mathf.Abs(self.move.x);
                self.push.x = right.move.x;
            }
            if(rCheck)
            {
                self.move.x = Mathf.Abs(self.move.x);
                self.push.x = left.move.x;
            }

            if(dCheck)
            {
                if (self.move.y < 0.8f) self.move.y += 0.04f * scale;
                else if (self.move.y > 0.8f) self.move.y = 0.8f;
            }
            else
            {
                if (self.move.y < 1) self.move.y += 0.07f * scale;
                else if (self.move.y > 1) self.move.y = 1;
            }

            if (Mathf.Abs(self.push.x) > scale)
            {
                self.push.x *= 0.98f;
            }
            else
            {
                self.push.x = 0;
                self.move.x *= 0.98f;
            }

            if (anim.GetInteger("State") != 3)
            {
                anim.SetInteger("State", 3);
                anim.SetTrigger("Walk");
            }

            transform.Translate(self.move + self.push);
        }
	}

    void GetVector(Vector2 i)
    {
        self.move.x = i.x / 1.5f;
        self.move.y = i.y / 2.5f;
    }

    void GetState(int i)
    {
        if (i == 4) i = 0;
        if ((i == 0 && anim.GetInteger("State") != 0) || (i == 3 && anim.GetInteger("State") != 3)) anim.SetTrigger("Walk");
        anim.SetInteger("State", i);
    }

    void GetForm(int i)
    {
        if (i == 2)
        {
            anim.SetTrigger("Walk");
            tag = "Balloon2";
        }
        else tag = "Balloon1";
        anim.SetInteger("Form", i);
    }

    void IsRight(bool i)
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetBool("isRight", i);
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

                if ((Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= 0.1f) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x)) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x)) && (Mathf.Abs(coll.bounds.min.y - results[i].bounds.max.y) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && self.move.y <= entity.move.y)
                {
                    below = entity;
                    if (entity.uTang)
                    {
                        uCheck = true;
                        collision.height = results[i].bounds.max.y;
                    }
                }
                else if ((Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= 0.1f) && (Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x)) && (Mathf.Abs(coll.bounds.min.x - results[i].bounds.max.x) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && self.move.x <= entity.move.x)
                {
                    left = entity;
                    if (entity.rTang)
                    {
                        rCheck = true;
                        collision.x = results[i].bounds.max.x;
                    }
                }
                else if ((Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x) <= 0.1f) && (Mathf.Abs(coll.bounds.max.x - results[i].bounds.min.x) <= Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y)) && self.move.x >= entity.move.x)
                {
                    right = entity;
                    if (entity.lTang)
                    {
                        lCheck = true;
                        collision.width = results[i].bounds.min.x;
                    }
                }
                else if ((Mathf.Abs(coll.bounds.max.y - results[i].bounds.min.y) <= 0.1f) && self.move.y >= entity.move.y)
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
