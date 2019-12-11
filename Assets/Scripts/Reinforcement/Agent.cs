using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float[] actions;
    public RLComponent rlComp;
    public Environment env;

    private void Start()
    {
        rlComp = GetComponent<RLComponent>();
        env = GetComponent<Environment>();
    }
    public void InitAgent()
    {
        // Load the model the agent will use
    }
    public float[] GetAction(float[] normalizedStates) // normalizedStates are passed into a neural network and returns action
    {
        // This function accepts a state and decides what action to perform
        // Uses Q-learning or variant like epsilon-greedy
        // Calculate Q(s,a) by taking argmax over a

        return actions;
    }
    public bool TrainAgent(float[] states, float[] actions, List<float> rewards) // State, action, reward, next state, isDone are passed in
    {
        // Input = s
        // if (isDone) then target = r
        // else  target = r + gamme * max {Q (s',:)}
        // Then use gradient descent with momentum with the target for the state
        return true;
    }

}
