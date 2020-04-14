using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Gemeente
    {
        [DataMember]
        public int gemeenteId { get; set; }
        [DataMember]
        public string gemeenteNaam { get; set; }
        [DataMember]
        public List<Straat> straatList { get; set; }

        public Gemeente(int gemeenteId, string gemeenteNaam, List<Straat> straatList)
        {
            this.gemeenteId = gemeenteId;
            this.gemeenteNaam = gemeenteNaam;
            this.straatList = straatList;
        }

        public override bool Equals(object obj)
        {
            return obj is Gemeente gemeente &&
                   gemeenteId == gemeente.gemeenteId &&
                   gemeenteNaam == gemeente.gemeenteNaam;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(gemeenteId, gemeenteNaam);
        }
    }
}
