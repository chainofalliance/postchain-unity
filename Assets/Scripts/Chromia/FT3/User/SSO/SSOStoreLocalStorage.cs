using UnityEngine;

namespace Chromia.Postchain.Ft3
{
    public class SSOStoreLocalStorage : SSOStore
    {
        public KeyPair TmpKeyPair
        {
            get
            {
                string privKeyString = FileIOWrapper.GetString("__ssoTmpPrivKey");
                if (privKeyString == null) return null;

                return new KeyPair(privKeyString);
            }
        }

        public KeyPair KeyPair
        {
            get
            {
                string privKeyString = FileIOWrapper.GetString("__ssoPrivKey");
                if (privKeyString == null) return null;

                return new KeyPair(privKeyString);
            }
        }

        public byte[] TmpPrivKey
        {
            get
            {
                string privKeyString = FileIOWrapper.GetString("__ssoTmpPrivKey");
                if (privKeyString == null) return null;

                return Util.HexStringToBuffer(privKeyString);
            }
            set
            {
                FileIOWrapper.SetString("__ssoTmpPrivKey", Util.ByteArrayToString(value));
            }
        }

        public byte[] PrivKey
        {
            get
            {
                string privKeyString = FileIOWrapper.GetString("__ssoPrivKey");
                if (privKeyString == null) return null;

                return Util.HexStringToBuffer(privKeyString);
            }
            set
            {
                FileIOWrapper.SetString("__ssoPrivKey", Util.ByteArrayToString(value));
            }
        }

        public string AccountID
        {
            get
            {
                string accountIdString = FileIOWrapper.GetString("__ssoAccountId");
                if (accountIdString == null) return null;

                return accountIdString;
            }
            set
            {
                FileIOWrapper.SetString("__ssoAccountId", value);
            }
        }

        public void ClearTmp()
        {
            FileIOWrapper.DeleteKey("__ssoTmpPrivKey");
        }

        public void Clear()
        {
            ClearTmp();
            FileIOWrapper.DeleteKey("__ssoPrivKey");
            FileIOWrapper.DeleteKey("__ssoAccountId");
        }
    }
}