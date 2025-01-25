using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage;

    [SerializeField] private GameObject particleBubbles;
    [SerializeField] private float timeParticle;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        particleBubbles.SetActive(true);
        particleBubbles.transform.SetParent(null);
        Destroy(particleBubbles, timeParticle); 
        Destroy(gameObject);   
    }
}
