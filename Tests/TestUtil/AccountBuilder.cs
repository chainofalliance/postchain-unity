using Chromia.Postchain.Ft3;

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class AccountBuilder
{
    private Blockchain _blockchain;
    private User _user;
    private int _balance = -1;
    private Asset _asset;
    private List<KeyPair> _participants = new List<KeyPair>() { new KeyPair() };
    private int _requiredSignaturesCount = 1;
    private List<FlagsType> _flags = new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer };
    private int _points = 0;

    public AccountBuilder(Blockchain blockchain, User user)
    {
        User __user = user;
        if (user == null)
        {
            __user = TestUser.SingleSig();
        }

        this._blockchain = blockchain;
        this._participants = new List<KeyPair>() { user.KeyPair };
        this._user = __user;
    }

    #region public
    public static AccountBuilder CreateAccountBuilder(Blockchain blockchain, User user = null)
    {
        return new AccountBuilder(blockchain, user);
    }

    public AccountBuilder WithAuthFlags(IEnumerable<FlagsType> flags)
    {
        this._flags = flags.ToList();
        return this;
    }
    public AccountBuilder WithParticipants(IEnumerable<KeyPair> participants)
    {
        this._participants = participants.ToList();
        return this;
    }

    public AccountBuilder WithBalance(Asset asset, int balance)
    {
        this._asset = asset;
        this._balance = balance;
        return this;
    }

    public AccountBuilder WithPoints(int points)
    {
        this._points = points;
        return this;
    }

    public AccountBuilder WithRequiredSignatures(int count)
    {
        this._requiredSignaturesCount = count;
        return this;
    }

    public IEnumerator Build(Action<Account> onSuccess)
    {
        Account account = null;

        yield return this.RegisterAccount((Account _account) => { account = _account; });

        if (account != null)
        {
            yield return this.AddBalanceIfNeeded(account);
            yield return this.AddPointsIfNeeded(account, (RateLimit _rateLimit) => { account.RateLimit = _rateLimit; });
        }

        onSuccess(account);
    }
    #endregion

    #region private
    private IEnumerator RegisterAccount(Action<Account> onSuccess)
    {
        yield return Account.Register(
            this.GetAuthDescriptor(),
            this._blockchain.NewSession(this._user),
            onSuccess
        );
    }

    private IEnumerator AddBalanceIfNeeded(Account account)
    {
        if (this._asset != null && this._balance != -1)
        {
            yield return AssetBalance.GiveBalance(account.Id, this._asset.Id, this._balance, this._blockchain,
            () => { });
        }
    }

    private IEnumerator AddPointsIfNeeded(Account account, Action<RateLimit> onSuccess)
    {
        if (this._points > 0)
        {
            yield return RateLimit.GivePoints(account.Id, this._points, this._blockchain, () => { });
        }

        yield return RateLimit.GetByAccountRateLimit(account.Id, this._blockchain, onSuccess);
    }

    private AuthDescriptor GetAuthDescriptor()
    {
        if (this._requiredSignaturesCount > this._participants.Count)
        {
            throw new Exception("Number of required signatures has to be less than number of participants");
        }

        if (this._participants.Count > 1)
        {
            var participants = new List<byte[]>();
            foreach (var participant in this._participants)
            {
                participants.Add(participant.PubKey);
            }

            return new MultiSignatureAuthDescriptor(
                participants,
                this._requiredSignaturesCount,
                this._flags.ToArray(),
                this._user.AuthDescriptor.Rule
            );
        }
        else
        {
            return new SingleSignatureAuthDescriptor(
                this._participants[0].PubKey,
                this._flags.ToArray(),
                this._user.AuthDescriptor.Rule
                );
        }
    }
    #endregion
}