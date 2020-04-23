using Unity.Entities;
using UnityEngine;

public static class QueryReferences
{
    public static void Query(ReferencedUnityObjects referencedUnityObjects)
    {
        foreach (var obj in referencedUnityObjects.Array)
        {
            
        }

        for (int i = 0; i < referencedUnityObjects.Array.Length; i++)
        {
            Debug.Log(i + " " + referencedUnityObjects.Array[i].name +" "+ referencedUnityObjects.Array[i].GetType());
        }
    }
}
