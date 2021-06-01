using System.Collections.Generic;
using System.Linq;
using System.Net;
using ABKTest.Options;
using Microsoft.Extensions.Options;

namespace ABKTest.Services
{
    public class EndpointBalancerService : IEndpointBalancerService
    {
        private readonly object _lock = new();

        private readonly LinkedList<IPEndPoint> _linkedList;
        private LinkedListNode<IPEndPoint> _current;

        public EndpointBalancerService(IOptions<ApplicationOptions> optionsProvider)
        {
            var options = optionsProvider.Value;

            _linkedList = new LinkedList<IPEndPoint>(options.Servers.Select(IPEndPoint.Parse));
        }

        public IPEndPoint GetNext()
        {
            lock (_lock)
            {
                _current = _current?.Next ?? _linkedList.First;

                return _current?.Value;
            }
        }
    }
}
