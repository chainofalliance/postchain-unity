using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Ft3
{
    public static class AccountDevOperations
    {
        public static Operation Register(AuthDescriptor authDescriptor)
        {
            var gtv = new List<dynamic>() {
                authDescriptor
            };

            return new Operation("ft3.dev_register_account", gtv.ToArray());
        }

        // one operation that updates the counter of rate limit of the account but does not cost points
        public static Operation FreeOp(byte[] accountID)
        {
           var gtv = new List<dynamic>() {
                accountID
            };

            return new Operation("ft3.dev_free_op", gtv.ToArray()); 
        }

        public static Operation GivePoints(byte[] accountID, int points)
        {
           var gtv = new List<dynamic>() {
                accountID,
                points
            };

            return new Operation("ft3.dev_give_points", gtv.ToArray()); 
        }
    }
}