using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ABKTest.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ABKTest.Services
{
    public class Application : IApplication, IDisposable
    {
        private readonly IEndpointBalancerService _endpointBalancerService;
        private readonly ApplicationOptions _options;
        private readonly ILogger _logger;

        private readonly TcpListener _listener;
        private bool _isRunning;

        public Application
        (
            IEndpointBalancerService endpointBalancerService,
            IOptions<ApplicationOptions> optionsProvider,
            ILogger<Application> logger
        )
        {
            _endpointBalancerService = endpointBalancerService;
            _logger = logger;

            _options = optionsProvider.Value;

            _listener = new TcpListener(IPAddress.Any, _options.Port);
        }

        public void Dispose()
        {
            _isRunning = false;
            _listener.Stop();
        }

        public async Task Run()
        {
            _logger.LogInformation("Start listening on port {port} with servers {servers}", _options.Port, string.Join(", ", _options.Servers));

            _listener.Start();
            _isRunning = true;

            while (_isRunning)
            {
                try
                {
                    var clientClient = await _listener.AcceptTcpClientAsync();
                    clientClient.NoDelay = true;

#pragma warning disable 4014
                    Process(clientClient);
#pragma warning restore 4014
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error during accept tcp client");
                }
            }
        }

        private async Task Process(TcpClient clientClient)
        {
            var clientEndpoint = clientClient.Client.RemoteEndPoint;
            var serverEndPoint = _endpointBalancerService.GetNext();

            _logger.LogInformation("Established {clientEndpoint} => {serverEndPoint}", clientEndpoint, serverEndPoint);

            try
            {
                using (clientClient)
                {
                    using var serverClient = new TcpClient { NoDelay = true };

                    await serverClient.ConnectAsync(serverEndPoint.Address, serverEndPoint.Port);

                    var serverStream = serverClient.GetStream();
                    var clientStream = clientClient.GetStream();

                    await Task.WhenAny(clientStream.CopyToAsync(serverStream), serverStream.CopyToAsync(clientStream));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during process");
            }

            _logger.LogInformation("Closed {clientEndpoint} => {serverEndPoint}", clientEndpoint, serverEndPoint);
        }
    }
}
