using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPawn : MonoBehaviour
{
    public GameObject projectileObject;
    [HideInInspector] public GameObject turretObject;
    private Transform tf;
    private Transform ttf;
    public Transform faceTf;
    [HideInInspector] public Transform projectile_tf;
    public Transform muzzle;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    private RaycastHit hit;
    private Vector3 vectorToTarget;
    private bool isReadyToFire;
    [HideInInspector] public float timerStart;
    [HideInInspector] public float angleToTarget;
    private float targetDistance;
    public float turretDelay;
    [HideInInspector] public bool isTurretVisible;

    private void Awake()
    {
        tf = GetComponent<Transform>();
        turretObject = RLManager.instance.turretTf.gameObject;
    }
    private void Start()
    {
        timerStart = Time.time;
    }
    private void Update()
    {
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit))
        {
            if (hit.collider.tag == "Turret")
            {
                isTurretVisible = true;
                //Debug.DrawLine(muzzle.position - (muzzle.up * 0.1f), hit.point, Color.white, 1f);
            }
            else
            {
                isTurretVisible = false;
            }
        }
        Attack();
    }
    // BOOLEAN VERSION OF ACTIONS USED WITH REINFORCEMENT ALGORITHM
    public void MoveForward(bool input)
    {
        if (input)
        {
            tf.Translate((tf.forward * moveSpeed * Time.deltaTime), Space.World);
        }      
    }
    public void MoveBack(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.forward * moveSpeed * Time.deltaTime), Space.World);
        }     
    }
    public void MoveRight(bool input)
    {
        if (input)
        {
            tf.Translate((tf.right * moveSpeed * Time.deltaTime), Space.World);
        }       
    }
    public void MoveLeft(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.right * moveSpeed * Time.deltaTime), Space.World);
        }       
    }
    public void RotateRight(bool input)
    {
        if (input)
        {
            tf.Rotate((Vector3.up * rotationSpeed * Time.deltaTime), Space.World);
        }        
    }
    public void RotateLeft(bool input)
    {
        if (input)
        {
            tf.Rotate((-Vector3.up * rotationSpeed * Time.deltaTime), Space.World);
        } 
    }
    public void NoMovement(bool input)
    {
        // Do nothing.
    }

    private void ReadyCheck()
    {
        if (Time.time >= timerStart + turretDelay) isReadyToFire = true;
    }
    public void Attack()
    {
        ttf = turretObject.GetComponent<Transform>();

        vectorToTarget = ttf.position - faceTf.position;
        // Get target position
        targetDistance = Vector3.Distance(ttf.position, faceTf.position);

        // Look towards target
        angleToTarget = Vector3.Angle(vectorToTarget, faceTf.forward);

        // Raycast vectorToTarget
        if (angleToTarget < RLManager.instance.settings.fieldOfView && targetDistance < RLManager.instance.settings.maxViewDistance)
        {
            if (Physics.Raycast(faceTf.position, vectorToTarget))
            {
                //tf.LookAt(hit.point);
                Vector3 newDirection = Vector3.RotateTowards(faceTf.forward, vectorToTarget, 5 * Time.deltaTime, 0.0f);
                faceTf.rotation = Quaternion.LookRotation(newDirection);

                ReadyCheck();

                if (isReadyToFire && isTurretVisible)
                {
                    // Fire projectile at target
                    GameObject projectileClone = Instantiate(projectileObject, muzzle.position, muzzle.rotation);

                    projectile_tf = projectileClone.GetComponent<Transform>();

                    //if (!rlComponent) rlComponent = GameObject.FindWithTag("Agent").GetComponent<RLComponent>();

                    StartCoroutine(DestroyProjectile(projectileClone));
                    //rlComponent.env.isProjectileActive = false;

                    timerStart = Time.time;
                    isReadyToFire = false;
                }
            }
        }
    }
    private IEnumerator DestroyProjectile(GameObject projectileClone)
    {
        yield return new WaitForSeconds(3f);
        Destroy(projectileClone);
    }
}
