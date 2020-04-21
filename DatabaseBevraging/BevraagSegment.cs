using StraatModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseBevraging
{
    public class BevraagSegment
    {
        public int segmentId { get; set; }
        public int beginknoopId { get; set; }
        public int eindknoopId { get; set; }
        public int hoofdknoop { get; set; }

        public List<Punt> vertices { get; set; }

        public BevraagSegment(int segmentId, int beginknoopId, int eindknoopId, int hoofdKnoop)
        {
            this.segmentId = segmentId;
            this.beginknoopId = beginknoopId;
            this.eindknoopId = eindknoopId;
            this.hoofdknoop = hoofdKnoop;
        }

        public override bool Equals(object obj)
        {
            return obj is BevraagSegment segment &&
                   segmentId == segment.segmentId &&
                   hoofdknoop == segment.hoofdknoop;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(segmentId, hoofdknoop);
        }
    }
}
