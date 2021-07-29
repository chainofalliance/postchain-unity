using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Ft3
{
    public static class AccountOperations
    {
        public static Operation AddAuthDescriptor(string accountId, string authDescriptorId, AuthDescriptor authDescriptor)
        {
            var gtv = new List<object>() {
                accountId,
                authDescriptorId,
                authDescriptor
            };

            return new Operation("ft3.add_auth_descriptor", gtv.ToArray());
        }

        public static Operation Transfer(object[] inputs, object[] outputs)
        {
            var gtv = new List<object>() {
                inputs,
                outputs
            };

            return new Operation("ft3.transfer", gtv.ToArray());
        }

        public static Operation XcTransfer(object[] source, object[] target, byte[][] hops)
        {
            var gtv = new List<object>() {
                source,
                target,
                hops
            };

            return new Operation("ft3.xc.init_xfer", gtv.ToArray());
        }

        public static Operation DeleteAllAuthDescriptorsExclude(string accountId, string excludeAuthDescriptorId)
        {
            var gtv = new List<object>() {
                accountId,
                excludeAuthDescriptorId
            };

            return new Operation("ft3.delete_all_auth_descriptors_exclude", gtv.ToArray());
        }

        public static Operation DeleteAuthDescriptor(string accountId, string authDescriptorId, string deleteAuthDescriptorId)
        {
            var gtv = new List<object>() {
                accountId,
                authDescriptorId,
                deleteAuthDescriptorId
            };

            return new Operation("ft3.delete_auth_descriptor", gtv.ToArray());
        }

        public static Operation Nop()
        {
            var gtv = new List<object>() {
                new Random().Next().ToString()
            };
            return new Operation("nop", gtv.ToArray());
        }

        public static Operation Op(string name, object[] args)
        {
            return new Operation(name, args);
        }
    }
}