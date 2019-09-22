// ***********************************************************************
// Copyright (c) 2019 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

namespace NUnit.Engine.Communication
{
    internal enum AgentServerRequestType : byte
    {
        ConnectAsAgentWorker = 1,

        // Future:

        // The agent server could run in a separate service to which the console could connect. It may or may
        // not be able to start some kinds of agents. Other kinds of agents may need to be started by some other means,
        // then connected to the agent server, and then the console could connect by demands (in TestPackage or other
        // form) or by a worker name. Worker name is easiest because the orchestrating CI script or person will know
        // which agents they started.

        // AcquireAgentWorker = 2,
        // ReleaseAgentWorker = 3,

        // Followed by an agent worker ID and a command to be forwarded. The console and agent worker must speak a
        // compatible protocol version since they are both talking to the same agent server. Given that, the command
        // could be treated as an opaque blob and just be blitted into the agent worker's stream. This is less
        // performance overhead than having the agent server parse the commands. This also means that the console and
        // worker agent would be able to use a protocol version inside the command blobs that is newer than the agent
        // server's own version.

        // SendAgentWorkerCommand = 4,
    }
}
