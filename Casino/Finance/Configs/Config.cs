using System.Collections.Generic;

namespace CSharp_Exam.Casino.Finance.Configs;

public readonly record struct PaymentConfig(
    string Name,
    ushort MinLength = 16,
    ushort MaxLength = 16
);

public record struct PaymentMethod(
    string Name,
    string Id,
    decimal Balance,
    ValidationRegistry.PaymentMethods Method
);

public static class ValidationRegistry
{
    public enum PaymentMethods { Card, Crypto };

    public static readonly Dictionary<string, PaymentConfig> CardIndexes = new()
    {
        // ==========================================
        // 1. SPECIFIC BANKS / BINs (6-Digit Matches)
        // ==========================================

        // Ukrainian PrivatBank (Privat24)
        ["414949"] = new PaymentConfig("PrivatBank Debit (Visa)"),
        ["414960"] = new PaymentConfig("PrivatBank Credit (Visa)"),
        ["414961"] = new PaymentConfig("PrivatBank Credit (Visa)"),
        ["473121"] = new PaymentConfig("PrivatBank Gold (Visa)"),
        ["516874"] = new PaymentConfig("PrivatBank Universal (MasterCard)"),
        ["516875"] = new PaymentConfig("PrivatBank Universal (MasterCard)"),

        // Russian T-Bank (Formerly Tinkoff)
        ["220070"] = new PaymentConfig("T-Bank (MIR)"),
        ["437772"] = new PaymentConfig("T-Bank (Visa)"),
        ["437773"] = new PaymentConfig("T-Bank (Visa)"),
        ["513990"] = new PaymentConfig("T-Bank (MasterCard)"),
        ["521324"] = new PaymentConfig("T-Bank (MasterCard)"),

        // ==========================================
        // 2. GENERIC FALLBACKS (Short Matches)
        // ==========================================
        ["34"] = new PaymentConfig("Amex", MinLength: 15, MaxLength: 15),
        ["37"] = new PaymentConfig("Amex", MinLength: 15, MaxLength: 15),
        ["51"] = new PaymentConfig("MasterCard"),
        ["52"] = new PaymentConfig("MasterCard"),
        ["53"] = new PaymentConfig("MasterCard"),
        ["54"] = new PaymentConfig("MasterCard"),
        ["55"] = new PaymentConfig("MasterCard"),
        ["4"] = new PaymentConfig("Visa"),
        ["6"] = new PaymentConfig("Discover"),
        ["2"] = new PaymentConfig("MIR")
    };

    public static readonly Dictionary<string, PaymentConfig> CryptoIndexes = new()
    {
        // Bitcoin (BTC)
        ["1"] = new PaymentConfig("Bitcoin (Legacy)", MinLength: 26, MaxLength: 35),
        ["3"] = new PaymentConfig("Bitcoin (Nested SegWit)", MinLength: 26, MaxLength: 35),
        ["bc1"] = new PaymentConfig("Bitcoin (Native SegWit/Taproot)", MinLength: 42, MaxLength: 62),

        // Ethereum (ETH)
        ["0x"] = new PaymentConfig("Ethereum", MinLength: 42, MaxLength: 42),

        // BNB Beacon Chain
        ["bnb"] = new PaymentConfig("Binance Chain (Legacy)", MinLength: 42, MaxLength: 42),

        // Solana (SOL)
        ["SOL_BASE58"] = new PaymentConfig("Solana", MinLength: 32, MaxLength: 44)
    };
}