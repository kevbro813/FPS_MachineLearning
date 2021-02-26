using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentProjectile : MonoBehaviour
{
    Transform tf;
    public float projectileSpeed;

    private void Start()
    {
        tf = GetComponent<Transform>();
    }
    private void Update() => tf.position += tf.forward * Time.deltaTime * projectileSpeed;

    private void OnCollisionEnter(Collision col)
    {
        Destroy(this.gameObject);
    }
}
