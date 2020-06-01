﻿using SharpCommunication.Codec;
using SharpCommunication.Codec.Packets;
using System.IO;

namespace SharpCommunication.Channels
{
    public class ChannelFactory<TPacket> : IChannelFactory<TPacket> where TPacket : IPacket
    {
        public ICodec<TPacket> Codec { get; protected set; }

        public ChannelFactory(ICodec<TPacket> codec)
        {
            Codec = codec;
        }

        public virtual IChannel<TPacket> Create(Stream stream)
        {
            return new Channel<TPacket>(Codec, stream);
        }
    }
}
