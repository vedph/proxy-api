using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxyApi.Controllers;

[ApiController]
[Route("api/proxy")]
public sealed class ProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ProxyController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the response from the specified URI.
    /// </summary>
    /// <param name="uri">The URI, e.g.
    /// <c>https://lookup.dbpedia.org/api/search?query=plato&format=json&maxResults=10</c>
    /// .</param>
    /// <returns>Response.</returns>
    [HttpGet]
    [ResponseCache(Duration = 60 * 10, VaryByQueryKeys = ["uri"], NoStore = false)]
    public async Task<IActionResult> Get([FromQuery] string uri)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content
                    .ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return StatusCode((int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            return StatusCode(500, ex.Message);
        }
    }
}
