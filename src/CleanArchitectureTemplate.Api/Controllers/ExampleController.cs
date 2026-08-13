/*using CleanArchitectureTemplate_Application.Dtos.Pagination;
using CleanArchitectureTemplate_Application.ServiceContract;
using CleanArchitectureTemplate_Domain.IRepositoryContract;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitectureTemplate.Api.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        private readonly IGenericRepository<YourEntity> _repository;

        public ExampleController(IGenericRepository<YourEntity> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get paginated list", Description = "Get a paginated list of entities")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<YourEntityDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var (data, totalCount) = await _repository.GetPagedWithCountAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                orderBy: q => q.OrderByDescending(x => x.CreatedAt),
                cancellationToken: cancellationToken);

            var result = new PagedResultDTO<YourEntityDTO>
            {
                Data = data.Select(x => MapToDTO(x)),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(new ApiResponse<PagedResultDTO<YourEntityDTO>>
            {
                Data = result,
                Message = "Data retrieved successfully"
            });
        }

        // Example with filtering
        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search and paginate", Description = "Search entities with pagination")]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDTO<YourEntityDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var (data, totalCount) = await _repository.GetPagedWithCountAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                predicate: x => x.Name.Contains(searchTerm),
                orderBy: q => q.OrderByDescending(x => x.CreatedAt),
                cancellationToken: cancellationToken);

            var result = new PagedResultDTO<YourEntityDTO>
            {
                Data = data.Select(x => MapToDTO(x)),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(new ApiResponse<PagedResultDTO<YourEntityDTO>>
            {
                Data = result,
                Message = "Search results retrieved successfully"
            });
        }

        private YourEntityDTO MapToDTO(YourEntity entity)
        {
            // Implement your mapping logic here
            throw new NotImplementedException();
        }
    }
}*/