
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Economy
{
    /// <summary>
    /// Tracks the overall financial status of the player.
    /// </summary>
    [Serializable]
    public class EconomyState
    {
        /// <summary>
        /// The current amount of money available to spend.
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// A history of all financial transactions.
        /// </summary>
        public List<LedgerEntry> Ledger { get; set; } = new List<LedgerEntry>();
    }

    /// <summary>
    /// Represents a single financial transaction (Income or Expense).
    /// </summary>
    [Serializable]
    public class LedgerEntry
    {
        /// <summary>
        /// Unique identifier for the transaction.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// When the transaction occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The monetary value. Positive for income, negative for expenses.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Textual reason for the transaction (e.g., "Sale of Coffee").
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The classification of the expense or income.
        /// </summary>
        public ExpenseCategory Category { get; set; }
        
        /// <summary>
        /// The product involved in the transaction, if any.
        /// </summary>
        public ProductType? RelatedProduct { get; set; }

        /// <summary>
        /// The ID of the bird involved, if applicable (e.g., for Vet bills).
        /// </summary>
        public string RelatedBirdId { get; set; }

        /// <summary>
        /// A short, user-friendly description of the transaction.
        /// </summary>
        public string ShortDescription { get; set; }
    }
}