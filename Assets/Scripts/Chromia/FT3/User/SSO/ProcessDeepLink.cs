using System;
using UnityEngine;

public class ProcessDeepLink : MonoBehaviour
{
    public static ProcessDeepLink Instance { get; private set; }
    public string DeepLinkURL;
    public string Payload;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            /*
            if (!String.IsNullOrEmpty(Application.absoluteURL))
            {
                onDeepLinkActivated(Application.absoluteURL);
            }
            else 
            */
            DeepLinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void onDeepLinkActivated(string url)
    {
        DeepLinkURL = url;
        Payload = url.Split("?"[0])[1];
    }
}
