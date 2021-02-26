using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentStates
{
    HuntObjective,
    TakeCover,
    Attack
};

[RequireComponent(typeof(RLComponent))]
public class AgentFSM : MonoBehaviour
{
    public AgentStates agentStates;
    public RLComponent rlComp;
    public NeuralNetwork[] stateNeuralNetworks;
    public NeuralNetwork currentStateNetwork;

    private void Awake() => stateNeuralNetworks = new NeuralNetwork[3];
    private void Start() => rlComp = GetComponent<RLComponent>();

    // State Machine
    private void Update()
    {
        if (agentStates == AgentStates.HuntObjective) currentStateNetwork = stateNeuralNetworks[0];
        
        else if (agentStates == AgentStates.TakeCover) currentStateNetwork = stateNeuralNetworks[1];
        
        else if (agentStates == AgentStates.Attack) currentStateNetwork = stateNeuralNetworks[2];
    }
}
