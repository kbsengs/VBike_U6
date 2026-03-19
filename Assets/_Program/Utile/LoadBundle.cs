using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Unity6 Migration

public class LoadBundle : MonoBehaviour {

    static bool isLoad = false;
    static AssetBundle _assetBundle;
    static public IEnumerator DownLoadBundle(string filename)
    {
        if (_assetBundle != null) _assetBundle.Unload(true); 
        string fullName = "file://" + Application.dataPath + "/StreamingAssets/" + filename + ".unity3d";
        //Debug.Log(fullName);
        //Debug.Log("Start Download Bundle!!");
        Caching.ClearCache();

        WWW www = WWW.LoadFromCacheOrDownload(fullName, 1);        
        yield return www;
        //Debug.Log("Complete Download Bundle!!");

        if (www.isDone)
        {
            AsyncOperation async;
            async = SceneManager.LoadSceneAsync(filename); // Unity6 Migration
            //Debug.Log("Start Load Scene!!");
            yield return async;
            //Debug.Log("Complete Load Scene!!");
        }
    }

    static public IEnumerator DownLoadAdditiveBundle(string filename)
    {
        if (_assetBundle != null) _assetBundle.Unload(true);
        string fullName = "file://" + Application.dataPath + "/StreamingAssets/" + filename + ".unity3d";
        //Debug.Log(fullName);
        //Debug.Log("Start Download Bundle!!");
        Caching.ClearCache();

        WWW www = WWW.LoadFromCacheOrDownload(fullName, 1);
        yield return www;
        //Debug.Log("Complete Download Bundle!!");

        if (www.isDone)
        {
            AsyncOperation async;
            async = SceneManager.LoadSceneAsync(filename, LoadSceneMode.Additive); // Unity6 Migration
            //Debug.Log("Start Load Scene!!");
            yield return async;
            //Debug.Log("Complete Load Scene!!");
        }
    }

    static public void DeleteBundle()
    {
        //Debug.Log("Destroy Bundle");
        if( _assetBundle != null )
            _assetBundle.Unload(true); 
    }
}
