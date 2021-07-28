using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Ft3
{
    public static class AccountOperations
    {
        public static Operation AddAuthDescriptor(byte[] accountId, byte[] authDescriptorId, AuthDescriptor authDescriptor)
        {
            var gtv = new List<dynamic>() {
                accountId,
                authDescriptorId,
                authDescriptor
            };

            return new Operation("ft3.add_auth_descriptor", gtv.ToArray());
        }

        public static Operation Transfer(dynamic[] inputs, dynamic[] outputs)
        {
            var gtv = new List<dynamic>() {
                inputs,
                outputs
            };

            return new Operation("ft3.transfer", gtv.ToArray());
        }

        public static Operation XcTransfer(dynamic[] source, dynamic[] target, byte[][] hops)
        {
            var gtv = new List<dynamic>() {
                source,
                target,
                hops
            };

            return new Operation("ft3.xc.init_xfer", gtv.ToArray());
        }

        public static Operation DeleteAllAuthDescriptorsExclude(byte[] accountId, byte[] excludeAuthDescriptorId)
        {
            var gtv = new List<dynamic>() {
                accountId,
                excludeAuthDescriptorId
            };

            return new Operation("ft3.delete_all_auth_descriptors_exclude", gtv.ToArray());
        }

        public static Operation DeleteAuthDescriptor(byte[] accountId, byte[] authDescriptorId, byte[] deleteAuthDescriptorId)
        {
            var gtv = new List<dynamic>() {
                accountId,
                authDescriptorId,
                deleteAuthDescriptorId
            };

            return new Operation("ft3.delete_auth_descriptor", gtv.ToArray());
        }

        public static Operation Nop()
        {
            var gtv = new List<dynamic>() {
                new Random().Next().ToString()
            };
            return new Operation("nop", gtv.ToArray());
        }

        public static Operation Op(string name, dynamic[] args)
        {
            return new Operation(name, args);
        }
    }
}