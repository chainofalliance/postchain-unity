using Chromia.Postchain.Client;
using System.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class BlockchainSession
    {
        public readonly User User;
        public readonly Blockchain Blockchain;

        public BlockchainSession(User user, Blockchain blockchain)
        {
            this.User = user;
            this.Blockchain = blockchain;
        }

        // public async Task<Account> GetAccountById(byte[] id)
        // {
        //     return await Account.GetById(id, this);
        // }

        // public async Task<Account[]> GetAccountsByParticipantId(byte[] id)
        // {
        //     return await Account.GetByParticipantId(id, this);
        // }

        // public async Task<Account[]> GetAccountsByAuthDescriptorId(byte[] id)
        // {
        //     return await Account.GetByAuthDescriptorId(id, this);
        // }

        // public async Task<(T content, PostchainErrorControl control)> Query<T>(string name, params (string name, object content)[] queryObject)
        // {
        //     return await this.Blockchain.Query<T>(name, queryObject);
        // }

        // public async Task<PostchainErrorControl> Call(Operation operation)
        // {
        //     return await this.Blockchain.Call(operation, this.User);
        // }
    }
}