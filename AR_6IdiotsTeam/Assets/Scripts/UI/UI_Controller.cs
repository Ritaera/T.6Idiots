using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : MonoBehaviour
{
    [SerializeField] private TMP_InputField _searchInput;
    [SerializeField] private Button _searchButton;
    [SerializeField] private Button _mapButton;
    [SerializeField] private ARMapController _arMapController;
    [SerializeField] private GameObject _mapTransform;
    private void Awake()
    {
        _searchButton = transform.Find("Button - Serch").GetComponent<Button>();
        _mapButton = transform.Find("Button - Map").GetComponent<Button>();
        _searchInput = transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        _mapTransform = gameObject.transform.Find("Map").gameObject;
    }

    void Start()
    {
        _searchButton.onClick.AddListener(OnSearchButtonClicked);
        _mapButton.onClick.AddListener(OnClickMap);
    }

    void OnClickMap()
    {
        _mapTransform.SetActive(true);
    }

    void OnSearchButtonClicked()
    {
        _arMapController.SearchAndDisplayRoute(_searchInput.text);
    }
}

