using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Punt
    {
        [DataMember]
        public double x { get; set; }
        [DataMember] 
        public double y { get; set; }

        public Punt(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Punt punt &&
                   x == punt.x &&
                   y == punt.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
