using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class UIManager : MonoBehaviour
{
    [HideInInspector] public string fileName;
    [HideInInspector] public string settingsName;
    public RLComponent rlComponent;
    public InputField fileIpt;
    public InputField maxEpsisodeIpt;
    public InputField stepsEpsIpt;
    public InputField frameStateIpt;
    public InputField frameBufferIpt;
    public InputField epsilonIpt;
    public InputField epsMinIpt;
    public InputField epsDecayIpt;
    public InputField expBufferSizeIpt;
    public InputField miniBatchInpt;
    public InputField netCopyRateIpt;
    public InputField gammaIpt;
    public InputField learningRateIpt;
    public InputField beta1Ipt;
    public InputField beta2Ipt;
    public InputField epsHatIpt;
    public InputField gradientThreshIpt;
    public InputField maxViewIpt;
    public InputField fovIpt;
    public InputField colDetectIpt;
    public InputField asIpt;
    public InputField clipIpt;
    public InputField entropyIpt;
    public InputField tauIpt;
    public InputField trEpochsIpt;
    public Text episodeNumber;
    public Text maxEpisodes;
    public Text epochs;
    public Text totalReward;
    public Text currentReward;
    public Text epsilon;
    public Text epsilonMin;
    public Text episodeSteps;
    public Text episodeMaxSteps;
    public Text episodeAverage;
    public GameObject resumeButton;
    public GameObject saveButton;
    public Text currentCost;
    public Toggle trainingToggle;
    public Toggle loadSettingsToggle;
    private bool isTrainingTemp;
    private bool isLoadSettings;
    public Dropdown algoDropdown;
    public InputField actorLRIpt;
    public InputField criticLRIpt;
    public InputField asyncAgentIpt;
    public Text dqnStructureDisplay;
    public Text actorStructureDisplay;
    public Text criticStructureDisplay;
    public InputField dqnInputipt;
    public InputField dqnOutputipt;
    public InputField dqnHiddenipt;
    public InputField actorInputipt;
    public InputField actorOutputipt;
    public InputField actorHiddenipt;
    public InputField criticInputipt;
    public InputField criticHiddenipt;
    public GameObject activationFunctionMenu;
    public int layerMax = 6; // TODO: Move to settings
    public List<string> algos;
    public List<string> actFun;
    public Dropdown[] dqnActDropdowns;
    public Dropdown[] actorActDropdowns;
    public Dropdown[] criticActDropdowns;
    public InputField saveLocationipt;

    private void Start()
    {
        isLoadSettings = true;
        isTrainingTemp = true;
        dqnHiddenipt.text = "10";
        actorHiddenipt.text = "10";
        criticHiddenipt.text = "10";
        PrepareDropdowns();
        UpdateSettingsDisplay();
    }

    private void PrepareDropdowns()
    {
        algos = Enum.GetNames(typeof(Settings.Algorithm)).ToList();
        algoDropdown.AddOptions(algos);
        actFun = Enum.GetNames(typeof(Settings.LayerActivations)).ToList();
    }

    private void PopulateActivationDropdown(Dropdown drop)
    {
        drop.AddOptions(actFun);
    }

    public void ActivationFunctionsDisplayed()
    {
        int dqnActQty = RLManager.instance.settings.dqnNetStructure.Length - 1;
        int actorActQty = RLManager.instance.settings.actorNetStructure.Length - 1;
        int criticActQty = RLManager.instance.settings.criticNetStructure.Length - 1;

        for (int i = 0; i < dqnActDropdowns.Length; i++)
        {
            if (i < dqnActQty)
            {
                dqnActDropdowns[i].gameObject.SetActive(true);
                PopulateActivationDropdown(dqnActDropdowns[i]);
                dqnActDropdowns[i].value = (int)RLManager.instance.settings.dqnActivations[i];
            }
            else
            {
                dqnActDropdowns[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < actorActDropdowns.Length; i++)
        {
            if (i < actorActQty)
            {
                actorActDropdowns[i].gameObject.SetActive(true);
                PopulateActivationDropdown(actorActDropdowns[i]);
                actorActDropdowns[i].value = (int)RLManager.instance.settings.actorActivations[i];
            }
            else
            {
                actorActDropdowns[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < criticActDropdowns.Length; i++)
        {
            if (i < criticActQty)
            {
                criticActDropdowns[i].gameObject.SetActive(true);
                PopulateActivationDropdown(criticActDropdowns[i]);
                criticActDropdowns[i].value = (int)RLManager.instance.settings.criticActivations[i];
            }
            else
            {
                criticActDropdowns[i].gameObject.SetActive(false);
            }
        }
        // Loop through arrays
    }

    public void SubmitActivationFunctions()
    {
        int dqnActQty = RLManager.instance.settings.dqnNetStructure.Length - 1;
        int actorActQty = RLManager.instance.settings.actorNetStructure.Length - 1;
        int criticActQty = RLManager.instance.settings.criticNetStructure.Length - 1;

        RLManager.instance.settings.dqnActivations = new Settings.LayerActivations[dqnActQty];
        RLManager.instance.settings.actorActivations = new Settings.LayerActivations[actorActQty];
        RLManager.instance.settings.criticActivations = new Settings.LayerActivations[criticActQty];

        for (int i = 0; i < dqnActQty; i++)
        {
            RLManager.instance.settings.dqnActivations[i] = (Settings.LayerActivations)dqnActDropdowns[i].value;
        }
        for (int i = 0; i < actorActQty; i++)
        {
            RLManager.instance.settings.actorActivations[i] = (Settings.LayerActivations)actorActDropdowns[i].value;
        }
        for (int i = 0; i < criticActQty; i++)
        {
            RLManager.instance.settings.criticActivations[i] = (Settings.LayerActivations)criticActDropdowns[i].value;
        }

        activationFunctionMenu.SetActive(false);
    }
    // Update is called once per frame
    private void Update()
    {
        if(GameManager.instance.adminMenu.activeSelf)
        {
            if (RLManager.instance.isAgentInitialized)
            {
                resumeButton.SetActive(true);
                saveButton.SetActive(true);
            }
            else
            {
                resumeButton.SetActive(false);
                saveButton.SetActive(false);
            }
        }
    }
    public void UpdateSettingsDisplay()
    {
        maxEpsisodeIpt.text = RLManager.instance.settings.episodeMax.ToString();
        stepsEpsIpt.text = RLManager.instance.settings.epiMaxSteps.ToString();
        frameStateIpt.text = RLManager.instance.settings.framesPerState.ToString();
        frameBufferIpt.text = RLManager.instance.settings.frameBufferSize.ToString();
        epsilonIpt.text = RLManager.instance.settings.epsilon.ToString();
        epsMinIpt.text = RLManager.instance.settings.epsilonMin.ToString();
        epsDecayIpt.text = RLManager.instance.settings.epsDecayRate.ToString();
        expBufferSizeIpt.text = RLManager.instance.settings.expBufferSize.ToString();
        miniBatchInpt.text = RLManager.instance.settings.miniBatchSize.ToString();
        netCopyRateIpt.text = RLManager.instance.settings.netCopyRate.ToString();
        gammaIpt.text = RLManager.instance.settings.gamma.ToString();
        learningRateIpt.text = RLManager.instance.settings.dqnLearningRate.ToString();
        beta1Ipt.text = RLManager.instance.settings.beta1.ToString();
        beta2Ipt.text = RLManager.instance.settings.beta2.ToString();
        epsHatIpt.text = RLManager.instance.settings.epsilonHat.ToString();
        gradientThreshIpt.text = RLManager.instance.settings.gradientThreshold.ToString();
        maxViewIpt.text = RLManager.instance.settings.maxViewDistance.ToString();
        fovIpt.text = RLManager.instance.settings.fieldOfView.ToString();
        colDetectIpt.text = RLManager.instance.settings.collisionDetectRange.ToString();
        asIpt.text = RLManager.instance.settings.autoSaveEpisode.ToString();
        clipIpt.text = RLManager.instance.settings.ppoClip.ToString();
        entropyIpt.text = RLManager.instance.settings.entropyBonus.ToString();
        tauIpt.text = RLManager.instance.settings.tau.ToString();
        trEpochsIpt.text = RLManager.instance.settings.trainingEpochs.ToString();
        loadSettingsToggle.isOn = isLoadSettings;
        trainingToggle.isOn = isTrainingTemp;
        actorLRIpt.text = RLManager.instance.settings.actorLearningRate.ToString();
        criticLRIpt.text = RLManager.instance.settings.criticLearningRate.ToString();
        asyncAgentIpt.text = RLManager.instance.settings.asyncAgents.ToString();
        algoDropdown.value = (int)RLManager.instance.settings.algo;
        saveLocationipt.text = RLManager.instance.settings.saveLocation;
        UpdateNetworkStructureDisplay();

        dqnInputipt.text = RLManager.instance.settings.dqnNetStructure[0].ToString();
        dqnOutputipt.text = RLManager.instance.settings.dqnNetStructure[RLManager.instance.settings.dqnNetStructure.Length - 1].ToString();
        actorInputipt.text = RLManager.instance.settings.dqnNetStructure[0].ToString();
        actorOutputipt.text = RLManager.instance.settings.actorNetStructure[RLManager.instance.settings.actorNetStructure.Length - 1].ToString();
        criticInputipt.text = RLManager.instance.settings.dqnNetStructure[0].ToString();
    }

    public void OpenActivationFunctionMenu()
    {
        activationFunctionMenu.SetActive(true);
        ActivationFunctionsDisplayed();
    }
    public void UpdateNetworkStructureDisplay()
    {
        string dqnStructString = "";
        string actorStructString = "";
        string criticStructString = "";
        for (int i = 0; i < RLManager.instance.settings.dqnNetStructure.Length; i++)
        {
            dqnStructString = dqnStructString + " : " + RLManager.instance.settings.dqnNetStructure[i];
        }
        for (int i = 0; i < RLManager.instance.settings.actorNetStructure.Length; i++)
        {
            actorStructString = actorStructString + " : " + RLManager.instance.settings.actorNetStructure[i];
        }
        for (int i = 0; i < RLManager.instance.settings.criticNetStructure.Length; i++)
        {
            criticStructString = criticStructString + " : " + RLManager.instance.settings.criticNetStructure[i];
        }
        dqnStructureDisplay.text = dqnStructString;
        actorStructureDisplay.text = actorStructString;
        criticStructureDisplay.text = criticStructString;
    }
    public void UpdateSaveLocation()
    {
        RLManager.instance.settings.saveLocation = saveLocationipt.text;
        if (RLManager.instance.settings.saveLocation == "")
        {
            RLManager.instance.settings.saveLocation = Application.persistentDataPath;
        }
    }   
    public void UpdateDQNInputs()
    {
        RLManager.instance.settings.dqnNetStructure[0] = int.Parse(dqnInputipt.text);
        UpdateNetworkStructureDisplay();
    }
    public void UpdateDQNOutputs()
    {
        RLManager.instance.settings.dqnNetStructure[RLManager.instance.settings.dqnNetStructure.Length - 1] = int.Parse(dqnOutputipt.text);
        UpdateNetworkStructureDisplay();
    }
    public void UpdateDQNHidden(bool isRemoving)
    {
        if (isRemoving)
        {
            if (RLManager.instance.settings.dqnNetStructure.Length > 2)
            {
                int[] tempNet = RLManager.instance.settings.dqnNetStructure; // Temporarily store the network structure
                RLManager.instance.settings.dqnNetStructure = new int[tempNet.Length - 1]; // Remove one layer

                for (int i = 0; i < RLManager.instance.settings.dqnNetStructure.Length - 1; i++) // Iterate through each layer and stop at second to last layer
                {
                    RLManager.instance.settings.dqnNetStructure[i] = tempNet[i]; // Set number of neurons in the layer
                }

                RLManager.instance.settings.dqnNetStructure[RLManager.instance.settings.dqnNetStructure.Length - 1] = tempNet[tempNet.Length - 1]; // Set the output layer
            }
        }
        else
        {
            // Check layer max
            if (RLManager.instance.settings.dqnNetStructure.Length <= layerMax)
            {
                // Check non-zero
                if (dqnHiddenipt.text != "0")
                {
                    int[] tempNet = RLManager.instance.settings.dqnNetStructure; // Temporarily save network
                    Settings.LayerActivations[] tempActs = RLManager.instance.settings.dqnActivations; // Temporarily save layer activations

                    RLManager.instance.settings.dqnNetStructure = new int[tempNet.Length + 1]; // Add one layer to network
                    RLManager.instance.settings.dqnActivations = new Settings.LayerActivations[tempActs.Length + 1]; // Add one layer to activation functions

                    for (int i = 0; i < tempNet.Length - 1; i++) // Iterate through each layer in the original array and stop at second to last layer
                    {
                        RLManager.instance.settings.dqnNetStructure[i] = tempNet[i]; // Set number of neurons in the layer
                        RLManager.instance.settings.dqnActivations[i] = tempActs[i]; // Set the activation function in the layer
                    }
                    // Set the neurons in the new hidden layer
                    RLManager.instance.settings.dqnNetStructure[RLManager.instance.settings.dqnNetStructure.Length - 2] = int.Parse(dqnHiddenipt.text);
                    // Set new hidden layer activation function to the previous layer activation function by default
                    RLManager.instance.settings.dqnActivations[RLManager.instance.settings.dqnActivations.Length - 2] = RLManager.instance.settings.dqnActivations[RLManager.instance.settings.dqnActivations.Length - 3];

                    // Set the output layer
                    RLManager.instance.settings.dqnNetStructure[RLManager.instance.settings.dqnNetStructure.Length - 1] = tempNet[tempNet.Length - 1];
                    RLManager.instance.settings.dqnActivations[RLManager.instance.settings.dqnActivations.Length - 1] = tempActs[tempActs.Length - 1];
                }
                else
                {
                    Debug.Log("Can't have zero neurons in a layer");
                }
            }
        }
        UpdateNetworkStructureDisplay();
    }
    public void UpdateActorInputs()
    {
        RLManager.instance.settings.actorNetStructure[0] = int.Parse(actorInputipt.text);
        UpdateNetworkStructureDisplay();
    }
    public void UpdateActorOutputs()
    {
        RLManager.instance.settings.actorNetStructure[RLManager.instance.settings.actorNetStructure.Length - 1] = int.Parse(actorOutputipt.text);
        UpdateNetworkStructureDisplay();
    }
    public void UpdateActorHidden(bool isRemoving)
    {
        if (isRemoving)
        {
            if (RLManager.instance.settings.actorNetStructure.Length > 2)
            {
                int[] tempNet = RLManager.instance.settings.actorNetStructure; // Temporarily store the network structure
                RLManager.instance.settings.actorNetStructure = new int[tempNet.Length - 1]; // Remove one layer

                for (int i = 0; i < RLManager.instance.settings.actorNetStructure.Length - 1; i++) // Iterate through each layer and stop at second to last layer
                {
                    RLManager.instance.settings.actorNetStructure[i] = tempNet[i]; // Set number of neurons in the layer
                }

                RLManager.instance.settings.actorNetStructure[RLManager.instance.settings.actorNetStructure.Length - 1] = tempNet[tempNet.Length - 1]; // Set the output layer
            }
        }
        else
        {
            // Check layer max
            if (RLManager.instance.settings.actorNetStructure.Length <= layerMax)
            {
                // Check non-zero
                if (actorHiddenipt.text != "0")
                {
                    int[] tempNet = RLManager.instance.settings.actorNetStructure; // Temporarily save network
                    Settings.LayerActivations[] tempActs = RLManager.instance.settings.actorActivations; // Temporarily save layer activations

                    RLManager.instance.settings.actorNetStructure = new int[tempNet.Length + 1]; // Add one layer to network
                    RLManager.instance.settings.actorActivations = new Settings.LayerActivations[tempActs.Length + 1]; // Add one layer to activation functions

                    for (int i = 0; i < tempNet.Length - 1; i++) // Iterate through each layer in the original array and stop at second to last layer
                    {
                        RLManager.instance.settings.actorNetStructure[i] = tempNet[i]; // Set number of neurons in the layer
                        RLManager.instance.settings.actorActivations[i] = tempActs[i]; // Set the activation function in the layer
                    }
                    // Set the neurons in the new hidden layer
                    RLManager.instance.settings.actorNetStructure[RLManager.instance.settings.actorNetStructure.Length - 2] = int.Parse(actorHiddenipt.text);
                    // Set new hidden layer activation function to the previous layer activation function by default
                    RLManager.instance.settings.actorActivations[RLManager.instance.settings.actorActivations.Length - 2] = RLManager.instance.settings.actorActivations[RLManager.instance.settings.actorActivations.Length - 3];

                    // Set the output layer
                    RLManager.instance.settings.actorNetStructure[RLManager.instance.settings.actorNetStructure.Length - 1] = tempNet[tempNet.Length - 1];
                    RLManager.instance.settings.actorActivations[RLManager.instance.settings.actorActivations.Length - 1] = tempActs[tempActs.Length - 1];
                }
                else
                {
                    Debug.Log("Can't have zero neurons in a layer");
                }
            }
        }
        UpdateNetworkStructureDisplay();
    }
    public void UpdateCriticInputs()
    {
        RLManager.instance.settings.criticNetStructure[0] = int.Parse(criticInputipt.text);
        UpdateNetworkStructureDisplay();
    }
    public void UpdateCriticHidden(bool isRemoving)
    {
        if (isRemoving)
        {
            if (RLManager.instance.settings.criticNetStructure.Length > 2)
            {
                int[] tempNet = RLManager.instance.settings.criticNetStructure; // Temporarily store the network structure
                RLManager.instance.settings.criticNetStructure = new int[tempNet.Length - 1]; // Remove one layer

                for (int i = 0; i < RLManager.instance.settings.criticNetStructure.Length - 1; i++) // Iterate through each layer and stop at second to last layer
                {
                    RLManager.instance.settings.criticNetStructure[i] = tempNet[i]; // Set number of neurons in the layer
                }

                RLManager.instance.settings.criticNetStructure[RLManager.instance.settings.criticNetStructure.Length - 1] = tempNet[tempNet.Length - 1]; // Set the output layer
            }
        }
        else
        {
            // Check layer max
            if (RLManager.instance.settings.criticNetStructure.Length <= layerMax)
            {
                // Check non-zero
                if (criticHiddenipt.text != "0")
                {
                    int[] tempNet = RLManager.instance.settings.criticNetStructure; // Temporarily save network
                    Settings.LayerActivations[] tempActs = RLManager.instance.settings.criticActivations; // Temporarily save layer activations

                    RLManager.instance.settings.criticNetStructure = new int[tempNet.Length + 1]; // Add one layer to network
                    RLManager.instance.settings.criticActivations = new Settings.LayerActivations[tempActs.Length + 1]; // Add one layer to activation functions

                    for (int i = 0; i < tempNet.Length - 1; i++) // Iterate through each layer in the original array and stop at second to last layer
                    {
                        RLManager.instance.settings.criticNetStructure[i] = tempNet[i]; // Set number of neurons in the layer
                        RLManager.instance.settings.criticActivations[i] = tempActs[i]; // Set the activation function in the layer
                    }
                    // Set the neurons in the new hidden layer
                    RLManager.instance.settings.criticNetStructure[RLManager.instance.settings.criticNetStructure.Length - 2] = int.Parse(criticHiddenipt.text);
                    // Set new hidden layer activation function to the previous layer activation function by default
                    RLManager.instance.settings.criticActivations[RLManager.instance.settings.criticActivations.Length - 2] = RLManager.instance.settings.criticActivations[RLManager.instance.settings.criticActivations.Length - 3];

                    // Set the output layer
                    RLManager.instance.settings.criticNetStructure[RLManager.instance.settings.criticNetStructure.Length - 1] = tempNet[tempNet.Length - 1];
                    RLManager.instance.settings.criticActivations[RLManager.instance.settings.criticActivations.Length - 1] = tempActs[tempActs.Length - 1];
                }
                else
                {
                    Debug.Log("Can't have zero neurons in a layer");
                }
            }
        }
        UpdateNetworkStructureDisplay();
    }
    public void UpdateAgentName()
    {
        RLManager.instance.settings.agentName = fileIpt.text;
    }
    public void UpdateEpiMax()
    {
        RLManager.instance.settings.episodeMax = int.Parse(maxEpsisodeIpt.text);
    }
    public void UpdateEpiMaxSteps()
    {
        RLManager.instance.settings.epiMaxSteps = int.Parse(stepsEpsIpt.text);
    }
    public void UpdateFramesPerState()
    {
        RLManager.instance.settings.framesPerState = int.Parse(frameStateIpt.text);
    }
    public void UpdateFrameBufferSize()
    {
        RLManager.instance.settings.frameBufferSize = int.Parse(frameBufferIpt.text);
    }
    public void UpdateEpsilon()
    {
        RLManager.instance.settings.epsilon = float.Parse(epsilonIpt.text);
    }
    public void UpdateEpsilonMin()
    {
        RLManager.instance.settings.epsilonMin = float.Parse(epsMinIpt.text);
    }
    public void UpdateEpsChangeFactor()
    {
        RLManager.instance.settings.epsDecayRate = float.Parse(epsDecayIpt.text);
    }
    public void UpdateExpBufferSize()
    {
        RLManager.instance.settings.expBufferSize = int.Parse(expBufferSizeIpt.text);
    }
    public void UpdateMiniBatchSize()
    {
        RLManager.instance.settings.miniBatchSize = int.Parse(miniBatchInpt.text);
    }
    public void UpdateNetCopyRate()
    {
        RLManager.instance.settings.netCopyRate = int.Parse(netCopyRateIpt.text);
    }
    public void UpdateGamma()
    {
        RLManager.instance.settings.gamma = float.Parse(gammaIpt.text);
    }
    public void UpdateLearningRate()
    {
        RLManager.instance.settings.dqnLearningRate = double.Parse(learningRateIpt.text);
    }
    public void UpdateBeta1()
    {
        RLManager.instance.settings.beta1 = float.Parse(beta1Ipt.text);
    }
    public void UpdateBeta2()
    {
        RLManager.instance.settings.beta2 = float.Parse(beta2Ipt.text);
    }
    public void UpdateEpsilonHat()
    {
        RLManager.instance.settings.epsilonHat = double.Parse(epsHatIpt.text);
    }
    public void UpdateGradientThreshold()
    {
        RLManager.instance.settings.gradientThreshold = double.Parse(gradientThreshIpt.text);
    }
    public void UpdateMaxViewDistance()
    {
        RLManager.instance.settings.maxViewDistance = float.Parse(maxViewIpt.text);
    }
    public void UpdateFoV()
    {
        RLManager.instance.settings.fieldOfView = float.Parse(fovIpt.text);
    }
    public void UpdateColDetectRange()
    {
        RLManager.instance.settings.collisionDetectRange = float.Parse(colDetectIpt.text);
    }
    public void UpdatePPOClip()
    {
        RLManager.instance.settings.ppoClip = float.Parse(clipIpt.text);
    }
    public void UpdateEntropyBonus()
    {
        RLManager.instance.settings.entropyBonus = double.Parse(entropyIpt.text);
    }
    public void UpdateTAU()
    {
        RLManager.instance.settings.tau = double.Parse(tauIpt.text);
    }
    public void UpdateTrainingEpochs()
    {
        RLManager.instance.settings.trainingEpochs = int.Parse(trEpochsIpt.text);
    }
    public void UpdateLoadSettingsToggle()
    {
        isLoadSettings = loadSettingsToggle.isOn;
    }

    public void UpdateTrainingToggle()
    {
        isTrainingTemp = trainingToggle.isOn;
    }
    public void UpdateActorLearning()
    {
        RLManager.instance.settings.actorLearningRate = double.Parse(actorLRIpt.text);
    }
    public void UpdateCriticLearning()
    {
        RLManager.instance.settings.criticLearningRate = double.Parse(criticLRIpt.text);
    }
    public void UpdateAsyncAgents()
    {
        RLManager.instance.settings.asyncAgents = int.Parse(asyncAgentIpt.text);
    }
    public void UpdateAlgoDropdown()
    {
        int index = algoDropdown.value;
        if (algoDropdown.options[index].text == "Proximal_Policy_Optimization")
        {
            RLManager.instance.settings.algo = Settings.Algorithm.Proximal_Policy_Optimization;
        }
        else if (algoDropdown.options[index].text == "Double_DQN")
        {
            RLManager.instance.settings.algo = Settings.Algorithm.Double_DQN;
        }
    }
    public void UpdateHUD()
    {
        episodeNumber.text = rlComponent.episodeNum.ToString();
        maxEpisodes.text = RLManager.instance.settings.episodeMax.ToString();
        epochs.text = rlComponent.epochs.ToString();
        totalReward.text = rlComponent.totalReward.ToString();
        currentReward.text = rlComponent.episodeReward.ToString();
        epsilon.text = RLManager.instance.settings.epsilon.ToString();
        epsilonMin.text = RLManager.instance.settings.epsilonMin.ToString();
        episodeSteps.text = rlComponent.epiSteps.ToString();
        episodeMaxSteps.text = RLManager.instance.settings.epiMaxSteps.ToString();
        
        if (rlComponent.epochs % RLManager.instance.costUpdateInEpochs == 0 && rlComponent.cost != null)
        {
            currentCost.text = rlComponent.cost.ToString();
        }
    }
    public void UpdateEpisodeAverage(double totalReward, int episodeNumber)
    {
        // Episode number - 1 is required since episode advances before average is calculated
        episodeAverage.text = (totalReward / (episodeNumber - 1)).ToString();
    }
    public void ResumeGame()
    {
        GameManager.instance.gameState = "continue";
        rlComponent.Init_Settings();
        SaveLoad.SaveSettings(settingsName, RLManager.instance.settings.saveLocation);
    }
    public void SaveAgent()
    {
        fileName = fileIpt.text + "_e" + rlComponent.episodeNum;
        settingsName = fileName + "_settings";
        Debug.Log("Saving..." + fileName);
        SaveLoad.SaveNet(fileName, rlComponent, RLManager.instance.settings.saveLocation);
        SaveLoad.SaveSettings(settingsName, RLManager.instance.settings.saveLocation);
    }
    public void LoadAgent()
    {
        if (RLManager.instance.rlComponent != null)
        {
            Destroy(RLManager.instance.agentObjectsList[0]);
            RLManager.instance.agentObjectsList.Clear();
        }
        RLManager.instance.SpawnAgent();
        rlComponent = RLManager.instance.rlComponent;
        fileName = fileIpt.text;
        settingsName = fileName + "_settings";
        SaveLoad.LoadNet(fileName, rlComponent, RLManager.instance.settings.saveLocation);
        if (isLoadSettings)
        {
            SaveLoad.LoadSettings(settingsName, RLManager.instance.settings.saveLocation);
            UpdateSettingsDisplay();
        }
        if (RLManager.instance.settings.algo == Settings.Algorithm.Double_DQN)
        {
            rlComponent.Init_New_DDQN_Session(false, isTrainingTemp, RLManager.instance.settings.dqnNetStructure);
        }
        else if (RLManager.instance.settings.algo == Settings.Algorithm.Proximal_Policy_Optimization)
        {
            rlComponent.Init_New_PPO_Session(false, isTrainingTemp, RLManager.instance.settings.actorNetStructure, RLManager.instance.settings.criticNetStructure);
        }

        Debug.Log("Loading..." + fileName);
        GameManager.instance.gameState = "continue";
    }
    public void NewAgent()
    {
        fileName = fileIpt.text;
        if (fileName == "")
        {
            Debug.Log("Please enter a valid name.");
        }
        else
        {
            if (RLManager.instance.rlComponent != null)
            {
                Destroy(RLManager.instance.agentObjectsList[0]);
                RLManager.instance.agentObjectsList.Clear();
            }
            settingsName = fileName + "_new_settings";
            fileName = fileName + "_new";
            RLManager.instance.SpawnAgent();
            rlComponent = RLManager.instance.rlComponent;
            if (RLManager.instance.settings.algo == Settings.Algorithm.Double_DQN)
            {
                rlComponent.Init_New_DDQN_Session(true, isTrainingTemp, RLManager.instance.settings.dqnNetStructure);
            }
            else if (RLManager.instance.settings.algo == Settings.Algorithm.Proximal_Policy_Optimization)
            {
                rlComponent.Init_New_PPO_Session(true, isTrainingTemp, RLManager.instance.settings.actorNetStructure, RLManager.instance.settings.criticNetStructure);
            }
            SaveLoad.SaveNet(fileName, rlComponent, RLManager.instance.settings.saveLocation);
            SaveLoad.SaveSettings(settingsName, RLManager.instance.settings.saveLocation);
            Debug.Log("New Game..." + fileName);
            GameManager.instance.gameState = "continue";
        }
    }
}
