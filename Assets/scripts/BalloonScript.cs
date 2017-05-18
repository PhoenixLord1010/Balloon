using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    private Vector2 moveVector = new Vector2(0, 0);
    private Animator anim;
    private float scale = 0.005f;

    // Use this for initialization
    void Start ()
    {
        anim = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(transform.parent == null)
        {
            if (moveVector.y < 1) moveVector.y += 0.07f * scale;
            else if (moveVector.y > 1) moveVector.y = 1;

            transform.Translate(moveVector);
        }
	}

    void GetState(int i)
    {
        if (i == 4) i = 0;
        anim.SetInteger("State", i);

        if ((i == 0 && anim.GetInteger("State") != 0) || (i == 3 && anim.GetInteger("State") != 3)) anim.SetTrigger("Walk");
    }

    void GetForm(int i)
    {
        anim.SetInteger("Form", i);
    }

    void IsRight(bool i)
    {
        anim.SetBool("isRight", i);
    }
}
