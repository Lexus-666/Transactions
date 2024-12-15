using System;
using System.Text;
using System.Text.Json;
using kursah_5semestr.Abstractions;
using kursah_5semestr.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace kursah_5semestr.Services
{
    public class DataUpdaterService : IDataUpdaterService
    {
        private IBrokerService _brokerService;
        private readonly IServiceScopeFactory _scopeFactory;

        public DataUpdaterService(IBrokerService brokerService, IServiceScopeFactory scopeFactory)
        {
            _brokerService = brokerService;
            _scopeFactory = scopeFactory;
        }

        private async Task ProcessMessage(String message)
        {
            try
            {
                var instanceChanged = JsonSerializer.Deserialize<InstanceChanged>(message);
                if (instanceChanged != null)
                {
                    switch (instanceChanged.Entity)
                    {
                        case "user":
                            switch (instanceChanged.Action)
                            {
                                case "create":
                                    var (user, error) = User.Create(
                                        id: Guid.Parse(instanceChanged.Data["Id"].ToString()),
                                        login: instanceChanged.Data["Login"].ToString(),
                                        passwordHash: "***************************************************");
                                    using (IServiceScope scope = _scopeFactory.CreateScope())
                                    {
                                        var usersService = scope.ServiceProvider.GetService<IUsersService>();
                                        await usersService.CreateUser(user);
                                    }
                                    break;
                            }
                            break;
                        case "order":
                            switch (instanceChanged.Action)
                            {
                                case "create":
                                    double amount = 0;
                                    instanceChanged.Data["Amount"].TryGetDouble(out amount);
                                    var (order, error) = Order.Create(
                                        id: Guid.Parse(instanceChanged.Data["Id"].ToString()!),
                                        date: DateTime.Parse(instanceChanged.Data["Date"].ToString()!).ToUniversalTime(),
                                        userId: Guid.Parse(instanceChanged.Data["UserId"].ToString()!),
                                        status: instanceChanged.Data["Status"].ToString()!,
                                        amount: amount);
                                    if (order != null)
                                    {
                                        using (IServiceScope scope = _scopeFactory.CreateScope())
                                        {
                                            var ordersService = scope.ServiceProvider.GetService<IOrdersService>();
                                            await ordersService!.CreateOrder(order);
                                            var transactionsService = scope.ServiceProvider.GetService<ITransactionsService>();
                                            var (transaction, error2) = Transaction.Create(
                                                transactionDetails: $"Транзакция на сумму {order.Amount} от {order.Date}",
                                                order: order);
                                            if (transaction != null)
                                            {
                                                await transactionsService!.CreateTransaction(transaction);
                                            } 
                                            else
                                            {
                                                Console.Write(error);
                                            }
                                        }
                                    } else
                                    {
                                        Console.WriteLine(error);
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public async Task Start()
        {
            await _brokerService.Subscribe("changes", ProcessMessage);
        }
    }
}
