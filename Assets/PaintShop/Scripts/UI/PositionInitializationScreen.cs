using System.Collections;
using TMPro;
using translator;
using UnityEngine;

/// <summary>
/// Screen for initialization of the position of the spray gun.
/// </summary>
public class PositionInitializationScreen : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public PaintShopMonitorController paintShopMonitorController;
    public GameObject sprayGunGhost;
    public GameObject sprayGunGhostCenter;
    public GameObject repeatButton;
    public GameObject finishButton;

    private bool pressed;
    private int phase;
    private bool blocked = true;

    private void Update()
    {
        // setting position is blocked in phase 1 as well as when first entering the screen due to accidental position setting otherwise
        if (blocked)
            return;

        if (ApplicationController.Instance.sprayGun.GetSprayingValue() > 0.1)
        {
            if (!pressed)
            {
                SetPosition();
                pressed = true;
            }
        }
        else
        {
            pressed = false;
        }
    }

    private void OnEnable()
    {
        blocked = true;
        ChangePhase(0);

        // waits before the setting is unblocked
        StartCoroutine(BlockAfterWaitForSeconds(1));
    }

    private void OnDisable()
    {
        blocked = true;
    }

    /// <summary>
    /// Resets the process.
    /// </summary>
    public void Repeat()
    {
        ChangePhase(0);
        blocked = false;
    }

    public void Finished()
    {
        paintShopMonitorController.ShowDefaultScreen();
    }

    private void SetPosition()
    {
        // sprayGunGhost has a different center from sprayGun, so for the position sprayGunGhostCenter is used instead
        ApplicationController.Instance.sprayGun.gameObject.transform.position = sprayGunGhostCenter.transform.position;

        Vector3 eulerRotation = new Vector3(sprayGunGhost.transform.eulerAngles.x + 90,
            sprayGunGhost.transform.eulerAngles.y, sprayGunGhost.transform.eulerAngles.z);
        ApplicationController.Instance.sprayGun.transform.eulerAngles = eulerRotation;

        ApplicationController.Instance.sprayGun.WriteValuesToFile();

        ChangePhase(1);
    }

    private void ChangePhase(int nextPhase)
    {
        phase = nextPhase;
        switch (phase)
        {
            case 0:
                instructionText.text =
                    TranslationController.Instance.Translate("paint-shop-initialize-position-description-1");
                repeatButton.SetActive(false);
                finishButton.SetActive(false);
                sprayGunGhost.SetActive(true);
                ApplicationController.Instance.sprayGun.SetTool(SprayGun.Tool.None);
                break;
            // Phase 1: SprayGun is visible again and the user may choose to redo the process
            case 1:
                instructionText.text =
                    TranslationController.Instance.Translate("paint-shop-initialize-position-description-2");
                repeatButton.SetActive(true);
                finishButton.SetActive(true);
                sprayGunGhost.SetActive(false);
                ApplicationController.Instance.sprayGun.SetTool(SprayGun.Tool.SprayGun);
                blocked = true;
                break;
        }
    }

    private IEnumerator BlockAfterWaitForSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        blocked = false;
    }
}
