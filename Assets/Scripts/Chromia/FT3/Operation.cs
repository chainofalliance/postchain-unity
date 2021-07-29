

namespace Chromia.Postchain.Ft3
{
    public class Operation
    {
        public readonly string Name;
        public readonly dynamic[] Args;

        public Operation(string name, dynamic[] args)
        {
            this.Name = name;
            this.Args = args;
        }
    }
}