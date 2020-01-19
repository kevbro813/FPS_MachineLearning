using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Environment
{
    private DQN dqn;
    private Transform tf;
    private Transform ttf; // TODO: Need to set to target when sighted

    public float[][] frameBuffer; // An array to hold the frame buffer
    public float[] reward;

    private RaycastHit[] hit;
    private Vector3[] directions;
    public float[] distancesToObstacles;  
    public float targetDistance;
    public int fbIndex;
    public int fbCount;
    public bool isOnObjective = false;

    // Initialize a new environment
    public void InitEnv(Transform agent, DQN d)
    {
        // TODO: Create the environment
        tf = agent;
        dqn = d;
        fbIndex = 0;
        fbCount = 0;
        distancesToObstacles = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        frameBuffer = new float[dqn.frameBufferSize][];
    }
    public void ResetEnv()
    {
        // TODO: Restart game
    }
    // Update the frame buffer by removing the oldest frame and appending the next frame
    public int UpdateFrameBuffer(float[] nf) // Pass in the next frame
    {
        if (fbIndex >= dqn.frameBufferSize) // If the frame buffer is not full...
        {
            fbIndex = 0;
        }

        frameBuffer[fbIndex] = nf; // Add the next frame to the frameBuffer

        fbCount = Mathf.Max(fbCount, fbIndex + 1);

        fbIndex = (fbIndex + 1) % dqn.frameBufferSize;
        // TODO: Make sure there is a method to fix bug when the frame buffer loops back through.

        return fbIndex; // Return the frameBuffer Index. This will be added to a tuple and the state/next state can be determined using this int
    }
    // Get the next_Frame array data from raycasts, colliders, etc.
    public float[] GetNextFrame()
    {
        CollisionDetection(); // CollisionDetection function will collect the distance to objects from the raycasts pointed in the 8 compass directions

        float[] next_Frame = new float[dqn.frameSize]; // Create a float array  to hold the next frame

        // Populate the next_Frame array with the respective values
        next_Frame[0] = distancesToObstacles[0];
        next_Frame[1] = distancesToObstacles[1];
        next_Frame[2] = distancesToObstacles[2];
        next_Frame[3] = distancesToObstacles[3];
        next_Frame[4] = distancesToObstacles[4];
        next_Frame[5] = distancesToObstacles[5];
        next_Frame[6] = distancesToObstacles[6];
        next_Frame[7] = distancesToObstacles[7];

        // TODO: Possible inputs
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
        float[][] state = new float[dqn.framesPerState][]; // Create a new array to hold the state
        float[] flatState = new float[dqn.framesPerState * dqn.frameSize]; // Flatten state into a 1-D array to input into neural net

        if (buffer[frameIndex] != null && fbCount - dqn.framesPerState > 0)
        {
            for (int i = 0; i < dqn.framesPerState; i++) // Loop through the frames, stopping before the last frame
            {  
                state[dqn.framesPerState - i - 1] = buffer[frameIndex];
            }
            int indx = 0;
            for (int j = 0; j < dqn.framesPerState; j++)
            {
                for (int k = 0; k < dqn.frameSize; k++)
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
        directions = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1),
            new Vector3(1, 0, 0), new Vector3(1, 0, -1),  new Vector3(0, 0, -1),
            new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1) };

        while (hitDir > 0)
        {
            hitDir--;
            if (Physics.Raycast(tf.position, directions[hitDir], out hit[hitDir], dqn.collisionDetectRange))
            {
                //Debug.DrawRay(tf.position, directions[hitL], Color.blue, distance);

                Vector3 hitLocation = hit[hitDir].point;

                float distanceToHit = Vector3.Distance(hitLocation, tf.position);
                
                // If the raycast hits an obstacle...
                if (hit[hitDir].collider.CompareTag("Obstacle"))
                {
                    // Check if the obstacle is within the collision detection range
                    if (distanceToHit <= dqn.collisionDetectRange)
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

        if (angleToTarget < dqn.fieldOfView && targetDistance < dqn.maxViewDistance)
        {
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");             // Add Walls layer to variable
            int objectiveLayer = LayerMask.NameToLayer("Objective");           // Add Player layer to variable
            int layerMask = (1 << obstacleLayer) | (1 << objectiveLayer);      // Create layermask

            RaycastHit hit;

            // Raycast to detect objective within field of view with max view distance as a limit
            if (Physics.Raycast(tf.position, vectorToTarget, out hit, dqn.maxViewDistance, layerMask))
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
