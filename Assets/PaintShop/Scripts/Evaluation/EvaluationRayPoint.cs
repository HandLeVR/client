using UnityEngine;

/// <summary>
/// Represents a end point of an evaluation ray in an evaluation cone. Normally, the origin of an evaluation ray is
/// the muzzle of the spray gun.
/// </summary>
public class EvaluationRayPoint : MonoBehaviour
{
  private LineDrawer lineDrawer;
  
  private void Start()
  {
    lineDrawer = GetComponent<LineDrawer>();
  }

  public void DrawDebugLine(Vector3 origin, float length)
  {
    lineDrawer.DrawLineInGameView(origin, origin + (transform.position - origin).normalized * length, Color.green);
  }
}
