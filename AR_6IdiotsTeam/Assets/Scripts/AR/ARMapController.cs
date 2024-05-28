using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using Newtonsoft.Json.Linq;

public class ARMapController : MonoBehaviour
{
    [SerializeField] private string _apiKey = "AIzaSyA9nKQa8ipA1HCbxCWrru-0QPDaeS5Cpbk";
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private ARSessionOrigin _arSessionOrigin;

    private Vector2 currentLocation;
    private List<Vector2> routePoints;

    void Start()
    {
        StartCoroutine(C_GetCurrentLocation());
    }

    IEnumerator C_GetCurrentLocation()
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
        StartCoroutine(C_SearchPlace(keyword));
    }

    IEnumerator C_SearchPlace(string keyword)
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
            Vector2 destination = ParseDestination(request.downloadHandler.text);
            StartCoroutine(C_GetRoute(currentLocation, destination));
        }
    }

    Vector2 ParseDestination(string json)
    {
        JObject jsonResponse = JObject.Parse(json);
        JToken location = jsonResponse["results"][0]["geometry"]["location"];
        float lat = location["lat"].Value<float>();
        float lng = location["lng"].Value<float>();
        return new Vector2(lat, lng);
    }

    IEnumerator C_GetRoute(Vector2 start, Vector2 end)
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
            Debug.Log(request.downloadHandler.text);
            routePoints = ParseRoute(request.downloadHandler.text);
            DrawRouteInAR(routePoints);
        }
    }

    List<Vector2> ParseRoute(string json)
    {
        List<Vector2> points = new List<Vector2>();
        JObject jsonResponse = JObject.Parse(json);
        JToken steps = jsonResponse["routes"][0]["legs"][0]["steps"];

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
        // GPS 좌표를 AR 세계 좌표로 변환
        // 필요한 경우 좌표 변환 로직 구현
        return new Vector3(location.x, 0, location.y);
    }
}
