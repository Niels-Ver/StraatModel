using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Straat
    {
        [DataMember]
        public Graaf graaf { get; set; }
        [DataMember]
        public int straatId { get; set; }
        [DataMember]
        public string straatnaam { get; set; }


        public Straat(int straatId, string straatnaam, Graaf graaf)
        {
            this.straatId = straatId;
            this.straatnaam = straatnaam;
            this.graaf = graaf;
        }

        public List<Knoop> getKnopen()
        {
            return graaf.getKnopen();
        }

        public void showStraat()
        {

        }

        public override bool Equals(object obj)
        {
            return obj is Straat straat &&
                   straatId == straat.straatId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(straatId);
        }
    }
}
