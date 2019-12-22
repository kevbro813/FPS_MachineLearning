using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
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
    public string aiState = "patrol";

    void Start()
    {
        tf = GetComponent<Transform>();
        aiPawn = GetComponent<AIPawn>();
        distancesToObstacles = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        mats = GetComponent<MeshRenderer>().material;
        ttf = GameManager.instance.objective.GetComponent<Transform>();
        lastLocation = GameManager.instance.spawnpoint.position;
    }

    private void Update()
    {
        if (aiState == "patrol")
        {
            // The AI Wanders around looking for a target
        }
        if (aiState == "alert")
        {
            // The AI sees the enemy
        }
        if (aiState == "obstacle")
        {
            // The AI encounters an obstacle
        }
        if (aiState == "threat")
        {
            // The AI is attacked without seeing the enemy
        }
        if (aiState == "cover")
        {
            // The AI is behind cover while being attacked
        }
        if (aiState == "advance")
        {
            // The AI can advance to the next cover
        }
        if (aiState == "attack")
        {
            // The AI attacks the enemy
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (initialized == true)
        {
            ColorChange(); // Function to change the color of the mesh to represent different AI types
            Vector3 vectorToTarget = (ttf.position - tf.position).normalized; // Used for target input function and AIVision
            float[] inputs = new float[10]; // Initialize an array to hold inputs

            CollisionDetection(); // Collision Detection function subtracts fitness when hitting a wall

            // Set input values in array

            // First two are angles to target
            inputs[0] = GetTargetInput(vectorToTarget.x, vectorToTarget.z);
            inputs[1] = GetTargetInput(vectorToTarget.z, vectorToTarget.x);

            // Raycasts in the 8 compass points around the AI to detect when it is running into an obstacle
            inputs[2] = distancesToObstacles[0];
            inputs[3] = distancesToObstacles[1];
            inputs[4] = distancesToObstacles[2];
            inputs[5] = distancesToObstacles[3];
            inputs[6] = distancesToObstacles[4];
            inputs[7] = distancesToObstacles[5];
            inputs[8] = distancesToObstacles[6];
            inputs[9] = distancesToObstacles[7];

            // ***Possible inputs
            // Distance to target (while in sight)
            // Health
            // Cover objects in sight
            // Enemy location (while in sight)
            // The direction the enemy is facing if sighted
            // Current AI state


            // Pass inputs to feed forward function to produce outputs
            float[] outputs = net.FeedForward(inputs);

            // Output values passed to aiPawn functions
            aiPawn.MoveForward(outputs[0]);
            aiPawn.MoveBack(outputs[1]);
            aiPawn.MoveRight(outputs[2]);
            aiPawn.MoveLeft(outputs[3]);
            aiPawn.RotateRight(outputs[4]);
            aiPawn.RotateLeft(outputs[5]);

            // ***Fitness Modifiers***

            AIVision(vectorToTarget); // Add fitness when target is in sight and as the AI moves closer
            CollisionFitness(outputs); // AI will stay off the walls, but they gravitate safely around in a circle
            DistanceTraveled(); // Makes the AI want to move more

            // Add fitness for 
            for (int input = 0; input < 2; input++)
            {
                //net.AddFitness((1f - Mathf.Abs(inputs[input]))); // When facing the target, it will add 1 - abs(radians / pi) to fitness every update
            }
        }
    }
    public void DistanceTraveled()
    { 
        // Incentivize moving as far away from spawnpoint as possible
        float distanceFromSpawn = Vector3.Distance(tf.position, GameManager.instance.spawnpoint.position);
        net.AddFitness(GameManager.instance.distanceReward * distanceFromSpawn);

        // Incentivize moving as much as possible
        float dt = Vector3.Distance(lastLocation, tf.position);
        net.AddFitness(GameManager.instance.distanceReward * dt);
        lastLocation = tf.position;
    }
    public void CollisionFitness(float[] outputs)
    {
        // Add fitness when AI is moving forward unobstructed
        if (outputs[0] < 0 && distancesToObstacles[0] == 0)
        {
            net.AddFitness(GameManager.instance.movingForwardReward);
        }
        for (int i = 0; i < distancesToObstacles.Length; i++)
        {
            if (distancesToObstacles[i] > 0)
            {
                net.AddFitness(GameManager.instance.obstacleCollisionPenalty);
            }
        }
        if (outputs[2] > 0)
        {
            net.AddFitness(GameManager.instance.rotationPenalty);
        }
        // Add fitness when the AI is moving laterally
        //if (outputs[1] > 0)
        //{
        //    net.AddFitness(GameManager.instance.movingLateralReward);
        //}
    }
    // Initialize the neural network for this AI
    public void Init(NeuralNetwork net)
    {
        this.net = net;
        initialized = true;
    }
    // Collision detection with obstacles
    private void OnCollisionEnter(Collision col)
    {
        //if (col.gameObject.CompareTag("Obstacle"))
        //{
        //    GameManager.instance.aiControllerList.Remove(this.gameObject.GetComponent<AIController>());
        //    Destroy(this.gameObject);
        //}
    }
    // Change color depending on AItype and proximity to objective
    public void ColorChange()
    {
        float dist = Vector3.Distance(tf.position, GameManager.instance.objective.GetComponent<Transform>().position);
        if (net.aiType == NeuralNetwork.AIType.Fittest)
        {
            mats.color = new Color((1 - 0.2f), 0, 0); // Red are the fittest
            if (dist < 20f)
            {
                mats.color = new Color(((20f / dist) - 0.2f), 0, 0); // Brighten when close to objective
            }
        }
        if (net.aiType == NeuralNetwork.AIType.Fit)
        {
            mats.color = new Color(0, (1 - 0.2f), 0); // Green are fit
            if (dist < 20f)
            {
                mats.color = new Color(0, ((20f / dist) - 0.2f), 0); // Brighten when close to objective
            }
        }
        if (net.aiType == NeuralNetwork.AIType.Children)
        {
            mats.color = new Color((1 - 0.2f), (1 - 0.2f), 0); // Yellow are Children
            if (dist < 20f)
            {
                mats.color = new Color(((20f / dist) - 0.2f), ((20f / dist) - 0.2f), 0); // Brighten when close to objective
            }
        }
        if (net.aiType == NeuralNetwork.AIType.Random)
        {
            mats.color = new Color((1 - 0.2f), (1 - 0.2f), (1 - 0.2f)); // White are new random
            if (dist < 20f)
            {
                mats.color = new Color(((20f / dist) - 0.2f), ((20f / dist) - 0.2f), ((20f / dist) - 0.2f)); // Brighten when close to objective
            }
        }
        if (net.aiType == NeuralNetwork.AIType.Saved)
        {
            mats.color = new Color(0, 0, (1 - 0.2f)); // Blue are saved
            if (dist < 20f)
            {
                mats.color = new Color(0, 0, ((20f / dist) - 0.2f)); // Brighten when close to objective
            }
        }
        if (net.aiType == NeuralNetwork.AIType.Survivor)
        {
            mats.color = new Color((1 - 0.2f), 0, (1 - 0.2f)); // Purple are survivors
            if (dist < 20f)
            {
                mats.color = new Color((1 - 0.2f), 0, ((20f / dist) - 0.2f)); // Brighten when close to objective
            }
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
                    reward = GameManager.instance.targetDistReward * (targetDistance/maxViewDistance); // Applies a bigger reward the closer to the target
                    net.AddFitness(reward);
                    Debug.DrawRay(tf.position, vectorToTarget * maxViewDistance, Color.red);
                    net.AddFitness(GameManager.instance.inSightReward); // Applies a reward when the target is in sight
                }
            }
        }
    }
    // Currently, the target inputs are used as neural network inputs (This will change eventually once reinforcement learning is introduced)
    public float GetTargetInput(float a, float b)
    {
        float angle = tf.eulerAngles.z % 360f;
        if (angle < 0f)
            angle += 360f; // Get the mod of the z angle and set it to a float

        float rad;

        rad = Mathf.Atan2(a, b); // Atan2 function between float a and b, both passed in.

        rad *= Mathf.Rad2Deg; // Convert the radians to degrees

        // Calculate the degrees for the angle
        rad = rad % 360;
        if (rad < 0) 
        {
            rad = 360 + rad;
        }

        rad = 90f - rad;
        if (rad < 0f)
        {
            rad += 360f;
        }
        rad = 360 - rad;
        rad -= angle;
        if (rad < 0)
            rad = 360 + rad;
        if (rad >= 180f)
        {
            rad = 360 - rad;
            rad *= -1f;
        }
        rad *= Mathf.Deg2Rad; // Convert degrees back to radians

        rad /= Mathf.PI; // Divide radians by PI to get a value between 0 and 1.

        return rad; // Return rad
    }
}
