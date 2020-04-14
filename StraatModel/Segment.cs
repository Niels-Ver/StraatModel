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
                   EqualityComparer<Knoop>.Default.Equals(beginKnoop, segment.beginKnoop) &&
                   EqualityComparer<Knoop>.Default.Equals(eindKnoop, segment.eindKnoop) &&
                   segmentId == segment.segmentId &&
                   EqualityComparer<List<Punt>>.Default.Equals(vertices, segment.vertices);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(beginKnoop, eindKnoop, segmentId, vertices);
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
