using Rift;
using Rift.Lang;
using Superpower;
using Superpower.Model;

namespace Rift
{
    public class RiftParser
    {
        public RiftParsedScript Parse(TokenList<RiftToken> tokens)
        {
            return RiftLanguageDescription.Script.Parse(tokens);
        }
    }
}