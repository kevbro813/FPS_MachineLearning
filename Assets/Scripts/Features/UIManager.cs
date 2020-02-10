using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public string fileName;
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

    public DQN dqn;

    // Start is called before the first frame update
    void Start()
    {
        maxEpsisodeIpt.text = "1000";
        stepsEpsIpt.text = "1000";
        frameStateIpt.text = "4";
        frameBufferIpt.text = "10000";
        epsilonIpt.text = "1";
        epsMinIpt.text = "0.1";
        epsChangeIpt.text = "500000";
        expBufferSizeIpt.text = "5000";
        miniBatchInpt.text = "32";
        netCopyRateIpt.text = "100";
        gammaIpt.text = "0.95";
        learningRateIpt.text = "0.000000001";
        beta1Ipt.text = "0.9";
        beta2Ipt.text = "0.999";
        epsHatIpt.text = "0.000001";
        gradientThreshIpt.text = "1";
        maxViewIpt.text = "100";
        fovIpt.text = "45";
        colDetectIpt.text = "10";
    }

    // Update is called once per frame
    void Update()
    {
        if (dqn != null)
        {
            episodeNumber.text = dqn.episodeNum.ToString();
            maxEpisodes.text = dqn.episodeMax.ToString();
            epochs.text = dqn.epochs.ToString();
            totalReward.text = dqn.episodeReward.ToString();
            currentReward.text = dqn.episodeReward.ToString();
            epsilon.text = dqn.epsilon.ToString();
            epsilonMin.text = dqn.epsilonMin.ToString();
            episodeSteps.text = dqn.epiSteps.ToString();
            episodeMaxSteps.text = dqn.epiMaxSteps.ToString();
        }
    }

    public void LoadAgent()
    {
        if (GameManager.instance.dqn != null)
        {
            Destroy(GameManager.instance.agentObjectsList[0]);
        }
        GameManager.instance.gameState = "continue";
        GameManager.instance.SpawnAgent();
        fileName = fileIpt.text;
        fileName = fileName + ".gd";
        Debug.Log(fileName);
        SaveLoad.LoadNet(fileName, GameManager.instance.dqn);
        dqn = GameManager.instance.dqn;
    }
    public void NewAgent()
    {
        if (GameManager.instance.dqn != null)
        {
            Destroy(GameManager.instance.agentObjectsList[0]);
            GameManager.instance.agentObjectsList.Clear();
        }
        GameManager.instance.gameState = "continue";
        GameManager.instance.SpawnAgent();
        dqn = GameManager.instance.dqn;
    }
    public void SaveAgent()
    {
        fileName = fileIpt.text;
        fileName = fileName + ".gd";
        Debug.Log(fileName);
        SaveLoad.SaveNet(fileName, GameManager.instance.dqn);
        dqn = GameManager.instance.dqn;
    }
    public void ResumeGame()
    {
        GameManager.instance.gameState = "continue";
    }
}
