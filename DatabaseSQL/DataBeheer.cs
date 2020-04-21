using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using StraatModel;

namespace DatabaseSQL
{
    public class DataBeheer
    {
        private string connectionString;

        public DataBeheer(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private SqlConnection getConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }

        public void SchrijfDataNaarDatabank(Dictionary<int, Provincie> provincies)
        {
            //Alle Punten uit de verzameling ophalen en uploaden in de database

            HashSet<Punt> knoopPunten = provincies.Select(p => p.Value).SelectMany(p => p.gemeentes).SelectMany(g => g.straatList).SelectMany(s => s.graaf.map).Select(k => k.Key).Select(p => p.punt).ToHashSet();
            HashSet<Punt> segmentPunten = provincies.Select(p => p.Value).SelectMany(p => p.gemeentes).SelectMany(g => g.straatList).SelectMany(s => s.graaf.map).SelectMany(v => v.Value).SelectMany(v => v.vertices).ToHashSet();

            uploadPunten(knoopPunten);
            uploadPunten(segmentPunten);

            //Alle Knopen ophalen en uploaden in de database
            HashSet<Knoop> knopen = provincies.Select(p => p.Value).SelectMany(p => p.gemeentes).SelectMany(g => g.straatList).SelectMany(s => s.graaf.map).Select(k => k.Key).ToHashSet();
            uploadKnopen(knopen);


            //Dit gaat niet correct zijn er zullen segmenten zijn met dezelfde segmentId waar de waardes niet hetzelfde zijn. Kijken voor op te lossen na alles werkt
            HashSet<Segment> segmenten = provincies.Select(p => p.Value).SelectMany(p => p.gemeentes).SelectMany(g => g.straatList).SelectMany(s => s.graaf.map).SelectMany(s => s.Value).ToHashSet();
            UploadSegmenten(segmenten);

            Dictionary<int, List<int>> gemeenteStraatDictionary = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> graafKnoopIdDictionary = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> knoopSegmentDictionary = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> segmentPuntDictionary = new Dictionary<int, List<int>>();
            

            foreach (Provincie provincie in provincies.Values)
            {
                //Voeg de Provintie Toe
                int provincieId = VoegProvincieToe(provincie);

                foreach (Gemeente gemeente in provincie.gemeentes)
                {
                    //Voeg de gemeente toe
                    int gemeenteId = VoegGemeenteToe(gemeente);

                    //Koppel de gemeentes aan de provintie
                    KoppelGemeenteAanProvintie(provincieId, gemeenteId);

                    List<int> graafList = gemeente.straatList.Select(straat => straat.graaf.graafId).ToList();
                    UploadGraaf(graafList);

                    //Upload alle straten voor de gemeente
                    UploadStraat(gemeente.straatList);

                    //Koppel de straten aan de gemeente
                    List<int> straatIdList = gemeente.straatList.Select(s => s.straatId).ToList();
                    gemeenteStraatDictionary.Add(gemeenteId, straatIdList);

                    foreach (Straat straat in gemeente.straatList)
                    {
                        List<int> knopenIdLijst = new List<Knoop>(straat.graaf.map.Keys).Select(k=>k.knoopID).ToList();
                        graafKnoopIdDictionary.Add(straat.graaf.graafId, knopenIdLijst);

                        foreach (Knoop knoop in straat.graaf.map.Keys)
                        {
                            List<int> segmentIds = straat.graaf.map[knoop].Select(s => s.segmentId).ToList();
                            if(!(knoopSegmentDictionary.ContainsKey(knoop.knoopID)))
                                knoopSegmentDictionary.Add(knoop.knoopID, segmentIds);
                        }
                    }
                }
            }

            KoppelStraatAanGemeente(gemeenteStraatDictionary);
            KoppelKnoopAanGraaf(graafKnoopIdDictionary);
            KoppelSegmentAanKnoop(knoopSegmentDictionary);
            KoppelPuntAanSegment(segmenten);

        }

