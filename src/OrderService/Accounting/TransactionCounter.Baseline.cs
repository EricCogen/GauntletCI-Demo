using OrderService.Abstractions;

namespace OrderService.Accounting;

public class TransactionCounter
{
    private int _counter;
    private readonly List<Transaction> _ledger = new();
    private readonly object _mutex = new();

    public void RecordTransaction(Transaction tx)
    {
        lock (_mutex)
        {
            _counter++;
            _ledger.Add(tx);
        }
    }

    public int GetCount()
    {
        lock (_mutex)
        {
            return _counter;
        }
    }

    public int GetLedgerCount()
    {
        lock (_mutex)
        {
            return _ledger.Count;
        }
    }
}

public record Transaction(Guid Id, decimal Amount, DateTime CreatedAt);
