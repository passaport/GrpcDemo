using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;

namespace GrpcClient
{
    public class ClientRpcConnection
    {
        Chat.ChatClient clientChannel;
        public ClientRpcConnection(string ip)
        {
            var channel = GrpcChannel.ForAddress(ip);
            clientChannel = new Chat.ChatClient(channel);
        }

        #region Unary gRPC
        public async Task UnaryGrpc()
        {
            Console.Write("Name: ");
            var inputUserName = Console.ReadLine();
            Console.Write("Message: ");
            var inputMessage = Console.ReadLine();
            var inputRequest = new SendMessagesRequest()
            {
                User = inputUserName,
                Text = inputMessage,
            };
            var requestResponse = await clientChannel.SendMessagesAsync(inputRequest);

            Console.WriteLine(requestResponse.Text);
        }
        #endregion

        #region Client-Side Streaming
        public async Task ClientStreamToServer()
        {

            using (var clientStream = clientChannel.ClientSendMessages())
            {
                while (true)
                {
                    Console.Write("Enter text: ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input) || input.ToLower() == "quit")
                        break;
                    await clientStream.RequestStream.WriteAsync(new ClientStreamToServerRequest() { Text = input });
                }
                Console.WriteLine("The stream will be closed.");
                await clientStream.RequestStream.CompleteAsync();
            }
            await ServerStreamToClient();
        }
        #endregion

        #region Server-Side Streaming
        public async Task ServerStreamToClient()
        {
            using (var serverStream = clientChannel.ServerSendMessages(new ServerStreamToClientRequest()))
            {
                while (await serverStream.ResponseStream.MoveNext())
                {
                    var mesajCurent = serverStream.ResponseStream.Current;
                    Console.WriteLine("Message from server: " + mesajCurent.Text);
                }
            }
        }
        #endregion

        #region Bidirectional Streaming
        public async Task BidirectionalStreaming()
        {
            using var bidirectionalStream = clientChannel.BidirectionalMessages();
            var response = Task.Run(async () =>
            {
                await foreach(var rm in bidirectionalStream.ResponseStream.ReadAllAsync())
                    Console.WriteLine(rm.Text);
            }
            );
            while(true)
            {
                Console.Write("Enter text: ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || input.ToLower() == "quit")
                    break;
                await bidirectionalStream.RequestStream.WriteAsync(new BidirectionalRequest() { Text = input });
            }
            Console.WriteLine("Disconectiing....");
            await bidirectionalStream.RequestStream.CompleteAsync();
            await response;
        }
        #endregion
    }
}
