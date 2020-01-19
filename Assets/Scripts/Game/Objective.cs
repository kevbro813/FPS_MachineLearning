using UnityEngine;

public class Objective : MonoBehaviour
{
    public int winners = 0;

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            //col.GetComponent<DQN>().env.isOnObjective = true;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            //col.GetComponent<DQN>().env.isOnObjective = false;
        }
    }
}
