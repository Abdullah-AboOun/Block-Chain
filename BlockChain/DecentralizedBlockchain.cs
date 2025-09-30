using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DecentralizedBlockchainDemo
{
    public class Transaction
    {
        public string Sender { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString() => $"Txn| {Sender} -> {Recipient}: {Amount:F2} | Time: {Timestamp:HH:mm:ss}";
    }

    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string PreviousHash { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public int Nonce { get; set; } = 0;

        public Block(int index, string previousHash, List<Transaction> transactions)
        {
            Index = index;
            Timestamp = DateTime.Now;
            PreviousHash = previousHash;
            Transactions = transactions;
        }

        public string CalculateHash()
        {
            string transactionsJson = JsonSerializer.Serialize(Transactions);
            string rawData = $"{Index}{Timestamp}{PreviousHash}{transactionsJson}{Nonce}";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(rawData);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public void MineBlock(int difficulty)
        {
            string target = new string('0', difficulty);

            do
            {
                Nonce++;
                Hash = CalculateHash();
            }
            while (Hash == null || !Hash.StartsWith(target));

            Console.WriteLine($"Block {Index} Mined! Hash: {Hash.Substring(0, 10)}...");
        }
    }

    public class Blockchain
    {
        public IList<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 4;
        public List<Transaction> CurrentTransactions { get; set; } = new List<Transaction>();
        public double MiningReward { get; } = 50.0;

        public string NodeId { get; private set; }

        public Blockchain(string nodeId)
        {
            NodeId = nodeId;
            Chain = new List<Block>();
            Chain.Add(CreateGenesisBlock());
        }

        private Block CreateGenesisBlock()
        {
            Block genesisBlock = new Block(0, "0", new List<Transaction> { new Transaction { Sender = "System", Recipient = NodeId, Amount = 0 } });
            genesisBlock.MineBlock(Difficulty);
            return genesisBlock;
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public void NewTransaction(string sender, string recipient, double amount)
        {
            var transaction = new Transaction
            {
                Sender = sender,
                Recipient = recipient,
                Amount = amount
            };
            CurrentTransactions.Add(transaction);
            Console.WriteLine($"[Node {NodeId}] New transaction added: {transaction}");
        }

        public Block MinePendingTransactions(string minerAddress)
        {
            NewTransaction("Blockchain System", minerAddress, MiningReward);

            Block newBlock = new Block(Chain.Count, GetLatestBlock().Hash, CurrentTransactions);

            Console.WriteLine($"\n[Node {NodeId}] --- Starting Mining for Block {newBlock.Index} ({CurrentTransactions.Count} transactions) ---");

            newBlock.MineBlock(Difficulty);

            Chain.Add(newBlock);
            CurrentTransactions = new List<Transaction>();

            Console.WriteLine($"[Node {NodeId}] Block {newBlock.Index} successfully added.");
            return newBlock;
        }

        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    Console.WriteLine($"[Node {NodeId}] Validation Failed: Block {currentBlock.Index} has an invalid internal hash.");
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    Console.WriteLine($"[Node {NodeId}] Validation Failed: Block {currentBlock.Index} PreviousHash is corrupted.");
                    return false;
                }

                string target = new string('0', Difficulty);
                if (!currentBlock.Hash.StartsWith(target))
                {
                    Console.WriteLine($"[Node {NodeId}] Validation Failed: Block {currentBlock.Index} did not meet Proof-of-Work requirement.");
                    return false;
                }
            }
            return true;
        }

        public bool ResolveConflicts(List<Blockchain> peerChains)
        {
            Blockchain longestValidChain = this;
            int maxLength = Chain.Count;
            bool chainReplaced = false;

            Console.WriteLine($"\n[Node {NodeId}] Starting Consensus Check. Current Length: {maxLength}");

            foreach (var peer in peerChains.Where(p => p.NodeId != NodeId))
            {
                Console.WriteLine($"[Node {NodeId}] Checking Peer {peer.NodeId} (Length: {peer.Chain.Count})...");

                if (peer.Chain.Count > maxLength && peer.IsChainValid())
                {
                    maxLength = peer.Chain.Count;
                    longestValidChain = peer;
                    chainReplaced = true;
                }
            }

            if (chainReplaced)
            {
                Chain = longestValidChain.Chain;
                Console.WriteLine($"[Node {NodeId}] *** CHAIN REPLACED by Node {longestValidChain.NodeId}'s chain (New Length: {maxLength}). ***");
                return true;
            }

            Console.WriteLine($"[Node {NodeId}] *** Current chain is the longest/authoritative. ***");
            return false;
        }
    }

    public static class DecentralizedDemo
    {
        public static void Run()
        {
            Console.WriteLine("--- Initializing Decentralized Blockchain Simulation (C#) ---");

            Blockchain nodeA = new Blockchain("Node-A");
            Blockchain nodeB = new Blockchain("Node-B");
            Blockchain nodeC = new Blockchain("Node-C");

            List<Blockchain> networkPeers = new List<Blockchain> { nodeA, nodeB, nodeC };

            Console.WriteLine("\n\n=============== ROUND 1: Node A Mines ===============");
            nodeA.NewTransaction("Alice", "Bob", 10.5);
            nodeA.NewTransaction("Bob", "Charlie", 2.0);
            nodeA.MinePendingTransactions("Miner-A");

            nodeB.NewTransaction("Alice", "Bob", 10.5);
            nodeB.NewTransaction("Bob", "Charlie", 2.0);

            Console.WriteLine("\n\n=============== ROUND 2: Node C Mines ===============");
            nodeC.NewTransaction("Dave", "Eve", 50.0);
            nodeC.MinePendingTransactions("Miner-C");

            Console.WriteLine("\n\n=============== ROUND 3: Consensus Check (Node B Syncs) ===============");

            List<Blockchain> nodeBPossibleChains = new List<Blockchain> { nodeA, nodeB, nodeC };
            nodeB.ResolveConflicts(nodeBPossibleChains);

            Console.WriteLine("\n\n=============== ROUND 4: Node B Mines and creates the longest chain ===============");
            nodeB.NewTransaction("Charlie", "Alice", 3.0);
            nodeB.MinePendingTransactions("Miner-B");

            Console.WriteLine("\n\n=============== ROUND 5: Consensus Check (Node A Syncs) ===============");

            List<Blockchain> currentNetworkState = new List<Blockchain> { nodeA, nodeB, nodeC };
            nodeA.ResolveConflicts(currentNetworkState);

            Console.WriteLine($"\n[Final Status] Node A Length: {nodeA.Chain.Count}, Node B Length: {nodeB.Chain.Count}, Node C Length: {nodeC.Chain.Count}");
        }
    }
}
