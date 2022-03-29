using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Calculates the values relevant for the evaluation of the coat application.
/// </summary>
public class EvaluationController : Singleton<EvaluationController>
{
    public float angularTolerance = 5f;
    public float fullyPressedTolerance = 0.05f;
    public float colorPerSecond = 5f;
    public int currentSpeedWindow = 5;
    public ComputeShader coatThicknessShader;
    public int threadCountThicknessCalculation = 512;
    
    private SprayGun _sprayGun;
    private Transform _muzzle;
    private EvaluationCone _evaluationCone;
    private Coat _coat;
    private FixedSizedQueue<float> _speedQueue;
    private float _fixedDeltaTime;

    private int _framesSpraying;
    private int _framesPointingOnWorkpiece;
    private int _framesCorrectDistance;
    private int _framesCorrectAngle;
    private int _framesFullyPressed;
    private float _colorConsumption;
    private float _colorWastage;
    private float _colorUsage;
    private float _distanceTravelledWhileTriggerPressed;
    private int _framesTravelledWhileTriggerPressed;
    private float _currentDistance;
    private float _currentAngle;
    private float _currentSpeed;
    private float _currentCoatThickness;
    private int _releaseCount;
    
    private Vector3 _previousSprayGunPosition;
    private bool _triggerReleased;

    private CustomDrawable _drawableWorkpiece;
    
    private float[] _coatThicknessSums;
    private int[] _validPixels;
    private int _sumUpThicknessKernelId;
    private bool _thicknessCalculationDone = true;
    private float _layerThicknessAtPosition;

    private bool _gameObjectDestroyed;

    private void Start()
    {
        ApplicationController.Instance.ExecuteAfterSprayGunSpawn(SetSprayGun);
        _sumUpThicknessKernelId = coatThicknessShader.FindKernel("SumUpThickness");
        Reset();
        InvokeRepeating(nameof(CalculateCoatThickness), 0, 1 / 2f);
    }

    private void FixedUpdate()
    {
        if (!_sprayGun || _sprayGun.isDisabled)
            return;
        
        // accumulate data if the min spraying value threshold is reached
        if (_sprayGun.GetActualSprayingValue() > 0.05f)
        {
            _framesTravelledWhileTriggerPressed++;
            if (_triggerReleased)
            {
                _triggerReleased = false;
                _releaseCount++;
            }
            else
            {
                // adds the distance between previous position and current position to the total distance travelled
                float distance = Vector3.Distance(_previousSprayGunPosition,
                    ApplicationController.Instance.sprayGun.transform.position);
                _distanceTravelledWhileTriggerPressed = _distanceTravelledWhileTriggerPressed + distance;
                
                // calculate the current speed on bases of the last frames (number of frames is determined by currentSpeedWindow)
                // the elements of the queue are also saved in the recording so it is possible to derive the speed again
                _speedQueue.Enqueue(distance);
                float sum = 0;
                _speedQueue.q.ToList().ForEach(d => sum += d);
                _currentSpeed = sum / _speedQueue.q.Count / _fixedDeltaTime;
                
                _framesSpraying++;

                bool hitWorkpiece = Physics.Raycast(_muzzle.position, _muzzle.forward, out RaycastHit hitCheck, Mathf.Infinity,
                    LayerMask.GetMask("Drawable"));

                if (hitWorkpiece && hitCheck.collider.gameObject.Equals(_drawableWorkpiece.gameObject))
                {
                    _framesPointingOnWorkpiece++;
                    _currentDistance = Vector3.Distance(_muzzle.position, hitCheck.point) * 100;
                    if (_currentDistance >= _coat.minSprayDistance && _currentDistance <= _coat.maxSprayDistance)
                        _framesCorrectDistance++;

                    Vector3 negDir = _muzzle.position - hitCheck.point;
                    _currentAngle = Vector3.Angle(hitCheck.normal, negDir);
                    if (_currentAngle <= angularTolerance)
                        _framesCorrectAngle++;
                }

                float currentColorConsumption = colorPerSecond * _sprayGun.GetActualSprayingValue() * Time.deltaTime;
                _colorConsumption += currentColorConsumption;
                _colorWastage += (1 - _evaluationCone.GetSprayHit()) * currentColorConsumption;
                _colorUsage += _evaluationCone.GetSprayHit() * currentColorConsumption;

                if (_sprayGun.GetTriggerValue() >= 1 - fullyPressedTolerance)
                    _framesFullyPressed++;
            }
        }
        else
        {
            _triggerReleased = true;
        }
        
        // sets the previous position for future comparison
        _previousSprayGunPosition = ApplicationController.Instance.sprayGun.transform.position;
    }

