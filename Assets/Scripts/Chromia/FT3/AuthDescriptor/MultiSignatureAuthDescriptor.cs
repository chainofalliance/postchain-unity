using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Chromia.Postchain.Ft3
{
    public class MultiSignatureAuthDescriptor : AuthDescriptor
    {
        public List<byte[]> PubKeys;
        public Flags Flags;
        public int SignatureRequired;
        public readonly IAuthdescriptorRule AuthRule;

        public MultiSignatureAuthDescriptor(List<byte[]> pubkeys, int signatureRequired, FlagsType[] flags, IAuthdescriptorRule rule = null)
        {
            if(signatureRequired > pubkeys.Count)
            {
                throw new Exception("Number of required signatures have to be less or equal to number of pubkeys");
            }

            this.PubKeys = pubkeys;
            this.SignatureRequired = signatureRequired;
            this.Flags = new Flags(flags.ToList());
            this.AuthRule = rule;
        }

        public List<byte[]> Signers
        {
            get => this.PubKeys;
        }

        public List<byte[]> PubKey
        {
            get => this.PubKeys;
        }

        public byte[] ID
        {
            get => this.Hash();
        }

        public IAuthdescriptorRule Rule
        {
            get => this.AuthRule;
        }
        
        public dynamic[] ToGTV()
        {
            var hexPubs = new List<string>();
            foreach (var pubkey in this.PubKeys)
            {
                hexPubs.Add(Util.ByteArrayToString(pubkey));
            }

            var gtv = new List<dynamic>(){
                Util.AuthTypeToString(AuthType.MultiSig),
                hexPubs.ToArray(),
                new List<dynamic>()
                {
                    this.Flags.ToGTV(),
                    this.SignatureRequired,
                    hexPubs.ToArray()
                }.ToArray(),
                this.AuthRule?.ToGTV()
            };
            return gtv.ToArray();
        }

        public byte[] Hash()
        {
            var hexPubs = new List<string>();
            foreach (var pubkey in this.PubKeys)
            {
                hexPubs.Add(Util.ByteArrayToString(pubkey));
            }


            var gtv = new List<dynamic>(){
                Util.AuthTypeToString(AuthType.MultiSig),
                this.PubKeys.ToArray(),
                new List<dynamic>()
                {
                    this.Flags.ToGTV(),
                    this.SignatureRequired,
                    hexPubs.ToArray()
                }.ToArray(),
                this.AuthRule?.ToGTV()
            }.ToArray();

            return PostchainUtil.HashGTV(gtv);
        }
    }
}