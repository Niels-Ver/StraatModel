 using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Graaf
    {
        [DataMember]
        public int graafId { get; set; }
        [DataMember]
        public Dictionary<Knoop, List<Segment>> map { get; set; }

        public Graaf(int graafId)
        {
            this.graafId = graafId;
            map = new Dictionary<Knoop, List<Segment>>();
        }

        public static Graaf buildGraaf(int graafId, List<Segment> segmentMap)
        {
            Graaf graaf = new Graaf(graafId);

            foreach (Segment segment in segmentMap)
            {
                addKnoopToGraaf(segment.beginKnoop, segment);
                addKnoopToGraaf(segment.eindKnoop, segment);
            }

            void addKnoopToGraaf(Knoop knoop, Segment segment)
            {
                if(graaf.map.ContainsKey(knoop))
                    graaf.map[knoop].Add(segment);
                else
                {
                    List<Segment> segmentLijst = new List<Segment>();
                    segmentLijst.Add(segment);
                    graaf.map.Add(knoop, segmentLijst);
                }                
            }
            return graaf;
        }

        public List<Knoop> getKnopen()
        {
            List<Knoop> knoopList = new List<Knoop>(map.Keys);
            return knoopList;
        }

        public void showGraaf()
        {

        }
    }
}
