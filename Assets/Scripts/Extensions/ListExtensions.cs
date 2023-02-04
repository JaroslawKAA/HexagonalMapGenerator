using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
    public static bool IsContentEqual<T>(this List<T> list1, List<T> list2) => 
        list1.All(list2.Contains) && list1.Count == list2.Count;
}
