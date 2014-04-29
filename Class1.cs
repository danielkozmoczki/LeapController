using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leap;

namespace MouseForm
{
    public static class Extensions
    {
        public static bool IsTapping(this Finger finger)
        {
            return (finger.TipVelocity.Magnitude > 150) && finger.TipVelocity.y < 0;
        }
        public static bool IsScrolling(this Finger finger)
        {
            return (finger.TipVelocity.Magnitude > 150) && finger.TipVelocity.y < 0;
        }

    }
}
