using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CentralizedBlockchainDemo
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
    public string PreviousHash { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Data { get; set; }
        public int Nonce { get; set; } = 0;

        public Block(int index, string previousHash, string data)
        {
            Index = index;
            Timestamp = DateTime.Now;
            PreviousHash = previousHash;
            Data = data;
        }

        public string CalculateHash()
        {
            string rawData = $"{Index}{Timestamp}{PreviousHash}{Data}{Nonce}";

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

            Console.WriteLine($"Block {Index} Mined! Hash: {Hash}");
        }
    }

    public class Blockchain
    {
        public IList<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 4;

        public Blockchain()
        {
            Chain = new List<Block>();
            Chain.Add(CreateGenesisBlock());
        }

        private Block CreateGenesisBlock()
        {
            Block genesisBlock = new Block(0, "0", "Genesis Block Data");
            genesisBlock.MineBlock(Difficulty);
            return genesisBlock;
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public void AddBlock(Block newBlock)
        {
            newBlock.PreviousHash = GetLatestBlock().Hash;

            Console.WriteLine($"\n--- Start Mining Block {newBlock.Index} ---");
            newBlock.MineBlock(Difficulty);

            Chain.Add(newBlock);
        }

        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    Console.WriteLine($"\nValidation Failed: Block {currentBlock.Index} has an invalid internal hash.");
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    Console.WriteLine($"\nValidation Failed: Block {currentBlock.Index} PreviousHash is corrupted.");
                    return false;
                }
            }
            Console.WriteLine("\nValidation Succeeded: The blockchain is secure.");
            return true;
        }
    }

    public static class CentralizedDemo
    {
        public static void Run()
        {
            Console.WriteLine("--- Initializing My Central Blockchain (C#) ---");

            Blockchain myChain = new Blockchain();
            Console.WriteLine($"Blockchain created. Difficulty: {myChain.Difficulty}\n");
            Console.WriteLine($"Genesis Block Hash: {myChain.Chain[0].Hash}\n");

            Block block1 = new Block(1, myChain.GetLatestBlock().Hash, "Transaction Data: Sender A to Receiver B, Amount 10 BTC");
            myChain.AddBlock(block1);

            Block block2 = new Block(2, myChain.GetLatestBlock().Hash, "Transaction Data: Sender C to Receiver D, Amount 5 ETH");
            myChain.AddBlock(block2);

            Block block3 = new Block(3, myChain.GetLatestBlock().Hash, "Transaction Data: Miner Reward, Amount 0.5 ZEC");
            myChain.AddBlock(block3);

            Console.WriteLine("\n\n--- Testing Chain Integrity ---");
            myChain.IsChainValid();

            Console.WriteLine("\n\n--- TAMPERING WITH BLOCK 2 DATA ---");

            Block tamperedBlock = myChain.Chain[2];
            tamperedBlock.Data = "Transaction Data: SENDER HACKER to TARGET, Amount 1000 BTC (FRAUDULENT)";

            Console.WriteLine($"Block 2 Data changed. Original Hash: {tamperedBlock.Hash}");
            Console.WriteLine($"Block 2 New Calculated Hash: {tamperedBlock.CalculateHash()}");

            Console.WriteLine("\n--- Re-Testing Chain Integrity After Tampering ---");
            myChain.IsChainValid();

            Console.WriteLine("\n--- Attacker Tries to Re-Mine Block 2 ---");
            tamperedBlock.MineBlock(myChain.Difficulty);

            Console.WriteLine("\n--- Re-Testing Chain Integrity After Re-Mining Block 2 ---");
            myChain.IsChainValid();
        }
    }
}