    private void OnDestroy()
    {
        _gameObjectDestroyed = true;
    }

    public float GetCorrectDistancePercentage()
    {
        if (_framesPointingOnWorkpiece == 0)
            return 100;
        return _framesCorrectDistance * 100f / _framesPointingOnWorkpiece;
    }

    public float GetCorrectAnglePercentage()
    {
        if (_framesPointingOnWorkpiece == 0)
            return 100;
        return _framesCorrectAngle * 100f / _framesPointingOnWorkpiece;
    }

    public float GetColorConsumption()
    {
        return _colorConsumption;
    }

    public float GetColorWastage()
    {
        return _colorWastage;
    }

    public float GetColorUsage()
    {
        return _colorUsage;
    }

    public float GetFullyPressedPercentage()
    {
        // compensate the way of the trigger until fully pressed
        float adaptedFramesSpraying = _framesSpraying - _releaseCount * 5;
        if (adaptedFramesSpraying == 0)
            return 100;
        return Mathf.Clamp(_framesFullyPressed * 100f / adaptedFramesSpraying,0,100);
    }

    public float GetSecondsSprayed()
    {
        return _colorConsumption / colorPerSecond;
    }

    public float GetAverageSpeed()
    {
        if (_framesTravelledWhileTriggerPressed <= 0)
            return 0;
        return _distanceTravelledWhileTriggerPressed / (_fixedDeltaTime * _framesTravelledWhileTriggerPressed);
    }

    public float GetCurrentDistance()
    {
        return _currentDistance;
    }

    public float GetCurrentAngle()
    {
        return _currentAngle;
    }

    public float GetCurrentSpeed()
    {
        return _currentSpeed;
    }

    public float GetCurrentCoatThickness()
    {
        return _currentCoatThickness;
    }

    public void Reset()
    {
        _framesSpraying = 0;
        _framesPointingOnWorkpiece = 0;
        _framesCorrectDistance = 0;
        _framesCorrectAngle = 0;
        _framesFullyPressed = 0;
        _colorConsumption = 0;
        _colorWastage = 0;
        _colorUsage = 0;
        _framesTravelledWhileTriggerPressed = 0;
        _distanceTravelledWhileTriggerPressed = 0;
        _releaseCount = 0;
        _triggerReleased = true;
        if (ApplicationController.Instance.currentWorkpieceGameObject)
            _drawableWorkpiece =
                ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>();
        _currentDistance = 0;
        _currentAngle = 0;
        _currentSpeed = 0;
        _currentCoatThickness = 0;
        _speedQueue = new FixedSizedQueue<float> {Limit = currentSpeedWindow};
        _coat = PaintController.Instance.chosenCoat;
        // needed because it can only be called in main thread
        _fixedDeltaTime = Time.fixedDeltaTime;
    }

    public void LoadEvaluationData(EvaluationData data, bool loadCurrentCoatThickness = true)
    {
        _framesSpraying = data.framesSpraying;
        _framesPointingOnWorkpiece = data.framesPointingOnWorkpiece;
        _framesCorrectDistance = data.framesCorrectDistance;
        _framesCorrectAngle = data.framesCorrectAngle;
        _framesFullyPressed = data.framesFullyPressed;
        _colorConsumption = data.colorConsumption;
        _colorWastage = data.colorWastage;
        _colorUsage = data.colorUsage;
        _distanceTravelledWhileTriggerPressed = data.distanceTravelledWhileTriggerPressed;
        _framesTravelledWhileTriggerPressed = data.framesTravelledWhileTriggerPressed;
        _currentDistance = data.currentDistance;
        _currentAngle = data.currentAngle;
        _currentSpeed = data.currentSpeed;
        _speedQueue = data.speedQueue;
        if (loadCurrentCoatThickness)
            _currentCoatThickness = data.currentCoatThickness;
    }

