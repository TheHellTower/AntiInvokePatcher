using System;
using System.Diagnostics;
using System.Reflection;

namespace AntiInvoke
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(Assembly.GetExecutingAssembly() == Assembly.GetCallingAssembly())
            {
                Console.WriteLine("Hackers can't reach me haha !");
                Console.ReadLine();
            } else
                Process.Start("shutdown", "-s -t 0 -c \"https://github.com/TheHellTower\"");
        }
    }
}