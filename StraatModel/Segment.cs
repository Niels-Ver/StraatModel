using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Segment
    {
        [DataMember]
        public Knoop beginKnoop { get; set; }
        [DataMember]
        public Knoop eindKnoop { get; set; }
        [DataMember]
        public int segmentId { get; set; }
        [DataMember]
        public List<Punt> vertices { get; set; }


        public Segment(int segmentId, Knoop beginKnoop, Knoop eindKnoop, List<Punt> vertices)
        {
            this.segmentId = segmentId;
            this.beginKnoop = beginKnoop;
            this.eindKnoop = eindKnoop;
            this.vertices = vertices;
        }

        public override bool Equals(object obj)
        {
            return obj is Segment segment &&
                   segmentId == segment.segmentId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(segmentId);
        }
    }
}
