using UnityEngine;

public class Objective : MonoBehaviour
{
    public int winners = 0;

    // When an AI enters the trigger...
    private void OnTriggerStay(Collider other)
    {
        // If the other object has the AI tag...
        if (other.gameObject.CompareTag("AI"))
        {   
            Debug.Log("Objective Hit!");
            int pop = GameManager.instance.populationSize; // Set pop to the population size

            // Incentivize getting to objective first. 
            // Can optionally make this logarithmic
            float scaledFitness = GameManager.instance.objectiveReward - ((GameManager.instance.objectiveReward / pop) * winners); 
            other.GetComponent<AIController>().net.AddFitness(scaledFitness); // Apply scaled fitness
            Destroy(other.gameObject); // Destroy the game object     
            winners++; // Increment winners
        }


    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            col.GetComponent<Environment>().isOnObjective = true;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Agent"))
        {
            col.GetComponent<Environment>().isOnObjective = false;
        }
    }
}
