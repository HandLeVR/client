using System;
using UnityEngine;
using Newtonsoft.Json;

public class Coat
{
    public long id;
    public Permission permission;
    public string name;
    public string description;
    public CoatType type;
    [JsonConverter(typeof(ColorJsonConverter))]
    public Color color;
    public float costs;
    public float solidVolume;
    public string hardenerMixRatio;
    public float thinnerPercentage;
    public float viscosity;
    public string dryingType;
    public float dryingTemperature;
    public int dryingTime;
    public float minSprayDistance;
    public float maxSprayDistance;
    public float glossWet;
    public float glossDry;
    public float targetMinThicknessWet;
    public float targetMaxThicknessWet;
    public float targetMinThicknessDry;
    public float targetMaxThicknessDry;
    public float fullOpacityMinThicknessWet;
    public float fullOpacityMinThicknessDry;
    public float fullGlossMinThicknessWet;
    public float fullGlossMinThicknessDry;
    public float runsStartThicknessWet;
    public float roughness;
}

[Serializable]
public enum CoatType
{
    Primer,
    Basecoat,
    Clearcoat,
    Topcoat
}