using System.Collections.Generic;

namespace Chromia.Postchain.Ft3
{
    public static class AccountDevOperations
    {
        public static Operation Register(AuthDescriptor authDescriptor)
        {
            var gtv = new List<object>() {
                authDescriptor.ToGTV()
            };

            return new Operation("ft3.dev_register_account", gtv.ToArray());
        }

        public static Operation FreeOp(string accountID)
        {
            var gtv = new List<object>() {
                accountID
            };

            return new Operation("ft3.dev_free_op", gtv.ToArray());
        }

        public static Operation GivePoints(string accountID, int points)
        {
            var gtv = new List<object>() {
                accountID,
                points
            };

            return new Operation("ft3.dev_give_points", gtv.ToArray());
        }
    }
}