using UnityEngine;

public class BulletParticleVFX : MonoBehaviour
{
    [SerializeField] private float reattachDelay = 1f;
    private ParticleSystem _ps;
    private Transform _returnParent;
    private bool _returnPending;
    private float _returnAt;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        if (!_ps) Debug.LogError("[BulletParticleVFX] Missing ParticleSystem.");
    }

    public void PlayDetached(Transform returnParent, Vector3 worldPos)
    {
        Debug.Log("PlayDetached");
        transform.SetParent(null, true);
        transform.position = worldPos;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        _returnParent = returnParent;
        
        gameObject.SetActive(true);

        if (_ps)
        {
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _ps.Play(true);
        }

        _returnPending = true;
        _returnAt = Time.time + reattachDelay;
    }

    private void Update()
    {
        if (_returnPending && Time.time >= _returnAt)
        {
            _returnPending = false;
            if (_ps) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ReattachAndHide();
        }
    }

    private void ReattachAndHide()
    {
        if (_returnParent) transform.SetParent(_returnParent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
}
