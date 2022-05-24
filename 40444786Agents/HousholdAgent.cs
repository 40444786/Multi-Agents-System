using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActressMas;

namespace AgentsCW
{
    public class HouseholdAgent : Agent
    {

        private int _valuation;
        private int _kwhGenerated;
        private int _kwhNeeded;
        private int _kwhTotal;
        private int _priceToBuyFromUtility;
        private int _priceToSellToUtility;

        public HouseholdAgent()
        {

        }

        public override void Setup()
        {
            Send("enviroment", "start");
        }
        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                 
                    case "inform":
                        Settings(parameters);
                        break;

                    case "sold":
                        Sold(Convert.ToInt32(parameters));
                        break;

                    case "win":
                        BidWinner(Convert.ToInt32(parameters));
                        break;

                    case "finish":
                        HandleMarketEnd();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // enviroment settings
        private void Settings(string message)
        {
            int[] _messageVals = message.Split(" ").Select(Int32.Parse).ToArray();
            _kwhNeeded = _messageVals[0];
            _kwhGenerated = _messageVals[1];
            _priceToBuyFromUtility = _messageVals[2];
            _priceToSellToUtility = _messageVals[3];
            _kwhTotal = _kwhGenerated;
            int kwh;

            // setting agent market stance and valuation anything under the utility price
            if (_kwhGenerated < _kwhNeeded)
            {
                kwh = _kwhNeeded - _kwhGenerated;
                _valuation = _priceToBuyFromUtility - 1;
                Send("auctioneer", $"buy {kwh} {_valuation}");

            }
            else if (_kwhGenerated > _kwhNeeded)
            {
                kwh = _kwhGenerated - _kwhNeeded;
                _valuation = _priceToSellToUtility + 1;
                Send("auctioneer", $"sell {kwh} {_valuation}");

            }
            else
            {
                Stop();
            }

        }

        // buying methods
        // bought a kwh on the market
        private void BidWinner(int price)
        {
            _kwhTotal = _kwhTotal + 1;

            if (_kwhTotal == _kwhNeeded)
            {
                Console.WriteLine($"[{Name}]: I have correct kwh");
                Stop();
            }


        }
           // selling methods
           // a kwh has been sold on the market
        private void Sold(int price)
        {
            _kwhTotal = +_kwhTotal - 1;

            if (_kwhTotal == _kwhNeeded)
            {
                Console.WriteLine($"[{Name}]: I have correct kwh");
                Stop();
            }

        }
        // does the agent need to buy or sell to the utility at the end of the market
        private void HandleMarketEnd()
        {
            int needed = _kwhNeeded;
            int total = _kwhTotal;

            if (_kwhNeeded > _kwhTotal)
            {
                Console.WriteLine($"[{Name}]: I need to buy " + (needed - total)+" kwh from utility");

            }
            else if (_kwhNeeded < _kwhTotal)
            {
                Console.WriteLine($"[{Name}]: I need to sell " + (total - needed) + " to utility");


            }
        }

    }

}

