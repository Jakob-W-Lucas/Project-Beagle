using UnityEngine;

[System.Serializable]
public class Point
{
    public Vertex Vertex;
    public Vector2 Position;
    public bool IsPointer;
    public bool IsStation;

    public Point(Agent a)
    {
        Vertex = a.Navigation.Origin;
        Position = Vertex.Position;
        IsPointer = Vertex.IsPointer;
        IsStation = Vertex.IsStation;
    }
}

namespace UnityUtils {
    public static class PathExtensions {
        
        public static Route CompareRoutes(this Route route, Route other) => route.CompareTo(other) == 1 ? route : other;

        const float Epsilon = 0.01f;
            
        public static bool IsPointBetweenPoints(this Vector2 point, Vector2 a, Vector2 b)
        {
            if (Mathf.Approximately(a.x, b.x))
                return WithinBounds(a, b, point) && 
                    Mathf.Abs(point.x - a.x) < Epsilon;
                
            float slope = (b.y - a.y) / (b.x - a.x);
            float intercept = a.y - slope * a.x;
            float calculatedY = slope * point.x + intercept;

            return WithinBounds(a, b, point) &&
                Mathf.Abs(point.y - calculatedY) < Epsilon;
        }

        public static bool WithinBounds(Vector2 a, Vector2 b, Vector2 point) => point.x > Mathf.Min(a.x, b.x) && point.x < Mathf.Max(a.x, b.x) &&
            point.y > Mathf.Min(a.y, b.y) && point.y < Mathf.Max(a.y, b.y);
    }
}