using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private Transform ttf;
    private GameObject targetObject;
    private float targetDistance;
    private Transform tf;
    private Vector3 vectorToTarget;
    public float angleToTarget;
    public GameObject projectileObject;
    public Transform muzzle;
    [HideInInspector] public float timerStart;
    private bool isReadyToFire;
    public float turretDelay;
    public float turretPower;
    public Transform projectile_tf;

    private void Start()
    {
        tf = GetComponent<Transform>();
        timerStart = Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Agent"))
        {
            targetObject = other.gameObject; // Get target
            ttf = targetObject.GetComponent<Transform>();

            vectorToTarget = ttf.position - tf.position;
            // Get target position
            targetDistance = Vector3.Distance(ttf.position, tf.position);

            // Look towards target
            angleToTarget = Vector3.Angle(vectorToTarget, tf.forward);

            // Raycast vectorToTarget
            if (angleToTarget < RLManager.instance.settings.fieldOfView && targetDistance < RLManager.instance.settings.maxViewDistance)
            {
                RaycastHit hit;
                if (Physics.Raycast(tf.position, vectorToTarget, out hit))
                {
                    //tf.LookAt(hit.point);
                    Vector3 newDirection = Vector3.RotateTowards(tf.forward, vectorToTarget, 5 * Time.deltaTime, 0.0f);
                    tf.rotation = Quaternion.LookRotation(newDirection);

                    ReadyCheck();

                    if (isReadyToFire)
                    {
                        // Fire projectile at target
                        GameObject projectileClone = Instantiate(projectileObject, muzzle.position, muzzle.rotation);
                        //projectileClone.GetComponent<Rigidbody>().AddForce(muzzle.forward * turretPower);

                        projectile_tf = projectileClone.GetComponent<Transform>();

                        StartCoroutine(DestroyProjectile(projectileClone));

                        timerStart = Time.time;
                        isReadyToFire = false;
                    }
                }
            }
        }

    }

    private IEnumerator DestroyProjectile(GameObject projectileClone)
    {
        yield return new WaitForSeconds(3f);
        GameObject.FindWithTag("Agent").GetComponent<RLComponent>().env.doesProjectileMiss = false;
        Destroy(projectileClone);
    }
    private void ReadyCheck()
    {
        if (Time.time >= timerStart + turretDelay) isReadyToFire = true;
    }
    /// <summary>
    /// AI Vision is limited to field of view and max view distance
    /// </summary>
    /// <param name="vectorToTarget"></param>
    /// <param name="ttf"></param>
    private void AIVision(Vector3 vectorToTarget, Transform ttf)
    {
        // Find the distance between the two vectors in float to compare with maxViewDistance
        targetDistance = Vector3.Distance(ttf.position, tf.position);

        // Find the angle between the direction our agent is facing (forward in local space) and the vector to the target.
        float angleToTarget = Vector3.Angle(vectorToTarget, tf.forward);

        if (angleToTarget < RLManager.instance.settings.fieldOfView && targetDistance < RLManager.instance.settings.maxViewDistance)
        {
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");             // Add Walls layer to variable
            int objectiveLayer = LayerMask.NameToLayer("Objective");           // Add Player layer to variable
            int layerMask = (1 << obstacleLayer) | (1 << objectiveLayer);      // Create layermask

            RaycastHit hit;

            // Raycast to detect objective within field of view with max view distance as a limit
            if (Physics.Raycast(tf.position, vectorToTarget, out hit, RLManager.instance.settings.maxViewDistance, layerMask))
            {
                if (hit.collider.CompareTag("Objective"))
                {
                    // boolean to indicate objective is sighted (state input)
                    // angleToTarget (state input)
                }
            }
        }
    }
}
