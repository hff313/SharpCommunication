﻿using SharpCommunication.Base.Channels.ChannelTools;
using SharpCommunication.Base.Codec.Packets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SharpCommunication.Base.Channels.Decorator
{
    public class CachedChannel<TPacket> : ChannelDecorator<TPacket> where TPacket : IPacket
    {
        private IOCache<TPacket> ioCache { get; set; }
        public CachedChannel(Channel<TPacket> innerChannel) : base(innerChannel)
        {
            ioCache = new IOCache<TPacket>(innerChannel);
        }

        public CachingManager<TPacket> CachingManager => ioCache.CachingManager;
        public ObservableCollection<PacketCacheInfo<TPacket>> packet { get { return ioCache.PacketCacheInfoCollection; } }

    }
}
