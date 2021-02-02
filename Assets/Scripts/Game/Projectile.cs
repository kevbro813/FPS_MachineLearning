using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Transform tf;
    public float projectileSpeed;
    public RLComponent rlComponent;

    private void Start()
    {
        tf = GetComponent<Transform>();
        rlComponent = GameObject.FindWithTag("Agent").GetComponent<RLComponent>();
        rlComponent.env.isProjectileActive = true;
    }
    private void OnDestroy()
    {
        rlComponent.env.isProjectileActive = false;
    }
    private void Update() => tf.position += tf.forward * Time.deltaTime * projectileSpeed;

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            rlComponent.env.isHitByProjectile = true;
            Destroy(this.gameObject);
        }

        else if (!col.gameObject.CompareTag("Turret"))
        {
            rlComponent.env.doesProjectileMiss = true;
            Destroy(this.gameObject);
        }
        else if (col.gameObject.CompareTag("Turret"))
        {
            Debug.Log("Turret hit");
        }
    }
}
