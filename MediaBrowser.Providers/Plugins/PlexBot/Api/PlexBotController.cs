using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public Task<IActionResult> Get(string path)
    {
        return Forward(HttpMethod.Get, path);
    }

    /// <summary>
    /// Acts as a proxy for POST requests to PlexBot.
    /// </summary>
    /// <param name="path">The url path.</param>
    /// <returns>The response from PlexBot API.</returns>
    [HttpPost("{*path}")]
    public Task<IActionResult> Post(string path)
    {
        return Forward(HttpMethod.Post, path);
    }

    /// <summary>
    /// Acts as a proxy for PUT requests to PlexBot.
    /// </summary>
    /// <param name="path">The url path.</param>
    /// <returns>The response from PlexBot API.</returns>
    [HttpPut("{*path}")]
    public Task<IActionResult> Put(string path)
    {
        return Forward(HttpMethod.Put, path);
    }

    /// <summary>
    /// Acts as a proxy for DELETE requests to PlexBot.
    /// </summary>
    /// <param name="path">The url path.</param>
    /// <returns>The response from PlexBot API.</returns>
    [HttpDelete("{*path}")]
    public Task<IActionResult> Delete(string path)
    {
        return Forward(HttpMethod.Delete, path);
    }

    private async Task<IActionResult> Forward(HttpMethod method, string path)
    {
        var request = await CreateForwardRequest(method, path).ConfigureAwait(false);
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return Content(content, "application/json");
        }

        return StatusCode((int)response.StatusCode);
    }

    private async Task<HttpRequestMessage> CreateForwardRequest(HttpMethod method, string path)
    {
        // Get the query string from the incoming request
        var queryString = HttpContext.Request.QueryString;

        // Get the body from the incoming request
        string requestBody;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        // Create the outgoing request
        var request = new HttpRequestMessage(method, $"{Plugin.Instance?.Configuration.ApiUrl}/{path}{queryString}");

        // Set the body of the outgoing request
        if (!string.IsNullOrEmpty(requestBody))
        {
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        }

        return request;
    }
}
