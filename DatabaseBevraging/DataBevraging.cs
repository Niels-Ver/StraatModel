using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using StraatModel;

namespace DatabaseBevraging
{
    public class DataBevraging
    {

        private string connectionString;    

        public DataBevraging(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }


        public List<int> GetStraatIdsVoorGemeente(string gemeentenaam)
        {
            SqlConnection connection = GetConnection();

            string query = "SELECT straat.Id " +
                "FROM dbo.straatSQL AS straat " +
                "INNER JOIN gemeente_straatSQL AS gs ON straat.Id = gs.straatId " +
                "INNER JOIN gemeenteSQL AS gemeente ON gs.gemeenteId = gemeente.Id " +
                "WHERE gemeente.gemeentenaam = @gemeentenaam";

            List<int> straatIds = new List<int>();


            using (SqlCommand command = connection.CreateCommand())
            {

                connection.Open();                
                try
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@gemeentenaam", SqlDbType.NVarChar));
                    command.Parameters["@gemeentenaam"].Value = gemeentenaam;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine($"Een lijst van straatIds voor de gemeente {gemeentenaam}");
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader[0]}");
                            straatIds.Add((int)reader[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                }
                finally
                {
                    connection.Close();                  
                }
                return straatIds;
            }
        }

        public void GetStraatVoorStraatId(int Id)
        {
            SqlConnection connection = GetConnection();
            string query = "SELECT punt.x, punt.y, sp.segmentId, segment.beginknoopId, segment.eindknoopId, kp.x, kp.y, gk.knoopId, gk.graafId, straat.Id, straat.straatnaam, gemeente.gemeentenaam, provincie.provincieNaam " +
                "FROM dbo.puntSQL AS punt " +
                "INNER JOIN segment_puntSQL AS sp ON punt.Id = sp.puntId " +
                "INNER JOIN segmentSQL AS segment ON sp.segmentId = segment.Id " +
                "INNER JOIN knoop_segmentSQL AS ks ON sp.segmentId = ks.segmentId " +
                "INNER JOIN knoopSQL AS knoop ON ks.knoopId = knoop.Id " +
                "INNER JOIN puntSQL as kp ON knoop.puntId = kp.Id " +
                "INNER JOIN graaf_knoopSQL AS gk ON ks.knoopId = gk.knoopId " +
                "INNER JOIN straatSQL AS straat ON gk.graafId = straat.graafId " +
                "INNER JOIN gemeente_straatSQL AS gs ON straat.Id = gs.straatId " +
                "INNER JOIN gemeenteSQL AS gemeente ON gs.gemeenteId = gemeente.Id " +
                "INNER JOIN provincie_gemeenteSQL AS pg ON gemeente.Id = pg.gemeenteId " +
                "INNER JOIN provincieSQL AS provincie ON pg.provincieId = provincie.Id " +
                "WHERE straat.Id = @Id";

            using (SqlCommand command = connection.CreateCommand())
            {
                Dictionary<Knoop, HashSet<BevraagSegment>> map = new Dictionary<Knoop, HashSet<BevraagSegment>>();
                Dictionary<BevraagSegment, List<Punt>> verticeDictionary = new Dictionary<BevraagSegment, List<Punt>>();

                int graafId =0;
                int straatId=0;
                string straatnaam ="";
                string gemeentenaam ="";
                string provincienaam ="";

                connection.Open();                               
                try
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                    command.Parameters["@Id"].Value = Id;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Punt punt = new Punt((decimal)reader[0], (decimal)reader[1]);
                            int segmentId = (int)reader[2];
                            int beginknoopId = (int)reader[3];
                            int eindknoopId = (int)reader[4];
                            int knoopId = (int)reader[7];
                            Knoop knoop = new Knoop(knoopId, new Punt((decimal)reader[5], (decimal)reader[6]));
                            graafId = (int)reader[8];
                            straatId = (int)reader[9];
                            straatnaam = (string)reader[10];
                            gemeentenaam = (string)reader[11];
                            provincienaam = (string)reader[12];

                            //segment koppelen aan punten
                            BevraagSegment segment = new BevraagSegment(segmentId, beginknoopId, eindknoopId, knoopId);
                           if(verticeDictionary.ContainsKey(segment))
                                verticeDictionary[segment].Add(punt);
                           else
                            {
                                List<Punt> punten = new List<Punt>();
                                punten.Add(punt);
                                verticeDictionary.Add(segment, punten);
                            }

                            if (map.ContainsKey(knoop))
                                map[knoop].Add(segment);
                            else
                            {
                                HashSet<BevraagSegment> segmentlist = new HashSet<BevraagSegment>();
                                map.Add(knoop, segmentlist);
                            }                             
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                }
                finally
                {
                    connection.Close();
                }

                //Uitprinten van de informatie van de straat
                Console.WriteLine($"{straatId}, {straatnaam}, {gemeentenaam}, {provincienaam}");
                Console.WriteLine($"Graaf: {graafId}");
                Console.WriteLine($"aantal knopen: {map.Values.Count()}");
                Console.WriteLine($"aantal segmenten: {map.SelectMany(s => s.Value).Distinct().Count()}");

                foreach (Knoop knoop in map.Keys)
                {
                    Console.WriteLine($"Knoop[{knoop.knoopID}, [{knoop.punt.x},{knoop.punt.y}]]");
                    foreach (BevraagSegment segment in map[knoop])
                    {
                        Console.WriteLine($"\t[Segment:{segment.segmentId}, begin:{segment.beginknoopId}, eind:{segment.eindknoopId}");
                        foreach (Punt punt in verticeDictionary[segment])
                        {
                            Console.WriteLine($"\t\t{punt}");
                        }
                    }
                }
            }
        }

