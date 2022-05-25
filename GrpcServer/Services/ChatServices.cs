using Grpc.Core;

namespace GrpcServer.Services
{
    public class ChatServices : Chat.ChatBase
    {
        private readonly ILogger<ChatServices> _logger;
        private List<string> _bidirectionalList;
        private string _path;
        public ChatServices(ILogger<ChatServices> logger)
        {
            _logger = logger;
            _path = @"D:\Visual Studio\GrpcDemo\GrpcServer\Messages.txt";
            _bidirectionalList = new List<string>();
        }

        #region Unary GRPC
        public override Task<SendMessagesResponse> SendMessages(SendMessagesRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Server: Recive a message from the {request.User} with text {request.Text}");
            return Task.FromResult(new SendMessagesResponse() { Text = $"Server response: We recive the message: {request.Text}" });
        }
        #endregion

        #region Client-Side Streaming
        public override async Task<ClientStreamToServerResponse> ClientSendMessages(
            IAsyncStreamReader<ClientStreamToServerRequest> requestStream, 
            ServerCallContext context)
        {
            while(await requestStream.MoveNext())
            {
                var message = requestStream.Current;
                File.AppendAllText(_path,message.Text + Environment.NewLine);
                // Verific sa primesc de la client mesajul
                _logger.LogInformation($"Server: Receive the following message from the client: {message.Text}");
            }
            return await Task.FromResult(new ClientStreamToServerResponse());

        }
        #endregion


        #region Server-Side Streaming
        public override async Task ServerSendMessages(
            ServerStreamToClientRequest request,
            IServerStreamWriter<ServerStreamToClientResponse> responseStream, 
            ServerCallContext context)
        {
            var content = File.ReadAllLines(_path);
            if(content.Length==0)
                await responseStream.WriteAsync(new ServerStreamToClientResponse() { Text = "No messages" });
            _logger.LogInformation($"Items in the list: {content.Length}");
            foreach(var message in content)
            { 
                //Verific sa trimit mesaj-ul cum trebuie catre server
                _logger.LogInformation($"Send to the client the message: {message}");
                await responseStream.WriteAsync(new ServerStreamToClientResponse() { Text = message });
                await Task.Delay(1000);
            }
            File.Create(_path);
        }
        #endregion


        #region Bidirectiona Streaming
        public override async Task BidirectionalMessages(IAsyncStreamReader<BidirectionalRequest> requestStream, IServerStreamWriter<BidirectionalResponse> responseStream, ServerCallContext context)
        {
            while(await requestStream.MoveNext())
            {
                var req = requestStream.Current.Text;
                _bidirectionalList.Add(req);
                // Verific sa primesc de la client mesajul
                _logger.LogInformation($"Server: Receive the following message from the client: {req}");
            }
            foreach (var element in _bidirectionalList)
            {

                await responseStream.WriteAsync(new BidirectionalResponse() { Text = element });
                await Task.Delay(1000);

                //Verific sa trimit mesaj-ul cum trebuie catre server
                _logger.LogInformation($"Send to the client the message: {element}");
            }
        }
        #endregion
    }
}
