using UnityEngine;
using System;

[Serializable]
public class Environment
{
    private RLComponent rlComponent;
    private Transform tf;
    private RaycastHit[] hit;
    private Vector3[] directions;
    public double[][] frameBuffer; // An array to hold the frame buffer
    public double[] distancesToObstacles;  
    public double targetDistance;
    public double rotation;
    public int fbIndex; // Frame buffer index used to fill the fb
    public int fbCount; // Frame buffer count used to track the size of the fb
    public bool isOnObjective = false; // If true, the agent will be rewarded as long as they are on the objective
    public bool isAtCheckpoint = false; // If true, agent will be rewarded one time
    public int frameBufferSize; // Size of the frame buffer, AKA maximum number of frames that can be stored at any time
    public int framesPerState; // Number of frames in a state
    private int inputsPerFrame;  // Number of inputs per frame
    private int inputsPerState; // AKA total number of inputs
    private float collisionDetectRange; // Range that the collision detection raycasts will extend
    public float currentCheckpointReward;

    /// <summary>
    /// Initialize a new environment.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="rl"></param>
    public void Init_Env(Transform agent, RLComponent rl)
    {
        tf = agent;
        rlComponent = rl;
        fbIndex = 0;
        fbCount = 0;
        distancesToObstacles = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        frameBufferSize = RLManager.instance.settings.frameBufferSize;
        framesPerState = RLManager.instance.settings.framesPerState;
        inputsPerFrame = rlComponent.inputsPerFrame;
        inputsPerState = rlComponent.inputsPerState;
        collisionDetectRange = RLManager.instance.settings.collisionDetectRange;
        frameBuffer = new double[frameBufferSize][];
    }
    /// <summary>
    /// Calculates the reward. This can be changed to reinforce behaviors.
    /// </summary>
    /// <returns></returns>
    public double CalculateReward()
    {
        double reward = 0;

        if (isOnObjective)
        {
            reward++;
        }
        else
        {
            reward--;
        }
        for (int i = 0; i < distancesToObstacles.Length; i++)
        {
            if (distancesToObstacles[i] < 1)
            {
                //reward -= (2 - distancesToObstacles[i]); // TODO: FIX THIS... Bool to indicate if a raycast hits a wall. If it is false and distanceToObstacles is 0, then it should set distanceToObstacles to max number to avoid confusion with points.
                reward--;
            }
        }
        reward += currentCheckpointReward;

        return reward;
    }
    /// <summary>
    /// Get the next frame which is one epochs worth of data, can be raycasts, Vector3 positions, health, etc.
    /// This method needs to be changed to collect the right inputs for the game.
    /// </summary>
    /// <returns></returns>
    public double[] GetNextFrame()
    {
        CollisionDetection(); // CollisionDetection function will collect the distance to objects from the raycasts pointed in the 8 compass directions

        double[] nextFrame = new double[inputsPerFrame]; // Create a float array to hold the next frame

        // Populate the next_Frame array with the respective values
        nextFrame[0] = tf.position.z;
        nextFrame[1] = tf.position.x;
        nextFrame[2] = distancesToObstacles[0];
        nextFrame[3] = distancesToObstacles[1];
        nextFrame[4] = distancesToObstacles[2];
        nextFrame[5] = distancesToObstacles[3];
        nextFrame[6] = distancesToObstacles[4];
        nextFrame[7] = distancesToObstacles[5];
        nextFrame[8] = distancesToObstacles[6];
        nextFrame[9] = distancesToObstacles[7];
        //nextFrame[10] = tf.transform.eulerAngles.y;

        return nextFrame; // Return nextFrame
    }
    /// <summary>
    /// Update the frame buffer by removing the oldest frame and appending the next frame
    /// </summary>
    /// <param name="nextFrame"></param>
    /// <returns></returns>
    public int AppendFrame(double[] nextFrame) // Pass in the next frame
    {
        frameBuffer[fbIndex] = nextFrame; // Add the next frame to the frameBuffer

        return fbIndex; // Return the frameBuffer Index. This will be added to a tuple and the state/next state can be determined using this int
    }
    /// <summary>
    /// Update the fbCount and fbIndex variables to track the frameBuffer.
    /// </summary>
    public void UpdateFrameBufferCounters()
    {
        fbCount = Mathf.Max(fbCount, fbIndex + 1);

        fbIndex = (fbIndex + 1) % frameBufferSize;
    }
    /// <summary>
    /// Get the state from frame buffer by taking the oldest frames
    /// </summary>
    /// <param name="frameIndex"></param>
    /// <returns></returns>
    public double[] GetState(int frameIndex)
    {
        int fps = framesPerState;
        double[] flatState = new double[inputsPerState]; // Flatten state into a 1-D array to input into neural net
        
        if (fbCount - fps >= 0) // Check that there are enough frames to create a state.
        {
            double[][] state = new double[fps][]; // Create a new array to hold the state
            int fi;

            for (int i = 0; i < fps; i++) // Loop through the frames  
            {
                if ((frameIndex - i) < 0) // If the frameIndex is a negative
                {
                    fi = frameBuffer.Length - 1 - i; // Use the frameIndex from the end of the frameBuffer
                }
                else
                {
                    fi = frameIndex - i; // Use the frameIndex if not negative
                }
                state[fps - 1 - i] = frameBuffer[fi]; // Set the state to the frame at index "fi"
            }

            int indx = 0;
            // Flatten the state into a 1D array
            for (int j = 0; j < fps; j++)
            {
                if (state[j] != null)
                { 
                    for (int k = 0; k < inputsPerFrame; k++)
                    {
                        flatState[indx] = state[j][k];
                        indx++; // Increment the flatState index
                    }
                }
            }    
        }
        return flatState; // Return the current state
    }

    /// <summary>
    /// Collision Detection Algorithm
    /// </summary>
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
            if (Physics.Raycast(tf.position, directions[hitDir], out hit[hitDir], collisionDetectRange))
            {
                //Debug.DrawRay(tf.position, directions[hitL], Color.blue, distance);

                Vector3 hitLocation = hit[hitDir].point;

                double distanceToHit = Vector3.Distance(hitLocation, tf.position);

                // If the raycast hits an obstacle...
                if (hit[hitDir].collider.CompareTag("Obstacle"))
                {
                    // Check if the obstacle is within the collision detection range
                    if (distanceToHit <= collisionDetectRange)
                    {
                        distancesToObstacles[hitDir] = distanceToHit; // Set the distance to the hit object in the distancesToObjects array
                    }
                }
            }
            else
            {
                distancesToObstacles[hitDir] = collisionDetectRange; // If no object is hit by raycast, then set distancesToObstacles to max
            }
        }
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
