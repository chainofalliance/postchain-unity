using System.Collections.Generic;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
    public class AuthDescriptorFactory
    {
        public AuthDescriptor Create(AuthType type, byte[] args)
        {
            switch(type)
            {
                case AuthType.SingleSig: 
                    return this.CreateSingleSig(args);
            }
            return null;
        }

        private SingleSignatureAuthDescriptor CreateSingleSig(byte[] args)
        {   
            var decodedDescriptor = PostchainUtil.DeserializeGTX(args);
            var flags = new List<FlagsType>();

            foreach (var flag in decodedDescriptor.Array[0].Array)
            {
                flags.Add(Util.StringToFlagType((string) flag.String));
            }
            
            return new SingleSignatureAuthDescriptor(
                Util.HexStringToBuffer((string) decodedDescriptor.Array[1].String),
                flags.ToArray()
            );
        }
    }
}