        private int VoegProvincieToe(Provincie p)
        {
            SqlConnection connection = getConnection();
            string query = "INSERT INTO dbo.provincieSQL (provincienaam) output INSERTED.ID VALUES(@provincienaam)";
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@provincienaam", SqlDbType.NVarChar));
                    command.CommandText = query;
                    command.Parameters["@provincienaam"].Value = p.provincieNaam;
                    return (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return -9;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private int VoegGemeenteToe(Gemeente g)
        {
            SqlConnection connection = getConnection();
            string query = "INSERT INTO dbo.gemeenteSQL (gemeentenaam) output INSERTED.ID VALUES(@gemeentenaam)";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                { 
                    command.Parameters.Add(new SqlParameter("@gemeentenaam", SqlDbType.NVarChar));
                    command.CommandText = query;
                    command.Parameters["@gemeentenaam"].Value = g.gemeenteNaam;
                    return (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return -9;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void KoppelGemeenteAanProvintie(int provintieId, int gemeenteId)
        {
            SqlConnection connection = getConnection();
            string queryS = "INSERT INTO dbo.provincie_gemeenteSQL(provincieId,gemeenteId) VALUES(@provincieId,@gemeenteId)";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@provincieId", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@gemeenteId", SqlDbType.Int));

                    command.CommandText = queryS;
                    command.Parameters["@provincieId"].Value = provintieId;
                    command.Parameters["@gemeenteId"].Value = gemeenteId;
                    command.ExecuteNonQuery();

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

        private void KoppelStraatAanGemeente(Dictionary<int, List<int>> gemeenteStraatIdDictionary)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("gemeenteId", typeof(int));
                        table.Columns.Add("straatId", typeof(int));

                        foreach (int gemeenteId in gemeenteStraatIdDictionary.Keys)
                        {
                            foreach (int straatId in gemeenteStraatIdDictionary[gemeenteId])
                            {
                                table.Rows.Add(gemeenteId, straatId);
                            }
                        }

                        bulkCopy.DestinationTableName = "dbo.gemeente_straatSQL";
                        bulkCopy.ColumnMappings.Add("gemeenteId", "gemeenteId");
                        bulkCopy.ColumnMappings.Add("straatId", "straatId");
                        bulkCopy.WriteToServer(table);

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

        private void KoppelKnoopAanGraaf(Dictionary<int, List<int>> graafKnoopIdDictionary)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("graafId", typeof(int));
                        table.Columns.Add("knoopId", typeof(int));

                        foreach (int graafId in graafKnoopIdDictionary.Keys)
                        {
                            foreach (int knoopId in graafKnoopIdDictionary[graafId])
                            {
                                table.Rows.Add(graafId, knoopId);
                            }                           
                        }

                        bulkCopy.DestinationTableName = "dbo.graaf_knoopSQL";
                        bulkCopy.ColumnMappings.Add("graafId", "graafId");
                        bulkCopy.ColumnMappings.Add("knoopId", "knoopId");
                        bulkCopy.WriteToServer(table);

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

        private void KoppelSegmentAanKnoop(Dictionary<int, List<int>> knoopSegmentDictionary)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("knoopId", typeof(int));
                        table.Columns.Add("segmentId", typeof(int));

                        foreach (int knoopId in knoopSegmentDictionary.Keys)
                        {
                            foreach (int segmentId in knoopSegmentDictionary[knoopId])
                            {
                                table.Rows.Add(knoopId, segmentId);
                            }
                        }

                        bulkCopy.DestinationTableName = "dbo.knoop_segmentSQL";
                        bulkCopy.ColumnMappings.Add("knoopId", "knoopId");
                        bulkCopy.ColumnMappings.Add("segmentId", "segmentId");
                        bulkCopy.WriteToServer(table);

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

        private void KoppelPuntAanSegment(HashSet<Segment> segmenten)
        {
            string query = "SELECT * FROM dbo.puntSQL";
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                Dictionary<Punt, int> puntDictionary = new Dictionary<Punt, int>();
                try
                {
                    command.CommandText = query;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Punt punt = new Punt((decimal)(reader["x"]), (decimal)(reader["y"]));
                            if (!(puntDictionary.ContainsKey(punt)))
                                puntDictionary.Add(punt, (int)reader["Id"]);
                        }
                    }

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("segmentId", typeof(int));
                        table.Columns.Add("puntId", typeof(int));

                        foreach (Segment segment in segmenten)
                        {
                            foreach (Punt punt in segment.vertices)
                            {
                                table.Rows.Add(segment.segmentId, puntDictionary[punt]);
                            }
                        }

                        bulkCopy.DestinationTableName = "dbo.segment_puntSQL";
                        bulkCopy.ColumnMappings.Add("segmentId", "segmentId");
                        bulkCopy.ColumnMappings.Add("puntId", "puntId");
                        bulkCopy.WriteToServer(table);

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

        private void UploadStraat(List<Straat> straten)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("Id", typeof(int));
                        table.Columns.Add("straatnaam", typeof(string));
                        table.Columns.Add("graafId", typeof(int));

                        foreach (Straat straat in straten)
                        {
                            table.Rows.Add(straat.straatId, straat.straatnaam, straat.graaf.graafId);

                        }

                        bulkCopy.DestinationTableName = "dbo.straatSQL";
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("straatnaam", "straatnaam");
                        bulkCopy.ColumnMappings.Add("graafId", "graafId");
                        bulkCopy.WriteToServer(table);

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

        private void UploadGraaf(IEnumerable<int> graafList)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("Id", typeof(int));

                        foreach (int graafId in graafList)
                        {
                            table.Rows.Add(graafId);

                        }

                        bulkCopy.DestinationTableName = "dbo.graafSQL";
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.WriteToServer(table);

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

        private void UploadSegmenten(HashSet<Segment> segmenten)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("Id", typeof(int));
                        table.Columns.Add("beginknoopId", typeof(int));
                        table.Columns.Add("eindknoopId", typeof(int));

                        foreach (Segment segment in segmenten)
                        {
                            table.Rows.Add(segment.segmentId, segment.beginKnoop.knoopID, segment.eindKnoop.knoopID);
                        }

                        bulkCopy.DestinationTableName = "dbo.segmentSQL";
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("beginknoopId", "beginknoopId");
                        bulkCopy.ColumnMappings.Add("eindknoopId", "eindknoopId");
                        bulkCopy.WriteToServer(table);

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


        private Dictionary<Knoop, int> getKnopen()
        {
            SqlConnection connection = getConnection();
            //string query = "SELECT knoopSQL.Id,puntSQL.Id, puntSQL.x, puntSQL.y  FROM [dbo][knoopSQL] INNER JOIN [dbo][puntSQL] ON [dbo][knoopSQL].puntId = [dbo][puntSQL].Id";
            string query = "SELECT knoopSQL.Id, puntSQL.x, puntSQL.y FROM [dbo].[knoopSQL] INNER JOIN [dbo].[puntSQL] ON [dbo].[knoopSQL].puntId = [dbo].[puntSQL].Id ";
            using (SqlCommand command = connection.CreateCommand())
            {

                connection.Open();
                Dictionary<Knoop, int> knoopDictionary = new Dictionary<Knoop, int>();
                try
                {
                    command.CommandText = query;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Punt punt = new Punt((decimal)(reader["x"]), (decimal)(reader["y"]));

                            Knoop knoop = new Knoop((int)(reader["Id"]), punt);
                            if (!(knoopDictionary.ContainsKey(knoop)))
                                knoopDictionary.Add(knoop, knoop.knoopID);
                        }
                    }
                    return knoopDictionary;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private Dictionary<Punt, int> getPunten()
        {
            SqlConnection connection = getConnection();
            string query = "SELECT * FROM dbo.puntSQL";
            using (SqlCommand command1 = connection.CreateCommand())
            {
                connection.Open();
                Dictionary<Punt, int> puntDictionary = new Dictionary<Punt, int>();
                try
                {
                    command1.CommandText = query;
                    using (SqlDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Punt punt = new Punt((decimal)(reader["x"]), (decimal)(reader["y"]));
                            if (!(puntDictionary.ContainsKey(punt)))
                                puntDictionary.Add(punt, (int)reader["Id"]);
                        }
                    }
                    return puntDictionary;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void uploadPunten(HashSet<Punt> punten)
        {
            SqlConnection connection = getConnection();
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("x", typeof(decimal));
                        table.Columns.Add("y", typeof(decimal));

                        foreach (Punt punt in punten)
                        {
                            table.Rows.Add(punt.x,punt.y);
                        }

                        bulkCopy.DestinationTableName = "dbo.puntSQL";
                        bulkCopy.ColumnMappings.Add("x", "x");
                        bulkCopy.ColumnMappings.Add("y", "y");
                        bulkCopy.WriteToServer(table);

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

        private void uploadKnopen(HashSet<Knoop> knopen)
        {
            SqlConnection connection = getConnection();
            string query = "SELECT * FROM dbo.puntSQL";
            using (SqlCommand command1 = connection.CreateCommand())
            {
                connection.Open();
                Dictionary<Punt, int> puntDictionary = new Dictionary<Punt, int>();
                try
                {
                    command1.CommandText = query;
                    using(SqlDataReader reader = command1.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            Punt punt = new Punt((decimal)(reader["x"]), (decimal)(reader["y"]));
                            if(!(puntDictionary.ContainsKey(punt)))
                                puntDictionary.Add(punt,(int)reader["Id"]);
                        }
                    }

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("Id", typeof(int));
                        table.Columns.Add("puntId", typeof(int));

                        

                        foreach (Knoop knoop in knopen)
                        {
                            if (!(puntDictionary.ContainsKey(knoop.punt)))
                                Console.WriteLine(knoop.punt.x + " " + knoop.punt.y);
                            else
                                table.Rows.Add(knoop.knoopID, puntDictionary[knoop.punt]);
                        }

                        bulkCopy.DestinationTableName = "dbo.knoopSQL";
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("puntId", "puntId");
                        bulkCopy.WriteToServer(table);

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
