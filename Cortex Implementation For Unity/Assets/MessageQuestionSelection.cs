using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MessageQuestionSelection
{
    /// <summary>
    /// Shows a menu with a given text and lets the user press one of two buttons.
    /// These buttons are assigned to a certain delegated passed as parameter. Finally, this functions on the delegates are called using the aproppiate arguments passed as parameter.
    /// </summary>
    /// <param name="messageQuestion"></param>
    /// Provided text to be shown.
    /// <param name="answer"></param>
    /// delegate containing the function that will be executed once the button is pressed.
    /// <param name="acceptParams"></param>
    /// String array with the accept parameters needed to execute the method in the delegate.
    /// <param name="rejectParams"></param>
    /// String array with the reject parameters needed to execute the method in the delegate.
    public static void ShowMessageWithSelection(string messageQuestion, DelegateSelectionMenu answer, Dictionary<string, string> acceptParams, Dictionary<string, string> rejectParams)
	{
		GameObject messageWithSelection = GameObject.Find("Canvas_Selection_Message");
		if(messageWithSelection == null)
		{
			messageWithSelection = GameObject.Instantiate(Resources.Load("Canvas_Selection_Message", typeof(GameObject))) as GameObject;
			messageWithSelection.name = "Select_Message";
		}

		messageWithSelection.SetActive(true);
		messageWithSelection.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = messageQuestion;

		messageWithSelection.transform.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(() => answer.Invoke(acceptParams));
		messageWithSelection.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(() => answer.Invoke(rejectParams));
	}

}

/// <summary>
/// A delegate is created to be used for the ShowMessageWithSelection answer method.
/// </summary>
/// <param name="parameters">
/// Call ShowMessageWithSelection answer.
/// </param>
public delegate void DelegateSelectionMenu (Dictionary<string,string> parameters); 
