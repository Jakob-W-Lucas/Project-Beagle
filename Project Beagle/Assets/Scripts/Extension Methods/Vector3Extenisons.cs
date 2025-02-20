using UnityEngine;

public static class Vector3Extenisons
{
    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null) {
        return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    }

    public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null) {
        return new Vector3(x:vector.x + (x ?? 0), y:vector.y + (y ?? 0), z:vector.z + (z ?? 0));
    }
}
