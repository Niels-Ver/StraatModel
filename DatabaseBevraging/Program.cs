using System;
using System.Runtime.InteropServices.ComTypes;

namespace DatabaseBevraging
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=DESKTOP-FUU1BI0\SQLEXPRESS;Initial Catalog=wegenNetwerk;Integrated Security=True";

            DataBevraging db = new DataBevraging(connectionString);

            
            //db.GetStraatIdsVoorGemeente("Houthalen-Helchteren");

            //db.GetStraatVoorStraatId(120383);

             //db.GetStraatVoorStraatnaamEnGemeente("Daknam-dorp", "Lokeren");

            db.GetStraatnamenVoorGemeente("Houthalen-Helchteren");




        }
    }
}
