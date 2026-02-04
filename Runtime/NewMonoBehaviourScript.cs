using UnityEditor;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string path = "Packages/com.xlh.xlhpackage/Cube.prefab";
        var so = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject obj = Instantiate(so);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
