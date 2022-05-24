using ActressMas;
using System;

namespace AgentsCW
{ 
    public class MultiAgentSystem
    {
        private static void Main(string[] args)
        {
            string input;
            int NoAgents;
            do
            {
                Console.WriteLine("how many agents would you like?");
                input = Console.ReadLine();
            } while (!Int32.TryParse(input, out NoAgents));

            //creating enviroment
            var env = new EnvironmentMas();
            // randomiser
            var randomise = new Random();
            //adding enviroment agent
            var enviromentAgent = new EnvironmentAgent();
            env.Add(enviromentAgent, "enviroment");
            //adding auctioneer
            var auctionAgent = new AuctionAgent();
            env.Add(auctionAgent, "auctioneer");
            //creating agents
            for (int i = 1; i <= NoAgents; i++)
            {
                var householdAgent = new HouseholdAgent();
                env.Add(householdAgent, $"agent{i:D2}");
            }

            env.Start();


            Console.WriteLine("press return to continue");
            Console.ReadLine();

        }
    }
}