using CSharp_Exam.Casino.Attributes;
using CSharp_Exam.Casino.Services.Exceptions;
using CSharp_Exam.Casino.Services.Types;
using System.Text.Json;

namespace CSharp_Exam.Casino.Finance.Classes;

partial class BankAccount
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    private string GetFilePath(string directoryPath)
        => Path.Combine(directoryPath, $"BankAccount_{Id}.json");

    public void Save(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(GetFilePath(directoryPath), json);
    }

    public void Load(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new DataOperationFailed(ActionType.Load, "Failed to load bank account data");

        string savedJson = File.ReadAllText(GetFilePath(directoryPath));
        BankAccount? loadedAccount = JsonSerializer.Deserialize<BankAccount>(savedJson, options)
            ?? throw new DataOperationFailed(ActionType.Load, "Failed to load bank account data");

        user = new(loadedAccount.user);
        TransactionsHistory = new(loadedAccount.TransactionsHistory);
        AccountCreatedAt = loadedAccount.AccountCreatedAt;
        PaymentMethods = new(loadedAccount.PaymentMethods);
    }
}

partial class Bank
{
    private string DirectoryPath => Path.Combine("SaveData", Name);
    private string FilePath => Path.Combine(DirectoryPath, "Bank.json");

    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    [Save(nameof(Bank))]
    public void Save()
    {
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        foreach (BankAccount account in Accounts.Values)
            account.Save(DirectoryPath);

        string json = JsonSerializer.Serialize(Accounts.Keys, options);
        File.WriteAllText(FilePath, json);
    }

    [Load(nameof(Bank))]
    public void Load()
    {
        Accounts.Clear();

        if (!Directory.Exists(DirectoryPath))
            throw new DataOperationFailed(ActionType.Load, "Failed to load bank data");

        if (!File.Exists(FilePath)) 
            throw new DataOperationFailed(ActionType.Load, "Failed to load bank data");

        string json = File.ReadAllText(FilePath);

        List<uint> accountIds = JsonSerializer.Deserialize<List<uint>>(json, options)
            ?? throw new DataOperationFailed(ActionType.Load, "Failed to load account IDs");

        foreach (uint id in accountIds)
        {
            BankAccount account = new(id);

            try
            {
                account.Load(DirectoryPath);
                Accounts.Add(id, account);
            }
            catch (DataOperationFailed e)
            {
                Console.WriteLine($"Failed to load bank account with ID {id}: {e.Message}");
            }
        }
    }

    public bool UsernameExists(string username)
        => Accounts.Values.Any(x =>
            string.Equals(
                x.user?.Username,
                username,
                StringComparison.OrdinalIgnoreCase
            ));
}