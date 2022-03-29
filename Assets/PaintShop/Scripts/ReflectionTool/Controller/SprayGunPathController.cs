using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows to display the path of the spray gun in form of a line. The color of the line represents the distance of
/// the spray gun to the workpiece and the thickness of the line represents the velocity of the spray gun.
/// </summary>
public class SprayGunPathController : Singleton<SprayGunPathController>
{
    [Header("Line Settings")]
    [Tooltip("Number of frames for averaging. Recommended value: 4")]
    public int frameCountCap;
    [Tooltip("Applied to the whole line to determine the overall line width.")]
    public float lineWidthMultiplier;
    [Tooltip("Is multiplied by the line width multiplier to determine the minimum line width.")]
    public float minLineWidth;
    [Tooltip("The range used to map the average speed to the line width range.")]
    public float averageSpeedRange = 0.2f;
    
    [Space(10)]
    [Header("Assignable Variables")]
    [SerializeField]
    public LayerMask raycastCollidableLayers;

    [Space(10)] 
    [Header("Start and end markers")]
    public Color startColor;
    public Color endColor;
    [Tooltip("If no object is assigned it will default to spheres")]
    public GameObject startObject;
    [Tooltip("If no object is assigned it will default to spheres")]
    public GameObject endObject;
    public float scalingFactor;

    [HideInInspector] public float finalAverageSpeed;

    private List<GameObject> _startAndEndMarkers;
    private List<LineData> _lineDataList;
    private int _frameCounter;
    private bool _lineRendererInitialized;
    private SprayGun _sprayGun;
    private List<LineRenderer> _lineRenderers;
    private List<Vector3> _previousPositions;
    private Color _oldColor;
    private Transform _workpiece;
    
    private bool _pathVisible;
    private bool _showSpeedValues;
    
    void Start()
    {
        _previousPositions = new List<Vector3>();
        _startAndEndMarkers = new List<GameObject>();
        _sprayGun = PlayRecordingController.Instance.sprayGunRecorder;
        Reset();
    }
    void FixedUpdate()
    {
        if (!PlayRecordingController.Instance.playing)
            return;
        
        _frameCounter++;
        _previousPositions.Add(_sprayGun.pinSpotOrigin.position);

        if (_sprayGun.currentMode == SprayGun.SprayGunMode.Spray)
        {
            if (_frameCounter > frameCountCap)
            {
                // grey is the neutral state and only indicates that nothing was drawn when last checked
                // yellow indicates the starting point and will be drawn thicker
                Color newColor = _oldColor == Color.gray ? Color.yellow : GetDistanceColor();
                
                // if newColor is different from oldColor thus requiring a new lineRenderer
                if (newColor != _oldColor)
                {
                    // if its a new starting point
                    if (newColor == Color.yellow)
                        DrawNewLine(_sprayGun.pinSpotOrigin.position, _sprayGun.pinSpotOrigin.position, Color.yellow);
                    // if its a different color connecting to an old line
                    else
                        DrawNewLine(GetEndOfLastLine(), GetAveragePosition(_previousPositions), newColor);
                }
                // if it is adding the same color to an old line
                else
                {
                    int number = _lineRenderers.Count;
                    AddToOldLine(_lineRenderers[number - 1], GetAveragePosition(_previousPositions));
                }

                _oldColor = newColor;
            }
        }
        else
        {
            // color gray indicates the neutral state
            // if the old color is not gray then the trigger was just released
            if (!(_oldColor == Color.gray))
            {
                DrawNewLine(GetEndOfLastLine(),GetEndOfLastLine(),Color.white);
                _previousPositions = new List<Vector3>();
                _oldColor = Color.gray;
            }
        }
        
        if (_frameCounter > frameCountCap)
        {
            _frameCounter = 0;
            _previousPositions = new List<Vector3>();
        }
    }

    /// <summary>
    /// Resets and removes all LineRenderers.
    /// </summary>
    public void Reset()
    {
        if (_lineRenderers != null)
            _lineRenderers.ForEach(lineRenderer =>
            {
                if (lineRenderer)
                    Destroy(lineRenderer.gameObject);
            });
        _lineRenderers = new List<LineRenderer>();
        _frameCounter = 0;
        _oldColor = Color.gray;
        _lineDataList = new List<LineData>();
        _startAndEndMarkers.ForEach(Destroy);
        _startAndEndMarkers = new List<GameObject>();
        _workpiece = ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>().transform;
    }


