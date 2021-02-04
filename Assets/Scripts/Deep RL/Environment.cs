using UnityEngine;
using System;

[Serializable]
public class Environment
{
    #region Variables
    private RLComponent rlComponent;
    private Transform tf;
    private RaycastHit[] hit;
    private Vector3[] directions;
    public double[][] frameBuffer; // An array to hold the frame buffer
    public double[] distancesToObstacles;  
    //public double targetDistance;
    //public double rotation;
    public int fbIndex; // Frame buffer index used to fill the fb
    public int fbCount; // Frame buffer count used to track the size of the fb
    public bool isOnObjective = false; // If true, the agent will be rewarded as long as they are on the objective
    public bool isHitByProjectile = false;
    public bool doesProjectileMiss = false;
    public bool isProjectileActive = false;
    //public bool isAtCheckpoint = false; // If true, agent will be rewarded one time
    public int frameBufferSize; // Size of the frame buffer, AKA maximum number of frames that can be stored at any time
    public int framesPerState; // Number of frames in a state
    private int inputsPerFrame;  // Number of inputs per frame
    private int inputsPerState; // AKA total number of inputs
    private float collisionDetectRange; // Range that the collision detection raycasts will extend
    public float currentCheckpointReward;

    //public double[] nextFrame; // Create a float array to hold the next frame
    public float zObjDist;
    public float xObjDist;
    private Turret turret;
    public float zProjectileDist;
    public float xProjectileDist;
    #endregion

