using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        Destroy(gameObject);   
    }
}
