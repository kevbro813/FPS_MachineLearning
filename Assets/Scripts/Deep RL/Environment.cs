using UnityEngine;
using System;

[Serializable]
public class Environment
{
    private DQN dqn;
    private Transform tf;

    public double[][] frameBuffer; // An array to hold the frame buffer

    private RaycastHit[] hit;
    private Vector3[] directions;
    public double[] distancesToObstacles;  
    public double targetDistance;
    public double rotation;
    public int fbIndex;
    public int fbCount;
    public bool isOnObjective = false;

    // Initialize a new environment
    public void Init_Env(Transform agent, DQN d)
    {
        tf = agent;
        dqn = d;
        fbIndex = 0;
        fbCount = 0;
        distancesToObstacles = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        frameBuffer = new double[GameManager.instance.settings.frameBufferSize][];
    }
    // Get the next_Frame array data from raycasts, colliders, etc.
    public double[] GetNextFrame()
    {
        //CollisionDetection(); // CollisionDetection function will collect the distance to objects from the raycasts pointed in the 8 compass directions

        double[] nextFrame = new double[dqn.inputsPerFrame]; // Create a float array to hold the next frame
        nextFrame[0] = tf.position.x;
        nextFrame[1] = tf.position.z;

        // Populate the next_Frame array with the respective values
        //nextFrame[2] = distancesToObstacles[0];
        //nextFrame[3] = distancesToObstacles[1];
        //nextFrame[4] = distancesToObstacles[2];
        //nextFrame[5] = distancesToObstacles[3];
        //nextFrame[6] = distancesToObstacles[4];
        //nextFrame[7] = distancesToObstacles[5];
        //nextFrame[8] = distancesToObstacles[6];
        //nextFrame[9] = distancesToObstacles[7];
        //next_Frame[10] = tf.transform.eulerAngles.y;

        // TODO: Possible inputs
        // Distance to target (while in sight)
        // Health
        // Cover objects in sight
        // Enemy location (while in sight)
        // The direction the enemy is facing if sighted
        // Current AI state

        return nextFrame; // Return next_Frame
    }
    // Update the frame buffer by removing the oldest frame and appending the next frame
    public int UpdateFrameBuffer(double[] nextFrame) // Pass in the next frame
    {
        frameBuffer[fbIndex] = nextFrame; // Add the next frame to the frameBuffer

        return fbIndex; // Return the frameBuffer Index. This will be added to a tuple and the state/next state can be determined using this int
    }

    public void UpdateFrameBufferCounters()
    {
        fbCount = Mathf.Max(fbCount, fbIndex + 1);

        fbIndex = (fbIndex + 1) % GameManager.instance.settings.frameBufferSize;
    }
    // Get the state from frame buffer by taking the oldest frames
    public double[] GetState(int frameIndex)
    {
        int framesPerState = GameManager.instance.settings.framesPerState;
        double[] flatState = new double[dqn.inputsPerState]; // Flatten state into a 1-D array to input into neural net

        if (fbCount - framesPerState >= 0) // Check that there are enough frames to create a state.
        {
            double[][] state = new double[framesPerState][]; // Create a new array to hold the state
            int fi;

            for (int i = 0; i < framesPerState; i++) // Loop through the frames
            {
                if ((frameIndex - i) < 0) // If the frameIndex is a negative
                {
                    fi = frameBuffer.Length - 1 - i; // Use the frameIndex from the end of the frameBuffer
                }
                else
                {
                    fi = frameIndex - i; // Use the frameIndex if not negative
                }
                state[framesPerState - 1 - i] = frameBuffer[fi];
            }

            int indx = 0;

            for (int j = 0; j < framesPerState; j++)
            {
                for (int k = 0; k < dqn.inputsPerFrame; k++)
                {
                    if (state[j] != null)
                    {
                        flatState[indx] = state[j][k];
                        indx++;
                    }
                }
            }    
        }
        return flatState; // Return the current state
    }
    // TODO: Calculate the reward
    public double CalculateReward(double[] nf)
    {
        double reward = 0;
        reward--;
        if (isOnObjective)
        {
            //Debug.Log("Reward!");
            reward += 11;
        }
        //else
        //{
        //    reward--;
        //}
        //for (int i = 0; i < distancesToObstacles.Length; i++)
        //{
        //    if (distancesToObstacles[i] < 5)
        //    {
        //        //reward -= (2 - distancesToObstacles[i]); // TODO: FIX THIS... Bool to indicate if a raycast hits a wall. If it is false and distanceToObstacles is 0, then it should set distanceToObstacles to max number to avoid confusion with points.
        //        reward--;
        //    }
        //}
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
            if (Physics.Raycast(tf.position, directions[hitDir], out hit[hitDir], GameManager.instance.settings.collisionDetectRange))
            {
                //Debug.DrawRay(tf.position, directions[hitL], Color.blue, distance);

                Vector3 hitLocation = hit[hitDir].point;

                double distanceToHit = Vector3.Distance(hitLocation, tf.position);

                // If the raycast hits an obstacle...
                if (hit[hitDir].collider.CompareTag("Obstacle"))
                {
                    // Check if the obstacle is within the collision detection range
                    if (distanceToHit <= GameManager.instance.settings.collisionDetectRange)
                    {
                        distancesToObstacles[hitDir] = distanceToHit; // Set the distance to the hit object in the distancesToObjects array
                    }
                }
            }
            else
            {
                distancesToObstacles[hitDir] = GameManager.instance.settings.collisionDetectRange; // If no object is hit by raycast, then set distancesToObstacles to max
            }
        }
    }

    // AI Vision is limited to field of view and max view distance
    private void AIVision(Vector3 vectorToTarget, Transform ttf)
    {
        // Find the distance between the two vectors in float to compare with maxViewDistance
        targetDistance = Vector3.Distance(ttf.position, tf.position);

        // Find the angle between the direction our agent is facing (forward in local space) and the vector to the target.
        float angleToTarget = Vector3.Angle(vectorToTarget, tf.forward);

        if (angleToTarget < GameManager.instance.settings.fieldOfView && targetDistance < GameManager.instance.settings.maxViewDistance)
        {
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");             // Add Walls layer to variable
            int objectiveLayer = LayerMask.NameToLayer("Objective");           // Add Player layer to variable
            int layerMask = (1 << obstacleLayer) | (1 << objectiveLayer);      // Create layermask

            RaycastHit hit;

            // Raycast to detect objective within field of view with max view distance as a limit
            if (Physics.Raycast(tf.position, vectorToTarget, out hit, GameManager.instance.settings.maxViewDistance, layerMask))
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
