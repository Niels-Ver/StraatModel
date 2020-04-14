using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace StraatModel
{
    public class DataReader
    {
        public void leesData()
        {
            char[] seperator = { ';' };
            string[] gesplitsteData;
            string data;

            //Uitlezen van WRdata en aanmaken van een segmentenMap
            Dictionary<int, List<Segment>> segmentMap = new Dictionary<int, List<Segment>>();
            using (StreamReader r = new StreamReader(@"D:\Niels\School\Prog3\WRdata\WRdata.csv"))
            {
                Dictionary<int, Knoop> knoopMap = new Dictionary<int, Knoop>();
                r.ReadLine();  //eerste lijn staat info over bestand, dit is niet nodig
                while ((data = r.ReadLine()) != null)
                {
                    gesplitsteData = data.Split(seperator);

                    int linkerstraatId = int.Parse(gesplitsteData[6]);
                    int rechterStraatId = int.Parse(gesplitsteData[7]);

                    if (!(linkerstraatId == -9 && rechterStraatId == -9))
                    {
                        string geo = gesplitsteData[1];

                        geo = geo.Substring(11);
                        geo = geo.Trim('(', ')');

                        string[] splitGeo = geo.Split(',');

                        List<Punt> vertices = new List<Punt>();
                        foreach (string coords in splitGeo)
                        {
                            string coordsTrim = coords.Trim();
                            String[] splitCoords = coordsTrim.Split(' ');

                            //Console.WriteLine($"x Coord = {splitCoords[0]}, y coord = {splitCoords[1]}");
                            Punt punt = new Punt(double.Parse(splitCoords[0].Replace('.', ',')), double.Parse(splitCoords[1].Replace('.', ',')));
                            vertices.Add(punt);
                        }

                        //Opmaken van het ingelezen wegsegment
                        int wegSegmentId = int.Parse(gesplitsteData[0]);
                        int beginWegknoopID = int.Parse(gesplitsteData[4]);
                        int eindWegknoopID = int.Parse(gesplitsteData[5]);

                        if (!knoopMap.ContainsKey(beginWegknoopID))
                        {
                            Knoop beginKnoop = new Knoop(beginWegknoopID, vertices.First());
                            knoopMap.Add(beginWegknoopID, beginKnoop);
                        }

                        if (!knoopMap.ContainsKey(eindWegknoopID))
                        {
                            Knoop eindKnoop = new Knoop(eindWegknoopID, vertices.First());
                            knoopMap.Add(eindWegknoopID, eindKnoop);
                        }

                        Segment segment = new Segment(wegSegmentId, knoopMap[beginWegknoopID], knoopMap[eindWegknoopID], vertices);

                        addToSegmentMap(linkerstraatId, segment);
                        addToSegmentMap(rechterStraatId, segment);
                    }
                }
            }

            void addToSegmentMap(int straatId, Segment segment)
            {
                //checken of de linkerstraat een geldige input heeft
                if (straatId != -9)
                {
                    //kijken of de linkerstraat al een bestaande graaf in de map heeft
                    if (segmentMap.ContainsKey(straatId))
                    {
                        //Kijken of het aangemaakte segment nog niet aanwezig is in de graaf
                        if (!segmentMap[straatId].Contains(segment))
                        {
                            segmentMap[straatId].Add(segment);
                        }
                    }
                    else
                    {
                        List<Segment> segmentList = new List<Segment>();
                        segmentList.Add(segment);
                        segmentMap.Add(straatId, segmentList);
                    }

                }
            }

            //Uitlezen van Straatnamen bij straatId en aanmaken van een dictionary van alle straten.
            Dictionary<int, String> straatNaamDictionary = new Dictionary<int, String>();
            Dictionary<int, Straat> straatDictionary = new Dictionary<int, Straat>();
            using (StreamReader r = new StreamReader(@"D:\Niels\School\Prog3\WRdata\WRstraatnamen.csv"))
            {
                r.ReadLine();  //eerste lijn staat info over bestand, dit is niet nodig
                r.ReadLine();  //2de lijn staat ook onnodige info
                while ((data = r.ReadLine()) != null)
                {                        
                    gesplitsteData = data.Split(seperator);                        
                    int straatId = int.Parse(gesplitsteData[0]);                        
                    string straatNaam = gesplitsteData[1];                                               
                    straatNaamDictionary.Add(straatId,straatNaam.Trim());
                }

                foreach (int straatId in segmentMap.Keys)
                {
                    int graafId = 1;
                    straatDictionary.Add(straatId, new Straat(straatId, straatNaamDictionary[straatId], Graaf.buildGraaf(graafId, segmentMap[straatId])));
                    graafId++;
                }
            }


            //Uitlezen van link tussen gemeenteId en straatId
            Dictionary<int, List<Straat>> gemeenteStraatIdDictionary = new Dictionary<int, List<Straat>>();
            using (StreamReader r = new StreamReader(@"D:\Niels\School\Prog3\WRdata\WRGemeenteID.csv"))
            {
                r.ReadLine();  //eerste lijn staat info over bestand, dit is niet nodig
                while ((data = r.ReadLine()) != null)
                {
                    gesplitsteData = data.Split(seperator);

                    int straatnaamId = int.Parse(gesplitsteData[0]);
                    int gemeenteId = int.Parse(gesplitsteData[1]);
                    if(segmentMap.ContainsKey(straatnaamId))
                    {
                        if (gemeenteStraatIdDictionary.ContainsKey(gemeenteId))
                            gemeenteStraatIdDictionary[gemeenteId].Add(straatDictionary[straatnaamId]);
                        else
                        {
                            List<Straat> straatList = new List<Straat>();
                            straatList.Add(straatDictionary[straatnaamId]);
                            gemeenteStraatIdDictionary.Add(gemeenteId, straatList);
                        }
                    }                
                }
            }

            //Uitlezen van Gemeentenamen en aanmaken dictionary van alle gemeentes
            Dictionary<int, Gemeente> gemeenteDictionary = new Dictionary<int, Gemeente>();
            using (StreamReader r = new StreamReader(@"D:\Niels\School\Prog3\WRdata\WRGemeentenaam.csv"))
            {
                r.ReadLine();  //eerste lijn staat info over bestand, dit is niet nodig
                while ((data = r.ReadLine()) != null)
                {
                    gesplitsteData = data.Split(seperator);
                    if (gesplitsteData[2] == "nl")
                    {
                        int gemeenteId = int.Parse(gesplitsteData[1]);
                        string gemeenteNaam = gesplitsteData[3];

                        //Lijst opmaken met alle straten die bij deze gemeente horen
                        if(gemeenteStraatIdDictionary.ContainsKey(gemeenteId))
                        {
                            gemeenteDictionary.Add(gemeenteId, new Gemeente(gemeenteId, gemeenteNaam.Trim(), gemeenteStraatIdDictionary[gemeenteId]));
                        }                      
                    }
                }
            }


            //Uitlezen Province Id en Provincie Naam
            Dictionary<int, Provincie> provincieDictionary = new Dictionary<int, Provincie>();
            List<int> provincieIdList = new List<int>();
            using (StreamReader r = new StreamReader(@"D:\Niels\School\Prog3\WRdata\ProvincieInfo.csv"))
            {
                r.ReadLine();  //eerste lijn staat info over bestand, dit is niet nodig
                while ((data = r.ReadLine()) != null)
                {
                    gesplitsteData = data.Split(seperator);

                    int gemeenteId = int.Parse(gesplitsteData[0]);
                    int provincieId = int.Parse(gesplitsteData[1]);
                    string taalcode = gesplitsteData[2];
                    string provincieNaam = gesplitsteData[3];

                    if (taalcode == "nl")
                    {
                        if(gemeenteDictionary.ContainsKey(gemeenteId))
                        {
                            if (provincieDictionary.ContainsKey(provincieId))
                                provincieDictionary[provincieId].gemeentes.Add(gemeenteDictionary[gemeenteId]);
                            else
                            {
                                List<Gemeente> gemeentes = new List<Gemeente>();
                                gemeentes.Add(gemeenteDictionary[gemeenteId]);
                                Provincie toeTeVoegenProvincie = new Provincie(provincieId, provincieNaam.Trim(), gemeentes);
                                provincieDictionary.Add(provincieId, toeTeVoegenProvincie);
                            }
                        }
                    }
                }
            }

            //Uitlezen te verwerken Provincies en een Dictionairy opmaken met enkel deze provincies
            Dictionary<int, Provincie> finaleDictionary = new Dictionary<int, Provincie>();
            using (StreamReader r = new StreamReader(@"D:\Niels\School\Prog3\WRdata\ProvincieIDsVlaanderen.csv"))
            {
                data = r.ReadLine();
                gesplitsteData = data.Split(',');
                
                foreach (String id in gesplitsteData)
                {
                    var filteredDictionary = provincieDictionary.Where(p => p.Key == int.Parse(id));
                    foreach (var dictionary in filteredDictionary)
                    {
                        finaleDictionary.Add(dictionary.Key, dictionary.Value);
                    }
                }
            }


            generateRaport(finaleDictionary);
            serializeData(finaleDictionary);
        }

        public void generateRaport(Dictionary<int, Provincie> provincies)
        {
            string path = @"D:\Niels\School\Prog3\Rapport.txt";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                int totaalAantalStratenProvincies = 0;
                foreach (Provincie provincie in provincies.Values)
                {
                    var straten = provincie.gemeentes.Select(g => g.straatList.Count());
                    foreach (var item in straten)
                    {
                        totaalAantalStratenProvincies += item;
                    }
                }

                sw.WriteLine($"Totaal aantal straten: {totaalAantalStratenProvincies}\n");
                sw.WriteLine("Totaal aantal straten per provincie");
                foreach (int provincieId in provincies.Keys)
                {
                    int aantalStraten = 0;
                    var straten = provincies[provincieId].gemeentes.Select(g => g.straatList.Count());
                    foreach (var item in straten)
                    {
                        aantalStraten += item;
                    }

                    sw.WriteLine($"\t{provincies[provincieId].provincieNaam} : {aantalStraten}");
                }
                sw.WriteLine("\n");


                foreach (int provincieId in provincies.Keys)
                {

                    sw.WriteLine("StraatInfo " + provincies[provincieId].provincieNaam);
                    foreach (Gemeente gemeente in provincies[provincieId].gemeentes)
                    {
                        double totaleLengte = 0;
                        Straat kortsteStraat = gemeente.straatList[0];
                        Straat langsteStraat = gemeente.straatList[0];
                        double lengteKortsteStraat = 0;
                        double lengteLangsteStraat = 0;
                        for (int i = 0; i < gemeente.straatList.Count; i++)
                        {
                            Straat straat = gemeente.straatList[i];
                            double straatLengte = calculateLength(straat);
                            totaleLengte += straatLengte;
                            if (i == 0)
                            {
                                kortsteStraat = straat;
                                langsteStraat = straat;
                                lengteKortsteStraat = straatLengte;
                                lengteLangsteStraat = straatLengte;
                            }
                            else
                            {
                                if (straatLengte > lengteLangsteStraat)
                                {
                                    langsteStraat = straat;
                                    lengteLangsteStraat = straatLengte;
                                }
                                else if (straatLengte < lengteKortsteStraat)
                                {
                                    kortsteStraat = straat;
                                    lengteKortsteStraat = straatLengte;
                                }
                            }
                        }

                        sw.WriteLine($"\t* {gemeente.gemeenteNaam} : <{gemeente.straatList.Count()}>, <{Math.Round(totaleLengte, 2)}>");
                        sw.WriteLine($"\t\t kortste straat: {kortsteStraat.straatnaam.Trim()} met Id: {kortsteStraat.straatId} met een lengte van {Math.Round(lengteKortsteStraat, 2)}");
                        sw.WriteLine($"\t\t langste straat: {langsteStraat.straatnaam.Trim()} met Id: {langsteStraat.straatId} met een lengte van {Math.Round(lengteLangsteStraat, 2)}");
                    }
                }
            }
            double calculateLength(Straat straat)
            {
                HashSet<Segment> uniqueSegments = new HashSet<Segment>();
                //Alle segmenten uit de graaf in 1 collectie steken
                IEnumerable<Segment> segmentList = straat.graaf.map.SelectMany(x => x.Value);

                //Segmenten in een hashset steken zodat we segmenten geen 2 keer bij de lengte bij rekenen
                foreach (Segment segment in segmentList)
                {
                    uniqueSegments.Add(segment);
                }

                double totaleLengte = 0;
                foreach (Segment segment in uniqueSegments)
                {
                    for (int i = 0; i < segment.vertices.Count - 1; i++)
                    {
                        Punt punt1 = segment.vertices[i];
                        Punt punt2 = segment.vertices[i + 1];
                        double lengteTussenPunten = Math.Sqrt(Math.Pow((punt1.x - punt2.x), 2) + Math.Pow((punt1.y - punt2.y), 2));
                        totaleLengte += lengteTussenPunten;
                    }
                }
                return totaleLengte;
            }
        }

        public void serializeData(Dictionary<int, Provincie> provincies)
        {
            string path = @"D:\Niels\School\Prog3\XMLSerial.xml";
            var serializer = new DataContractSerializer(provincies.GetType());
            string xmlString;
            using (var sw = new StreamWriter(path))
            {
                using (var writer = new XmlTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented; // indent the Xml so it's human readable
                    serializer.WriteObject(writer, provincies);
                    writer.Flush();
                    xmlString = sw.ToString();
                }
            }
        }        
    }
}
