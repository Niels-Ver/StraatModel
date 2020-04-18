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

       

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Knoop knoop &&
                   knoopID == knoop.knoopID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(knoopID);
        }
    }
}
