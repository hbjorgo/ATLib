﻿using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.MessageReader
{
    public interface ICommunicator<TMessage>
    {
        ValueTask<TMessage> ReadSingleMessageAsync(byte delimiter, CancellationToken cancellationToken = default);
        ValueTask Write(string input, CancellationToken cancellationToken = default);
    }
}