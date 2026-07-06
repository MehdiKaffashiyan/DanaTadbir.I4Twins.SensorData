using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DanaTadbir.I4Twins.SensorData.Api.Controllers
{
    [ApiController]
    [Route("api/sensor-readings")]
    public sealed class SensorReadingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SensorReadingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SensorReadingBucketDto>>> GetAsync(
            [FromQuery] GetAggregatedReadingsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync<GetAggregatedReadingsRequest, IReadOnlyList<SensorReadingBucketDto>>(
                request,
                cancellationToken);

            return Ok(result);
        }
    }
}
