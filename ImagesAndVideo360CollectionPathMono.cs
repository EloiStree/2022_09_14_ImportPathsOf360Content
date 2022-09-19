using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImagesAndVideo360CollectionPathMono : MonoBehaviour
{
    public ImagesAndVideo360Collection m_collection;

    public void SetWith(ImagesAndVideo360Collection collection) {
        m_collection = collection;
    }

    public void GetRandom(out AbstractMono360WithPath content)
    {
        m_collection.GetAllIdPresent(out List<string> ids);
        Eloi.E_UnityRandomUtility.GetRandomOf( out string value, ids);
        Debug.Log("H" + string.Join("-", ids));
        FindFirst(value, out bool found, out content, out Type t);
        Debug.Log("H"+found+" " + value);
    }

    public void RefreshUniqueIdList() { m_collection.RefreshUniqueIdList(); }

    public void FindFirst(in string targetId, out bool found, out AbstractMono360WithPath content, out Type type)
    {
        m_collection.FindFirst(in targetId, out found, out content, out type);
    }
    public void FindFirst(in string targetId, out bool found, out AbstractMono360WithPath content)
    {
        m_collection.FindFirst(in targetId, out found, out content);
    }
    public void FindFirst(in string targetId, out bool found, out ImageMono360WithPath content)
    {
        m_collection.FindFirst(in targetId, out found, out content);
    }
    public void FindFirst(in string targetId, out bool found, out ImageDuo360WithPath content)
    {
        m_collection.FindFirst(in targetId, out found, out content);
    }
    public void FindFirst(in string targetId, out bool found, out VideoDuo360WithPath content)
    {
        m_collection.FindFirst(in targetId, out found, out content);
    }
    public void FindFirst(in string targetId, out bool found, out VideoMono360WithPath content)
    {
        m_collection.FindFirst(in targetId, out found, out content);
    }
}
