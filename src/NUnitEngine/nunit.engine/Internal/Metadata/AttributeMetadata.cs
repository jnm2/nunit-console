namespace NUnit.Engine.Internal.Metadata
{
    internal struct AttributeMetadata
    {
        public object[] PositionalArguments { get; }
        public NamedArgument[] NamedArguments { get; }

        public AttributeMetadata(object[] positionalArguments, NamedArgument[] namedArguments)
        {
            PositionalArguments = positionalArguments;
            NamedArguments = namedArguments;
        }

        public object GetNamedArgumentOrDefault(string name)
        {
            for (var i = 0; i < NamedArguments.Length; i++)
            {
                if (NamedArguments[i].Name == name)
                    return NamedArguments[i].Value;
            }

            return null;
        }
    }
}
