using System;
using System.IO;

namespace NUnit.Engine.Communication.Model
{
    internal struct RequestStatus
    {
        internal RequestStatusCode Code { get; }

        public string ErrorMessage { get; }

        private RequestStatus(RequestStatusCode code, string errorMessage)
        {
            Code = code;
            ErrorMessage = errorMessage;
        }

        public static RequestStatus Success => default(RequestStatus);

        public static RequestStatus Error(RequestStatusCode code, string message)
        {
            if (code == RequestStatusCode.Success)
                throw new ArgumentException("The specified code does not indicate an error.", nameof(code));

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("An error message must be specified.", nameof(message));

            return new RequestStatus(code, message);
        }

        public static Result<RequestStatus> Read(BinaryReader reader)
        {
            var code = (RequestStatusCode)reader.ReadByte();

            if (code == RequestStatusCode.Success)
                return Result.Success(RequestStatus.Success);

            if (!Enum.IsDefined(typeof(RequestStatusCode), code))
                return Result.Error("Unrecognized status code");

            return Result.Success(RequestStatus.Error(code, reader.ReadString()));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)Code);

            if (Code != RequestStatusCode.Success)
                writer.Write(ErrorMessage);
        }
    }
}
