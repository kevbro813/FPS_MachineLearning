using UnityEngine;
// This is attached to an objective and will trigger a true bool on the agent that enters the trigger. Sets to false when leaving.
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
