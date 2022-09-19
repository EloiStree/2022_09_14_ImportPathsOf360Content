using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldersTo360DoubleFilesContentMono : MonoBehaviour
{
    public FetchAllFilesInFoldersWithFilterMono m_fetchFiles;

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        m_fetchFiles.Refresh();
        m_fetchFiles.GetFilesFound(out string[] paths);

    }
}
