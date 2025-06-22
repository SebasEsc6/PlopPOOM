using System.Collections;
using UnityEngine;

public class PowerUp_SpeedBoost : PowerUpBase
{
    public override IEnumerator ApplyPowerUp(GameObject player)
    {
        //TODO: Create a public currentSpeed variable and also a deafultSpeed
        NetworkStatsController networkStats= player.GetComponent<NetworkStatsController>();
        // networkStats.currentSpeed.Value += sO_PowerUps.valueToIncrease;
        yield return new WaitForSeconds(sO_PowerUps.lifeTime);
        // networkStats.currentSpeed.Value += networkStats.deafultSpeed.Value;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(ApplyPowerUp(collision.gameObject));
            OnPickedUp(collision.gameObject);
        }
    }
}
