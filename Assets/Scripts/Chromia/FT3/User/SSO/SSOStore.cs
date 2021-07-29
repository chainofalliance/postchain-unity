namespace Chromia.Postchain.Ft3
{
    public interface SSOStore
    {
        KeyPair TmpKeyPair
        {
            get;
        }
        KeyPair KeyPair
        {
            get;
        }
        byte[] TmpPrivKey
        {
            get; set;
        }
        byte[] PrivKey
        {
            get; set;
        }
        string AccountID
        {
            get; set;
        }
        void ClearTmp();
        void Clear();
    }
}