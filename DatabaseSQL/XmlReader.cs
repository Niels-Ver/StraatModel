using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using StraatModel;

namespace DatabaseSQL
{
    public class XmlReader
    {
        Dictionary<int, Provincie> provincies = new Dictionary<int, Provincie>();


        public void ReadFile()
        {
            string path = @"D:\Niels\School\Prog3\XMLSerial.xml";

            FileStream fs = new FileStream(path, FileMode.Open);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(provincies.GetType());

            provincies = (Dictionary<int, Provincie>)ser.ReadObject(reader, true);

            reader.Close();
            fs.Close();

            Console.WriteLine("De data is ingelezen");
        }

        public Dictionary<int, Provincie> getProvincies()
        {
            return provincies;
        }
    }
}
