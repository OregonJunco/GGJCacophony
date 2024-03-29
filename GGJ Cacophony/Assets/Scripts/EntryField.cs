﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EntryField : MonoBehaviour{

    public static InputField inputField;

	// Use this for initialization
	void Start () {
        inputField = GetComponent<InputField>();
    }

    void Update()
    {

        if (inputField.wasCanceled) {
            inputField.ActivateInputField();
            inputField.text = "";
        }
        if (inputField.interactable) {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }
        else
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void SendTextMessage()
    {
        if (Input.GetKey(KeyCode.Return) && !string.IsNullOrEmpty(inputField.text))
        {
            MessageSender instance = MessageSender.instance;
            if (instance != null)
            {
                instance.SendTextMessage(inputField.text);
            }
            inputField.ActivateInputField();
            inputField.text = "";
        }
    }

    public void Reset()
    {
        inputField.ActivateInputField();
        inputField.text = "";
    }

    public void RemoveCommands()
    {
        inputField.text = inputField.text.Replace("[", "");
        inputField.text = inputField.text.Replace("]", "");
        inputField.text = inputField.text.Replace(@"\", "");
        inputField.text = inputField.text.Replace("-", "");
        inputField.text = inputField.text.Replace("=", "");
        inputField.text = inputField.text.Replace("+", "");
        inputField.text = inputField.text.Replace("_", "");
        inputField.text = inputField.text.Replace("{", "");
        inputField.text = inputField.text.Replace("}", "");
        inputField.text = inputField.text.Replace("|", "");
    }

}
