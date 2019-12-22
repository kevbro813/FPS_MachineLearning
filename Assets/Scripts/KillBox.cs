using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<AIController>() != null)
        {
            other.gameObject.GetComponent<AIController>().net.AddFitness(GameManager.instance.killBoxPenalty);
        }      
        GameManager.instance.aiObjectsList.Remove(other.gameObject);
        GameManager.instance.aiControllerList.Remove(other.gameObject.GetComponent<AIController>());
        Destroy(other.gameObject);
    }
}
