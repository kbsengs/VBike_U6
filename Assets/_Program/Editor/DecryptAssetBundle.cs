using UnityEngine; 
using UnityEditor;
using System.IO;
using System.Collections;

public class DecryptAssetBundle : EditorWindow
{

    private static string assetURL = "";
    private static string assetName = "";
    private static string _fileName = "";
    private static WWW request;

    private static bool run = false;
    private static IEnumerator en = null;

    private static bool isScene = false;

    [MenuItem("ZOIT/Decrypt Asset Bundle", true)]
    public static bool LoadBundleValidator()
    {
        assetName = AssetDatabase.GetAssetPath(Selection.activeObject);
        _fileName = Path.GetFileNameWithoutExtension(assetName);
        return Path.GetExtension(assetName) == ".unity3d";
    }

    [MenuItem("ZOIT/Decrypt Asset Bundle")]
    static void Init()
    {  
        DecryptAssetBundle window = EditorWindow.GetWindow(typeof(DecryptAssetBundle)) as DecryptAssetBundle;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 450, 80);
    }
    void OnGUI()
    {
        assetName = EditorGUILayout.TextField("Asset bundle URL: ", assetName);

        assetURL = string.Concat("file:///", Application.dataPath, "/../", assetName);

        GUILayout.BeginHorizontal();        
        if (GUILayout.Button("Decrypt"))
        {
            isScene = false;
            run = true;
        }
        if (GUILayout.Button("Scene"))
        {
            isScene = true;
            run = true;
        }
        if (GUILayout.Button("Abort"))
        {
            run = false;
        }
        GUILayout.EndHorizontal();
    }

    public void Update()
    {
        if (!run)
        {
            if (en != null)
                en = null;
            return;
        }
        if (en == null)
        {
            en = LoadAsset(assetURL);
        }
        if (!en.MoveNext())
        {
            EditorApplication.ExecuteMenuItem("Assets/Reimport");
            Close();
            Debug.Log("Finish!");
            run = false;
        }
    }

    private IEnumerator LoadAsset(string s)
    {
        Debug.Log("Loading " + s + " ...");
        
        yield return new WaitForSeconds(1.0f);

        request = new WWW(s);
        //yield return request;
        //while (request.progress < 1.0f )
        //{
        //    Debug.Log( request.progress);
        //    // avoid freezing the editor:
        //    yield return ""; // just wait.
        //}
        // I don't like the following line because it will freeze up 
        // your editor.  So I commented it out.  
        //yield return request;

        while (request.progress < 1.0f)
        {
            Debug.Log(request.progress);
            yield return new WaitForEndOfFrame();
        }

        if (request.error != null)
            Debug.LogError(request.error);

        //if (request.isDone)
        {
            if (isScene)
            {
                AssetBundle p = request.assetBundle;
                Debug.Log(_fileName);

                UnityEngine.SceneManagement.SceneManager.LoadScene(_fileName); // Unity6 Migration
                //AsyncOperation async = Application.LoadLevelAsync(_fileName);

                //while (async.progress < 1.0f)
                //{
                //    Debug.Log(async.progress);
                //    yield return new WaitForEndOfFrame();
                //}

                //EditorApplication.OpenScene(s);
                Debug.Log("open");

                //p.Unload(false);
                
                AssetDatabase.Refresh();
                Debug.Log("refresh");
                //EditorApplication.
            }
            else
            {   
                GameObject g = (GameObject)request.assetBundle.mainAsset as GameObject;
                Instantiate(g);
            }
        }

        //Close();
    }

}