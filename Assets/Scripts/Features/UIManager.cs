using UnityEngine;
using UnityEngine.UI;

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
    public Text episodeNumber;
    public Text maxEpisodes;
    public Text epochs;
    public Text totalReward;
    public Text currentReward;
    public Text epsilon;
    public Text epsilonMin;
    public Text episodeSteps;
    public Text episodeMaxSteps;
    private bool isUpdate;
    public GameObject resumeButton;
    public GameObject saveButton;
    public Text currentCost;
    public Toggle trainingToggle;
    private bool isTrainingTemp;
    private void Start()
    {
        UpdateSettingsDisplay();
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
        learningRateIpt.text = RLManager.instance.settings.learningRate.ToString();
        beta1Ipt.text = RLManager.instance.settings.beta1.ToString();
        beta2Ipt.text = RLManager.instance.settings.beta2.ToString();
        epsHatIpt.text = RLManager.instance.settings.epsilonHat.ToString();
        gradientThreshIpt.text = RLManager.instance.settings.gradientThreshold.ToString();
        maxViewIpt.text = RLManager.instance.settings.maxViewDistance.ToString();
        fovIpt.text = RLManager.instance.settings.fieldOfView.ToString();
        colDetectIpt.text = RLManager.instance.settings.collisionDetectRange.ToString();
        asIpt.text = RLManager.instance.settings.autoSaveEpisode.ToString();
        if (rlComponent)
        {
            trainingToggle.isOn = rlComponent.isTraining;
        }
        else
        {
            trainingToggle.isOn = true;
        }
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
        RLManager.instance.settings.learningRate = double.Parse(learningRateIpt.text);
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

    public void UpdateTrainingToggle()
    {
        if (rlComponent)
        {
            rlComponent.isTraining = trainingToggle.isOn;
        }
        else
        {
            isTrainingTemp = trainingToggle.isOn;
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
    public void ResumeGame()
    {
        GameManager.instance.gameState = "continue";
        SaveLoad.SaveSettings(settingsName);
    }
    public void SaveAgent()
    {
        fileName = fileIpt.text + "_e" + rlComponent.episodeNum;
        settingsName = fileName + "_settings.gd";
        fileName = fileName + ".gd";
        Debug.Log("Saving..." + fileName);
        SaveLoad.SaveNet(fileName, rlComponent);
        SaveLoad.SaveSettings(settingsName);
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
        settingsName = fileName + "_settings.gd";
        fileName = fileName + ".gd";
        SaveLoad.LoadNet(fileName, rlComponent);
        SaveLoad.LoadSettings(settingsName);
        UpdateSettingsDisplay();
        rlComponent.Init_New_Session(false, isTrainingTemp);
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
            settingsName = fileName + "_new_settings.gd";
            fileName = fileName + "_new.gd";
            RLManager.instance.SpawnAgent();
            rlComponent = RLManager.instance.rlComponent;
            rlComponent.Init_New_Session(true, isTrainingTemp);
            SaveLoad.SaveNet(fileName, rlComponent);
            SaveLoad.SaveSettings(settingsName);
            Debug.Log("New Game..." + fileName);
            GameManager.instance.gameState = "continue";
        }
    }
}
