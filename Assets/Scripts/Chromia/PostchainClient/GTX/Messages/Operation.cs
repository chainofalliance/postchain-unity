using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Client
{
    internal class Operation
    {
        public string OpName;
        public List<GTXValue> Args;
        private object[] _rawArgs;

        public Operation(string opName, object[] args): this()
        {
            this.OpName = opName;
            this._rawArgs = args;
            
            foreach (var opArg in args)
            {
                Args.Add(Gtx.ArgToGTXValue(opArg));
            }
        }

        private Operation()
        {
            this.OpName = "";
            this.Args = new List<GTXValue>();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                Operation gtxOperation = (Operation) obj;
                
                return this.OpName.Equals(gtxOperation.OpName)
                    && ((this.Args == null || gtxOperation.Args == null) ? this.Args == gtxOperation.Args : Enumerable.SequenceEqual(this.Args, gtxOperation.Args));
            }   
        }

        public override int GetHashCode()
        {
            return OpName.GetHashCode();
        }

        public GTXValue ToGtxValue()
        {
            var gtxValue = new GTXValue();
            gtxValue.Choice = GTXValueChoice.Array;
            gtxValue.Array = new List<GTXValue>(){Gtx.ArgToGTXValue(this.OpName)};
            gtxValue.Array.AddRange(this.Args);

            return gtxValue;
        }

        public object[] Raw()
        {
            return new object[]{this.OpName, this._rawArgs};
        }

        public byte[] Encode()
        {
            var messageWriter = new ASN1.AsnWriter();
            messageWriter.PushSequence();

            messageWriter.WriteUTF8String(this.OpName);

            messageWriter.PushSequence();
            if (this.Args.Count > 0)
            {
                foreach(var arg in this.Args)
                {
                    messageWriter.WriteEncodedValue(arg.Encode());
                }
            }
            messageWriter.PopSequence();

            messageWriter.PopSequence();
            return messageWriter.Encode();
        }

        public static Operation Decode(ASN1.AsnReader outerSequence)
        {
            var op = new Operation();
            var operationSequence = outerSequence.ReadSequence();

            op.OpName = operationSequence.ReadUTF8String();

            var valueSequence = operationSequence.ReadSequence();
            while (valueSequence.RemainingBytes > 0)
            {
                op.Args.Add(null);
            }

            return op;
        }
    }
}