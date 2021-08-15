using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class TransferTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { }
    private void EmptyCallback() { }

    // should succeed when balance is higher than amount to transfer
    [UnityTest]
    public IEnumerator TransferTestRun1()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset, 200);
        accountBuilder.WithPoints(1);
        Account account1 = null;
        yield return accountBuilder.Build((Account _account) => account1 = _account);

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        Account account2 = null;
        yield return accountBuilder2.Build((Account _account) => account2 = _account);

        yield return account1.Transfer(account2.Id, asset.Id, 10, EmptyCallback);

        AssetBalance assetBalance1 = null;
        yield return AssetBalance.GetByAccountAndAssetId(account1.Id, asset.Id, blockchain, (AssetBalance balance) => assetBalance1 = balance);
        AssetBalance assetBalance2 = null;
        yield return AssetBalance.GetByAccountAndAssetId(account2.Id, asset.Id, blockchain, (AssetBalance balance) => assetBalance2 = balance);

        Assert.AreEqual(190, assetBalance1.Amount);
        Assert.AreEqual(10, assetBalance2.Amount);
    }

    // should fail when balance is lower than amount to transfer
    [UnityTest]
    public IEnumerator TransferTestRun2()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset, 5);
        Account account1 = null;
        yield return accountBuilder.Build((Account _account) => account1 = _account);

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        Account account2 = null;
        yield return accountBuilder.Build((Account _account) => account2 = _account);

        yield return account1.Transfer(account2.Id, asset.Id, 10, EmptyCallback);
    }

    // should fail if auth descriptor doesn't have transfer rights
    [UnityTest]
    public IEnumerator TransferTestRun3()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithAuthFlags(new List<FlagsType>() { FlagsType.Account });
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset, 200);
        accountBuilder.WithPoints(1);
        Account account1 = null;
        yield return accountBuilder.Build((Account _account) => account1 = _account);

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        Account account2 = null;
        yield return accountBuilder2.Build((Account _account) => account2 = _account);

        bool successfully = false;
        yield return account1.Transfer(account2.Id, asset.Id, 10, () => successfully = true);
        Assert.False(successfully);
    }

    // should succeed if transferring tokens to a multisig account
    [UnityTest]
    public IEnumerator TransferTestRun4()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset, 200);
        accountBuilder.WithPoints(1);
        Account account1 = null;
        yield return accountBuilder.Build((Account _account) => account1 = _account);

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        accountBuilder2.WithParticipants(new List<KeyPair>() { new KeyPair(), new KeyPair() });
        accountBuilder2.WithRequiredSignatures(2);
        Account account2 = null;
        yield return accountBuilder2.Build((Account _account) => account2 = _account);

        yield return account1.Transfer(account2.Id, asset.Id, 10, EmptyCallback);

        AssetBalance assetBalance1 = null;
        yield return AssetBalance.GetByAccountAndAssetId(account1.Id, asset.Id, blockchain, (AssetBalance balance) => assetBalance1 = balance);
        AssetBalance assetBalance2 = null;
        yield return AssetBalance.GetByAccountAndAssetId(account2.Id, asset.Id, blockchain, (AssetBalance balance) => assetBalance2 = balance);

        Assert.AreEqual(190, assetBalance1.Amount);
        Assert.AreEqual(10, assetBalance2.Amount);
    }

    // should succeed burning tokens
    [UnityTest]
    public IEnumerator TransferTestRun5()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset, 200);
        accountBuilder.WithPoints(1);
        Account account = null;
        yield return accountBuilder.Build((Account _account) => account = _account);

        yield return account.BurnTokens(asset.Id, 10, EmptyCallback);
        AssetBalance assetBalance = account.GetAssetById(asset.Id);

        Assert.AreEqual(190, assetBalance.Amount);
    }

    // // should have one payment history entry if one transfer made
    // [UnityTest]
    // public IEnumerator TransferTestRun6()
    // {
    //     yield return SetupBlockchain();
    //     Asset asset = null;
    //     yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);

    //     User user = TestUser.SingleSig();

    //     AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
    //     accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
    //     accountBuilder.WithBalance(asset, 200);
    //     accountBuilder.WithPoints(1);
    //     Account account = null;
    //     yield return accountBuilder.Build((Account _account) => account = _account);

    //     AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
    //     Account account2 = null;
    //     yield return accountBuilder2.Build((Account _account) => account2 = _account);

    //     yield return account.Transfer(account2.Id, asset.Id, 10, EmptyCallback);

    //     PaymentHistoryEntryShort[] paymentHistory = yield return account.GetPaymentHistory();
    //     Assert.AreEqual(1, paymentHistory.Length);
    // }

    // // should have two payment history entries if two transfers made
    // [UnityTest]
    // public IEnumerator TransferTestRun7()
    // {
    //     yield return SetupBlockchain();
    //     Asset asset = null;
    //     yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset);

    //     User user = TestUser.SingleSig();

    //     AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
    //     accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
    //     accountBuilder.WithBalance(asset, 200);
    //     accountBuilder.WithPoints(2);
    //     Account account = null;
    //     yield return accountBuilder.Build((Account _account) => account = _account);

    //     AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
    //     Account account2 = null;
    //     yield return accountBuilder2.Build((Account _account) => account2 = _account);

    //     yield return account.Transfer(account2.Id, asset.Id, 10);
    //     yield return account.Transfer(account2.Id, asset.Id, 11);

    //     PaymentHistoryEntryShort[] paymentHistory = yield return account.GetPaymentHistory();
    //     Assert.AreEqual(2, paymentHistory.Length);
    // }
}