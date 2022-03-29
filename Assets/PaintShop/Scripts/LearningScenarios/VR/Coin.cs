using System;
using System.Collections.Generic;
using QuickOutline;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A configurable coin which can be collected by the user.
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Appearance Settings")] public CoinType coinType;
    public Material coinMaterial;

    [Header("Rotation Settings")] [Range(0, 250)]
    public int rotationSpeed;
    public bool clockwise;

    [Header("Hover Settings")] [Range(0.0f, 1f)]
    public float hoverSpeed;
    [Range(0.0f, 1f)] public float hoverVerticalRange;

    [Header("Fade Settings")]
    public float fadeTime = 0.5f;
    public float heightIncrease = 0.5f;
    public int finalRotationSpeed = 1000;

    private Outline _outline;
    private bool _highlighted;
    private SprayGun _sprayGun;
    private float _distanceDelta;
    private int _directionFactor;
    private bool _isTriggered;
    private UnityAction _doAfterFadeOut;
    private UnityAction _doAfterSelection;
    private float _actualRotationSpeed;
    private AudioSource _audioSource;
    private List<Renderer> _meshList;

    private void Awake()
    {
        _outline = GetComponentInChildren<Outline>();
        _audioSource = GetComponent<AudioSource>();

        // avoids that the original material is changed
        coinMaterial = new Material(coinMaterial);
        _meshList = new List<Renderer>();
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
            if (child.gameObject.name != "Visual Timer")
            {
                Renderer mesh = child.gameObject.GetComponent<Renderer>();
                _meshList.Add(mesh);
                mesh.material = coinMaterial;
            }
        }

        children[0].gameObject.GetComponent<Renderer>().material = coinMaterial;
        children[1].SetActive(coinType == CoinType.Left);
        children[2].SetActive(coinType == CoinType.LeftLeft);
        children[3].SetActive(coinType == CoinType.Pause);
        children[4].SetActive(coinType == CoinType.Right);
        children[5].SetActive(coinType == CoinType.RightRight);
        children[6].SetActive(coinType == CoinType.Star);

        // Set SprayGun
        ApplicationController.Instance.ExecuteAfterSprayGunSpawn(() =>
        {
            _sprayGun = ApplicationController.Instance.sprayGun;
        });
    }

    private void Update()
    {
        Animation();
    }

    private void Reset()
    {
        _distanceDelta = 0;
        _directionFactor = 1;
        _outline.enabled = false;
        _actualRotationSpeed = rotationSpeed;
    }

    /// <summary>
    /// Is called from the spray gun if the coin is touched.
    /// </summary>
    public void DoInteract()
    {
        _outline.enabled = true;

        if (_sprayGun.GetSprayingValue() > 0.05f)
            TriggerCoin();
    }

    public void StopInteract()
    {
        _outline.enabled = false;
    }

    /// <summary>
    /// If the muzzle of the paint gun enters the collider the CurrentCoinController of the ApplicationController is
    /// set. This indicates, that the paint gun touches a coin which can be collected. If OnTriggerEnter is used
    /// instead of OnTriggerStay currentTouchedCoin will set to null if the spray gun leaves the mesh collider of
    /// the coin.
    /// </summary>
    protected void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("SprayGun"))
            return;

        ApplicationController.Instance.currentTouchedCoin = this;
    }

    /// <summary>
    /// Resets everything if the muzzle of the paint gun leaves the collider.
    /// </summary>
    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("SprayGun"))
            return;

        ApplicationController.Instance.currentTouchedCoin = null;
        StopInteract();
    }

    /// <summary>
    /// Manages hovering and rotation.
    /// </summary>
    private void Animation()
    {
        if (clockwise)
            transform.Rotate(0, _actualRotationSpeed * Time.deltaTime, 0);
        else
            transform.Rotate(0, -(_actualRotationSpeed * Time.deltaTime), 0);

        transform.Translate(0, hoverSpeed * Time.deltaTime * _directionFactor, 0);
        _distanceDelta = _distanceDelta + (hoverSpeed * Time.deltaTime * _directionFactor);

        if (_distanceDelta >= hoverVerticalRange || _distanceDelta <= -hoverVerticalRange)
        {
            _distanceDelta = 0;
            _directionFactor = -_directionFactor;
        }
    }

    /// <summary>
    /// Collects the coin and animates it.
    /// </summary>
    private void TriggerCoin()
    {
        if (!CoinController.Instance.coinSelectable)
            return;

        CoinController.Instance.coinSelectable = false;
        _doAfterSelection.Invoke();
        FadeOut(true, _doAfterFadeOut);
    }

    public void FadeIn(float currentUserHeight, UnityAction afterSelection, UnityAction afterFadeOut)
    {
        _doAfterFadeOut = afterFadeOut;
        _doAfterSelection = afterSelection;

        if (gameObject.activeSelf)
            return;

        gameObject.SetActive(true);
        transform.position = new Vector3(transform.position.x, currentUserHeight, transform.position.z);
        Reset();
        _meshList.ForEach(m => m.FadeInMaterial(fadeTime));
    }

    public void FadeOut(bool selected = false, UnityAction afterFadeOut = null)
    {
        if (selected)
        {
            CoinController.Instance.FadeOutCoins(this);
            _audioSource.Play();
            StartCoroutine(Lerp.Float(f => _actualRotationSpeed = f, _actualRotationSpeed, finalRotationSpeed, fadeTime));
            StartCoroutine(Lerp.Float(
                f => transform.position = new Vector3(transform.position.x, f, transform.position.z),
                transform.position.y, transform.position.y + heightIncrease, fadeTime));
        }

        _meshList.FadeOutMaterial(fadeTime, () =>
        {
            if (selected)
                ApplicationController.Instance.currentTouchedCoin = null;
            gameObject.SetActive(false);
            CoinController.Instance.coinSelectable = true;
            afterFadeOut?.Invoke();
        });
    }
}

[Serializable]
public enum CoinType
{
    Left,
    LeftLeft,
    Pause,
    Right,
    RightRight,
    Star
}