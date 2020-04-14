using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Knoop
    {
        [DataMember]
        public Punt punt { get; set; }
        [DataMember]
        public int knoopID { get; set; }

        public Knoop(int knoopID, Punt punt)
        {
            this.knoopID = knoopID;
            this.punt = punt;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
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
