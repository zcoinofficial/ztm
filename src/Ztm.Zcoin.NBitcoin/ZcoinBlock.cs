using NBitcoin;

namespace Ztm.Zcoin.NBitcoin
{
    public class ZcoinBlock : Block
    {
        public ZcoinBlock(ZcoinBlockHeader header) : base(header)
        {
        }
    }
}