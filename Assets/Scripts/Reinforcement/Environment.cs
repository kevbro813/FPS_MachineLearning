using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public float[] states;
    public float[] stateMeans;
    public float[] stateVariance;
    public float[] stateStdDev;
    public float[] normalizedStates;

    public float[] rewards;

    public const int INPUT_QTY = 10;
    public float stateCounter = 1;

    public bool initialized = false;
    private AIPawn aiPawn;
    public NeuralNetwork net;
    private Material mats;
    private Transform tf;
    public float distance = 5.0f;
    public RaycastHit[] hit;
    public Vector3[] directions;
    public float[] distancesToObstacles;
    public string aiType;
    public Transform ttf;
    public float targetDistance;
    public float maxViewDistance = 100f;
    public float fieldOfView = 45f;
    public float collisionDetectRange = 1f;
    public float reward;
    public Vector3 lastLocation;

    private void Start()
    {
        tf = GetComponent<Transform>();
        aiPawn = GetComponent<AIPawn>();
        distancesToObstacles = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        mats = GetComponent<MeshRenderer>().material;
        ttf = GameManager.instance.objective.GetComponent<Transform>();
        lastLocation = GameManager.instance.spawnpoint.position;

        InitEnv();
    }
    public void InitEnv()
    {
        initialized = true;
        // Create the environment
        states = new float[INPUT_QTY];
        stateMeans = new float[INPUT_QTY];
        stateVariance = new float[INPUT_QTY];
        stateStdDev = new float[INPUT_QTY];
        normalizedStates = new float[INPUT_QTY];
    }
    public void ResetEnv()
    {
        // Create the environment
        states = new float[INPUT_QTY];
        stateMeans = new float[INPUT_QTY];
        stateVariance = new float[INPUT_QTY];
        stateStdDev = new float[INPUT_QTY];
        normalizedStates = new float[INPUT_QTY];

        // TODO: Restart game
    }
    public void Step(float[] actions)
    {
        // Take in an action and perform it.
        // Make next state (Append next frame to state to get next state for next iteration of the loop)
        // Calculate the reward
        // isDone = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (initialized == true)
        {
            CollisionDetection();

            // Raycasts in the 8 compass points around the AI to detect when it is running into an obstacle
            states[0] = distancesToObstacles[0];
            states[1] = distancesToObstacles[1];
            states[2] = distancesToObstacles[2];
            states[3] = distancesToObstacles[3];
            states[4] = distancesToObstacles[4];
            states[5] = distancesToObstacles[5];
            states[6] = distancesToObstacles[6];
            states[7] = distancesToObstacles[7];
            states[8] = 0;
            states[9] = 0;

            // ***Possible inputs
            // Distance to target (while in sight)
            // Health
            // Cover objects in sight
            // Enemy location (while in sight)
            // The direction the enemy is facing if sighted
            // Current AI state

            // Pass inputs to feed forward function to produce outputs
            float[] outputs = net.FeedForward(normalizedStates);

            // Output values passed to aiPawn functions
            aiPawn.ForwardBackMovement(outputs[0]);
            aiPawn.LateralMovement(outputs[1]);
            aiPawn.Rotation(outputs[2]);
        }
    }
    // Collision Detection Algorithm (Experimental)
    private void CollisionDetection()
    {
        hit = new RaycastHit[8];
        int hitDir = hit.Length;

        // Directions Start forward then moves clockwise
        directions = new Vector3[] { new Vector3(0, 0, distance), new Vector3(distance, 0, distance),
            new Vector3(distance, 0, 0), new Vector3(distance, 0, -distance),  new Vector3(0, 0, -distance),
            new Vector3(-distance, 0, -distance), new Vector3(-distance, 0, 0), new Vector3(-distance, 0, distance) };

        while (hitDir > 0)
        {
            hitDir--;
            if (Physics.Raycast(tf.position, directions[hitDir], out hit[hitDir], distance))
            {
                //Debug.DrawRay(tf.position, directions[hitL], Color.blue, distance);

                Vector3 hitLocation = hit[hitDir].point;

                float distanceToHit = Vector3.Distance(hitLocation, tf.position);

                // If the raycast hits an obstacle...
                if (hit[hitDir].collider.CompareTag("Obstacle"))
                {
                    // Check if the obstacle is within the collision detection range
                    if (distanceToHit < collisionDetectRange)
                    {
                        distancesToObstacles[hitDir] = distanceToHit; // Set the distance to the hit object in the distancesToObjects array
                    }
                }
                // If the raycasthits anything else, reset distances to 0
                else
                {
                    distancesToObstacles[hitDir] = 0f;
                }
            }
        }
    }
    // AI Vision is limited to field of view and max view distance
    private void AIVision(Vector3 vectorToTarget)
    {
        // Find the distance between the two vectors in float to compare with maxViewDistance
        targetDistance = Vector3.Distance(ttf.position, tf.position);

        // Find the angle between the direction our agent is facing (forward in local space) and the vector to the target.
        float angleToTarget = Vector3.Angle(vectorToTarget, tf.forward);

        if (angleToTarget < fieldOfView && targetDistance < maxViewDistance)
        {
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");             // Add Walls layer to variable
            int objectiveLayer = LayerMask.NameToLayer("Objective");           // Add Player layer to variable
            int layerMask = (1 << obstacleLayer) | (1 << objectiveLayer);      // Create layermask

            RaycastHit hit;

            // Raycast to detect objective within field of view with max view distance as a limit
            if (Physics.Raycast(tf.position, vectorToTarget, out hit, maxViewDistance, layerMask))
            {
                if (hit.collider.CompareTag("Objective"))
                {
                    reward = GameManager.instance.targetDistReward * (targetDistance / maxViewDistance); // Applies a bigger reward the closer to the target
                    net.AddFitness(reward);
                    Debug.DrawRay(tf.position, vectorToTarget * maxViewDistance, Color.red);
                    net.AddFitness(GameManager.instance.inSightReward); // Applies a reward when the target is in sight
                }
            }
        }
    }
}
