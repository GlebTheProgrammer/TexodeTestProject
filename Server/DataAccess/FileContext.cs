using Newtonsoft.Json;
using Server.Interfaces;
using Server.Models;

namespace Server.DataAccess
{
    public class FileContext : IFileContext
    {
        public IEnumerable<InformationCard> Deserialize()
        {
            string fileName = "../Server/Files/DataFile.json";

            if(File.Exists(fileName))
            {
                var cards = JsonConvert.DeserializeObject<List<InformationCard>>(File.ReadAllText(fileName));

                return cards == null ? new List<InformationCard>() : cards;
            }
            return null;

        }

        public string DeserializeIntoStr()
        {
            string fileName = "../Server/Files/DataFile.json";

            if (File.Exists(fileName))
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();

                    return json == null ? string.Empty : json;
                }
            }
            return null;
        }

        public bool Serialize(IEnumerable<InformationCard> cards)
        {
            try
            {
                using (StreamWriter file = File.CreateText("../Server/Files/DataFile.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, cards);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
