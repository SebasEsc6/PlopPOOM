using System.Collections;
using UnityEngine;

public class PowerUpBase : PickableBase
{
    [SerializeField] protected SO_PowerUps sO_PowerUps;

    public virtual IEnumerator ApplyPowerUp(GameObject player)
    {
        Debug.Log("Start PowerUp");
        yield return new WaitForSeconds(sO_PowerUps.lifeTime);
        Debug.Log("Finished PowerUp");
    }
}
