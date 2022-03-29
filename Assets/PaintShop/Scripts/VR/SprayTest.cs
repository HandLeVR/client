using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// The spray test paper.
/// </summary>
public class SprayTest : MonoBehaviour
{
    public float minSprayDurationSprayTest = 0.5f;
    public float maxSprayDistanceSprayTest = 0.3f;

    [Tooltip("The probability with which a faulty cookie arises")]
    public float errorRate = 0.2f;

    [Tooltip("The possible faulty cookies that can arise.")]
    public Texture[] faultyCookies;

    [Tooltip("The audio played on a faulty cookie.")]
    public AudioClip[] audioClips;

    [Header("Debug")] [Tooltip("If checked the error indicated by Error Index will be forced.")]
    public bool overwriteError;

    [Tooltip("Determines the index of the error. Should be a value between 0 and the number of faulty cookies.")]
    public int errorIndex;

    private float _originalEmissionRate;
    private float _sprayStartTime;
    private int _faultyCookieIndex;
    private Texture _originalCookie;
    private bool _sprayTestStarted;
    private bool _flutterSpray;
    private bool _fluttering;

    private void Update()
    {
        if (ApplicationController.Instance.sprayGun == null ||
            ApplicationController.Instance.sprayGun.GetActualSprayingValue() <= 0)
            _sprayStartTime = Time.realtimeSinceStartup;
    }

    private void LateUpdate()
    {
        if (_fluttering && ApplicationController.Instance.sprayGun.spray.isPlaying &&
            ApplicationController.Instance.sprayGun.GetActualSprayingValue() >= 0.5f)
        {
            var sprayCone = ApplicationController.Instance.sprayGun.sprayCone;
            var audioSource = ApplicationController.Instance.sprayGun.audioSource;
            sprayCone.currentVisibility = audioSource.volume;
        }
    }

    /// <summary>
    /// Causes the spray to flutter.
    /// </summary>
    private void FlutterSpray()
    {
        SprayGun sprayGun = ApplicationController.Instance.sprayGun;
        float time = Random.Range(0.05f, 0.1f);
        ParticleSystem.EmissionModule primEmission = sprayGun.spray.emission;
        primEmission.rateOverTime = _flutterSpray ? _originalEmissionRate / 2 : _originalEmissionRate;
        StartCoroutine(Lerp.Volume(sprayGun.audioSource, _flutterSpray ? 0.5f : 1f, time, () =>
        {
            _fluttering = true;
            _flutterSpray = !_flutterSpray;
            if (_sprayTestStarted)
                FlutterSpray();
        }));
    }

    public void InitSprayTest(List<int> possibleErrors = null)
    {
        _sprayTestStarted = true;
        _originalCookie = null;
        _faultyCookieIndex = 6;
        if (Random.value > errorRate && !overwriteError)
            return;

        if (possibleErrors == null)
            _faultyCookieIndex = overwriteError ? errorIndex : Random.Range(0, faultyCookies.Length);
        else
        {
            if (possibleErrors.Count == 0)
                return;
            _faultyCookieIndex = possibleErrors[Random.Range(0, possibleErrors.Count)];
        }

        _originalCookie = PaintController.Instance.cookie;
        PaintController.Instance.cookie = faultyCookies[_faultyCookieIndex];
        _originalEmissionRate = ApplicationController.Instance.sprayGun.spray.emission.rateOverTime.constant;
        if (_faultyCookieIndex == faultyCookies.Length - 1)
            FlutterSpray();
        // scaling irregular cookies can lead to bad results
        ApplicationController.Instance.sprayGun.scaleCookie = false;
    }

    public void ResetSprayTest()
    {
        if (_originalCookie != null)
            PaintController.Instance.cookie = _originalCookie;
        ApplicationController.Instance.sprayGun.scaleCookie = true;
        _sprayTestStarted = false;
        _fluttering = false;
    }

    public bool SprayTestDone()
    {
        if (!_sprayTestStarted)
            return false;

        SprayGun sprayGun = ApplicationController.Instance.sprayGun;
        float sprayDuration = Time.realtimeSinceStartup - _sprayStartTime;

        bool hitSprayTestPaper = Physics.Raycast(sprayGun.pinSpotOrigin.position, sprayGun.pinSpotOrigin.forward,
            out RaycastHit hitCheck, Mathf.Infinity,
            LayerMask.GetMask("Drawable"));
        float sprayDistance = Mathf.Infinity;
        if (hitSprayTestPaper && hitCheck.collider.gameObject.name.Equals(name))
            sprayDistance = Vector3.Distance(sprayGun.pinSpotOrigin.position, hitCheck.point);

        return sprayDuration > minSprayDurationSprayTest && sprayDistance <= maxSprayDistanceSprayTest;
    }

    public AudioClip GetErrorAudioClip()
    {
        if (_faultyCookieIndex == 6)
            return null;
        return audioClips[_faultyCookieIndex];
    }
}