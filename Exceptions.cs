using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandy.Expression
{
    [Serializable]
    public class InvalidSyntaxException : Exception
    {
        public InvalidSyntaxException(string message) : base(message){}
    }

    [Serializable]
    public class ComputationException : Exception
    {
        public ComputationException(string message) : base(message) {}
    }
}
