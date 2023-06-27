using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
using Broccollie.UI;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gallery
{
    public class FlowNavigationController : MonoBehaviour
    {
        private const string s_mainItemsKey = "MainItem";
        private const int s_emptyIndex = -1;

        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private List<FlowItemPreset> _itemPresets = null;
        [SerializeField] private List<Transform> _itemAnchors = null;
        [SerializeField] private List<Transform> _subItemAnchors = null;
        [SerializeField] private Transform _itemHolder = null;
        [SerializeField] private Transform _selectedItemAnchor = null;
        [SerializeField] private Transform _selectedCameraAnchor = null;
        [SerializeField] private int _mainItemCenterIndex = 2; // first index is 0
        [SerializeField] private int _subItemCenterIndex = 3;
        [SerializeField] private float _moveToIndexDelay = 0.7f;

        [SerializeField] private FlowMovementPreset _itemMovePreset = null;
        [SerializeField] private FlowMovementPreset _itemSelectionMovePreset = null;
        [SerializeField] private FlowMovementPreset _cameraSelectionMovePreset = null;

        [SerializeField] private TextMeshProUGUI _itemNameText = null;
        [SerializeField] private TextMeshProUGUI _itemDescriptionText = null;
        [SerializeField] private ButtonUI _upButton = null;
        [SerializeField] private ButtonUI _downButton = null;
        [SerializeField] private ButtonUI _selectButton = null;
        [SerializeField] private ButtonUI _backButton = null;

        private Dictionary<string, List<FlowItem>> _createdItems = null;
        private Dictionary<string, List<int>> _activeIndexes = null;
        private readonly Dictionary<string, Stack<int>> _topStacks = new();
        private readonly Dictionary<string, Stack<int>> _botStacks = new();

        private string _selectedItemsKey = null;
        private List<Task> _movePositionsTask = null;
        private CancellationTokenSource _cts = new();

        private void Awake()
        {
            _upButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(true);
            _downButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(false);
            _selectButton.OnClick += (sender, args) => SelectItemAsync();
            _backButton.OnClick += (sender, args) => BackSelectionAsync();

            _backButton.ChangeState(UIStates.Hide.ToString(), true, false, false);
        }

        #region Subscribers
        private async void MovePositionsByDirectionAsync(bool up)
        {
            if (!_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> flowItems) ||
                !_activeIndexes.TryGetValue(_selectedItemsKey, out List<int> indexes)) return;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            if ((!up && indexes[centerIndex] == flowItems.Count - 1) || (up && indexes[centerIndex] == 0))
            {
                // error sound?
                return;
            }

            List<Transform> anchors = _selectedItemsKey == s_mainItemsKey ? _itemAnchors : _subItemAnchors;
            if (up)
            {
                if (_botStacks[_selectedItemsKey].TryPop(out int botpop))
                {
                    indexes.Insert(0, botpop);
                    flowItems[botpop].MoveToAnchorReady(anchors[0]);
                }
                else
                    indexes.Insert(0, s_emptyIndex);

                int lastActiveIndex = indexes.Last();
                if (lastActiveIndex != s_emptyIndex)
                {
                    _topStacks[_selectedItemsKey].Push(lastActiveIndex);
                    flowItems[lastActiveIndex].Disable();
                }
                indexes.RemoveAt(indexes.Count - 1);
            }
            else
            {
                if (_topStacks[_selectedItemsKey].TryPop(out int toppop))
                {
                    indexes.Insert(indexes.Count, toppop);
                    flowItems[toppop].MoveToAnchorReady(anchors[anchors.Count - 1]);
                }
                else
                    indexes.Insert(indexes.Count, s_emptyIndex);

                int firstActiveIndex = indexes.First();
                if (firstActiveIndex != s_emptyIndex)
                {
                    _botStacks[_selectedItemsKey].Push(firstActiveIndex);
                    flowItems[firstActiveIndex].Disable();
                }
                indexes.RemoveAt(0);
            }
            await UpdateActiveItemsPositionAsync(flowItems, indexes, anchors, _itemMovePreset);
        }

        private async void SelectItemAsync()
        {
            if (!_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> flowItems) ||
                !_activeIndexes.TryGetValue(_selectedItemsKey, out List<int> indexes)) return;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            if (indexes[centerIndex] == s_emptyIndex) return;

            MoveItemsToSelectionPointsAsync();
            _backButton.ChangeState(UIStates.Show.ToString());

            _selectedItemsKey = flowItems[indexes[centerIndex]].InjectedPreset.Name;
            if (_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> subItems))
            {
                await MoveToIndexAsync(0);
            }
            else
            {
                _itemDescriptionText.enabled = true;
                _upButton.SetActive(false);
                _downButton.ChangeState(UIStates.Hide.ToString());
            }

            async void MoveItemsToSelectionPointsAsync()
            {
                _movePositionsTask = new List<Task>
                {
                    flowItems[indexes[_mainItemCenterIndex]].MoveToAnchorAsync(_selectedItemAnchor, _itemSelectionMovePreset),
                    Camera.main.transform.LerpPositionAsync(_selectedCameraAnchor.position, _cameraSelectionMovePreset.PositionDuration, _cts.Token, _cameraSelectionMovePreset.PositionCurve)
                };
                await Task.WhenAll(_movePositionsTask);
            }
        }

        private async void BackSelectionAsync()
        {
            _selectedItemsKey = s_mainItemsKey;
            _itemDescriptionText.enabled = false;
            _backButton.ChangeState(UIStates.Hide.ToString());
            _upButton.SetActive(true);
            _downButton.ChangeState(UIStates.Show.ToString());

            _movePositionsTask = new List<Task>
            {
                UpdateActiveItemsPositionAsync(_createdItems[_selectedItemsKey], _activeIndexes[_selectedItemsKey], _itemAnchors, _itemSelectionMovePreset),
                Camera.main.transform.LerpPositionAsync(Vector3.zero, _cameraSelectionMovePreset.PositionDuration, _cts.Token, _cameraSelectionMovePreset.PositionCurve)
            };
            await Task.WhenAll(_movePositionsTask);
        }

        #endregion

        #region Public Functions
        public async Task InitializeAsync()
        {
            // Items
            _selectedItemsKey = s_mainItemsKey;
            _createdItems = new Dictionary<string, List<FlowItem>>();
            List<FlowItem> mainItems = new List<FlowItem>();

            await CreateFlowItems(_itemPresets, mainItems);
            _createdItems.Add(s_mainItemsKey, mainItems);

            foreach (FlowItemPreset preset in _itemPresets)
            {
                if (preset.SubFlowItems == null || preset.SubFlowItems.Count <= 0) continue;
                List<FlowItem> subItems = new List<FlowItem>();
                await CreateFlowItems(preset.SubFlowItems, subItems);
                _createdItems.Add(preset.Name, subItems);
            }

            // Canvas
            if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;

            // Indexes
            _mainItemCenterIndex = _mainItemCenterIndex < 0 || _mainItemCenterIndex > _itemAnchors.Count - 1 ? _itemAnchors.Count / 2 : _mainItemCenterIndex;
            _subItemCenterIndex = _subItemCenterIndex < 0 || _subItemCenterIndex > _subItemAnchors.Count - 1 ? _subItemAnchors.Count / 2 : _subItemCenterIndex;

            _activeIndexes = new Dictionary<string, List<int>>();
            List<int> mainItemIndexes = new List<int>();
            for (int i = 0; i < _itemAnchors.Count; i++)
                mainItemIndexes.Add(s_emptyIndex);
            _activeIndexes.Add(s_mainItemsKey, mainItemIndexes);

            foreach (FlowItemPreset preset in _itemPresets)
            {
                List<int> subItemIndexes = new List<int>();
                for (int i = 0; i < _subItemAnchors.Count; i++)
                    subItemIndexes.Add(s_emptyIndex);
                _activeIndexes.Add(preset.Name, subItemIndexes);
            }

            // Stacks
            Stack<int> mainTop = new Stack<int>();
            _topStacks.Add(s_mainItemsKey, mainTop);

            Stack<int> mainBot = new Stack<int>();
            _botStacks.Add(s_mainItemsKey, mainBot);

            if (_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> flowItems))
            {
                for (int i = flowItems.Count - 1; i >= 0; i--)
                    _topStacks[_selectedItemsKey].Push(i);
            }

            foreach (FlowItemPreset preset in _itemPresets)
            {
                Stack<int> top = new Stack<int>();
                _topStacks.Add(preset.Name, top);

                Stack<int> bot = new Stack<int>();
                _botStacks.Add(preset.Name, bot);

                if (_createdItems.TryGetValue(preset.Name, out List<FlowItem> subFlowItems))
                {
                    for (int i = subFlowItems.Count - 1; i >= 0; i--)
                        _topStacks[preset.Name].Push(i);
                }
            }
            await MoveToIndexAsync(0);

            async Task CreateFlowItems(List<FlowItemPreset> presets, List<FlowItem> items)
            {
                for (int i = 0; i < presets.Count; i++)
                {
                    AsyncOperationHandle<GameObject> mainloadHandle = presets[i].ItemRef.LoadAssetAsync<GameObject>();
                    await mainloadHandle.Task;
                    if (mainloadHandle.Status != AsyncOperationStatus.Succeeded) continue;

                    GameObject mainGo = Instantiate(mainloadHandle.Result, _itemHolder);
                    mainGo.transform.localPosition = Vector3.zero;
                    mainGo.transform.LookAt(Vector3.forward, Vector3.up);
                    mainGo.SetActive(false);

                    if (mainGo.TryGetComponent<FlowItem>(out FlowItem item))
                    {
                        item.Initialize(presets[i]);
                        items.Add(item);
                    }
                }
            }
        }

        public async Task MoveToIndexAsync(int index)
        {
            if (!_activeIndexes.TryGetValue(_selectedItemsKey, out List<int> indexes)) return;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            if (index < 0 || index > _createdItems[_selectedItemsKey].Count - 1) return;

            bool up = indexes[centerIndex] - index > 0;
            while (indexes[centerIndex] != index)
            {
                MovePositionsByDirectionAsync(up);
                await Task.Delay((int)_moveToIndexDelay * 1000);
            }
        }

        #endregion

        private async Task UpdateActiveItemsPositionAsync(List<FlowItem> flowItems, List<int> indexes, List<Transform> anchors, FlowMovementPreset preset)
        {
            _movePositionsTask = new List<Task>();
            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == s_emptyIndex) continue;
                _movePositionsTask.Add(flowItems[indexes[i]].MoveToAnchorAsync(anchors[i], preset));
            }
            await Task.WhenAll(_movePositionsTask);
        }
    }
}
