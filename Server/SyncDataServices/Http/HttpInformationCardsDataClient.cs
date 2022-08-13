using Server.DTOs;
using System.Text;
using System.Text.Json;

namespace Server.SyncDataServices.Http
{
    public class HttpInformationCardsDataClient : IInformationCommandsDataClient
    {
        private readonly HttpClient httpClient;
        public HttpInformationCardsDataClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task SendInformationCommandsToUser(List<InformationCardReadDto> cards)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(cards),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync("", httpContent);

            if(response.IsSuccessStatusCode)
                Console.WriteLine("----> Sync post to the client was OK");
            else
                Console.WriteLine("----> Sync post to the client was FAILED");
        }
    }
}
