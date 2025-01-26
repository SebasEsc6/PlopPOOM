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
        // Activate the particle system
        particleBubbles.SetActive(true);

        // Detach the particle system from the current parent
        particleBubbles.transform.SetParent(null);

        Vector3 positiveScale = particleBubbles.transform.localScale;
        positiveScale.x = Mathf.Abs(positiveScale.x);
        positiveScale.y = Mathf.Abs(positiveScale.y);
        positiveScale.z = Mathf.Abs(positiveScale.z);
        particleBubbles.transform.localScale = positiveScale;

        // Destroy the particle system after a set amount of time
        Destroy(particleBubbles, timeParticle);

        // Destroy this game object
        Destroy(gameObject);
    }

}
