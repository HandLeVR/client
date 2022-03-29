using UnityEngine;
using System.IO.Ports;
using System.IO;
using System;

/// <summary>
/// Used when a real or printed spray gun is connected.
/// </summary>
public class RealSprayGun : SprayGun
{
    // the knobs used to control the spray
    public Transform materialRegulationKnob;
    public Transform wideStreamRegulationKnob;
    public Transform airMicrometerKnob;

    // max values for the knobs
    public float maxMaterialRegulationKnobRotation = 360f;
    public float maxWideStreamRegulationKnobRotation = 360f;
    public float maxAirMicrometerKnobRotation = 90f;

    // the min difference between to frames to virtually rotate a knob
    public float knobAnimationTolerance = 1;

    // current values of the trigger and the knobs.
    private float _triggerValue;
    private float _materialRegulationValue;
    private float _wideStreamRegulationValue;
    private float _airMicrometerValue;

    // min and max values for the knobs determined by the initialization file
    private float _minMaterialRegulationValue;
    private float _maxMaterialRegulationValue;
    private float _minWideStreamRegulationValue;
    private float _maxWideStreamRegulationValue;
    private float _minAirMicrometerValue;
    private float _maxAirMicrometerValue;

    // rotation of the knobs on start of the application
    private Quaternion _materialRegulationKnobStartRotation;
    private Quaternion _wideStreamRegulationKnobStartRotation;
    private Quaternion _airMicrometerKnobStartRotation;

    // the usb port
    private SerialPort _sp;

    // current values of the trigger and the knobs
    private float _lastMaterialRegulationValue;
    private float _lastWideStreamRegulationValue;
    private float _lastAirMicrometerValue;
    private float _airTriggerValue01;

    // determines whether there was an update of the values
    private bool _valuesUpdate;

    // path to the initialization file
    private string _realSprayGunValuesFile;

    public override void Start()
    {
        _sp = new SerialPort(ApplicationController.Instance.realSprayGunCOMPortName, 19200);
        _sp.ReadTimeout = 25;
        _sp.WriteTimeout = 25;
        _sp.Open();

        _realSprayGunValuesFile = Application.dataPath + "/StreamingAssets/RealSprayGunCalibration.json";

        // read the calibration file and set variables 
        ReadValuesFromFile();
        InvokeRepeating(nameof(PullArduinoValues), 0, 0.05f);

        _materialRegulationKnobStartRotation = materialRegulationKnob.localRotation;
        _wideStreamRegulationKnobStartRotation = wideStreamRegulationKnob.localRotation;
        _airMicrometerKnobStartRotation = airMicrometerKnob.localRotation;

        _lastMaterialRegulationValue = _materialRegulationValue;
        _lastWideStreamRegulationValue = _wideStreamRegulationValue;
        _lastAirMicrometerValue = _airMicrometerValue;
        _airTriggerValue01 = MapTo01(airTriggerValue, startTriggerValue, fullTriggerValue);
    }

    /// <summary>
    /// Closes the serial port.
    /// </summary>
    public void Disconnect()
    {
        if (_sp != null && _sp.IsOpen)
            _sp.Close();
    }

    /// <summary>
    /// Disconnects the spray gun if not already done through the ApplicationController.
    /// For some reason sometimes the spray gun is destroyed first and sometimes the ApplicationController.
    /// </summary>
    public void OnDisable()
    {
        Disconnect();
    }

