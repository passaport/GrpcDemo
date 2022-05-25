using GrpcClient;

namespace GrpcClientDemo
{
    public class GrpcClient
    {
        public async static Task Main()
        {
            ClientRpcConnection clientRpcConnection = new ClientRpcConnection("http://localhost:5125");
            Console.WriteLine("List of options: ");
            Console.WriteLine("0. Unary GRPC");
            Console.WriteLine("1. Client streaming");
            Console.WriteLine("2. Server streaming");
            Console.WriteLine("3. Bidirectional streaming");
            Console.WriteLine("4. Exit");
            Start:
            Console.Write("Choose the option: ");
            if (!int.TryParse(Console.ReadLine(), out int input))
                throw new Exception("Error: No number as input!");
            switch(input)
            {
                case 0:
                    await clientRpcConnection.UnaryGrpc();
                    break;
                case 1:
                    await clientRpcConnection.ClientStreamToServer();
                    break;
                case 2:
                    await clientRpcConnection.ServerStreamToClient();
                    break;
                case 3:
                    await clientRpcConnection.BidirectionalStreaming();
                    break;
                case 4:
                    break;
                default:
                    Console.WriteLine("You didn't select any number from the list.");
                    goto Start;
            }
        }

    }

}

