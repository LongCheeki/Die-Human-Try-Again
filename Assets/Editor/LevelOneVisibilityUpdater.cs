using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class LevelOneVisibilityUpdater
{
    public static string Apply()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(EditorApplication.isPlaying || scene.path!="Assets/Scenes/Level01_Castle.unity")throw new Exception("Open first level in edit mode.");
        foreach(var rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(rect!=null && (rect.name=="Boss health" || rect.name=="Victory screen"))Object.DestroyImmediate(rect.gameObject);
        var path="Assets/Level01/ClosedDoorMarker.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(material,path);}
        material.SetColor("_BaseColor",new Color(0.9f,0.12f,0.06f));
        var gates=Object.FindObjectsByType<RoomGate>(FindObjectsSortMode.None);
        foreach(var gate in gates)
        {
            if(gate.closedMarker==null)
            {
                gate.tallVisuals=gate.GetComponentsInChildren<Renderer>(true);
                var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name="Low closed-door marker";
                marker.transform.SetParent(gate.transform,false);
                marker.transform.localPosition=new Vector3(0,0.09f,0);
                marker.transform.localScale=new Vector3(2.9f,0.16f,0.3f);
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().sharedMaterial=gate.name.Contains("Golden")?AssetDatabase.LoadAssetAtPath<Material>("Assets/Level01/BossGateGold.mat"):material;
                gate.closedMarker=marker;
            }
            gate.SetOpen(gate.initiallyOpen,true);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Removed Boss UI; configured six doors with unobstructed combat visuals and unchanged blockers.";
    }
}
