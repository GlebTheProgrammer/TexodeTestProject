using Server.DataAccess;
using Server.DTOs;
using Server.Interfaces;
using Server.Models;

namespace Server.Data
{
    public class InformationCardRepo : IInformationCardRepo
    {
        private readonly IFileContext context;
        public InformationCardRepo(IFileContext context)
        {
            this.context = context;
        }
        public void CreateInformationCard(InformationCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            var cards = context.Deserialize().ToList();
            cards.Add(card);
            context.Serialize(cards);

            return;
        }

        public void DeleteInformationCard(int id)
        {

            var informationCards = context.Deserialize().ToList();
            var card = informationCards.FirstOrDefault(x => x.Id == id);
            var a  = informationCards.Remove(card);

            context.Serialize(informationCards);


            return;
        }

        public IEnumerable<InformationCard> GetAllInformationCards()
        {
            return context.Deserialize().ToList();
        }

        public InformationCard GetInformationCardById(int id)
        {
            return context.Deserialize().ToList().FirstOrDefault(card => card.Id == id);
        }

        public void UpdateInformationCard(int id, InformationCardUpdateDto card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            var informationCards = context.Deserialize().ToList();

            informationCards.FirstOrDefault(item => item.Id == id).Name = card.Name;
            informationCards.FirstOrDefault(item => item.Id == id).Image = card.Image;

            context.Serialize(informationCards);

            return;
        }
    }
}
