using UnityEngine;

namespace ThirdPersonController
{

    public static class ThirdPersonHelpers
    {
        public static Vector3 Horizontal(this Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }
    }

}