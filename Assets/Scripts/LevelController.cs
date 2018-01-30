using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    public static LevelController current;

    public Canvas userInterface;
    public CanvasGroup userInterfaceGroup;
    public CanvasGroup onPlayerHitCanvasGroup;
    public Player player;

    public static bool isDialogueOpen;
    public Image dialogueImage;

    private void Awake() {
        current = this;


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
        float force = projectileComp.projectileData.speed;
        Vector3 heading = targetPosition - position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;

        // local scale z is 0 to detect that it is created with localscale.x
        projectile.transform.localScale = new Vector3(direction.x, 1, 0);
        projectileRb.AddForce(direction * force, ForceMode2D.Impulse);
        projectile.transform.SetParent(Game.gameHolder);

        return projectileComp;
    }

    public static Projectile CreateProjectileTowardsAngle(GameObject type, Vector3 position, Vector3 targetPosition, float angle) {
        GameObject projectile = Instantiate<GameObject>(type, position, Quaternion.Euler(new Vector3(0, 0, angle)));
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        Projectile projectileComp = projectile.GetComponent<Projectile>();
        float force = projectileComp.projectileData.speed;
        Vector3 heading = targetPosition - position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;
        
        projectile.transform.localScale = new Vector3(1, 1, 1);
        projectileRb.AddForce(direction * force, ForceMode2D.Impulse);
        projectile.transform.SetParent(Game.gameHolder);

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
        current.dialogueImage.gameObject.SetActive(true);

        Game.PauseGame();
    }

    public static void HideDialogue() {
        isDialogueOpen = false;
        current.dialogueImage.gameObject.SetActive(false);

        Game.ResumeGame();
    }

    public static void FlashScreen() {
        current.onPlayerHitCanvasGroup.alpha = 1;

        current.StartCoroutine(StartFade(current.onPlayerHitCanvasGroup, 1));
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
