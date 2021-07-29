

namespace Chromia.Postchain.Ft3
{
    public class Operation
    {
        public readonly string Name;
        public readonly object[] Args;

        public Operation(string name, params object[] args)
        {
            this.Name = name;
            this.Args = args;
        }
    }
}