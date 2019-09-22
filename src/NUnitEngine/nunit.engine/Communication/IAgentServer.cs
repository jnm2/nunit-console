using NUnit.Engine.Communication.Model;

namespace NUnit.Engine.Communication
{
    internal interface IAgentServer
    {
        RequestStatus ConnectAsAgentWorker(ConnectAsAgentWorkerRequest request);
    }
}