    /// <summary>
    /// Gets the current state of the trigger and the knobs from the arduino.
    /// </summary>
    void PullArduinoValues()
    {
        try
        {
            _sp.WriteLine("v");
            string stringValue = _sp.ReadLine();
            string[] values = stringValue.Split(' ');
            _triggerValue = Convert.ToSingle(values[0]);

            if (values.Length > 1)
            {
                _materialRegulationValue = Convert.ToSingle(values[1]);
                _wideStreamRegulationValue = Convert.ToSingle(values[2]);
                _airMicrometerValue = Convert.ToSingle(values[3]);
            }

            _valuesUpdate = true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public new void SetStartVal()
    {
        startTriggerValue = _triggerValue;
    }

    public new void SetAirVal()
    {
        airTriggerValue = _triggerValue;
    }

    public new void SetFullVal()
    {
        fullTriggerValue = _triggerValue;
    }

    protected override void AnimateSprayGun()
    {
        // animate trigger by considering material regulation value (trigger is blocked if knob is not fully open)
        float sprayRangeTriggerValue =
            Mathf.Min(MapTo01(_triggerValue, startTriggerValue, fullTriggerValue), _airTriggerValue01);
        float airRangeTriggerValue = MapTo01(RestrictValueByMaterialRegulation(GetTriggerValue()), 0, 1, 0,
            1 - _airTriggerValue01);
        TriggerRotationPoint.localRotation =
            _initialTriggerRotation *
            Quaternion.Euler(0, maxTriggerRotation * (sprayRangeTriggerValue + airRangeTriggerValue), 0);

        // animate knobs
        if (Mathf.Abs(_lastMaterialRegulationValue - _materialRegulationValue) > knobAnimationTolerance)
            materialRegulationKnob.localRotation = _materialRegulationKnobStartRotation *
                                                   Quaternion.Euler(
                                                       maxMaterialRegulationKnobRotation * GetMaterialRegulationValue(),
                                                       0, 0);
        if (Mathf.Abs(_lastWideStreamRegulationValue - _wideStreamRegulationValue) > knobAnimationTolerance)
            wideStreamRegulationKnob.localRotation = _wideStreamRegulationKnobStartRotation *
                                                     Quaternion.Euler(0,
                                                         maxWideStreamRegulationKnobRotation *
                                                         GetWideStreamRegulationValue(), 0);
        if (Mathf.Abs(_lastAirMicrometerValue - _airMicrometerValue) > knobAnimationTolerance)
            airMicrometerKnob.localRotation = _airMicrometerKnobStartRotation *
                                              Quaternion.Euler(maxAirMicrometerKnobRotation * GetAirMicrometerValue(),
                                                  0, 0);

        _lastMaterialRegulationValue = _materialRegulationValue;
        _lastWideStreamRegulationValue = _wideStreamRegulationValue;
        _lastAirMicrometerValue = _airMicrometerValue;
    }


    public void SetMinMaterialRegulationValue()
    {
        _minMaterialRegulationValue = _materialRegulationValue;
    }

    public void SetMaxMaterialRegulationValue()
    {
        _maxMaterialRegulationValue = _materialRegulationValue;
    }

    public void SetMinWideStreamRegulationValue()
    {
        _minWideStreamRegulationValue = _wideStreamRegulationValue;
    }

    public void SetMaxWideStreamRegulationValue()
    {
        _maxWideStreamRegulationValue = _wideStreamRegulationValue;
    }

    public void SetMinAirMicrometerValue()
    {
        _minAirMicrometerValue = _airMicrometerValue;
    }

    public void SetMaxAirMicrometerValue()
    {
        _maxAirMicrometerValue = _airMicrometerValue;
    }

    class SprayGunValues
    {
        public float startTriggerValue;
        public float airTriggerValue;
        public float fullTriggerValue;
        public float minMaterialRegulationValue;
        public float maxMaterialRegulationValue;
        public float minWideStreamRegulationValue;
        public float maxWideStreamRegulationValue;
        public float minAirMicrometerValue;
        public float maxAirMicrometerValue;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    public override void WriteValuesToFile()
    {
        SprayGunValues sprayGunValues = new SprayGunValues
        {
            startTriggerValue = startTriggerValue,
            airTriggerValue = airTriggerValue,
            fullTriggerValue = fullTriggerValue,
            minMaterialRegulationValue = _minMaterialRegulationValue,
            maxMaterialRegulationValue = _maxMaterialRegulationValue,
            minWideStreamRegulationValue = _minWideStreamRegulationValue,
            maxWideStreamRegulationValue = _maxWideStreamRegulationValue,
            minAirMicrometerValue = _minAirMicrometerValue,
            maxAirMicrometerValue = _maxAirMicrometerValue,
            localRotation = transform.localRotation,
            localPosition = transform.localPosition
        };
        File.WriteAllText(_realSprayGunValuesFile, JsonUtility.ToJson(sprayGunValues));
        _airTriggerValue01 = MapTo01(airTriggerValue, startTriggerValue, fullTriggerValue);
    }

    public override void ReadValuesFromFile()
    {
        if (File.Exists(_realSprayGunValuesFile))
        {
            SprayGunValues sprayGunValues = JsonUtility.FromJson<SprayGunValues>(
                File.ReadAllText(_realSprayGunValuesFile));
            startTriggerValue = sprayGunValues.startTriggerValue;
            airTriggerValue = sprayGunValues.airTriggerValue;
            fullTriggerValue = sprayGunValues.fullTriggerValue;
            _minMaterialRegulationValue = sprayGunValues.minMaterialRegulationValue;
            _maxMaterialRegulationValue = sprayGunValues.maxMaterialRegulationValue;
            _minWideStreamRegulationValue = sprayGunValues.minWideStreamRegulationValue;
            _maxWideStreamRegulationValue = sprayGunValues.maxWideStreamRegulationValue;
            _minAirMicrometerValue = sprayGunValues.minAirMicrometerValue;
            _maxAirMicrometerValue = sprayGunValues.maxAirMicrometerValue;
            transform.localRotation = sprayGunValues.localRotation;
            transform.localPosition = sprayGunValues.localPosition;
        }
    }

    /// <summary>
    /// Restricts the trigger value in dependence of the material regulation and the pressure value.
    /// </summary>
    public override float GetSprayingValue()
    {
        float pressureTriggerValue = Mathf.Lerp(0.2f, 1, GetAirMicrometerValue()) * GetTriggerValue();
        return RestrictValueByMaterialRegulation(pressureTriggerValue);
    }

    private float RestrictValueByMaterialRegulation(float value)
    {
        return Mathf.Min(value, Mathf.Lerp(0.1f, 1, GetMaterialRegulationValue()));
    }

    protected override float GetMaterialRegulationValue()
    {
        if (Mathf.Abs(_maxMaterialRegulationValue - _minMaterialRegulationValue) <= 1)
            return base.GetMaterialRegulationValue();
        return MapTo01(_materialRegulationValue, _minMaterialRegulationValue, _maxMaterialRegulationValue);
    }

    public override float GetWideStreamRegulationValue()
    {
        if (Mathf.Abs(_maxWideStreamRegulationValue - _minWideStreamRegulationValue) <= 1)
            return base.GetWideStreamRegulationValue();
        return MapTo01(_wideStreamRegulationValue, _minWideStreamRegulationValue, _maxWideStreamRegulationValue);
    }

    protected override float GetAirMicrometerValue()
    {
        if (Mathf.Abs(_maxAirMicrometerValue - _minAirMicrometerValue) <= 1)
            return base.GetAirMicrometerValue();
        return MapTo01(_airMicrometerValue, _minAirMicrometerValue, _maxAirMicrometerValue);
    }

    public override float GetTriggerValue()
    {
        if (!_valuesUpdate)
            return 0;
        return MapTo01(_triggerValue, airTriggerValue, fullTriggerValue);
    }

    /// <summary>
    /// Maps a value in a given range to another range (0 to 1 normally).
    /// </summary>
    private float MapTo01(float currentValue, float from1, float to1, float from2 = 0, float to2 = 1)
    {
        return Mathf.Clamp((currentValue - from1) / (to1 - from1) * (to2 - from2) + from2, Mathf.Min(from2, to2),
            Mathf.Max(from2, to2));
    }
}