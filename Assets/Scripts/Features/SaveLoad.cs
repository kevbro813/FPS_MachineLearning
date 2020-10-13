using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static void SaveNet(string fileName, RLComponent rl)
    {
        if (File.Exists(Path.Combine("E://ML_Specimen", fileName)))
        {
            Debug.Log("A file with that name already exists. Please choose a different name.");
        }
        else
        {
            NeuralNetwork nets = rl.mainNet;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Path.Combine("E://ML_Specimen", fileName));
            bf.Serialize(file, nets);
            file.Close();
            //Debug.Log("Save Network: " + fileName);
        }
    }
    public static void LoadNet(string fileName, RLComponent rl)
    {
        if (File.Exists(Path.Combine("E://ML_Specimen", fileName)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine("E://ML_Specimen", fileName), FileMode.Open);
            rl.mainNet = (NeuralNetwork)bf.Deserialize(file);
            file.Close();
            Debug.Log("Load Network: " + fileName);
        }
        else
        {
            Debug.Log("There is no file with that name.");
        }
    }
    public static void SaveSettings(string fileName)
    {
        Settings s = RLManager.instance.settings;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path.Combine("E://ML_Specimen", fileName));
        bf.Serialize(file, s);
        file.Close();
        //Debug.Log("Save Settings: " + fileName);
    }
    public static void LoadSettings(string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Path.Combine("E://ML_Specimen", fileName), FileMode.Open);
        RLManager.instance.loadSettings = (Settings)bf.Deserialize(file);
        file.Close();
        RLManager.instance.LoadSettings();
        //Debug.Log("Load Settings: " + fileName);
    }

}
