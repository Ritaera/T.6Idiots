using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

public class ARMapController : MonoBehaviour
{
    [SerializeField] private string _apiKey = "AIzaSyCRKHlpmFQ1SRnIX465zxw9gslA6T7UiM8";
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private ARSessionOrigin _arSessionOrigin;
    [SerializeField] private SearchResultsController _searchResultsController;

    private Vector2 currentLocation;
    private List<Vector2> routePoints;

    void Start()
    {
        StartCoroutine(GetCurrentLocation());
    }

    IEnumerator GetCurrentLocation()
    {
        if (!Input.location.isEnabledByUser)
            yield break;

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            currentLocation = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
            Debug.Log("Location: " + currentLocation.x + " " + currentLocation.y);
        }
    }

    public void SearchAndDisplayRoute(string keyword)
    {
        StartCoroutine(SearchPlace(keyword));
    }

    IEnumerator SearchPlace(string keyword)
    {
        string url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={keyword}&key={_apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            List<SearchResult> searchResults = ParseSearchResults(request.downloadHandler.text);
            _searchResultsController.DisplaySearchResults(searchResults);
        }
    }

    List<SearchResult> ParseSearchResults(string json)
    {
        List<SearchResult> results = new List<SearchResult>();
        JObject jsonResponse = JObject.Parse(json);
        string status = jsonResponse["status"].ToString();

        if (status == "ZERO_RESULTS")
        {
            Debug.LogError("No results found.");
            return results; // 빈 리스트 반환
        }

        JArray searchResults = (JArray)jsonResponse["results"];
        foreach (JToken result in searchResults)
        {
            string name = result["name"].ToString();
            JToken location = result["geometry"]["location"];
            float lat = location["lat"].Value<float>();
            float lng = location["lng"].Value<float>();
            results.Add(new SearchResult { Name = name, Location = new Vector2(lat, lng) });
        }
        return results;
    }

    public void DisplayRoute(Vector2 destination)
    {
        StartCoroutine(GetRoute(currentLocation, destination));
    }

    IEnumerator GetRoute(Vector2 start, Vector2 end)
    {
        string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={start.x},{start.y}&destination={end.x},{end.y}&key={_apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Route JSON Response: " + jsonResponse);

            routePoints = ParseRoute(jsonResponse);

            if (routePoints.Count > 0)
            {
                DrawRouteInAR(routePoints);
            }
            else
            {
                Debug.LogError("No route points found.");
            }
        }
    }

    List<Vector2> ParseRoute(string json)
    {
        List<Vector2> points = new List<Vector2>();
        JObject jsonResponse = JObject.Parse(json);
        string status = jsonResponse["status"].ToString();

        if (status == "ZERO_RESULTS")
        {
            Debug.LogError("No routes found in the response");
            return points; // 빈 리스트 반환
        }

        JArray routes = (JArray)jsonResponse["routes"];
        if (routes.Count == 0)
        {
            Debug.LogError("No routes available.");
            return points;
        }

        JToken legs = routes[0]["legs"];
        if (legs == null || !legs.HasValues)
        {
            Debug.LogError("No legs found in the route.");
            return points;
        }

        JToken steps = legs[0]["steps"];
        if (steps == null || !steps.HasValues)
        {
            Debug.LogError("No steps found in the leg.");
            return points;
        }

        foreach (JToken step in steps)
        {
            float startLat = step["start_location"]["lat"].Value<float>();
            float startLng = step["start_location"]["lng"].Value<float>();
            float endLat = step["end_location"]["lat"].Value<float>();
            float endLng = step["end_location"]["lng"].Value<float>();

            points.Add(new Vector2(startLat, startLng));
            points.Add(new Vector2(endLat, endLng));
        }

        return points;
    }

    void DrawRouteInAR(List<Vector2> routePoints)
    {
        List<Vector3> arPoints = new List<Vector3>();
        foreach (var point in routePoints)
        {
            Vector3 arPoint = ARLocationToWorld(point);
            arPoints.Add(arPoint);
        }
        _lineRenderer.positionCount = arPoints.Count;
        _lineRenderer.SetPositions(arPoints.ToArray());
    }

    Vector3 ARLocationToWorld(Vector2 location)
    {
        // 필요한 경우 좌표 변환 로직 구현
        return new Vector3(location.x, 0, location.y);
    }
}

public class SearchResult
{
    public string Name { get; set; }
    public Vector2 Location { get; set; }
}
