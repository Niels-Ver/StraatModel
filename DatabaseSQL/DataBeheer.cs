using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public void VoegProvincieToe(Provincie p)
        {
            SqlConnection connection = getConnection();
            string query = "INSERT INTO dbo.provincieSQL (Id, provincienaam) VALUES(@Id, @provincienaam)";
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@provincienaam", SqlDbType.NVarChar));
                    command.CommandText = query;
                    command.Parameters["@Id"].Value = p.provincieId;
                    command.Parameters["@provincienaam"].Value = p.provincieNaam;
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

        public void VoegGemeenteToe(Gemeente g)
        {
            SqlConnection connection = getConnection();
            string query = "INSERT INTO dbo.gemeenteSQL (Id, gemeentenaam) VALUES(@Id, @gemeentenaam)";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                { 
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@gemeentenaam", SqlDbType.NVarChar));
                    command.CommandText = query;
                    command.Parameters["@Id"].Value = g.gemeenteId;
                    command.Parameters["@gemeentenaam"].Value = g.gemeenteNaam;
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

        public void KoppelGemeenteAanProvintie(int provintieId, List<int> gemeenteIds)
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

                    foreach (int gemeenteId in gemeenteIds)
                    {
                        command.Parameters["@gemeenteId"].Value = gemeenteId;
                        command.ExecuteNonQuery();
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

        public void KoppelStraatAanGemeente(int gemeenteId, List<int> straatIds)
        {
            SqlConnection connection = getConnection();
            string queryS = "INSERT INTO dbo.gemeente_straatSQL(gemeenteId,straatId) VALUES(@gemeenteId,@straatId)";

            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    command.Parameters.Add(new SqlParameter("@gemeenteId", SqlDbType.Int));
                    command.Parameters.Add(new SqlParameter("@straatId", SqlDbType.Int));

                    command.CommandText = queryS;
                    command.Parameters["@gemeenteId"].Value = gemeenteId;

                    foreach (int straatId in straatIds)
                    {
                        command.Parameters["@straatId"].Value = straatId;
                        command.ExecuteNonQuery();
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

        public void UploadStraat(List<Straat> straten)
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

                        foreach (Straat straat in straten)
                        {
                            //table.Rows.Add(punt.x,punt.y);
                            table.Rows.Add(straat.straatId, straat.straatnaam);
                        }

                        bulkCopy.DestinationTableName = "dbo.straatSQL";
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("straatnaam", "straatnaam");
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

        public void VoegGraafToe(Graaf g)
        {

        }

        public void voegSegmentToe(List<Segment> segmenten)
        {

        }


        public void uploadPunten(List<Punt> punten)
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
                            //table.Rows.Add(punt.x,punt.y);
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

        public void uploadKnopen(List<Knoop> knopen)
        {
            SqlConnection connection = getConnection();
            string query = "SELECT * FROM dbo.puntSQL";
            using (SqlCommand command1 = connection.CreateCommand())
            using (SqlCommand command2 = connection.CreateCommand())
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



                    //SqlParameter paramX = new SqlParameter();
                    //SqlParameter paramY = new SqlParameter();
                    //paramX.ParameterName = "@x";
                    //paramY.ParameterName = "@y";
                    //paramX.DbType = DbType.Decimal;
                    //paramY.DbType = DbType.Decimal;
                    //foreach (Knoop knoop in knopen)
                    //{
                    //    paramX.Value = knoop.punt.x;
                    //    paramY.Value = knoop.punt.y;
                    //    command.Parameters.Add(paramX);
                    //    command.Parameters.Add(paramY);
                    //    command.ExecuteNonQuery();
                    //    command.Parameters.Remove(paramX);
                    //    command.Parameters.Remove(paramY);
                    //}                    
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
