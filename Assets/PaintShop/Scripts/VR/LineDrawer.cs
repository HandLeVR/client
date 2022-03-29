using UnityEngine;

/// <summary>
/// Draws a line in game using a LineRenderer.
/// 
/// Based on: https://stackoverflow.com/questions/42819071/debug-drawline-not-showing-in-the-gameview
/// </summary>
public class LineDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private bool visible;

    public void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));
        visible = false;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        // automatically disable ray in the next frame (DrawLineInGameView needs to be called to activate it again)
        lineRenderer.enabled = visible;
        visible = false;
    }
    
    /// <summary>
    /// Draws lines through the provided vertices
    /// </summary>
    public void DrawLineInGameView(Vector3 start, Vector3 end, Color color)
    {
        if (!enabled)
            return; 
        
        lineRenderer.enabled = true;
        // set color
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // set width
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;

        // set line count which is 2
        lineRenderer.positionCount = 2;

        // set the position of both two lines
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        
        visible = true;
    }
}