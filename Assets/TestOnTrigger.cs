using UnityEngine;

public class TestOnTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hola");
    }
}
