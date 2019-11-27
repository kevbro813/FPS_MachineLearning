using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public string fileName;
    public InputField fileInputField;

    public void SaveNetwork()
    {
        fileName = fileInputField.text;
        fileName = fileName + ".gd";
        Debug.Log(fileName);
        SaveLoad.SaveNet(fileName);
    }
    public  void LoadNetwork()
    {
        fileName = fileInputField.text;
        fileName = fileName + ".gd";
        Debug.Log(fileName);
        SaveLoad.LoadNet(fileName);
    }
}
