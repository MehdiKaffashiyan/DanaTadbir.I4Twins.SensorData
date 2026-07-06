using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests;
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
        public async Task<ActionResult<IReadOnlyList<SensorReadingDto>>> GetAsync(
            [FromQuery] GetSensorReadingsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync<GetSensorReadingsRequest, IReadOnlyList<SensorReadingDto>>(
                request,
                cancellationToken);

            return Ok(result);
        }
    }
}
