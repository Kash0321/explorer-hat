using System;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Represents a distance measurement
    /// </summary>
    public class Distance : IComparable
    {
        /// <summary>
        /// Value measured
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector Vector { get; private set; }

        /// <summary>
        /// Initializes a <see cref="Distance"/> instance
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        public Distance(Vector vector, double value)
        {
            Vector = vector;
            Value = value;
        }

        /// <inheritdoc />
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