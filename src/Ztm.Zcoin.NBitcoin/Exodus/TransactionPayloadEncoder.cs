using System;
using System.IO;
using System.Net;
using NBitcoin;

namespace Ztm.Zcoin.NBitcoin.Exodus
{
    public abstract class TransactionPayloadEncoder : ITransactionPayloadEncoder
    {
        protected TransactionPayloadEncoder()
        {
        }

        public abstract int Type { get; }

        protected abstract ExodusTransaction Decode(
            BitcoinAddress sender,
            BitcoinAddress receiver,
            BinaryReader payload,
            int version);

        protected abstract void Encode(BinaryWriter writer, ExodusTransaction transaction);

        /// <summary>
        /// Read a <see cref="PropertyAmount"/> from <paramref name="reader"/>.
        /// </summary>
        /// <exception cref="EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        protected static PropertyAmount DecodePropertyAmount(BinaryReader reader)
        {
            var value = IPAddress.NetworkToHostOrder(reader.ReadInt64());

            return new PropertyAmount(value);
        }

        /// <summary>
        /// Read a <see cref="PropertyId"/> from <paramref name="reader"/>.
        /// </summary>
        /// <exception cref="EndOfStreamException">
        /// The end of the stream is reached.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The value is not a valid property identifier.
        /// </exception>
        protected static PropertyId DecodePropertyId(BinaryReader reader)
        {
            long value = (uint)IPAddress.NetworkToHostOrder((int)reader.ReadUInt32());

            return new PropertyId(value);
        }

        /// <summary>
        /// Write a <see cref="PropertyAmount"/> to <paramref name="writer"/>.
        /// </summary>
        protected static void EncodePropertyAmount(BinaryWriter writer, PropertyAmount amount)
        {
            var value = IPAddress.HostToNetworkOrder(amount.Indivisible);

            writer.Write(value);
        }

        /// <summary>
        /// Write a <see cref="PropertyId"/> to <paramref name="writer"/>.
        /// </summary>
        protected static void EncodePropertyId(BinaryWriter writer, PropertyId id)
        {
            var value = IPAddress.HostToNetworkOrder((int)id.Value);

            writer.Write(value);
        }

        ExodusTransaction ITransactionPayloadEncoder.Decode(
            BitcoinAddress sender,
            BitcoinAddress receiver,
            BinaryReader payload,
            int version)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            // Don't check if version is valid or not due to it is better to let implementation handle it.
            return Decode(sender, receiver, payload, version);
        }

        void ITransactionPayloadEncoder.Encode(BinaryWriter writer, ExodusTransaction transaction)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            Encode(writer, transaction);
        }
    }
}
