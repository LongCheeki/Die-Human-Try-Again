using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class SpikeTrapModelUpdater
{
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play mode first.");
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/Level01_Castle.unity")throw new Exception("Open Level01 first.");
        EditorSceneManager.SaveScene(scene);
        System.IO.File.Copy(scene.path,"Temp/Level01-before-spike-model.unity",true);
        const string path="Assets/Level01/Quaternius/Hazard_SpikeTrap.fbx";
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c=>c.name=="SpikeTrap_Activate");
        var traps=Object.FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None);
        foreach(var trap in traps)
        {
            foreach(var child in trap.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
            trap.name="Floor spikes - immediate hit then 1 HP per second";
            var model=(GameObject)PrefabUtility.InstantiatePrefab(source,trap.transform);
            model.name="Quaternius spike mechanism";
            clip.SampleAnimation(model,0.125f);
            foreach(var animator in model.GetComponentsInChildren<Animator>())Object.DestroyImmediate(animator);
            foreach(var r in model.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials=r.sharedMaterials.Select(original=>{
                    string matPath="Assets/Level01/Quaternius/"+original.name+".mat";
                    var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=original.color;mat.SetFloat("_Smoothness",0.25f);AssetDatabase.CreateAsset(mat,matPath);}
                    return mat;
                }).ToArray();
            }
            var bounds=Measure(model);
            model.transform.localScale*=1.7f/Mathf.Max(bounds.size.x,bounds.size.z);
            bounds=Measure(model);
            model.transform.position+=trap.transform.position-new Vector3(bounds.center.x,bounds.min.y+0.28f,bounds.center.z);
            var area=trap.GetComponent<BoxCollider>();area.center=new Vector3(0,0.35f,0);area.size=new Vector3(1.7f,0.9f,1.7f);
            trap.damageInterval=1;trap.damagePerTick=1;
        }
        PrefabUtility.SaveAsPrefabAsset(traps[0].gameObject,"Assets/Level01/Prefabs/FloorSpikes.prefab");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Replaced "+traps.Length+" spike models and updated reusable prefab.";
    }
    private static Bounds Measure(GameObject model)
    {
        var renderers=model.GetComponentsInChildren<Renderer>();
        var bounds=renderers[0].bounds;
        foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
}
