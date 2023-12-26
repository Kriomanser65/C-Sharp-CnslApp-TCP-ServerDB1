using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data.SqlClient;

namespace TCPserverDBcars
{
    internal class Program
    {
        private TcpListener listener;
        string connectionString = "Data Source=DESKTOP-4SK39GD;Initial Catalog=Olympiad2;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("192.168.0.100", 8081);
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.ASCII.GetBytes("REQUEST_DATA");
            stream.Write(data, 0, data.Length);
            byte[] responseBuffer = new byte[1024];
            int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
            string response = Encoding.ASCII.GetString(responseBuffer, 0, bytesRead);
            Console.WriteLine("Server response: " + response);
            client.Close();
            Program server = new Program();
        }

        public Program()
        {
            IPAddress ipAddress = IPAddress.Parse("192.168.0.100");
            listener = new TcpListener(ipAddress, 8081);
            listener.Start();
            Console.WriteLine("Server is running...");
            Console.WriteLine("Waiting connections...");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected");
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                string response = ProcessRequest(dataReceived);
                byte[] dataToSend = Encoding.ASCII.GetBytes(response);
                stream.Write(dataToSend, 0, dataToSend.Length);
            }
            client.Close();
        }

        private string ProcessRequest(string request)
        {
            string response = "Data Source=DESKTOP-4SK39GD;Initial Catalog=Olympiad2;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string[] requestData = request.Split(':');
                    if (requestData.Length == 2 && requestData[0] == "SEARCH_BY_ID")
                    {
                        int id;
                        if (int.TryParse(requestData[1], out id))
                        {
                            string sqlQuery = "SELECT * FROM Car WHERE Id = @CarId";
                            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                            {
                                command.Parameters.AddWithValue("@CarId", id);
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            response += $"Car ID: {reader["Id"]}, Price: {reader["CarPrice"]}, Model: {reader["CarModel"]}, Sell: {reader["CarNONSell"]}, Add: {reader["CarAdd"]}, AddSell: {reader["CarAddSell"]}, InfoEdit: {reader["CarInfoEdit"]}";
                                        }
                                    }
                                    else
                                    {
                                        response = "Car with specified ID not found";
                                    }
                                }
                            }
                        }
                        else
                        {
                            response = "Invalid ID format";
                        }
                    }
                    else
                    {
                        response = "Invalid request format for search by ID";
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Error: " + ex.Message;
            }
            return response;
        }
    }
}
//create table Car
//(
//    Id int primary key identity(1,1),
//    CarPrice int not null,
//    CarModel nvarchar(100) not null,
//    CarNONSell int not null,
//    CarAdd nvarchar(100) not null,
//    CarAddSell int not null,
//    CarInfoEdit nvarchar(100) not null
//);
