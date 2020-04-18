using System;
using System.Collections.Generic;
using System.Linq;
using StraatModel;

namespace DatabaseSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReader xr = new XmlReader();
            xr.ReadFile();

            Dictionary<int, Provincie> provincies = xr.getProvincies();

            string connectionString = @"Data Source=DESKTOP-FUU1BI0\SQLEXPRESS;Initial Catalog=wegenNetwerk;Integrated Security=True";
            DataBeheer db = new DataBeheer(connectionString);

            HashSet<Punt> finalePunten = new HashSet<Punt>();
            HashSet<Knoop> finaleKnopen = new HashSet<Knoop>();

            foreach (Provincie provincie in provincies.Values)
            {
                //Voeg provincies toe    
                //db.VoegProvincieToe(provincie);

                List<int> gemeenteIds = new List<int>();
                foreach (Gemeente gemeente in provincie.gemeentes)
                {
                    //Voeg gemeentes toe
                    Console.WriteLine(gemeente.gemeenteNaam);
                    //db.VoegGemeenteToe(gemeente);

                    gemeenteIds.Add(gemeente.gemeenteId);


                    List<int> straatIds = new List<int>();
                    List<Straat> straten = new List<Straat>();
                    foreach (Straat straat in gemeente.straatList)
                    {
                        //Steek alle punten van de wegsegmenten inde straat in 1 collectie
                        //List<Punt> punten = straat.graaf.map.SelectMany(x => x.Value).SelectMany(x => x.vertices).ToList();
                        // totalePunten += punten.Count;
                        //Doe een bulk upload van alle punten

                        //foreach (Punt punt in punten)
                        //{
                        //    finalePunten.Add(punt);
                        //}

                        //List<Knoop> knopen = straat.graaf.getKnopen();
                        //foreach (Knoop knoop in knopen)
                        //{
                        //    finaleKnopen.Add(knoop);
                        //}

                        straten.Add(straat);

                        straatIds.Add(straat.straatId);
                    }
                    //db.UploadStraat(straten);
                    db.KoppelStraatAanGemeente(gemeente.gemeenteId, straatIds);
                }
                //db.KoppelGemeenteAanProvintie(provincie.provincieId, gemeenteIds);
            }
            
            Console.WriteLine("punten: " +  finalePunten.Count);
            //db.uploadPunten(finalePunten.ToList());
            Console.WriteLine("knopen " + finaleKnopen.Count);
            //db.uploadKnopen(finaleKnopen.ToList());

            //Voeg gemeentes toe

        }
    }
}
