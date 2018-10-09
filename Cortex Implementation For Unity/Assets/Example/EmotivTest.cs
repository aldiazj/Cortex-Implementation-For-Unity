using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotivTest : MonoBehaviour
{
    // Constants

    const string EMOTIV_URL = "wss://emotivcortex.com:54321";
    const string CLIENT_ID = "DT0fkgceOzKZgjuoBEyNYkHVAGVPJESnlsBT23y6";
    const string CLIENT_SECRET = "5XMdQKCJEBz9IGbNBBJPYwgGmzKx4Vy55WQsMbhajvaB5XIQG1XGEGP5Cg2Ou893aDHtMWbsbjMyMI7JK5PDelmDsD59p48e3XiDbpDmawpGIBXPAPMiRpKCwVD6Dbl6";

    // Private fields

    static WebSocket w;

    // Use this for initialization
    public void Init ()
    {
        StartCoroutine(Initialize());	
	}

    private IEnumerator Initialize()
    {
        w = new WebSocket(new Uri(EMOTIV_URL));
        yield return StartCoroutine(w.Connect());
    }
}
