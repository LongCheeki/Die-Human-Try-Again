using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class ClearRewardsValidation
{
    static ClearRewardsValidation()
    {
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("ClearRewardsValidation",false))new GameObject("Temporary clear rewards validation").AddComponent<ClearRewardsRunner>();
        };
    }
    public static string Run(){SessionState.SetBool("ClearRewardsValidation",true);EditorApplication.isPlaying=true;return "Clear rewards validation started";}
}
public class ClearRewardsRunner:MonoBehaviour
{
    private string report="";
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
        File.WriteAllText("Temp/clear-rewards-validation.txt",report);
        Time.timeScale=1;SessionState.SetBool("ClearRewardsValidation",false);EditorApplication.isPlaying=false;
    }
    private void Move(PlayerHealth player,Vector3 position)
    {
        player.playerMovement.enabled=false;
        var combat=player.GetComponent<PlayerCombat>();combat.enabled=false;combat.StopAllCoroutines();
        foreach(var sword in player.GetComponentsInChildren<SwordHitbox>(true))sword.DisableDamage();
        player.GetComponent<Rigidbody>().isKinematic=true;player.transform.position=position;Physics.SyncTransforms();
    }
    private IEnumerator Validate()
    {
        yield return new WaitForSeconds(0.3f);
        var exit=Object.FindFirstObjectByType<LevelExit>();var player=Object.FindFirstObjectByType<PlayerHealth>();
        var enemies=Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach(var enemy in enemies){enemy.GetComponent<EnemyAI>().enabled=false;enemy.GetComponent<Rigidbody>().isKinematic=true;}
        Check(exit.requireSceneClear && exit.destinationScene=="Level01_Castle" && !exit.gate.IsOpen,"Test exit starts locked and targets Level01");
        Move(player,exit.transform.position+Vector3.down*0.24f);yield return new WaitForSeconds(0.15f);
        Check(SceneManager.GetActiveScene().name=="Test","Locked portal cannot transfer player");
        Move(player,new Vector3(48,0.76f,-1.5f));
        for(int i=0;i<enemies.Length-1;i++)enemies[i].TakeDamage(999,Vector3.zero);
        yield return new WaitForSeconds(0.15f);Check(!exit.gate.IsOpen,"One remaining enemy keeps portal locked");
        enemies[enemies.Length-1].TakeDamage(999,Vector3.zero);yield return new WaitForSeconds(0.15f);
        Check(exit.gate.IsOpen && !exit.gate.blocker.enabled,"Last enemy disappearing opens Test portal");
        Check(!Physics.SphereCast(new Vector3(49.1f,0.76f,-1.5f),0.25f,Vector3.right,out var hit,1.55f,~0,QueryTriggerInteraction.Ignore),"Open portal can be reached before the right boundary");
        Move(player,new Vector3(50.7f,0.76f,-1.5f));
        float deadline=Time.realtimeSinceStartup+15;
        while(SceneManager.GetActiveScene().name!="Level01_Castle" && Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSeconds(0.3f);
        Check(SceneManager.GetActiveScene().name=="Level01_Castle","Walking into unlocked portal loads first level");
        player=Object.FindFirstObjectByType<PlayerHealth>();
        var weapon=Object.FindFirstObjectByType<WeaponPickup>(FindObjectsInactive.Include);
        var orb=Object.FindFirstObjectByType<MimicPickup>(FindObjectsInactive.Include);
        Check(!weapon.gameObject.activeSelf && !orb.gameObject.activeSelf,"First-level weapon and orb start hidden");
        var south=weapon.GetComponentInParent<BattleRoom>();var west=orb.GetComponentInParent<BattleRoom>();
        Check(Vector3.Distance(orb.transform.position,west.reward.transform.position)>2,"Orb is separated from the medical pack");
        Move(player,south.transform.position+new Vector3(0,0.76f,2.5f));yield return new WaitForSeconds(0.15f);
        Check(!weapon.gameObject.activeSelf,"Weapon stays hidden during south room fight");
        foreach(var enemy in south.enemies)enemy.TakeDamage(999,Vector3.zero);
        yield return new WaitForSeconds(0.15f);
        Check(weapon.gameObject.activeSelf && !orb.gameObject.activeSelf,"South clear reveals only its own weapon reward");
        Move(player,west.transform.position+new Vector3(2.5f,0.76f,0));yield return new WaitForSeconds(0.15f);
        west.enemies[0].TakeDamage(999,Vector3.zero);yield return new WaitForSeconds(0.15f);
        Check(!orb.gameObject.activeSelf && !west.reward.activeSelf,"Partial west clear reveals neither orb nor medical reward");
        foreach(var enemy in west.enemies)if(enemy!=null)enemy.TakeDamage(999,Vector3.zero);
        yield return new WaitForSeconds(0.15f);
        Check(orb.gameObject.activeSelf && west.reward.activeSelf,"West clear reveals orb and separate medical pack");
        player.playerMimic.warriorUnlocked=false;
        Move(player,new Vector3(orb.transform.position.x,0.76f,orb.transform.position.z));yield return new WaitForSeconds(0.15f);
        Check(orb==null && player.playerMimic.warriorUnlocked,"Revealed orb still unlocks transformation");
        Move(player,new Vector3(weapon.transform.position.x,0.76f,weapon.transform.position.z));yield return new WaitForSeconds(0.15f);
        var ui=weapon.choiceUI;Check(ui.choicePanel.activeSelf,"Revealed weapon still opens pickup choice");
        ui.ChooseUse();yield return new WaitForSeconds(0.1f);
        Check(weapon==null && player.GetComponent<WeaponManager>().GetCurrentWeapon().GetComponent<SwordHitbox>()!=null,"Revealed weapon can be equipped");
    }
}
