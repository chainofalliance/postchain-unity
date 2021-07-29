using UnityEngine;

namespace Chromia.Postchain.Ft3
{
    public class SSOStoreDefault : SSOStore
    {
        public string AccountID
        {
            get => null;
            set { }
        }

        public KeyPair KeyPair
        {
            get => null;
        }

        public byte[] PrivKey
        {
            get => null;
            set { }
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

        public KeyPair TmpKeyPair
        {
            get
            {
                string privKeyString = PlayerPrefs.GetString("__ssoTmpPrivKey", null);
                if (privKeyString == null) return null;

                return new KeyPair(privKeyString);
            }
        }

        public void ClearTmp()
        {
            PlayerPrefs.DeleteKey("__ssoTmpPrivKey");
        }

        public void Clear()
        {
            ClearTmp();
        }
    }
}