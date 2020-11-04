using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public float checkpointReward;
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            Environment env = col.GetComponent<RLComponent>().env;
            //env.isAtCheckpoint = true;
            env.currentCheckpointReward = checkpointReward;

            //this.GetComponent<Collider>().enabled = false;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            Environment env = col.GetComponent<RLComponent>().env;
            //env.isAtCheckpoint = true;
            env.currentCheckpointReward = 0;

            //this.GetComponent<Collider>().enabled = false;
        }
    }
}
