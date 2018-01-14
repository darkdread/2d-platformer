using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    public static LevelController current;
    private List<GameObject> projectileList = new List<GameObject>();
    public static Canvas userInterface;
    public static CanvasGroup userInterfaceGroup;

    private void Awake() {
        current = this;

        userInterface = GameObject.Find("User Interface").GetComponent<Canvas>();
        userInterfaceGroup = userInterface.GetComponent<CanvasGroup>();
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
        //projectileList.Add(projectile);

        return projectileComp;
    }

    public static void SetProjectileEnemyTowards(Projectile projectile, string tag) {
        projectile.AddEnemyTag(tag);
    }

    public static void FlashScreen() {
        userInterfaceGroup.alpha = 1;

        current.StartCoroutine(StartFade(userInterfaceGroup, 1));
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
