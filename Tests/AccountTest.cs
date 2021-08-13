using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class AccountTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { }
    private void EmptyCallback() { }

    // Correctly creates keypair
    [UnityTest]
    public void AccountTest1()
    {
        var keyPair = Chromia.Postchain.Client.PostchainUtil.MakeKeyPair();
        var user = new KeyPair(Util.ByteArrayToString(keyPair["privKey"]));

        Assert.AreEqual(user.PrivKey, keyPair["privKey"]);
        Assert.AreEqual(user.PubKey, keyPair["pubKey"]);
    }

    // Register account on blockchain
    [UnityTest]
    public IEnumerator AccountTest2()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();
        Account account = null;
        yield return blockchain.RegisterAccount(user.AuthDescriptor, user, (Account _account) => { account = _account; });

        Assert.NotNull(account);
    }

    // can add new auth descriptor if has account edit rights
    [UnityTest]
    public IEnumerator AccountTest3()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithPoints(1);

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });
        Assert.NotNull(account);

        yield return account.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                    user.KeyPair.PubKey,
                    new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ),
            EmptyCallback
        );

        Assert.AreEqual(2, account.AuthDescriptor.Count);
    }

    // cannot add new auth descriptor if account doesn't have account edit rights
    [UnityTest]
    public IEnumerator AccountTest4()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();
        Account account = null;
        yield return Account.Register(
            new SingleSignatureAuthDescriptor(
                user.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ),
            blockchain.NewSession(user),
            (Account _account) => account = _account
        );
        Assert.NotNull(account);

        yield return account.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                user.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ), EmptyCallback
        );

        // Assert.AreEqual(true, response.Error);
        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should create new multisig account
    [UnityTest]
    public IEnumerator AccountTest5()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        Account account = null;
        yield return Account.Register(
           new MultiSignatureAuthDescriptor(
               new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
               },
               2,
               new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
           ),
           blockchain.NewSession(user1),
           (Account _account) => account = _account
       );

        Assert.NotNull(account);
    }

    // should update account if 2 signatures provided
    [UnityTest]
    public IEnumerator AccountTest6()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        Account account = null;
        yield return Account.Register(
           new MultiSignatureAuthDescriptor(
               new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
               },
               2,
               new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
           ),
           blockchain.NewSession(user1),
           (Account _account) => account = _account
       );

        Assert.NotNull(account);
        AuthDescriptor authDescriptor = new SingleSignatureAuthDescriptor(
                user1.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
        );
        yield return account.AddAuthDescriptor(authDescriptor, EmptyCallback);

        Assert.AreEqual(2, account.AuthDescriptor.Count);
    }

    // should fail if only one signature provided
    [UnityTest]
    public IEnumerator AccountTest7()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        Account account = null;
        yield return Account.Register(
            new MultiSignatureAuthDescriptor(
                new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
                },
                2,
                new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
            ),
            blockchain.NewSession(user1),
            (Account _account) => account = _account
        );

        Assert.NotNull(account);

        yield return account.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                user1.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ), EmptyCallback
        );

        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should be returned when queried by participant id
    [UnityTest]
    public IEnumerator AccountTest8()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();
        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        Account[] accounts = null;
        yield return Account.GetByParticipantId(
            Util.ByteArrayToString(user.KeyPair.PubKey),
            blockchain.NewSession(user),
            (Account[] _accounts) => { accounts = _accounts; }
        );
        Assert.AreEqual(1, accounts.Length);
        Assert.AreEqual(Util.ByteArrayToString(user.KeyPair.PubKey), Util.ByteArrayToString(accounts[0].AuthDescriptor[0].Signers[0]));
    }

    // should return two accounts when account is participant of two accounts
    [UnityTest]
    public IEnumerator AccountTest9()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain, user2);
        accountBuilder2.WithParticipants(new List<KeyPair>() { user2.KeyPair });
        accountBuilder2.WithPoints(1);
        Account account2 = null;
        yield return accountBuilder.Build((Account _account) => { account2 = _account; });

        yield return account2.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(user1.KeyPair.PubKey, new List<FlagsType>() { FlagsType.Transfer }.ToArray()),
            EmptyCallback
        );
        Account[] accounts = null;
        yield return Account.GetByParticipantId(
            Util.ByteArrayToString(user1.KeyPair.PubKey),
            blockchain.NewSession(user1),
            (Account[] _accounts) => { accounts = _accounts; }
        );

        Assert.AreEqual(2, accounts.Length);
    }

    // should return account by id
    [UnityTest]
    public IEnumerator AccountTest10()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        yield return Account.GetById(account.Id, blockchain.NewSession(user),
        (Account _account) => Assert.AreEqual(account.Id, _account.Id));
    }

    // should have only one auth descriptor after calling deleteAllAuthDescriptorsExclude
    [UnityTest]
    public IEnumerator AccountTest11()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();
        User user3 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        accountBuilder.WithPoints(3);

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        AuthDescriptor authDescriptor1 = new SingleSignatureAuthDescriptor(
            user2.KeyPair.PubKey,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        AuthDescriptor authDescriptor2 = new SingleSignatureAuthDescriptor(
            user3.KeyPair.PubKey,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        yield return account.AddAuthDescriptor(authDescriptor1, EmptyCallback);
        yield return account.AddAuthDescriptor(authDescriptor2, EmptyCallback);

        yield return account.DeleteAllAuthDescriptorsExclude(user1.AuthDescriptor, EmptyCallback);
        yield return blockchain.NewSession(user1).GetAccountById(account.Id,
            (Account _account) => Assert.AreEqual(1, _account.AuthDescriptor.Count)
        );
    }

    // should be able to register account by directly calling \'register_account\' operation
    [UnityTest]
    public IEnumerator AccountTest12()
    {
        yield return SetupBlockchain();

        User user = TestUser.SingleSig();

        yield return blockchain.Call(AccountOperations.Op("ft3.dev_register_account",
            new object[] { user.AuthDescriptor })
        , user, EmptyCallback);

        BlockchainSession session = blockchain.NewSession(user);

        Account account = null;
        yield return session.GetAccountById(user.AuthDescriptor.ID, (Account _account) => account = _account);
        Assert.NotNull(account);
    }
}
