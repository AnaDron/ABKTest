using System.Net;

namespace ABKTest.Services
{
    public interface IEndpointBalancerService
    {
        IPEndPoint GetNext();
    }
}