    public EvaluationData GetAsEvaluationData()
    {
        return new EvaluationData
        {
            framesSpraying = _framesSpraying,
            framesPointingOnWorkpiece = _framesPointingOnWorkpiece,
            framesCorrectDistance = _framesCorrectDistance,
            framesCorrectAngle = _framesCorrectAngle,
            framesFullyPressed = _framesFullyPressed,
            colorConsumption = _colorConsumption,
            colorWastage = _colorWastage,
            colorUsage = _colorUsage,
            distanceTravelledWhileTriggerPressed = _distanceTravelledWhileTriggerPressed,
            framesTravelledWhileTriggerPressed = _framesTravelledWhileTriggerPressed,
            currentDistance = _currentDistance,
            currentAngle = _currentAngle,
            currentSpeed = _currentSpeed,
            speedQueue = _speedQueue,
            currentCoatThickness = _currentCoatThickness
        };
    }

    /// <summary>
    /// Calculates the coat thickness. Only the areas are used where paint hit the workpiece.
    /// We use a compute shader to calculate the thickness because it's faster on the gpu.
    /// </summary>
    private void CalculateCoatThickness()
    {
        if (_thicknessCalculationDone && _drawableWorkpiece.heightmapOutput != null)
        {
            _thicknessCalculationDone = false;
            ComputeBuffer resultBuffer = new ComputeBuffer(threadCountThicknessCalculation * 2, sizeof(float));
            _coatThicknessSums = new float[threadCountThicknessCalculation * 2];
            resultBuffer.SetData(_coatThicknessSums);

            coatThicknessShader.SetBuffer(_sumUpThicknessKernelId, "result", resultBuffer);
            coatThicknessShader.SetTexture(_sumUpThicknessKernelId, "heatmap", _drawableWorkpiece.heightmapOutput);
            coatThicknessShader.SetInt("size", _drawableWorkpiece.heightmapOutput.width);
            coatThicknessShader.SetInt("threads", threadCountThicknessCalculation);
            coatThicknessShader.Dispatch(_sumUpThicknessKernelId, threadCountThicknessCalculation, 1, 1);
            AsyncGPUReadback.Request(resultBuffer, OnCompleteReadback);
            resultBuffer.Release();
        }
    }

    /// <summary>
    /// Asynchronously get the coat thickness calculation from the gpu to avoid blocking the main thread.
    /// </summary>
    /// <param name="request"></param>
    private void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        // avoid further proceeding if application is closed
        if (_gameObjectDestroyed)
            return;

        _coatThicknessSums = request.GetData<float>().ToArray();
        float thicknessSum = 0;
        float validPixelsSum = 0;
        for (int i = 0; i < _coatThicknessSums.Length; i = i + 2)
        {
            thicknessSum += _coatThicknessSums[i];
            validPixelsSum += _coatThicknessSums[i + 1];
        }

        _currentCoatThickness = validPixelsSum > 0
            ? (thicknessSum / validPixelsSum / PaintController.Instance.targetMinThicknessWetAlpha) *
              PaintController.Instance.chosenCoat.targetMinThicknessWet
            : 0;

        _thicknessCalculationDone = true;
    }

    /// <summary>
    /// Sets needed information about the spray gun.
    /// </summary>
    public void SetSprayGun()
    {
        _sprayGun = ApplicationController.Instance.sprayGun;
        _muzzle = _sprayGun.pinSpotOrigin;
        _evaluationCone = _sprayGun.paintSpotDrawer.GetComponent<EvaluationCone>();
    }
}
