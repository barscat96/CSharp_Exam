using System.Collections.Generic;

namespace CSharp_Exam.Casino.Finance;

interface IConfigBase
{
    string Name { get; }
    ushort MinLength { get; }
    ushort MaxLength { get; }
}

public readonly record struct CardConfig(
    string Name, 
    ushort MinLength = 16, 
    ushort MaxLength = 16
) : IConfigBase;
public readonly record struct CryptoConfig(
    string Name, 
    ushort MinLength,
    ushort MaxLength, 
    string Prefix
) : IConfigBase;

static class ValidationRegistry
{
    public enum PaymentTypes { Card, Crypto };

    public static readonly Dictionary<string, CardConfig> CardIndexes = new()
    {
        // ==========================================
        // 1. SPECIFIC BANKS / BINs (6-Digit Matches)
        // ==========================================

        // Ukrainian PrivatBank (Privat24)
        ["414949"] = new("PrivatBank Debit (Visa)"),
        ["414960"] = new("PrivatBank Credit (Visa)"),
        ["414961"] = new("PrivatBank Credit (Visa)"),
        ["473121"] = new("PrivatBank Gold (Visa)"),
        ["516874"] = new("PrivatBank Universal (MasterCard)"),
        ["516875"] = new("PrivatBank Universal (MasterCard)"),

        // Russian T-Bank (Formerly Tinkoff)
        ["220070"] = new("T-Bank (MIR)"),
        ["437772"] = new("T-Bank (Visa)"),
        ["437773"] = new("T-Bank (Visa)"),
        ["513990"] = new("T-Bank (MasterCard)"),
        ["521324"] = new("T-Bank (MasterCard)"),

        // ==========================================
        // 2. GENERIC FALLBACKS (Short Matches)
        // ==========================================
        ["34"] = new("Amex", MinLength: 15, MaxLength: 15),
        ["37"] = new("Amex", MinLength: 15, MaxLength: 15),
        ["51"] = new("MasterCard"),
        ["52"] = new("MasterCard"),
        ["53"] = new("MasterCard"),
        ["54"] = new("MasterCard"),
        ["55"] = new("MasterCard"),
        ["4"] = new("Visa"),
        ["6"] = new("Discover"),
        ["2"] = new("MIR")
    };

    public static readonly Dictionary<string, CryptoConfig> CryptoWalletPrefixes = new()
    {
        // Bitcoin (BTC) — у каждого типа своя точная длина
        ["1"] = new("Bitcoin (Legacy)", MinLength: 26, MaxLength: 35, Prefix: "1"),
        ["3"] = new("Bitcoin (Nested SegWit)", MinLength: 26, MaxLength: 35, Prefix: "3"),
        ["bc1"] = new("Bitcoin (Native SegWit/Taproot)", MinLength: 42, MaxLength: 62, Prefix: "bc1"),

        // Ethereum (ETH) / EVM-сети
        ["0x"] = new("Ethereum / EVM Network", MinLength: 42, MaxLength: 42, Prefix: "0x"),

        // BNB Beacon Chain
        ["bnb"] = new("Binance Chain (Legacy)", MinLength: 42, MaxLength: 42, Prefix: "bnb"),

        // Solana (SOL) — префикса нет, используем пустую строку. Проверка идет по длине (32-44) и Base58
        ["SOL_BASE58"] = new("Solana", MinLength: 32, MaxLength: 44, Prefix: "")
    };
}