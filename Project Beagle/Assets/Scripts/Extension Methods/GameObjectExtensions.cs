using UnityEngine;

public static class GameObjectExtensions 
{
    public static T GetOrAdd<T> (this GameObject gameObject) where T : Component {
        T compontent = gameObject.GetComponent<T>();
        if (!compontent) compontent = gameObject.AddComponent<T>();

        return compontent;
    }

    public static T OrNull<T> (this T obj) where T : Object => (bool)obj ? obj : null;

    public static void DestroyChildren(this GameObject gameObject) {
        gameObject.transform.DestroyChildren();
    }
}
