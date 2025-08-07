using System;

namespace Example1
{
    internal class Trackball
    {
        public Trackball() => Reset();

        public float Theta { get; set; }
        public float Phi { get; set; }
        public float Radius { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        
        public void Reset()
        {
            Theta = 0f;
            Phi = (float)-Math.PI;
            Radius = 0.8f;
            X = 0f;
            Y = 0f;            
        }
    }
}
