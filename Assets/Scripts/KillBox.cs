using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponent<AIController>().net.AddFitness(GameManager.instance.killBoxPenalty);
        Destroy(other.gameObject);
    }
}
