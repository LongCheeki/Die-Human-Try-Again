using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class LevelOneBossValidation
{
    static LevelOneBossValidation()
    {
        EditorApplication.playModeStateChanged+=state=>{
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("BossValidation",false))
                new GameObject("Temporary Boss validation").AddComponent<BossValidationRunner>();
        };
    }
    public static string Run(){SessionState.SetBool("BossValidation",true);EditorApplication.isPlaying=true;return "Boss validation started";}
}

public class BossValidationRunner:MonoBehaviour
{
    private string report="";
    private void Check(bool ok,string message)
    {
        if(!ok)throw new Exception(message+" at "+Time.time);
        report+="PASS: "+message+"\n";
    }
    private IEnumerator Start()
    {
        Application.runInBackground=true;
        float maximum=Time.maximumDeltaTime;
        Time.maximumDeltaTime=Time.fixedDeltaTime;
        var routine=Validate();
        while(true)
        {
            object next;
            try{if(!routine.MoveNext())break;next=routine.Current;}
            catch(Exception e){report+="FAIL: "+e+"\n";break;}
            yield return next;
        }
        Time.timeScale=1;Time.maximumDeltaTime=maximum;
        File.WriteAllText("Temp/Level01-boss-validation.txt",report);
        SessionState.SetBool("BossValidation",false);
        EditorApplication.isPlaying=false;
    }
    private IEnumerator WaitTicks(float seconds)
    {
        for(int i=0;i<Mathf.CeilToInt(seconds/Time.fixedDeltaTime);i++)yield return new WaitForFixedUpdate();
    }
    private void Teleport(PlayerHealth player,Vector3 position)
    {
        player.transform.position=position;Physics.SyncTransforms();
    }
    private void Freeze(EnemyHealth enemy)
    {
        var ai=enemy.GetComponent<EnemyAI>();ai.enabled=false;ai.CancelInvoke();
        if(ai.animator!=null)ai.animator.enabled=false;
        var rb=enemy.GetComponent<Rigidbody>();if(!rb.isKinematic)rb.linearVelocity=Vector3.zero;rb.isKinematic=true;
    }
    private void FreezeMinions(BossEncounter encounter)
    {
        foreach(var e in encounter.GetComponentsInChildren<EnemyHealth>())if(e!=encounter.boss)Freeze(e);
    }
    private IEnumerator Validate()
    {
        yield return WaitTicks(0.3f);
        var e=Object.FindFirstObjectByType<BossEncounter>();
        var player=Object.FindFirstObjectByType<PlayerHealth>();
        player.restartSceneOnDeath=false;
        player.playerMovement.enabled=false;player.GetComponent<Rigidbody>().isKinematic=true;
        var combat=player.GetComponent<PlayerCombat>();combat.enabled=false;combat.StopAllCoroutines();
        foreach(var sword in player.GetComponentsInChildren<SwordHitbox>(true))sword.DisableDamage();
        var normal=Object.FindObjectsByType<BattleRoom>(FindObjectsSortMode.None).First(x=>x.name.StartsWith("01")).enemies[0];
        Check(e.boss.maxHealth==normal.maxHealth*5 && e.boss.maxHealth==15,"Boss HP equals five ordinary enemies: 15");
        Check(e.boss.GetComponent<EnemyAI>().moveSpeed<normal.GetComponent<EnemyAI>().moveSpeed && e.boss.transform.localScale.x>normal.transform.localScale.x,"Boss is larger and slower");
        Check(e.GetComponentsInChildren<SpikeTrap>(true).Length==0 && e.transform.Cast<Transform>().Count(x=>x.name.EndsWith("obstacle"))==2,"Boss room has two obstacles and no traps");
        Check(GameObject.Find("Boss room floor - 18 x 16").transform.localScale==new Vector3(18,1,16),"Boss floor is larger than the 12 x 10 ordinary floor");
        var gold=e.gate.GetComponentsInChildren<Renderer>(true).First().sharedMaterial.GetColor("_BaseColor");
        Check(gold.r>0.9f && gold.g>0.5f && gold.b<0.2f,"Boss doorway uses gold material");
        Check(e.gate.IsOpen && !e.boss.gameObject.activeSelf,"Boss waits behind the open gold entrance");
        Teleport(player,new Vector3(0,0.76f,26));yield return WaitTicks(0.1f);
        Check(!e.IsFighting,"Boss cannot start before the north room is cleared");
        Teleport(player,new Vector3(0,0.76f,11));yield return WaitTicks(0.1f);
        Check(e.previousRoom.State==BattleRoom.RoomState.Fighting && e.previousRoom.exit.blocker.enabled,"North battle locks the onward passage");
        Check(Object.FindObjectsByType<RoomGate>(FindObjectsSortMode.None).All(g=>g.tallVisuals.All(r=>!r.enabled)),"All tall doorway visuals hide during ordinary combat");
        Check(e.previousRoom.entrance.closedMarker.activeSelf && e.previousRoom.exit.closedMarker.activeSelf,"Closed doors retain low visible markers and physical blockers");
        foreach(var enemy in e.previousRoom.enemies)enemy.TakeDamage(999,Vector3.zero);
        yield return WaitTicks(0.2f);
        Check(e.previousRoom.State==BattleRoom.RoomState.Cleared && e.previousRoom.exit.IsOpen,"North clear opens onward passage");
        Check(e.previousRoom.entrance.tallVisuals.All(r=>r.enabled) && !e.previousRoom.entrance.closedMarker.activeSelf,"Door architecture returns after clearing combat");
        Check(!Physics.SphereCast(e.checkpoint.position,0.3f,Vector3.forward,out var hit,2.5f,~0,QueryTriggerInteraction.Ignore),"Golden doorway has a clear physical route");
        Teleport(player,new Vector3(0,0.76f,26));yield return WaitTicks(0.2f);
        Check(e.IsFighting && e.gate.blocker.enabled && e.boss.GetCurrentHealth()==15,"Entering starts Boss combat, locks gate");
        Check(e.gate.closedMarker.activeSelf && Object.FindObjectsByType<RoomGate>(FindObjectsSortMode.None).All(g=>g.tallVisuals.All(r=>!r.enabled)),"Boss combat hides every tall doorway while retaining the gold marker");
        float z=e.boss.transform.position.z;
        yield return WaitTicks(1f);
        Check(e.boss.transform.position.z<z-0.5f,"Boss walks toward the player");
        Freeze(e.boss);
        Check(Object.FindObjectsByType<BossWarningCircle>(FindObjectsSortMode.None).Length==0,"No early warning circle");
        float deadline=Time.time+6;
        while(Object.FindFirstObjectByType<BossWarningCircle>()==null && Time.time<deadline)yield return new WaitForFixedUpdate();
        var first=Object.FindFirstObjectByType<BossWarningCircle>();
        Check(first!=null && Vector2.Distance(new Vector2(first.transform.position.x,first.transform.position.z),new Vector2(0,26))<0.01f,"Circle appears at player's current position after five seconds");
        var fixedPoint=first.transform.position;
        Teleport(player,new Vector3(4,0.76f,28));
        yield return WaitTicks(1.7f);
        Check(first.HasStruck && first.transform.position==fixedPoint && player.GetCurrentHealth()==5,"Leaving the fixed circle avoids its hit");
        yield return WaitTicks(1.9f);
        Check(first==null,"Circle disappears two seconds after detonation");
        deadline=Time.time+6;
        while(Object.FindFirstObjectByType<BossWarningCircle>()==null && Time.time<deadline)yield return new WaitForFixedUpdate();
        var second=Object.FindFirstObjectByType<BossWarningCircle>();Check(second!=null,"Boss repeats the circle on its five-second cycle");
        yield return WaitTicks(0.55f);
        var block=new MaterialPropertyBlock();second.disc.GetPropertyBlock(block);var pale=block.GetColor("_BaseColor");
        ScreenCapture.CaptureScreenshot("Temp/Boss-Warning-Light.png");
        Time.timeScale=0;yield return new WaitForSecondsRealtime(0.3f);
        Check(!second.HasStruck && player.GetCurrentHealth()==5,"Pausing also pauses warning damage");Time.timeScale=1;
        yield return WaitTicks(0.7f);
        Check(!second.HasStruck && player.GetCurrentHealth()==5,"Circle does not damage before 1.5 seconds");
        yield return WaitTicks(0.4f);
        second.disc.GetPropertyBlock(block);var dark=block.GetColor("_BaseColor");
        Check(second.HasStruck && player.GetCurrentHealth()==4 && dark.g<pale.g && dark.a>pale.a,"Circle darkens and deals exactly one HP at detonation");
        ScreenCapture.CaptureScreenshot("Temp/Boss-Warning-Dark.png");
        yield return WaitTicks(1.1f);
        Check(second!=null && player.GetCurrentHealth()==4,"Lingering dark circle causes no repeated damage");
        yield return WaitTicks(0.9f);Check(second==null,"Second circle expires after its two-second linger");
        e.boss.TakeDamage(2,Vector3.zero);Check(e.SummonedCount==0,"No summon before losing one fifth HP");
        e.boss.TakeDamage(1,Vector3.zero);yield return WaitTicks(0.05f);FreezeMinions(e);
        Check(e.SummonedCount==1 && e.GetComponentsInChildren<EnemyHealth>().Count(x=>x!=e.boss && x.maxHealth==3)==1,"80 percent HP summons one normal enemy");
        e.boss.TakeDamage(6,Vector3.zero);yield return WaitTicks(0.05f);FreezeMinions(e);
        Check(e.SummonedCount==3,"Burst damage crossing two thresholds summons two more enemies");
        e.boss.TakeDamage(3,Vector3.zero);yield return WaitTicks(0.05f);FreezeMinions(e);
        e.boss.TakeDamage(0.5f,Vector3.zero);
        Check(e.SummonedCount==4,"20 percent threshold summons the fourth minion; no duplicate waves");
        var ai=e.boss.GetComponent<EnemyAI>();
        e.boss.transform.position=player.transform.position+new Vector3(0,0,1.8f);Physics.SyncTransforms();
        ai.enabled=true;ai.animator.enabled=true;
        float before=player.GetCurrentHealth();deadline=Time.time+1.3f;
        while(player.GetCurrentHealth()==before && Time.time<deadline)yield return new WaitForFixedUpdate();
        Check(player.GetCurrentHealth()==before-1,"Boss melee animation hits for one HP");
        Freeze(e.boss);
        player.TakeTrapDamage(99);yield return WaitTicks(0.15f);
        Check(!e.IsFighting && e.gate.IsOpen && !e.boss.gameObject.activeSelf && e.SummonedCount==0 && Object.FindFirstObjectByType<BossWarningCircle>()==null,"Player death resets Boss, removes summons and warnings, opens gate");
        yield return WaitTicks(2.1f);
        Check(player.GetCurrentHealth()==player.maxHealth && Vector3.Distance(player.transform.position,e.checkpoint.position)<0.15f,"Player respawns safely outside gold door");
        player.playerMovement.enabled=false;player.GetComponent<Rigidbody>().isKinematic=true;
        Teleport(player,new Vector3(0,0.76f,26));yield return WaitTicks(0.15f);
        Check(e.IsFighting && e.boss.GetCurrentHealth()==15,"Re-entry starts a fresh full-health Boss fight");
        Freeze(e.boss);e.boss.TakeDamage(3,Vector3.zero);yield return WaitTicks(0.05f);FreezeMinions(e);
        e.boss.TakeDamage(99,Vector3.zero);yield return null;
        Check(e.IsComplete && e.victory.IsComplete && Time.timeScale==1,"Boss death completes combat without pausing or a victory page");
        Check(e.GetComponentsInChildren<EnemyHealth>().Length==0 && Object.FindFirstObjectByType<PauseMenu>().enabled,"Boss clear removes minions and leaves pause controls available");
        ScreenCapture.CaptureScreenshot("Temp/Boss-Victory.png");
        yield return new WaitForSecondsRealtime(0.3f);
        Check(!Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(x=>x.name=="Boss health" || x.name=="Victory screen"),"No Boss health bar or victory page exists");
    }
}
