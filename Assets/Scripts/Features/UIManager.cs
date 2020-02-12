using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public string fileName;
    public string settingsName;
    public InputField fileIpt;
    public InputField maxEpsisodeIpt;
    public InputField stepsEpsIpt;
    public InputField frameStateIpt;
    public InputField frameBufferIpt;
    public InputField epsilonIpt;
    public InputField epsMinIpt;
    public InputField epsChangeIpt;
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
    public Text episodeNumber;
    public Text maxEpisodes;
    public Text epochs;
    public Text totalReward;
    public Text currentReward;
    public Text epsilon;
    public Text epsilonMin;
    public Text episodeSteps;
    public Text episodeMaxSteps;
    private bool isUpdate = true;
    public DQN dqn;
    public GameObject resumeButton;
    public GameObject saveButton;

    // Start is called before the first frame update
    void Start()
    {
        UpdateSettingsDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (dqn != null && !GameManager.instance.adminMenu.activeSelf)
        {
            UpdateHUD();
            isUpdate = true;
        }
        else if(GameManager.instance.adminMenu.activeSelf && isUpdate)
        {
            UpdateSettingsDisplay();
            if (GameManager.instance.isAgentLoaded)
            {
                resumeButton.SetActive(true);
                saveButton.SetActive(true);
            }
            else
            {
                resumeButton.SetActive(false);
                saveButton.SetActive(false);
            }
            isUpdate = false;
        }
    }
    public void UpdateSettingsDisplay()
    {
        maxEpsisodeIpt.text = GameManager.instance.settings.episodeMax.ToString();
        stepsEpsIpt.text = GameManager.instance.settings.epiMaxSteps.ToString();
        frameStateIpt.text = GameManager.instance.settings.framesPerState.ToString();
        frameBufferIpt.text = GameManager.instance.settings.frameBufferSize.ToString();
        epsilonIpt.text = GameManager.instance.settings.epsilon.ToString();
        epsMinIpt.text = GameManager.instance.settings.epsilonMin.ToString();
        epsChangeIpt.text = GameManager.instance.settings.epsChangeFactor.ToString();
        expBufferSizeIpt.text = GameManager.instance.settings.expBufferSize.ToString();
        miniBatchInpt.text = GameManager.instance.settings.miniBatchSize.ToString();
        netCopyRateIpt.text = GameManager.instance.settings.netCopyRate.ToString();
        gammaIpt.text = GameManager.instance.settings.gamma.ToString();
        learningRateIpt.text = GameManager.instance.settings.learningRate.ToString();
        beta1Ipt.text = GameManager.instance.settings.beta1.ToString();
        beta2Ipt.text = GameManager.instance.settings.beta2.ToString();
        epsHatIpt.text = GameManager.instance.settings.epsilonHat.ToString();
        gradientThreshIpt.text = GameManager.instance.settings.gradientThreshold.ToString();
        maxViewIpt.text = GameManager.instance.settings.maxViewDistance.ToString();
        fovIpt.text = GameManager.instance.settings.fieldOfView.ToString();
        colDetectIpt.text = GameManager.instance.settings.collisionDetectRange.ToString();
    }
    public void UpdateAgentName()
    {
        GameManager.instance.settings.agentName = fileIpt.text;
    }
    public void UpdateEpiMax()
    {
        GameManager.instance.settings.episodeMax = int.Parse(maxEpsisodeIpt.text);
    }
    public void UpdateEpiMaxSteps()
    {
        GameManager.instance.settings.epiMaxSteps = int.Parse(stepsEpsIpt.text);
    }
    public void UpdateFramesPerState()
    {
        GameManager.instance.settings.framesPerState = int.Parse(frameStateIpt.text);
    }
    public void UpdateFrameBufferSize()
    {
        GameManager.instance.settings.frameBufferSize = int.Parse(frameBufferIpt.text);
    }
    public void UpdateEpsilon()
    {
        GameManager.instance.settings.epsilon = float.Parse(epsilonIpt.text);
    }
    public void UpdateEpsilonMin()
    {
        GameManager.instance.settings.epsilonMin = float.Parse(epsMinIpt.text);
    }
    public void UpdateEpsChangeFactor()
    {
        GameManager.instance.settings.epsChangeFactor = float.Parse(epsChangeIpt.text);
    }
    public void UpdateExpBufferSize()
    {
        GameManager.instance.settings.expBufferSize = int.Parse(expBufferSizeIpt.text);
    }
    public void UpdateMiniBatchSize()
    {
        GameManager.instance.settings.miniBatchSize = int.Parse(miniBatchInpt.text);
    }
    public void UpdateNetCopyRate()
    {
        GameManager.instance.settings.netCopyRate = int.Parse(netCopyRateIpt.text);
    }
    public void UpdateGamma()
    {
        GameManager.instance.settings.gamma = float.Parse(gammaIpt.text);
    }
    public void UpdateLearningRate()
    {
        GameManager.instance.settings.learningRate = double.Parse(learningRateIpt.text);
    }
    public void UpdateBeta1()
    {
        GameManager.instance.settings.beta1 = float.Parse(beta1Ipt.text);
    }
    public void UpdateBeta2()
    {
        GameManager.instance.settings.beta2 = float.Parse(beta2Ipt.text);
    }
    public void UpdateEpsilonHat()
    {
        GameManager.instance.settings.epsilonHat = double.Parse(epsHatIpt.text);
    }
    public void UpdateGradientThreshold()
    {
        GameManager.instance.settings.gradientThreshold = double.Parse(gradientThreshIpt.text);
    }
    public void UpdateMaxViewDistance()
    {
        GameManager.instance.settings.maxViewDistance = float.Parse(maxViewIpt.text);
    }
    public void UpdateFoV()
    {
        GameManager.instance.settings.fieldOfView = float.Parse(fovIpt.text);
    }
    public void UpdateColDetectRange()
    {
        GameManager.instance.settings.collisionDetectRange = float.Parse(colDetectIpt.text);
    }
    public void UpdateHUD()
    {
        episodeNumber.text = dqn.episodeNum.ToString();
        maxEpisodes.text = dqn.episodeMax.ToString();
        epochs.text = dqn.epochs.ToString();
        totalReward.text = dqn.totalReward.ToString();
        currentReward.text = dqn.episodeReward.ToString();
        epsilon.text = dqn.epsilon.ToString();
        epsilonMin.text = dqn.epsilonMin.ToString();
        episodeSteps.text = dqn.epiSteps.ToString();
        episodeMaxSteps.text = dqn.epiMaxSteps.ToString();
    }
    public void ResumeGame()
    {
        GameManager.instance.gameState = "continue";
        SaveLoad.SaveSettings(settingsName);
        GameManager.instance.dqn.LoadSettings();
    }
    public void SaveAgent()
    {
        fileName = fileIpt.text + "_e" + dqn.episodeNum;
        settingsName = fileName + "_settings.gd";
        fileName = fileName + ".gd";
        Debug.Log("Saving..." + fileName);
        SaveLoad.SaveNet(fileName, GameManager.instance.dqn);
        SaveLoad.SaveSettings(settingsName);
        dqn = GameManager.instance.dqn;
    }
    public void LoadAgent()
    {
        if (GameManager.instance.dqn != null)
        {
            Destroy(GameManager.instance.agentObjectsList[0]);
            GameManager.instance.agentObjectsList.Clear();
        }
        GameManager.instance.SpawnAgent();
        fileName = fileIpt.text;
        settingsName = fileName + "_settings.gd";
        fileName = fileName + ".gd";
        Debug.Log("Loading..." + fileName);
        SaveLoad.LoadNet(fileName, GameManager.instance.dqn);
        SaveLoad.LoadSettings(settingsName);
        UpdateSettingsDisplay();
        dqn = GameManager.instance.dqn;
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
            if (GameManager.instance.dqn != null)
            {
                Destroy(GameManager.instance.agentObjectsList[0]);
                GameManager.instance.agentObjectsList.Clear();
            }
            settingsName = fileName + "_new_settings.gd";
            fileName = fileName + "_new.gd";
            Debug.Log("New Game..." + fileName);

            GameManager.instance.gameState = "continue";
            GameManager.instance.SpawnAgent();
            dqn = GameManager.instance.dqn;
            SaveLoad.SaveNet(fileName, dqn);
            SaveLoad.SaveSettings(settingsName);
        }
    }
}
