using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Transform tf;
    public float projectileSpeed;
    private void OnEnable()
    {
        tf = GetComponent<Transform>();
    }
    private void Update()
    {
        tf.position += tf.forward * Time.deltaTime * projectileSpeed;
    }
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            col.gameObject.GetComponent<RLComponent>().env.isHitByProjectile = true;
            Debug.Log("Agent hit");
        }
        Destroy(this.gameObject);
    }
}
