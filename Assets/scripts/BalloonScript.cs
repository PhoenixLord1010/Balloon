﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    private Animator anim;
    private EntityScript self;

    private float scale = 0.005f;

    // Use this for initialization
    void Start ()
    {
        name = "Balloon";
        anim = this.GetComponent<Animator>();
        self = this.GetComponent<EntityScript>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(transform.parent == null)
        {
            if (self.move.y < 1) self.move.y += 0.07f * scale;
            else if (self.move.y > 1) self.move.y = 1;

            if (anim.GetInteger("State") != 3)
            {
                anim.SetInteger("State", 3);
                anim.SetTrigger("Walk");
            }

            transform.Translate(self.move);
        }
	}

    void GetVector(Vector2 i)
    {
        self.move.x = i.x / 3;
        self.move.y = i.y / 5;
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
}
