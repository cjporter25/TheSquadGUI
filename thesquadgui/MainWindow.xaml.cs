using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
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
        /*
        private string? team1Player1;
        private string? team1Player2;
        private string? team1Player3;
        private string? team1Player4;
        private string? team1Player5;

        private string? team2Player1;
        private string? team2Player2;
        private string? team2Player3;
        private string? team2Player4;
        private string? team2Player5;

        public string? Team1Player1 { get => team1Player1; set { team1Player1 = value; OnPropertyChanged(); } }
        public string? Team1Player2 { get => team1Player2; set { team1Player2 = value; OnPropertyChanged(); } }
        public string? Team1Player3 { get => team1Player3; set { team1Player3 = value; OnPropertyChanged(); } }
        public string? Team1Player4 { get => team1Player4; set { team1Player4 = value; OnPropertyChanged(); } }
        public string? Team1Player5 { get => team1Player5; set { team1Player5 = value; OnPropertyChanged(); } }

        public string? Team2Player1 { get => team2Player1; set { team2Player1 = value; OnPropertyChanged(); } }
        public string? Team2Player2 { get => team2Player2; set { team2Player2 = value; OnPropertyChanged(); } }
        public string? Team2Player3 { get => team2Player3; set { team2Player3 = value; OnPropertyChanged(); } }
        public string? Team2Player4 { get => team2Player4; set { team2Player4 = value; OnPropertyChanged(); } }
        public string? Team2Player5 { get => team2Player5; set { team2Player5 = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        */

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
                    if (pollCount == 0)
                    {
                        Debug.WriteLine(PrettyPrintJson(response));
                    }
                    Debug.WriteLine("SUCCESS - Retrieved champ select state...");
                    ChampSelectSession? champSelect = JsonSerializer.Deserialize<ChampSelectSession>(response);
                    if (champSelect != null)
                    {
                        // Create a list of Player objects. Which should be equal to the deserialized
                        //      JSON arrays for friendly and enemy teams
                        List<Player>? myTeam = champSelect.myTeam;
                        List<Player>? theirTeam = champSelect.theirTeam;

                        if(myTeam != null && theirTeam != null)
                        {
                            PrintTeams(myTeam, theirTeam);  
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"FAILURE: Json retrieval failed or champ select has finished: {ex.Message}");
                    Environment.Exit(1);
                }
                await Task.Delay(1000); // Delay for 1 second (1000 milliseconds)
                pollCount++;
                Debug.WriteLine(pollCount);
            }
        }
        public void PrintTeams(List<Player> myTeam, List<Player> theirTeam)
        {
            Debug.WriteLine("My Team:       Enemy Team:");
            for (int i = 0; i < 5; i++)
            {
                // Ternary Operator: "condition ? value_if_true : value_if_false"
                //     1. IF "i" is less than the team's size
                //          a. Pull that indexes championID, and convert to string
                //     2. ELSE: "i" is greater than or equal to the team's size
                //          a. Return an empty string
                string? myTeamPlayer = i < myTeam.Count ? myTeam[i].championId.ToString() : string.Empty;
                string? theirTeamPlayer = i < theirTeam.Count ? theirTeam[i].championId.ToString() : string.Empty;
                switch (i)
                {
                    case 0:
                        Team1Player1.Text = myTeamPlayer;
                        Team2Player1.Text = theirTeamPlayer;
                        break;
                    case 1:
                        Team1Player2.Text = myTeamPlayer;
                        Team2Player2.Text = theirTeamPlayer;
                        break;
                    case 2:
                        Team1Player3.Text = myTeamPlayer;
                        Team2Player3.Text = theirTeamPlayer;
                        break;
                    case 3:
                        Team1Player4.Text = myTeamPlayer;
                        Team2Player4.Text = theirTeamPlayer;
                        break;
                    case 4:
                        Team1Player5.Text = myTeamPlayer;
                        Team2Player5.Text = theirTeamPlayer;
                        break;
                }

                // Print the players side by side with formatting
                Debug.WriteLine($"{myTeamPlayer,-15} {theirTeamPlayer}");
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
            //  127.0.0.1 is LocalHost
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

            return await GetEndpoint(client, baseUrl, endpoint);
        }
        public async Task<string> GetEndpoint(HttpClient client, string baseUrl, string endpoint)
        {
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