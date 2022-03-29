using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that represents a full recording.
/// </summary>
[Serializable]
public class RecordingData
{
    public Recording recording;
    public EvaluationData evaluationData;
    public List<Frame> frames;

    public RecordingData()
    {
        frames = new List<Frame>();
    }
}

/// <summary>
/// Class that represents a frame.
/// </summary>
[Serializable]
public class Frame
{
    public float sprayingValue;
    public float triggerValue;
    public float? wideStreamRegulationValue;
    public ObjectData followHead;
    public ObjectData rightHand;
    public ObjectData pinSpotOrigin;

    public SprayGun.SprayGunMode mode;
    public ObjectData holder1;
    public ObjectData holder2;
    public ObjectData holder3;
    public ObjectData holder4;
    public ObjectData centralRod;
    public ObjectData workpiece;
    public List<RecordedCollision> recordedCollisionList;
}

/// <summary>
/// Class that saves all relevant information for the according frame.
/// </summary>
[Serializable]
public class ObjectData
{
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public ObjectData()
    {
    }

    public ObjectData(Transform t)
    {
        var position = t.position;
        posX = position.x;
        posY = position.y;
        posZ = position.z;
        var rotation = t.rotation;
        rotX = rotation.x;
        rotY = rotation.y;
        rotZ = rotation.z;
        rotW = rotation.w;
    }

    public Vector3 getPosition()
    {
        return new Vector3(posX, posY, posZ);
    }

    public Quaternion getRotation()
    {
        return new Quaternion(rotX, rotY, rotZ, rotW);
    }

    public void SetTransform(Transform t)
    {
        t.position = new Vector3(posX, posY, posZ);
        t.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
    }
}

/// <summary>
/// Collision Decal Data
/// </summary>
[Serializable]
public class RecordedCollision
{
    public float hitPosX;
    public float hitPosY;
    public float hitPosZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;
}

/// <summary>
/// The final evaluation data.
/// </summary>
[Serializable]
public class EvaluationData
{
    public int framesSpraying;
    public int framesPointingOnWorkpiece;
    public int framesCorrectDistance;
    public int framesCorrectAngle;
    public int framesFullyPressed;
    public float colorConsumption;
    public float colorWastage;
    public float colorUsage;
    public float distanceTravelledWhileTriggerPressed;
    public int framesTravelledWhileTriggerPressed;
    public float currentDistance;
    public float currentAngle;
    public float currentSpeed;
    public float currentCoatThickness;
    public FixedSizedQueue<float> speedQueue;
}