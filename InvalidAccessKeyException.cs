using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlanning.Client
{
    public class InvalidAccessKeyException : Exception
    {
        public InvalidAccessKeyException() : base("This access key is invalid")
        {
        }
    }
}
