using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class RoomRosterValidation
{
    static RoomRosterValidation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("RoomRosterValidation",false))new GameObject("Temporary room roster validation").AddComponent<RoomRosterRunner>();
        };
    }
    public static string Run(){SessionState.SetBool("RoomRosterValidation",true);EditorApplication.isPlaying=true;return "Room roster validation started";}
}
public class RoomRosterRunner:MonoBehaviour
{
    private string report="";
    private void Check(bool ok,string label){if(!ok)throw new Exception(label);report+="PASS: "+label+"\n";}
    private IEnumerator Start()
    {
        Application.runInBackground=true;
        var routine=Validate();
        while(true)
        {
            object next;
            try{if(!routine.MoveNext())break;next=routine.Current;}catch(Exception ex){report+="FAIL: "+ex+"\n";break;}
            yield return next;
        }
        File.WriteAllText("Temp/room-roster-validation.txt",report);
        Time.timeScale=1;SessionState.SetBool("RoomRosterValidation",false);EditorApplication.isPlaying=false;
    }
    private void Move(PlayerHealth player,Vector3 point)
    {
        player.playerMovement.enabled=false;
        var combat=player.GetComponent<PlayerCombat>();combat.enabled=false;combat.StopAllCoroutines();
        foreach(var sword in player.GetComponentsInChildren<SwordHitbox>(true))sword.DisableDamage();
        player.GetComponent<Rigidbody>().isKinematic=true;
        player.transform.position=point;Physics.SyncTransforms();
    }
    private void Freeze(BattleRoom room)
    {
        foreach(var enemy in room.enemies)
        {
            enemy.GetComponent<EnemyAI>().enabled=false;
            enemy.GetComponent<Rigidbody>().isKinematic=true;
        }
    }
    private bool RewardsHidden(BattleRoom room)
    {
        return (room.reward==null || !room.reward.activeSelf) && (room.clearRewards==null || room.clearRewards.All(r=>r==null || !r.activeSelf));
    }
    private IEnumerator Validate()
    {
        yield return new WaitForSeconds(0.3f);
        var rooms=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).OrderBy(r=>r.name).ToArray();
        var player=Object.FindFirstObjectByType<PlayerHealth>();
        player.restartSceneOnDeath=false;
        Check(rooms.Select(r=>r.enemies.Length).SequenceEqual(new[]{3,4,3,5}),"All 15 ordinary enemies registered across rooms: 3,4,3,5");
        Check(rooms.All(r=>r.enemies.All(e=>!e.gameObject.activeSelf)),"New enemies also wait inactive before room entry");
        foreach(var room in rooms)
        {
            int count=room.enemies.Length;
            room.enemies=room.enemies.Concat(new EnemyHealth[]{null,room.enemies[0]}).ToArray();room.RefreshEnemies();
            Check(room.enemies.Length==count,"Null and duplicate entries are removed: "+room.name);
            var positions=room.enemies.Select(e=>e.transform.position).ToArray();
            var health=room.enemies.Select(e=>e.maxHealth).ToArray();
            var direction=room.transform.position-room.checkpoint.position;direction.y=0;direction.Normalize();
            var entry=room.transform.position-direction*2.5f+Vector3.up*0.76f;
            Move(player,entry);yield return new WaitForSeconds(0.15f);Freeze(room);
            Check(room.State==BattleRoom.RoomState.Fighting && room.enemies.All(e=>e.gameObject.activeSelf) && room.entrance.blocker.enabled,"Every enemy joins the fight and doors lock: "+room.name);
            for(int i=0;i<count-1;i++)room.enemies[i].TakeDamage(999,Vector3.zero);
            yield return new WaitForSeconds(0.15f);
            Check(room.State==BattleRoom.RoomState.Fighting && room.entrance.blocker.enabled && RewardsHidden(room),"Last new enemy prevents early opening and rewards: "+room.name);
            player.TakeTrapDamage(999);yield return new WaitForSeconds(0.15f);
            Check(room.State==BattleRoom.RoomState.Waiting && room.enemies.Length==count && room.enemies.All(e=>e!=null && !e.gameObject.activeSelf),"Player death restores every enemy: "+room.name);
            Check(room.enemies.Select((e,i)=>Vector3.Distance(e.transform.position,positions[i])<0.01f && e.maxHealth==health[i]).All(x=>x),"Retry preserves all spawn positions and HP: "+room.name);
            yield return new WaitForSeconds(2.1f);
            Check(player.GetCurrentHealth()==player.maxHealth && RewardsHidden(room),"Retry has full player health and no premature reward: "+room.name);
            Move(player,entry);yield return new WaitForSeconds(0.15f);Freeze(room);
            Check(room.enemies.All(e=>e.GetCurrentHealth()==e.maxHealth),"All respawned enemies start at full HP: "+room.name);
            foreach(var enemy in room.enemies)enemy.TakeDamage(999,Vector3.zero);
            yield return new WaitForSeconds(0.15f);
            Check(room.State==BattleRoom.RoomState.Cleared && room.entrance.IsOpen && room.exit.IsOpen,"Clearing every enemy opens both doors: "+room.name);
            Check((room.reward==null || room.reward.activeSelf) && (room.clearRewards==null || room.clearRewards.All(r=>r==null || r.activeSelf)),"Clear rewards appear only after all enemies disappear: "+room.name);
        }
        var boss=Object.FindFirstObjectByType<BossEncounter>();
        Move(player,new Vector3(0,0.76f,26));yield return new WaitForSeconds(0.15f);
        Check(boss.IsFighting,"Expanded north room still leads into Boss combat");
        player.playerMovement.enabled=true;player.GetComponent<PlayerCombat>().enabled=true;
        boss.boss.TakeDamage(999,Vector3.zero);yield return new WaitForSeconds(0.15f);
        var exit=Object.FindFirstObjectByType<LevelExit>();
        Check(boss.IsComplete && exit.gate.IsOpen && Time.timeScale==1 && player.playerMovement.enabled,"Boss defeat opens exit without pausing after expanded rooms");
    }
}
