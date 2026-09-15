using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class LevelOneProgressionValidation
{
    static LevelOneProgressionValidation()
    {
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("ProgressionValidation",false))
                new GameObject("Temporary progression validation").AddComponent<ProgressionValidationRunner>();
        };
    }
    public static string Run(){SessionState.SetBool("ProgressionValidation",true);EditorApplication.isPlaying=true;return "Progression validation started";}
}

public class ProgressionValidationRunner:MonoBehaviour
{
    private string report="";
    private void Check(bool ok,string label){if(!ok)throw new Exception(label);report+="PASS: "+label+"\n";}
    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);Application.runInBackground=true;
        var routine=Validate();
        while(true)
        {
            object step;
            try{if(!routine.MoveNext())break;step=routine.Current;}
            catch(Exception ex){report+="FAIL: "+ex+"\n";break;}
            yield return step;
        }
        File.WriteAllText("Temp/Level01-progression-validation.txt",report);
        Time.timeScale=1;SessionState.SetBool("ProgressionValidation",false);EditorApplication.isPlaying=false;
    }
    private void Move(PlayerHealth player,Vector3 position)
    {
        player.playerMovement.enabled=false;
        player.GetComponent<PlayerCombat>().enabled=false;player.GetComponent<PlayerCombat>().StopAllCoroutines();
        foreach(var sword in player.GetComponentsInChildren<SwordHitbox>(true))sword.DisableDamage();
        player.GetComponent<Rigidbody>().isKinematic=true;
        player.transform.position=position;Physics.SyncTransforms();
    }
    private IEnumerator Validate()
    {
        yield return new WaitForSeconds(0.3f);
        var player=Object.FindFirstObjectByType<PlayerHealth>();
        var weapon=Object.FindFirstObjectByType<WeaponPickup>(FindObjectsInactive.Include);
        var orb=Object.FindFirstObjectByType<MimicPickup>(FindObjectsInactive.Include);
        var exit=Object.FindFirstObjectByType<LevelExit>();
        Check(weapon.GetComponentInParent<BattleRoom>()!=orb.GetComponentInParent<BattleRoom>() && weapon.GetComponentInParent<BossEncounter>()==null && orb.GetComponentInParent<BossEncounter>()==null,"Pickups occupy different ordinary rooms");
        Check(weapon.choiceUI!=null && weapon.weaponPrefab.GetComponent<SwordHitbox>()!=null && weapon.weaponPrefab.GetComponent<WeaponPickup>()==null,"Weapon has local choice UI and a separate usable combat prefab");
        Check(!exit.gate.IsOpen && exit.gate.blocker.enabled && exit.destinationScene=="","Exit starts locked with destination intentionally unassigned");
        var south=weapon.GetComponentInParent<BattleRoom>();
        Move(player,south.transform.position+new Vector3(0,0.76f,2.5f));yield return new WaitForSeconds(0.15f);
        foreach(var enemy in south.enemies)enemy.TakeDamage(999,Vector3.zero);
        yield return new WaitForSeconds(0.15f);
        Move(player,new Vector3(weapon.transform.position.x,0.76f,weapon.transform.position.z));
        yield return new WaitForSeconds(0.2f);
        var ui=weapon.choiceUI;
        Check(ui.choicePanel.activeSelf,"Touching the copied sword opens weapon choice");
        ui.ChooseUse();yield return new WaitForSeconds(0.1f);
        Check(weapon==null && player.GetComponent<WeaponManager>().GetCurrentWeapon().GetComponent<SwordHitbox>()!=null && !ui.choicePanel.activeSelf,"Choosing use equips the copied sword and consumes pickup");
        foreach(var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))enemy.TakeDamage(999,Vector3.zero);
        player.playerMimic.warriorUnlocked=false;
        var west=orb.GetComponentInParent<BattleRoom>();
        Move(player,west.transform.position+new Vector3(2.5f,0.76f,0));yield return new WaitForSeconds(0.15f);
        foreach(var enemy in west.enemies)enemy.TakeDamage(999,Vector3.zero);
        yield return new WaitForSeconds(0.15f);
        Move(player,new Vector3(orb.transform.position.x,0.76f,orb.transform.position.z));
        yield return new WaitForSeconds(0.2f);
        Check(orb==null && player.playerMimic.warriorUnlocked,"Copied orb unlocks warrior form and disappears");
        player.playerMimic.SetWarriorForm(false);Check(player.playerMimic.IsWarrior(),"Unlocked form can be activated");
        player.playerMimic.SetSlimeForm(false);
        foreach(var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))enemy.TakeDamage(999,Vector3.zero);
        Move(player,new Vector3(0,0.76f,11));yield return new WaitForSeconds(0.15f);
        foreach(var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))enemy.TakeDamage(999,Vector3.zero);
        yield return new WaitForSeconds(0.15f);
        Move(player,new Vector3(0,0.76f,26));yield return new WaitForSeconds(0.15f);
        Check(exit.encounter.IsFighting && exit.gate.blocker.enabled,"Boss combat keeps onward exit sealed");
        player.playerMovement.enabled=true;player.GetComponent<PlayerCombat>().enabled=true;
        exit.encounter.boss.TakeDamage(999,Vector3.zero);yield return new WaitForSeconds(0.2f);
        Check(exit.encounter.IsComplete && Time.timeScale==1 && player.playerMovement.enabled && player.GetComponent<PlayerCombat>().enabled,"Boss death leaves game time and player controls running");
        Check(exit.gate.IsOpen && !exit.gate.blocker.enabled,"Boss death opens onward door");
        Check(!Physics.SphereCast(new Vector3(0,0.76f,38.5f),0.3f,Vector3.forward,out var hit,4f,~0,QueryTriggerInteraction.Ignore),"Onward doorway and landing are physically passable");
        Move(player,new Vector3(0,0.76f,42));yield return new WaitForSeconds(0.15f);
        Check(SceneManager.GetActiveScene().name=="Level01_Castle","Unassigned exit does not load a wrong scene");
        exit.destinationScene="Test";
        float deadline=Time.realtimeSinceStartup+15;
        while(SceneManager.GetActiveScene().name!="Test" && Time.realtimeSinceStartup<deadline)yield return null;
        Check(SceneManager.GetActiveScene().name=="Test" && Time.timeScale==1,"Assigning a valid destination transitions successfully without pausing");
        Check(Object.FindFirstObjectByType<WeaponPickup>()!=null && Object.FindFirstObjectByType<MimicPickup>()!=null,"Original Test pickups remain present");
    }
}
