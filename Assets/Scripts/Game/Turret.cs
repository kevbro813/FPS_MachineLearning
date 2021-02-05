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
    public Transform projectile_tf;
    public RLComponent rlComponent;
    public RaycastHit hit;
    public bool isTargetVisible;

    private void Start()
    {
        tf = GetComponentInParent<Transform>();
        timerStart = Time.time;
    }
    private void Update()
    {
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit))
        {
            if (hit.collider.tag == "CoverObject")
            {
                //Debug.Log("Target not visible");
                isTargetVisible = false;
                Debug.DrawLine(muzzle.position - (muzzle.up * 0.1f), hit.point, Color.green, 1f);
            }
            else
            {
                //Debug.Log("Target is visible");
                isTargetVisible = true;
                Debug.DrawLine(muzzle.position - (muzzle.up * 0.1f), hit.point, Color.red, 1f);
            }
        }

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
                if (Physics.Raycast(tf.position, vectorToTarget))
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

                        if (!rlComponent) rlComponent = GameObject.FindWithTag("Agent").GetComponent<RLComponent>();

                        StartCoroutine(DestroyProjectile(projectileClone));
                        rlComponent.env.isProjectileActive = false;

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
        Destroy(projectileClone);
    }
    private void ReadyCheck()
    {
        if (Time.time >= timerStart + turretDelay) isReadyToFire = true;
    }
}
