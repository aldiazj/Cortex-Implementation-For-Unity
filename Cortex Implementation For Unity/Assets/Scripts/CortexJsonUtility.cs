using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class creates the JSONObjects needed to comunicate with the cortex service
/// </summary>
public class CortexJsonUtility
{
    /// <summary>
    /// Creates a JSONObject with to request a given method to the cortex
    /// </summary>
    /// <param name="methodName">
    /// Method to be called
    /// </param>
    /// <param name="methodId">
    /// Id number to the request response
    /// </param>
    /// <param name="parameters">
    /// Dictionary with the various parameters needed by the given method, if not provided or null is passed, "params" object will be empty
    /// </param>
    /// <returns></returns>
    public static string GetMethodJSON(string methodName, int methodId, Dictionary<string,string> parameters = null)
    {
        JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
        j.AddField("jsonrpc", "2.0");
        j.AddField("method", methodName);
        JSONObject paramsObj = new JSONObject(JSONObject.Type.OBJECT);
        foreach (KeyValuePair<string, string> entry in parameters)
        {
            paramsObj.AddField(entry.Key, entry.Value);
        }
        j.AddField("params", paramsObj);
        j.AddField("id", methodId);
        return j.Print();
    }

    /// <summary>
    /// Creates a JSONObject with the requirements needed to a succesful subscribe method
    /// </summary>
    /// <param name="auth">
    /// authorization token
    /// </param>
    /// <param name="methodId">
    /// Id number to the request response
    /// </param>
    /// <param name="streams">
    /// String with name of the stream needed to be suscribed
    /// </param>
    /// <returns>
    /// String to be sent through the WebSocket
    /// </returns>
    public static string GetSuscribtionJson(int methodId, string auth, string[] streams)
    {
        JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
        j.AddField("jsonrpc", "2.0");
        j.AddField("method", "subscribe");
        JSONObject paramsObj = new JSONObject(JSONObject.Type.OBJECT);

        paramsObj.AddField("_auth", auth/*EmotivConnection.Auth.Replace("\"", "")*/);
        JSONObject streamsArray = new JSONObject(JSONObject.Type.ARRAY);
        for (int i = 0; i < streams.Length; i++)
        {
            streamsArray.Add(streams[i]);
        }
        paramsObj.AddField("streams", streamsArray);

        j.AddField("params", paramsObj);
        j.AddField("id", methodId);
        return j.Print();
    }

    /// <summary>
    /// Gets the value of a field in a JSONObject
    /// </summary>
    /// <param name="jObj">
    /// JSONObject to be searched
    /// </param>
    /// <returns>
    /// String corresponding to the value of a requested field
    /// </returns>
    public static string GetFieldFromJSONObject(JSONObject jObj, string field)
    {
        string value = "";
        if (jObj)
        {

            JSONObject resultObj = jObj.GetField("result");
            if (resultObj)
            {
                JSONObject fieldObject = resultObj.GetField(field);
                value = fieldObject.ToString();
            }

        }
        return value;
    }
}
