using UnityEngine;
using System.Collections;

namespace VelandelPiracyHill
{
    public static class TransformExtensions
    {
        public static T FindAnyChild<T>(this Transform trans, string name) where T : Component
        {
            for (int n = 0; n < trans.childCount; n++)
            {
                if (trans.GetChild(n).childCount > 0)
                {
                    var child = trans.GetChild(n).FindAnyChild<Transform>(name);
                    if (child != null) return child.GetComponent<T>();
                }

                if (trans.GetChild(n).name == name)
                {
                    return trans.GetChild(n).GetComponent<T>();
                }
            }
            return default(T);
        }
    }
}