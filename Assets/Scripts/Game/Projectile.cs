using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Transform tf;
    public float projectileSpeed;
    public RLComponent rlComponent;
    private void OnEnable()
    {
        tf = GetComponent<Transform>();
        rlComponent = GameObject.FindWithTag("Agent").GetComponent<RLComponent>();

    }
    private void Update()
    {
        tf.position += tf.forward * Time.deltaTime * projectileSpeed;
    }
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            rlComponent.env.isHitByProjectile = true;
            Debug.Log("Agent hit");
        }
        else
        {
            rlComponent.env.doesProjectileMiss = true;
        }
        Destroy(this.gameObject);
    }
}
