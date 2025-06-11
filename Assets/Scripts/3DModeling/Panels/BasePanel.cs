//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public abstract class BasePanel : MonoBehaviour
//{
//    public string panelTitle;
//    public GameObject fullView;
//    public GameObject minimizedView;

//    public Button closeButton;
//    public Button minimizeButton;
//    public Text titleText;

//    protected bool isOpen = true;
//    protected bool isMinimized = false;
//    protected bool isDocked = false;

//    public DockSide dockSide;
//    public bool isExpanded = true;

//    public RectTransform titleBar; // Assign in inspector
//    private bool isDragging = false;
//    private Vector3 dragOffset;
//    void Awake()
//    {
//        closeButton.onClick.AddListener(ClosePanel);
//        minimizeButton.onClick.AddListener(ToggleMinimize);
//        SetTitle(panelTitle);
//    }
//    void Start()
//    {
//        if (titleBar != null)
//        {
//            EventTrigger trigger = titleBar.gameObject.AddComponent<EventTrigger>();

//            EventTrigger.Entry dragBegin = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
//            dragBegin.callback.AddListener((e) => BeginDrag());
//            trigger.triggers.Add(dragBegin);

//            EventTrigger.Entry drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
//            drag.callback.AddListener((e) => Drag());
//            trigger.triggers.Add(drag);

//            EventTrigger.Entry dragEnd = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
//            dragEnd.callback.AddListener((e) => EndDrag());
//            trigger.triggers.Add(dragEnd);
//        }
//    }

//    //void BeginDrag()
//    //{
//    //    isDragging = true;
//    //    dragOffset = transform.position - Camera.main.transform.position;
//    //}

//    //void Drag()
//    //{
//    //    if (isDragging)
//    //    {
//    //        Vector3 cursorWorld = Camera.main.transform.position + Camera.main.transform.forward * 1f;
//    //        transform.position = cursorWorld;
//    //        transform.LookAt(Camera.main.transform);
//    //    }
//    //}

//    //void EndDrag()
//    //{
//    //    isDragging = false;

//    //    if (TryGetDock(out DockSide newSide))
//    //    {
//    //        PanelManager.Instance.DockPanel(this, newSide);
//    //    }
//    //    else
//    //    {
//    //        PanelManager.Instance.DockToWorld(this);
//    //    }
//    //}


//    private void Update()
//    {
//        if (!isDocked && followTarget != null)
//        {
//            transform.position = followTarget.position + followTarget.forward * 0.5f;
//            transform.rotation = Quaternion.LookRotation(followTarget.forward);
//        }
//    }

//    public void Init(Transform followTransform)
//    {
//        followTarget = followTransform;
//    }

//    public void SetTitle(string title)
//    {
//        panelTitle = title;
//        if (titleText != null)
//            titleText.text = title;
//    }

//    public void ToggleMinimize()
//    {
//        isMinimized = !isMinimized;
//        UpdateView();
//    }

//    public void ClosePanel()
//    {
//        isOpen = false;
//        gameObject.SetActive(false);
//    }

//    public void OpenPanel()
//    {
//        isOpen = true;
//        gameObject.SetActive(true);
//        UpdateView();
//    }

//    public void DockPanel(Vector3 position, Quaternion rotation)
//    {
//        isDocked = true;
//        transform.position = position;
//        transform.rotation = rotation;
//    }

//    public void Undock()
//    {
//        isDocked = false;
//    }

//    private void UpdateView()
//    {
//        fullView.SetActive(!isMinimized);
//        minimizedView.SetActive(isMinimized);
//    }
//}