using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Object = UnityEngine.Object;

public static partial class LevelOneCrossBuilder
{
    public static string BuildBoss()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/Level01_Castle.unity")
            throw new Exception("Open Level01 in edit mode first.");
        if (Object.FindFirstObjectByType<BossEncounter>() != null) return "Boss room already exists.";
        EditorSceneManager.SaveScene(scene);
        File.Copy(scene.path, "Temp/Level01-before-boss.unity", true);
        layout = scene.GetRootGameObjects().First(x => x.name.StartsWith("LEVEL 01")).transform;
        var templates = scene.GetRootGameObjects().First(x => x.name == "Source templates (inactive)").transform;
        rail = templates.GetChild(1).gameObject;
        pillar = templates.GetChild(2).gameObject;
        guard = Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).First(x=>x.name.StartsWith("01")).enemies[0].gameObject;
        floorMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowpolyTexturesPack/Materials/Desert_Tiles.mat");
        var north = Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).First(x => x.name.StartsWith("04"));
        var back = layout.GetComponentsInChildren<BoxCollider>().First(x => !x.isTrigger && x.name == "Low Test balustrade boundary" && Mathf.Abs(x.transform.TransformPoint(x.center).z - 19f) < 0.01f && x.size.x > 11f);
        Object.DestroyImmediate(back.gameObject);
        Edge(new Vector3(-6,0,19),new Vector3(-1.5f,0,19));
        Edge(new Vector3(1.5f,0,19),new Vector3(6,0,19));
        Corridor(new Vector3(0,0,21.5f),3,5,true);
        var onward = Object.Instantiate(north.entrance.gameObject,north.transform);
        onward.name = "Onward door to Boss passage";
        onward.transform.SetPositionAndRotation(new Vector3(0,-0.1f,19),Quaternion.identity);
        north.exit = onward.GetComponent<RoomGate>();
        north.exit.initiallyOpen = false;
        north.exit.SetOpen(false,true);

        var center = new Vector3(0,0,32);
        var roomObject = new GameObject("05 - Boss room");
        roomObject.transform.SetParent(layout);
        roomObject.transform.position = center;
        Floor("Boss room floor - 18 x 16",center,18,16);
        for (int side=0;side<4;side++) Perimeter(center,18,16,side,side==2);
        foreach (float x in new[]{-8.7f,8.7f})
        {
            var p=Object.Instantiate(pillar,roomObject.transform);
            p.name="Test stone pillar";
            p.transform.position=center+new Vector3(x,-0.3f,7.8f);
            p.SetActive(true);
        }
        foreach (var entry in new[]{new {asset="barrel",point=new Vector3(-6,-0.1f,4)},new {asset="rocks",point=new Vector3(6,-0.1f,-4)}})
        {
            var prop=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Level01/Prefabs/"+entry.asset+".prefab"),roomObject.transform);
            prop.name=entry.asset+" obstacle";
            prop.transform.SetPositionAndRotation(center+entry.point,Quaternion.identity);
        }
        var goldPath="Assets/Level01/BossGateGold.mat";
        var gold=AssetDatabase.LoadAssetAtPath<Material>(goldPath);
        if(gold==null){gold=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(gold,goldPath);}
        gold.SetColor("_BaseColor",new Color(1f,0.66f,0.12f));
        gold.SetFloat("_Metallic",0.7f);gold.SetFloat("_Smoothness",0.45f);
        var golden=Object.Instantiate(north.entrance.gameObject,roomObject.transform);
        golden.name="Golden Boss entrance";
        golden.transform.SetPositionAndRotation(new Vector3(0,-0.1f,24),Quaternion.identity);
        foreach(var r in golden.GetComponentsInChildren<Renderer>(true))r.sharedMaterials=r.sharedMaterials.Select(_=>gold).ToArray();
        var encounter=roomObject.AddComponent<BossEncounter>();
        encounter.gate=golden.GetComponent<RoomGate>();
        encounter.gate.initiallyOpen=true;encounter.gate.SetOpen(true,true);
        encounter.previousRoom=north;
        var minionSource=Object.Instantiate(guard);
        minionSource.name="Boss reinforcement";minionSource.SetActive(false);
        encounter.minionTemplate=PrefabUtility.SaveAsPrefabAsset(minionSource,"Assets/Level01/Prefabs/BossReinforcement.prefab");
        Object.DestroyImmediate(minionSource);
        var area=roomObject.GetComponent<BoxCollider>();area.isTrigger=true;area.center=Vector3.up;
        area.size=new Vector3(15,5,13);
        var checkpoint=new GameObject("Boss checkpoint outside golden door").transform;
        checkpoint.SetParent(roomObject.transform);checkpoint.position=new Vector3(0,0.76f,22.5f);
        encounter.checkpoint=checkpoint;
        var boss=Object.Instantiate(guard,roomObject.transform);
        boss.name="Castle Warden - Boss";
        boss.transform.SetPositionAndRotation(new Vector3(0,0.75f,35),Quaternion.Euler(0,180,0));
        boss.transform.localScale=guard.transform.localScale*1.7f;
        var hp=boss.GetComponent<EnemyHealth>();hp.maxHealth=guard.GetComponent<EnemyHealth>().maxHealth*5;
        var ai=boss.GetComponent<EnemyAI>();ai.moveSpeed=guard.GetComponent<EnemyAI>().moveSpeed*0.6f;
        ai.attackRange=2.3f;ai.detectionRange=40;
        if(ai.visual!=null)ai.visual.localPosition+=Vector3.down*0.245f;
        ai.player=Object.FindFirstObjectByType<PlayerHealth>().transform;
        boss.SetActive(false);
        encounter.bossTemplate=PrefabUtility.SaveAsPrefabAsset(boss,"Assets/Level01/Prefabs/CastleWarden.prefab");
        encounter.boss=hp;
        boss.SetActive(true);
        encounter.circlePrefab=CreateWarningCircle();
        CreateBossEnding(encounter);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Boss room saved: gold entrance, 18 x 16 floor, 15 HP Warden, two obstacles, no traps.";
    }

    private static BossWarningCircle CreateWarningCircle()
    {
        var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);
        material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite",0);material.SetInt("_Cull",0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
        material.SetOverrideTag("RenderType","Transparent");
        material.SetColor("_BaseColor",new Color(1,0.45f,0.45f,0.45f));
        AssetDatabase.CreateAsset(material,"Assets/Level01/BossWarningRed.mat");
        const int segments=96;
        var vertices=new Vector3[segments+1];var triangles=new int[segments*3];
        for(int i=0;i<segments;i++)
        {
            float angle=i*Mathf.PI*2/segments;
            vertices[i+1]=new Vector3(Mathf.Cos(angle)*1.7f,0,Mathf.Sin(angle)*1.7f);
            triangles[i*3]=0;triangles[i*3+1]=(i+1)%segments+1;triangles[i*3+2]=i+1;
        }
        var mesh=new Mesh{name="Boss warning disc"};mesh.vertices=vertices;mesh.triangles=triangles;
        mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,"Assets/Level01/BossWarningDisc.asset");
        var g=new GameObject("Boss warning circle");g.AddComponent<MeshFilter>().sharedMesh=mesh;
        var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;
        r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
        var circle=g.AddComponent<BossWarningCircle>();circle.disc=r;
        var prefab=PrefabUtility.SaveAsPrefabAsset(g,"Assets/Level01/Prefabs/BossWarningCircle.prefab");
        Object.DestroyImmediate(g);
        return prefab.GetComponent<BossWarningCircle>();
    }

    private static void CreateBossEnding(BossEncounter encounter)
    {
        var manager=new GameObject("Level ending");
        manager.transform.SetParent(encounter.transform,false);
        encounter.victory=manager.AddComponent<LevelVictory>();
    }
}