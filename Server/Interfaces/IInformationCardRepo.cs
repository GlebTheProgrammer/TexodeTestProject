using Microsoft.Extensions.Logging;
using Server.DTOs;
using Server.Models;

namespace Server.Interfaces
{
    public interface IInformationCardRepo
    {
        public IEnumerable<InformationCard> GetAllInformationCards();
        public InformationCard GetInformationCardById(int id);
        public void CreateInformationCard(InformationCard card);
        public void UpdateInformationCard(int id, InformationCardUpdateDto card);
        public void DeleteInformationCard(int id);

    }
}
