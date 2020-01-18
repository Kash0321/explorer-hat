using System;
using System.Collections.Generic;
using System.Linq;

namespace ExplorerHat.ObstacleAvoidance
{
    public class DistanceTuple
    {
        private Dictionary<Vector, Distance> _tuple;

        public double LeftDistance 
        { 
            get
            {
                var result = Math.Round(_tuple[Vector.Left].Value, 4, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        public double CenterDistance 
        { 
            get
            {
                var result = Math.Round(_tuple[Vector.Center].Value, 4, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        public double RightDistance 
        { 
            get
            {
                var result = Math.Round(_tuple[Vector.Right].Value, 4, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        public Distance MinimumDistance 
        {
            get
            {
                var result = _tuple.Values.Min(d => d);
                return result;
            }
        }

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