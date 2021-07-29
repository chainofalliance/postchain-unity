using UnityEngine;

namespace Chromia.Postchain.Ft3
{
    public class SSOStoreLocalStorage : SSOStore
    {
        public KeyPair TmpKeyPair
        {
            get
            {
                string privKeyString = PlayerPrefs.GetString("__ssoTmpPrivKey", null);
                if (privKeyString == null) return null;

                return new KeyPair(privKeyString);
            }
        }

        public KeyPair KeyPair
        {
            get
            {
                string privKeyString = PlayerPrefs.GetString("__ssoPrivKey", null);
                if (privKeyString == null) return null;

                return new KeyPair(privKeyString);
            }
        }

        public byte[] TmpPrivKey
        {
            get
            {
                string privKeyString = PlayerPrefs.GetString("__ssoTmpPrivKey", null);
                if (privKeyString == null) return null;

                return Util.HexStringToBuffer(privKeyString);
            }
            set
            {
                PlayerPrefs.SetString("__ssoTmpPrivKey", Util.ByteArrayToString(value));
            }
        }

        public byte[] PrivKey
        {
            get
            {
                string privKeyString = PlayerPrefs.GetString("__ssoPrivKey", null);
                if (privKeyString == null) return null;

                return Util.HexStringToBuffer(privKeyString);
            }
            set
            {
                PlayerPrefs.SetString("__ssoPrivKey", Util.ByteArrayToString(value));
            }
        }

        public string AccountID
        {
            get
            {
                string accountIdString = PlayerPrefs.GetString("__ssoAccountId", null);
                if (accountIdString == null) return null;

                return accountIdString;
            }
            set
            {
                PlayerPrefs.SetString("__ssoAccountId", value);
            }
        }

        public void ClearTmp()
        {
            PlayerPrefs.DeleteKey("__ssoTmpPrivKey");
        }

        public void Clear()
        {
            ClearTmp();
            PlayerPrefs.DeleteKey("__ssoPrivKey");
            PlayerPrefs.DeleteKey("__ssoAccountId");
        }
    }
}