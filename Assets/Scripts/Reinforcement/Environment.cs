using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    //*** For testing normalizing functions
    //public float[] states;
    //public float[] stateMeans;
    //public float[] stateVariance;
    //public float[] stateStdDev;
    //public float[] normalizedStates;

    // Required for DQN
    public const int FRAMES_PER_STATE = 5; // The number of frames per state
    public const int FRAME_SIZE = 10;
    public float stateCounter = 0;

    public int frameBufferSize;
    public float[] frameBuffer;
    public float[] rewards;

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
    public float[] reward;
    public Vector3 lastLocation;
    public DQN dqn;

    private void Start()
    {
        tf = GetComponent<Transform>();
        aiPawn = GetComponent<AIPawn>();      
        mats = GetComponent<MeshRenderer>().material;
        ttf = GameManager.instance.objective.GetComponent<Transform>();
        lastLocation = GameManager.instance.spawnpoint.position;
        dqn = GetComponent<DQN>();
        InitEnv();
        frameBufferSize = (FRAME_SIZE * FRAMES_PER_STATE) + FRAME_SIZE;
    }
    public void InitEnv()
    {
        initialized = true;
        // Create the environment
        distancesToObstacles = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        stateCounter = 0;
    }
    public void ResetEnv()
    {
        // Create the environment
        // TODO: Restart game
    }
    public float[] UpdateFrameBuffer(float[] nf)
    {
        List<float> fb = new List<float>();

        for (int i = 0; i < frameBufferSize; i++)
        {
            fb.Add(frameBuffer[i + FRAME_SIZE]);
        }
        for(int i = 0; i < FRAME_SIZE; i++)
        {
            fb.Add(nf[i]);
        }

        return fb.ToArray();
    }
    public float[] GetNextFrame()
    {
        // Make next state (Append next frame to state to get next state for next iteration of the loop)
        CollisionDetection();

        float[] next_Frame = new float[FRAME_SIZE];

        // Raycasts in the 8 compass points around the AI to detect when it is running into an obstacle
        next_Frame[0] = distancesToObstacles[0];
        next_Frame[1] = distancesToObstacles[1];
        next_Frame[2] = distancesToObstacles[2];
        next_Frame[3] = distancesToObstacles[3];
        next_Frame[4] = distancesToObstacles[4];
        next_Frame[5] = distancesToObstacles[5];
        next_Frame[6] = distancesToObstacles[6];
        next_Frame[7] = distancesToObstacles[7];
        next_Frame[8] = 0;
        next_Frame[9] = 0;

        // ***Possible inputs
        // Distance to target (while in sight)
        // Health
        // Cover objects in sight
        // Enemy location (while in sight)
        // The direction the enemy is facing if sighted
        // Current AI state

        return next_Frame;
    }
    public float[] GetState(float[] buffer)
    {
        float[] state = new float[FRAME_SIZE * FRAMES_PER_STATE];

        for(int i = 0; i < (FRAME_SIZE * FRAMES_PER_STATE) - FRAME_SIZE; i++)
        {
            state[i] = buffer[i];
        }

        return state;  
    }
    public float[] GetNextState(float[] buffer)
    {
        float[] nState = new float[FRAME_SIZE * FRAMES_PER_STATE];

        for (int i = FRAME_SIZE; i < (FRAME_SIZE * FRAMES_PER_STATE) + FRAME_SIZE; i++)
        {
            nState[i] = buffer[i];
        }

        return nState;
    }
    public float CalculateReward()
    {
        float reward = 0;
        // Calculate the reward
        return reward;
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
                    // boolean to indicate objective is sighted (state input)
                    // angleToTarget (state input)
                }
            }
        }
    }
    // Scalar function used to normalize all input values that make up a state
    public float[] NormalizeState(float[] means, float[] states, float counter, float[] variances, float[] stdDevs) // TODO: Should be normalize frame, need to change all states array to frame 
    {
        float[] normalizedStates = new float[states.Length];

        // Iterate scalar function through each state input
        for (int i = 0; i < states.Length; i++)
        {
            // Update the mean with the new datapoint
            means[i] = UpdateMean(counter, means[i], states[i]);

            // Calculate the squared difference for the new datapoint
            float sqrdDiff = SquaredDifference(states[i], means[i]);

            // Calculate total squared difference from variance
            float totalSqrdDiff = variances[i] * counter;

            // Update the total squared difference
            totalSqrdDiff += sqrdDiff;

            counter++; // TODO: Need to move this. It should only update once per tick, not once per 

            // Recalculate Variance and Standard Deviation
            variances[i] = Variance(totalSqrdDiff, counter);
            stdDevs[i] = StdDeviation(variances[i]);

            // Normalize the current state values
            normalizedStates[i] = ScalarFunction(states[i], means[i], stdDevs[i]);
        }

        return normalizedStates;
    }
    public float ScalarFunction(float dp, float mean, float stdDev)
    {
        // Calculate a state's Z-score = (data point - mean) / standard deviation
        float zScore = (dp - mean) / stdDev;

        return zScore;
    }
    public float UpdateMean(float sampleSize, float mean, float dp)
    {
        float sampleTotal = sampleSize * mean;
        sampleTotal += dp;
        sampleSize++;
        mean = sampleTotal / sampleSize;

        return mean;
    }
    public float SquaredDifference(float dp, float mean)
    {
        float sqrdDiff = Mathf.Pow(dp - mean, 2);
        return sqrdDiff;
    }
    public float Variance(float totalSqrdDiff, float stateCounter)
    {
        float variance = totalSqrdDiff / stateCounter;
        return variance;
    }
    public float StdDeviation(float variance)
    {
        float stdDev = Mathf.Sqrt(variance);
        return stdDev;
    }
}
