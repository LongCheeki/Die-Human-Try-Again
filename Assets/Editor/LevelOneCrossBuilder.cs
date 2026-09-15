using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static partial class LevelOneCrossBuilder
{
    private static Transform layout;
    private static GameObject rail, pillar, guard, health, gateTemplate;
    private static Material floorMaterial, propMaterial, metal;
    private const string Props = "Assets/Level01/KenneyMiniDungeon/";

    public static string Build()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/Level01_Castle.unity")
            throw new Exception("Open Level01 in edit mode first.");
        EditorSceneManager.SaveScene(scene);
        File.Copy(scene.path, "Temp/Level01-before-cross.unity", true);
        var roots = scene.GetRootGameObjects();
        var templates = roots.First(x => x.name == "Source templates (inactive)").transform;
        rail = templates.GetChild(1).gameObject; pillar = templates.GetChild(2).gameObject;
        guard = templates.GetChild(4).gameObject; health = templates.GetChild(5).gameObject;
        gateTemplate = Object.Instantiate(Object.FindObjectsByType<RoomGate>(FindObjectsSortMode.None)[0].gameObject);
        gateTemplate.SetActive(false);
        floorMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowpolyTexturesPack/Materials/Desert_Tiles.mat");
        metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Level01/GateMetal.mat");
        propMaterial = AssetDatabase.LoadAssetAtPath<Material>(Props + "DungeonProps.mat");
        if (propMaterial == null)
        {
            propMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            propMaterial.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Props + "colormap.png"));
            propMaterial.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(propMaterial, Props + "DungeonProps.mat");
        }
        foreach (var root in roots)
            if (root.name.StartsWith("LEVEL 01") || root.name.StartsWith("Test floor")) Object.DestroyImmediate(root);
        layout = new GameObject("LEVEL 01 - Cross castle").transform;
        Floor("Central safe hall", Vector3.zero, 8, 8);
        for (int side = 0; side < 4; side++) Perimeter(Vector3.zero, 8, 8, side, true);
        Corridor(new Vector3(0,0,6.5f),3,5,true);
        Corridor(new Vector3(0,0,-6.5f),3,5,true);
        Corridor(new Vector3(7,0,0),6,3,false);
        Corridor(new Vector3(-7,0,0),6,3,false);

        var centers = new[] { new Vector3(0,0,-14), new Vector3(-16,0,0), new Vector3(16,0,0), new Vector3(0,0,14) };
        var names = new[] { "01 - South Guardroom", "02 - West Barracks", "03 - East Armory", "04 - North Captain room" };
        int[] entrySides = {0,1,3,2};
        int[] counts = {1,2,2,3};
        var player = Object.FindFirstObjectByType<PlayerHealth>();
        for (int i=0;i<4;i++)
        {
            Vector3 center=centers[i];
            var root=new GameObject(names[i]);root.transform.SetParent(layout);root.transform.position=center;
            Floor(names[i]+" floor",center,12,10);
            for(int side=0;side<4;side++)Perimeter(center,12,10,side,side==entrySides[i]);
            foreach(float x in new[]{-5.7f,5.7f})
            {
                var p=Object.Instantiate(pillar,root.transform);p.name="Test stone pillar";
                p.transform.position=center+new Vector3(x,-0.3f,4.8f);p.SetActive(true);
            }
            Vector3 inward=-center.normalized;
            float half=(i==1||i==2)?6:5;
            var gate=Object.Instantiate(gateTemplate,root.transform);gate.name="Battle door";
            gate.transform.position=center+inward*half+Vector3.down*0.1f;
            gate.transform.rotation=Quaternion.Euler(0,(i==1||i==2)?90:0,0);gate.SetActive(true);
            var room=root.AddComponent<BattleRoom>();room.entrance=room.exit=gate.GetComponent<RoomGate>();
            room.entrance.initiallyOpen=true;room.entrance.SetOpen(true,true);room.enemyTemplate=guard;
            var area=root.AddComponent<BoxCollider>();area.isTrigger=true;area.center=Vector3.up;
            area.size=new Vector3(9.5f,4,7.5f);
            var checkpoint=new GameObject("Safe checkpoint outside door").transform;checkpoint.SetParent(root.transform);
            checkpoint.position=center+inward*(half+1.5f)+Vector3.up*0.76f;room.checkpoint=checkpoint;
            room.enemies=new EnemyHealth[counts[i]];
            for(int n=0;n<counts[i];n++)
            {
                var enemy=Object.Instantiate(guard,root.transform);enemy.name="Guard "+(n+1);
                enemy.transform.position=center-inward*2.3f+Vector3.Cross(inward,Vector3.up)*(n-(counts[i]-1)*0.5f)*1.8f+Vector3.up*0.76f;
                enemy.GetComponent<EnemyAI>().player=player.transform;
                var hp=enemy.GetComponent<EnemyHealth>();hp.maxHealth=i==3&&n==1?4:3;room.enemies[n]=hp;enemy.SetActive(true);
            }
            if(i==1||i==3)
            {
                var reward=Object.Instantiate(health,root.transform);reward.name="Clear reward - health";
                reward.transform.position=center+new Vector3(3.8f,0.4f,2.6f);reward.SetActive(false);room.reward=reward;
            }
            Prop("barrel",root.transform,center+new Vector3(-4.3f,-0.1f,2.7f),1.1f,true);
            Prop("barrel",root.transform,center+new Vector3(-3.2f,-0.1f,3.3f),0.9f,true);
            Prop(i%2==0?"rocks":"table",root.transform,center+new Vector3(4.1f,-0.1f,-2.8f),1.5f,true);
            Trap(root.transform,center);
            if(i>0)Trap(root.transform,center+new Vector3(-2.5f,0,-1.9f));
            if(i==3)Trap(root.transform,center+new Vector3(2.5f,0,1.9f));
        }
        Object.DestroyImmediate(gateTemplate);
        player.transform.position=new Vector3(0,0.76f,0);
        player.respawnPoint.position=player.transform.position;
        var camera=Camera.main;camera.transform.position=player.transform.position+camera.GetComponent<CameraFollow>().offset;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.LookAt(Vector3.zero,Quaternion.Euler(70,0,0),32,true);
        return "Cross layout saved: central hall, four rooms, four gates, eight guards, eight spike patches.";
    }

    private static void Floor(string name,Vector3 center,float width,float depth)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(layout);
        g.transform.position=center+Vector3.down*0.6f;g.transform.localScale=new Vector3(width,1,depth);
        g.GetComponent<Renderer>().sharedMaterial=floorMaterial;
    }
    private static void Corridor(Vector3 center,float width,float depth,bool alongZ)
    {
        Floor("Connecting passage",center,width,depth);
        if(alongZ){Edge(center+new Vector3(-width/2,0,-depth/2),center+new Vector3(-width/2,0,depth/2));Edge(center+new Vector3(width/2,0,-depth/2),center+new Vector3(width/2,0,depth/2));}
        else {Edge(center+new Vector3(-width/2,0,-depth/2),center+new Vector3(width/2,0,-depth/2));Edge(center+new Vector3(-width/2,0,depth/2),center+new Vector3(width/2,0,depth/2));}
    }
    private static void Perimeter(Vector3 c,float width,float depth,int side,bool opening)
    {
        Vector3 mid,axis;float half;
        if(side==0||side==2){mid=c+Vector3.forward*(side==0?depth/2:-depth/2);axis=Vector3.right;half=width/2;}
        else{mid=c+Vector3.right*(side==1?width/2:-width/2);axis=Vector3.forward;half=depth/2;}
        if(opening){Edge(mid-axis*half,mid-axis*1.5f);Edge(mid+axis*1.5f,mid+axis*half);}
        else Edge(mid-axis*half,mid+axis*half);
    }
    private static void Edge(Vector3 a,Vector3 b)
    {
        var group=new GameObject("Low Test balustrade boundary").transform;group.SetParent(layout);
        Vector3 delta=b-a;float length=delta.magnitude;int count=Mathf.CeilToInt(length/2f);
        for(int i=0;i<count;i++)
        {
            var wrapper=new GameObject("Stone rail section").transform;wrapper.SetParent(group);
            var g=Object.Instantiate(rail,wrapper);g.SetActive(true);g.transform.localPosition=Vector3.zero;
            g.transform.localRotation=Quaternion.identity;
            foreach(var col in g.GetComponentsInChildren<Collider>())Object.DestroyImmediate(col);
            var rs=g.GetComponentsInChildren<Renderer>();Bounds bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
            g.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            wrapper.localScale=new Vector3((length/count)/bounds.size.x,1,1);
            wrapper.rotation=Quaternion.Euler(0,Mathf.Abs(delta.z)>0.1f?90:0,0);
            wrapper.position=a+delta*((i+0.5f)/count)+Vector3.down*0.1f;
        }
        var block=group.gameObject.AddComponent<BoxCollider>();
        block.center=(a+b)/2+Vector3.up*1.1f;
        block.size=new Vector3(Mathf.Abs(delta.x)+0.22f,2.4f,Mathf.Abs(delta.z)+0.22f);
    }
    private static GameObject Prop(string asset,Transform parent,Vector3 position,float width,bool solid)
    {
        var root=new GameObject(asset=="trap"?"Spike visuals":asset+" obstacle");root.transform.SetParent(parent);root.transform.position=position;
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Props+asset+".fbx"),root.transform);
        model.transform.localPosition=Vector3.zero;
        var rs=model.GetComponentsInChildren<Renderer>();Bounds b=rs[0].bounds;foreach(var r in rs){b.Encapsulate(r.bounds);r.sharedMaterials=r.sharedMaterials.Select(_=>propMaterial).ToArray();}
        float scale=width/Mathf.Max(b.size.x,b.size.z);model.transform.localScale*=scale;
        b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
        model.transform.position+=position-new Vector3(b.center.x,b.min.y,b.center.z);
        if(solid){var col=root.AddComponent<BoxCollider>();col.center=new Vector3(0,b.size.y/2,0);col.size=new Vector3(b.size.x,b.size.y,b.size.z);}
        return root;
    }
    private static void Trap(Transform parent,Vector3 pos)
    {
        var reusable=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Level01/Prefabs/FloorSpikes.prefab");
        if(reusable!=null && reusable.transform.Find("Quaternius spike mechanism")!=null)
        {
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(reusable,parent);
            instance.transform.position=pos;
            return;
        }
        var root=new GameObject("Floor spikes - 1 HP each second");root.transform.SetParent(parent);root.transform.position=pos;
        var plate=GameObject.CreatePrimitive(PrimitiveType.Cube);plate.name="Recessed iron plate";plate.transform.SetParent(root.transform,false);
        plate.transform.localPosition=new Vector3(0,-0.08f,0);plate.transform.localScale=new Vector3(1.7f,0.06f,1.7f);
        plate.GetComponent<Renderer>().sharedMaterial=metal;Object.DestroyImmediate(plate.GetComponent<Collider>());
        var spikes=Prop("trap",root.transform,pos+Vector3.down*0.05f,1.5f,false);spikes.transform.localScale=new Vector3(1,1.5f,1);
        var area=root.AddComponent<BoxCollider>();area.isTrigger=true;area.center=new Vector3(0,0.35f,0);area.size=new Vector3(1.7f,0.9f,1.7f);
        root.AddComponent<SpikeTrap>();
    }
}