    /// <summary>
    /// Method call creates a new LineRenderer from a previous position to the current Position.
    /// </summary>
    /// <param name="previousPos">from position</param>
    /// <param name="currentPos">to position</param>
    /// <param name="color">may contain information about starting point and end point</param>
    private void DrawNewLine(Vector3 previousPos, Vector3 currentPos, Color color)
    {
        if (!_workpiece)
            _workpiece = ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>().transform;
        
        if (_showSpeedValues)
            ApplyAverageSpeedValues(true);
            
        GameObject go = new GameObject("Line Renderer Nr."+_lineRenderers.Count);
        go.transform.SetParent(_workpiece);
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        _lineRenderers.Add(lineRenderer);
        
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // if the color is yellow it is a starting point
        if (color == Color.yellow)
        {
            lineRenderer.startWidth = 1;
            lineRenderer.endWidth = 0;
            
            // create start marker
            GameObject marker =
                startObject ? Instantiate(startObject) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.SetParent(_workpiece);
            marker.transform.position = currentPos;
            marker.transform.localScale = marker.transform.localScale * scalingFactor;
            marker.GetComponent<Renderer>().material.color = startColor;
            marker.SetActive(_pathVisible);
            _startAndEndMarkers.Add(marker);
        }

        // if the color is white it is an end point
        if (color == Color.white)
        {
            lineRenderer.startWidth = 0;
            lineRenderer.endWidth = 1;
            
            // create end marker
            GameObject marker =
                endObject ? Instantiate(endObject) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.SetParent(_workpiece);
            marker.transform.position = previousPos;
            marker.transform.localScale = marker.transform.localScale * scalingFactor;
            marker.GetComponent<Renderer>().material.color = endColor;
            marker.SetActive(_pathVisible);
            _startAndEndMarkers.Add(marker);
        }
        
        lineRenderer.widthMultiplier = lineWidthMultiplier;
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0,previousPos);
        lineRenderer.SetPosition(1,currentPos);
        lineRenderer.gameObject.SetActive(_pathVisible);

        // collect Line Data that stores average speed per line segment position
        LineData lineData = new LineData();
        lineData.lineRendererIndex = _lineRenderers.Count - 1;
        lineData.positionIndex = 0;
        lineData.averageSpeed = GetAverageSpeedOfLastFrames(_previousPositions);
        _lineDataList.Add(lineData);

