using Server.DTOs;

namespace Server.SyncDataServices.Http
{
    public interface IInformationCommandsDataClient
    {
        Task SendInformationCommandsToUser(List<InformationCardReadDto> cards);
    }
}
