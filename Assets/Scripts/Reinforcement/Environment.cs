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
    public int framesPerState = 5; // The number of frames per state
    public int frameSize = 8; // The number of inputs that comprise each frame
    public int stepCounter = 0; // TODO: Need to implement this
    public int epiMaxSteps = 1000; // Steps per episode
    public int stateSize;
    public int frameBufferSize = 2000; // The size of the frame buffer (Calculated when environment is initialized)
    public int fbIndex = 0;
    public int fbCount = 0;
    public float[][] frameBuffer; // An array to hold the frame buffer
    public bool isOnObjective = false;

    private AIPawn aiPawn; 
    private Material mats;
    private Transform tf;
    public float distance = 10.0f;
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

        distancesToObstacles = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        stateSize = (frameSize * framesPerState) + frameSize; // frameBufferSize is calculated by multiplying the FRAME_SIZE by FRAMES_PER_STATE which will give you the total size of a state
        // Then add FRAME_SIZE to account for the next frame (The frame buffer will be all the frames that comprise both the current and next state, remember most of the frames will be repeated for both states)
        // current state = [t-4 : t] and next state = [t-3: t+1] (t = current frame)
        frameBuffer = new float[frameBufferSize][];
    }
    // Initialize a new environment
    public void InitEnv()
    {
        // TODO: Create the environment

    }
    public void ResetEnv()
    {
        // TODO: Restart game
    }
    // Update the frame buffer by removing the oldest frame and appending the next frame
    public int UpdateFrameBuffer(float[] nf) // Pass in the next frame
    {
        if (fbIndex >= frameBufferSize) // If the frame buffer is not full...
        {
            fbIndex = 0;
        }

        frameBuffer[fbIndex] = nf; // Add the next frame to the frameBuffer

        fbCount = Mathf.Max(fbCount, fbIndex + 1);

        fbIndex = (fbIndex + 1) % frameBufferSize;
        // TODO: Make sure there is a method to fix bug when the frame buffer loops back through.

        return fbIndex; // Return the frameBuffer Index. This will be added to a tuple and the state/next state can be determined using this int
    }
    // Get the next_Frame array data from raycasts, colliders, etc.
    public float[] GetNextFrame()
    {
        CollisionDetection(); // CollisionDetection function will collect the distance to objects from the raycasts pointed in the 8 compass directions

        float[] next_Frame = new float[frameSize]; // Create a float array  to hold the next frame

        // Populate the next_Frame array with the respective values
        next_Frame[0] = distancesToObstacles[0];
        next_Frame[1] = distancesToObstacles[1];
        next_Frame[2] = distancesToObstacles[2];
        next_Frame[3] = distancesToObstacles[3];
        next_Frame[4] = distancesToObstacles[4];
        next_Frame[5] = distancesToObstacles[5];
        next_Frame[6] = distancesToObstacles[6];
        next_Frame[7] = distancesToObstacles[7];

        // ***Possible inputs
        // Distance to target (while in sight)
        // Health
        // Cover objects in sight
        // Enemy location (while in sight)
        // The direction the enemy is facing if sighted
        // Current AI state

        return next_Frame; // Return next_Frame
    }
    // Get the current state by taking the oldest frames
    public float[] GetState(float[][] buffer, int frameIndex)
    {
        float[][] state = new float[framesPerState][]; // Create a new array to hold the state
        float[] flatState = new float[framesPerState * frameSize];

        if (buffer[frameIndex] != null && fbCount - framesPerState > 0)
        {
            for (int i = 0; i < framesPerState; i++) // Loop through the frames, stopping before the last frame
            {  
                state[framesPerState - i - 1] = buffer[frameIndex];
            }
            int indx = 0;
            for (int j = 0; j < framesPerState; j++)
            {
                for (int k = 0; k < frameSize; k++)
                {
                    flatState[indx] = state[j][k];
                    indx++;
                }
            }    
        }
        return flatState; // Return the current state
    }
    // TODO: Calculate the reward
    public float CalculateReward(float[] nf)
    {  
        float reward = 0;
        if (isOnObjective)
        {
            Debug.Log("Reward!");
            reward++;
        }
        for (int i = 0; i < distancesToObstacles.Length; i++)
        {
            if (distancesToObstacles[i] < 3 && distancesToObstacles[i] != 0)
            {
                reward--;
            }
        }
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
                    if (distanceToHit <= collisionDetectRange)
                    {
                        distancesToObstacles[hitDir] = distanceToHit; // Set the distance to the hit object in the distancesToObjects array
                    }
                    else
                    {
                        distancesToObstacles[hitDir] = 0f;
                    }
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

}
