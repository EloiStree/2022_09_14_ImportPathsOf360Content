using Eloi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class FetchAllFilesInFoldersWithFilterMono : MonoBehaviour
{
    public FoldersTo360ContentToImport m_toImport;

    public string[] extensions = new string[] { ".mp4", ".jpg", ".jpeg", ".png" };

    public string m_dataPathFolder;
    public string m_persistancePathFolder;

    public string[] filesFound;
    public string[] filesFoundFiltered;

    [System.Serializable]
    public class UnityPathsEvent : UnityEvent<string[]>{}
    public UnityPathsEvent m_onRefresh;

    public void GetFilesFound( out string [] paths) { paths = filesFoundFiltered; }


    [ContextMenu("Refresh")]
    public void Refresh()
    {

        List<string> paths = new List<string>();
        for (int i = 0; i < m_toImport.m_absolutePaths.Count; i++)
        {
            try {
                string absolutPath = m_toImport.m_absolutePaths[i].GetPath();
                if (absolutPath.IndexOf(":") <0 ) {
                      Eloi.E_FilePathUnityUtility.TrimAtStartSlashAndBackSlashIfThereAre(absolutPath, out string dd);
                      absolutPath = AndroidRootPathWithJava() + dd;
                }
                Directory.CreateDirectory(absolutPath);
                string[] p = Directory.GetFiles(absolutPath,"*", SearchOption.AllDirectories);
                paths.AddRange(p);
            }
            catch (Exception e) { }

        }

        string relativePath = Application.dataPath;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {

            relativePath += "/..";
        }

        if (Application.platform == RuntimePlatform.Android)
            relativePath = AndroidPersistancePathWithJava();
        MetaAbsolutePathDirectory rd = new MetaAbsolutePathDirectory(relativePath);

        m_dataPathFolder = rd.GetPath();
        for (int i = 0; i < m_toImport.m_dataRelativePaths.Count; i++)
        {
            IMetaAbsolutePathDirectoryGet dir = E_FileAndFolderUtility.Combine(rd, m_toImport.m_dataRelativePaths[i]);
            Directory.CreateDirectory(dir.GetPath());
            string[] p = Directory.GetFiles(dir.GetPath(), "*", SearchOption.AllDirectories);
            paths.AddRange(p);
        }


        if (Application.platform == RuntimePlatform.Android)
            relativePath = AndroidPersistancePathWithJava();
        else
            relativePath = Application.persistentDataPath;
        MetaAbsolutePathDirectory pd = new MetaAbsolutePathDirectory(relativePath);
        
        m_persistancePathFolder = pd.GetPath();
        for (int i = 0; i < m_toImport.m_persistanceRelativePaths.Count; i++)
        {
            IMetaAbsolutePathDirectoryGet dir = E_FileAndFolderUtility.Combine(pd, m_toImport.m_dataRelativePaths[i]);
            Directory.CreateDirectory(dir.GetPath());
            string[] p = Directory.GetFiles(dir.GetPath(), "*", SearchOption.AllDirectories);
            paths.AddRange(p);
        }
        filesFound = paths.ToArray();

        for (int i = paths.Count - 1; i >= 0; i--)
        {
            bool containExt = false;
            for (int y = 0; y < extensions.Length; y++)
            {
                if (paths.Count > i)
                {

                    if (E_StringUtility.EndWith(paths[i].ToLower(), extensions[y].ToLower()))
                    {
                        containExt = true;
                        //continue;
                    }
                }
            }
            if (!containExt)
                paths.RemoveAt(i);
        }
        if (paths.Count > 0)
            filesFoundFiltered = paths.ToArray();
        else filesFoundFiltered = new string[0];

        m_onRefresh.Invoke(filesFoundFiltered);
    }



    public static string AndroidRootPathWithJava()
    {
        string result = "";
        //https://answers.unity.com/questions/946029/get-sdcard-root-path.html
        AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
        IntPtr getExternalStorageDirectoryMethod = AndroidJNI.GetStaticMethodID(jc.GetRawClass(), "getExternalStorageDirectory", "()Ljava/io/File;");
        IntPtr file = AndroidJNI.CallStaticObjectMethod(jc.GetRawClass(), getExternalStorageDirectoryMethod, new jvalue[] { });
        IntPtr getPathMethod = AndroidJNI.GetMethodID(AndroidJNI.GetObjectClass(file), "getPath", "()Ljava/lang/String;");
        IntPtr path = AndroidJNI.CallObjectMethod(file, getPathMethod, new jvalue[] { });
        result = AndroidJNI.GetStringUTFChars(path);
        AndroidJNI.DeleteLocalRef(file);
        AndroidJNI.DeleteLocalRef(path);
        Debug.Log("m_sdCardPath = " + result);

        return result;
    }


    private static string AndroidPersistancePathWithJava()
    {//http://anja-haumann.de/unity-how-to-save-on-sd-card/
        using (AndroidJavaClass unityPlayer =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject context =
                   unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // Get all available external file directories (emulated and sdCards)
                AndroidJavaObject[] externalFilesDirectories =
                                    context.Call<AndroidJavaObject[]>
                                    ("getExternalFilesDirs", (object)null);

                AndroidJavaObject emulated = null;
                AndroidJavaObject sdCard = null;

                for (int i = 0; i < externalFilesDirectories.Length; i++)
                {
                    AndroidJavaObject directory = externalFilesDirectories[i];
                    using (AndroidJavaClass environment =
                           new AndroidJavaClass("android.os.Environment"))
                    {
                        // Check which one is the emulated and which the sdCard.
                        bool isRemovable = environment.CallStatic<bool>
                                          ("isExternalStorageRemovable", directory);
                        bool isEmulated = environment.CallStatic<bool>
                                          ("isExternalStorageEmulated", directory);
                        if (isEmulated)
                            emulated = directory;
                        else if (isRemovable && isEmulated == false)
                            sdCard = directory;
                    }
                }
                // Return the sdCard if available
                if (sdCard != null)
                    return sdCard.Call<string>("getAbsolutePath");
                else
                    return emulated.Call<string>("getAbsolutePath");
            }
        }
    }
    }
