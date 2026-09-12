using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class LevelOneValidation
{
    private static BattleRoom[] rooms;
    private static PlayerHealth player;
    private static int stage;
    private static double next;
    private static string results;

    static LevelOneValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("LevelOneValidate",false))
            {
                stage=0; next=EditorApplication.timeSinceStartup+1;
                results=""; EditorApplication.update-=Tick; EditorApplication.update+=Tick;
            }
        };
    }
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Already playing.");
        SessionState.SetBool("LevelOneValidate",true);
        EditorApplication.isPlaying=true;
        return "Runtime validation started";
    }
    private static void Check(bool condition,string label)
    {
        if(!condition)throw new Exception(label);
        results+="PASS: "+label+"\n";
    }
    private static void Tick()
    {
        if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next)return;
        next=EditorApplication.timeSinceStartup+0.5;
        try
        {
            if(stage==0)
            {
                Application.runInBackground=true;
                rooms=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).OrderBy(r=>r.name).ToArray();
                player=Object.FindFirstObjectByType<PlayerHealth>();
                Check(rooms.Length==4,"Four battle rooms");
                Check(rooms.Sum(r=>r.enemies.Length)==8,"Eight guards");
                Check(rooms.All(r=>r.State==BattleRoom.RoomState.Waiting && r.entrance.IsOpen && !r.exit.IsOpen),"Initial entry gates open, exits closed");
                Check(rooms.All(r=>r.enemies.All(e=>!e.gameObject.activeInHierarchy)),"Unentered room enemies inactive");
                player.playerMovement.enabled=false;
                player.GetComponent<Rigidbody>().isKinematic=true;
                foreach(var room in rooms)
                {
                    var start=room.checkpoint.position;
                    Check(!Physics.SphereCast(start,0.3f,Vector3.right,out var hit,4f,~0,QueryTriggerInteraction.Ignore),room.name+" entrance is physically passable");
                }
                player.transform.position=rooms[0].transform.position+new Vector3(-3,0.76f,0);
                Physics.SyncTransforms();
            }
            else if(stage==1)
            {
                Check(rooms[0].State==BattleRoom.RoomState.Fighting && !rooms[0].entrance.IsOpen && !rooms[0].exit.IsOpen,"Entering room locks both gates (state="+rooms[0].State+", player="+player.transform.position+", time="+Time.time+")");
                Check(rooms[0].enemies.All(e=>e.gameObject.activeInHierarchy),"Room entry activates guards");
                Check(rooms.Skip(1).All(r=>r.enemies.All(e=>!e.gameObject.activeInHierarchy)),"Future room guards stay inactive");
                player.TakeDamage(100);
            }
            else if(stage==2)
            {
                Check(rooms[0].State==BattleRoom.RoomState.Waiting && rooms[0].entrance.IsOpen && !rooms[0].exit.IsOpen,"Death resets gates");
                Check(rooms[0].enemies.All(e=>e!=null && !e.gameObject.activeInHierarchy),"Death restores encounter guards");
                next=EditorApplication.timeSinceStartup+2.2;
            }
            else if(stage==3)
            {
                Check(player.GetCurrentHealth()==player.maxHealth,"Player respawns with full health");
                Check(Vector3.Distance(player.transform.position,rooms[0].checkpoint.position)<0.1f,"Respawn is outside locked encounter");
                player.playerMovement.enabled=false;
                player.transform.position=rooms[0].transform.position+new Vector3(-3,0.76f,0);Physics.SyncTransforms();
            }
            else if(stage>=4 && stage<=11)
            {
                int index=(stage-4)/2;
                var room=rooms[index];
                if(stage%2==0)
                {
                    Check(room.State==BattleRoom.RoomState.Fighting,"Room "+(index+1)+" starts combat");
                    Check(!room.entrance.IsOpen && !room.exit.IsOpen,"Room "+(index+1)+" physically sealed");
                    Check(room.entrance.blocker.enabled && room.exit.blocker.enabled,"Room "+(index+1)+" gate colliders enabled");
                    foreach(var e in room.enemies)e.TakeDamage(999,Vector3.zero);
                }
                else
                {
                    Check(room.State==BattleRoom.RoomState.Cleared && room.entrance.IsOpen && room.exit.IsOpen,"Room "+(index+1)+" clears and opens both gates");
                    if(room.reward!=null)Check(room.reward.activeInHierarchy,"Room "+(index+1)+" grants health reward");
                    if(index<3){player.transform.position=rooms[index+1].transform.position+new Vector3(-3,0.76f,0);Physics.SyncTransforms();}
                }
            }
            else
            {
                Check(rooms.All(r=>r.State==BattleRoom.RoomState.Cleared),"All four rooms completed sequentially");
                Finish(true);return;
            }
            stage++;
        }
        catch(Exception e){results+="FAIL: "+e+"\n";Finish(false);}
    }
    private static void Finish(bool success)
    {
        Directory.CreateDirectory("Temp");
        File.WriteAllText("Temp/Level01-validation.txt",results);
        SessionState.SetBool("LevelOneValidate",false);
        EditorApplication.update-=Tick;
        Debug.Log("Level01 validation "+(success?"PASSED":"FAILED"));
        EditorApplication.isPlaying=false;
    }
}
