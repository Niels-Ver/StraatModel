using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StraatModel
{
    [DataContract]
    public class Provincie
    {
        [DataMember]
        public int provincieId { get; set; }
        [DataMember]
        public string provincieNaam { get; set; }
        [DataMember]
        public List<Gemeente> gemeentes { get; set; }

        public Provincie(int provincieId, string provincieNaam, List<Gemeente> gemeentes)
        {
            this.provincieId = provincieId;
            this.provincieNaam = provincieNaam;
            this.gemeentes = gemeentes;
        }

        public override bool Equals(object obj)
        {
            return obj is Provincie provincie &&
                   provincieId == provincie.provincieId &&
                   provincieNaam == provincie.provincieNaam;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(provincieId, provincieNaam);
        }
    }
}
