using HomeBoxLanding.Api.Core.Events.Types;
using HomeBoxLanding.Api.Features.Links;
using HomeBoxLanding.Api.Features.Links.Types;
using HomeBoxLanding.Api.Features.Sonarr.Types;
using HomeBoxLanding.Api.Features.WebSockets.Types;
using Minio;
using Newtonsoft.Json;

namespace HomeBoxLanding.Api.Features.Sonarr;

public class SonarrService : ISubscriber
{
    private readonly LinksService _linksService;
    private bool _isStarted = false;

    public SonarrService(LinksService linksService)
    {
        _linksService = linksService;
    }

    public SonarrActivityResponse GetActivity(Guid identifier)
    {
        var link = _linksService.GetAllLinks().Links.FirstOrDefault(x => x.Identifier == identifier);

        if (link == null)
        {
            return new SonarrActivityResponse();
        }

        var totalSeries = GetTotalSeries(link);
        
        var totalMissing = GetTotalMissing(link);
        
        var totalQueue = GetTotalQueue(link);

        if (totalSeries == null)
        {
            return new SonarrActivityResponse();
        }

        return new SonarrActivityResponse
        {
            TotalNumberOfSeries = totalSeries.Count,
            TotalNumberOfQueuedEpisodes = totalQueue.Total,
            TotalNumberOfMissingEpisodes = totalMissing.Total
        };
    }

    private List<SonarrSeries> GetTotalSeries(Link link)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(20);
        var result = httpClient.GetAsync($"{link.Url}api/v3/series?apiKey=f0d38b8abcfa4ade991ecd8d6ecb5674").Result;
        var response = result.Content.ReadAsStringAsync().Result;

        List<SonarrSeries>? parsedResponse;
        
        try
        {
            parsedResponse = JsonConvert.DeserializeObject<List<SonarrSeries>>(response);
        }
        catch (Exception)
        {
            return new List<SonarrSeries>();
        }

        return parsedResponse ?? new List<SonarrSeries>();
    }

    private SonarrMissing GetTotalMissing(Link link)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(20);
        var result = httpClient.GetAsync($"{link.Url}api/v3/wanted/missing?apiKey=f0d38b8abcfa4ade991ecd8d6ecb5674").Result;
        var response = result.Content.ReadAsStringAsync().Result;

        SonarrMissing? parsedResponse;
        
        try
        {
            parsedResponse = JsonConvert.DeserializeObject<SonarrMissing>(response);
        }
        catch (Exception)
        {
            return new SonarrMissing();
        }

        return parsedResponse ?? new SonarrMissing();
    }

    private SonarrQueue GetTotalQueue(Link link)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(20);
        var result = httpClient.GetAsync($"{link.Url}api/v3/queue?apiKey=f0d38b8abcfa4ade991ecd8d6ecb5674").Result;
        var response = result.Content.ReadAsStringAsync().Result;

        SonarrQueue? parsedResponse;
        
        try
        {
            parsedResponse = JsonConvert.DeserializeObject<SonarrQueue>(response);
        }
        catch (Exception)
        {
            return new SonarrQueue();
        }

        return parsedResponse ?? new SonarrQueue();
    }

    public void OnStarted()
    {
        _isStarted = true;

        Task.Run(() =>
        {
            while (_isStarted)
            {
                var linkService = new LinksService(new LinksRepository(), new MinioClient());
                var radarrLink = linkService.GetAllLinks().Links.FirstOrDefault(x => x.Name.ToUpper().Contains("SONARR"));

                if (radarrLink is null)
                    return;
                
                var activity = GetActivity(radarrLink.Identifier!.Value);    
                    
                WebSockets.WebSocketManager.Instance().SendToAllClients(WebSocketKey.SonarrActivity, new
                {
                    Response = new
                    {
                        Data = new
                        {
                            TotalNumberOfSeries = activity.TotalNumberOfSeries,
                            TotalNumberOfQueuedEpisodes = activity.TotalNumberOfQueuedEpisodes,
                            TotalNumberOfMissingEpisodes = activity.TotalNumberOfMissingEpisodes
                        }
                    }
                });
                
                Thread.Sleep(5000);
            }
        }, CancellationToken.None);
    }

    public void OnStopping()
    {
        _isStarted = false;
    }

    public void OnStopped()
    {
        // Do nothing
    }
}