# Proxy API

A simple proxy API to get around CORS issues. You can use this to add an endpoint to your API.

For a typical Cadmus API and app, follow the instructions below.

## API

In your backend API, add a **proxy API controller** like this ([ProxyController.cs](ProxyApi/Controllers/ProxyController.cs)):

```cs
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
```

This requires CORS, which should already be setup for the API, and the following services:

```cs
builder.Services.AddHttpClient();
// for caching
builder.Services.AddResponseCaching();
```

If you are not using response caching (i.e. you remove the `ResponseCacheAttribute` from the sample code above) you can remove `AddResponseCaching`.

Also, if you use response caching (which is the default) add this middleware to your app's pipeline (immediately after CORS):

```cs
app.UseResponseCaching();
```

## Angular App

Once you provide this proxy endpoint, **configure the Angular app** like this:

```ts
{ provide: HTTP_INTERCEPTORS, useClass: ProxyInterceptor, multi: true },
{ provide: PROXY_INTERCEPTOR_OPTIONS, useValue: {
    proxyUrl: 'http://localhost:5161/api/proxy',
    urls: [
      'http://lookup.dbpedia.org/api/search',
      'http://lookup.dbpedia.org/api/prefix'
    ]
  }
},
```

This configures the [Cadmus bricks proxy interceptor](https://github.com/vedph/cadmus-bricks-shell), whose task is intercepting requests to services not supporting CORS, like DBPedia, and rewrite them so that they are redirected to the proxy service endpoint shown above under (1). Using a proxy bypasses the issues of browsers in consuming services not providing support for CORS or JSONP.
