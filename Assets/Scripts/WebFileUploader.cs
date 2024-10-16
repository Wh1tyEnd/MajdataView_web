// https://habr.com/en/post/684936/
using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Helper component to get a file
public class WebFileUploader : MonoBehaviour
{
    private void Start()
    {
    		// We don't need to delete it on the new scene, because system is singletone
        DontDestroyOnLoad(gameObject);
    }
		
    // This method is called from JS via SendMessage
    void FileRequestCallback(string path)
    {
    		// Sending the received link back to the FileUploaderHelper
        WebFileUploaderHelper.SetResult(path);
    }
}

public static class WebFileUploaderHelper
{
    static WebFileUploader fileUploaderObject;
    static Action<string> pathCallback;

    static WebFileUploaderHelper()
    {
        string methodName = "FileRequestCallback"; // We will not use reflection, so as not to complicate things, hardcode :)
        string objectName = typeof(WebFileUploaderHelper).Name; // But not here
				
      	// Create a helper object for the FileUploader system
        var wrapperGameObject = new GameObject(objectName, typeof(WebFileUploader));
        fileUploaderObject = wrapperGameObject.GetComponent<WebFileUploader>();
				
      	// Initializing the JS part of the FileUploader system
        InitFileLoader(objectName, methodName);
    }

    /// <summary>
    /// Requests a file from the user.
    /// Should be called when the user clicks!
    /// </summary>
    /// <param name="callback">Will be called after the user selects a file, the Http path to the file is passed as a parameter</param>
    /// <param name="extensions">File extensions that can be selected, example: ".jpg, .jpeg, .png"</param>
    public static void RequestFile(Action<string> callback, string extensions = ".jpg, .jpeg, .png")
    {
        RequestUserFile(extensions);
        pathCallback = callback;
    }

    /// <summary>
    /// For internal use
    /// </summary>
    /// <param name="path">The path to the file</param>
    public static void SetResult(string path)
    {
        pathCallback.Invoke(path);
        Dispose();
    }

    public static void Dispose()
    {
        ResetFileLoader();
        pathCallback = null;
    }
		
  	// Below we declare external functions from our .jslib file
    [DllImport("__Internal")]
    private static extern void InitFileLoader(string objectName, string methodName);

    [DllImport("__Internal")]
    private static extern void RequestUserFile(string extensions);

    [DllImport("__Internal")]
    private static extern void ResetFileLoader();
}
