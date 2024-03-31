using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MediaBrowser.Providers.Plugins.PlexBot.Api;

/// <summary>
/// The PlexBot api controller.
/// </summary>
[ApiController]
[Authorize(Policy = Policies.PlexBotAccess)]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class PlexBotController : ControllerBase
{
#pragma warning disable IDISP006
    private readonly HttpClient _httpClient;
#pragma warning restore IDISP006

    /// <summary>
    /// Initializes a new instance of the <see cref="PlexBotController"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    public PlexBotController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("PlexBot");
    }

    /// <summary>
    /// Acts as a proxy for GET requests to PlexBot.
    /// </summary>
    /// <param name="path">The url path.</param>
    /// <returns>The response from PlexBot API.</returns>
    [HttpGet("{*path}")]
    public async Task<IActionResult> Get(string path)
    {
        var response = await _httpClient.GetAsync($"{Plugin.Instance?.Configuration.ApiUrl}/{path}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return Content(content, "application/json");
        }

        return StatusCode((int)response.StatusCode);
    }
}
