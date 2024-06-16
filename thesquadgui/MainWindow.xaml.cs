using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics; // To be able to print out console shit in a WPF app
using System.Xaml;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace thesquadgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Debug.WriteLine("Initializing Component...");
            InitializeComponent();
            Debug.WriteLine("Polling...");
            //PollOnce();
            StartPolling();
        }

        private async void PollOnce()
        {
            // Get live client config. Port # and pw used is different
            //      each time the cllient launches
            var (port, password) = await GetLiveClientConfig();
            Debug.WriteLine("Got live client config...");

            // Riot has a standard security certificate to used when making
            //      requests
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string certPath = System.IO.Path.Combine(baseDirectory, "riotgames.pem");
            Debug.WriteLine("Successfully found the certificate path...");
            Debug.WriteLine($"The path chosen: {certPath}");

            try
            {
                // Make a call to the local client's API using the localhost, port #
                //      password, and certificate path
                string response = await GetClientState_ChampSelect(port, password, certPath);
                Debug.WriteLine("Got live client info...");
                Debug.WriteLine(PrettyPrintJson(response));
            } 
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON EXCEPTION|Client not in Champ Select: {ex.Message}");
                Environment.Exit(1);
            }
            

            //var champSelect = JsonSerializer.Deserialize<ChampSelectSession>(response);
        }
        private async void StartPolling()
        {
            // Get live client config. Port # and pw used is different
            //      each time the cllient launches
            var (port, password) = await GetLiveClientConfig();
            Debug.WriteLine("SUCCESS - Retrieved client config...");

            // Riot has a standard security certificate to used when making
            //      requests
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string certPath = System.IO.Path.Combine(baseDirectory, "riotgames.pem");
            Debug.WriteLine("SUCCESS - Retrieved certificate path...");
            Debug.WriteLine($"PATH: {certPath}");

            int pollCount = 0;

            while (true)
            {
                try
                {
                    string response = await GetClientState_ChampSelect(port, password, certPath);
                    Debug.WriteLine("SUCCESS - Retrieved champ select state...");
                    ChampSelectSession? champSelect = JsonSerializer.Deserialize<ChampSelectSession>(response);
                    if (champSelect != null)
                    {
                        // Create a list of Player objects. Which should be equal to the deserialized
                        //      JSON arrays for friendly and enemy teams
                        List<Player>? myTeam = champSelect.myTeam;
                        List<Player>? theirTeam = champSelect.theirTeam;

                        Debug.WriteLine("My Team: ");
                        if (myTeam != null)
                        {
                            foreach (var player in myTeam)
                            {
                                Debug.WriteLine($"{player.championId}");
                            }
                        }
                        Debug.WriteLine("Enemy Team: ");
                        if (theirTeam != null)
                        {
                            foreach (var player in theirTeam)
                            {
                                Debug.WriteLine($"{player.championId}");
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"JSON Exception: {ex.Message}");
                }
                await Task.Delay(1000); // Delay for 1 second (1000 milliseconds)
                pollCount++;
                Debug.WriteLine(pollCount);
            }
        }
        private async Task<(string, string)> GetLiveClientConfig()
        {
            string filePath = @"C:\Riot Games\League of Legends\lockfile";
            string content = string.Empty;

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    content = await reader.ReadToEndAsync();
                }
            }
            catch (IOException ex)
            {
                throw new Exception("Unable to access the lockfile. Client isn't open or is intalled in a non-default location", ex);
            }

            string pattern = @"^(.+?):(\d+):(\d+):(.+?):(.+?)$";
            Match match = Regex.Match(content, pattern);

            if (match.Success)
            {
                string portNum = match.Groups[3].Value;
                string passWord = match.Groups[4].Value;
                Console.WriteLine("Successfully Pulled Config");
                return (portNum, passWord);
            }
            else
            {
                throw new Exception("File format does not match the expected pattern.");
            }
        }
        public async Task<string> GetClientState_ChampSelect(string port, string password, string certPath)
        {
            // Construct the base URL for the LCU API
            string baseUrl = $"https://127.0.0.1:{port}";
            Console.WriteLine("Base URL: " + baseUrl);

            // Construct the Authorization header
            string authStr = $"riot:{password}";
            string authBase64 = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authStr));
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            if (!string.IsNullOrEmpty(certPath))
            {
                var cert = new X509Certificate2(certPath);
                handler.ClientCertificates.Add(cert);
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBase64);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Console.WriteLine("Headers: ");
            Console.WriteLine(client.DefaultRequestHeaders);

            // Endpoint to get current champion select session details
            string endpoint = "/lol-champ-select/v1/session";

            try
            {
                HttpResponseMessage response = await client.GetAsync(baseUrl + endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"Error: Received status code {response.StatusCode}";
                }
            }
            catch (Exception e)
            {
                return $"Request failed: {e.Message}";
            }
        }
        public class Player
        {
            public string? assignedPosition { get; set; }
            public int cellId { get; set; }
            public int championId { get; set; }
            public int championPickIntent { get; set; }
            public string? nameVisibilityType { get; set; }
            public string? obfuscatedPuuid { get; set; }
            public int obfuscatedSummonerId { get; set; }
            public string? puuid { get; set; }
            public int selectedSkinId { get; set; }
            public double spell1Id { get; set; }
            public double spell2Id { get; set; }
            public int summonerId { get; set; }
            public int team { get; set; }
            public int wardSkinId { get; set; }
        }
        public class ChampSelectSession
        {
            public List<Player> myTeam { get; set; } = new List<Player>();
            public List<Player> theirTeam { get; set; } = new List<Player>();
        }
        public static string PrettyPrintJson(string json)
        {
            using (var jsonDoc = JsonDocument.Parse(json))
            {
                return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
}