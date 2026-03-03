using UnityEngine;
using System.Collections;

public class PlayerCollisionHandler : MonoBehaviour
{
    public enum Difficulty { Easy, Hard }
    public Difficulty currentDifficulty;

    private EasyCarController easyController;
    private ThesisCarController thesisController;

    private bool isStunned = false;

    void Start()
    {
        easyController = GetComponent<EasyCarController>();
        thesisController = GetComponent<ThesisCarController>();
    }

    public void Stun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // Disable movement scripts
        if (easyController != null)
            easyController.enabled = false;

        if (thesisController != null)
            thesisController.enabled = false;

        yield return new WaitForSeconds(duration);

        // Re-enable both
        if (easyController != null)
            easyController.enabled = true;

        if (thesisController != null)
            thesisController.enabled = true;

        isStunned = false;
    }
}