using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentsCW
{
    public class AuctionAgent : Agent
    {
        private List<string> _buyers;
        private List<string> _sellers;
        private int _turnsToWait;
        private List<Sale> _sellList;
        private List<Sale> _marketOrder;
        private List<Bid> _bidList;
        private bool _marketStart = false;


        private struct Bid
        {
            public int _bidprice { get; set; }
            public string _name { get; set; }

            public Bid(string bidder, int bidprice)
            {
                _name = bidder;
                _bidprice = bidprice;
            }
        }
        private struct Sale
        {
            public int _sellPrice { get; set; }
            public string _name { get; set; }

            public Sale(string seller, int sellPrice)
            {
                _name = seller;
                _sellPrice = sellPrice;
            }
        }



        public override void Setup()
        {

            _buyers = new List<string>();

            _sellers = new List<string>();

            _sellList = new List<Sale>();

            _bidList = new List<Bid>();

            _turnsToWait = 4;



        }
        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");

                message.Parse(out string action, out string parameters);
                _turnsToWait = 4;
                switch (action)
                {

                    case "buy":
                        HandleBuyer(message.Sender, parameters);
                        break;

                    case "sell":
                        HandleSeller(message.Sender, parameters);
                        break;

                }

            }

            catch (Exception ex)
            {



                Console.WriteLine(ex.ToString());
            }
        }
        public override void ActDefault()
        {
            if (--_turnsToWait <= 0 && _marketStart == false)
            {
                HandleMarket();

            }

        }

        private void HandleBuyer(string sender, string parameters)
        {
            _buyers.Add(sender);
            string[] buyVals = parameters.Split(' ');
            int kwh = Convert.ToInt32(buyVals[0]);
            int price = Convert.ToInt32(buyVals[1]);
            for (int i = 0; i < kwh; i++)
            {
                _bidList.Add(new Bid(sender, price));
            }
        }
        private bool MoreBids(string bidder)
        {
            bool moreBids = _bidList.Any(Bid => Bid._name == bidder);


            if (moreBids == false)
            {
                for (int i = 0; i < _buyers.Count; i++)
                {
                    if (_buyers[i] == bidder)
                    {
                        Console.WriteLine($"removed {bidder}");

                        _buyers.RemoveAt(i);
                    }
                }
            }
            return moreBids;
        }
        private void HandleSeller(string sender, string parameters)
        {
            _sellers.Add(sender);
            string[] sellVals = parameters.Split(" ");
            int kwh = Convert.ToInt32(sellVals[0]);
            int price = Convert.ToInt32(sellVals[1]);
            for (int i = 0; i < kwh; i++)
            {
                _sellList.Add(new Sale(sender, price));

            }
        }

        private bool MoreSales(string seller)
        {
            bool moreSales = _marketOrder.Any(Sale => Sale._name == seller);

            if (moreSales == false)
            {
                for (int i = 0; i < _sellers.Count; i++)
                {
                    if (_sellers[i] == seller)
                    {
                        Console.WriteLine($"removed {seller}");
                        _sellers.RemoveAt(i);

                    }
                }
            }
            return moreSales;
        }
        int market_number = 0;

        private void HandleTest()
        {
            market_number++;
            Console.WriteLine("auction number "+market_number);
            for (int i = 0; i < _sellList.Count; i++)
            {
                Console.WriteLine("seller "+_sellList[i]._name + " " + _sellList[i]._sellPrice);
            }
            for (int i = 0; _bidList.Count > i; i++)
            {
                Console.WriteLine("buyer "+_bidList[i]._name + " " + _bidList[i]._bidprice);
            }
            for (int i = 0; i < _marketOrder.Count; i++)
            {
                Console.WriteLine("market " + i + " " + _marketOrder[i]._name + " " + _marketOrder[i]._sellPrice);
            }
            Console.WriteLine("auction end \n");
        }

        private void HandleMarket()
        {
            //ordering by lowest to highest 
            _marketOrder = _sellList.OrderBy(o => o._sellPrice).ToList();
            _bidList = _bidList.OrderBy(o => o._bidprice).ToList();

            // making sure there are buyers and sellers to start market
            if (_buyers.Count == 0)
            {
                for (int i = 0; i < _sellers.Count; i++)
                {
                    Send(_sellers[i], "finish");

                }
                Stop();
            }
            else if (_sellers.Count == 0 || _marketOrder.Count == 0)
            {
                for (int i = 0; i < _buyers.Count; i++)
                {

                    Send(_buyers[i], "finish");

                }
                Stop();

            }
            else
            {
                //starting the market
                // remove handle test slashes to view a market auction in the command line
               // HandleTest();
                //stating market started 
                _marketStart = true;
                //market variables
                string highestBidder = "";
                string seller = _marketOrder[0]._name;
                int highestBid = int.MinValue;
                int secondBid = int.MinValue;
                int[] bidVals = new int[_bidList.Count];
                List<int> lostBids = new List<int>();

                //retrieving highest bid
                _bidList.Reverse();
                highestBidder = _bidList[0]._name;
                highestBid = _bidList[0]._bidprice;

                //getting second bid by finding the highest bid that wasnt from the winning agent
                if (_buyers.Count == 1)
                {

                }
                else
                {
                   
                    for (int i = 0;i < _bidList.Count; i++)
                    {
                        if (_bidList[i]._name != highestBidder)
                        {
                            lostBids.Add(_bidList[i]._bidprice);
                        }
                    }
                    secondBid = lostBids[0]; 
                    highestBid = secondBid;
                }


                //sending information to seller and auction winner
                Send(highestBidder, $"win {highestBid}");
                    Send(_marketOrder[0]._name, $"sold {highestBid}");
                    //removing sale and bid from auction
                    _sellList.RemoveAt(0);
                    _bidList.RemoveAt(0);

                    //removing buyer from list if no more bids are found
                    MoreBids(highestBidder);
                    //removing seller from list if no more sales are found
                    MoreSales(seller);

                    if (_bidList.Count > 0 && _marketOrder.Count > 0)
                    {
                        HandleMarket();
                    }
                    else if (_marketStart == true)
                    {
                        if (_bidList.Count > 0)
                        {
                            for (int i = 0; i < _buyers.Count; i++)
                            {
                                Send(_buyers[i], "finish");
                            }
                        }
                        else
                        {
                            for (int i = 0; i < _sellers.Count; i++)
                            {
                                Send(_sellers[i], "finish");
                            }
                        }
                        Stop();
                    }

                
            }


        }
    }
}
