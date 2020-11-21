using System;
using System.Collections.Generic;
using System.Linq;

namespace ExplorerHat.ObstacleAvoidance
{
    /// <summary>
    /// Tuple of <see cref="Distance"/> measurements
    /// </summary>
    public class DistanceTuple
    {
        private Dictionary<Vector, Distance> _tuple;

        /// <summary>
        /// Distance measured by left sensor
        /// </summary>
        public double LeftDistance 
        { 
            get
            {
                var result = Math.Round(_tuple[Vector.Left].Value, 4, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        /// <summary>
        /// Distance measured by center sensor
        /// </summary>
        public double CenterDistance 
        { 
            get
            {
                var result = Math.Round(_tuple[Vector.Center].Value, 4, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        /// <summary>
        /// Distance measured by right sensor
        /// </summary>
        public double RightDistance 
        { 
            get
            {
                var result = Math.Round(_tuple[Vector.Right].Value, 4, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        /// <summary>
        /// Minimum distance value from all sensors
        /// </summary>
        public Distance MinimumDistance 
        {
            get
            {
                var result = _tuple.Values.Min(d => d);
                return result;
            }
        }

        /// <summary>
        /// Initializes a <see cref="DistanceTuple" /> instance
        /// </summary>
        /// <param name="leftDistance">Distance measured by left sensor</param>
        /// <param name="centerDistance">Distance measured by center sensor</param>
        /// <param name="rightDistance">Distance measured by right sensor</param>
        public DistanceTuple(double leftDistance, double centerDistance, double rightDistance)
        {
            _tuple = new Dictionary<Vector, Distance>()
            {
                { Vector.Left, new Distance(Vector.Left, leftDistance) },
                { Vector.Center, new Distance(Vector.Center, centerDistance) },
                { Vector.Right, new Distance(Vector.Right, rightDistance) }
            };
        }
    }
}