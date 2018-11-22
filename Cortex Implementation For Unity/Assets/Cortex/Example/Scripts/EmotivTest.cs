using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Different method's id send to the cortex, one for every method.
/// </summary>
public enum MethodsID
{
    Login,
    Logout,
    Authorize,
    CreateSession,
    Suscribe,
    Training
}

public class EmotivTest : MonoBehaviour
{
    // Constants

    const string EMOTIV_URL = "wss://emotivcortex.com:54321";
    const string CLIENT_ID = "DT0fkgceOzKZgjuoBEyNYkHVAGVPJESnlsBT23y6";
    const string CLIENT_SECRET = "5XMdQKCJEBz9IGbNBBJPYwgGmzKx4Vy55WQsMbhajvaB5XIQG1XGEGP5Cg2Ou893aDHtMWbsbjMyMI7JK5PDelmDsD59p48e3XiDbpDmawpGIBXPAPMiRpKCwVD6Dbl6";

    // static fields

    static WebSocket w;

    static string username = "XXX";

    static string userPassword = "XXX!";

    public void set_Username(string s)
    {
        username = s;
    }

    public void set_UserPassword(string s)
    {
        userPassword = s;
    }

    static bool isTrainingDone;

    static bool isTrainingComplete = true;

    // private fields
    [SerializeField]
    GameObject loginCanvas;

    [SerializeField]
    GameObject userInfoCanvas;

    [SerializeField]
    GameObject trainingCanvas;

    string _auth;

    // Events

    public delegate void MentalCommandEvent(string action);
    public static event MentalCommandEvent OnMentalCommandEvent;

    public void Init ()
    {
        StartCoroutine(Initialize());	
	}

    /// <summary>
    /// This thread creates a connection with the cortex service through the websocket and then calls the login, createSession and subscribe methods.
    /// </summary>
    private IEnumerator Initialize()
    {
        w = new WebSocket(new Uri(EMOTIV_URL));
        yield return StartCoroutine(w.Connect());
        StartCoroutine(GetReply());

        Dictionary<string, string> loginDictionary = new Dictionary<string, string>();

        loginDictionary.Add("username", username);
        loginDictionary.Add("password", userPassword);
        loginDictionary.Add("client_id", CLIENT_ID);
        loginDictionary.Add("client_secret", CLIENT_SECRET);
        w.SendString(
            CortexJsonUtility.GetMethodJSON
            (
                "login",
                (int)MethodsID.Login,
                loginDictionary
            ));

        Authorize();
        StartCoroutine(CreateSession());
        StartCoroutine(SubscribeStreams());
    }

    /// <summary>
    /// Calls the authorize method with the corresponding client id and secret.
    /// </summary>
    void Authorize()
    {
        Dictionary<string, string> authorizeDictionary = new Dictionary<string, string>();
        authorizeDictionary.Add("client_id", CLIENT_ID);
        authorizeDictionary.Add("client_secret", CLIENT_SECRET);

        w.SendString(
           CortexJsonUtility.GetMethodJSON
           (
               "authorize",
               (int)MethodsID.Authorize,
               authorizeDictionary
           ));
    }

    /// <summary>
    /// Waits until the _auth token is gathered, then it calls the createSession method.
    /// </summary>
    IEnumerator CreateSession()
    {
        while (_auth == null)
        {
            yield return 0;
        }

        Dictionary<string, string> sessionDictionary = new Dictionary<string, string>();

        sessionDictionary.Add("_auth", _auth);
        sessionDictionary.Add("status", "open");

        w.SendString(
            CortexJsonUtility.GetMethodJSON
            (
                "createSession",
                (int)MethodsID.CreateSession,
                sessionDictionary
            ));
    }

