﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SharpCommunication.Channels;
using SharpCommunication.Codec;
using SharpCommunication.Codec.Packets;

namespace SharpCommunication.Transport
{
    public abstract class DataTransport<TPacket> : IDataTransport<TPacket> where TPacket : IPacket
    {
        private bool _isOpen;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Task<Task> _checkingIsOpenedTask;
        protected ObservableCollection<IChannel<TPacket>> InnerChannels;
        protected ILogger Log { get; }
        protected readonly IChannelFactory<TPacket> ChannelFactory;
        public readonly IOptions<DataTransportOption> Option;
        public event EventHandler IsOpenChanged;
        public event EventHandler CanOpenChanged;
        public event EventHandler CanCloseChanged;

        protected DataTransport(IChannelFactory<TPacket> channelFactory, IOptions<DataTransportOption> option) : this(channelFactory, option, NullLoggerProvider.Instance.CreateLogger("DataTransport"))
        {

        }

        protected DataTransport(IChannelFactory<TPacket> channelFactory, IOptions<DataTransportOption> option,
            ILogger log)
        {
            Log = log;
            Option = option;
            ChannelFactory = channelFactory;
            InnerChannels = new ObservableCollection<IChannel<TPacket>>();
            Channels = new ReadOnlyObservableCollection<IChannel<TPacket>>(InnerChannels);

            if (option.Value.AutoCheckIsOpen)
            {
                _tokenSource = new CancellationTokenSource();

                _checkingIsOpenedTask = Task.Factory.StartNew(async () =>
                {
                    var token = _tokenSource.Token;
                    while (true)
                    {
                        try
                        {
                            if (IsOpenCore != IsOpen)
                            {
                                IsOpen = IsOpenCore;
                            }

                            await Task.Delay(Option.Value.AutoCheckIsOpenTime, token);
                            if (token.IsCancellationRequested)
                                break;
                        }
                        catch (OperationCanceledException e)
                        {
                            break;
                        }
                        catch (Exception)
                        {
                            await Task.Delay(1000);
                        }
                    }
                });
            }
        }

        public void Open()
        {
            if (IsOpen)
                throw new InvalidOperationException("Device Is Open");
            Log.LogInformation("Try Open Transport Layer...");
            OpenCore();
            if (IsOpenCore != IsOpen)
                IsOpen = IsOpenCore;
        }

        public void Close()
        {
            if (IsOpen == false)
                throw new InvalidOperationException();
            Log.LogInformation("Try Close Transport Layer...");
            CloseCore();
            if (IsOpenCore != IsOpen)
                IsOpen = IsOpenCore;
        }


        public bool CanOpen => !IsOpen && CanOpenCore;

        public bool CanClose => IsOpen && CanCloseCore;

        public bool IsOpen
        {
            get => _isOpen;
            protected set
            {
                if (value == _isOpen)
                    return;
                _isOpen = value;
                OnIsOpenChanged();
            }
        }


        public ReadOnlyObservableCollection<IChannel<TPacket>> Channels { get; }

        public ICodec<TPacket> Codec => ChannelFactory.Codec;


        protected abstract bool IsOpenCore { get; }

        protected abstract void OpenCore();

        protected abstract void CloseCore();

        protected virtual bool CanOpenCore => true;

        protected virtual bool CanCloseCore => true;


        public virtual void Dispose()
        {
            if (IsOpen)
                Close();
            if (Option.Value.AutoCheckIsOpen)
            {
                _tokenSource.Cancel();
                Task.WaitAll(new Task[] {_checkingIsOpenedTask}, 1000);
                _tokenSource?.Dispose();
            }
        }

        protected virtual void OnIsOpenChanged()
        {
            IsOpenChanged?.Invoke(this, null);
            if (!IsOpen)
            {
                foreach (var ch in InnerChannels)
                {
                    ch.Dispose();
                }
                InnerChannels.Clear();
                Log.LogInformation("Transport Layer is closed.");
            }else
                Log.LogInformation("Transport Layer is Open.");

            OnCanOpenChanged();
            OnCanCloseChanged();

        }


        protected virtual void OnCanOpenChanged()
        {
            CanOpenChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCanCloseChanged()
        {
            CanCloseChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
