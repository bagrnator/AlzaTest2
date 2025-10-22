using System.Threading.Channels;

namespace AlzaTest.Api.Services
{
    public class InMemoryStockUpdateQueue : IStockUpdateQueue
    {
        private readonly Channel<StockUpdate> _queue = Channel.CreateUnbounded<StockUpdate>();

        public void Enqueue(StockUpdate stockUpdate)
        {
            _queue.Writer.TryWrite(stockUpdate);
        }

        public async Task EnqueueAsync(StockUpdate stockUpdate)
        {
             await _queue.Writer.WriteAsync(stockUpdate);
        }

        public async Task<StockUpdate> DequeueAsync()
        {
            return await _queue.Reader.ReadAsync();
        }
    }
}
