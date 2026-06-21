using SDSMillimar.Utils;
using System;

namespace SDSMillimar.Models
{
    public class ChannelModel
    {
        public uint ChannelIdx { get; set; }

        public double Value { get; set; }

        public N1700Lib.tDataValueType ValueType { get; set; }

        public bool Referenced { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"ChannelIdx={ChannelIdx}, " +
                   $"Value={Value}, " +
                   $"ValueType={ValueType}, " +
                   $"Referenced={Referenced}, " +
                   $"Timestamp={Timestamp:yyyy-MM-dd HH:mm:ss.fff}";
        }
    }

}
