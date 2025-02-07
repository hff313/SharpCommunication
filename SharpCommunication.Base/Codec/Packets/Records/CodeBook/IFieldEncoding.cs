﻿namespace SharpCommunication.Base.Codec.Packets.Records.CodeBook
{
    public interface IFieldEncoding : IEncoding<Field>
    {
        int Id { get; }

        string Name { get; }

        int Size { get; }

        string Description { get; }

        FieldFormat Format { get; }
    }
}
