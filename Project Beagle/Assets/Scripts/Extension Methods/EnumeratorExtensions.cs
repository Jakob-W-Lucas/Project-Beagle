using System.Collections.Generic;

public static class EnumeratorExtensions
{
    public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator) {
        while (enumerator.MoveNext()) {
            yield return enumerator.Current;
        }
    }
}
