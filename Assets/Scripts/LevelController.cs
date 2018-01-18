using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    public static LevelController current;

    public static Canvas userInterface;
    public static CanvasGroup userInterfaceGroup;
    public static CanvasGroup onPlayerHitCanvasGroup;

    public static bool isDialogueOpen;
    public static Image dialogueImage;

    private void Awake() {
        current = this;
        
        userInterface = GameObject.Find("User Interface").GetComponent<Canvas>();
        userInterfaceGroup = userInterface.GetComponent<CanvasGroup>();
        onPlayerHitCanvasGroup = GameObject.Find("On Player Hit").GetComponent<CanvasGroup>();
        dialogueImage = GameObject.Find("Dialogue Box").GetComponent<Image>();

        dialogueImage.gameObject.SetActive(false);
    }

    private void Update() {
        // Dialogue Controller
        if (!isDialogueOpen) {

        }
    }

    public static Projectile CreateProjectileTowardsDirection(GameObject type, Vector3 position, Vector3 targetPosition) {
        GameObject projectile = Instantiate<GameObject>(type, position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        Projectile projectileComp = projectile.GetComponent<Projectile>();
        float force = projectileComp.projectileData.force;
        Vector3 heading = targetPosition - position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;

        projectile.transform.localScale = new Vector3(direction.x, 1, 1);
        projectileRb.AddForce(direction * force, ForceMode2D.Impulse);

        return projectileComp;
    }

    public static void SetProjectileEnemyAgainst(Projectile projectile, string tag) {
        projectile.AddEnemyTag(tag);
    }

    public static void RemoveProjectileEnemyAgainst(Projectile projectile, string tag) {
        projectile.RemoveEnemyTag(tag);
    }

    public static void ShowDialogue() {
        isDialogueOpen = true;
        dialogueImage.gameObject.SetActive(true);

        Game.PauseGame();
    }

    public static void HideDialogue() {
        isDialogueOpen = false;
        dialogueImage.gameObject.SetActive(false);

        Game.ResumeGame();
    }

    public static void FlashScreen() {
        onPlayerHitCanvasGroup.alpha = 1;

        current.StartCoroutine(StartFade(onPlayerHitCanvasGroup, 1));
    }

    static IEnumerator StartFade(CanvasGroup cg, float time) {
        //when the canvas is still visible
        while (cg.alpha > 0) {
            //decrease the opacity over time
            cg.alpha -= Time.deltaTime / time;
            //updates per frame
            yield return null;
        }

        //tell the coroutine it has finished fading
        yield return null;
    }
}
