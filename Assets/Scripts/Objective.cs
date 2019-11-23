using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("AI"))
        {
            Debug.Log("Objective Hit!");
            other.GetComponent<AIController>().net.AddFitness(10000f);
            Destroy(other.gameObject);       
        }
    }
}
