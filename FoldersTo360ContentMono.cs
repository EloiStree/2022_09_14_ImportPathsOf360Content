using Eloi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class FoldersTo360ContentMono : MonoBehaviour
{
    public DefaultImportContent360 m_importer = new DefaultImportContent360();
    public FoldersTo360Content m_contentFound;
    public UnityEvent m_onChanged;

    public void Append(string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            Content360Path c = new Content360Path(paths[i]);
            m_contentFound.m_foundPath.Add(c);
            m_importer.Convert(c, out bool found, out Content360PathWithInfo info);
            if (found)
            {
                m_contentFound.m_fount360Content.Add(info);
            }
        }
        m_onChanged.Invoke();
    }
    public void Set(string[] paths)     
    {
        m_contentFound.Clear();
        for (int i = 0; i < paths.Length; i++)
        {
            Content360Path c = new Content360Path(paths[i]);
            m_contentFound.m_foundPath.Add(c);
            m_importer.Convert(c, out bool found, out Content360PathWithInfo info);
            if (found)
            {
                m_contentFound.m_fount360Content.Add(info);
            }
        }
        m_onChanged.Invoke();
    }
}

[System.Serializable]
public class FoldersTo360ContentToImport
{
    public List<MetaAbsolutePathDirectory> m_absolutePaths= new List<MetaAbsolutePathDirectory>();
    public List<MetaRelativePathDirectory> m_dataRelativePaths = new List<MetaRelativePathDirectory>();
    public List<MetaRelativePathDirectory> m_persistanceRelativePaths = new List<MetaRelativePathDirectory>();
}

[System.Serializable]
public class FoldersTo360Content
{
    public List<Content360Path> m_foundPath = new List<Content360Path>();
    public List<Content360PathWithInfo> m_fount360Content = new List<Content360PathWithInfo>();

    internal void Clear()
    {
        m_foundPath.Clear();
        m_fount360Content.Clear();
    }
}

[System.Serializable]
public class Content360Path
{
    public MetaAbsolutePathFile m_path;
    public Content360Path(string path) {
        m_path = new MetaAbsolutePathFile(path);
    }
}
[System.Serializable]
public class Content360PathWithInfo
{
    public Content360Path m_source;
    public string m_fileIdUsed;
    public TextureStereoType360 m_textureType;
    public TextureType360 m_contentType;
    public int m_widthPxInPath;
    public int m_heightPxInPath;
    //public long m_fileSize;
    //public int m_widthPxInFileInfo;
    //public int m_heightPxInFileInfo;
}


public abstract class AbstractImportContent360
{

    public abstract void Convert(in Content360Path path, out bool converted, out Content360PathWithInfo infoFound);
  
}
public class DefaultImportContent360: AbstractImportContent360
{
    public override void Convert(in Content360Path path, out bool converted, out Content360PathWithInfo infoFound)
    {
        string p = path.m_path.GetPath();
        infoFound = new Content360PathWithInfo();
        Eloi.E_FileAndFolderUtility.GetFileInfoFromPath(path.m_path, out IMetaFileNameWithExtensionGet file);
        file.GetExtensionWithoutDot(out string ext);
        ext = ext.ToLower();
        file.GetFileNameWithoutExtension(out string fileName);
        string[] tokens = fileName.Split('_');
        if (tokens.Length <= 0 || string.IsNullOrWhiteSpace(tokens[0])) {
            converted = false;
            return;
        }
        infoFound.m_contentType = ext == "mp4" || ext=="avi" ? TextureType360.Video : TextureType360.Image;
        infoFound.m_fileIdUsed = tokens[0];
        for (int i = 1; i < tokens.Length; i++)
        {
            if (IsResolutionToken(tokens[i], out int width, out int height))
            {
                infoFound.m_heightPxInPath = height;
                infoFound.m_widthPxInPath = width;
            }
            else if (IsTypeEnum(tokens[i], out TextureStereoType360 stereoType))
            {
                infoFound.m_textureType = stereoType;
            }
        }
        infoFound.m_source= path;
       Eloi.E_CodeTag.ToCodeLater.Info("To code later but I don't have internet to find the corresponding code");
        FileAttributes attributes = File.GetAttributes(p);
        converted = true;


    }

    private bool IsTypeEnum(string token, out TextureStereoType360 stereoType)
    {
        token = token.ToLower().Trim();
        if (token == "l") { stereoType = TextureStereoType360.SplitLeft; }
        else if (token == "left") { stereoType = TextureStereoType360.SplitLeft; }

        else if (token == "r") { stereoType = TextureStereoType360.SplitRight; }
        else if (token == "right") { stereoType = TextureStereoType360.SplitRight; }

        else if (token == "m") { stereoType = TextureStereoType360.Mono; }
        else if (token == "mono") { stereoType = TextureStereoType360.Mono; }

        else if (token == "lr") { stereoType = TextureStereoType360.HorizontalLeftRight; }
        else if (token == "rl") { stereoType = TextureStereoType360.HorizontalRightLeft; }

        else if (token == "tldr") { stereoType = TextureStereoType360.Vertical_TopLeft_DownRight; }
        else if (token == "trdl") { stereoType = TextureStereoType360.Vertical_TopRight_DownLeft; }
        else if (token == "dltr") { stereoType = TextureStereoType360.Vertical_TopRight_DownLeft; }
        else if (token == "drtl") { stereoType = TextureStereoType360.Vertical_TopLeft_DownRight; }

        else { 
            stereoType = TextureStereoType360.Mono;
            return false;
        }
        return true;
    }
    private bool IsResolutionToken(string token, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (token.ToLower().IndexOf("x")>-1)
        {
            string [] t = token.ToLower().Split('x');
            if (int.TryParse(t[0], out width) && int.TryParse(t[1], out height))
            {
                return true;
            }
            else return false;
        }
        else return false;
    }
}


