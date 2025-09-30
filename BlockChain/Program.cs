using CentralizedBlockchainDemo;
using DecentralizedBlockchainDemo;

Console.WriteLine("Select a blockchain demo to run:");
Console.WriteLine("1 - Centralized Blockchain Demo");
Console.WriteLine("2 - Decentralized Blockchain Demo");
Console.WriteLine("3 - Run Both Demos");
Console.WriteLine("0 - Exit");
Console.Write("Enter option: ");

string? choice = Console.ReadLine()?.Trim();

switch (choice)
{
    case "1":
        Console.WriteLine("\n=========== CENTRALIZED BLOCKCHAIN DEMO ===========\n");
        CentralizedDemo.Run();
        break;
    case "2":
        Console.WriteLine("\n=========== DECENTRALIZED BLOCKCHAIN DEMO ===========\n");
        DecentralizedDemo.Run();
        break;
    case "3":
        Console.WriteLine("\n=========== CENTRALIZED BLOCKCHAIN DEMO ===========\n");
        CentralizedDemo.Run();

        Console.WriteLine("\n\n=========== DECENTRALIZED BLOCKCHAIN DEMO ===========\n");
        DecentralizedDemo.Run();
        break;
    case "0":
        Console.WriteLine("Exiting application.");
        break;
    default:
        Console.WriteLine("Invalid option selected. Please run the application again and choose 1, 2, 3, or 0.");
        break;
}
