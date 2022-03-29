using UnityEngine;

/// <summary>
/// Base class for all objects that can be used in the estimation sub task.
/// </summary>
public abstract class EstimationObject : MonoBehaviour
{
   public abstract void Reset();
   
   public abstract void FadeIn();
   
   public abstract void FadeOut();

   public abstract bool ShowSolution();
}
