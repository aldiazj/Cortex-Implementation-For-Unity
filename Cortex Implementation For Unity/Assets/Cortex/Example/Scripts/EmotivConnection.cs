using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EmotivConnection : MonoBehaviour
{
    // Constants

    const string EMOTIV_URL = "wss://emotivcortex.com:54321";    
    const string CLIENT_ID = "DT0fkgceOzKZgjuoBEyNYkHVAGVPJESnlsBT23y6";
    const string CLIENT_SECRET = "5XMdQKCJEBz9IGbNBBJPYwgGmzKx4Vy55WQsMbhajvaB5XIQG1XGEGP5Cg2Ou893aDHtMWbsbjMyMI7JK5PDelmDsD59p48e3XiDbpDmawpGIBXPAPMiRpKCwVD6Dbl6";

    // Private fields

    static WebSocket w;

    // Events

    public delegate void MentalCommandEvent(string action);
    public static event MentalCommandEvent OnMentalCommandEvent;

    // Properties

    static string username = "XXX";

    static string userPassword = "XXX!";

    public void set_Username(string s)
    {
        username = s;
    }

    public void set_UserPassword(string s)
    { userPassword = s; }

    static string _auth = "";

    public static string Auth
    {
        get
        {
            return _auth;
        }
        set
        {
            _auth = value;
        }
    }

    static bool isTrainingDone;

    public static bool IsTrainingDone
    {
        get
        {
            return isTrainingDone;
        }

        set
        {
            isTrainingDone = value;
        }
    }

    static bool isTrainingComplete = true;

    public static bool IsTrainingComplete
    {
        get
        {
            return isTrainingComplete;
        }

        set
        {
            isTrainingComplete = value;
        }
    }

    static bool isInitialized = false;

    public static bool IsInitialized
    {
        get
        {
            return isInitialized;
        }
    }

    /// <summary>
    /// Different method's id send to the cortex, one for every method
    /// </summary>
    public enum MethodsID
    {
        UserLogin,
        Authorize,
        Login,
        Logout,
        QueryHeadsets,
        CreateSession,
        Suscribe,
        Training
    }

    public void Init()
    {
        StartCoroutine(InitializeConnection());
    }

    void Start()
    {
        if (isInitialized)
            StartCoroutine(AccessData());
    }

    /// <summary>
    /// This coroutine is the initial thread that establishes the communication with the cortex service, 
    /// it creates the instance of the websocket and starts AccesData coroutine to begin recieving cortex's messages.
    /// Then, it sends the authorize and queryheadsets methods to the cortex.
    /// Finally, a new coroutine is started to create a new session and start receiving events from the cortex.
    /// </summary>
    IEnumerator InitializeConnection ()
	{
		w = new WebSocket (new Uri (EMOTIV_URL));
		yield return StartCoroutine (w.Connect ());
        StartCoroutine(AccessData());
        Dictionary<string, string> loginDictionary = new Dictionary<string, string>();

        //w.SendString(
        //    CortexJsonUtility.GetMethodJSON
        //    (
        //        "getUserLogin",
        //        (int)MethodsID.UserLogin
        //        ));

        loginDictionary.Add("username", username);
        loginDictionary.Add("password", userPassword);
        loginDictionary.Add("CLIENT_ID", CLIENT_ID);
        loginDictionary.Add("CLIENT_SECRET", CLIENT_SECRET);
        w.SendString(
            CortexJsonUtility.GetMethodJSON
            (
                "login",
                (int)MethodsID.Login,
                loginDictionary
            ));

        //if (_auth == "" || _auth == null)
        //    w.SendString(
        //        CortexJsonUtility.GetMethodJSON(
        //            "authorize",
        //            (int)MethodsID.Authorize
        //        ));
        //StartCoroutine(CreateSession());
    }

    private void OnDisable()
    {
        w.Close();
    }

    /// <summary>
    /// If the auth token is provided a session is created and a suscribtion is made to the "sys" and "com]" streams
    /// "sys" stream returns events related to training status. 
    /// "com" stream returns events related to mentalcommand actions
    /// </summary>
    IEnumerator CreateSession()
    {
        while (_auth == "" || _auth == null)
            yield return 0;
        Dictionary<string, string> sessionParameters = new Dictionary<string, string>();
        sessionParameters.Add("_auth", _auth);
        sessionParameters.Add("status", "open");
        w.SendString(CortexJsonUtility.GetMethodJSON("createSession",(int)MethodsID.CreateSession, sessionParameters));
        w.SendString(CortexJsonUtility.GetSuscribtionJson((int)MethodsID.Suscribe, _auth, new string[] { "sys", "com" }));
        isInitialized = true;
    }

    /// <summary>
    /// Starts the training of a given mental command action
    /// </summary>
    /// <param name="action">
    /// Action to be trained
    /// </param>
    public void Train(string action)
    {
        StartCoroutine(InitializeTraining("mentalCommand", action));
    }

    public void ResetTrain(string action)
    {
        Dictionary<string, string> eraseParams = new Dictionary<string, string>();
        eraseParams.Add("_auth", _auth);
        eraseParams.Add("detection", "mentalCommand");
        eraseParams.Add("action", action);
        eraseParams.Add("status", "erase");
        SendTrainMessage(eraseParams);
    }

    /// <summary>
    /// Coroutine in charge of the trainning initialization
    /// </summary>
    /// <param name="detectionType">
    /// String with the name of the detection type to be trained ("mentalCommand" or "facialExpression")
    /// </param>
    /// <param name="action">
    /// String with the name of the action to be trained, have a look at the list of action in the getdetectioninfo method reference at https://emotiv.github.io/cortex-docs/#getdetectioninfo
    /// </param>
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
                "Aceptas el entrenamiento realizado?",
                delegateSelection,
                acceptParams,
                rejectParams
                );
        }
    }


    /// <summary>
    /// Send a train message to the cortex
    /// </summary>
    /// <param name="parameters">
    /// array of strings containing the following parameters and in the given order:
    /// 
    /// detectionType: String with the name of the detection type to be trained ("mentalCommand" or "facialExpression")
    /// 
    /// action: String with the name of the action to be trained, 
    /// have a look at the list of action in the getdetectioninfo method reference at https://emotiv.github.io/cortex-docs/#getdetectioninfo
    /// 
    /// status: String with the status of the training ("start", "accept", "reject", "erase", "reset") 
    /// </param>
    public void SendTrainMessage(Dictionary<string, string> parameters)
    {
            w.SendString(CortexJsonUtility.GetMethodJSON("training", (int)MethodsID.Training, parameters));
    }

    /// <summary>
    /// Thread in charge of the Corte Service responses.
    /// Depending on the fields found in the JSONObject response, a certain action is done
    /// </summary>
    IEnumerator AccessData()
    {
        while (true)
        {
            string reply = w.RecvString();
            if (reply != null)
            {
                JSONObject replyObj = new JSONObject(reply);
                Debug.Log(reply);

                // If there´s a field named "result", it is the actual response to sent method
                JSONObject resultObject = replyObj.GetField("result");
                if (resultObject)
                {
                    JSONObject idObject = replyObj.GetField("id");

                    switch ((MethodsID)(int)idObject.n)
                    {
                        case MethodsID.Authorize:
                            _auth = CortexJsonUtility.GetFieldFromJSONObject(replyObj,"_auth");
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
}