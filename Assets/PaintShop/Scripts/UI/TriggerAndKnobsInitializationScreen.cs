using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Collections;
using translator;
using UnityEngine.InputSystem;

/// <summary>
/// Allows the initialization of the trigger and the knobs of the real spray gun.
/// </summary>
public class TriggerAndKnobsInitializationScreen : MonoBehaviour
{
    public GameObject instructions;
    public GameObject buttons;
    public GameObject description;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI countDownText;
    public PaintShopMonitorController paintShopMonitorController;

    private readonly List<Action> _delegateList = new();

    private List<string> _instructionList;

    private void Awake()
    {
        _instructionList = new()
        {
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-1"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-2"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-3"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-4"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-5"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-6"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-7"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-8"),
            TranslationController.Instance.Translate("paint-shop-initialize-spray-gun-9")
        };
    }

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame && buttons.activeSelf)
            StartInitialization();
    }

    void OnEnable()
    {
        instructions.SetActive(false);
        buttons.SetActive(true);
        description.SetActive(true);
    }

    public void StartInitialization()
    {
        instructions.SetActive(true);
        buttons.SetActive(false);
        description.SetActive(false);
        
        if (ApplicationController.Instance.RealSprayGunConnected())
            InitRealSprayGun();
        else
            InitControllerSprayGun();
    }

    private void InitRealSprayGun()
    {
        RealSprayGun sprayGun = ApplicationController.Instance.sprayGun.GetComponent<RealSprayGun>();
        _delegateList.Add(() => sprayGun.SetStartVal());
        _delegateList.Add(() => sprayGun.SetAirVal());
        _delegateList.Add(() => sprayGun.SetFullVal());
        _delegateList.Add(() => sprayGun.SetMinMaterialRegulationValue());
        _delegateList.Add(() => sprayGun.SetMaxMaterialRegulationValue());
        _delegateList.Add(() => sprayGun.SetMinWideStreamRegulationValue());
        _delegateList.Add(() => sprayGun.SetMaxWideStreamRegulationValue());
        _delegateList.Add(() => sprayGun.SetMinAirMicrometerValue());
        _delegateList.Add(() => sprayGun.SetMaxAirMicrometerValue());

        StartCoroutine(ShowInstructions(_instructionList));
    }

    private void InitControllerSprayGun()
    {
        SprayGun sprayGun = ApplicationController.Instance.sprayGun.GetComponent<SprayGun>();
        _delegateList.Add(() => sprayGun.SetStartVal());
        _delegateList.Add(() => sprayGun.SetAirVal());
        _delegateList.Add(() => sprayGun.SetFullVal());

        StartCoroutine(ShowInstructions(_instructionList.GetRange(0, 3)));
    }

    private IEnumerator ShowInstructions(List<string> instructions)
    {
        ApplicationController.Instance.sprayGunIsInitializing = true;
        for (int i = 0; i < instructions.Count; i++)
        {
            instructionText.text = instructions[i];
            // countdown from 5
            for (int j = 10; j > 0; j--)
            {
                int seconds = j;
                countDownText.text = seconds.ToString();
                yield return new WaitForSeconds(1);
            }

            _delegateList[i].Invoke();
        }

        ApplicationController.Instance.sprayGun.WriteValuesToFile();
        paintShopMonitorController.ShowDefaultScreen();
        ApplicationController.Instance.sprayGunIsInitializing = false;
    }
}