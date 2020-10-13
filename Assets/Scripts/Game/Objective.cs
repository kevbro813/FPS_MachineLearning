using UnityEngine;

public class Objective : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            col.GetComponent<RLComponent>().env.isOnObjective = true;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            col.GetComponent<RLComponent>().env.isOnObjective = false;
        }
    }
}
