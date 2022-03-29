using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Table used in the self assessment task.
/// </summary>
public class SelfAssessmentTable : MonoBehaviour
{
    public List<Transform> sprayGuns;
    public Collider basket;
    
    private List<MeshRenderer> _meshList;
    
    private void Awake()
    {
        _meshList = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    public int GetNumberOfSprayGuns()
    {
        int result = 0;
        foreach (Transform sprayGun in sprayGuns)
            if (basket.bounds.Contains(sprayGun.position))
                result++;
        return result;
    }

    private void Start()
    {
        _meshList.ForEach(mesh => mesh.FadeInMaterials(0.5f));
    }

    public void FadeOut()
    {
        _meshList.ForEach(mesh => mesh.FadeOutMaterials(0.5f, () =>
        {
            sprayGuns.ForEach(s => Destroy(s.gameObject));
            Destroy(gameObject);
        }));
    }
}
