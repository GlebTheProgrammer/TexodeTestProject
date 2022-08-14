using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Interfaces;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InformationCardsController : ControllerBase
    {
        private readonly IInformationCardRepo repository;
        private readonly IMapper mapper;
        private int IdProvider;
        public InformationCardsController(IInformationCardRepo repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
            IdProvider = repository.GetAllInformationCards().LastOrDefault() == null ? 0 : repository.GetAllInformationCards().LastOrDefault().Id + 1;
        }

        //GET api/InformationCards
        [HttpGet]
        public ActionResult<IEnumerable<InformationCardReadDto>> GetInformationCards()
        {
            var cards = repository.GetAllInformationCards();

            return Ok(mapper.Map<IEnumerable<InformationCardReadDto>>(cards));
        }

        //GET api/InformationCards/{id}
        [HttpGet("{id:int}", Name = "GetInformationCardById")]
        public ActionResult<InformationCardReadDto> GetInformationCardById(int id)
        {
            var card = repository.GetInformationCardById(id);

            if (card != null)
                return Ok(mapper.Map<InformationCardReadDto>(card));

            return NotFound();
        }

        //POST api/InformationCards
        [HttpPost]
        public ActionResult<InformationCardReadDto> AddNewInformationCard(InformationCardCreateDto cardCreateDto)
        {
            var card = mapper.Map<InformationCard>(cardCreateDto);
            card.Id = IdProvider;

            IdProvider++;

            repository.CreateInformationCard(card);

            var cardReadDto = mapper.Map<InformationCardReadDto>(card);

            return CreatedAtRoute(nameof(GetInformationCardById), new { Id = card.Id }, cardReadDto);
        }

        //PUT api/InformationCards/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateInformationCard(int id, InformationCardUpdateDto cardUpdateDto)
        {
            var informationCardModelFromRepo = repository.GetInformationCardById(id);

            if (informationCardModelFromRepo == null)
                return NotFound();


            repository.UpdateInformationCard(id, cardUpdateDto);

            return NoContent();
        }

        //DELETE api/InformationCards/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteInformationCard(int id)
        {
            var card = repository.GetInformationCardById(id);

            if (card == null)
                return NotFound();

            repository.DeleteInformationCard(id);

            return NoContent();
        }

        #region Comunication Between Server and Client

        [HttpGet]
        [Route("AsAString")]
        public string GetInformationCardsAsStr()
        {
            return repository.GetAllInformationCardsAsStr();
        }


        #endregion
    }
}
