using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class LevelOneClearRewardsBuilder
{
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Use edit mode.");
        var test=SceneManager.GetSceneByPath("Assets/Scenes/Test.unity");
        if(!test.isLoaded)test=EditorSceneManager.OpenScene("Assets/Scenes/Test.unity",OpenSceneMode.Additive);
        var level=SceneManager.GetSceneByPath("Assets/Scenes/Level01_Castle.unity");
        if(!level.isLoaded)level=EditorSceneManager.OpenScene("Assets/Scenes/Level01_Castle.unity",OpenSceneMode.Additive);
        var weapon=level.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<WeaponPickup>(true)).Single();
        var orb=level.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MimicPickup>(true)).Single();
        weapon.GetComponentInParent<BattleRoom>().clearRewards=new[]{weapon.gameObject};
        var west=orb.GetComponentInParent<BattleRoom>();west.clearRewards=new[]{orb.gameObject};
        orb.transform.position=west.transform.position+new Vector3(0,0.4f,3.2f);
        weapon.gameObject.SetActive(false);orb.gameObject.SetActive(false);
        if(!test.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<LevelExit>(true)).Any())
        {
            var source=weapon.GetComponentInParent<BattleRoom>().entrance;
            var door=Object.Instantiate(source.gameObject);
            SceneManager.MoveGameObjectToScene(door,test);
            door.name="Exit to Level 01";
            door.transform.SetPositionAndRotation(new Vector3(50.1f,-0.1f,-1.5f),Quaternion.Euler(0,90,0));
            var gate=door.GetComponent<RoomGate>();gate.initiallyOpen=false;gate.SetOpen(false,true);
            var destination=new GameObject("Level 01 portal trigger");SceneManager.MoveGameObjectToScene(destination,test);
            destination.transform.SetParent(door.transform,true);destination.transform.position=new Vector3(50.85f,1,-1.5f);
            destination.transform.rotation=Quaternion.identity;
            var area=destination.AddComponent<BoxCollider>();area.isTrigger=true;area.size=new Vector3(0.7f,3,2.7f);
            var exit=destination.AddComponent<LevelExit>();exit.gate=gate;exit.requireSceneClear=true;exit.destinationScene="Level01_Castle";
        }
        EditorSceneManager.MarkSceneDirty(level);EditorSceneManager.SaveScene(level);
        EditorSceneManager.MarkSceneDirty(test);EditorSceneManager.SaveScene(test);
        EditorSceneManager.CloseScene(level,true);SceneManager.SetActiveScene(test);
        Selection.activeGameObject=test.GetRootGameObjects().First(r=>r.name=="Exit to Level 01");
        return "Test exit configured at far right; Level01 weapon and separated warrior orb are room-clear rewards.";
    }
}