        public void GetStraatVoorStraatnaamEnGemeente(string straatnaam, string gemeentenaam)
        {
            SqlConnection connection = GetConnection();
            string query = "SELECT punt.x, punt.y, sp.segmentId, segment.beginknoopId, segment.eindknoopId, kp.x, kp.y, gk.knoopId, gk.graafId, straat.Id, straat.straatnaam, gemeente.gemeentenaam, provincie.provincieNaam " +
                "FROM dbo.puntSQL AS punt " +
                "INNER JOIN segment_puntSQL AS sp ON punt.Id = sp.puntId " +
                "INNER JOIN segmentSQL AS segment ON sp.segmentId = segment.Id " +
                "INNER JOIN knoop_segmentSQL AS ks ON sp.segmentId = ks.segmentId " +
                "INNER JOIN knoopSQL AS knoop ON ks.knoopId = knoop.Id " +
                "INNER JOIN puntSQL as kp ON knoop.puntId = kp.Id " +
                "INNER JOIN graaf_knoopSQL AS gk ON ks.knoopId = gk.knoopId " +
                "INNER JOIN straatSQL AS straat ON gk.graafId = straat.graafId " +
                "INNER JOIN gemeente_straatSQL AS gs ON straat.Id = gs.straatId " +
                "INNER JOIN gemeenteSQL AS gemeente ON gs.gemeenteId = gemeente.Id " +
                "INNER JOIN provincie_gemeenteSQL AS pg ON gemeente.Id = pg.gemeenteId " +
                "INNER JOIN provincieSQL AS provincie ON pg.provincieId = provincie.Id " +
             "WHERE straat.straatnaam = @straatnaam AND gemeente.gemeentenaam = @gemeentenaam";

            using (SqlCommand command = connection.CreateCommand())
            {

                Dictionary<Knoop, HashSet<BevraagSegment>> map = new Dictionary<Knoop, HashSet<BevraagSegment>>();
                Dictionary<BevraagSegment, List<Punt>> verticeDictionary = new Dictionary<BevraagSegment, List<Punt>>();

                int graafId = 0;
                int straatId = 0;
                string snaam = "";
                string gnaam = "";
                string provincienaam = "";

                connection.Open();                    
                //Dictionary<Punt, int> puntDictionary = new Dictionary<Punt, int>();      
                
                try                
                {                
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@straatnaam", SqlDbType.NVarChar));
                    command.Parameters.Add(new SqlParameter("@gemeentenaam", SqlDbType.NVarChar));
                    command.Parameters["@straatnaam"].Value = straatnaam;
                    command.Parameters["@gemeentenaam"].Value = gemeentenaam;
                    using (SqlDataReader reader = command.ExecuteReader())                        
                    {
                        while (reader.Read())
                        {
                            Punt punt = new Punt((decimal)reader[0], (decimal)reader[1]);
                            int segmentId = (int)reader[2];
                            int beginknoopId = (int)reader[3];
                            int eindknoopId = (int)reader[4];
                            int knoopId = (int)reader[7];
                            Knoop knoop = new Knoop(knoopId, new Punt((decimal)reader[5], (decimal)reader[6]));
                            graafId = (int)reader[8];
                            straatId = (int)reader[9];
                            snaam = (string)reader[10];
                            gnaam = (string)reader[11];
                            provincienaam = (string)reader[12];

                            //segment koppelen aan punten
                            BevraagSegment segment = new BevraagSegment(segmentId, beginknoopId, eindknoopId, knoopId);
                            if (verticeDictionary.ContainsKey(segment))
                                verticeDictionary[segment].Add(punt);
                            else
                            {
                                List<Punt> punten = new List<Punt>();
                                punten.Add(punt);
                                verticeDictionary.Add(segment, punten);
                            }

                            if (map.ContainsKey(knoop))
                                map[knoop].Add(segment);
                            else
                            {
                                HashSet<BevraagSegment> segmentlist = new HashSet<BevraagSegment>();
                                map.Add(knoop, segmentlist);
                            }
                        }
                    }                   
                }                
                catch (Exception ex)                
                {                
                    Console.WriteLine(ex);
                                        
                }                
                finally               
                {                
                    connection.Close();                    
                }

                //Uitprinten van de informatie van de straat
                Console.WriteLine($"{straatId}, {snaam}, {gnaam}, {provincienaam}");
                Console.WriteLine($"Graaf: {graafId}");
                Console.WriteLine($"aantal knopen: {map.Values.Count()}");
                Console.WriteLine($"aantal segmenten: {map.SelectMany(s => s.Value).Distinct().Count()}");

                foreach (Knoop knoop in map.Keys)
                {
                    Console.WriteLine($"Knoop[{knoop.knoopID}, [{knoop.punt.x},{knoop.punt.y}]]");
                    foreach (BevraagSegment segment in map[knoop])
                    {
                        Console.WriteLine($"\t[Segment:{segment.segmentId}, begin:{segment.beginknoopId}, eind:{segment.eindknoopId}");
                        foreach (Punt punt in verticeDictionary[segment])
                        {
                            Console.WriteLine($"\t\t{punt}");
                        }
                    }
                }
            }
        }

        public void GetStraatnamenVoorGemeente(string gemeentenaam)
        {
            SqlConnection connection = GetConnection();

            string query = "SELECT straat.straatnaam " +
                "FROM dbo.straatSQL AS straat " +
                "INNER JOIN gemeente_straatSQL AS gs ON straat.Id = gs.straatId " +
                "INNER JOIN gemeenteSQL AS gemeente ON gs.gemeenteId = gemeente.Id " +
                "WHERE gemeente.gemeentenaam = @gemeentenaam " +
                "ORDER BY straat.straatnaam ASC";

            using (SqlCommand command = connection.CreateCommand())
            {

                connection.Open();
                try
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@gemeentenaam", SqlDbType.NVarChar));
                    command.Parameters["@gemeentenaam"].Value = gemeentenaam;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine($"Een lijst van straatNamen voor de gemeente {gemeentenaam}");
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader[0]}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
