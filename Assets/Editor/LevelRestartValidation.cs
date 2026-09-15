using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class LevelRestartValidation
{
    static LevelRestartValidation()
    {
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("LevelRestartValidation",false))new GameObject("Temporary level restart validation").AddComponent<LevelRestartRunner>();
        };
    }
    public static string Run(){SessionState.SetBool("LevelRestartValidation",true);EditorApplication.isPlaying=true;return "Level restart validation started";}
}
public class LevelRestartRunner:MonoBehaviour
{
    private string report="";
    private int initialBonus;
    private bool initialUnlock;
    private string initialWeapon;
    private void Check(bool ok,string label){if(!ok)throw new Exception(label);report+="PASS: "+label+"\n";}
    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);Application.runInBackground=true;
        var routine=Validate();
        while(true)
        {
            object next;
            try{if(!routine.MoveNext())break;next=routine.Current;}catch(Exception ex){report+="FAIL: "+ex+"\n";break;}
            yield return next;
        }
        File.WriteAllText("Temp/level-restart-validation.txt",report);
        Time.timeScale=1;SessionState.SetBool("LevelRestartValidation",false);EditorApplication.isPlaying=false;
    }
    private void Move(PlayerHealth player,Vector3 point)
    {
        player.playerMovement.enabled=false;
        var combat=player.GetComponent<PlayerCombat>();combat.enabled=false;combat.StopAllCoroutines();
        foreach(var sword in player.GetComponentsInChildren<SwordHitbox>(true))sword.DisableDamage();
        player.GetComponent<Rigidbody>().isKinematic=true;player.transform.position=point;Physics.SyncTransforms();
    }
    private void CheckInitialState()
    {
        var p=Object.FindFirstObjectByType<PlayerHealth>();
        Check(p.restartSceneOnDeath && p.GetCurrentHealth()==p.maxHealth && new Vector2(p.transform.position.x,p.transform.position.z).magnitude<0.1f,"Player restarts with full HP at original central spawn");
        Check(Time.timeScale==1 && p.playerMovement.enabled && p.GetComponent<PlayerCombat>().enabled && p.playerMimic.enabled,"Time and player controls restore after restart");
        Check(!p.playerMimic.IsWarrior() && p.playerMimic.warriorUnlocked==initialUnlock && p.GetComponent<WeaponManager>().sacrificeDamageBonus==initialBonus && p.GetComponent<WeaponManager>().GetCurrentWeapon().name==initialWeapon,"Form unlock, equipment and upgrades reset to initial values");
        var rooms=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None);
        Check(rooms.Sum(r=>r.enemies.Length)==15 && rooms.All(r=>r.State==BattleRoom.RoomState.Waiting && r.enemies.All(e=>e!=null && !e.gameObject.activeSelf)),"All four rooms and fifteen ordinary enemies reset");
        Check(rooms.All(r=>r.entrance.IsOpen && (r.exit==r.entrance || !r.exit.IsOpen)),"Ordinary doors return to initial states");
        var weapon=Object.FindFirstObjectByType<WeaponPickup>(FindObjectsInactive.Include);
        var orb=Object.FindFirstObjectByType<MimicPickup>(FindObjectsInactive.Include);
        Check(weapon!=null && orb!=null && !weapon.gameObject.activeSelf && !orb.gameObject.activeSelf,"Consumed weapon and orb return as hidden clear rewards");
        var packs=Object.FindObjectsByType<HealthPickup>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(h=>h.GetComponentInParent<BattleRoom>()!=null).ToArray();
        Check(packs.Length==2 && packs.All(h=>!h.gameObject.activeSelf),"Both medical packs return as hidden clear rewards");
        var boss=Object.FindFirstObjectByType<BossEncounter>();var exit=Object.FindFirstObjectByType<LevelExit>();
        Check(!boss.IsFighting && !boss.IsComplete && boss.SummonedCount==0 && boss.boss.maxHealth==15 && !boss.boss.gameObject.activeSelf && !exit.gate.IsOpen,"Boss, summons and onward exit reset");
        Check(Object.FindFirstObjectByType<BossWarningCircle>()==null && Object.FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None).Length==8,"Warning circles disappear and eight traps are restored");
        Check(Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None).Length==1 && Object.FindObjectsByType<CastleBattleMusic>(FindObjectsSortMode.None).Length==1,"Restart leaves one player and one music controller");
    }
    private IEnumerator Validate()
    {
        yield return new WaitForSeconds(0.3f);
        var player=Object.FindFirstObjectByType<PlayerHealth>();
        initialBonus=player.GetComponent<WeaponManager>().sacrificeDamageBonus;initialUnlock=player.playerMimic.warriorUnlocked;initialWeapon=player.GetComponent<WeaponManager>().GetCurrentWeapon().name;
        Check(player.restartSceneOnDeath,"Whole-level restart enabled in Level01");
        var rooms=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).OrderBy(r=>r.name).ToArray();
        foreach(var room in rooms)
        {
            var direction=room.transform.position-room.checkpoint.position;direction.y=0;direction.Normalize();
            Move(player,room.transform.position-direction*2.5f+Vector3.up*0.76f);yield return new WaitForSeconds(0.15f);
            foreach(var enemy in room.enemies)enemy.TakeDamage(999,Vector3.zero);
            yield return new WaitForSeconds(0.15f);
        }
        var weapon=Object.FindFirstObjectByType<WeaponPickup>();Move(player,new Vector3(weapon.transform.position.x,0.76f,weapon.transform.position.z));yield return new WaitForSeconds(0.15f);
        weapon.choiceUI.ChooseUse();yield return new WaitForSeconds(0.1f);
        var orb=Object.FindFirstObjectByType<MimicPickup>();Move(player,new Vector3(orb.transform.position.x,0.76f,orb.transform.position.z));yield return new WaitForSeconds(0.15f);
        player.GetComponent<WeaponManager>().SacrificeWeapon();player.playerMimic.SetWarriorForm(false);
        player.TakeTrapDamage(1);var pack=rooms[1].reward;
        Move(player,new Vector3(pack.transform.position.x,0.76f,pack.transform.position.z));yield return new WaitForSeconds(0.15f);
        Check(weapon==null && orb==null && pack==null && player.playerMimic.IsWarrior(),"Run progressed: rooms cleared, weapon and orb taken, medical pack consumed, form changed");
        var boss=Object.FindFirstObjectByType<BossEncounter>();Move(player,new Vector3(0,0.76f,26));yield return new WaitForSeconds(0.15f);
        boss.boss.GetComponent<EnemyAI>().enabled=false;boss.boss.GetComponent<Rigidbody>().isKinematic=true;
        boss.boss.TakeDamage(3,Vector3.zero);yield return new WaitForSeconds(0.1f);
        foreach(var enemy in boss.GetComponentsInChildren<EnemyHealth>()){enemy.GetComponent<EnemyAI>().enabled=false;enemy.GetComponent<Rigidbody>().isKinematic=true;}
        float deadline=Time.realtimeSinceStartup+8;
        while(Object.FindFirstObjectByType<BossWarningCircle>()==null && Time.realtimeSinceStartup<deadline)yield return null;
        Check(boss.SummonedCount==1 && Object.FindFirstObjectByType<BossWarningCircle>()!=null,"Boss damage, summoned enemy and warning circle exist before death");
        var oldPlayer=player;player.TakeTrapDamage(999);Time.timeScale=0;
        deadline=Time.realtimeSinceStartup+8;
        while(oldPlayer!=null && Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(0.4f);
        Check(oldPlayer==null,"Death reloads scene even if pause is active during the death delay");CheckInitialState();
        player=Object.FindFirstObjectByType<PlayerHealth>();
        var south=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).First(r=>r.name.StartsWith("01"));
        Move(player,south.transform.position+new Vector3(0,0.76f,2.5f));yield return new WaitForSeconds(0.15f);
        south.enemies[0].TakeDamage(999,Vector3.zero);
        oldPlayer=player;player.TakeTrapDamage(999);deadline=Time.realtimeSinceStartup+8;
        while(oldPlayer!=null && Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(0.4f);
        Check(oldPlayer==null,"Death during an ordinary room also reloads the whole scene");CheckInitialState();
    }
}
