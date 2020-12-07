using UnityEngine;
// This is attached to an objective and will trigger a true bool on the agent that enters the trigger. Sets to false when leaving.
public class Objective : MonoBehaviour
{
    private RandomObjective randObj;
    private void Start()
    {
        randObj = GetComponent<RandomObjective>();
    }
    //private void OnTriggerEnter(Collider col)
    //{
    //    if (col.gameObject.CompareTag("Agent"))
    //    {
    //        col.GetComponent<RLComponent>().env.isOnObjective = true;
    //    }
    //}
    //private void OnTriggerExit(Collider col)
    //{
    //    if (col.gameObject.CompareTag("Agent"))
    //    {
    //        col.GetComponent<RLComponent>().env.isOnObjective = false;
    //    }
    //}
    private void OnTriggerStay(Collider col) // TODO: RANDOM OBJECTIVE
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            col.GetComponent<RLComponent>().env.isOnObjective = true;
            randObj.RandomLocation();
        }
    }
}
