namespace Unobserver.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Task.Run(async () =>
            {
                await Task.Delay(4000);

                _logger.LogInformation("[WARNING] About to throw an unobserved exception.");

                throw new InvalidOperationException("This is an unobserved exception. " +
                    "The ONLY place you can see this is in TaskScheduler.UnobservedTaskException handler " +
                    "and ONLY after this task gets garbage-collected!!!");
            });

            _logger.LogInformation("[SUCCESS] GET api/values");

            return new[] { "value1", "value2" };
        }

        // Invoke this action to force GC collection; otherwise you'll have to wait for a loooooooong time
        //
        // GET api/values/gccollect
        [HttpGet("gccollect")]
        public ActionResult<string> GCCollect()
        {
            // Collects all GC generations up to MaxGeneration
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: false);

            return "Garbage collected successfully.";
        }
    }
}