    #region Initialization
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
        zObjDist = 0;
        xObjDist = 0;
        distancesToObstacles = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        frameBufferSize = RLManager.instance.settings.frameBufferSize;
        framesPerState = RLManager.instance.settings.framesPerState;
        inputsPerFrame = rlComponent.inputsPerFrame;
        inputsPerState = rlComponent.inputsPerState;
        collisionDetectRange = RLManager.instance.settings.collisionDetectRange;
        frameBuffer = new double[frameBufferSize][];
        turret = GameObject.FindWithTag("Turret").GetComponent<Turret>();
    }
    #endregion

    #region Reward Method
    /// <summary>
    /// Calculates the reward. This can be changed to reinforce various behaviors.
    /// </summary>
    /// <returns></returns>
    public double CalculateReward()
    { 
        double reward = 0;

        if (isOnObjective) reward += 3; // Reward if standing on objective
        else reward -= 1; // Penalize if not on objective

        // Keep agent from running into walls too frequently (Max collisions seems to be 5 of the 8 directions which equals a -2.5 reward)
        for (int i = 0; i < distancesToObstacles.Length; i++)
        {
            bool isPenalized = distancesToObstacles[i] < 1f;
            if (isPenalized) reward -= 0.25f;
        }

        if (isHitByProjectile)
        {
            Debug.Log("Hit");
            reward -= 5;
            isHitByProjectile = false;
        }
        if (doesProjectileMiss)
        {
            Debug.Log("Miss");
            //reward += 5;
            doesProjectileMiss = false;
        }

        if (!turret.isTargetVisible)
        {
            //Debug.Log("Target is not visible");
            reward += 3;
        }

        return reward;
    }
    #endregion

    #region Frame Buffer and State Methods
    /// <summary>
    /// Get the next frame which is one epochs worth of data, can be raycasts, Vector3 positions, health integer, etc.
    /// This method needs to be changed to collect the right inputs for the game.
    /// </summary>
    /// <returns></returns>
    public double[] GetNextFrame(int episodeStep)
    {
        CollisionDetection(); // CollisionDetection function will collect the distance to objects from the raycasts pointed in the 8 compass directions

        double[] nextFrame = new double[inputsPerFrame]; // Create a double array to hold the next frame

        // Populate the nextFrame array with the values for each input

        if (turret.projectile_tf) // Z and X axes distance to projectile when the projectile has been shot
        {
            zProjectileDist = turret.projectile_tf.position.z - tf.position.z;
            xProjectileDist = turret.projectile_tf.position.x - tf.position.x;

            nextFrame[0] = zProjectileDist;
            nextFrame[1] = xProjectileDist;
        }
        else // Distance when no projectile is detected uses the barrel opening transform position (AKA muzzle)
        {
            zProjectileDist = turret.muzzle.position.z - tf.position.z;
            xProjectileDist = turret.muzzle.position.x - tf.position.x;

            nextFrame[0] = zProjectileDist;
            nextFrame[1] = xProjectileDist;
        }

        // Raycast sensor in 8 compass directions around the agent
        nextFrame[2] = distancesToObstacles[0];
        nextFrame[3] = distancesToObstacles[1];
        nextFrame[4] = distancesToObstacles[2];
        nextFrame[5] = distancesToObstacles[3];
        nextFrame[6] = distancesToObstacles[4];
        nextFrame[7] = distancesToObstacles[5];
        nextFrame[8] = distancesToObstacles[6];
        nextFrame[9] = distancesToObstacles[7];

        nextFrame[10] = Convert.ToDouble(turret.isTargetVisible); // Indicates if the agent is visible to the turret using a raycast hit

        // The z and x position of the agent (used to know where it is in the level)
        nextFrame[11] = tf.position.z;
        nextFrame[12] = tf.position.x;

        // Calculate the agent's distance to the objective
        zObjDist = tf.position.z - RLManager.instance.objectiveLocation.z; // z distance to objective
        xObjDist = tf.position.x - RLManager.instance.objectiveLocation.x; // x distance to objective
        nextFrame[13] = zObjDist;
        nextFrame[14] = xObjDist;


        // Indicate if projectile is active
        //nextFrame[2] = Convert.ToDouble(isProjectileActive);
        //nextFrame[0] = episodeStep; // Time represented by ticks of the update method each episode, AKA episode epochs
        //nextFrame[10] = tf.position.z;
        //nextFrame[11] = tf.position.x;
        //nextFrame[12] = tf.transform.eulerAngles.y;

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
    #endregion

    #region Input Methods (Used to provide input data for the neural network)
    /// <summary>
    /// Collision Detection Algorithm
    /// </summary>
    private void CollisionDetection()
    {   
        hit = new RaycastHit[8];
        //int hitDir = hit.Length;

        // Directions Start forward then moves clockwise
        directions = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1),
            new Vector3(1, 0, 0), new Vector3(1, 0, -1),  new Vector3(0, 0, -1),
            new Vector3(-1, 0, -1), new Vector3(-1, 0, 0), new Vector3(-1, 0, 1) };

        //while (hitDir > 0)
        for (int hitDir = 0; hitDir < hit.Length; hitDir++)
        {
            //hitDir--;
            if (Physics.Raycast(tf.position, directions[hitDir], out hit[hitDir], collisionDetectRange))
            {
                //Debug.DrawRay(tf.position, directions[hitL], Color.blue, distance);

                // If the raycast hits an obstacle or cover object...
                if (hit[hitDir].collider.CompareTag("Obstacle") || hit[hitDir].collider.CompareTag("CoverObject") || hit[hitDir].collider.CompareTag("Turret"))
                {
                    Vector3 hitLocation = hit[hitDir].point;

                    double distanceToHit = Vector3.Distance(hitLocation, tf.position);

                    Debug.DrawLine(tf.position, hit[hitDir].point, Color.blue, 1f);

                    // Check if the obstacle is within the collision detection range
                    if (distanceToHit <= collisionDetectRange) distancesToObstacles[hitDir] = distanceToHit; // Set the distance to the hit object in the distancesToObjects array
                }
            }
            else distancesToObstacles[hitDir] = collisionDetectRange; // If no object is hit by raycast, then set distancesToObstacles to max
        }
        
    }
    #endregion
}
