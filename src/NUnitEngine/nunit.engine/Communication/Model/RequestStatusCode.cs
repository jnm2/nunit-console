namespace NUnit.Engine.Communication.Model
{
    internal enum RequestStatusCode : byte
    {
        Success = 0,
        UnsupportedProtocolVersion,
        ProtocolError
    }
}
