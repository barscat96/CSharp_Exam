using System.Collections.Generic;

namespace CSharp_Exam.Casino.Finance.Configs;

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
    public enum PaymentMethods { Card, Crypto };

    public static readonly Dictionary<string, IConfigBase> CardIndexes = new()
    {
        // ==========================================
        // 1. SPECIFIC BANKS / BINs (6-Digit Matches)
        // ==========================================

        // Ukrainian PrivatBank (Privat24)
        ["414949"] = new CardConfig("PrivatBank Debit (Visa)"),
        ["414960"] = new CardConfig("PrivatBank Credit (Visa)"),
        ["414961"] = new CardConfig("PrivatBank Credit (Visa)"),
        ["473121"] = new CardConfig("PrivatBank Gold (Visa)"),
        ["516874"] = new CardConfig("PrivatBank Universal (MasterCard)"),
        ["516875"] = new CardConfig("PrivatBank Universal (MasterCard)"),

        // Russian T-Bank (Formerly Tinkoff)
        ["220070"] = new CardConfig("T-Bank (MIR)"),
        ["437772"] = new CardConfig("T-Bank (Visa)"),
        ["437773"] = new CardConfig("T-Bank (Visa)"),
        ["513990"] = new CardConfig("T-Bank (MasterCard)"),
        ["521324"] = new CardConfig("T-Bank (MasterCard)"),

        // ==========================================
        // 2. GENERIC FALLBACKS (Short Matches)
        // ==========================================
        ["34"] = new CardConfig("Amex", MinLength: 15, MaxLength: 15),
        ["37"] = new CardConfig("Amex", MinLength: 15, MaxLength: 15),
        ["51"] = new CardConfig("MasterCard"),
        ["52"] = new CardConfig("MasterCard"),
        ["53"] = new CardConfig("MasterCard"),
        ["54"] = new CardConfig("MasterCard"),
        ["55"] = new CardConfig("MasterCard"),
        ["4"] = new CardConfig("Visa"),
        ["6"] = new CardConfig("Discover"),
        ["2"] = new CardConfig("MIR")
    };

    public static readonly Dictionary<string, IConfigBase> CryptoIndexes = new()
    {
        // Bitcoin (BTC)
        ["1"] = new CryptoConfig("Bitcoin (Legacy)", MinLength: 26, MaxLength: 35, Prefix: "BTC"),
        ["3"] = new CryptoConfig("Bitcoin (Nested SegWit)", MinLength: 26, MaxLength: 35, Prefix: "BTC"),
        ["bc1"] = new CryptoConfig("Bitcoin (Native SegWit/Taproot)", MinLength: 42, MaxLength: 62, Prefix: "BTC"),

        // Ethereum (ETH)
        ["0x"] = new CryptoConfig("Ethereum", MinLength: 42, MaxLength: 42, Prefix: "ETH"),

        // BNB Beacon Chain
        ["bnb"] = new CryptoConfig("Binance Chain (Legacy)", MinLength: 42, MaxLength: 42, Prefix: "BNB"),

        // Solana (SOL)
        ["SOL_BASE58"] = new CryptoConfig("Solana", MinLength: 32, MaxLength: 44, Prefix: "SOL")
    };
}