        lineData = new LineData();
        lineData.lineRendererIndex = _lineRenderers.Count - 1;
        lineData.positionIndex = 1;
        lineData.averageSpeed = GetAverageSpeedOfLastFrames(_previousPositions);
        _lineDataList.Add(lineData);
    }

    /// <summary>
    /// If an already existing LineRenderer receives a new line segment without having to change colors.
    /// </summary>
    /// <param name="lr">The Line Renderer</param>
    /// <param name="position">The new position to be added</param>
    private void AddToOldLine(LineRenderer lr, Vector3 position)
    {
        lr.widthMultiplier = lineWidthMultiplier;
        lr.positionCount = lr.positionCount + 1;
        lr.SetPosition(lr.positionCount-1,position);
        lr.gameObject.SetActive(_pathVisible);
        
        // Collect Line Data that stores average speed per line segment position
        LineData lineData = new LineData();
        lineData.lineRendererIndex = _lineRenderers.Count - 1;
        lineData.positionIndex = lr.positionCount - 1;
        lineData.averageSpeed = GetAverageSpeedOfLastFrames(_previousPositions);
        _lineDataList.Add(lineData);
    }

    /// <summary>
    /// Returns the color that corresponds to the distance from the workpiece.
    /// </summary>
    /// <returns>Distance Color</returns>
    private Color GetDistanceColor()
    {
        float optimalDistance = (PaintController.Instance.minSprayDistance + PaintController.Instance.maxSprayDistance) / 2;
        float optimalDistanceRayOffset = optimalDistance - PaintController.Instance.minSprayDistance;

        Physics.Raycast(_sprayGun.pinSpotOrigin.position, _sprayGun.pinSpotOrigin.forward, out var hitCheck, Mathf.Infinity, raycastCollidableLayers);
        if (hitCheck.collider == null)
            return Color.black;
        float distance = Vector3.Distance(_sprayGun.pinSpotOrigin.position, hitCheck.point);

        if (distance > optimalDistance + optimalDistanceRayOffset)
            return Color.blue;
        if (distance < optimalDistance - optimalDistanceRayOffset)
            return Color.red;
        return Color.green;
    }

    /// <summary>
    /// Averages out the positions in a Vector3 list.
    /// </summary>
    /// <param name="positions"></param>
    /// <returns>Average Position</returns>
    private Vector3 GetAveragePosition(List<Vector3> positions)
    {
        if (positions.Count == 0)
            return new Vector3(0, 0, 0);

        float x = 0;
        float y = 0;
        float z = 0;

        foreach (var vector in positions)
        {
            x = x + vector.x;
            y = y + vector.y;
            z = z + vector.z;
        }

        Vector3 output = new Vector3(x / positions.Count, y / positions.Count, z / positions.Count);

        return output;

    }

    /// <summary>
    /// Returns the position of the end of the last line.
    /// </summary>
    /// <returns>Last line end position</returns>
    private Vector3 GetEndOfLastLine()
    {
        int lastElement = _lineRenderers.Count-1;
        return _lineRenderers[lastElement].GetPosition(_lineRenderers[lastElement].positionCount-1);
    }
    
    /// <summary>
    /// Toggles visibility of LineRenderers.
    /// </summary>
    public void ShowPath(bool active)
    {
        _pathVisible = active;
        
        foreach (var lr in _lineRenderers)
            lr.gameObject.SetActive(_pathVisible);

        foreach (var marker in _startAndEndMarkers)
            marker.SetActive(_pathVisible);
    }
    
    /// <summary>
    /// Saves the current data. Copies are made of the current start and end marker and the line renderer.
    /// They are deactivated and can be activated if this data is loaded.
    /// </summary>
    public SprayGunPathData GetCurrentSprayGunPathData()
    {
        SprayGunPathData result = new SprayGunPathData
        {
            frameCounter = _frameCounter,
            lineRenderers = new List<LineRenderer>(),
            oldColor = _oldColor,
            previousPositions = _previousPositions,
            lineDataList = _lineDataList,
            startAndEndMarkers = new List<GameObject>()
        };
        _lineRenderers.ForEach(lineRenderer =>
        {
            LineRenderer newLineRenderer = Instantiate(lineRenderer, _workpiece);
            newLineRenderer.gameObject.SetActive(false);
            result.lineRenderers.Add(newLineRenderer);
        });
        _startAndEndMarkers.ForEach(obj =>
        {
            GameObject newObj = Instantiate(obj, _workpiece);
            newObj.SetActive(false);
            result.startAndEndMarkers.Add(newObj);
        });
        return result;
    }

    /// <summary>
    /// Loads previously saved data.
    /// </summary>
    public void LoadSprayGunPathData(SprayGunPathData sprayGunPathData)
    {
        if (_lineRenderers != null)
            _lineRenderers.ForEach(lineRenderer => Destroy(lineRenderer.gameObject));
        _lineRenderers = new List<LineRenderer>();
        sprayGunPathData.lineRenderers.ForEach(lineRenderer =>
        {
            LineRenderer newLineRenderer = Instantiate(lineRenderer, _workpiece);
            newLineRenderer.gameObject.SetActive(_pathVisible);
            _lineRenderers.Add(newLineRenderer);
        });
        _frameCounter = sprayGunPathData.frameCounter;
        _oldColor = sprayGunPathData.oldColor;
        _lineDataList = sprayGunPathData.lineDataList;
        _startAndEndMarkers.ForEach(Destroy);
        _startAndEndMarkers = new List<GameObject>();
        sprayGunPathData.startAndEndMarkers.ForEach(obj =>
        {
            GameObject newObj = Instantiate(obj, _workpiece);
            newObj.SetActive(_pathVisible);
            _startAndEndMarkers.Add(newObj);
        });
        _previousPositions = sprayGunPathData.previousPositions;
    }

    /// <summary>
    /// Returns the average speed of the last few frames.
    /// </summary>
    private float GetAverageSpeedOfLastFrames(List<Vector3> positions)
    {
        float distanceTravelled = 0.0f;
        Vector3 previous = Vector3.zero;
        foreach (var position in positions)
        {
            if (previous == Vector3.zero)
            {
                previous = position;
            }
            else
            {
                distanceTravelled = distanceTravelled + Vector3.Distance(previous,position);
                previous = position;
            }
        }
        
        return distanceTravelled / (Time.fixedDeltaTime * positions.Count);
    }

    /// <summary>
    /// Creates a curve according to the average speed values of the lineData in comparison to the global average speed.
    /// </summary>
    public void ApplyAverageSpeedValues(bool active)
    {
        _showSpeedValues = active;
        int lineRenderersIterationIndex = 0;

        foreach (var lr in _lineRenderers)
        {
            AnimationCurve curve = new AnimationCurve();
            float totalLength = GetLength(lr, lr.positionCount);

            Color color = lr.startColor;

            if (color != Color.yellow && color != Color.white)
            {
                if (active)
                    foreach (var lineSegment in _lineDataList)
                    {
                        if (lineSegment.lineRendererIndex == lineRenderersIterationIndex)
                            curve.AddKey(GetLength(lr, lineSegment.positionIndex + 1) / totalLength,
                                ConvertToCurveValue(lineSegment.averageSpeed));
                    }
                else
                    curve.AddKey(0,1);

                lr.widthMultiplier = active ? lineWidthMultiplier * 2 : lineWidthMultiplier;
                lr.widthCurve = curve;
            }
            lineRenderersIterationIndex++;
        }
    }

    /// <summary>
    /// Takes the difference between localAverageSpeed and final average speed and outputs the necessary line width
    /// in accordance with the settings.
    /// </summary>
    /// <param name="localAverageSpeed">Average speed stored in Line Data</param>
    private float ConvertToCurveValue(float localAverageSpeed)
    {
        float minAverageSpeed = finalAverageSpeed - averageSpeedRange / 2;
        float maxAverageSpeed = finalAverageSpeed + averageSpeedRange / 2;
        return Mathf.Lerp(minLineWidth, 1, (localAverageSpeed - minAverageSpeed) / (maxAverageSpeed - minAverageSpeed));
    }

    /// <summary>
    /// To get total length positionIndex = positionCount
    /// To get partial length up to positionIndex otherwise
    /// </summary>
    /// <param name="lr">LineRenderer</param>
    /// <param name="positionIndex">The position to which length will be summed up</param>
    private float GetLength(LineRenderer lr, int positionIndex)
    {
        // account for difference between count and array index
        positionIndex = positionIndex - 1;
        
        if (lr.positionCount == 0 || lr.positionCount == 1)
            return 0;

        Vector3[] positionsArray = new Vector3[lr.positionCount];
        lr.GetPositions(positionsArray);
        int i = 0;
        Vector3 prevPos = new Vector3();
        float length = 0;

        foreach (var position in positionsArray)
        {
            if (i == positionIndex)
            {
                return length;
            }

            if (i == 0)
            {
                prevPos = position;
                i++;
            }
            else
            {
                length = length + Vector3.Distance(prevPos, position);
                prevPos = position;
                i++;
            }
        }

        return length;
    }
}

/// <summary>
/// Class that keeps track of averageSpeed at the positions of a LineRenderer as they are drawn
/// </summary>
[Serializable]
public class LineData
{
    public int lineRendererIndex;
    public int positionIndex;
    public float averageSpeed;

    public LineData(int lri, int poi, float avs)
    {
        lineRendererIndex = lri;
        positionIndex = poi;
        averageSpeed = avs;
    }
    
    public LineData()
    {
    }
}

public class SprayGunPathData
{
    public List<GameObject> startAndEndMarkers;
    public List<LineData> lineDataList;
    public List<LineRenderer> lineRenderers;
    public List<Vector3> previousPositions;
    public Color oldColor;
    public int frameCounter;
}
