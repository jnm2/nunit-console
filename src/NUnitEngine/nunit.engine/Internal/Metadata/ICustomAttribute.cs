namespace NUnit.Engine.Internal.Metadata
{
    internal interface ICustomAttribute
    {
        string TypeFullName { get; }
        object[] GetConstructorArguments();
    }
}