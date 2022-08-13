using Server.Models;

namespace Server.Interfaces
{
    public interface IFileContext
    {
        public IEnumerable<InformationCard> Deserialize();
        public bool Serialize(IEnumerable<InformationCard> cards);

        public string DeserializeIntoStr();
    }
}