    /// <summary>
    /// Handles the cortex responses.
    /// </summary>
    IEnumerator GetReply()
    {
        while (true)
        {
            string reply = w.RecvString();
            if (reply != null)
            {
                Debug.Log(reply);

                JSONObject replyObj = new JSONObject(reply);

                // If there´s a field named "result", it is the actual response to the sent method
                JSONObject resultObject = replyObj.GetField("result");
                if (resultObject)
                {
                    JSONObject idObject = replyObj.GetField("id");

                    switch ((MethodsID)(int)idObject.n)
                    {
                        case MethodsID.Login:
                            HandleLoginSuccess();
                            break;
                        case MethodsID.Logout:
                            HandleLogoutSuccess();
                            break;
                        case MethodsID.Authorize:
                            _auth = CortexJsonUtility.GetFieldFromJSONObject(replyObj, "_auth");
                            break;
                        case MethodsID.CreateSession:
                            trainingCanvas.SetActive(true);
                            break;
                    }
                }

                // If there´s a field named "sys", it is an event triggered from a training process
                JSONObject sysArray = replyObj.GetField("sys");
                if (sysArray)
                {
                    switch (sysArray[1].str)
                    {
                        case "MC_Succeeded":
                            isTrainingDone = true;
                            break;
                        case "MC_Completed":
                            isTrainingComplete = true;
                            break;
                        case "MC_Rejected":
                            isTrainingComplete = true;
                            break;
                        case "MC_Failed":
                            isTrainingComplete = true;
                            break;
                    }
                }

                // If there´s a field named "com", it is an event triggered from a mental command detection
                JSONObject comArray = replyObj.GetField("com");
                if (comArray)
                {
                    if (OnMentalCommandEvent != null)
                    {
                        // Call functions assigned to the OnMentalCommandEvent delegate with the action received from the Cortex as the parameter
                        OnMentalCommandEvent(comArray[0].str);
                    }
                }
            }
            yield return 0;
        }
    }

    /// <summary>
    /// Hides login canvas and shows the userInfo canvas with the corresponding username.
    /// </summary>
    void HandleLoginSuccess()
    {
        loginCanvas.SetActive(false);
        userInfoCanvas.SetActive(true);
        userInfoCanvas.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = username;
    }

    /// <summary>
    /// Calls the logout method
    /// </summary>
    public void Logout()
    {
        Dictionary<string, string> logoutDictionary = new Dictionary<string, string>();

        logoutDictionary.Add("username", username);
        w.SendString(
            CortexJsonUtility.GetMethodJSON
            (
                "logout",
                (int)MethodsID.Logout,
                logoutDictionary
            ));
    }

    /// <summary>
    /// Hides the userInfo canvas, the training canvas and shows the login canvas.
    /// </summary>
    private void HandleLogoutSuccess()
    {
        loginCanvas.SetActive(true);
        userInfoCanvas.SetActive(false);
        trainingCanvas.SetActive(false);
    }

    /// <summary>
    /// Waits until the _auth token is gathered, then calls the subscribe method with the "sys" and "com" streams as parameters.
    /// </summary>
    IEnumerator SubscribeStreams()
    {
        while (_auth == null)
        {
            yield return 0;
        }
        w.SendString(
            CortexJsonUtility.GetSuscribtionJson
            (
                (int)MethodsID.Suscribe, 
                _auth,
                new string[] { "sys", "com" }
            ));
    }

    /// <summary>
    /// Starts the InitializeTraining coroutine with a given action.
    /// </summary>
    /// <param name="action">
    /// Action to be trained
    /// </param>
    public void Train(string action)
    {
        StartCoroutine(InitializeTraining("mentalCommand", action));
    }

    /// <summary>
    /// Thread in charge of the whole proccess of training an action, it could end up in an accepted training, a rejected one or a failed one.
    /// </summary>
    /// <param name="detectionType"></param>
    /// <param name="action"></param>
    IEnumerator InitializeTraining(string detectionType, string action)
    {
        if (isTrainingComplete)
        {
            Dictionary<string, string> initialParams = new Dictionary<string, string>();
            initialParams.Add("_auth", _auth);
            initialParams.Add("detection", detectionType);
            initialParams.Add("action", action);
            initialParams.Add("status", "start");
            SendTrainMessage(initialParams);
            isTrainingDone = false;
            isTrainingComplete = false;
            while (!isTrainingDone)
                yield return 0;
            DelegateSelectionMenu delegateSelection = SendTrainMessage;

            Dictionary<string, string> acceptParams = new Dictionary<string, string>();
            acceptParams.Add("_auth", _auth);
            acceptParams.Add("detection", detectionType);
            acceptParams.Add("action", action);
            acceptParams.Add("status", "accept");

            Dictionary<string, string> rejectParams = new Dictionary<string, string>();
            rejectParams.Add("_auth", _auth);
            rejectParams.Add("detection", detectionType);
            rejectParams.Add("action", action);
            rejectParams.Add("status", "reject");

            MessageQuestionSelection.ShowMessageWithSelection
                (
                "Aceptas el entrenamiento realizado de la accion " + action.ToUpper() + "?",
                delegateSelection,
                acceptParams,
                rejectParams
                );
        }
    }

    /// <summary>
    /// Calls a training method with some given parameters.
    /// </summary>
    /// <param name="parameters"></param>
    public void SendTrainMessage(Dictionary<string, string> parameters)
    {
        w.SendString(
            CortexJsonUtility.GetMethodJSON
            (
                "training", 
                (int)MethodsID.Training, 
                parameters
            ));
    }
}
