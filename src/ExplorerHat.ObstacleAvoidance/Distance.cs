using System;

namespace ExplorerHat.ObstacleAvoidance
{
    public class Distance : IComparable
    {
        public double Value { get; private set; }

        public Vector Vector { get; private set; }

        public Distance(Vector vector, double value)
        {
            Vector = vector;
            Value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            
            Distance other = obj as Distance;

            if (other != null)
            {
                return this.Value.CompareTo(other.Value);
            }
            else
            {
                throw new ArgumentException("Object is not a Distance");
            }
        }
    }
}