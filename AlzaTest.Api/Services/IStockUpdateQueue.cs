using System.Threading.Tasks;

namespace AlzaTest.Api.Services
{
    public record StockUpdate(int ProductId, int Quantity);

    public interface IStockUpdateQueue
    {
        Task EnqueueAsync(StockUpdate stockUpdate);
        Task<StockUpdate> DequeueAsync();
    }
}
