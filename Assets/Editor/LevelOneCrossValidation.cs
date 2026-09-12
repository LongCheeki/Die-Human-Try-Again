using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class LevelOneCrossValidation
{
    static LevelOneCrossValidation()
    {
        EditorApplication.playModeStateChanged += s => {
            if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("CrossValidation",false))
                new GameObject("Temporary validation").AddComponent<CrossValidationRunner>();
        };
    }
    public static string Run(){SessionState.SetBool("CrossValidation",true);EditorApplication.isPlaying=true;return "Cross validation started";}
}
public class CrossValidationRunner : MonoBehaviour
{
    private string report="";
    private void Check(bool ok,string label){if(!ok)throw new Exception(label+"; time="+Time.time+"; player="+Object.FindFirstObjectByType<PlayerHealth>().GetCurrentHealth()+"; enemies="+string.Join(";",Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Select(e=>e.GetCurrentHealth()+" at "+e.transform.position)));report+="PASS: "+label+"\n";}
    private IEnumerator Start()
    {
        Application.runInBackground=true;
        float previousMaximumDeltaTime=Time.maximumDeltaTime;
        Time.maximumDeltaTime=Time.fixedDeltaTime;
        var routine=Validate();
        while(true)
        {
            object step;
            try{if(!routine.MoveNext())break;step=routine.Current;}
            catch(Exception e){report+="FAIL: "+e+"\n";break;}
            yield return step;
        }
        Time.timeScale=1;
        Time.maximumDeltaTime=previousMaximumDeltaTime;
        File.WriteAllText("Temp/Level01-cross-validation.txt",report);
        SessionState.SetBool("CrossValidation",false);
        EditorApplication.isPlaying=false;
    }
    private IEnumerator WaitTicks(float seconds)
    {
        int ticks=Mathf.CeilToInt(seconds/Time.fixedDeltaTime);
        for(int i=0;i<ticks;i++)yield return new WaitForFixedUpdate();
    }
    private IEnumerator Validate()
    {
        yield return WaitTicks(0.3f);
        var player=Object.FindFirstObjectByType<PlayerHealth>();
        var rooms=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).OrderBy(r=>r.name).ToArray();
        var traps=Object.FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None);
        Check(rooms.Length==4 && traps.Length==8,"Four cross rooms and eight spike patches");
        Check(rooms.All(r=>r.entrance==r.exit && r.entrance.IsOpen),"All four branch doors initially open");
        player.playerMovement.enabled=false;player.GetComponent<Rigidbody>().isKinematic=true;
        player.GetComponent<PlayerCombat>().enabled=false;
        player.GetComponent<PlayerCombat>().StopAllCoroutines();
        foreach(var sword in player.GetComponentsInChildren<SwordHitbox>(true))sword.DisableDamage();
        foreach(var room in rooms)
        {
            Vector3 direction=(room.transform.position-room.checkpoint.position);direction.y=0;direction.Normalize();
            Check(!Physics.SphereCast(room.checkpoint.position,0.3f,direction,out var hit,2.4f,~0,QueryTriggerInteraction.Ignore),room.name+" doorway physically passable");
            player.transform.position=room.transform.position-direction*2.5f+Vector3.up*0.76f;Physics.SyncTransforms();
            yield return WaitTicks(0.15f);
            Check(room.State==BattleRoom.RoomState.Fighting && room.entrance.blocker.enabled,"Branch entry locks "+room.name);
            foreach(var enemy in room.enemies)enemy.TakeDamage(999,Vector3.zero);
            yield return WaitTicks(0.15f);
            Check(room.State==BattleRoom.RoomState.Cleared && room.entrance.IsOpen,"Clearing reopens "+room.name);
        }
        player.transform.position=new Vector3(0,0.76f,0);player.maxHealth=10;player.Heal(99);Physics.SyncTransforms();
        var trap=traps[0];Vector3 center=trap.transform.position;
        var enemyObject=Object.Instantiate(rooms[0].enemyTemplate,center+new Vector3(0.3f,0.76f,0),Quaternion.identity);
        var enemyHealth=enemyObject.GetComponent<EnemyHealth>();enemyHealth.maxHealth=10;
        enemyObject.SetActive(true);enemyObject.GetComponent<EnemyAI>().enabled=false;
        enemyObject.GetComponent<Rigidbody>().isKinematic=true;
        player.transform.position=center+new Vector3(-0.3f,0.76f,0);
        var extra=player.gameObject.AddComponent<BoxCollider>();extra.size=new Vector3(0.3f,1,0.3f);
        Physics.SyncTransforms();
        yield return WaitTicks(0.1f);
        Check(player.GetCurrentHealth()==9 && enemyHealth.GetCurrentHealth()==10,"Player takes immediate damage; enemy waits two seconds");
        yield return WaitTicks(0.7f);
        Check(player.GetCurrentHealth()==9 && enemyHealth.GetCurrentHealth()==10,"No duplicate damage before one full second");
        yield return WaitTicks(0.3f);
        Check(player.GetCurrentHealth()==8 && enemyHealth.GetCurrentHealth()==10,"Player loses one per second; enemy has no early damage; colliders deduplicated");
        yield return WaitTicks(1f);
        Check(player.GetCurrentHealth()==7 && enemyHealth.GetCurrentHealth()==9.5f,"Player ticks each second and enemy loses 0.5 after two seconds");
        player.transform.position=new Vector3(0,0.76f,0);enemyObject.transform.position=new Vector3(2,0.76f,0);Physics.SyncTransforms();
        yield return WaitTicks(1.1f);
        Check(player.GetCurrentHealth()==7 && enemyHealth.GetCurrentHealth()==9.5f,"Leaving stops damage for both characters");
        player.playerMimic.warriorUnlocked=true;player.playerMimic.SetWarriorForm(false);
        Check(player.playerMimic.IsWarrior(),"Warrior form active for damage test");
        player.transform.position=center+Vector3.up*0.76f;Physics.SyncTransforms();
        yield return WaitTicks(0.6f);
        Check(player.GetCurrentHealth()==6,"Re-entry immediately hits once then restarts interval");
        Time.timeScale=0;yield return new WaitForSecondsRealtime(0.7f);
        Check(player.GetCurrentHealth()==6,"Paused game does not tick trap damage");
        Time.timeScale=1;yield return WaitTicks(0.5f);
        Check(player.GetCurrentHealth()==5,"Warrior form also loses exactly one HP");
        player.transform.position=new Vector3(0,0.76f,0);Physics.SyncTransforms();
        enemyHealth.TakeDamage(8.5f,Vector3.zero);
        enemyObject.transform.position=center+Vector3.up*0.76f;Physics.SyncTransforms();
        yield return WaitTicks(2.2f);
        Check(enemyHealth.GetCurrentHealth()==0.5f,"Enemy re-entry waits two seconds then loses 0.5");
        yield return WaitTicks(2f);
        Check(enemyHealth==null,"Spikes can kill enemies through the normal death flow");
        player.TakeTrapDamage(4);
        player.transform.position=center+Vector3.up*0.76f;Physics.SyncTransforms();
        yield return WaitTicks(0.2f);
        Check(player.GetCurrentHealth()==0,"Spikes can kill the player");
        yield return WaitTicks(2.2f);
        Check(player.GetCurrentHealth()==player.maxHealth && Vector3.Distance(player.transform.position,player.respawnPoint.position)<0.1f,"Trap death respawns player safely with full health (HP="+player.GetCurrentHealth()+", position="+player.transform.position+", checkpoint="+player.respawnPoint.position+")");
    }
}
