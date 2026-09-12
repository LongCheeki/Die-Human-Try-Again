using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class LevelOneBuilder
{
    private const string ScenePath = "Assets/Scenes/Level01_Castle.unity";
    private static GameObject railBack, railFront, pillar, wall;
    private static Material metal;

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode before rebuilding.");
        var source = SceneManager.GetActiveScene();
        if (source.path != "Assets/Scenes/Test.unity" || source.isDirty)
            throw new Exception("Open saved Test first; unsaved work will not be discarded.");
        Directory.CreateDirectory("Temp");
        if(File.Exists(ScenePath)) File.Copy(ScenePath,"Temp/Level01-before-rebuild.unity",true);
        if(!EditorSceneManager.SaveScene(source,ScenePath,true))throw new Exception("Scene copy failed.");
        var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        var roots=scene.GetRootGameObjects();
        var player=Object.FindFirstObjectByType<PlayerHealth>();
        var guard=Object.FindFirstObjectByType<EnemyAI>();
        var pickup=Object.FindFirstObjectByType<HealthPickup>();
        var floor=roots.First(g=>g.name=="Cube" && g.transform.localScale.x>60);
        railBack=Object.Instantiate(roots.First(g=>g.name=="Handrail_A"));
        railFront=Object.Instantiate(roots.First(g=>g.name=="Handrail_A (38)"));
        pillar=Object.Instantiate(roots.First(g=>g.name=="Pillar_A"));
        wall=Object.Instantiate(roots.First(g=>g.name=="Wall_C"));
        var enemyTemplate=Object.Instantiate(guard.gameObject);
        var healthTemplate=Object.Instantiate(pickup.gameObject);
        healthTemplate.transform.localScale=pickup.transform.lossyScale;
        var templates=new GameObject("Source templates (inactive)");
        foreach(var g in new[]{railBack,railFront,pillar,wall,enemyTemplate,healthTemplate})
        {g.transform.SetParent(templates.transform);g.SetActive(false);}
        templates.SetActive(false);
        var keep=new[]{"Player","Main Camera","Directional Light","Global Volume","Canvas","EventSystem","RespawnPoint"};
        foreach(var g in roots)if(g!=floor && !keep.Contains(g.name))Object.DestroyImmediate(g);
        floor.name="Test floor - original material and scale";
        enemyTemplate.name="Guard template";
        healthTemplate.name="Health template";
        var cam=Camera.main;
        var follow=cam.GetComponent<CameraFollow>();follow.target=player.transform;
        player.transform.position=new Vector3(-10,0.76f,-1.5f);
        cam.transform.position=player.transform.position+follow.offset;
        var rb=player.GetComponent<Rigidbody>();rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        var pc=player.GetComponent<CapsuleCollider>();pc.radius=Mathf.Max(0.3f,pc.radius);
        player.playerMovement=player.GetComponent<PlayerMovement>();player.playerMimic=player.GetComponent<PlayerMimic>();
        player.slimeAnimator=player.playerMimic.slimeAnimator;
        player.healthUI=Object.FindFirstObjectByType<PlayerHealthUI>();
        if(player.healthUI!=null)player.healthUI.playerHealth=player;
        var spawn=GameObject.Find("RespawnPoint")??new GameObject("RespawnPoint");
        spawn.transform.position=player.transform.position;player.respawnPoint=spawn.transform;
        var enemyCapsule=enemyTemplate.GetComponent<CapsuleCollider>();enemyCapsule.radius=Mathf.Max(0.3f,enemyCapsule.radius);
        enemyTemplate.GetComponent<Rigidbody>().collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;

        if(!AssetDatabase.IsValidFolder("Assets/Level01"))AssetDatabase.CreateFolder("Assets","Level01");
        metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/Level01/GateMetal.mat");
        if(metal==null){metal=new Material(Shader.Find("Universal Render Pipeline/Lit"));metal.color=new Color(0.20f,0.18f,0.15f);metal.SetFloat("_Metallic",0.45f);metal.SetFloat("_Smoothness",0.3f);AssetDatabase.CreateAsset(metal,"Assets/Level01/GateMetal.mat");}
        var layout=new GameObject("LEVEL 01 - Test-style castle rooms").transform;
        for(float x=-10.8f;x<50.5f;x+=2f)
        {
            Clone(railBack,layout,new Vector3(x,-0.1f,2.36f),"Rear stone balustrade");
            Clone(railFront,layout,new Vector3(x,-0.1f,-5.3f),"Low front balustrade");
        }
        for(float x=-9.66f;x<52f;x+=6.5f)Clone(pillar,layout,new Vector3(x,-0.3f,2.68f),"Test rear pillar");
        Block("Rear invisible boundary",layout,new Vector3(20,1.1f,2.65f),new Vector3(64,2.4f,0.4f));
        Block("Front invisible boundary",layout,new Vector3(20,1.1f,-5.55f),new Vector3(64,2.4f,0.4f));
        Partition(layout,-11,-5.3f,2.4f,"Start wall");
        Partition(layout,51,-5.3f,2.4f,"End wall");
        var names=new[]{"01 - Guardroom","02 - Barracks","03 - Armory","04 - Captain room"};
        int[] counts={1,2,2,3};BattleRoom previous=null;
        for(int i=0;i<4;i++)
        {
            float cx=-2+i*14;
            var root=new GameObject(names[i]);root.transform.SetParent(layout);root.transform.position=new Vector3(cx,0,-1.5f);
            var room=root.AddComponent<BattleRoom>();room.previousRoom=previous;room.enemyTemplate=enemyTemplate;
            var area=root.AddComponent<BoxCollider>();area.isTrigger=true;area.center=new Vector3(0,1,0);area.size=new Vector3(8.5f,4,6.2f);
            room.entrance=Gate(layout,cx-6,true,names[i]+" Entrance");room.exit=Gate(layout,cx+6,false,names[i]+" Exit");
            var checkpoint=new GameObject("Checkpoint before entrance").transform;checkpoint.SetParent(root.transform);checkpoint.position=new Vector3(cx-7,0.76f,-1.5f);room.checkpoint=checkpoint;
            room.enemies=new EnemyHealth[counts[i]];
            for(int n=0;n<counts[i];n++)
            {
                var e=Object.Instantiate(enemyTemplate,root.transform);e.name="Guard "+(n+1);
                e.transform.position=new Vector3(cx+1.7f+(n%2)*1.2f,0.76f,-1.5f+(n-(counts[i]-1)*0.5f)*1.8f);
                e.transform.rotation=Quaternion.Euler(0,270,0);
                e.GetComponent<EnemyAI>().player=player.transform;e.GetComponent<EnemyAI>().detectionRange=40;
                var hp=e.GetComponent<EnemyHealth>();hp.maxHealth=i==3&&n==1?4:3;room.enemies[n]=hp;e.SetActive(true);
            }
            if(i==1||i==3)
            {
                var reward=Object.Instantiate(healthTemplate,root.transform);reward.name="Clear reward - health";
                reward.transform.position=new Vector3(cx+3.5f,0.4f,0.8f);reward.SetActive(false);room.reward=reward;
            }
            previous=room;
        }
        var buildScenes=EditorBuildSettings.scenes.ToList();if(!buildScenes.Any(s=>s.path==ScenePath))buildScenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=buildScenes.ToArray();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.LookAt(new Vector3(20,0,-1.5f),Quaternion.Euler(65,0,0),34,true);
        return "Rebuilt using Test camera, lighting, original floor material, rails, pillars and walls. Four horizontal rooms / eight guards / eight gates.";
    }
    private static GameObject Clone(GameObject source,Transform parent,Vector3 pos,string name)
    {var g=Object.Instantiate(source,parent);g.name=name;g.transform.position=pos;g.SetActive(true);return g;}
    private static void Block(string name,Transform parent,Vector3 pos,Vector3 size)
    {var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=pos;g.AddComponent<BoxCollider>().size=size;}
    private static void Partition(Transform parent,float x,float z0,float z1,string name)
    {
        var wrapper=new GameObject(name).transform;wrapper.SetParent(parent);wrapper.position=Vector3.zero;
        var g=Clone(wall,wrapper,Vector3.zero,"Test wall module");var render=g.GetComponentsInChildren<Renderer>();
        var b=render[0].bounds;foreach(var r in render)b.Encapsulate(r.bounds);
        g.transform.position-=b.center;
        wrapper.localScale=new Vector3(0.75f/b.size.x,0.94f/b.size.y,(z1-z0)/b.size.z);
        wrapper.position=new Vector3(x,0.37f,(z0+z1)/2);
        Block(name+" collision",parent,new Vector3(x,1.2f,(z0+z1)/2),new Vector3(0.75f,2.6f,z1-z0));
    }
    private static RoomGate Gate(Transform parent,float x,bool open,string name)
    {
        Partition(parent,x,-5.3f,-2.95f,name+" front wall");Partition(parent,x,-0.05f,2.4f,name+" rear wall");
        var root=new GameObject(name);root.transform.SetParent(parent);root.transform.position=new Vector3(x,-0.1f,-1.5f);root.transform.rotation=Quaternion.Euler(0,90,0);
        var arch=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DungeonModularPack/Prefabs/Arch_A.prefab"),root.transform);
        arch.transform.localPosition=Vector3.zero;arch.transform.localRotation=Quaternion.identity;arch.transform.localScale=new Vector3(1.5f,0.85f,1);
        foreach(var c in arch.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
        var gate=root.AddComponent<RoomGate>();gate.initiallyOpen=open;gate.hideWhenOpen=true;
        var blocker=root.AddComponent<BoxCollider>();blocker.center=new Vector3(0,1.2f,0);blocker.size=new Vector3(2.95f,2.6f,0.6f);gate.blocker=blocker;
        var bars=new GameObject("Iron gate").transform;bars.SetParent(root.transform,false);gate.bars=bars;
        for(int n=0;n<8;n++)Bar(bars,new Vector3(-1.32f+n*0.377f,1.2f,0),new Vector3(0.07f,2.4f,0.10f));
        for(int n=0;n<3;n++)Bar(bars,new Vector3(0,0.3f+n*0.85f,0),new Vector3(2.9f,0.09f,0.12f));
        gate.SetOpen(open,true);return gate;
    }
    private static void Bar(Transform parent,Vector3 pos,Vector3 size)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Ironwork";g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=metal;Object.DestroyImmediate(g.GetComponent<Collider>());}
}
