using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the collision of the spray gun with the paint stand.
/// </summary>
public class PaintStandHitController : Singleton<PaintStandHitController>
{
    [Header("Hit Settings")] [Tooltip("Leave empty to play no sound.")]
    public AudioClip collisionSound;

    [Tooltip("Time the spray gun is stopped on collision.")]
    public float collisionStayTime;

    [Tooltip("Time needed to smoothly reset the position and rotation of the spray gun after collision.")]
    public float collisionReturnTime;

    [Tooltip("Color applies to decal with or without texture.")]
    public Color decalColor;

    [Tooltip("Decal mask.")] public Texture2D decalMaskTexture;

    [Tooltip("Leave empty to use blank color.")]
    public Texture2D decalTexture;

    public GameObject decalPrefab;
    [Range(0, 50)] public int maxDecals;
    [Range(0.1f, 5f)] public float decalSizeMultiplier;
    
    [HideInInspector] public bool playAudio = true;

    private bool _collisionActive;
    private float _collisionTime;
    private Vector3 _collisionPosition;
    private Quaternion _collisionRotation;
    private Vector3 _preCollisionLocalPosition;
    private Quaternion _preCollisionLocalRotation;

    private readonly List<GameObject> decals = new();

    private static readonly int MaskDecalID = Shader.PropertyToID("Mask_decal");
    private static readonly int MainTextureDecalID = Shader.PropertyToID("MainTexture_decal");
    private static readonly int ColorDecalID = Shader.PropertyToID("Color_decal");

    /// <summary>
    /// Deletes decals when the number of maxDecals is exceeded.
    /// </summary>
    void Update()
    {
        if (decals.Count >= maxDecals)
        {
            Destroy(decals[0]);
            decals.Remove(decals[0]);
        }

        if (!_collisionActive)
            return;

        _collisionTime -= Time.deltaTime;

        // only check for collision deactivation if the spray gun shall not be stopped
        if (!ApplicationController.Instance.stopSprayGunOnCollision)
        {
            if (_collisionTime <= 0)
                _collisionActive = false;
            return;
        }

        Transform sprayGunTransform = ApplicationController.Instance.primarySprayGun.transform;
        // set the position and rotation of the spray gun the the collision point
        if (_collisionTime > collisionReturnTime)
        {
            sprayGunTransform.position = _collisionPosition;
            sprayGunTransform.rotation = _collisionRotation;
        }
        // smoothly reset the actual position and rotation of the spray gun
        else if (_collisionTime > 0)
        {
            float lerpFactor = (collisionReturnTime - _collisionTime) / collisionReturnTime;
            sprayGunTransform.localPosition =
                Vector3.Lerp(sprayGunTransform.localPosition, _preCollisionLocalPosition, lerpFactor);
            sprayGunTransform.localRotation =
                Quaternion.Lerp(sprayGunTransform.localRotation, _preCollisionLocalRotation, lerpFactor);
        }
        // reset the actual position and rotation of the spray gun and finish the collision mode
        else if (_collisionTime <= 0)
        {
            _collisionActive = false;
            sprayGunTransform.localPosition = _preCollisionLocalPosition;
            sprayGunTransform.localRotation = _preCollisionLocalRotation;
        }
    }

    public void ClearHits()
    {
        decals.ForEach(Destroy);
        decals.Clear();
    }

    public void PlayAudio(Vector3 hitPos)
    {
        if (playAudio)
            AudioSource.PlayClipAtPoint(collisionSound, hitPos);
    }

    /// <summary>
    /// Spawns hit decals based on collision data sent from PaintStandHitListener components
    /// </summary>
    public void HandleHit(Vector3 hitPos, Quaternion rot, Transform detector)
    {
        // don't spawn decals when it's a recording, recording handles spawning of decals by itself
        if (_collisionActive || PlayRecordingController.Instance.playing)
            return;

        PlayAudio(hitPos);

        if (maxDecals == 0)
            return;

        decals.Add(SpawnHit(hitPos, rot, detector));

        _collisionActive = true;
        _collisionTime = collisionStayTime + collisionReturnTime;

        if (!ApplicationController.Instance.stopSprayGunOnCollision)
            return;

        Transform sprayGunTransform = ApplicationController.Instance.sprayGun.transform;
        _preCollisionLocalPosition = sprayGunTransform.localPosition;
        _preCollisionLocalRotation = sprayGunTransform.localRotation;
        _collisionPosition = sprayGunTransform.position;
        _collisionRotation = sprayGunTransform.rotation;
    }

    /// <summary>
    /// Spawns a decal at the given position and with the given rotation.
    /// </summary>
    public GameObject SpawnHit(Vector3 hitPos, Quaternion rot, Transform parent)
    {
        GameObject decalInstance = Instantiate(decalPrefab, hitPos, rot, parent);
        decalInstance.transform.localScale = decalInstance.transform.localScale * decalSizeMultiplier;
        var meshRenderer = decalInstance.GetComponent<MeshRenderer>();
        meshRenderer.material.SetTexture(MaskDecalID, decalMaskTexture);
        meshRenderer.material.SetTexture(MainTextureDecalID, decalTexture);
        meshRenderer.material.SetColor(ColorDecalID, decalColor);
        // 45 degree rotation to achieve diagonal caution stripe look 
        decalInstance.transform.Rotate(0, 45, 0);
        return decalInstance;
    }

    /// <summary>
    /// Returns a list of the current hit decals
    /// </summary>
    public List<RecordedCollision> GenerateRecordedCollisionsList()
    {
        List<RecordedCollision> recordedCollisions = new List<RecordedCollision>();

        foreach (var decal in decals)
        {
            RecordedCollision collision = new RecordedCollision();
            collision.hitPosX = decal.transform.position.x;
            collision.hitPosY = decal.transform.position.y;
            collision.hitPosZ = decal.transform.position.z;
            collision.rotX = decal.transform.rotation.x;
            collision.rotY = decal.transform.rotation.y;
            collision.rotZ = decal.transform.rotation.z;
            collision.rotW = decal.transform.rotation.w;
            recordedCollisions.Add(collision);
        }

        return recordedCollisions;
    }
}