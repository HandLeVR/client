using UnityEngine;

/// <summary>
/// Represents an ellipse.
/// 
/// Source: http://world-create.blogspot.com/2009/01/ellipse-maths.html
/// </summary>
public class Ellipse
{
    private readonly Vector3 m_centre;
    private readonly Vector3 m_up;
    private readonly Vector3 m_along;
    private readonly float m_h;
    private readonly float m_l;

    public Ellipse(Vector3 mCentre, Vector3 mUp, Vector3 mAlong, float mH, float mL)
    {
        m_centre = mCentre;
        m_up = mUp;
        m_along = mAlong;
        m_h = mH;
        m_l = mL;
    }

    /// <summary>
    /// Allows to find a point on the ellipse.
    /// </summary>
    public Vector3 PointAt(float t)
    {
        float c = Mathf.Cos(t);
        float s = Mathf.Sin(t);

        return m_h * c * m_up + m_l * s * m_along + m_centre;      
    }
}

