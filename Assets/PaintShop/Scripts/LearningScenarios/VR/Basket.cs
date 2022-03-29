using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents a basket used for the sorting task. Turns green if the correct objects are placed in the basket.
/// </summary>
public class Basket : MonoBehaviour
{
    [HideInInspector] public HashSet<GameObject> correctObjects;
    [HideInInspector] public UnityAction onCorrect;

    private MeshRenderer _meshRenderer;
    private HashSet<GameObject> _currentObjects;
    private bool _isCorrect;

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void OnEnable()
    {
        _currentObjects = new HashSet<GameObject>();
        correctObjects = new HashSet<GameObject>();
        _isCorrect = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.GetComponent<Canister>())
            _currentObjects.Add(other.transform.parent.gameObject);
        CheckCorrect();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.GetComponent<Canister>())
            _currentObjects.Remove(other.transform.parent.gameObject);
        CheckCorrect();
    }

    private void CheckCorrect()
    {
        bool newCorrect = _currentObjects.SetEquals(correctObjects);
        if (newCorrect != _isCorrect)
            StartCoroutine(Lerp.Color(_meshRenderer, _meshRenderer.material.color,
                _currentObjects.SetEquals(correctObjects) ? Color.green : Color.red, 0.5f));
        _isCorrect = newCorrect;
        if (_isCorrect)
            onCorrect?.Invoke();
    }
}