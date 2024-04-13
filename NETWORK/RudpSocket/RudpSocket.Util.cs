using System.Text;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public const ushort
            BUFFER_SIZE = 1472,
            DATA_SIZE = BUFFER_SIZE - RudpHeader.PREFIXE_LENGTH;

        public static readonly Encoding UTF8 = Encoding.UTF8;
    }
}