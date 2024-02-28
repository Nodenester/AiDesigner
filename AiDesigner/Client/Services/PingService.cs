namespace AiDesigner.Client.Services
{
    public class PingService
    {
        private readonly HttpClient _http;
        private Timer _timer;
        private bool _isStarted = false; // Flag to check if the service has already started

        public PingService(HttpClient http)
        {
            _http = http;
        }

        public void Start()
        {
            if (_isStarted)
            {
                Console.WriteLine("PingService is already started.");
                return;
            }

            // Set the timer to ping every 10 minutes
            _timer = new Timer(async _ => await PingServer(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            _isStarted = true; // Set the flag to true after starting
            Console.WriteLine("PingService started successfully.");
        }

        private async Task PingServer()
        {
            try
            {
                // Assuming "/api/ping" is the endpoint in your controller
                var response = await _http.GetAsync("Tutorial/Ping");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Server pinged successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pinging server: {ex.Message}");
            }
        }
    }
}