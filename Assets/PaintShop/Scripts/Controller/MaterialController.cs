using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages materials to allow switching between transparent and opaque modes. Different materials for the transparent
/// and opaque version of an object are needed because switching the surface type on runtime caused unsolvable issues. 
/// </summary>
public class MaterialController : Singleton<MaterialController>
{
    public List<Material> opaqueMaterials;
    public List<Material> transparentMaterials;
    public List<Material> dontChangeMaterials;

    private Dictionary<string, Material> _opaqueToTransparent;
    private Dictionary<string, Material> _transparentToOpaque;
    private Dictionary<string, Material> _dontChange;
    
    private void Awake()
    {
        _opaqueToTransparent = new Dictionary<string, Material>();
        _transparentToOpaque = new Dictionary<string, Material>();
        _dontChange = new Dictionary<string, Material>();
        for (int i = 0; i < opaqueMaterials.Count; i++)
        {
            _opaqueToTransparent[opaqueMaterials[i].name] = transparentMaterials[i];
            _transparentToOpaque[transparentMaterials[i].name] = opaqueMaterials[i];
        }
        foreach (Material material in dontChangeMaterials)
            _dontChange[material.name] = material;
    }

    /// <summary>
    /// Sets all materials of a Renderer transparent.
    /// </summary>
    public void SetMaterialsTransparent(Renderer mesh)
    {
        if (!mesh.enabled)
            return;
        Material[] materials = mesh.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].shader.name != "Universal Render Pipeline/Lit")
                continue;
            materials[i] = GetTransparentMaterial(materials[i]);
        }
        mesh.materials = materials;
    }

    /// <summary>
    /// Sets all materials of a Renderer opaque.
    /// </summary>
    public void SetMaterialsOpaque(Renderer mesh)
    {
        if (!mesh.enabled)
            return;
        Material[] materials = mesh.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].shader.name != "Universal Render Pipeline/Lit")
                continue;
            materials[i] = GetOpaqueMaterial(materials[i]);
        }
        mesh.materials = materials;
    }

    public void SetMaterialTransparent(Renderer mesh)
    {
        if (mesh.enabled)
            mesh.material = GetTransparentMaterial(mesh.material);
    }

    public void SetMaterialOpaque(Renderer mesh)
    {
        if (mesh.enabled)
            mesh.material = GetOpaqueMaterial(mesh.material);
    }

    /// <summary>
    /// Returns the transparent variant of the given material.
    /// The original material and the corresponding transparent material need to exist in
    /// the opaqueMaterials and transparentMaterials lists.
    /// </summary>
    private Material GetTransparentMaterial(Material m)
    {
        string materialName = CleanName(m.name);
        if (_dontChange.ContainsKey(materialName))
            return m;
        Material result = m;
        if (_opaqueToTransparent.ContainsKey(materialName))
            result = new Material(_opaqueToTransparent[CleanName(m.name)]);
        // if transparentToOpaque contains the material it is already set to transparent
        else if (!_transparentToOpaque.ContainsKey(materialName))
            Debug.LogErrorFormat("Material {0} is not available!", materialName);
        return result;
    }


    /// <summary>
    /// Returns the opaque variant of the given material.
    /// The original material and the corresponding opaque material need to exist in
    /// the opaqueMaterials and transparentMaterials lists.
    /// </summary>
    private Material GetOpaqueMaterial(Material m)
    {
        string materialName = CleanName(m.name);
        if (_dontChange.ContainsKey(materialName))
            return m;
        Material result = m;
        if (_transparentToOpaque.ContainsKey(materialName))
            result = new Material(_transparentToOpaque[CleanName(m.name)]);
        // if opaqueToTransparent contains the material it is already set to opaque
        else if (!_opaqueToTransparent.ContainsKey(materialName))
            Debug.LogErrorFormat("Material {0} is not available!", materialName);
        return result;
    }

    private string CleanName(string n)
    {
        return n.Replace(" (Instance)", "");
    }
}