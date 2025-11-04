using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDEngine.Core.Extensions
{
    public static class Vector3Extensions
    {
        public static string ToFixed(this Vector3 vec, int precision = 1)
        {
            string format = $"F{precision}";
            return $"({vec.X.ToString(format)}, {vec.Y.ToString(format)}, {vec.Z.ToString(format)})";
        }
    }
}
