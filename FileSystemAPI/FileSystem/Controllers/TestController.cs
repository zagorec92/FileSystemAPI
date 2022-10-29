using FileSystem.Core.Persistence;
using FileSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FileSystem.Controllers
{
	[ApiController]
	[Route("{customerId:guid}/test")]
	public class TestController : ControllerBase
	{
		private readonly ILogger<TestController> _logger;
		private readonly FileSystemDbContext _dbContext;

		public TestController(ILogger<TestController> logger, FileSystemDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		[HttpGet(Name = "GetAllCustomerContent")]
		public IEnumerable<ContentViewModel> GetAll(Guid customerId)
		{
			var customerDirectories = _dbContext.Content
				.Where(x => x.CustomerId == customerId)
				.ToList();

			var retVal = customerDirectories.Select(x => new ContentViewModel(x));

			return retVal;
		}
	}
}