namespace NUnit.Engine.Internal.Metadata
{
    internal struct NamedArgument
    {
        public NamedArgument(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public object Value { get; }
    }
}