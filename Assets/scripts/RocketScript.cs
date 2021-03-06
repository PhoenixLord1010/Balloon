﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    private float gravity, grav;
    private bool uCheck, dCheck, lCheck, rCheck;
    private Rect collision;

    private Animator anim;
    private BoxCollider2D coll;
    private ContactFilter2D filter;
    private EntityScript self, above, below, left, right;

    private float scale = 0.005f;

    // Use this for initialization
    void Start()
    {
        gravity = -15 * scale;
        grav = 2.4f * scale;

        name = "Rocket";
        anim = this.GetComponent<Animator>();
        coll = this.GetComponent<BoxCollider2D>();
        self = this.GetComponent<EntityScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.parent == null)
        {
            if (anim.GetBool("Fly") == true) anim.SetBool("Fly", false);

            checkCollisions();            

            if (uCheck)
            {
                if (self.move.y < -2 * scale) self.move.y = Mathf.Abs(self.move.y * 0.5f);
                else
                {
                    self.move.y = 0;
                    transform.position = new Vector2(transform.position.x, collision.height - (coll.offset.y - coll.size.y * 0.5f));
                }

                if (Mathf.Abs(self.move.x) > 1 * scale)
                    self.move.x *= 0.5f;
                else
                    self.move.x = 0;

            }
            else if(self.move.y > gravity)
            {
                self.move.y -= grav;
            }
            if (dCheck)
                self.move.y = Mathf.Abs(self.move.y * 0.5f);
            if (lCheck)
                self.move.x = -Mathf.Abs(self.move.x * 0.5f);
            if (rCheck)
                self.move.x = Mathf.Abs(self.move.x * 0.5f);

            transform.Translate(self.move);
        }
    }

    void Fly(bool i)
    {
        anim.SetBool("Fly", i);
    }

    void IsRight(bool i)
    {
        if (i == true)
        {
            transform.position = new Vector2(transform.parent.transform.position.x - 0.07f, transform.parent.transform.position.y - 0.43f);
            self.rTang = false;
            self.lTang = true;
        }
        else
        {
            transform.position = new Vector2(transform.parent.transform.position.x + 0.37f, transform.parent.transform.position.y - 0.43f);
            self.lTang = false;
            self.rTang = true;
        }
    }

    void GetVector(Vector2 i)
    {
        self.move = i;
        if (self.rTang == false)
        {
            self.move.x -= 1.5f * scale;
            self.rTang = true;
        }
        else if (self.lTang == false)
        {
            self.move.x += 1.5f * scale;
            self.lTang = true;
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
