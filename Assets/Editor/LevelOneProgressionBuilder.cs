using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static partial class LevelOneCrossBuilder
{
    public static string AddProgression()
    {
        if(EditorApplication.isPlaying)throw new Exception("Use edit mode.");
        var source=SceneManager.GetSceneByPath("Assets/Scenes/Test.unity");
        var target=SceneManager.GetSceneByPath("Assets/Scenes/Level01_Castle.unity");
        if(!source.isLoaded || !target.isLoaded)throw new Exception("Open Test and Level01 together.");
        SceneManager.SetActiveScene(target);
        var rooms=target.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<BattleRoom>(true)).OrderBy(r=>r.name).ToArray();
        var originalPickup=source.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<WeaponPickup>(true)).Single();
        var originalMimic=source.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MimicPickup>(true)).Single();
        var pickup=Object.Instantiate(originalPickup,rooms[0].transform);
        var mimic=Object.Instantiate(originalMimic,rooms[1].transform);
        var ui=target.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<WeaponChoiceUI>(true)).Single();
        var equipped=Object.Instantiate(pickup.gameObject);
        equipped.name="Great Sword";
        Object.DestroyImmediate(equipped.GetComponent<WeaponPickup>());
        foreach(var col in equipped.GetComponentsInChildren<Collider>())col.isTrigger=true;
        equipped.AddComponent<SwordHitbox>();
        equipped.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        var weapon=PrefabUtility.SaveAsPrefabAsset(equipped,"Assets/Level01/Prefabs/GreatSwordEquipped.prefab");
        Object.DestroyImmediate(equipped);
        pickup.transform.position=rooms[0].transform.position+new Vector3(2.8f,0.25f,2.8f);
        pickup.choiceUI=ui;pickup.weaponPrefab=weapon;pickup.name="Great Sword pickup";
        mimic.transform.position=rooms[1].transform.position+new Vector3(2.8f,0.4f,2.8f);
        mimic.name="Warrior form orb";
        var boss=target.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<BossEncounter>(true)).Single();
        layout=target.GetRootGameObjects().First(r=>r.name.StartsWith("LEVEL 01")).transform;
        var templates=target.GetRootGameObjects().First(r=>r.name=="Source templates (inactive)").transform;
        rail=templates.GetChild(1).gameObject;
        floorMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/LowpolyTexturesPack/Materials/Desert_Tiles.mat");
        var back=layout.GetComponentsInChildren<BoxCollider>().First(c=>c.name=="Low Test balustrade boundary" && Mathf.Abs(c.transform.TransformPoint(c.center).z-40)<0.01f && c.size.x>17);
        Object.DestroyImmediate(back.gameObject);
        Edge(new Vector3(-9,0,40),new Vector3(-1.5f,0,40));
        Edge(new Vector3(1.5f,0,40),new Vector3(9,0,40));
        Corridor(new Vector3(0,0,42),3,4,true);
        Edge(new Vector3(-1.5f,0,44),new Vector3(1.5f,0,44));
        var door=Object.Instantiate(boss.gate.gameObject,boss.transform);
        door.name="Next level exit door";
        door.transform.SetPositionAndRotation(new Vector3(0,-0.1f,40),Quaternion.identity);
        var exitGate=door.GetComponent<RoomGate>();exitGate.initiallyOpen=false;exitGate.SetOpen(false,true);
        var trigger=new GameObject("Next level destination");trigger.transform.SetParent(boss.transform);trigger.transform.position=new Vector3(0,1,42);
        var volume=trigger.AddComponent<BoxCollider>();volume.isTrigger=true;volume.size=new Vector3(2.8f,3,2.2f);
        var exit=trigger.AddComponent<LevelExit>();exit.encounter=boss;exit.gate=exitGate;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(target);
        EditorSceneManager.SaveScene(target);
        if(!source.isDirty)EditorSceneManager.CloseScene(source,true);
        Selection.activeGameObject=trigger;
        return "Copied Great Sword to south room and warrior orb to west room; Test unchanged; added locked Boss exit and destination setting.";
    }
}
