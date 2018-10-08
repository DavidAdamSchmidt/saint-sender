using System;
using SaintSender.Interfaces;

namespace SaintSender.Services
{
    public class GreetService : IGreetService
    {
        public string Greet(string name)
        {
            return $"Welcome {name}, my friend!";
        }
    }
}
