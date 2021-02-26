using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    #region Save/Load Network
    /// <summary>
    /// This method saves a neural network.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="rl"></param>
    /// <param name="saveLocation"></param>
    public static void SaveNet(string fileName, RLComponent rl, string saveLocation)
    {
        if (File.Exists(Path.Combine(saveLocation, fileName + ".gd"))) // If a file already exists with that name...
        {
            Debug.Log("A file with that name already exists. Please choose a different name.");
            // TODO: Add prompt for a new name.
        }
        else // The file is unique
        {
            if (rl.algo == Settings.Algorithm.Double_DQN) // If a Double DQN network, save the main net only (target net is a copy of main net)
            {
                NeuralNetwork nets = rl.mainNet;
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(Path.Combine(saveLocation, fileName +".gd"));
                bf.Serialize(file, nets);
                file.Close();
            }
            else if (rl.algo == Settings.Algorithm.Proximal_Policy_Optimization) // If PPO, save actor and critic networks
            {
                NeuralNetwork actorNet = rl.actorNet;
                BinaryFormatter bf1 = new BinaryFormatter();
                FileStream actorFile = File.Create(Path.Combine(saveLocation, fileName + "_Actor.gd"));
                bf1.Serialize(actorFile, actorNet);
                actorFile.Close();

                NeuralNetwork criticNet = rl.criticNet;
                BinaryFormatter bf2 = new BinaryFormatter();
                FileStream criticFile = File.Create(Path.Combine(saveLocation, fileName + "_Critic.gd"));
                bf2.Serialize(criticFile, criticNet);
                criticFile.Close();
            }
            //Debug.Log("Save Network: " + fileName);
        }
    }
    /// <summary>
    /// This method loads a neural network from file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="rl"></param>
    /// <param name="saveLocation"></param>
    public static void LoadNet(string fileName, RLComponent rl, string saveLocation)
    {
        if (File.Exists(Path.Combine(saveLocation, fileName + "_settings.gd"))) // Check that the file exists
        {
            if (File.Exists(Path.Combine(saveLocation, fileName +".gd"))) // If the file does not have an "_Actor" suffix, then it is a dqn network
            {
                rl.algo = Settings.Algorithm.Double_DQN;
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Path.Combine(saveLocation, fileName + ".gd"), FileMode.Open);
                rl.mainNet = (NeuralNetwork)bf.Deserialize(file);
                file.Close();
                Debug.Log("Load Network: " + fileName);
            }
            else if (File.Exists(Path.Combine(saveLocation, fileName + "_Actor.gd"))) // If the file has an "_Actor" suffix, then load both PPO networks
            {
                rl.algo = Settings.Algorithm.Proximal_Policy_Optimization; 
                BinaryFormatter bf1 = new BinaryFormatter();
                FileStream actorFile = File.Open(Path.Combine(saveLocation, fileName + "_Actor.gd"), FileMode.Open); // Load actor network
                rl.actorNet = (NeuralNetwork)bf1.Deserialize(actorFile);
                actorFile.Close();

                BinaryFormatter bf2 = new BinaryFormatter(); 
                FileStream criticFile = File.Open(Path.Combine(saveLocation, fileName + "_Critic.gd"), FileMode.Open); // Load critic network
                rl.criticNet = (NeuralNetwork)bf2.Deserialize(criticFile);
                criticFile.Close();
                Debug.Log("Load Network: " + fileName);
            }
        }
        else // File does not exist
        {
            Debug.Log("There is no file with that name.");
        }
    }

    public static void LoadFSMNetwork(string fileName, AgentFSM fsm, string saveLocation, int stateNumber)
    {

        if (File.Exists(Path.Combine(saveLocation, fileName + "_settings.gd"))) // Check that the file exists
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream actorFile = File.Open(Path.Combine(saveLocation, fileName + "_Actor.gd"), FileMode.Open); // Load actor network
            fsm.stateNeuralNetworks[stateNumber] = (NeuralNetwork)bf.Deserialize(actorFile);
            actorFile.Close();
        }
        else // File does not exist
        {
            Debug.Log("There is no file with that name.");
        }
    }
    #endregion

    #region Save/Load Settings
    /// <summary>
    /// Save the current settings.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="saveLocation"></param>
    public static void SaveSettings(string fileName, string saveLocation)
    {
        Settings s = RLManager.instance.settings;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path.Combine(saveLocation, fileName + ".gd"));
        bf.Serialize(file, s);
        file.Close();
        //Debug.Log("Save Settings: " + fileName);
    }
    /// <summary>
    /// Load settings. Typically loads with a neural network.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="saveLocation"></param>
    public static void LoadSettings(string fileName, string saveLocation)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Path.Combine(saveLocation, fileName + ".gd"), FileMode.Open);
        RLManager.instance.loadSettings = (Settings)bf.Deserialize(file);
        file.Close();
        RLManager.instance.LoadSettings();
        //Debug.Log("Load Settings: " + fileName);
    }
    #endregion
}
