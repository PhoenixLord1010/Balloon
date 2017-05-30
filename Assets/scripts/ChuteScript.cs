using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuteScript : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    void Rip()
    {
        anim.SetTrigger("Rip");
    }
}
