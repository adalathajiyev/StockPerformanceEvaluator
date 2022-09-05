using System;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection.PortableExecutable;
using StockPerformanceEvaluator.Services;
using StockPerformanceEvaluator.Services.ApiClientService;
using Castle.Core.Configuration;
using StockPerformanceEvaluator.Controllers;
using StockPerformanceEvaluator.Models;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace StockPerformanceEvaluator.Tests.Services;

public class StockPerformanceControllerTests
{
    protected StockPerformanceController _contoller { get; set; } = null!;

    protected StockPerformanceDTO stockPerformanceDTO = new StockPerformanceDTO
    {
        InputSymbol = "",
        BenchmarkSymbol = "",
        StockPerformance = new List<DailyPerformanceDTO>(),
    };

    public class GetDailyPerformanceTest : StockPerformanceControllerTests
    {
        public GetDailyPerformanceTest()
        {
            var logger = new Mock<ILogger<StockPerformanceController>>();
            var stockPriceService = new Mock<IStockPriceService>();

            stockPriceService.Setup(m => m.EvaluateDailyStockPerformance(It.Is<string>(a => a.Length > 1))).ReturnsAsync(stockPerformanceDTO);

            stockPriceService.Setup(m => m.EvaluateDailyStockPerformance(It.Is<string>(a => a.Length < 2))).ThrowsAsync(new BadHttpRequestException("symbol should be more than 1 charachter"));

            _contoller = new StockPerformanceController(
                logger: logger.Object,
                stockPriceService: stockPriceService.Object
                );
        }

        [Fact]
        public async Task CallsTheStrategyWithTheHandler()
        {
            await Assert.ThrowsAsync<BadHttpRequestException>(() => _contoller.GetDailyPerformance("a"));
        }

        [Theory]
        [InlineData("IBM")]
        [InlineData("APL")]
        [InlineData("TSL")]
        public void CanAddTheory(string symbol)
        {

            var actual = JsonConvert.SerializeObject(_contoller.GetDailyPerformance(symbol));
            var expected = JsonConvert.SerializeObject(stockPerformanceDTO);

            Assert.Equal(expected, actual);
        }


    }
}