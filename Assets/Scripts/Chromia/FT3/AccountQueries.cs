using System.Collections.Generic;

namespace Chromia.Postchain.Ft3
{
    public static class AccountQueries
    {
        public static dynamic[] AccountAuthDescriptors(byte[] accountId)
        {
            var gtv = new List<dynamic>() {
                "ft3.get_account_auth_descriptors",
                ("id", Util.ByteArrayToString(accountId))
            };

            return gtv.ToArray();
        }

        public static dynamic[] AccountById(byte[] id)
        {
            var gtv = new List<dynamic>() {
                "ft3.get_account_by_id",
                ("id", Util.ByteArrayToString(id))
            };

            return gtv.ToArray();
        }

        public static dynamic[] AccountsByParticipantId(byte[] id)
        {
            var gtv = new List<dynamic>() {
                "ft3.get_accounts_by_participant_id",
                ("id", Util.ByteArrayToString(id))
            };

            return gtv.ToArray();
        }

        public static dynamic[] AccountsByAuthDescriptorId(byte[] id)
        {
            var gtv = new List<dynamic>() {
                "ft3.get_accounts_by_auth_descriptor_id",
                ("descriptor_id", Util.ByteArrayToString(id))
            };

            return gtv.ToArray();
        }
    }
}